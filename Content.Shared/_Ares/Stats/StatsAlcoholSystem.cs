// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Drunk;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

public sealed class StatDrunkennessSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StatsComponent, SharedDrunkSystem.DrunkEvent>(OnDrunk);
    }

    private void OnDrunk(Entity<StatsComponent> ent, ref SharedDrunkSystem.DrunkEvent args)
    {
        var toughPrototype = new ProtoId<StatPrototype>("Toughness");

        if (!_prototypes.HasIndex(toughPrototype))
            return;

        var thg = _stats.GetStatLevel(ent, toughPrototype, ent.Comp);
        var multiplier = Math.Max(1f - thg / 100f, 0.1f);
        args.Duration *= multiplier;
    }
}