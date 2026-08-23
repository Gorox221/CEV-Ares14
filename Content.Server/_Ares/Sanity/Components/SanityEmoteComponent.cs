// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Sanity.Components;

[RegisterComponent]
public sealed partial class SanityEmoteComponent : Component
{
    /// <summary>
    /// IDs of <see cref="EmotePrototype"/> emotes the entity can randomly send.
    /// </summary>
    [DataField]
    public List<ProtoId<EmotePrototype>> Emotes = new();

    /// <summary>
    /// Seconds between emote roll attempts.
    /// </summary>
    [DataField]
    public float Interval = 2f;

    /// <summary>
    /// Chance (0..1) to send an emote on each interval tick.
    /// </summary>
    [DataField]
    public float Chance = 0.5f;

    [DataField]
    public TimeSpan NextEmoteTime;
}
