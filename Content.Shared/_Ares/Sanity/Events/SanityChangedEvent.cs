// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Events;

[ByRefEvent]
public record struct SanityChangedEvent(EntityUid Entity, float OldValue, float NewValue, float Delta);
