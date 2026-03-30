using Config;
using Config.ConfigCells.Character;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Ai;
using GameData.Domains.Character.Creation;
using GameData.Domains.Combat;
using GameData.Domains.Combat.Ai;
using GameData.Domains.CombatSkill;
using GameData.Domains.Item;
using GameData.Domains.Map;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Agile;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Defense;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Neigong.Boss;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Neigong.RandomEnemy;
using GameData.Domains.World;
using GameData.Utilities;
using HarmonyLib;
using NLog;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using TaiwuBossMod;
using TaiwuBossMod_Backend.Passives;
using TaiwuBossMod_Backend.Utils;
using TaiwuBossMod_Frontend.Utils;
using TaiwuModdingLib.Core.Plugin;


namespace TaiwuBossMod_Backend
{

    [PluginConfig("TaiwuBossMod", "Izayoixx", "1.0.0")]
    public class BossPlugin : TaiwuRemakePlugin
    {
        // private const string MyGUID = "com.Taba.TaiwuBossMod";
        private Harmony harmony;
        public static readonly NLog.Logger BackendLogger = LogManager.GetLogger("TaiwuBossMod");
        public static string pluginDir;

        public static bool EnableBoss = false;
        public static bool CustomFeatures = false;
        public static bool SpecialEffectPatch = false;

        public static Dictionary<string, object> ModConfigData;

        private static Dictionary<string, List<Type>> CustomEffectDict = new Dictionary<string, List<Type>>();

        public static int TaiwuID;
        public static int Taiwu_Template;
        public static int featureConfigCount;

        private static List<short> NewSkillList = new List<short>();
        

        public override void Initialize()
        {

            FileLogger.Init();
            FileLogger.Info("Backend loaded");
            pluginDir = DomainManager.Mod.GetModDirectory(base.ModIdStr);
            this.harmony = Harmony.CreateAndPatchAll(typeof(BossPlugin), null);
            ModConfigData = DataFileHandler.LoadAllJsons();
            InitializeModData();
            MergeModData();
        }
        public override void OnModSettingUpdate()
        {
            BackendLogger.Info("Setting Updated!");
            DomainManager.Mod.GetSetting(base.ModIdStr, "EnableBoss", ref BossPlugin.EnableBoss);
            DomainManager.Mod.GetSetting(base.ModIdStr, "CustomFeatures", ref BossPlugin.CustomFeatures);
            DomainManager.Mod.GetSetting(base.ModIdStr, "SpecialEffectPatch", ref BossPlugin.SpecialEffectPatch);
        }
        public override void Dispose()
        {
            bool flag = this.harmony != null;
            bool flag2 = flag;
            if (flag2)
            {
                this.harmony.UnpatchSelf();
            }
        }

        public override void OnLoadedArchiveData()
        {
            base.OnLoadedArchiveData();
            FileLogger.Init();
            MergeModData();
            InitializeModData();
            UpdatePlayerChar();

        }

        public override void OnEnterNewWorld()
        {
            base.OnEnterNewWorld();
            FileLogger.Init();
            MergeModData();
            InitializeModData();
            UpdatePlayerChar();
        }

        public static void UpdatePlayerChar()
        {
            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            if (taiwu == null)
            {
                return;
            }
            TaiwuID = taiwu.GetId();
            Taiwu_Template = taiwu.GetTemplateId();
            if (EnableBoss)
            {
                //UpdateBossItems();
            }
        }

        //Obsolete for now, just change Taiwu's template ID on one of the existing Bosses and use their BossItem entry instead.
        public static void UpdateBossItems()
        {
            if (ModConfigData["BossItem"] == null) return;
            List<BossItem> bossitem = DataFileHandler.ConvertToTypedList<BossItem>(ModConfigData["BossItem"]);
            if (bossitem.Count <= 0) return;
            BossItem playerBoss = Boss.Instance[bossitem[0].TemplateId]; //only updates the first entry in BossItem
            if (playerBoss == null) return;
            short[] ids = { (short)Taiwu_Template };
            Helpers.SetPrivateField<short[]>(playerBoss, "CharacterIdList", ids);
            foreach (short id in playerBoss.CharacterIdList)
            {
                FileLogger.Info($"Boss Template ID: {playerBoss.TemplateId}, Character ID: {id}");
            }
        }

