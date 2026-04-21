using Content.Shared._DV.Clothing.Components;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Damage.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public sealed class HystericalStrengthPowerSystem : BasePsionicPowerSystem<HystericalStrengthPowerComponent, HystericalStrengthPowerActionEvent>
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly FixtureSystem _fixture = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public static readonly EntProtoId HystericalStrengthEffectProto = "HystericalStrengthStatusEffect";
    private readonly string _fixtureID = "HystericalStrengthFixture";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HystericalStrengthStatusEffectComponent, StatusEffectRelayedEvent<MobStateChangedEvent>>(OnMobStateChanged);
        SubscribeLocalEvent<HystericalStrengthStatusEffectComponent, StatusEffectRelayedEvent<DispelledEvent>>(OnActiveDispelled);
        SubscribeLocalEvent<HystericalStrengthStatusEffectComponent, StatusEffectRelayedEvent<PsionicSuppressedEvent>>(OnActiveSuppressed);
    }

    protected override void OnPowerUsed(Entity<HystericalStrengthPowerComponent> psionic, ref HystericalStrengthPowerActionEvent args)
    {
        args.Toggle = true; // This will toggle them AFTER the code has run.
        // If the action ISN'T toggled, it WILL be toggled after this code, so we have to treat it as if it IS toggled on.
        if (!args.Action.Comp.Toggled)
        {
            if (!_statusEffects.TryUpdateStatusEffectDuration(args.Performer, HystericalStrengthEffectProto))
                return;

            _fixture.TryCreateFixture(args.Performer, new PhysShapeCircle(), _fixtureID, 500, false, friction: 0f);

            var messageUser = Loc.GetString("psionic-power-hysterical-strength-used");
            var messageOthers = Loc.GetString("psionic-power-hysterical-strength-used-others", ("user", Identity.Entity(args.Performer, EntityManager)));

            Popup.PopupPredicted(messageUser, messageOthers, args.Performer, args.Performer, PopupType.Medium);
        }
        else
        {
            StopPower(args.Performer);
        }

        AfterPowerUsed(psionic, args.Performer);
    }

    private void StopPower(Entity<HystericalStrengthPowerComponent?> performer, bool toggleOff = false)
    {
        _statusEffects.TryRemoveStatusEffect(performer, HystericalStrengthEffectProto);
        _fixture.DestroyFixture(performer, _fixtureID);

        if (toggleOff && Resolve(performer, ref performer.Comp))
            Action.SetToggled(performer.Comp.ActionEntity, false);
    }

    private void PunishPsionic(EntityUid victim, EntityUid? dispeller = null)
    {
        StopPower(victim, true);

        var message = Loc.GetString("psionic-power-hysterical-strength-being-dispelled", ("dispelled", Identity.Entity(victim, EntityManager)));

        Popup.PopupPredicted(message, victim, dispeller ?? victim, PopupType.MediumCaution);
        // TODO: Fix StatusEffectsArrays firing an error when adding a statuseffect amidst relaying events to statuseffects.
        //_stun.TryAddParalyzeDuration(args.Args.Target, TimeSpan.FromSeconds(6));
    }

    private void OnActiveDispelled(Entity<HystericalStrengthStatusEffectComponent> effect, ref StatusEffectRelayedEvent<DispelledEvent> args)
    {
        PunishPsionic(args.Args.Target, args.Args.Target);
    }

    private void OnActiveSuppressed(Entity<HystericalStrengthStatusEffectComponent> ent, ref StatusEffectRelayedEvent<PsionicSuppressedEvent> args)
    {
        PunishPsionic(args.Args.Victim, args.Args.Victim);
    }

    private void OnMobStateChanged(Entity<HystericalStrengthStatusEffectComponent> effect, ref StatusEffectRelayedEvent<MobStateChangedEvent> args)
    {
        if (args.Args.NewMobState == MobState.Alive)
            return;

        StopPower(args.Args.Target, true);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<HystericalStrengthStatusEffectComponent, StatusEffectComponent>();

        while (query.MoveNext(out var uid, out var comp, out var statusEffect))
        {
            if (comp.NextTick > Timing.CurTime || statusEffect.AppliedTo is null)
                continue;
            comp.NextTick = Timing.CurTime + comp.DamageDelay;
            Dirty(uid, comp);

            Glimmer.Glimmer += comp.PassiveGlimmerGeneration;
            _damageable.TryChangeDamage(statusEffect.AppliedTo.Value, comp.Damage, ignoreResistances: true, interruptsDoAfters: false);
        }
    }
}
