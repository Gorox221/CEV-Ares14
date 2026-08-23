// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;

namespace Content.Shared._Ares.Perks.Effects;

public sealed partial class GasEmissionPerkEffect : PerkEffect<GasEmissionPerkEffect>
{
    [DataField]
    public Gas Gas = Gas.Ammonia;

    [DataField]
    public float MolesPerSecond = 5f;
}
