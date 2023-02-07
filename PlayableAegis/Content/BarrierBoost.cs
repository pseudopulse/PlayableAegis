using System;

namespace PlayableAegis.States {
    public class BarrierBoost : BaseState {
        private float percentageDrained = 0;
        private float minDistance = 35f;
        private float maxDistance = 70f;
        private float drainPerSecond = 35f;
        private float minDuration = 0.5f;

        public override void OnEnter()
        {
            base.OnEnter();
            AkSoundEngine.PostEvent(Events.Play_voidman_m2_charged_loop, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (healthComponent.barrier < healthComponent.fullHealth * 0.02f || base.fixedAge >= minDuration && !inputBank.skill3.down) {
                outer.SetNextStateToMain();
            }

            float barrierDrain = healthComponent.barrier * (drainPerSecond * 0.01f * Time.fixedDeltaTime);
            drainPerSecond += 20 * Time.fixedDeltaTime;
            percentageDrained += drainPerSecond * Time.fixedDeltaTime;
            healthComponent.barrier -= barrierDrain;
        }

        public override void OnExit()
        {
            base.OnExit();
            AkSoundEngine.PostEvent(Events.Stop_voidman_m2_charged_loop, base.gameObject);
            if (base.characterMotor) {
                float force = Util.Remap(percentageDrained, 0, 100, minDistance, maxDistance);
                force *= base.characterMotor.mass;
                if (inputBank.moveVector == Vector3.zero) {
                    Vector3 vector = base.GetAimRay().direction;
                    vector.y = 0;
                    base.characterMotor.ApplyForce(vector * force, true, false);
                }
                else {
                    base.characterMotor.ApplyForce(base.inputBank.moveVector * force, true, false);
                }
            }
            AkSoundEngine.PostEvent(Events.Play_huntress_R_snipe_shoot, base.gameObject);
        }
    }
}