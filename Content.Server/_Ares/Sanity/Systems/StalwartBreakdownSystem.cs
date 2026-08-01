// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Damage;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Handles the stalwart breakdown: heals the target and restores sanity immediately.
/// </summary>
public sealed partial class StalwartBreakdownSystem : SanityBreakdownEffectSystem<StalwartBreakdownBehavior>
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    protected override void OnBreakdownTriggered(Entity<SanityComponent> ent, ref SanityBreakdownTriggeredEvent<StalwartBreakdownBehavior> args)
    {
        if (args.Behavior.Healing != null)
            _damageable.TryChangeDamage(ent, args.Behavior.Healing, ignoreResistances: true);

        var oldValue = ent.Comp.CurrentSanity;
        var newValue = Math.Clamp(args.Behavior.SanityReturn, ent.Comp.MinSanity, ent.Comp.MaxSanity);
        ent.Comp.CurrentSanity = newValue;
        Dirty(ent, ent.Comp);

        var ev = new SanityChangedEvent(ent, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(ent, ref ev, true);
    }
}
