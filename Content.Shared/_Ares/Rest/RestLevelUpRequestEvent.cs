// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Ares.Sanity.Events;

[Serializable, NetSerializable]
public sealed class RestLevelUpRequestEvent : EntityEventArgs
{
    public readonly NetEntity Player;
    public readonly string Choice;

    public RestLevelUpRequestEvent(NetEntity player, string choice)
    {
        Player = player;
        Choice = choice;
    }
}