        public static void InitializeModData()
        {
            CustomEffectDict.Clear();
            NewSkillList.Clear();

            if (ModConfigData["CombatSkillItem"] != null)
            {
                List<CombatSkillItem> modSkills = DataFileHandler.ConvertToTypedList<CombatSkillItem>(ModConfigData["CombatSkillItem"]);
                foreach (CombatSkillItem skill in modSkills)
                {
                    NewSkillList.Add(skill.TemplateId);
                }
            }


            CustomEffectDict.Add("HeavenlyDemonAttack", new List<Type>
            {
                typeof(HeavenlyDemonAttack)
            });
            CustomEffectDict.Add("HeavenlyDemonAssist", new List<Type>
            {
                typeof(HeavenlyDemonAssist)
            });

            CustomEffectDict.Add("HeavenlyDemonDefense", new List<Type>
            {
                typeof(SanYuanJiuDunTianDiBian), //heal random defeat markers
                typeof(XuanYuJueShen), //apply random debuffs to enemy
                typeof(XiTuShiLing) //steal true qi when hit
            });
            CustomEffectDict.Add("HeavenlyDemonAgile", new List<Type>
            {
                typeof(HeavenlyDemonAgile),
                typeof(XieNiWuSheng),
                typeof(CheDiBingTian),
                typeof(YiFeiYan),
                typeof(ChenRan),
                typeof(BaiLongQianYuan)

            });
            CustomEffectDict.Add("HeavenlyDemonNeigong", new List<Type>
            {
                typeof(HeavenlyDemonNeigong),
                typeof(XuanYuJiuLao), //steal enemy qi on damage them
                typeof(HuanMu),
                typeof(YaoXinShiXian),
                typeof(DuoXinJiuBuZhong),
                typeof(ZhongXiangSheng)
            });
        }


