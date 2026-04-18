using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Psionics.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RegenerativeSleepingStatusEffectComponent : Component
{
    /// <summary>
    /// The next tick for the solution to be applied.
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan NextTick;

    /// <summary>
    /// The reagent ID that will be inserted into the bloodstream.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ReagentId = "Prometheum";

    /// <summary>
    /// The amount of reagent to be inserted every second.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 Quantity = 0.5f;
}
