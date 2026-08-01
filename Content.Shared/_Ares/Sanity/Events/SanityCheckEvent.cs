// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Events;

/// <summary>
/// Raised every sanity check tick for an entity. The tick-based sanity change
/// systems (aura, regen, puddles) subscribe to this broadcast event.
/// </summary>
[ByRefEvent]
public record struct SanityCheckEvent(EntityUid Entity);
