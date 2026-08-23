// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Origins;
using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;
using Content.Shared._Ares.Stats;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks;

public sealed partial class PerkSystem : EntitySystem, IPerkEffectApplier
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly OriginSystem _origins = default!;
    [Dependency] private readonly AresStatsSystem _stats = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<RoleAddedEvent>(OnRoleAdded);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        _origins.ApplyOrigin(ev.Mob, ev.Profile.Origin);
        ApplyPerk(ev.Mob, ev.Profile.Perk);

        if (ev.JobId != null && _prototypes.TryIndex(ev.JobId, out JobPrototype? job))
        {
            foreach (var perk in job.Perks)
                ApplyPerk(ev.Mob, perk);

            foreach (var (stat, delta) in job.Stats)
                _stats.ModifyStatLevel(ev.Mob, stat, delta);
        }
    }

    private void OnRoleAdded(RoleAddedEvent args)
    {
        if (args.Mind.OwnedEntity is not { } entity)
            return;

        foreach (var role in args.Mind.MindRoleContainer.ContainedEntities)
        {
            if (!TryComp<MindRoleComponent>(role, out var mindRole)
                || mindRole.AntagPrototype is not { } antagId
                || !_prototypes.TryIndex(antagId, out AntagPrototype? antag))
                continue;

            foreach (var perk in antag.Perks)
                ApplyPerk(entity, perk);
        }
    }

    public void ApplyPerk(EntityUid target, ProtoId<PerkPrototype> perkId)
    {
        if (string.IsNullOrEmpty(perkId))
            return;

        if (!_prototypes.TryIndex(perkId, out PerkPrototype? perk))
        {
            Log.Error($"No perk found with ID {perkId}!");
            return;
        }

var comp = EnsureComp<PerksComponent>(target);
        if (comp.Perks.Contains(perkId))
            return;

        comp.Perks.Add(perkId);
        Dirty(target, comp);

        foreach (var effect in perk.Effects)
        {
            effect.Apply(target, this);
        }
    }

    public void ApplyEffect<T>(EntityUid target, T effect) where T : PerkEffect<T>
    {
        var ev = new PerkEffectAppliedEvent<T>(effect);
        RaiseLocalEvent(target, ref ev, true);
    }

    public void RemovePerk(EntityUid target, ProtoId<PerkPrototype> perkId)
    {
        if (string.IsNullOrEmpty(perkId) || !TryComp<PerksComponent>(target, out var comp))
            return;

        if (!comp.Perks.Remove(perkId))
            return;

        Dirty(target, comp);
    }
}