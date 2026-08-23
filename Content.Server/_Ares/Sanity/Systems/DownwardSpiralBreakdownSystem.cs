// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class DownwardSpiralBreakdownSystem : SanityBreakdownEffectSystem<DownwardSpiralBreakdownBehavior>
{
    protected override void OnBreakdownTriggered(Entity<SanityComponent> ent, ref SanityBreakdownTriggeredEvent<DownwardSpiralBreakdownBehavior> args)
    {
        ent.Comp.MaxSanity = Math.Max(0f, ent.Comp.MaxSanity - args.Behavior.MaxSanityPenalty);

        var oldValue = ent.Comp.CurrentSanity;
        var newValue = Math.Clamp(args.Behavior.SanityReturn, ent.Comp.MinSanity, ent.Comp.MaxSanity);
        ent.Comp.CurrentSanity = newValue;
        ent.Comp.CurrentBreakdown = null;
        Dirty(ent, ent.Comp);

        var ev = new SanityChangedEvent(ent, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(ent, ref ev, true);
    }
}