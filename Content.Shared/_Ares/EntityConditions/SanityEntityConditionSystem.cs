// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.EntityConditions;

public sealed partial class SanityEntityConditionSystem : EntityConditionSystem<SanityComponent, SanityCondition>
{
    protected override void Condition(Entity<SanityComponent> entity, ref EntityConditionEvent<SanityCondition> args)
    {
        args.Result = entity.Comp.CurrentSanity >= args.Condition.Min && entity.Comp.CurrentSanity <= args.Condition.Max;
    }
}

public sealed partial class SanityCondition : EntityConditionBase<SanityCondition>
{
    [DataField]
    public float Min;

    [DataField]
    public float Max = float.PositiveInfinity;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype) =>
        Loc.GetString("entity-condition-guidebook-total-sanity",
            ("min", Min),
            ("max", float.IsPositiveInfinity(Max) ? int.MaxValue : Max));
}