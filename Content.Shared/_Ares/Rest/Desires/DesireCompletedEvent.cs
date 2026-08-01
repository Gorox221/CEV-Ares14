// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Events;

/// <summary>
/// Raised on the objective entity when its <c>DesireCondition</c> is marked as completed.
/// The rest system reacts to grant the level-up and remove the objective from the mind.
/// </summary>
[ByRefEvent]
public record struct DesireCompletedEvent(EntityUid Objective);
