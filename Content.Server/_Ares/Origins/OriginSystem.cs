// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Origins;
using Content.Shared._Ares.Stats;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Origins;

public sealed class OriginSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly AresStatsSystem _statsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        var originId = ev.Profile.Origin;
        if (string.IsNullOrEmpty(originId))
            return;

        if (!_prototypes.TryIndex(originId, out OriginPrototype? origin))
        {
            Log.Error($"No origin found with ID {originId}!");
            return;
        }

        foreach (var (stat, delta) in origin.Stats)
        {
            _statsSystem.ModifyStatLevel(ev.Mob, stat, delta);
        }
    }
}