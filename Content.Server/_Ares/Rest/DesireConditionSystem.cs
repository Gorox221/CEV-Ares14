// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Desires;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Objectives.Components;
using Robust.Shared.Random;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Objective-side handler of the desire condition: picks a recipe group for
/// Eat desires, sets names and reports progress to the objective pipeline.
/// </summary>
public sealed class DesireConditionSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DesireConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<DesireConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    /// <summary>
    /// Marks the objective's desire condition as completed and notifies the rest system.
    /// </summary>
    public void SetCompleted(EntityUid objEnt, DesireConditionComponent comp)
    {
        if (comp.Completed)
            return;

        comp.Completed = true;
        Dirty(objEnt, comp);

        var ev = new DesireCompletedEvent(objEnt);
        RaiseLocalEvent(ref ev);
    }

    private void OnGetProgress(EntityUid uid, DesireConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = comp.Completed ? 1f : 0f;
    }

    private void OnAfterAssign(EntityUid uid, DesireConditionComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        if (comp.RecipeGroup == null && comp.Behavior is EatDesireBehavior eat)
        {
            comp.RecipeGroup = eat.Groups[_random.Next(eat.Groups.Count)].Id;
            Dirty(uid, comp);
        }

        if (comp.DesireType == null)
            return;

        if (comp.RecipeGroup != null)
        {
            var groupName = Loc.GetString("rest-desire-group-" + comp.RecipeGroup);
            _metaData.SetEntityName(uid, Loc.GetString("rest-desire-" + comp.DesireType, ("group", groupName)), args.Meta);
            _metaData.SetEntityDescription(uid, Loc.GetString("rest-description-" + comp.DesireType, ("group", groupName)), args.Meta);
        }
        else
        {
            _metaData.SetEntityName(uid, Loc.GetString("rest-desire-" + comp.DesireType), args.Meta);
            _metaData.SetEntityDescription(uid, Loc.GetString("rest-description-" + comp.DesireType), args.Meta);
        }
    }
}
