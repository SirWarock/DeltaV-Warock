using Robust.Shared.GameStates;

namespace Content.Shared._DV.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ClothingSlowResistanceComponent : Component
{
    /// <summary>
    /// Modifier for both walk and sprint slowdown.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Modifier = 0.25f;
}
