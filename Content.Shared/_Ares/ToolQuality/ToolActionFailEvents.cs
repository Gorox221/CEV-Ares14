// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Shared._Ares.ToolQuality;

[ByRefEvent]
public struct ToolActionFailCheckEvent
{
    public EntityUid User;
    public EntityUid Tool;
    public EntityUid Target;
    public bool Cancelled;
    public bool CritFailed;

    public ToolActionFailCheckEvent(EntityUid user, EntityUid tool, EntityUid target)
    {
        User = user;
        Tool = tool;
        Target = target;
        Cancelled = false;
        CritFailed = false;
    }
}

public sealed class ToolActionCritFailEvent : EntityEventArgs
{
    public EntityUid User;
    public EntityUid Tool;
    public EntityUid Target;

    public ToolActionCritFailEvent(EntityUid user, EntityUid tool, EntityUid target)
    {
        User = user;
        Tool = tool;
        Target = target;
    }
}