        public static void InitializeModFeatures(GameData.Domains.Character.Character hero)
        {
            CharacterFeature instance = CharacterFeature.Instance;
            List<short> _featureIds = Helpers.GetPrivateField<List<short>>(hero, "_featureIds");
            featureConfigCount = instance.Count;
            MergeModData();
            if (ModConfigData == null) return;
            if (ModConfigData["CharacterFeatureItem"] == null) return;
            List<CharacterFeatureItem> features = DataFileHandler.ConvertToTypedList<CharacterFeatureItem>(ModConfigData["CharacterFeatureItem"]);

            if (featureConfigCount <= features[0].TemplateId)
            {
                return;
            }

            UpdatePlayerChar();

            foreach (CharacterFeatureItem item in features)
            {
                _featureIds.Add(item.TemplateId);
            }
        }



        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "OfflineApplyProtagonistFeature")]
        [HarmonyPrefix]
        public static bool GiveFeatures(GameData.Domains.Character.Character __instance, short protagonistFeatureId)
        {
            BackendLogger.Warn("In test");
            if (protagonistFeatureId <= 29) return true;
            if (protagonistFeatureId == 30)
            {
                InitializeModFeatures(__instance);
            }
            return false;
        }

        [HarmonyPatch(typeof(CombatCharacter), "EnableEnterCombatSkillEffect")]
        [HarmonyPostfix]
        public static void SetupTaiwu(CombatCharacter __instance, DataContext context, IEnumerable<short> skillListInCombat)
        {
            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            if (taiwu == null || __instance.GetId() != taiwu.GetId())
                return;

            // Check if the trigger skill (724), modified energizing practice neigong
            bool hasTriggerSkill = skillListInCombat.Any(skill => skill == 724);
            if (!hasTriggerSkill) return;

            var featureIds = taiwu.GetFeatureIds();
            var inventory = taiwu.GetInventory();
            var equipment = taiwu.GetEquipment();

            // =========================
            // Remove negative features
            // =========================
            if (Helpers.HasCustomFeature())
            {
                // Remove invalid features (Level < 0)
                foreach (var featureId in featureIds.ToList())
                {
                    var feature = CharacterFeature.Instance[featureId];
                    if (feature.Level < 0)
                    {
                        DomainManager.Character.GmCmd_RemoveFeature(context, taiwu.GetId(), featureId);
                    }
                }
            }

            List<WeaponItem> weapons = new List<WeaponItem>();

            if (ModConfigData["WeaponItem"] != null)
            {
                weapons = DataFileHandler.ConvertToTypedList<WeaponItem>(ModConfigData["WeaponItem"]);
            }
            if (weapons.Count > 0)
            {
                int id = weapons[0].TemplateId;
                bool hasItem = equipment.Any(item => item.TemplateId == id) || inventory.Items.Any(pair => pair.Key.TemplateId == id);

                // =========================
                // Give items if missing
                // =========================
                if (!hasItem)
                {
                    foreach (WeaponItem item in weapons)
                    {
                        var weapon = DomainManager.Item.CreateWeapon(context, item.TemplateId, 1);
                        taiwu.AddInventoryItem(context, weapon, 1);
                    }
                }
            }
            // =========================
            // Teach new skills
            // =========================

            Helpers.LearnSkillList(__instance, context, NewSkillList);
            List<short> attackSkills = new List<short>();
            foreach (short skill in NewSkillList)
            {
                CombatSkillItem SkillConfig = Config.CombatSkill.Instance[skill];
                if (SkillConfig.EquipType == 1)
                {
                    attackSkills.Add(skill);
                }

            }
            Helpers.EquipAttackSkillList(__instance, context, attackSkills);
            LearnBossSkills(__instance, context);


        }

        [HarmonyPatch(typeof(SpecialEffectDomain), "Add", new Type[]
        {
            typeof(DataContext),
            typeof(int),
            typeof(short),
            typeof(sbyte),
            typeof(sbyte)
        })]
        [HarmonyPrefix]
        public static bool CustomEffectPatch(SpecialEffectDomain __instance, int charId, short skillTemplateId, sbyte effectActiveType, sbyte direction, DataContext context)
        {
            bool specialEffectPatch = SpecialEffectPatch;
            if (true)
            {
                GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
                if (taiwu == null)
                {
                    return true;
                }
                bool flag = charId == taiwu.GetId();
                if (flag)
                {
                    CombatSkillItem combatSkillItem = Config.CombatSkill.Instance[skillTemplateId];
                    SpecialEffectItem specialEffectItem = SpecialEffect.Instance[(short)combatSkillItem.ReverseEffectID];
                    if (CustomEffectDict.ContainsKey(specialEffectItem.ClassName))
                    {
                        //do nothing, the special effect is done via the InitSkill patch
                        return false;
                    }
                    return true;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(BossNeigongBase), "OnCharAboutToFall")]
        [HarmonyPrefix]
        public static bool PlayerBossPhase(BossNeigongBase __instance, DataContext context, CombatCharacter combatChar)
        {
            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            bool flag = CombatDomain.CharId2BossId.ContainsKey(taiwu.GetTemplateId());
            bool flag2 = taiwu.GetId() == combatChar.GetId() && !flag;
            bool result;
            if (flag2)
            {
                bool flag3 = combatChar.OuterInjuryAutoHealSpeeds.Max<short>() != 1;
                if (flag3)
                {
                    DomainManager.Combat.Reset(context, combatChar);
                    DomainManager.Combat.ShowSpecialEffectTips(combatChar.GetId(), __instance.EffectId, 0);
                    combatChar.ChangeBossPhaseEffectId = __instance.EffectId;
                    Events.RaiseChangeBossPhase(context);
                    Helpers.Call(__instance, "ActivePhase2Effect", context);
                    result = false;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        [HarmonyPatch(typeof(CombatSkillDomain), "InitializeOnInitializeGameDataModule")]
        [HarmonyPrefix]
        public static void InitializeEquipSkills()
        {
            MergeModData();
            UpdatePlayerChar();
        }

        [HarmonyPatch(typeof(CombatDomain), "InitializeOnInitializeGameDataModule")]
        [HarmonyPrefix]
        public static void InitializeBossData()
        {
            MergeModData();
            UpdatePlayerChar();
        }

        [HarmonyPatch(typeof(CombatDomain), "SkillCostEnough")]
        [HarmonyPostfix]
        public static void RemoveSkillCost(CombatCharacter character, ref bool __result)
        {
            if (character.GetId() != TaiwuID) return;
            if (!Helpers.HasCustomFeature()) return;
            __result = true;
        }

        [HarmonyPatch(typeof(CombatDomain), "WeaponHasNeedTrick")]
        [HarmonyPostfix]
        public static void RemoveWeaponTrickCost(CombatCharacter character, ref bool __result)
        {
            if (character.GetId() != TaiwuID) return;
            if (!Helpers.HasCustomFeature()) return;
            __result = true;
        }

        [HarmonyPatch(typeof(CombatDomain), "HasNeedTrick")]
        [HarmonyPostfix]
        public static void RemoveTrickCost(CombatCharacter character, ref bool __result)
        {
            if (character.GetId() != TaiwuID) return;
            if (!Helpers.HasCustomFeature()) return;
            __result = true;
        }

        [HarmonyPatch(typeof(CombatDomain), "UpdateSkillCanUse", new Type[] { typeof(DataContext), typeof(CombatCharacter), typeof(short) })]
        [HarmonyPrefix]
        public static void PatchSkillKeys(CombatDomain __instance, DataContext context, CombatCharacter character, short skillId)
        {
            //if (character.GetId() != TaiwuID) return;
            CombatSkillKey skillKey = new CombatSkillKey(character.GetId(), skillId);
            if (__instance.CombatSkillDataExist(skillKey)) return;
            Helpers.Call(__instance, "AddCombatSkillData", context, character.GetId(), skillId);

        }

        [HarmonyPatch(typeof(MapDomain), "GetCharacterTemplateId")]
        [HarmonyPostfix]
        public static void CustomTaiwuTemplate(ref short __result)
        {
            MergeModData();
            if (DomainManager.Taiwu.GetTaiwu() != null) return;
            if (ModConfigData["CharacterItem"] == null) return;

            List<CharacterItem> characters = DataFileHandler.ConvertToTypedList<CharacterItem>(ModConfigData["CharacterItem"]);
            __result = characters[0].TemplateId; //the first entry in the .Json will be the protagonist template ID.
            //__result = (short)133;
        }

        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetTitles")]
        [HarmonyPrefix]
        public static void TemplateID(GameData.Domains.Character.Character __instance, ref List<short> titleIds)
        {
            if (DomainManager.Taiwu.GetTaiwu() == null) return;
            UpdatePlayerChar();
            if (TaiwuID != __instance.GetId()) return;
            if (ModConfigData["CharacterTitleItem"] == null) return;
            List<CharacterTitleItem> modTitles = DataFileHandler.ConvertToTypedList<CharacterTitleItem>(ModConfigData["CharacterTitleItem"]);
            if (__instance.GetXiangshuType() == 4)
            {
                //FileLogger.Info($"Adding Title ID: {modTitles[0].Name}");
                titleIds.Add(modTitles[0].TemplateId);
            }

        }

        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetTemplateId")]
        [HarmonyPostfix]
        public static void TemplateID(GameData.Domains.Character.Character __instance, ref short __result)
        {
            if (DomainManager.Taiwu.GetTaiwu() == null) return;
            if (TaiwuID != __instance.GetId()) return;
            if (EnableBoss)
            {
                __result = (short)48; //edit this to set it via ingame setting instead.
                return;
            }

        }

        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetCombatSkillGridCost")]
        [HarmonyPostfix]
        public static void SkillGridCost(GameData.Domains.Character.Character __instance, ref sbyte __result)
        {
            if (DomainManager.Taiwu.GetTaiwu() == null) return;
            if (TaiwuID != __instance.GetId()) return;
            __result = 1;
        }


        [HarmonyPatch(typeof(CombatCharacter), "Init")]
        [HarmonyPrefix]
        public static void InitCombatCharacter(CombatCharacter __instance, CombatDomain combatDomain, int characterId, DataContext context)
        {
            UpdatePlayerChar();
            MergeModData();
            if (EnableBoss)
            {
                AppendCharId2BossId();

                GameData.Domains.Character.Character combatChar = DomainManager.Character.GetElement_Objects(characterId);
                GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
                if (taiwu.GetId() != combatChar.GetId()) return;
                BossItem bossConfig = Boss.Instance[CombatDomain.CharId2BossId[__instance.GetCharacter().GetTemplateId()]];
                if (bossConfig == null) return;
                List<short> bossSkills = bossConfig.PhaseAttackSkills[0].ToList();
                Helpers.LearnSkillList(__instance, context, bossSkills);
            }

            //DomainManager.Taiwu.UpdateCombatSkillPlan(context, 0);
        }

        [HarmonyPatch(typeof(CombatCharacter), "OnFrameBegin")]
        [HarmonyPrefix]
        public static void AddSpecialEffect(CombatCharacter __instance)
        {
            //this is a custom function for agile skills with custom class to work!
            if (__instance.GetId() != TaiwuID) return;
            if (__instance.NeedAddEffectAgileSkillId <= 0) return; //currently for agile skills only!
            short skillTemplateId = __instance.NeedAddEffectAgileSkillId;
            if (Config.CombatSkill.Instance[skillTemplateId] != null)
            {
                CombatDomain combatDomain = DomainManager.Combat;
                DataContext context = __instance.GetDataContext();
                short charId = (short)TaiwuID;
                CombatSkillKey skillKey = new CombatSkillKey(charId, skillTemplateId);
                if (!combatDomain.CombatSkillDataExist(skillKey) && skillTemplateId > 0)
                {
                    CombatSkillData skillData = new CombatSkillData(skillKey);
                    Helpers.Call(combatDomain, "AddElement_SkillDataDict", skillKey, skillData);
                    skillData.SetLeftCdFrame(0, context);
                }
                if (DomainManager.SpecialEffect != null)
                {
                    CombatSkillItem combatSkillItem = Config.CombatSkill.Instance[skillTemplateId];
                    SpecialEffectItem specialEffectItem = SpecialEffect.Instance[(short)combatSkillItem.ReverseEffectID];
                    //only add effects of agile skills
                    if (CustomEffectDict.ContainsKey(specialEffectItem.ClassName) && combatSkillItem.EquipType == 2)
                    {
                        foreach (Type specialEffectType in CustomEffectDict[specialEffectItem.ClassName])
                        {
                            Helpers.ApplySpecialEffect(DomainManager.SpecialEffect, context, skillKey, specialEffectType, specialEffectItem.EffectActiveType);
                        }
                    }
                    __instance.NeedAddEffectAgileSkillId = -1;
                }

            }


        }


        //patch children spawned with illegal templates, important!
        [HarmonyPatch(typeof(CharacterDomain), "GenerateTemplateId")]
        [HarmonyPostfix]
        public static void patchIllegalChild(CharacterDomain __instance, short charBaseTemplateId, sbyte gender, ref short __result)
        {
            if (Config.Character.Instance[__result].CreatingType == 1) return; //valid type
            Random rng = new Random();
            int random_template = rng.Next(0, 31);
            __result = (short)(random_template + gender);
        }


        [HarmonyPatch(typeof(CombatCharacter), "Init")]
        [HarmonyPostfix]
        public static void InitSkill(CombatCharacter __instance, CombatDomain combatDomain, int characterId, DataContext context)
        {
            if (__instance.GetId() != TaiwuID) return;

            //this emulates the EnableEnterCombatSkill() function of adding special effects
            GameData.Domains.Character.Character taiwu = __instance.GetCharacter();
            short charId = (short)TaiwuID;
            foreach (short skillTemplateId in taiwu.GetEquippedCombatSkills().ToList())
            {
                CombatSkillKey skillKey = new CombatSkillKey(charId, skillTemplateId);
                if (!combatDomain.CombatSkillDataExist(skillKey) && skillTemplateId > 0)
                {
                    CombatSkillData skillData = new CombatSkillData(skillKey);
                    Helpers.Call(combatDomain, "AddElement_SkillDataDict", skillKey, skillData);
                    skillData.SetLeftCdFrame(0, context);
                }
                if (DomainManager.SpecialEffect != null)
                {
                    if (Config.CombatSkill.Instance[skillTemplateId] != null)
                    {
                        CombatSkillItem combatSkillItem = Config.CombatSkill.Instance[skillTemplateId];
                        SpecialEffectItem specialEffectItem = SpecialEffect.Instance[(short)combatSkillItem.ReverseEffectID];
                        if (CustomEffectDict.ContainsKey(specialEffectItem.ClassName) && combatSkillItem.EquipType != 2)
                        {
                            foreach (Type specialEffectType in CustomEffectDict[specialEffectItem.ClassName])
                            {
                                Helpers.ApplySpecialEffect(DomainManager.SpecialEffect, context, skillKey, specialEffectType, specialEffectItem.EffectActiveType);
                            }
                        }

                    }

                }


            }
            if (!EnableBoss) return;
            BossItem bossConfig = Boss.Instance[CombatDomain.CharId2BossId[__instance.GetCharacter().GetTemplateId()]];
            if (bossConfig == null) return;
            List<short> bossSkills = bossConfig.PhaseAttackSkills[0].ToList();
            Helpers.ReplaceAttackSkillList(__instance, context, bossSkills);
        }

        public static void AppendCharId2BossId()
        {
            sbyte bossId = 0;
            while ((int)bossId < Boss.Instance.Count)
            {
                short[] charIdList = Boss.Instance[bossId].CharacterIdList;
                foreach (short charId in charIdList)
                {
                    CombatDomain.CharId2BossId[charId] = bossId;
                }
                bossId += 1;
            }
        }

        /*
        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "OfflineCreateIntelligentCharacter")]
        [HarmonyPrefix]
        public static bool PatchCharType(GameData.Domains.Character.Character __instance, ref IntelligentCharacterCreationInfo info)
        {
            CharacterItem template = Config.Character.Instance[info.CharTemplateId];
            if (template.CreatingType == 1) return true;
            return false;
            Helpers.SetPrivateField<byte>(template, "CreatingType", (byte)1);

        }
        [HarmonyPatch(typeof(CharacterDomain), "TryCreateGeneralRelation")]
        [HarmonyPrefix]
        public static bool PatchRelation(GameData.Domains.Character.Character selfChar, GameData.Domains.Character.Character relatedChar)
        {
            if (selfChar.GetCreatingType() != 1 || selfChar.GetCreatingType() != 1) return false ;
            return true;

        }
        */

        [HarmonyPatch(typeof(CombatCharacter), "SetAffectingMoveSkillId")]
        [HarmonyPostfix]
        public static void MoveSkillID(CombatCharacter __instance, short affectingMoveSkillId)
        {
            if (DomainManager.Taiwu.GetTaiwu() == null) return;
            if (TaiwuID != __instance.GetId()) return;
            FileLogger.Info($"[SetAffectingMoveSkilId] MoveSkillId = {affectingMoveSkillId}");
        }

        public static void LearnBossSkills(CombatCharacter character, DataContext context)
        {
            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            if (taiwu == null) return;
            if (character.GetId() != TaiwuID) return;
            List<short> bossSkills = new List<short>();
            for (int i = 0; i < Config.Boss.Instance.Count; i++)
            {
                BossItem bossConfig = Boss.Instance[i];
                bossSkills.AddRange(bossConfig.PlayerCastSkills);
            }
            Helpers.LearnSkillList(character, context, bossSkills);
        }

        public static void MergeModData()
        {
            ModConfigData = DataFileHandler.LoadAllJsons();
            DataFileHandler.LoadAndMergeAll();
        }
    }
}
