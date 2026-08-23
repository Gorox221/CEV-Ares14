// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class HystericBreakdownSystem : SanityBreakdownEffectSystem<HystericBreakdownBehavior>
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SanityEmoteSystem _sanityEmote = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HystericBreakdownComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<HystericBreakdownComponent, SanityComponent>();
        while (query.MoveNext(out var uid, out var hysteric, out var sanity))
        {
            if (_timing.CurTime >= hysteric.EndTime)
            {
                EndBreakdown(uid, hysteric, sanity);
                continue;
            }

            hysteric.StaminaAccumulator += frameTime;
            if (hysteric.StaminaAccumulator < hysteric.StaminaInterval)
                continue;

            hysteric.StaminaAccumulator = 0f;
            _stamina.TakeStaminaDamage(uid, hysteric.StaminaDamage, visual: false);
        }
    }

    protected override void OnBreakdownTriggered(Entity<SanityComponent> ent, ref SanityBreakdownTriggeredEvent<HystericBreakdownBehavior> args)
    {
        if (args.Behavior.Duration <= 0f)
            return;

        var knockdownTime = _timing.TickPeriod * Math.Max(0, args.Behavior.KnockdownTicks);
        _stun.TryKnockdown(ent.Owner, knockdownTime, refresh: true, autoStand: true, drop: true, force: true);

        EnsureComp<MutedComponent>(ent);

        var hysteric = EnsureComp<HystericBreakdownComponent>(ent);
        hysteric.EndTime = _timing.CurTime + TimeSpan.FromSeconds(args.Behavior.Duration);
        hysteric.StaminaAccumulator = 0f;
        hysteric.StaminaInterval = args.Behavior.StaminaInterval;
        hysteric.StaminaDamage = args.Behavior.StaminaDamage;
        hysteric.ReturnSanity = args.Behavior.SanityReturn;
    }

    private void EndBreakdown(EntityUid uid, HystericBreakdownComponent hysteric, SanityComponent sanity)
    {
        var oldValue = sanity.CurrentSanity;
        var newValue = Math.Clamp(hysteric.ReturnSanity, sanity.MinSanity, sanity.MaxSanity);

        RemComp<HystericBreakdownComponent>(uid);
        RemComp<MutedComponent>(uid);
        _sanityEmote.StopEmoting(uid);
        sanity.CurrentSanity = newValue;
        sanity.CurrentBreakdown = null;
        Dirty(uid, sanity);

        _popup.PopupEntity(Loc.GetString("sanity-breakdown-hysteric-end-popup"), uid, uid);

        var ev = new SanityChangedEvent(uid, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(uid, ref ev, true);
    }

    private void OnMobStateChanged(Entity<HystericBreakdownComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.Dead or MobState.Critical)
        {
            RemComp<HystericBreakdownComponent>(ent);
            RemComp<MutedComponent>(ent);
            _sanityEmote.StopEmoting(ent);
        }
    }
}
