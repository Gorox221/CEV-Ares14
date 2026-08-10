// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Stats;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class DeathWitnessPerkEffectSystem : PerkQueryEffectSystem<DeathWitnessPerkEffect>
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PerksComponent, DeathWitnessedEvent>(OnDeathWitnessed);
    }

    // Nihilist and Survivor perk in one system
    private void OnDeathWitnessed(Entity<PerksComponent> ent, ref DeathWitnessedEvent args)
    {
        if (args.Delta < 0f)
            args.Delta *= GetSurvivorMultiplier(ent);

        var effect = GetEffect(ent);
        if (effect == null)
            return;

        var roll = _random.Next(4);
        switch (roll)
        {
            case 0:
                // Zero sanity loss.
                args.Delta = 0f;
                _popup.PopupEntity(Loc.GetString("perk-nihilist-witness-zero"), ent, ent);
                break;
            case 1:
                args.Delta = 0f;
                _stats.ModifyStatLevel(ent, new ProtoId<StatPrototype>("Cognition"), effect.CognitionDelta);
                _popup.PopupEntity(Loc.GetString("perk-nihilist-witness-cognition-up"), ent, ent);
                break;
            case 2:
                args.Delta = 0f;
                _stats.ModifyStatLevel(ent, new ProtoId<StatPrototype>("Cognition"), -effect.CognitionDelta);
                _popup.PopupEntity(Loc.GetString("perk-nihilist-witness-cognition-down"), ent, ent);
                break;
            default:
                // Rebuild loss into recovery.
                args.Delta = -args.Delta;
                _popup.PopupEntity(Loc.GetString("perk-nihilist-witness-recover"), ent, ent);
                break;
        }
    }

    private float GetSurvivorMultiplier(Entity<PerksComponent> ent)
    {
        var multiplier = 1f;

        foreach (var perkId in ent.Comp.Perks)
        {
            if (!_prototypes.TryIndex(perkId, out PerkPrototype? perk))
                continue;

            foreach (var effect in perk.Effects)
            {
                if (effect is SurvivorPerkEffect typed)
                    multiplier *= typed.Multiplier;
            }
        }

        return multiplier;
    }
}