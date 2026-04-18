using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Components.PsionicPowers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HystericalStrengthPowerComponent : BasePsionicPowerComponent
{
    public override EntProtoId ActionProtoId { get; set; } = "ActionHystericalStrength";

    public override string PowerName { get; set; } = "psionic-power-name-hysterical-strength";

    public override int MinGlimmerChanged { get; set; } = 0;

    public override int MaxGlimmerChanged { get; set; } = 0;

    /// <summary>
    /// The density added to the user when the power is active.
    /// </summary>
    [DataField]
    public float AddedDensity = 500;

    /// <summary>
    /// The strength of the resistance to the clothing slowdown.
    /// </summary>
    /// <example>
    /// 0.75 equals to a 75% resistance to clothing slowdown modifiers.
    /// </example>
    [DataField]
    public float ClothingSlowdownModifier = 0.75f;

    /// <summary>
    /// How much glimmer it generates each damage tick.
    /// </summary>
    [DataField]
    public int PassiveGlimmerGeneration = 1;

    /// <summary>
    /// The damage dealt to the psionic when the power is active.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new ()
        {
            { "Asphyxiation", 2.5 },
            { "Bloodloss", 1.5 },
        }
    };
}
