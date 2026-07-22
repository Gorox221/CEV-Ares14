// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Stats;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Stats;

public sealed partial class AresHackingSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<AccessReaderComponent, GetVerbsEvent<AlternativeVerb>>(AddHackVerb);
        SubscribeLocalEvent<AccessReaderComponent, AresHackingDoAfterEvent>(OnHackDoAfter);
    }

    private void AddHackVerb(Entity<AccessReaderComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || args.Hands == null)
            return;

        if (!ent.Comp.Enabled || ent.Comp.AccessLists.Count == 0)
            return;

        if (!_power.IsPowered(ent.Owner))
            return;

        var user = args.User;

        if (!HasComp<StatsComponent>(user))
            return;

        var cogPrototype = new ProtoId<StatPrototype>("Cognition");
        if (!_prototypes.HasIndex(cogPrototype))
            return;

        var cogLevel = _stats.GetStatLevel(user, cogPrototype);

        if (cogLevel <= 25)
            return;

        var time = Math.Max(1.0, 22.0 - cogLevel * 0.2);

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                StartHack(ent, user, time);
            },
            Text = Loc.GetString("ares-hacking-verb-hack"),
            Message = Loc.GetString("ares-hacking-verb-hack-tooltip", ("time", time.ToString("F1"))),
            Priority = 2
        };
        args.Verbs.Add(verb);
    }

    private void StartHack(EntityUid target, EntityUid user, double time)
    {
        if (!_net.IsServer)
            return;

        var userName = Identity.Name(user, EntityManager);
        _popup.PopupEntity(Loc.GetString("ares-hacking-popup-start", ("user", userName)), target, user);

        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            TimeSpan.FromSeconds(time),
            new AresHackingDoAfterEvent(),
            target,
            target)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnHackDoAfter(Entity<AccessReaderComponent> ent, ref AresHackingDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!_power.IsPowered(ent.Owner))
            return;

        _accessReader.SetActive(ent, false);
        var userName = Identity.Name(args.User, EntityManager);
        _popup.PopupEntity(Loc.GetString("ares-hacking-popup-success", ("user", userName)), ent, args.User);
        args.Handled = true;
    }
}
