using System;

namespace PlayableAegis.States {
    public class Aegizap : BaseState {
        private float damage = 12f;
        private GameObject tracer => PlayableAegis.AegizapTracer;

        public override void OnEnter()
        {
            base.OnEnter();
            BulletAttack attack = new();
            attack.owner = base.gameObject;
            attack.origin = base.GetAimRay().origin;
            attack.maxSpread = 0;
            attack.minSpread = 0;
            attack.radius = 1.3f;
            attack.stopperMask = LayerIndex.world.collisionMask;
            attack.damage = base.damageStat * damage;
            attack.tracerEffectPrefab = tracer;
            attack.weapon = base.gameObject;
            attack.procCoefficient = 1f;
            attack.aimVector = base.GetAimRay().direction;
            attack.Fire();

            AkSoundEngine.PostEvent(Events.Play_commando_M2, base.gameObject);
            outer.SetNextStateToMain();
            base.characterDirection.forward = base.GetAimRay().direction;
        }
    }
}