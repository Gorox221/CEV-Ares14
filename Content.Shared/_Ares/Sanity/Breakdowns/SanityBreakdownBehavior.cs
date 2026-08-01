// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Breakdowns;

/// <summary>
/// A data-driven behavior of a sanity breakdown. Stored on a <see cref="SanityBreakdownPrototype"/>
/// and dispatched as a typed event to the behavior's own system, mirroring EntityEffect.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class SanityBreakdownBehavior
{
    /// <summary>
    /// Sanity the target is restored to when the breakdown ends.
    /// </summary>
    [DataField]
    public float SanityReturn = 25f;

    public abstract void Trigger(Entity<SanityComponent> ent, ISanityBreakdownTrigger trigger);
}

/// <summary>
/// Self-typing base used to raise the breakdown without losing its concrete type.
/// </summary>
public abstract partial class SanityBreakdownBehavior<T> : SanityBreakdownBehavior where T : SanityBreakdownBehavior<T>
{
    public override void Trigger(Entity<SanityComponent> ent, ISanityBreakdownTrigger trigger)
    {
        if (this is not T type)
            return;

        trigger.RaiseBreakdown(ent, type);
    }
}

/// <summary>
/// Dispatches a breakdown behavior to the entity as a typed event.
/// </summary>
public interface ISanityBreakdownTrigger
{
    void RaiseBreakdown<T>(Entity<SanityComponent> ent, T behavior) where T : SanityBreakdownBehavior<T>;
}
