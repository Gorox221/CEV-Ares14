// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Events;

[ByRefEvent]
public record struct DeathWitnessedEvent(EntityUid DeadEntity)
{
    public float Delta = 0f;
}