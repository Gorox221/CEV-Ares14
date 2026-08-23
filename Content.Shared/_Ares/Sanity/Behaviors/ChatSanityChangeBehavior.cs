// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Behaviors;

/// <summary>
/// Sanity gained when the entity speaks in local or radio chat.
/// </summary>
public sealed partial class ChatSanityChangeBehavior : SanityChangeBehavior<ChatSanityChangeBehavior>
{
    /// <summary>
    /// Sanity gained per message.
    /// </summary>
    [DataField]
    public float Change = 1f;

    /// <summary>
    /// Minimum seconds between gains.
    /// </summary>
    [DataField]
    public float MinCooldown = 30f;

    /// <summary>
    /// Maximum seconds between gains (randomized).
    /// </summary>
    [DataField]
    public float MaxCooldown = 45f;
}
