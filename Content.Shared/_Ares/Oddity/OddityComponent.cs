// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Stats;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Oddity;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
public sealed partial class OddityComponent : Component
{
    /// <summary>
    /// Shader applied to the entity's sprite while this component is present.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string Shader = "OddityGold";

    /// <summary>
    /// Ranges of stat points granted when the oddity is used.
    /// </summary>
    [DataField]
    public List<OddityStatRange> StatRanges = new();

    /// <summary>
    /// Concrete values rolled from <see cref="StatRanges"/> when the entity spawned.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<StatPrototype>, int> GrantedStats = new();

    /// <summary>
    /// Chance the oddity gets a random oddity perk (a perk with
    /// <see cref="PerkPrototype.IsOddity"/> set) rolled at spawn.
    /// </summary>
    [DataField]
    public float PerkChance;

    /// <summary>
    /// Perk rolled at spawn if the <see cref="PerkChance"/> check passed.
    /// Granted together with the stats when the oddity is used.
    /// </summary>
    [DataField]
    public ProtoId<PerkPrototype> RolledPerk = string.Empty;
}

[DataDefinition]
public sealed partial class OddityStatRange
{
    /// <summary>
    /// Stat to grant points to.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<StatPrototype> Stat = default!;

    /// <summary>
    /// Minimum granted points (inclusive).
    /// </summary>
    [DataField]
    public int Min = 1;

    /// <summary>
    /// Maximum granted points (inclusive).
    /// </summary>
    [DataField]
    public int Max = 1;
}