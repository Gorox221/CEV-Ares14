// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Fluids.Components;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanityPuddleSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SanityComponent, GetSanityAffectorsEvent>(OnGetSanityAffectors);
    }

    private void OnGetSanityAffectors(Entity<SanityComponent> ent, ref GetSanityAffectorsEvent args)
    {
        var xform = Transform(ent);
        var range = ent.Comp.Range;

        var puddles = _lookup.GetEntitiesInRange<PuddleComponent>(xform.Coordinates, range);
        foreach (var (puddleUid, puddleComp) in puddles)
        {
            var puddleXform = Transform(puddleUid);
            var originMap = _transform.ToMapCoordinates(xform.Coordinates);
            var otherMap = _transform.ToMapCoordinates(puddleXform.Coordinates);

            if (!_interaction.InRangeUnobstructed(originMap, otherMap, range))
                continue;

            Entity<SolutionComponent>? solutionEntity = null;
            if (!_solutionContainer.ResolveSolution(puddleUid, puddleComp.SolutionName, ref solutionEntity, out var solution))
                continue;

            var totalVol = solution.Volume.Float();
            if (totalVol <= 0f)
                continue;

            var bloodFraction = solution.GetTotalPrototypeQuantity(new ProtoId<ReagentPrototype>("Blood")).Float() / totalVol;
            var vomitFraction = solution.GetTotalPrototypeQuantity(new ProtoId<ReagentPrototype>("Vomit")).Float() / totalVol;

            if (bloodFraction >= 0.4f || vomitFraction >= 0.4f)
                args.TotalChange -= 1f;
        }
    }
}
