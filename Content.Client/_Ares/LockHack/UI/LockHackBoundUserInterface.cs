// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.LockHack;
using Robust.Client.UserInterface;

namespace Content.Client._Ares.LockHack.UI;

public sealed class LockHackBoundUserInterface : BoundUserInterface
{
    private LockHackMenu? _menu;

    public LockHackBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<LockHackMenu>();
        _menu.OnPulse += OnPulse;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        _menu?.Populate((LockHackBoundUserInterfaceState)state);
    }

    private void OnPulse(int wireId)
    {
        SendMessage(new LockHackPulseMessage(wireId));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _menu?.Dispose();
    }
}
