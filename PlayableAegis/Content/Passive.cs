using System;

namespace PlayableAegis {
    internal class Passive {
        internal static void Hooks() {
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) => {
                orig(self);
                if (self.bodyIndex == BodyCatalog.FindBodyIndex("AegisBody")) {
                    self.barrierDecayRate = self.maxHealth / 35;
                    self.maxBarrier = float.MaxValue;
                }
            };

            On.RoR2.HealthComponent.Heal += (orig, self, amount, proc, silent) => {
                if (self.body.bodyIndex == BodyCatalog.FindBodyIndex("AegisBody")) {
                    self.AddBarrier(amount);
                    return 0f;
                }
                else {
                    return orig(self, amount, proc, silent);
                }
            };

            On.RoR2.GlobalEventManager.ServerDamageDealt += (orig, report) => {
                orig(report);
                if (report.attackerBody && report.damageInfo.HasModdedDamageType(PlayableAegis.HealOnHit)) {
                    float amount = report.damageDealt * 0.3f;
                    report.attackerBody.healthComponent.Heal(amount, new(), false);
                }
            };
        }
    }
}