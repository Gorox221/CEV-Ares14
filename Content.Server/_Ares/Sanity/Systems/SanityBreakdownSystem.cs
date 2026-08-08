// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Sanity.Prototypes;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanityBreakdownSystem : EntitySystem, ISanityBreakdownTrigger
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SanityEmoteSystem _sanityEmote = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SanityChangedEvent>(OnSanityChanged);
        SubscribeLocalEvent<SanityCheckEvent>(OnSanityCheck);
        SubscribeLocalEvent<SanityComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(Entity<SanityComponent> ent, ref MobStateChangedEvent args)
    {
        if (!_mobState.IsIncapacitated(ent))
            return;

        ent.Comp.CurrentBreakdown = null;
        Dirty(ent, ent.Comp);
        _sanityEmote.StopEmoting(ent);
    }

    private void OnSanityCheck(ref SanityCheckEvent args)
    {
        if (!TryComp<SanityComponent>(args.Entity, out var sanity))
            return;

        TryTriggerBreakdown(args.Entity, sanity);
    }

    private void OnSanityChanged(ref SanityChangedEvent args)
    {
        var uid = args.Entity;
        if (!TryComp<SanityComponent>(uid, out var sanity))
            return;

        TryTriggerBreakdown(uid, sanity);
    }

    private void TryTriggerBreakdown(EntityUid uid, SanityComponent sanity)
    {
        if (_mobState.IsIncapacitated(uid)
            || sanity.CurrentSanity > 0
            || sanity.CurrentBreakdown != null
            || _timing.CurTime < sanity.NextBreakdownTime)
            return;

        var breakdown = PickBreakdown(uid);
        if (breakdown == null)
            return;

        sanity.NextBreakdownTime = _timing.CurTime + TimeSpan.FromMinutes(7);
        sanity.CurrentBreakdown = breakdown.ID;
        Dirty(uid, sanity);

        _popup.PopupEntity(Loc.GetString(breakdown.Popup), uid, uid);
        breakdown.Behavior.Trigger((uid, sanity), this);

        if (breakdown.Behavior.Emotes.Count > 0)
            _sanityEmote.StartEmoting(uid, breakdown.Behavior.Emotes, breakdown.Behavior.EmoteInterval, breakdown.Behavior.EmoteChance);
    }

    public void RaiseBreakdown<T>(Entity<SanityComponent> ent, T behavior) where T : SanityBreakdownBehavior<T>
    {
        var ev = new SanityBreakdownTriggeredEvent<T>(behavior);
        RaiseLocalEvent(ent, ref ev, true);
    }

    /// <summary>
    /// Picks a breakdown for the entity. Positive breakdowns have their weight
    /// scaled by perk modifiers, e.g. to boost or fully remove their chance.
    /// </summary>
    private SanityBreakdownPrototype? PickBreakdown(EntityUid uid)
    {
        var all = _prototypes.EnumeratePrototypes<SanityBreakdownPrototype>().ToList();
        if (all.Count == 0)
            return null;

        var positive = new PositiveBreakdownChanceModifierEvent();
        RaiseLocalEvent(uid, ref positive, true);

        float WeightOf(SanityBreakdownPrototype proto)
        {
            var weight = proto.Weight;
            if (proto.Positive)
                weight *= positive.PositiveMultiplier;

            return weight;
        }

        var totalWeight = all.Sum(WeightOf);
        var roll = (float)_random.NextDouble() * totalWeight;

        foreach (var proto in all)
        {
            roll -= WeightOf(proto);
            if (roll <= 0)
                return proto;
        }

        return all[^1];
    }
}
