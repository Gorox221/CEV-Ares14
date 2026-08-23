// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Sanity.Systems;
using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class MoralistPerkEffectSystem : SanityChangeSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobThresholdSystem _thresholds = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SanityCheckEvent>(OnSanityCheck);
        SubscribeLocalEvent<InsightGainModifierEvent>(OnInsightGain);
        SubscribeLocalEvent<SanityRegenModifierEvent>(OnSanityRegen);
    }

    private MoralistPerkEffect? GetMoralistEffect(Entity<PerksComponent> ent)
    {
        foreach (var perkId in ent.Comp.Perks)
        {
            if (!_prototypes.TryIndex(perkId, out PerkPrototype? perk))
                continue;

            foreach (var effect in perk.Effects)
            {
                if (effect is MoralistPerkEffect typed)
                    return typed;
            }
        }

        return null;
    }

    private void OnSanityCheck(ref SanityCheckEvent args)
    {
        if (!CanPerceiveSanityEffects(args.Entity))
            return;

        if (!TryComp<SanityComponent>(args.Entity, out var sanity)
            || !TryComp<PerksComponent>(args.Entity, out var perks))
            return;

        var effect = GetMoralistEffect((args.Entity, perks));
        if (effect == null)
            return;

        var sick = CountSickNearby((args.Entity, perks), effect);
        if (sick <= 0)
            return;

        var delta = -effect.DamagePerSick * effect.DamageMultiplier * GetVigilanceMultiplier(args.Entity) * sick;
        ApplyChange((args.Entity, sanity), delta);
    }

    private void OnInsightGain(ref InsightGainModifierEvent args)
    {
        if (!TryComp<PerksComponent>(args.Entity, out var perks))
            return;

        var effect = GetMoralistEffect((args.Entity, perks));
        if (effect == null)
            return;

        var sane = CountSaneNearby((args.Entity, perks), effect);
        if (sane > 0)
            args.Multiplier *= 1f + effect.MultiplierPerSane * sane;
    }

    private void OnSanityRegen(ref SanityRegenModifierEvent args)
    {
        if (!TryComp<PerksComponent>(args.Entity, out var perks))
            return;

        var effect = GetMoralistEffect((args.Entity, perks));
        if (effect == null)
            return;

        var sane = CountSaneNearby((args.Entity, perks), effect);
        if (sane > 0)
            args.Multiplier *= 1f + effect.MultiplierPerSane * sane;
    }

    private int CountSaneNearby(Entity<PerksComponent> ent, MoralistPerkEffect effect)
    {
        if (!TryComp<SanityComponent>(ent, out var sanity))
            return 0;

        var count = 0;
        foreach (var (other, otherSanity) in _lookup.GetEntitiesInRange<SanityComponent>(Transform(ent).Coordinates, sanity.Range))
        {
            if (other == ent.Owner)
                continue;

            if (_mobState.IsIncapacitated(other))
                continue;

            if (effect.RequiresLineOfSight
                && !_interaction.InRangeUnobstructed(ent.Owner, other, sanity.Range))
                continue;

            if (otherSanity.CurrentSanity > effect.SaneSanityThreshold)
                count++;
        }

        return count;
    }

    private int CountSickNearby(Entity<PerksComponent> ent, MoralistPerkEffect effect)
    {
        if (!TryComp<SanityComponent>(ent, out var sanity))
            return 0;

        var count = 0;
        foreach (var (other, otherSanity) in _lookup.GetEntitiesInRange<SanityComponent>(Transform(ent).Coordinates, sanity.Range))
        {
            if (other == ent.Owner)
                continue;

            if (_mobState.IsIncapacitated(other))
                continue;

            if (effect.RequiresLineOfSight
                && !_interaction.InRangeUnobstructed(ent.Owner, other, sanity.Range))
                continue;

            if (IsSick(other, otherSanity, effect))
                count++;
        }

        return count;
    }

    private bool IsSick(EntityUid uid, SanityComponent sanity, MoralistPerkEffect effect)
    {
        if (sanity.CurrentSanity < effect.SickSanityThreshold)
            return true;

        return TryComp<DamageableComponent>(uid, out var damageable)
            && TryComp<MobThresholdsComponent>(uid, out var thresholds)
            && _thresholds.TryGetDeadPercentage(uid, damageable.TotalDamage, out var deadPercentage, thresholds)
            && deadPercentage.Value.Float() > effect.SickHealthPercent / 100f;
    }
}
