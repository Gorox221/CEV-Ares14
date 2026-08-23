// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Desires;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Handles fulfillment of the Smoke desire: the reagent in the bloodstream.
/// </summary>
public sealed partial class SmokeDesireSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainers = default!;
    [Dependency] private readonly RestSystem _rest = default!;
    [Dependency] private readonly DesireConditionSystem _desireConditions = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<RestComponent>();
        while (query.MoveNext(out var uid, out var rest))
        {
            if (rest.LevelUpPending)
                continue;

            if (!_rest.TryGetDesireObjective(uid, out var objEnt, out var cond))
                continue;

            if (cond.Behavior is not SmokeDesireBehavior smoke)
                continue;

            if (HasReagent(uid, smoke))
                _desireConditions.SetCompleted(objEnt, cond);
        }
    }

    private bool HasReagent(EntityUid uid, SmokeDesireBehavior smoke)
    {
        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return false;

        if (!_solutionContainers.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var solution))
            return false;

        return smoke.ContainsReagent(solution);
    }
}
