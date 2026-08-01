// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Behaviors;

/// <summary>
/// Data-driven behavior of a sanity change source. Each behavior type is handled
/// by its own system, mirroring EntityEffect. Player-side behaviors are configured
/// in the <c>changes</c> list of <c>SanityComponent</c>; world entities use
/// <c>SanityAffectorComponent</c>.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class SanityChangeBehavior;

/// <summary>
/// Self-typing base so a behavior keeps its concrete type when stored in the changes list.
/// </summary>
public abstract partial class SanityChangeBehavior<T> : SanityChangeBehavior where T : SanityChangeBehavior<T>;
