// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class TheFabricBreakdownSystem : SanityBreakdownEffectSystem<TheFabricBreakdownBehavior>
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SanityEmoteSystem _sanityEmote = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TheFabricBreakdownComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<TheFabricBreakdownComponent, SanityComponent>();
        while (query.MoveNext(out var uid, out var fabric, out var sanity))
        {
            if (_timing.CurTime >= fabric.EndTime)
            {
                EndBreakdown(uid, fabric, sanity);
                continue;
            }

            fabric.Accumulator += frameTime;
            if (fabric.Accumulator < fabric.UpdateInterval)
                continue;

            fabric.Accumulator = 0f;
            RefreshTargets(uid, fabric);
        }
    }

    protected override void OnBreakdownTriggered(Entity<SanityComponent> ent, ref SanityBreakdownTriggeredEvent<TheFabricBreakdownBehavior> args)
    {
        if (args.Behavior.Duration <= 0f)
            return;

        var fabric = EnsureComp<TheFabricBreakdownComponent>(ent);
        fabric.EndTime = _timing.CurTime + TimeSpan.FromSeconds(args.Behavior.Duration);
        fabric.Accumulator = 0f;
        fabric.UpdateInterval = args.Behavior.UpdateInterval;
        fabric.Range = args.Behavior.Range;
        fabric.Textures = new(args.Behavior.Textures);
        RefreshTargets(ent.Owner, fabric);
    }

    private void RefreshTargets(EntityUid uid, TheFabricBreakdownComponent fabric)
    {
        var targets = new List<NetEntity>();

        var range = fabric.Range;
        var rangeSqr = range * range;
        var holderXform = Transform(uid);
        var holderPos = holderXform.WorldPosition;
        var mapId = holderXform.MapID;

        var query = EntityQueryEnumerator<HumanoidAppearanceComponent, TransformComponent>();
        while (query.MoveNext(out var target, out _, out var xform))
        {
            if (target == uid || !_mobState.IsAlive(target))
                continue;

            if (xform.MapID != mapId)
                continue;

            if (Vector2.DistanceSquared(xform.WorldPosition, holderPos) > rangeSqr)
                continue;

            targets.Add(GetNetEntity(target));
        }

        if (fabric.Targets.SequenceEqual(targets))
            return;

        fabric.Targets = targets;
        Dirty(uid, fabric);
    }

    private void EndBreakdown(EntityUid uid, TheFabricBreakdownComponent fabric, SanityComponent sanity)
    {
        RemComp<TheFabricBreakdownComponent>(uid);
        _sanityEmote.StopEmoting(uid);
        sanity.CurrentBreakdown = null;
        Dirty(uid, sanity);

        _popup.PopupEntity(Loc.GetString("sanity-breakdown-fabric-end-popup"), uid, uid);
    }

    private void OnMobStateChanged(Entity<TheFabricBreakdownComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.Dead or MobState.Critical)
        {
            RemComp<TheFabricBreakdownComponent>(ent);
            _sanityEmote.StopEmoting(ent);
        }
    }
}
