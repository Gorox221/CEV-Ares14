// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Stats;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanityDeathSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (!HasComp<HumanoidAppearanceComponent>(args.Target))
            return;

        var deadXform = Transform(args.Target);
        var range = 8f;

        var viewers = _lookup.GetEntitiesInRange<SanityComponent>(deadXform.Coordinates, range);
        foreach (var (viewerUid, sanity) in viewers)
        {
            if (viewerUid == args.Target)
                continue;

            if (!CanPerceiveSanityEffects(viewerUid))
                continue;

            var viewerPos = _transform.ToMapCoordinates(Transform(viewerUid).Coordinates);
            var deadPos = _transform.ToMapCoordinates(deadXform.Coordinates);

            if (!_interaction.InRangeUnobstructed(deadPos, viewerPos, range))
                continue;

            var vigMultiplier = GetVigilanceMultiplier(viewerUid);
            var rawDelta = 10f * vigMultiplier;
            var oldValue = sanity.CurrentSanity;
            var newValue = Math.Clamp(oldValue - rawDelta, sanity.MinSanity, sanity.MaxSanity);

            if (MathHelper.CloseTo(oldValue, newValue))
                continue;

            sanity.CurrentSanity = newValue;
            Dirty(viewerUid, sanity);

            var ev = new SanityChangedEvent(viewerUid, oldValue, newValue, oldValue - newValue);
            RaiseLocalEvent(viewerUid, ref ev);
        }
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
