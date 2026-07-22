using Robust.Shared.Configuration;

namespace Content.Shared._ADT.CCVar;

[CVarDefs]
public sealed class EconomyCCVars
{
    public static readonly CVarDef<bool> PaySalary =
        CVarDef.Create("economy.pay_salary", true, CVar.SERVERONLY | CVar.ARCHIVE);
}
