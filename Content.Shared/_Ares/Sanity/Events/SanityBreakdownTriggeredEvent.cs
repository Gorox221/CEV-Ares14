// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Breakdowns;

namespace Content.Shared._Ares.Sanity.Events;

/// <summary>
/// Raised on the target when a sanity breakdown is triggered, carrying the typed behavior
/// so the behavior's own system can handle it.
/// </summary>
[ByRefEvent]
public readonly record struct SanityBreakdownTriggeredEvent<T>(T Behavior) where T : SanityBreakdownBehavior<T>;
