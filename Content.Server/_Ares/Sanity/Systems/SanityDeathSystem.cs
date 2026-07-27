// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanityDeathSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
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

            var viewerPos = _transform.ToMapCoordinates(Transform(viewerUid).Coordinates);
            var deadPos = _transform.ToMapCoordinates(deadXform.Coordinates);

            if (!_interaction.InRangeUnobstructed(deadPos, viewerPos, range))
                continue;

            var oldValue = sanity.CurrentSanity;
            var newValue = Math.Clamp(oldValue - 10f, sanity.MinSanity, sanity.MaxSanity);

            if (MathHelper.CloseTo(oldValue, newValue))
                continue;

            sanity.CurrentSanity = newValue;
            Dirty(viewerUid, sanity);

            var ev = new SanityChangedEvent(viewerUid, oldValue, newValue, oldValue - newValue);
            RaiseLocalEvent(viewerUid, ref ev);
        }
    }
}
