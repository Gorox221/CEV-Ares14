// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Behaviors;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Regenerates sanity over time when the entity has not taken sanity damage recently.
/// </summary>
public sealed partial class RegenSanityChangeSystem : SanityChangeSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SanityCheckEvent>(OnSanityCheck);
    }

    private void OnSanityCheck(ref SanityCheckEvent args)
    {
        if (!TryComp<SanityComponent>(args.Entity, out var sanity))
            return;

        var behavior = sanity.Changes.OfType<RegenSanityChangeBehavior>().FirstOrDefault();
        if (behavior == null)
            return;

        if (_timing.CurTime - sanity.LastDamageTime < TimeSpan.FromSeconds(behavior.Delay))
            return;

        ApplyChange((args.Entity, sanity), behavior.Amount);
    }
}
