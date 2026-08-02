// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement.Components;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Adds a line to the examine description based on the target's sanity level.
/// </summary>
public sealed partial class SanityExamineSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SanityComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<SanityComponent> ent, ref ExaminedEvent args)
    {
        // Don't reveal sanity through a hidden identity (e.g. a mask).
        var identityEv = new SeeIdentityAttemptEvent();
        RaiseLocalEvent(args.Examined, identityEv);
        if (identityEv.Cancelled)
            return;

        if (ent.Comp.CurrentSanity > 33)
            return;

        var locId = ent.Comp.CurrentSanity <= 5
            ? "sanity-examine-insane"
            : "sanity-examine-strange";

        args.PushMarkup(Loc.GetString(locId, ("ent", args.Examined)), 15);
    }
}
