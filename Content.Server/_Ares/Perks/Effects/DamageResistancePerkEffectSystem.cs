// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class DamageResistancePerkEffectSystem : PerkEffectSystem<DamageResistancePerkEffect>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    protected override void OnEffectApplied(Entity<PerksComponent> ent, ref PerkEffectAppliedEvent<DamageResistancePerkEffect> args)
    {
        if (!_prototypeManager.TryIndex(args.Effect.ModifierSetId, out DamageModifierSetPrototype? modifierSet))
            return;

        var buffComp = EnsureComp<DamageProtectionBuffComponent>(ent);
        if (!buffComp.Modifiers.ContainsKey(args.Effect.ModifierSetId))
            buffComp.Modifiers.Add(args.Effect.ModifierSetId, modifierSet);
    }
}
