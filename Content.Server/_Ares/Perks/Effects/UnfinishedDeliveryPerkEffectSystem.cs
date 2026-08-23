// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.Traits.Assorted;
using Content.Goobstation.Shared.Devour.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class UnfinishedDeliveryPerkEffectSystem : EntitySystem
{
    [Dependency] private readonly PerkSystem _perks = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly SleepingSystem _sleeping = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || args.OldMobState == MobState.Dead)
            return;

        if (!TryComp<PerksComponent>(args.Target, out var perks))
            return;

        Entity<PerksComponent> entity = (args.Target, perks);
        var effect = GetEffect(entity, out var perkId);
        if (effect == null || perkId == null)
            return;

        if (!_random.Prob(effect.Chance))
            return;

        if (HasComp<UnrevivableComponent>(args.Target))
            return;

        var reviveEv = new BeforeSelfRevivalEvent(args.Target, "self-revive-fail");
        RaiseLocalEvent(args.Target, ref reviveEv);
        if (reviveEv.Cancelled)
            return;

        var spec = new DamageSpecifier(_prototypes.Index<DamageGroupPrototype>("Brute"), -effect.BruteHeal)
            + new DamageSpecifier(_prototypes.Index<DamageGroupPrototype>("Burn"), -effect.BurnHeal)
            + new DamageSpecifier(_prototypes.Index<DamageTypePrototype>("Asphyxiation"), -effect.OxyHeal);
        _damageable.TryChangeDamage(entity, spec, ignoreResistances: true, interruptsDoAfters: false);

        _mobState.ChangeMobState(args.Target, MobState.Alive, args.Component);

        var ticks = _random.Next(effect.MinSleepTicks, effect.MaxSleepTicks + 1);
        _statusEffect.TryAddStatusEffectDuration(entity, SleepingSystem.StatusEffectForcedSleeping,
            TimeSpan.FromSeconds(ticks * 2));
        _sleeping.TrySleeping((args.Target, args.Component));

        _perks.RemovePerk(args.Target, perkId.Value);
    }

    private UnfinishedDeliveryPerkEffect? GetEffect(Entity<PerksComponent> ent, out ProtoId<PerkPrototype>? perkId)
    {
        perkId = null;

        foreach (var id in ent.Comp.Perks)
        {
            if (!_prototypes.TryIndex(id, out PerkPrototype? perk))
                continue;

            foreach (var effect in perk.Effects)
            {
                if (effect is not UnfinishedDeliveryPerkEffect typed)
                    continue;

                perkId = id;
                return typed;
            }
        }

        return null;
    }
}