// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;
using Content.Shared._Ares.Sanity.Components;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class MaxSanityPerkEffectSystem : PerkEffectSystem<MaxSanityPerkEffect>
{
    protected override void OnEffectApplied(Entity<PerksComponent> ent, ref PerkEffectAppliedEvent<MaxSanityPerkEffect> args)
    {
        if (!TryComp<SanityComponent>(ent, out var sanity))
            return;

        sanity.MaxSanity = Math.Max(0f, sanity.MaxSanity + args.Effect.Delta);
        sanity.CurrentSanity = Math.Min(sanity.CurrentSanity, sanity.MaxSanity);
        Dirty(ent, sanity);
    }
}