// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Sanity.Components;
using Content.Server.Chat.Systems;
using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Chat;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class HeraldBreakdownSystem : SanityBreakdownEffectSystem<HeraldBreakdownBehavior>
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SanityQuoteSystem _sanityQuote = default!;
    [Dependency] private readonly SanityEmoteSystem _sanityEmote = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HeraldBreakdownComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<HeraldBreakdownComponent, SanityComponent>();
        while (query.MoveNext(out var uid, out var herald, out var sanity))
        {
            if (_timing.CurTime >= herald.EndTime)
            {
                EndBreakdown(uid, herald, sanity);
                continue;
            }

            herald.QuoteAccumulator += frameTime;
            if (herald.QuoteAccumulator < herald.QuoteInterval)
                continue;

            herald.QuoteAccumulator = 0f;
            _chat.TrySendInGameICMessage(uid,
                _sanityQuote.PickInsaneQuote(),
                InGameICChatType.Speak,
                hideChat: false,
                ignoreActionBlocker: true);
        }
    }

    protected override void OnBreakdownTriggered(Entity<SanityComponent> ent, ref SanityBreakdownTriggeredEvent<HeraldBreakdownBehavior> args)
    {
        if (args.Behavior.Duration <= 0f)
            return;

        var herald = EnsureComp<HeraldBreakdownComponent>(ent);
        herald.EndTime = _timing.CurTime + TimeSpan.FromSeconds(args.Behavior.Duration);
        herald.QuoteAccumulator = 0f;
        herald.QuoteInterval = args.Behavior.QuoteInterval;
        herald.ReturnSanity = args.Behavior.SanityReturn;
    }

    private void EndBreakdown(EntityUid uid, HeraldBreakdownComponent herald, SanityComponent sanity)
    {
        var oldValue = sanity.CurrentSanity;
        var newValue = Math.Clamp(herald.ReturnSanity, sanity.MinSanity, sanity.MaxSanity);

        RemComp<HeraldBreakdownComponent>(uid);
        _sanityEmote.StopEmoting(uid);
        sanity.CurrentSanity = newValue;
        sanity.CurrentBreakdown = null;
        Dirty(uid, sanity);

        _popup.PopupEntity(Loc.GetString("sanity-breakdown-herald-end-popup"), uid, uid);

        var ev = new SanityChangedEvent(uid, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(uid, ref ev, true);
    }

    private void OnMobStateChanged(Entity<HeraldBreakdownComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.Dead or MobState.Critical)
        {
            RemComp<HeraldBreakdownComponent>(ent);
            _sanityEmote.StopEmoting(ent);
        }
    }
}