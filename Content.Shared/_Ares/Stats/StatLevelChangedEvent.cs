// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

[ByRefEvent]
public record struct StatLevelChangedEvent(EntityUid Entity, ProtoId<StatPrototype> StatId, int OldValue, int NewValue);