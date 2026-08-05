// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Stats;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class LessonLearntBreakdownSystem : SanityBreakdownEffectSystem<LessonLearntBreakdownBehavior>
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AresStatsSystem _stats = default!;

    protected override void OnBreakdownTriggered(Entity<SanityComponent> ent, ref SanityBreakdownTriggeredEvent<LessonLearntBreakdownBehavior> args)
    {
        foreach (var stat in _prototypes.EnumeratePrototypes<StatPrototype>())
        {
            var amount = _random.Next(args.Behavior.StatMinIncrease, args.Behavior.StatMaxIncrease + 1);
            _stats.ModifyStatLevel(ent, new ProtoId<StatPrototype>(stat.ID), amount);
        }

        var oldValue = ent.Comp.CurrentSanity;
        var newValue = Math.Clamp(args.Behavior.SanityReturn, ent.Comp.MinSanity, ent.Comp.MaxSanity);
        ent.Comp.CurrentSanity = newValue;
        ent.Comp.CurrentBreakdown = null;
        Dirty(ent, ent.Comp);

        var ev = new SanityChangedEvent(ent, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(ent, ref ev, true);
    }
}