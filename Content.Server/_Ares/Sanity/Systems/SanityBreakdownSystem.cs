// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Sanity.Prototypes;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Picks a breakdown when sanity hits zero and dispatches its behavior to the entity,
/// where each breakdown type is handled by its own system.
/// </summary>
public sealed partial class SanityBreakdownSystem : EntitySystem, ISanityBreakdownTrigger
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SanityChangedEvent>(OnSanityChanged);
        SubscribeLocalEvent<SanityCheckEvent>(OnSanityCheck);
    }

    private void OnSanityCheck(ref SanityCheckEvent args)
    {
        if (!TryComp<SanityComponent>(args.Entity, out var sanity))
            return;

        TryTriggerBreakdown(args.Entity, sanity);
    }

    private void OnSanityChanged(ref SanityChangedEvent args)
    {
        var uid = args.Entity;
        if (!TryComp<SanityComponent>(uid, out var sanity))
            return;

        TryTriggerBreakdown(uid, sanity);
    }

    private void TryTriggerBreakdown(EntityUid uid, SanityComponent sanity)
    {
        if (sanity.CurrentSanity > 0 || sanity.CurrentBreakdown != null || _timing.CurTime < sanity.NextBreakdownTime)
            return;

        var breakdown = PickBreakdown();
        if (breakdown == null)
            return;

        sanity.NextBreakdownTime = _timing.CurTime + TimeSpan.FromMinutes(7);
        sanity.CurrentBreakdown = breakdown.ID;
        Dirty(uid, sanity);

        _popup.PopupEntity(Loc.GetString(breakdown.Popup), uid, uid);
        breakdown.Behavior.Trigger((uid, sanity), this);
    }

    public void RaiseBreakdown<T>(Entity<SanityComponent> ent, T behavior) where T : SanityBreakdownBehavior<T>
    {
        var ev = new SanityBreakdownTriggeredEvent<T>(behavior);
        RaiseLocalEvent(ent, ref ev, true);
    }

    private SanityBreakdownPrototype? PickBreakdown()
    {
        var all = _prototypes.EnumeratePrototypes<SanityBreakdownPrototype>().ToList();
        if (all.Count == 0)
            return null;

        var totalWeight = all.Sum(p => p.Weight);
        var roll = (float)_random.NextDouble() * totalWeight;

        foreach (var proto in all)
        {
            roll -= proto.Weight;
            if (roll <= 0)
                return proto;
        }

        return all[^1];
    }
}
