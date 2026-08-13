// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Extends the target's clear whisper hearing range by tiles, letting it read full whisper messages through walls.
/// </summary>
public sealed partial class WhisperHearingPerkEffect : PerkEffect<WhisperHearingPerkEffect>
{
    [DataField]
    public float ClearRangeExtension = 2f;
}