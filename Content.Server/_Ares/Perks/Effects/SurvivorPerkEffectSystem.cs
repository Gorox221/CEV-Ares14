// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Sanity.Events;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class SurvivorPerkEffectSystem : PerkQueryEffectSystem<SurvivorPerkEffect>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PerksComponent, DeathWitnessedEvent>(OnDeathWitnessed);
    }

    private void OnDeathWitnessed(Entity<PerksComponent> ent, ref DeathWitnessedEvent args)
    {
        if (args.Delta >= 0f)
            return;

        args.Delta *= GetMultiplier(ent, effect => effect.Multiplier);
    }
}