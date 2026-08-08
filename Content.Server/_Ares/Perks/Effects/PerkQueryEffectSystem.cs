// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks.Effects;

public abstract partial class PerkQueryEffectSystem<TEffect> : EntitySystem
    where TEffect : PerkEffect<TEffect>
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    protected float GetMultiplier(Entity<PerksComponent> ent, Func<TEffect, float> select)
    {
        var multiplier = 1f;

        foreach (var perkId in ent.Comp.Perks)
        {
            if (!_prototypes.TryIndex(perkId, out PerkPrototype? perk))
                continue;

            foreach (var effect in perk.Effects)
            {
                if (effect is TEffect typed)
                    multiplier *= select(typed);
            }
        }

        return multiplier;
    }

    protected TEffect? GetEffect(Entity<PerksComponent> ent)
    {
        foreach (var perkId in ent.Comp.Perks)
        {
            if (!_prototypes.TryIndex(perkId, out PerkPrototype? perk))
                continue;

            foreach (var effect in perk.Effects)
            {
                if (effect is TEffect typed)
                    return typed;
            }
        }

        return null;
    }
}