// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class SprintSpeedPerkEffectSystem : PerkEffectSystem<SprintSpeedPerkEffect>
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PerksComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiers);
    }

    protected override void OnEffectApplied(Entity<PerksComponent> ent, ref PerkEffectAppliedEvent<SprintSpeedPerkEffect> args)
    {
        EnsureComp<MovementSpeedModifierComponent>(ent);
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnRefreshMovementSpeedModifiers(Entity<PerksComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var effect = GetSprintSpeedEffect(ent);
        if (effect == null)
            return;

        args.ModifySpeed(1f, effect.SprintSpeedMultiplier);
    }

    private SprintSpeedPerkEffect? GetSprintSpeedEffect(Entity<PerksComponent> ent)
    {
        foreach (var perkId in ent.Comp.Perks)
        {
            if (!_prototypes.TryIndex(perkId, out PerkPrototype? perk))
                continue;

            foreach (var effect in perk.Effects)
            {
                if (effect is SprintSpeedPerkEffect typed)
                    return typed;
            }
        }

        return null;
    }
}