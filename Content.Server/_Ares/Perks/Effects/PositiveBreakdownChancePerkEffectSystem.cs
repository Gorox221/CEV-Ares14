// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Sanity.Events;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class PositiveBreakdownChancePerkEffectSystem : PerkQueryEffectSystem<PositiveBreakdownChancePerkEffect>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PerksComponent, PositiveBreakdownChanceModifierEvent>(OnBreakdownChance);
    }

    private void OnBreakdownChance(Entity<PerksComponent> ent, ref PositiveBreakdownChanceModifierEvent args)
    {
        args.PositiveMultiplier *= GetMultiplier(ent, effect => effect.Multiplier);
    }
}