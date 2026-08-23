// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Footprints;
using Content.Shared._Ares.Sanity.Behaviors;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Fluids.Components;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Lowers sanity every check tick while standing in a blood or vomit puddle.
/// </summary>
public sealed partial class PuddleSanityChangeSystem : SanityChangeSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SanityCheckEvent>(OnSanityCheck);
    }

    private void OnSanityCheck(ref SanityCheckEvent args)
    {
        if (!TryComp<SanityComponent>(args.Entity, out var sanity))
            return;

        var behavior = sanity.Changes.OfType<PuddleSanityChangeBehavior>().FirstOrDefault();
        if (behavior == null)
            return;

        var xform = Transform(args.Entity);

        var puddles = _lookup.GetEntitiesInRange<PuddleComponent>(xform.Coordinates, behavior.Range);
        var totalChange = 0f;

        foreach (var (puddleUid, puddleComp) in puddles)
        {
            if (HasComp<FootprintComponent>(puddleUid))
                continue;

            var puddleXform = Transform(puddleUid);
            var originMap = _transform.ToMapCoordinates(xform.Coordinates);
            var otherMap = _transform.ToMapCoordinates(puddleXform.Coordinates);

            if (!_interaction.InRangeUnobstructed(originMap, otherMap, behavior.Range))
                continue;

            Entity<SolutionComponent>? solutionEntity = null;
            if (!_solutionContainer.ResolveSolution(puddleUid, puddleComp.SolutionName, ref solutionEntity, out var solution))
                continue;

            var totalVol = solution.Volume.Float();
            if (totalVol <= 0f)
                continue;

            var bloodFraction = solution.GetTotalPrototypeQuantity(new ProtoId<ReagentPrototype>("Blood")).Float() / totalVol;
            var vomitFraction = solution.GetTotalPrototypeQuantity(new ProtoId<ReagentPrototype>("Vomit")).Float() / totalVol;

            if (bloodFraction >= behavior.MinFraction || vomitFraction >= behavior.MinFraction)
                totalChange += behavior.Change;
        }

        ApplyChange((args.Entity, sanity), totalChange);
    }
}
