// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared.Mobs.Systems;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class GasEmissionPerkEffectSystem : PerkQueryEffectSystem<GasEmissionPerkEffect>
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private float _accumulator;

    public override void Update(float frameTime)
    {
        _accumulator += frameTime;
        if (_accumulator < 1f)
            return;

        var seconds = _accumulator;
        _accumulator = 0f;

        var query = EntityQueryEnumerator<PerksComponent>();
        while (query.MoveNext(out var uid, out var perks))
        {
            if (!_mobState.IsAlive(uid))
                continue;

            var effect = GetEffect((uid, perks));
            if (effect == null)
                continue;

            var tileMix = _atmosphere.GetTileMixture(uid, excite: true);
            tileMix?.AdjustMoles(effect.Gas, effect.MolesPerSecond * seconds);
        }
    }
}
