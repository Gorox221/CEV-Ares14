// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Desires;

/// <summary>
/// Data-driven behavior of a rest desire. Stored on a <c>DesireConditionComponent</c>
/// of the desire's objective; the desire's own system checks the behavior type
/// to handle fulfillment.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class DesireBehavior;
