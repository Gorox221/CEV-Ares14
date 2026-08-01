// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Behaviors;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared.Damage;
using System.Linq;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Lowers sanity when the entity takes damage, scaled by vigilance.
/// </summary>
public sealed partial class SanityDamageSystem : SanityChangeSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SanityComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<SanityComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta == null)
            return;

        var behavior = ent.Comp.Changes.OfType<DamageSanityChangeBehavior>().FirstOrDefault();
        if (behavior == null)
            return;

        var totalDamage = args.DamageDelta.GetTotal().Float();
        if (totalDamage <= 0f)
            return;

        var delta = -totalDamage * behavior.ChangePerDamage;
        delta *= GetVigilanceMultiplier(ent);
        ApplyChange(ent, delta);
    }
}
