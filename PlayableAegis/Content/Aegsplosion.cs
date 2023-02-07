using System;

namespace PlayableAegis.States {
    public class Aegsplosion : BaseState {
        private GameObject areaIndicatorInstance;
        private GameObject prefab => PlayableAegis.AegisRadiusIndicator;
        private GameObject explosionPrefab => PlayableAegis.AegisExplosion;
        private float minDamage = 4f;
        private float maxDamage = 36f;
        private float minRadius = 25f;
        private float maxRadius = 25f;
        private float percentageDrained = 0;
        private float maxPercentage = 100f;
        private float drainRate = 25f;
        private float radius => Util.Remap(percentageDrained, 0, maxPercentage, minRadius, maxRadius);
        private float damage => Util.Remap(percentageDrained, 0f, maxPercentage, minDamage, maxDamage);

        public override void OnEnter()
        {
            base.OnEnter();
            areaIndicatorInstance = GameObject.Instantiate(prefab, base.gameObject.transform);
            areaIndicatorInstance.transform.localScale = new(radius * 2, radius * 2, radius * 2);
            AkSoundEngine.PostEvent(Events.Play_loader_shift_charge_loop, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.FixedUpdate();
            if (healthComponent.barrier < healthComponent.fullHealth * 0.05f || base.fixedAge >= 1f && !inputBank.skill4.down) {
                outer.SetNextStateToMain();
            }

            float barrierDrain = healthComponent.barrier * (drainRate * 0.01f * Time.fixedDeltaTime);
            percentageDrained += drainRate * Time.fixedDeltaTime;
            healthComponent.barrier -= barrierDrain;
            drainRate += 20 * Time.fixedDeltaTime;

            if (areaIndicatorInstance) {
                areaIndicatorInstance.transform.localScale = new(radius * 2, radius * 2, radius * 2);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (areaIndicatorInstance) {
                Destroy(areaIndicatorInstance);
            }

            BlastAttack attack = new();
            attack.radius = radius;
            attack.baseDamage = base.damageStat * damage;
            attack.attacker = base.gameObject;
            attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            attack.procCoefficient = 1;
            attack.position = base.characterBody.corePosition;
            attack.Fire();
            EffectManager.SpawnEffect(explosionPrefab, new EffectData {
                scale = radius,
                origin = base.characterBody.corePosition,
            }, true);
            AkSoundEngine.PostEvent(Events.Stop_loader_shift_charge_loop, base.gameObject);
            AkSoundEngine.PostEvent(Events.Play_vagrant_R_explode, base.gameObject);

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}