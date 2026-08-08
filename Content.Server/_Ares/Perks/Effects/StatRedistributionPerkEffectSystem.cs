// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;
using Content.Shared._Ares.Stats;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class StatRedistributionPerkEffectSystem : PerkEffectSystem<StatRedistributionPerkEffect>
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    protected override void OnEffectApplied(Entity<PerksComponent> ent, ref PerkEffectAppliedEvent<StatRedistributionPerkEffect> args)
    {
        var levels = new List<(ProtoId<StatPrototype> Id, int Level)>();

        foreach (var stat in _prototypes.EnumeratePrototypes<StatPrototype>())
        {
            var statId = new ProtoId<StatPrototype>(stat.ID);
            levels.Add((statId, _stats.GetStatLevel(ent, statId)));
        }

        if (levels.Count == 0)
            return;

        levels.Sort((a, b) => b.Level.CompareTo(a.Level));

        _stats.ModifyStatLevel(ent, levels[0].Id, args.Effect.HighestDelta);

        for (var i = 1; i < levels.Count; i++)
        {
            _stats.ModifyStatLevel(ent, levels[i].Id, args.Effect.OtherDelta);
        }
    }
}