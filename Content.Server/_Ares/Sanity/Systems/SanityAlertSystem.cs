// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Alert;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanityAlertSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SanityComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<SanityChangedEvent>(OnSanityChanged);
    }

    private void OnMapInit(Entity<SanityComponent> ent, ref MapInitEvent args)
    {
        UpdateAlert(ent, ent.Comp);
    }

    private void OnComponentStartup(Entity<SanityComponent> ent, ref ComponentStartup args)
    {
        UpdateAlert(ent, ent.Comp);
    }

    private void OnSanityChanged(ref SanityChangedEvent args)
    {
        var uid = args.Entity;

        if (!TryComp<SanityComponent>(uid, out var sanity))
            return;

        UpdateAlert(uid, sanity);
    }

    private void UpdateAlert(EntityUid uid, SanityComponent comp)
    {
        var ratio = comp.CurrentSanity / comp.MaxSanity;

        short severity;
        if (ratio >= 0.8f)
            severity = 0;
        else if (ratio >= 0.6f)
            severity = 1;
        else if (ratio >= 0.4f)
            severity = 2;
        else if (ratio >= 0.2f)
            severity = 3;
        else if (ratio > 0f)
            severity = 4;
        else
            severity = 5;

        _alerts.ShowAlert(uid, comp.SanityAlertType, severity);
    }
}
