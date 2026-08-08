// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Origins;
using Content.Shared._Ares.Stats;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Origins;

public sealed class OriginSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly AresStatsSystem _statsSystem = default!;

    public void ApplyOrigin(EntityUid mob, ProtoId<OriginPrototype> originId)
    {
        if (string.IsNullOrEmpty(originId))
            return;

        if (!_prototypes.TryIndex(originId, out OriginPrototype? origin))
        {
            Log.Error($"No origin found with ID {originId}!");
            return;
        }

        foreach (var (stat, delta) in origin.Stats)
        {
            _statsSystem.ModifyStatLevel(mob, stat, delta);
        }
    }
}