// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Perks.Effects;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Stats;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Ares.Perks.Effects;

public sealed partial class TerribleFatePerkEffectSystem : PerkQueryEffectSystem<TerribleFatePerkEffect>
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SanityComponent, DeathWitnessedEvent>(OnDeathWitnessed);
    }

    private void OnDeathWitnessed(Entity<SanityComponent> witness, ref DeathWitnessedEvent args)
    {
        if (!TryComp<PerksComponent>(args.DeadEntity, out var deadPerks))
            return;

        var effect = GetEffect((args.DeadEntity, deadPerks));
        if (effect == null)
            return;

        var vigPrototype = new ProtoId<StatPrototype>("Vigilance");
        if (!_prototypes.HasIndex(vigPrototype))
            return;

        var vigLevel = _stats.GetStatLevel(witness, vigPrototype);
        var chance = Math.Clamp(effect.Chance * (1.2f - Math.Clamp(vigLevel, 0, 60) / 60f), 0f, 1f);
        if (!_random.Prob(chance))
            return;

        var oldValue = witness.Comp.CurrentSanity;
        if (oldValue <= 0f)
            return;

        witness.Comp.CurrentSanity = 0f;
        Dirty(witness);

        var ev = new SanityChangedEvent(witness, oldValue, 0f, -oldValue);
        RaiseLocalEvent(witness, ref ev, true);
    }
}