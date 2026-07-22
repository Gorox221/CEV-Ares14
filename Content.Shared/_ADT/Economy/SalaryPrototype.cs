using Robust.Shared.Prototypes;

namespace Content.Shared._ADT.Economy;

[Prototype("salary")]
public sealed partial class SalaryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("salaries")]
    public Dictionary<string, int> Salaries = new();
}
