using System;

namespace PlayableAegis.States {
    public class BarrierBeam : BaseState {
        private float duration = 0.6f;
        private float damage = 2f;
        private int shotCount = 3;
        private float maxSpread = 2f;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = duration / base.attackSpeedStat;

            for (int i = 0; i < shotCount; i++) {
                BulletAttack attack = new();
                attack.owner = base.gameObject;
                attack.weapon = base.gameObject;
                attack.damage = base.damageStat * damage;
                attack.minSpread = 0;
                attack.maxSpread = maxSpread;
                attack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
                attack.aimVector = base.GetAimRay().direction;
                attack.origin = base.GetAimRay().origin;
                attack.isCrit = base.RollCrit();
                attack.damageType = DamageType.Generic;
                attack.procCoefficient = 1;
                attack.AddModdedDamageType(PlayableAegis.HealOnHit);
                attack.tracerEffectPrefab = PlayableAegis.AegisTracer;
                attack.procChainMask = new();
                
                attack.Fire();
            }
            AkSoundEngine.PostEvent(Events.Play_voidman_m1_shoot, base.gameObject);
            base.characterDirection.forward = base.GetAimRay().direction;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
            base.characterDirection.forward = base.GetAimRay().direction;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}