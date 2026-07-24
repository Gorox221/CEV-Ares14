// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

public sealed class StatBuffMeleeSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StatsComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<StatsComponent> ent, ref MeleeHitEvent args)
    {
        if (args.Weapon != ent.Owner)
            return;

        var robPrototype = new ProtoId<StatPrototype>("Robustness");

        if (!_prototypes.HasIndex(robPrototype))
            return;

        var robLevel = _stats.GetStatLevel(ent, robPrototype, ent.Comp);

        var bonus = Math.Min(robLevel / 4, 15);

        if (bonus <= 0)
            return;

        args.BonusDamage.DamageDict["Blunt"] = args.BonusDamage.DamageDict.GetValueOrDefault("Blunt") + bonus;
    }
}