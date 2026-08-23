// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks;

/// <summary>
/// Grants the entity extended clear whisper hearing: full whisper messages
/// ignoring line of sight. Added while a perk granting it is active.
/// </summary>
[RegisterComponent]
public sealed partial class WhisperHearingComponent : Component
{
    [DataField]
    public float ClearRangeExtension = 2f;
}