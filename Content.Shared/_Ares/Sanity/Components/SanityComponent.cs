// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Behaviors;
using Content.Shared._Ares.Sanity.Prototypes;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SanityComponent : Component
{
    /// <summary>
    /// Sources that lower or raise sanity, each handled by its own system.
    /// </summary>
    [DataField]
    public List<SanityChangeBehavior> Changes = new();

    [DataField, AutoNetworkedField]
    public float CurrentSanity = 100f;

    /// <summary>
    /// ID of the breakdown currently in progress, networked so clients
    /// (e.g. the admin overlay) can show it.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<SanityBreakdownPrototype>? CurrentBreakdown;

    [DataField]
    public float MaxSanity = 100f;

    [DataField]
    public float MinSanity = 0f;

    /// <summary>
    /// Perception radius used by the aura sanity change system.
    /// </summary>
    [DataField]
    public float Range = 8f;

    [DataField]
    public float CheckInterval = 2f;

    [DataField]
    public float Accumulator;

    [DataField]
    public TimeSpan LastDamageTime;

    [DataField]
    public TimeSpan NextMessageTime;

    [DataField]
    public TimeSpan NextBreakdownTime;

    /// <summary>
    /// Server-side cooldown for hallucinatory chat messages.
    /// </summary>
    [DataField]
    public TimeSpan NextHallucinationTime;

    [DataField]
    public ProtoId<AlertPrototype> SanityAlertType = "Sanity";

    [DataField]
    public ProtoId<AlertCategoryPrototype> SanityAlertCategory = "Sanity";
}
