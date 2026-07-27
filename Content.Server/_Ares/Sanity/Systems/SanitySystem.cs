// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Interaction;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanitySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
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

        var oldValue = sanity.CurrentSanity;
        var newValue = Math.Clamp(oldValue + totalChange, sanity.MinSanity, sanity.MaxSanity);
        sanity.CurrentSanity = newValue;
        Dirty(uid, sanity);

        var changedEv = new SanityChangedEvent(uid, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(uid, ref changedEv);
    }
}
