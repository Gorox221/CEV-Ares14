using Robust.Shared.GameStates;

namespace Content.Shared.Roles.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodWormRoleComponent : BaseMindRoleComponent
{
    [DataField] public float LifetimeConsumedBlood;
}
