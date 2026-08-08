// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Data-driven effect of a perk. Stored on a <see cref="PerkPrototype"/>
/// and dispatched as a typed event to the effect's own system, mirroring
/// SanityBreakdownBehavior and EntityEffect.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class PerkEffect
{
    /// <summary>
    /// Called when the perk is applied to an entity, dispatching the
    /// effect to its own system.
    /// </summary>
    public abstract void Apply(EntityUid target, IPerkEffectApplier applier);
}

/// <summary>
/// Self-typing base used to raise the effect without losing its concrete type.
/// </summary>
public abstract partial class PerkEffect<T> : PerkEffect where T : PerkEffect<T>
{
    public override void Apply(EntityUid target, IPerkEffectApplier applier)
    {
        if (this is not T type)
            return;

        applier.ApplyEffect(target, type);
    }
}

/// <summary>
/// Dispatches a perk effect to the entity as a typed event.
/// </summary>
public interface IPerkEffectApplier
{
    void ApplyEffect<T>(EntityUid target, T effect) where T : PerkEffect<T>;
}