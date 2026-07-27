// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Damage;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanityDamageSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SanityComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<SanityComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta == null)
            return;

        var totalDamage = args.DamageDelta.GetTotal().Float();
        if (totalDamage <= 0f)
            return;

        var delta = totalDamage / 5f * 0.6f;
        var oldValue = ent.Comp.CurrentSanity;
        var newValue = Math.Clamp(oldValue - delta, ent.Comp.MinSanity, ent.Comp.MaxSanity);

        if (MathHelper.CloseTo(oldValue, newValue))
            return;

        ent.Comp.CurrentSanity = newValue;
        Dirty(ent, ent.Comp);

        var ev = new SanityChangedEvent(ent, oldValue, newValue, oldValue - newValue);
        RaiseLocalEvent(ent, ref ev);
    }
}
