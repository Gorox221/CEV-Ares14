// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

public sealed class AresVaultingSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    private readonly Dictionary<EntityUid, float> _baseClimbDelays = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<ClimbableComponent, AttemptClimbEvent>(OnAttemptClimb);
        SubscribeLocalEvent<ClimbableComponent, ComponentShutdown>(OnClimbableShutdown);
    }

    private void OnClimbableShutdown(Entity<ClimbableComponent> ent, ref ComponentShutdown args)
    {
        _baseClimbDelays.Remove(ent);
    }

    private void OnAttemptClimb(Entity<ClimbableComponent> ent, ref AttemptClimbEvent args)
    {
        var vigPrototype = new ProtoId<StatPrototype>("Vigilance");

        if (!_prototypes.HasIndex(vigPrototype))
            return;

        var user = args.User;
        if (user == default)
            return;

        if (!HasComp<StatsComponent>(user))
            return;

        if (!_baseClimbDelays.ContainsKey(ent))
            _baseClimbDelays[ent] = ent.Comp.ClimbDelay;

        var baseDelay = _baseClimbDelays[ent];

        var vigLevel = _stats.GetStatLevel(user, vigPrototype);

        var clampedVig = Math.Clamp(vigLevel, 0, 60);
        var mult = 1.0 - clampedVig / 60.0;

        ent.Comp.ClimbDelay = Math.Max((float)(baseDelay * mult), baseDelay * 0.66f);
    }
}
