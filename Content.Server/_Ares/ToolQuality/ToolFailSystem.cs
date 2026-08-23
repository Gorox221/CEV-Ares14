// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Wires;
using Content.Shared._Ares.Stats;
using Content.Shared._Ares.ToolQuality;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Wires;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Ares.ToolQuality;

public sealed class ToolFailSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ToolActionFailCheckEvent>(OnFailCheck);
        SubscribeLocalEvent<ToolActionCritFailEvent>(OnCritFail);

        SubscribeLocalEvent<WireDoAfterEvent>(OnWireDoAfterBroadcast);
    }

    private void OnFailCheck(ref ToolActionFailCheckEvent args)
    {
        if (!TryComp<ToolActionFailComponent>(args.Target, out var failComp))
            return;

        // Skip entities with WiresComponent — wire cutting handled separately
        if (HasComp<WiresComponent>(args.Target))
            return;

        if (TryRollFail(args.User, args.Tool, failComp, out var critFail))
        {
            args.Cancelled = true;
            args.CritFailed = critFail;
            ShowFailPopup(args.User);
        }
    }

    private void OnCritFail(ToolActionCritFailEvent args)
    {
        if (!TryComp<ToolActionFailComponent>(args.Target, out var failComp))
            return;

        ApplyCritFailEffect(args.User, args.Tool);
    }

    private void OnWireDoAfterBroadcast(WireDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (args.Args is not { Target: { } target, Used: { } tool })
            return;

        var user = args.Args.User;

        if (!TryComp<ToolActionFailComponent>(target, out var failComp))
            return;

        if (TryRollFail(user, tool, failComp, out var critFail))
        {
            args.Handled = true;
            ShowFailPopup(user);
            if (critFail)
                ApplyCritFailEffect(user, tool);
        }
    }

    private bool TryRollFail(EntityUid user, EntityUid tool, ToolActionFailComponent failComp, out bool critFail)
    {
        critFail = false;

        var statId = failComp.Stat;
        if (!_prototypes.HasIndex(statId))
            return false;

        var statLevel = _stats.GetStatLevel(user, statId);

        var quality = 0;
        if (TryComp<ToolQualityComponent>(tool, out var qualityComp))
            quality = qualityComp.Quality;

        var failChance = failComp.BaseFailChance - quality - statLevel;
        failChance = Math.Clamp(failChance, 0, 100);

        if (failChance <= 0)
            return false;

        if (!_random.Prob(failChance / 100f))
            return false;

        var critFailChance = 25 - statLevel;
        critFailChance = Math.Clamp(critFailChance, 0, 100);

        if (critFailChance > 0 && _random.Prob(critFailChance / 100f))
            critFail = true;

        return true;
    }

    private void ShowFailPopup(EntityUid user)
    {
        _popup.PopupEntity(Loc.GetString("ares-tool-fail"), user, user);
    }

    private void ApplyCritFailEffect(EntityUid user, EntityUid tool)
    {
        switch (_random.Next(3))
        {
            case 0:
                _hands.TryDrop(user, tool);
                _popup.PopupEntity(Loc.GetString("ares-tool-critfail-drop"), user, user);
                break;
            case 1:
                _stun.TryKnockdown(user, TimeSpan.FromSeconds(3));
                _popup.PopupEntity(Loc.GetString("ares-tool-critfail-slip"), user, user);
                break;
            case 2:
            {
                var damage = GetToolDamage(tool);
                if (damage != null)
                    _damageable.TryChangeDamage(user, damage, origin: tool);
                _popup.PopupEntity(Loc.GetString("ares-tool-critfail-selfdamage"), user, user);
                break;
            }
        }
    }

    private DamageSpecifier? GetToolDamage(EntityUid tool)
    {
        if (TryComp<MeleeWeaponComponent>(tool, out var melee))
            return melee.Damage;

        return new DamageSpecifier(_prototypes.Index<DamageTypePrototype>("Blunt"), 10);
    }
}
