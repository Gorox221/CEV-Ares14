// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Perks;
using Content.Server.Station.Systems;
using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Stats;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Ares;

[TestFixture]
public sealed class PerkTests
{
    private static readonly ProtoId<StatPrototype>[] StatIds =
    {
        "Robustness",
        "Toughness",
        "Biology",
        "Mechanical",
        "Vigilance",
        "Cognition",
    };

    [Test]
    public async Task TestPaperWorm()
    {
        var pair = await PoolManager.GetServerClient(new PoolSettings()
        {
            Dirty = true,
        });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var statsSystem = entMan.System<AresStatsSystem>();
        var perkSystem = entMan.System<PerkSystem>();
        var stationSystem = entMan.System<StationSpawningSystem>();
        var testMap = await pair.CreateTestMap();

        await server.WaitAssertion(() =>
        {
            var mob = stationSystem.SpawnPlayerMob(testMap.GridCoords, job: null, new HumanoidCharacterProfile(), station: null);
            var statsComp = entMan.EnsureComponent<StatsComponent>(mob);

            var level = 10;
            foreach (var id in StatIds)
                statsSystem.SetStatLevel(mob, id, level++, statsComp);

            perkSystem.ApplyPerk(mob, "PaperWorm");

            foreach (var id in StatIds)
            {
                Assert.That(statsSystem.GetStatLevel(mob, id, statsComp),
                    Is.EqualTo(Array.IndexOf(StatIds, id)),
                    $"Wrong {id} level after Paper Worm");
            }

            var breakdown = new PositiveBreakdownChanceModifierEvent();
            entMan.EventBus.RaiseLocalEvent(mob, ref breakdown, true);
            Assert.That(breakdown.PositiveMultiplier, Is.EqualTo(1.2f));

            entMan.DeleteEntity(mob);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestRejectedGenius()
    {
        var pair = await PoolManager.GetServerClient(new PoolSettings()
        {
            Dirty = true,
        });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var perkSystem = entMan.System<PerkSystem>();
        var stationSystem = entMan.System<StationSpawningSystem>();
        var testMap = await pair.CreateTestMap();

        await server.WaitAssertion(() =>
        {
            var mob = stationSystem.SpawnPlayerMob(testMap.GridCoords, job: null, new HumanoidCharacterProfile(), station: null);

            perkSystem.ApplyPerk(mob, "RejectedGenius");

            var sanity = entMan.GetComponent<SanityComponent>(mob);
            Assert.That(sanity.MaxSanity, Is.EqualTo(80f));
            Assert.That(sanity.CurrentSanity, Is.EqualTo(80f));

            var breakdown = new PositiveBreakdownChanceModifierEvent();
            entMan.EventBus.RaiseLocalEvent(mob, ref breakdown, true);
            Assert.That(breakdown.PositiveMultiplier, Is.EqualTo(0f));

            var insight = new InsightGainModifierEvent();
            entMan.EventBus.RaiseLocalEvent(mob, ref insight, true);
            Assert.That(insight.Multiplier, Is.EqualTo(1.5f));

            entMan.DeleteEntity(mob);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestFreelancer()
    {
        var pair = await PoolManager.GetServerClient(new PoolSettings()
        {
            Dirty = true,
        });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var statsSystem = entMan.System<AresStatsSystem>();
        var perkSystem = entMan.System<PerkSystem>();
        var stationSystem = entMan.System<StationSpawningSystem>();
        var testMap = await pair.CreateTestMap();

        await server.WaitAssertion(() =>
        {
            var mob = stationSystem.SpawnPlayerMob(testMap.GridCoords, job: null, new HumanoidCharacterProfile(), station: null);
            var statsComp = entMan.EnsureComponent<StatsComponent>(mob);

            var level = 10;
            foreach (var id in StatIds)
                statsSystem.SetStatLevel(mob, id, level++, statsComp);

            perkSystem.ApplyPerk(mob, "Freelancer");

            // Highest stat (15) gets -10, the rest get +4.
            for (var i = 0; i < StatIds.Length; i++)
            {
                var expected = i == StatIds.Length - 1 ? 5 : 10 + i + 4;
                Assert.That(statsSystem.GetStatLevel(mob, StatIds[i], statsComp),
                    Is.EqualTo(expected),
                    $"Wrong {StatIds[i]} level after Freelancer");
            }

            entMan.DeleteEntity(mob);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestSeveralPerksPerRound()
    {
        var pair = await PoolManager.GetServerClient(new PoolSettings()
        {
            Dirty = true,
        });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var statsSystem = entMan.System<AresStatsSystem>();
        var perkSystem = entMan.System<PerkSystem>();
        var stationSystem = entMan.System<StationSpawningSystem>();
        var testMap = await pair.CreateTestMap();

        await server.WaitAssertion(() =>
        {
            var mob = stationSystem.SpawnPlayerMob(testMap.GridCoords, job: null, new HumanoidCharacterProfile(), station: null);
            var statsComp = entMan.EnsureComponent<StatsComponent>(mob);

            var level = 10;
            foreach (var id in StatIds)
                statsSystem.SetStatLevel(mob, id, level++, statsComp);

            perkSystem.ApplyPerk(mob, "Freelancer");
            perkSystem.ApplyPerk(mob, "PaperWorm");

            var perks = entMan.GetComponent<PerksComponent>(mob);
            Assert.That(perks.Perks, Has.Count.EqualTo(2));

            // Freelancer first: 15 -> 5, others +4; then Paper Worm: -10 to all.
            for (var i = 0; i < StatIds.Length; i++)
            {
                var expected = i == StatIds.Length - 1 ? 0 : 10 + i + 4 - 10;
                Assert.That(statsSystem.GetStatLevel(mob, StatIds[i], statsComp),
                    Is.EqualTo(expected),
                    $"Wrong {StatIds[i]} level after two perks");
            }

            entMan.DeleteEntity(mob);
        });

        await pair.CleanReturnAsync();
    }
}
