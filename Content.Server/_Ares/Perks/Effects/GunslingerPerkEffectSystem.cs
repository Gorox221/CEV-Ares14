// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class GunslingerPerkEffectSystem : PerkEffectSystem<GunslingerPerkEffect>
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PerksComponent, GunRefreshModifiersEvent>(OnGunRefresh);
        SubscribeLocalEvent<GunComponent, ItemWieldedEvent>(OnGunWielded);
        SubscribeLocalEvent<GunComponent, ItemUnwieldedEvent>(OnGunUnwielded);
    }

    protected override void OnEffectApplied(Entity<PerksComponent> ent, ref PerkEffectAppliedEvent<GunslingerPerkEffect> args)
    {
        if (!TryComp<HandsComponent>(ent, out var hands))
            return;

        foreach (var (handId, _) in hands.Hands)
        {
            if (_hands.GetHeldItem((ent, hands), handId) is not { } held
                || !TryComp<GunComponent>(held, out var gun))
                continue;

            _gun.RefreshModifiers((held, gun));
        }
    }

    private void OnGunRefresh(Entity<PerksComponent> ent, ref GunRefreshModifiersEvent args)
    {
        var effect = GetGunslingerEffect(ent);
        if (effect == null)
            return;

        if (TryComp<WieldableComponent>(args.Gun, out var wieldable) && wieldable.Wielded)
            return;

        args.FireRate *= effect.FireRateMultiplier;
        args.BurstFireRate *= effect.FireRateMultiplier;
    }

    private void OnGunWielded(Entity<GunComponent> gun, ref ItemWieldedEvent args)
    {
        _gun.RefreshModifiers((gun, gun.Comp), args.User);
    }

    private void OnGunUnwielded(Entity<GunComponent> gun, ref ItemUnwieldedEvent args)
    {
        _gun.RefreshModifiers((gun, gun.Comp), args.User);
    }

    private GunslingerPerkEffect? GetGunslingerEffect(Entity<PerksComponent> ent)
    {
        foreach (var perkId in ent.Comp.Perks)
        {
            if (!_prototypes.TryIndex(perkId, out PerkPrototype? perk))
                continue;

            foreach (var effect in perk.Effects)
            {
                if (effect is GunslingerPerkEffect typed)
                    return typed;
            }
        }

        return null;
    }
}