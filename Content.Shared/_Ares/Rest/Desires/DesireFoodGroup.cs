// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Desires;

/// <summary>
/// A pickable food group of the Eat desire: the group shown to the player and
/// the recipe groups whose dishes fulfill it.
/// </summary>
[DataDefinition]
public sealed partial class DesireFoodGroup
{
    [DataField(required: true)]
    public string Id = string.Empty;

    [DataField(required: true)]
    public List<string> Recipes = new();
}
