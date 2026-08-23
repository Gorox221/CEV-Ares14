// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Grab;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared.Examine;
using Content.Shared.HealthExaminable;
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
        SubscribeLocalEvent<SanityComponent, HealthBeingExaminedEvent>(OnHealthBeingExamined);
    }

    private void OnExamined(Entity<SanityComponent> ent, ref ExaminedEvent args)
    {
        // Don't reveal sanity through a hidden identity (e.g. a mask).
        if (IsIdentityHidden(args.Examined))
            return;

        if (ent.Comp.CurrentSanity > 33)
            return;

        var locId = ent.Comp.CurrentSanity <= 5
            ? "sanity-examine-insane"
            : "sanity-examine-strange";

        args.PushMarkup(Loc.GetString(locId, ("ent", args.Examined)), 15);
    }

    private void OnHealthBeingExamined(Entity<SanityComponent> ent, ref HealthBeingExaminedEvent args)
    {
        // These signs are only noticeable on a grabbed (i.e. restrained) person.
        var grabbedEv = new CheckGrabbedEvent();
        RaiseLocalEvent(ent.Owner, ref grabbedEv);
        if (!grabbedEv.IsGrabbed)
            return;

        // Don't reveal sanity through a hidden identity (e.g. a mask).
        if (IsIdentityHidden(ent.Owner))
            return;

        var sanity = ent.Comp.CurrentSanity;
        string locId;
        if (sanity < 5)
            locId = "sanity-medical-examine-acute-shock";
        else if (sanity <= 30)
            locId = "sanity-medical-examine-shock";
        else if (sanity <= 50)
            locId = "sanity-medical-examine-nervous";
        else
            return;

        args.Message.PushNewline();
        args.Message.AddMarkupOrThrow(Loc.GetString(locId, ("target", ent.Owner)));
    }

    private bool IsIdentityHidden(EntityUid uid)
    {
        var identityEv = new SeeIdentityAttemptEvent();
        RaiseLocalEvent(uid, identityEv);
        return identityEv.Cancelled;
    }
}
