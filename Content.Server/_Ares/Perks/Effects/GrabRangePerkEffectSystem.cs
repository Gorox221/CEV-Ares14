// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class GrabRangePerkEffectSystem : PerkEffectSystem<GrabRangePerkEffect>
{
    protected override void OnEffectApplied(Entity<PerksComponent> ent, ref PerkEffectAppliedEvent<GrabRangePerkEffect> args)
    {
        var comp = EnsureComp<GrabRangeComponent>(ent);
        comp.RangeExtension = Math.Max(comp.RangeExtension, args.Effect.RangeExtension);
        Dirty(ent, comp);
    }
}
