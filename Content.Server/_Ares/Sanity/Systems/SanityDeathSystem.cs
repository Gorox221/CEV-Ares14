// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Behaviors;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using System.Linq;

namespace Content.Server._Ares.Sanity.Systems;
/// <summary>
/// Lowers sanity of nearby humanoids that perceive someone die.
/// </summary>
public sealed partial class SanityDeathSystem : SanityChangeSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || args.OldMobState == MobState.Dead)
            return;

        var deadUid = args.Target;
        if (!HasComp<HumanoidAppearanceComponent>(deadUid))
            return;

        var deadXform = Transform(deadUid);
        var deadPos = _transform.GetWorldPosition(deadXform);

        var viewers = _lookup.GetEntitiesInRange<SanityComponent>(deadXform.Coordinates, 8f);
        foreach (var (viewerUid, viewer) in viewers)
        {
            if (viewerUid == deadUid)
                continue;

            var behavior = viewer.Changes.OfType<DeathSanityChangeBehavior>().FirstOrDefault();
            if (behavior == null)
                continue;

            if (!CanPerceiveSanityEffects(viewerUid))
                continue;

            var viewerXform = Transform(viewerUid);
            if ((_transform.GetWorldPosition(viewerXform) - deadPos).Length() > behavior.Range)
                continue;

            var delta = behavior.Change;
            if (delta < 0f)
                delta *= GetVigilanceMultiplier(viewerUid);

            ApplyChange((viewerUid, viewer), delta);
        }
    }
}
