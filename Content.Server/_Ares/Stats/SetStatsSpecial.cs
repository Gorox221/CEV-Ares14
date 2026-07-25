// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Stats;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Stats;

public sealed partial class SetStatsSpecial : JobSpecial
{
    [DataField]
    public Dictionary<ProtoId<StatPrototype>, int> Stats { get; private set; } = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var statsSystem = entMan.System<AresStatsSystem>();
        var statsComp = entMan.EnsureComponent<StatsComponent>(mob);

        var allStats = new Dictionary<ProtoId<StatPrototype>, int>();
        foreach (var (statId, level) in Stats)
        {
            var current = statsComp.Stats.GetValueOrDefault(statId, 0);
            allStats[statId] = current + level;
        }

        statsSystem.SetAllStatLevels(mob, allStats, statsComp);
    }
}
