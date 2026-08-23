// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Sanity.Events;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class NegativeBreakdownChancePerkEffectSystem : PerkQueryEffectSystem<NegativeBreakdownChancePerkEffect>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PerksComponent, NegativeBreakdownChanceModifierEvent>(OnBreakdownChance);
    }

    private void OnBreakdownChance(Entity<PerksComponent> ent, ref NegativeBreakdownChanceModifierEvent args)
    {
        args.Multiplier *= GetMultiplier(ent, effect => effect.Multiplier);
    }
}
