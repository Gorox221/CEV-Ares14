// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;

namespace Content.Shared._Ares.Sanity.Events;

/// <summary>
/// Raised by <c>RestSystem</c> when a player ingests something. The event bus of this
/// fork allows only one directed subscription per component-event pair, so the rest
/// system owns the ingest subscription and relays it to the desire systems through
/// this broadcast event.
/// </summary>
[ByRefEvent]
public record struct RestDesireIngestedEvent(EntityUid Player, EntityUid Food, Solution Split, bool ForceFed);
