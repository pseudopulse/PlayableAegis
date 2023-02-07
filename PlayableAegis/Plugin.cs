using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;

namespace PlayableAegis {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class PlayableAegis : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "ModAuthorName";
        public const string PluginName = "PlayableAegis";
        public const string PluginVersion = "1.0.0";

        public static AssetBundle bundle;
        public static BepInEx.Logging.ManualLogSource ModLogger;

        // assets
        public static SkinDef SkinAegis;
        public static SkinDef SkinAegisAlt;
        public static GameObject AegisBody;
        public static SkillDef AegisPrimary;
        public static SkillDef AegisSecondary;
        public static SkillDef AegisUtility;
        public static SkillDef AegisSpecial;
        public static SurvivorDef sdAegis;
        public static GameObject AegisTracer;
        public static DamageAPI.ModdedDamageType HealOnHit = DamageAPI.ReserveDamageType();
        public static GameObject AegisRadiusIndicator;
        public static GameObject AegisExplosion;
        public static GameObject AegizapTracer;
        public static UnlockableDef MasteryUnlockable;

        public void Awake() {
            // assetbundle loading 
            bundle = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("PlayableAegis.dll", "aegisbundle"));

            // set logger
            ModLogger = Logger;

            LoadAssets();
            SetupLanguage();
            ModifyAssets();

            ContentAddition.AddBody(AegisBody);
            ContentAddition.AddSurvivorDef(sdAegis);
            ContentAddition.AddEffect(AegisTracer);
            ContentAddition.AddEffect(AegisExplosion);
            ContentAddition.AddEffect(AegizapTracer);
            //ContentAddition.AddUnlockableDef(MasteryUnlockable);
            foreach (SkillDef def in bundle.LoadAllAssets<SkillDef>()) {
                ContentAddition.AddSkillDef(def);
            }

            foreach (SkillFamily family in bundle.LoadAllAssets<SkillFamily>()) {
                ContentAddition.AddSkillFamily(family);
            }

