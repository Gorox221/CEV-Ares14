// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;
using Content.Shared._Ares.Stats;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class StatDeltaPerkEffectSystem : PerkEffectSystem<StatDeltaPerkEffect>
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    protected override void OnEffectApplied(Entity<PerksComponent> ent, ref PerkEffectAppliedEvent<StatDeltaPerkEffect> args)
    {
        if (args.Effect.AllDelta != 0)
        {
            foreach (var stat in _prototypes.EnumeratePrototypes<StatPrototype>())
            {
                _stats.ModifyStatLevel(ent, new ProtoId<StatPrototype>(stat.ID), args.Effect.AllDelta);
            }
        }

        foreach (var (statId, delta) in args.Effect.Deltas)
        {
            _stats.ModifyStatLevel(ent, statId, delta);
        }
    }
}