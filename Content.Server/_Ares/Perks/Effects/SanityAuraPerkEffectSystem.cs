// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Sanity.Systems;
using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks.Effects;

/// This is a separate system instead of reusing SanityAffectorComponent
/// In order to better match the pattern of the other perk effects.
public sealed partial class SanityAuraPerkEffectSystem : SanityChangeSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SanityCheckEvent>(OnSanityCheck);
    }

    private void OnSanityCheck(ref SanityCheckEvent args)
    {
        var uid = args.Entity;

        if (!CanPerceiveSanityEffects(uid))
            return;

        if (!TryComp<SanityComponent>(uid, out var sanity))
            return;

        var xform = Transform(uid);
        var totalChange = 0f;

        foreach (var (perkHolder, perks) in _lookup.GetEntitiesInRange<PerksComponent>(xform.Coordinates, sanity.Range))
        {
            if (perkHolder == uid)
                continue;

            var effect = GetAuraEffect((perkHolder, perks));
            if (effect == null)
                continue;

            if (effect.RequiresAlive && !_mobState.IsAlive(perkHolder))
                continue;

            var holderXform = Transform(perkHolder);
            var distance = (_transform.GetWorldPosition(holderXform) - _transform.GetWorldPosition(xform)).Length();
            if (distance > effect.Range)
                continue;

            if (effect.RequiresLineOfSight
                && !_interaction.InRangeUnobstructed(uid, perkHolder, distance))
                continue;

            totalChange += effect.Change;
        }

        if (MathHelper.CloseTo(totalChange, 0f))
            return;

        ApplyChange((uid, sanity), totalChange);
    }

    private SanityAuraPerkEffect? GetAuraEffect(Entity<PerksComponent> ent)
    {
        foreach (var perkId in ent.Comp.Perks)
        {
            if (!_prototypes.TryIndex(perkId, out PerkPrototype? perk))
                continue;

            foreach (var effect in perk.Effects)
            {
                if (effect is SanityAuraPerkEffect typed)
                    return typed;
            }
        }

        return null;
    }
}
