// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Sanity.Prototypes;
using Content.Shared._Ares.Stats;
using Content.Shared.Mind;
using Content.Shared.Nutrition;
using Content.Shared.Objectives.Systems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Coordinates rest desires: spawns a desire objective when insight maxes out
/// and handles the level-up choice after the objective completes. The objective
/// pipeline (see <see cref="DesireConditionSystem"/>) reports fulfillment.
/// </summary>
public sealed partial class RestSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private const float InsightDecayReset = 80f;

    private readonly Dictionary<EntityUid, EntityUid> _desireObjectives = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanityChangedEvent>(OnSanityChanged);
        SubscribeLocalEvent<DesireCompletedEvent>(OnDesireCompleted);
        SubscribeLocalEvent<RestComponent, IngestingEvent>(OnIngested);
        SubscribeNetworkEvent<RestLevelUpRequestEvent>(OnLevelUpRequest);
    }

    /// <summary>
    /// The ingest subscription is owned by the rest system because the event bus of this
    /// fork allows only one directed subscription per component-event pair. The desire
    /// systems pick it up through <see cref="RestDesireIngestedEvent"/>.
    /// </summary>
    private void OnIngested(Entity<RestComponent> ent, ref IngestingEvent args)
    {
        if (ent.Comp.LevelUpPending)
            return;

        var ev = new RestDesireIngestedEvent(ent, args.Food, args.Split, args.ForceFed);
        RaiseLocalEvent(ref ev);
    }

    /// <summary>
    /// Tries to get the player's active desire objective and its condition.
    /// </summary>
    public bool TryGetDesireObjective(EntityUid uid, out EntityUid objEnt, out DesireConditionComponent condition)
    {
        if (_desireObjectives.TryGetValue(uid, out var obj))
        {
            if (TryComp<DesireConditionComponent>(obj, out var comp))
            {
                objEnt = obj;
                condition = comp;
                return true;
            }

            _desireObjectives.Remove(uid);
        }

        objEnt = default!;
        condition = default!;
        return false;
    }

    private void OnSanityChanged(ref SanityChangedEvent args)
    {
        var uid = args.Entity;

        if (!HasComp<InsightComponent>(uid))
            return;

        var rest = EnsureComp<RestComponent>(uid);
        var insight = Comp<InsightComponent>(uid);

        if (_desireObjectives.ContainsKey(uid) || rest.LevelUpPending)
        {
            if (insight.CurrentInsight >= insight.MaxInsight)
            {
                insight.CurrentInsight = InsightDecayReset;
                insight.LevelChange = 0f;
                Dirty(uid, insight);

                RemoveDesireObjective(uid);
                rest.LevelUpPending = false;
                Dirty(uid, rest);

                _popup.PopupEntity(Loc.GetString("rest-insight-faded"), uid, uid);
            }
            return;
        }

        if (insight.CurrentInsight < insight.MaxInsight)
            return;

        insight.CurrentInsight = 0f;
        insight.LevelChange = 0f;
        Dirty(uid, insight);

        var desire = PickDesire();
        if (desire == null)
            return;

        CreateDesireObjective(uid, desire);
        _popup.PopupEntity(Loc.GetString("rest-desire-start"), uid, uid);
    }

    private void CreateDesireObjective(EntityUid uid, DesirePrototype desire)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        var objEnt = _objectives.TryCreateObjective(mindId, mind, desire.Objective);
        if (objEnt == null)
            return;

        _mind.AddObjective(mindId, mind, objEnt.Value);
        _desireObjectives[uid] = objEnt.Value;
    }

    private void OnDesireCompleted(ref DesireCompletedEvent ev)
    {
        var uid = EntityUid.Invalid;
        foreach (var (player, obj) in _desireObjectives)
        {
            if (obj == ev.Objective)
            {
                uid = player;
                break;
            }
        }

        if (!uid.IsValid() || !TryComp<RestComponent>(uid, out var rest))
            return;

        _desireObjectives.Remove(uid);
        rest.LevelUpPending = true;
        Dirty(uid, rest);

        RemoveObjectiveFromMind(uid, ev.Objective);

        _popup.PopupEntity(Loc.GetString("rest-desire-complete"), uid, uid);
        _popup.PopupEntity(Loc.GetString("rest-levelup-available"), uid, uid);
    }

    private void RemoveDesireObjective(EntityUid uid)
    {
        if (!_desireObjectives.TryGetValue(uid, out var objEnt))
            return;

        _desireObjectives.Remove(uid);
        RemoveObjectiveFromMind(uid, objEnt);
    }

    private void RemoveObjectiveFromMind(EntityUid uid, EntityUid objEnt)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        var index = mind.Objectives.IndexOf(objEnt);
        if (index >= 0)
            _mind.TryRemoveObjective(mindId, mind, index);
    }

    private void OnLevelUpRequest(RestLevelUpRequestEvent args, EntitySessionEventArgs session)
    {
        var uid = GetEntity(args.Player);
        if (session.SenderSession.AttachedEntity != uid)
            return;

        if (!TryComp<RestComponent>(uid, out var rest) || !rest.LevelUpPending)
            return;

        rest.LevelUpPending = false;
        Dirty(uid, rest);

        if (args.Choice == "Internalize")
            DistributeStats(uid);

        _popup.PopupEntity(Loc.GetString("rest-levelup-complete"), uid, uid);
    }

    private void DistributeStats(EntityUid uid)
    {
        var totalPoints = 45;
        var statIds = _prototypes.EnumeratePrototypes<StatPrototype>()
            .Select(s => new ProtoId<StatPrototype>(s.ID))
            .ToList();

        var points = new int[statIds.Count];
        var remaining = totalPoints;

        for (var i = 0; i < points.Length; i++)
        {
            points[i] = _random.Next(0, remaining + 1);
            remaining -= points[i];
        }

        for (var i = 0; i < statIds.Count; i++)
            _stats.ModifyStatLevel(uid, statIds[i], points[i]);
    }

    private DesirePrototype? PickDesire()
    {
        var all = _prototypes.EnumeratePrototypes<DesirePrototype>().ToList();
        if (all.Count == 0)
            return null;

        var totalWeight = all.Sum(p => p.Weight);
        var roll = (float)_random.NextDouble() * totalWeight;

        foreach (var proto in all)
        {
            roll -= proto.Weight;
            if (roll <= 0)
                return proto;
        }

        return all[^1];
    }
}
