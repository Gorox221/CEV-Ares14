// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

public sealed class AresGunAccuracySystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StatsComponent, GunRefreshModifiersEvent>(OnGunRefreshModifiers);
    }

    private void OnGunRefreshModifiers(Entity<StatsComponent> ent, ref GunRefreshModifiersEvent args)
    {
        var vigPrototype = new ProtoId<StatPrototype>("Vigilance");

        if (!_prototypes.HasIndex(vigPrototype))
            return;

        var vigLevel = _stats.GetStatLevel(ent, vigPrototype, ent.Comp);

        var clampedVig = Math.Clamp(vigLevel, 1, 60);

        var divisor = clampedVig / 8.0;

        args.MinAngle = Angle.FromDegrees(args.MinAngle.Degrees / divisor);
        args.MaxAngle = Angle.FromDegrees(args.MaxAngle.Degrees / divisor);
    }
}
