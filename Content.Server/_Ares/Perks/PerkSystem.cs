// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Origins;
using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Perks.Events;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Perks;

public sealed partial class PerkSystem : EntitySystem, IPerkEffectApplier
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly OriginSystem _origins = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        _origins.ApplyOrigin(ev.Mob, ev.Profile.Origin);
        ApplyPerk(ev.Mob, ev.Profile.Perk);
    }

    /// <summary>
    /// Gives the entity a perk, storing it on <see cref="PerksComponent"/>
    /// and dispatching every perk effect. The entity can hold several perks
    /// acquired during the round.
    /// </summary>
    public void ApplyPerk(EntityUid target, ProtoId<PerkPrototype> perkId)
    {
        if (string.IsNullOrEmpty(perkId))
            return;

        if (!_prototypes.TryIndex(perkId, out PerkPrototype? perk))
        {
            Log.Error($"No perk found with ID {perkId}!");
            return;
        }

var comp = EnsureComp<PerksComponent>(target);
        if (comp.Perks.Contains(perkId))
            return;

        comp.Perks.Add(perkId);
        Dirty(target, comp);

        foreach (var effect in perk.Effects)
        {
            effect.Apply(target, this);
        }
    }

    public void ApplyEffect<T>(EntityUid target, T effect) where T : PerkEffect<T>
    {
        var ev = new PerkEffectAppliedEvent<T>(effect);
        RaiseLocalEvent(target, ref ev, true);
    }
}