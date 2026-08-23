// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;

namespace Content.Server._Ares.Perks.Effects;

/// <summary>
/// Base system for a single perk effect type. Subscribes to the typed apply
/// event raised by PerkSystem, mirroring SanityBreakdownEffectSystem.
/// </summary>
public abstract partial class PerkEffectSystem<TEffect> : EntitySystem
    where TEffect : PerkEffect<TEffect>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PerksComponent, PerkEffectAppliedEvent<TEffect>>(OnEffectApplied);
    }

    protected abstract void OnEffectApplied(Entity<PerksComponent> ent, ref PerkEffectAppliedEvent<TEffect> args);
}