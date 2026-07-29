// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client._Ares.Sanity.Overlays;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client._Ares.Sanity.Systems;

public sealed class SanityOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;

    private SanityStaticOverlay _overlay = default!;

    public override void Initialize()
    {
        _overlay = new SanityStaticOverlay();

        if (_playerMan.LocalEntity != null)
            _overlayMan.AddOverlay(_overlay);

        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnLocalPlayerAttached);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnLocalPlayerDetached);
    }

    public override void Shutdown()
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnLocalPlayerAttached(LocalPlayerAttachedEvent ev)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnLocalPlayerDetached(LocalPlayerDetachedEvent ev)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }
}
