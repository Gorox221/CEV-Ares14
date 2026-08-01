// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared._Ares.CCVar;

/// <summary>
/// Ares specific cvars.
/// </summary>
[CVarDefs]
public sealed partial class AresCCVars
{
    /// <summary>
    /// If true, the admin overlay will show the sanity level and the current breakdown.
    /// </summary>
    public static readonly CVarDef<bool> AdminOverlayShowSanity =
        CVarDef.Create("ui.admin_overlay_show_sanity", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
