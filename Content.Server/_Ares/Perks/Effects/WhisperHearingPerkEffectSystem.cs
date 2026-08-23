// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class WhisperHearingPerkEffectSystem : PerkEffectSystem<WhisperHearingPerkEffect>
{
    protected override void OnEffectApplied(Entity<PerksComponent> ent, ref PerkEffectAppliedEvent<WhisperHearingPerkEffect> args)
    {
        var comp = EnsureComp<WhisperHearingComponent>(ent);
        comp.ClearRangeExtension = Math.Max(comp.ClearRangeExtension, args.Effect.ClearRangeExtension);
    }
}