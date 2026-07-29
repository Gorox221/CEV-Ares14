// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Prototypes;

[Prototype]
public sealed partial class SanityBreakdownPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Popup { get; private set; } = string.Empty;

    [DataField]
    public float Weight { get; private set; } = 100f;

    [DataField]
    public float SanityReturn { get; private set; } = 25f;

    [DataField]
    public float Duration { get; private set; }

    [DataField]
    public DamageSpecifier? Healing { get; private set; }

    [DataField]
    public float SelfHarmInterval { get; private set; }

    [DataField]
    public DamageSpecifier? SelfHarmDamage { get; private set; }
}