            Passive.Hooks();
        }

        internal void LoadAssets() {
            SkinAegis = bundle.LoadAsset<SkinDef>("Assets/PlayableAegis/skinAegis.asset");
            SkinAegisAlt = bundle.LoadAsset<SkinDef>("Assets/PlayableAegis/skinAegisAlt.asset");
            AegisBody = bundle.LoadAsset<GameObject>("Assets/PlayableAegis/AegisBody.prefab");
            AegisUtility = bundle.LoadAsset<SkillDef>("Assets/PlayableAegis/Skills/Skills/sdAegisUtility.asset");
            AegisSecondary = bundle.LoadAsset<SkillDef>("Assets/PlayableAegis/Skills/Skills/sdAegisSecondary.asset");
            AegisPrimary = bundle.LoadAsset<SkillDef>("Assets/PlayableAegis/Skills/Skills/sdAegisPrimary.asset");
            AegisSpecial = bundle.LoadAsset<SkillDef>("Assets/PlayableAegis/Skills/Skills/sdAegisSpecial.asset");
            sdAegis = bundle.LoadAsset<SurvivorDef>("Assets/PlayableAegis/sdAegis.asset");
            MasteryUnlockable = bundle.LoadAsset<UnlockableDef>("Assets/PlayableAegis/AegisUnlockSkinAlt.asset");

            AegisTracer = Utils.Paths.GameObject.VoidSurvivorBeamTracer.Load<GameObject>().InstantiateClone("AegisTracer");
            AegisTracer.GetComponent<LineRenderer>().material = Utils.Paths.Material.matGoldOrbTrail.Load<Material>();
            AegisTracer.GetComponentInChildren<Light>().color = Color.yellow;

            AegisExplosion = Utils.Paths.GameObject.VagrantNovaExplosion.Load<GameObject>().InstantiateClone("AegisExplosion");
            AegisExplosion.GetComponentInChildren<Light>().color = Color.yellow;
            foreach (ParticleSystem system in AegisExplosion.GetComponentsInChildren<ParticleSystem>()) {
                ParticleSystem.MainModule main = system.main;
                main.startColor = Color.yellow;
            }

            AegisRadiusIndicator = Utils.Paths.GameObject.VagrantNovaAreaIndicator.Load<GameObject>().InstantiateClone("AegisIndicator");
            AegisRadiusIndicator.GetComponentInChildren<Light>().color = Color.yellow;
            foreach (ParticleSystem system in AegisRadiusIndicator.GetComponentsInChildren<ParticleSystem>()) {
                ParticleSystem.MainModule main = system.main;
                main.startColor = Color.yellow;
            }

            AegizapTracer = Utils.Paths.GameObject.TracerVoidRaidCrabTripleBeamSmall.Load<GameObject>().InstantiateClone("AegizapTracer");
            foreach (LineRenderer renderer in AegizapTracer.GetComponentsInChildren<LineRenderer>()) {
                renderer.material = Utils.Paths.Material.matGoldBeaconActivate.Load<Material>();
            }
            foreach (ParticleSystem system in AegizapTracer.GetComponentsInChildren<ParticleSystem>()) {
                ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
                renderer.material = Utils.Paths.Material.matGoldOrbTrail.Load<Material>();
            }
            AegizapTracer.GetComponentInChildren<Light>().color = Color.yellow;
        }

        internal void SetupLanguage() {
            "AEGIS_PRIMARY_NAME".Add("Barrier Beam");
            "AEGIS_SECONDARY_NAME".Add("Aegizap");
            "AEGIS_UTILITY_NAME".Add("Barrier Boost");
            "AEGIS_SPECIAL_NAME".Add("Aegsplosion");
            "AEGIS_PASSIVE_NAME".Add("Bulwark");
            "AEGIS_SKIN_NAME".Add("Default");
            "AEGIS_BODY_NAME".Add("Aegis");
            "AEGIS_BODY_SUBTITLE".Add("Item Scrap, Red");
            "AEGIS_SKIN_ALT_NAME".Add("Redeemed");
            "ACHIEVEMENT_AEGISCLEARGAMEMONSOON_NAME".Add("Aegis: Mastery");
            "AEGIS_BODY_DESC".Add(
                """
                Order: Artifact E-8EE572
                Tracking Number: 490******
                Estimated Delivery: 08/10/2056
                Shipping Method: Priority
                Shipping Address: Titan Museum of History and Culture, Titan

                Sorry about the delay, we've had a flood of orders come in from this site. But it was exactly where you said we should look - there was a sealed off room where you marked the excavation diagram. I finished translating the engraving too, so consider that a bonus for the time we took to get to it:

                "I am the will to survive made manifest. To those who never lose hope, to they who try in the face of impossible odds, I offer not 
                protection, but the means to bring one's unconquerable spirit forth as the defender of their mortal lives."

                It’s so lightweight, we figure it must've been entirely decorative. That seems to line up with the text. In any case, I hope it makes a good exhibit! I'm a big fan of the museum, so it wouldn't hurt to give me a partner's discount next time I visit, right?
                """
            );
            "AEGIS_BODY_LORE".Add(
                """
                Order: Artifact E-8EE572
                Tracking Number: 490******
                Estimated Delivery: 08/10/2056
                Shipping Method: Priority
                Shipping Address: Titan Museum of History and Culture, Titan

                Sorry about the delay, we've had a flood of orders come in from this site. But it was exactly where you said we should look - there was a sealed off room where you marked the excavation diagram. I finished translating the engraving too, so consider that a bonus for the time we took to get to it:

                "I am the will to survive made manifest. To those who never lose hope, to they who try in the face of impossible odds, I offer not 
                protection, but the means to bring one's unconquerable spirit forth as the defender of their mortal lives."

                It’s so lightweight, we figure it must've been entirely decorative. That seems to line up with the text. In any case, I hope it makes a good exhibit! I'm a big fan of the museum, so it wouldn't hurt to give me a partner's discount next time I visit, right?
                """
            );
            "AEGIS_BODY_OUTRO".Add("And so it left, remaining the peak of a healing build.");
            "AEGIS_BODY_FAILURE".Add("And so it vanished, never to be printed into clover again.");
            "KEYWORD_DRAINING".Add("<style=cKeywordName>Draining</style>Using this skill <style=cDeath>consumes barrier</style>.");
            "AEGIS_PRIMARY_DESC".Add("Fire a spread of <style=cIsDamage>barrier beams</style> for <style=cIsDamage>3x200% damage</style>. <style=cIsUtility>30%</style> of damage dealt is <style=cIsHealing>returned as barrier</style>.");
            "AEGIS_SECONDARY_DESC".Add("Fire a concentrated beam of <style=cIsHealth>barrier</style> for <style=cIsDamage>1200% damage</style>.");
            "AEGIS_UTILITY_DESC".Add("<style=cDeath>Draining.</style> <style=cIsUtility>Agile.</style> Channel barrier to <style=cIsUtility>launch forward</style>.");
            "AEGIS_SPECIAL_DESC".Add("<style=cDeath>Draining.</style> Expel your <style=cIsHealth>temporary barrier</style> to release a devastating blast for <style=cIsDamage>400%-3600% damage</style>.");
            "AEGIS_PASSIVE_DESC".Add("The <style=cIsUtility>Aegis</style> has no <style=cIsHealth>maximum barrier cap</style>.\nAll healing is converted into <style=cIsUtility>temporary barrier</style> that decays slower.");
            "ACHIEVEMENT_AEGISCLEARGAMEMONSOON_DESCRIPTION".Add("As Aegis, beat the game or obliterate on Monsoon.");
        }

        internal void ModifyAssets() {
            AegisBody.GetComponent<CharacterBody>().preferredPodPrefab = Utils.Paths.GameObject.SurvivorPod.Load<GameObject>();
            AegisPrimary.activationState = ContentAddition.AddEntityState<States.BarrierBeam>(out bool _);
            AegisUtility.activationState = ContentAddition.AddEntityState<States.BarrierBoost>(out bool _);
            AegisSpecial.activationState = ContentAddition.AddEntityState<States.Aegsplosion>(out bool _);
            AegisSecondary.activationState = ContentAddition.AddEntityState<States.Aegizap>(out bool _);
            SkinAegis.icon = LoadoutAPI.CreateSkinIcon(Color.yellow, Color.white, Color.grey, Color.Lerp(Color.yellow, Color.white, 5));
            SkinAegisAlt.icon = LoadoutAPI.CreateSkinIcon(Color.grey, Color.white, Color.red, Color.Lerp(Color.red, Color.white, 1));
            MasteryUnlockable.achievementIcon = SkinAegisAlt.icon;

            // default

            SkinAegis.rendererInfos = new CharacterModel.RendererInfo[] {
                new CharacterModel.RendererInfo {
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    renderer = SkinAegis.rootObject.transform.Find("MeshAegis").GetComponent<MeshRenderer>(),
                    defaultMaterial = Utils.Paths.Material.matAegis.Load<Material>(),
                    ignoreOverlays = false,
                    hideOnDeath = true
                }
            };

            // alt
            
            SkinAegisAlt.rendererInfos = new CharacterModel.RendererInfo[] {
                new CharacterModel.RendererInfo {
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    renderer = SkinAegisAlt.rootObject.transform.Find("MeshAegis").GetComponent<MeshRenderer>(),
                    defaultMaterial = Utils.Paths.Material.matTrimsheetScrapper.Load<Material>(),
                    ignoreOverlays = false,
                    hideOnDeath = true
                }
            };

            SkinAegisAlt.meshReplacements = new SkinDef.MeshReplacement[] {
                new SkinDef.MeshReplacement {
                    mesh = bundle.LoadAsset<Mesh>("Assets/PlayableAegis/mdl/ScrapBoxMesh.asset"),
                    renderer = SkinAegisAlt.rootObject.transform.Find("MeshAegis").GetComponent<MeshRenderer>()
                }
            };
        }
    }
}