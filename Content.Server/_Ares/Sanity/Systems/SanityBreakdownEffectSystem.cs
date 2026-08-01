// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Base system for a single sanity breakdown type. Subscribes to the typed trigger event
/// raised by SanityBreakdownSystem, mirroring EntityEffectSystem.
/// </summary>
public abstract partial class SanityBreakdownEffectSystem<TBehavior> : EntitySystem
    where TBehavior : SanityBreakdownBehavior<TBehavior>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SanityComponent, SanityBreakdownTriggeredEvent<TBehavior>>(OnBreakdownTriggered);
    }

    protected abstract void OnBreakdownTriggered(Entity<SanityComponent> ent, ref SanityBreakdownTriggeredEvent<TBehavior> args);
}
