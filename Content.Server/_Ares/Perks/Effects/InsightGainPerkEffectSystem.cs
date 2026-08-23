// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Sanity.Events;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class InsightGainPerkEffectSystem : PerkQueryEffectSystem<InsightGainPerkEffect>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PerksComponent, InsightGainModifierEvent>(OnInsightGain);
    }

    private void OnInsightGain(Entity<PerksComponent> ent, ref InsightGainModifierEvent args)
    {
        args.Multiplier *= GetMultiplier(ent, effect => effect.Multiplier);
    }
}