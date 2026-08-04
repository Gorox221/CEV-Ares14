// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Behaviors;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Applies sanity changes of nearby <see cref="SanityAffectorComponent"/> entities
/// (e.g. horror auras) every sanity check tick.
/// </summary>
public sealed partial class AuraSanityChangeSystem : SanityChangeSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SanityCheckEvent>(OnSanityCheck);
    }

    private void OnSanityCheck(ref SanityCheckEvent args)
    {
        var uid = args.Entity;

        if (!CanPerceiveSanityEffects(uid))
            return;

        if (!TryComp<SanityComponent>(uid, out var sanity))
            return;

        var xform = Transform(uid);
        var nearby = _lookup.GetEntitiesInRange<SanityAffectorComponent>(xform.Coordinates, sanity.Range);
        var totalChange = 0f;

        foreach (var (affectorUid, affector) in nearby)
        {
            if (affectorUid == uid)
                continue;

            var behavior = affector.Behavior;
            if (behavior == null)
                continue;

            if (behavior.RequiresAlive && !_mobState.IsAlive(affectorUid))
                continue;

            if (!TryComp(affectorUid, out TransformComponent? affectorXform))
                continue;

            var distance = (_transform.GetWorldPosition(affectorXform) - _transform.GetWorldPosition(xform)).Length();
            if (distance > behavior.Range)
                continue;

            if (behavior.RequiresLineOfSight
                && !_interaction.InRangeUnobstructed(uid, affectorUid, distance))
                continue;

            totalChange += behavior.Change;
        }

        if (MathHelper.CloseTo(totalChange, 0f))
            return;

        if (totalChange < 0f)
            totalChange *= GetVigilanceMultiplier(uid);

        ApplyChange((uid, sanity), totalChange);
    }
}
