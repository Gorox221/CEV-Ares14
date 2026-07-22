using Content.Shared.Cargo.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._ADT.Economy;

[RegisterComponent, NetworkedComponent]
public sealed partial class BankCardComponent : Component
{
    [DataField]
    public int? AccountId;

    [DataField]
    public int StartingBalance;

    [DataField]
    public bool CommandBudgetCard;

    [DataField]
    public ProtoId<CargoAccountPrototype>? CommandBudgetType;
}
