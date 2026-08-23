// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;
using Content.Shared.Strip.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks.Effects;

/// <summary>
/// Makes all strip attempts of the perk holder stealthy, so no popup is
/// shown to the stripped character.
/// </summary>
public sealed partial class StealthStripPerkEffectSystem : PerkEffectSystem<StealthStripPerkEffect>
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PerksComponent, BeforeStripEvent>(OnBeforeStrip);
    }

    protected override void OnEffectApplied(Entity<PerksComponent> ent, ref PerkEffectAppliedEvent<StealthStripPerkEffect> args)
    {
    }

    private void OnBeforeStrip(EntityUid uid, PerksComponent component, BeforeStripEvent args)
    {
        if (!HasStealthStrip(uid, component))
            return;

        args.Stealth = true;
    }

    private bool HasStealthStrip(EntityUid uid, PerksComponent component)
    {
        foreach (var perkId in component.Perks)
        {
            if (!_prototypes.TryIndex(perkId, out PerkPrototype? perk))
                continue;

            foreach (var effect in perk.Effects)
            {
                if (effect is StealthStripPerkEffect)
                    return true;
            }
        }

        return false;
    }
}