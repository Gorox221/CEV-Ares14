// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

public sealed partial class DeathWitnessPerkEffect : PerkEffect<DeathWitnessPerkEffect>
{
    /// <summary>
    /// Cognition gained or lost in the cognition outcomes.
    /// </summary>
    [DataField]
    public int CognitionDelta = 5;
}