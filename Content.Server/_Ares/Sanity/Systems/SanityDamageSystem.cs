// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Stats;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanityDamageSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

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
        delta *= GetVigilanceMultiplier(ent);
        var oldValue = ent.Comp.CurrentSanity;
        var newValue = Math.Clamp(oldValue - delta, ent.Comp.MinSanity, ent.Comp.MaxSanity);

        if (MathHelper.CloseTo(oldValue, newValue))
            return;

        ent.Comp.CurrentSanity = newValue;
        Dirty(ent, ent.Comp);

        var ev = new SanityChangedEvent(ent, oldValue, newValue, oldValue - newValue);
        RaiseLocalEvent(ent, ref ev, true);
    }

    private float GetVigilanceMultiplier(EntityUid uid)
    {
        var vigPrototype = new ProtoId<StatPrototype>("Vigilance");
        if (!_prototypes.HasIndex(vigPrototype))
            return 1f;

        var vigLevel = _stats.GetStatLevel(uid, vigPrototype);
        var clampedVig = Math.Clamp(vigLevel, 0, 60);
        return (float)(1.2 - clampedVig / 60.0);
    }
}
