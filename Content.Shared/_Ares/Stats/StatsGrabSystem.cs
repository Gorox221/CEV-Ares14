// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Grab;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

public sealed class AresGrabSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RaiseGrabModifierEventEvent>(OnRaiseGrabModifier);
    }

    private void OnRaiseGrabModifier(ref RaiseGrabModifierEventEvent args)
    {
        var robPrototype = new ProtoId<StatPrototype>("Robustness");

        if (!_prototypes.HasIndex(robPrototype))
            return;

        var grabberRob = _stats.GetStatLevel(args.User, robPrototype);
        var grabberFactor = Math.Max(1f - grabberRob / 200f, 0f);
        args.Multiplier *= grabberFactor;

        if (!TryComp<PullerComponent>(args.User, out var puller) || puller.Pulling == null)
            return;

        var victimRob = _stats.GetStatLevel(puller.Pulling.Value, robPrototype);
        var victimBonus = victimRob / 200f;
        args.Modifier += victimBonus;

        var diff = victimRob - grabberRob;
        var speedFactor = Math.Clamp(1f - diff / 200f, 0.1f, 2f);
        args.SpeedMultiplier *= speedFactor;
    }
}