// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Handles the absolute concentration breakdown: instantly restores full sanity,
/// grants immunity to sanity loss while active, then ends with a popup once the
/// duration elapses.
/// </summary>
public sealed partial class AbsoluteConcentrationBreakdownSystem : SanityBreakdownEffectSystem<AbsoluteConcentrationBreakdownBehavior>
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SanityEmoteSystem _sanityEmote = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AbsoluteConcentrationComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<AbsoluteConcentrationComponent, SanityComponent>();
        while (query.MoveNext(out var uid, out var concentration, out var sanity))
        {
            if (_timing.CurTime < concentration.EndTime)
                continue;

            EndBreakdown(uid, sanity);
        }
    }

    protected override void OnBreakdownTriggered(Entity<SanityComponent> ent, ref SanityBreakdownTriggeredEvent<AbsoluteConcentrationBreakdownBehavior> args)
    {
        if (args.Behavior.Duration <= 0f)
            return;

        var oldValue = ent.Comp.CurrentSanity;
        var newValue = ent.Comp.MaxSanity;
        ent.Comp.CurrentSanity = newValue;
        Dirty(ent, ent.Comp);

        var ev = new SanityChangedEvent(ent, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(ent, ref ev, true);

        var concentration = EnsureComp<AbsoluteConcentrationComponent>(ent);
        concentration.EndTime = _timing.CurTime + TimeSpan.FromSeconds(args.Behavior.Duration);
    }

    private void EndBreakdown(EntityUid uid, SanityComponent sanity)
    {
        RemComp<AbsoluteConcentrationComponent>(uid);
        _sanityEmote.StopEmoting(uid);
        sanity.CurrentBreakdown = null;
        Dirty(uid, sanity);

        _popup.PopupEntity(Loc.GetString("sanity-breakdown-absoluteconcentration-end-popup"), uid, uid);
    }

    private void OnMobStateChanged(Entity<AbsoluteConcentrationComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.Dead or MobState.Critical)
            RemComp<AbsoluteConcentrationComponent>(ent);
    }
}
