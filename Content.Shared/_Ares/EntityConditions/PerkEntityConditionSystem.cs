// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.EntityConditions;

public sealed partial class PerkEntityConditionSystem : EntityConditionSystem<PerksComponent, PerkCondition>
{
    protected override void Condition(Entity<PerksComponent> entity, ref EntityConditionEvent<PerkCondition> args)
    {
        args.Result = entity.Comp.Perks.Contains(args.Condition.Perk);
    }
}

public sealed partial class PerkCondition : EntityConditionBase<PerkCondition>
{
    [DataField(required: true)]
    public ProtoId<PerkPrototype> Perk;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype) =>
        Loc.GetString("entity-condition-guidebook-has-perk", ("perk", Perk), ("inverted", Inverted));
}