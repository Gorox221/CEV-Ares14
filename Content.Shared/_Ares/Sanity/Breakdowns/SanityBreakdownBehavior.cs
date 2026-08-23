// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared.Chat.Prototypes;
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

    /// <summary>
    /// IDs of <see cref="EmotePrototype"/> emotes the target may randomly send
    /// while this breakdown is active. Empty means no emotes are sent.
    /// </summary>
    [DataField]
    public List<ProtoId<EmotePrototype>> Emotes = new();

    /// <summary>
    /// Seconds between emote roll attempts.
    /// </summary>
    [DataField]
    public float EmoteInterval = 2f;

    /// <summary>
    /// Chance (0..1) to send an emote on each interval tick.
    /// </summary>
    [DataField]
    public float EmoteChance = 0.5f;

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
