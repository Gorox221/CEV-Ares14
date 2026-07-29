// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Stats;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanitySystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SanityComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var sanity, out var xform))
        {
            sanity.Accumulator += frameTime;
            if (sanity.Accumulator < sanity.CheckInterval)
                continue;

            sanity.Accumulator = 0f;
            ProcessSanityCheck(uid, sanity, xform);
        }
    }

    private void ProcessSanityCheck(EntityUid uid, SanityComponent sanity, TransformComponent xform)
    {
        if (!CanPerceiveSanityEffects(uid))
            return;

        var nearby = _lookup.GetEntitiesInRange<SanityAffectorComponent>(xform.Coordinates, sanity.Range);
        var totalChange = 0f;

        foreach (var (affectorUid, affector) in nearby)
        {
            if (affectorUid == uid)
                continue;

            if (!TryComp(affectorUid, out TransformComponent? affectorXform))
                continue;

            var distance = (_transform.GetWorldPosition(affectorXform) - _transform.GetWorldPosition(xform)).Length();
            if (distance > affector.Range)
                continue;

            if (affector.RequiresLineOfSight
                && !_interaction.InRangeUnobstructed(uid, affectorUid, distance))
                continue;

            totalChange += affector.SanityChange;
        }

        var ev = new GetSanityAffectorsEvent(uid);
        RaiseLocalEvent(uid, ref ev);
        totalChange += ev.TotalChange;

        if (MathHelper.CloseTo(totalChange, 0f))
            return;

        if (totalChange < 0f)
            totalChange *= GetVigilanceMultiplier(uid);

        var oldValue = sanity.CurrentSanity;
        var newValue = Math.Clamp(oldValue + totalChange, sanity.MinSanity, sanity.MaxSanity);
        sanity.CurrentSanity = newValue;
        Dirty(uid, sanity);

        var changedEv = new SanityChangedEvent(uid, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(uid, ref changedEv);
    }

    private float GetVigilanceMultiplier(EntityUid uid)
    {
        var vigPrototype = new ProtoId<StatPrototype>("Vigilance");
        if (!_prototypes.HasIndex(vigPrototype))
            return 1f;

        var vigLevel = _stats.GetStatLevel(uid, vigPrototype);
        var clampedVig = Math.Clamp(vigLevel, 0, 60);
        return (float)(1.2 - clampedVig / 60.0);
    }

    private bool CanPerceiveSanityEffects(EntityUid uid)
    {
        if (TryComp<MobStateComponent>(uid, out var mobState)
            && mobState.CurrentState is MobState.Dead or MobState.Critical)
            return false;

        if (TryComp<BlindableComponent>(uid, out var blindable) && blindable.IsBlind)
            return false;

        return true;
    }
}
