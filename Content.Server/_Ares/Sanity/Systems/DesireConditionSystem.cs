// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server._Ares.Sanity.Systems;

public sealed class DesireConditionSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DesireConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<DesireConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnGetProgress(EntityUid uid, DesireConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = comp.Completed ? 1f : 0f;
    }

    private void OnAfterAssign(EntityUid uid, DesireConditionComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        if (comp.DesireType != null)
        {
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
}
