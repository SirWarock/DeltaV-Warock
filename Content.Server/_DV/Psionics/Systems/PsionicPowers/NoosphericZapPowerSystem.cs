using Content.Server.Electrocution;
using Content.Server.Lightning;
using Content.Server.Power.EntitySystems;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.PowerCell;

namespace Content.Server._DV.Psionics.Systems.PsionicPowers;

public sealed class NoosphericZapPowerSystem : SharedNoosphericZapPowerSystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;

    protected override void OnPowerUsed(Entity<NoosphericZapPowerComponent> psionic, ref NoosphericZapPowerActionEvent args)
    {
        // As this can target batteries, it doesn't require the target to be psionic.
        if (!Psionic.CanBeTargeted(args.Target, hasAggressor: args.Performer, ignorePsionicRequirement: true))
            return;
        // This checks for IPCS and Borgs too.
        if (_powerCell.TryGetBatteryFromEntityOrSlot(args.Target, out var battery))
        {
            ChargeBattery(battery.Value.AsNullable(), psionic.Comp.AddedBatteryCharge, args.Target);
            ShootLightning(args.Performer, args.Target, psionic.Comp);
            AfterPowerUsed(psionic, args.Performer);
            return;
        }
        // If there is no battery, the target needs to be potentially psionic.
        if (!Psionic.CanBeTargeted(args.Target, showPopup: false))
            return;

        ShootLightning(args.Performer, args.Target, psionic.Comp);
        AfterPowerUsed(psionic, args.Performer);
    }

    private void ChargeBattery(Entity<BatteryComponent?> battery, float amount, EntityUid container)
    {
        var message = Loc.GetString("psionic-power-noospheric-zap-battery", ("battery", Identity.Entity(container, EntityManager)));
        Popup.PopupEntity(message, battery, PopupType.Medium);
        _battery.ChangeCharge(battery, amount);
    }

    private void ShootLightning(EntityUid shooter, EntityUid target, NoosphericZapPowerComponent comp)
    {
        var message = Loc.GetString("psionic-power-noospheric-zap-user", ("user", Identity.Entity(shooter, EntityManager)));
        Popup.PopupEntity(message, shooter, PopupType.LargeCaution);
        _lightning.ShootLightning(shooter, target, comp.LightningPrototpyeId);
        _electrocution.TryDoElectrocution(target, shooter, comp.ShockDamage, comp.StunDuration, true);
    }
}
