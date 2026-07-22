using Content.Server.Station.Systems;
using Content.Shared.Paper;
using Content.Shared.Cargo.Prototypes;

namespace Content.Server._ADT.Economy;

public sealed class CommandBudgetSystem : EntitySystem
{
    [Dependency] private readonly BankCardSystem _bankCard = default!;
    [Dependency] private readonly PaperSystem _paper = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CommandBudgetPinPaperComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, CommandBudgetPinPaperComponent component, MapInitEvent args)
    {
        if (component.CommandBudgetType == null)
            return;

        var account = _bankCard.FindDepartmentAccount(component.CommandBudgetType.Value);

        if (account != null)
        {
            var pin = account.AccountPin;
            _paper.SetContent((uid, EnsureComp<PaperComponent>(uid)), Loc.GetString("command-budget-pin-message", ("pin", pin)));
        }
    }
}
