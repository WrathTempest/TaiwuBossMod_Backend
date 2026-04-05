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
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Attack.HuanXin;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Attack.ShuFang;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Attack.WeiQi;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Attack.XiangShu;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Attack.XueFeng;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Attack.YiXiang;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Defense;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Neigong.Boss;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Neigong.RandomEnemy;
using GameData.Domains.SpecialEffect.LegendaryBook.Leg;
using GameData.Domains.World;
using GameData.Utilities;
using HarmonyLib;
using NLog;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using TaiwuBossMod;
using TaiwuBossMod_Backend.Passives;
using TaiwuBossMod_Backend.Utils;
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


        public static int SectRef = 1;
        public static int BossID = 14;
        public static bool RemoveNegFeatures = false;
        public static bool LearnBossSkills = false;
        public static bool SetHarmony = false;
        public static bool FreeSkills = false;
        public static bool NoWeight = false;
        public static bool CustomWeapon = false;
        public static bool HealthBuff = false;
        public static bool ModifyRatio = false;
        public static int InnerRatio = 50;

        private static List<CustomEffect> CustomEffectDict = new List<CustomEffect>();

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
            MergeModData();
            InitializeModData();


        }

        private static void DumpSectIds()
        {
            Dictionary<sbyte, string> sectDict = new Dictionary<sbyte, string>();
            foreach (CombatSkillItem skill in Config.CombatSkill.Instance)
            {
                sbyte sectID = skill.SectId;
                foreach (OrganizationItem org in Config.Organization.Instance)
                {
                    if (sectID == org.TemplateId && !sectDict.ContainsKey(org.TemplateId))
                    {
                        sectDict.TryAdd(sectID, org.Name);
                    }
                }
            }
            foreach (var kv in sectDict)
            {
                FileLogger.Info($"Sect ID: {kv.Key}, Sect Name: {kv.Value}");
            }
        }
        public override void OnModSettingUpdate()
        {
            DomainManager.Mod.GetSetting(base.ModIdStr, "SectRef", ref SectRef);
            DomainManager.Mod.GetSetting(base.ModIdStr, "BossID", ref BossID);
            DomainManager.Mod.GetSetting(base.ModIdStr, "RemoveNegFeatures", ref RemoveNegFeatures);
            DomainManager.Mod.GetSetting(base.ModIdStr, "LearnBossSkills", ref LearnBossSkills);
            DomainManager.Mod.GetSetting(base.ModIdStr, "SetHarmony", ref SetHarmony);
            DomainManager.Mod.GetSetting(base.ModIdStr, "FreeSkills", ref FreeSkills);
            DomainManager.Mod.GetSetting(base.ModIdStr, "NoWeight", ref NoWeight);
            DomainManager.Mod.GetSetting(base.ModIdStr, "CustomWeapon", ref CustomWeapon);
            DomainManager.Mod.GetSetting(base.ModIdStr, "HealthBuff", ref HealthBuff);
            DomainManager.Mod.GetSetting(base.ModIdStr, "ModifyRatio", ref ModifyRatio);
            DomainManager.Mod.GetSetting(base.ModIdStr, "InnerRatio", ref InnerRatio);
            if (BossID >= 14 || BossID == 10)
            {
                BossID = -1;
            }
            FileLogger.Info($"Current Sect ID:{SectRef}");
            //DumpSectIds();
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
        }

        public static void InitializeModData()
        {
            CustomEffectDict.Clear();
            NewSkillList.Clear();

            List<CombatSkillItem> modSkills = ModConfigData.GetModConfigData<CombatSkillItem>("CombatSkillItem");
            foreach (CombatSkillItem skill in modSkills)
            {
                NewSkillList.Add(skill.TemplateId);
            }

            CustomEffectDict.Add(new CustomEffect("HeavenlyDemonAttack", new List<Type>
            {
                typeof(HeavenlyDemonAttack),
                typeof(ChaiRen),
                typeof(XieWangWuZheng)
            }));
            CustomEffectDict.Add(new CustomEffect("HeavenlyDemonAssist", new List<Type>
            {
                typeof(HeavenlyDemonAssist)
            }));
            CustomEffectDict.Add(new CustomEffect("HeavenlyDemonDefense", new List<Type>
            {
                typeof(SanYuanJiuDunTianDiBian), //heal random defeat markers
                typeof(XuanYuJueShen), //apply random debuffs to enemy
                typeof(XiTuShiLing), //steal true qi when hit
                typeof(JiuBao) //after skill redistribute injuries
            }));
            CustomEffectDict.Add(new CustomEffect("HeavenlyDemonAgile", new List<Type>
            {
                typeof(HeavenlyDemonAgile),
                typeof(XieNiWuSheng),
                typeof(CheDiBingTian),
                typeof(YiFeiYan),
                typeof(ChenRan),
                typeof(BaiLongQianYuan)
            }));
            CustomEffectDict.Add(new CustomEffect("HeavenlyDemonNeigong", new List<Type>
            {
                typeof(HeavenlyDemonNeigong),
                typeof(XuanYuJiuLao), //steal enemy qi on damage them
                typeof(HuanMu),
                typeof(BaiXie),
                typeof(YaoXinShiXian),
                typeof(DuoXinJiuBuZhong),
                typeof(ZhongXiangSheng),
                typeof(ShenNvHuanJian), //MoNyu second phase
                typeof(QiHanLingQi), //reduce inhale/stance recovery in attack range
                typeof(RongChenHuaYu),
                typeof(FuJunYouYu),
                typeof(QingGuoJueShi)
            }));
        }


        public static void InitializeModFeatures(GameData.Domains.Character.Character hero)
        {
            CharacterFeature instance = CharacterFeature.Instance;
            List<short> _featureIds = Helpers.GetPrivateField<List<short>>(hero, "_featureIds");
            featureConfigCount = instance.Count;
            MergeModData();
            List<CharacterFeatureItem> features = ModConfigData.GetModConfigData<CharacterFeatureItem>("CharacterFeatureItem");

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
            if (protagonistFeatureId <= 29) return true;
            if (protagonistFeatureId == 30)
            {
                InitializeModFeatures(__instance);
            }
            return false;
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

            CombatSkillItem combatSkillItem = Config.CombatSkill.Instance[skillTemplateId];
            SpecialEffectItem specialEffectItem = SpecialEffect.Instance[(short)combatSkillItem.ReverseEffectID];
            foreach (CustomEffect effect in CustomEffectDict)
            {
                if (specialEffectItem.ClassName == effect.ClassName)
                {
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(BossNeigongBase), "OnCharAboutToFall")]
        [HarmonyPrefix]
        public static bool PlayerBossPhase(BossNeigongBase __instance, DataContext context, CombatCharacter combatChar)
        {
            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            bool result;
            if (taiwu.GetId() == combatChar.GetId())
            {
                bool flag3 = combatChar.OuterInjuryAutoHealSpeeds.Max<short>() != 1;
                if (flag3)
                {
                    DomainManager.Combat.Reset(context, combatChar);               
                    combatChar.ChangeBossPhaseEffectId = __instance.EffectId;
                    DomainManager.Combat.ShowSpecialEffectTips(combatChar.GetId(), __instance.EffectId, 0);
                    combatChar.SetXiangshuEffectId((short)__instance.EffectId, context);
                    combatChar.SetMindMarkInfinityCount(0, context);
                    combatChar.SetMindMarkInfinityProgress(0, context);
                    combatChar.SetHazardValue(0, context);
                    combatChar.GetCharacter().SetHealth(combatChar.GetCharacter().GetLeftMaxHealth(false), context);
                    combatChar.GetCharacter().SetDisorderOfQi(combatChar.GetOldDisorderOfQi(), context);
                    combatChar.SetMixPoisonAffectedCount(combatChar.GetMixPoisonAffectedCount().Clear(), context);
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
            if (!FreeSkills) return;
            __result = true;
        }

        [HarmonyPatch(typeof(CombatDomain), "WeaponHasNeedTrick")]
        [HarmonyPostfix]
        public static void RemoveWeaponTrickCost(CombatCharacter character, ref bool __result)
        {
            if (character.GetId() != TaiwuID) return;
            if (!FreeSkills) return;
            __result = true;
        }

        [HarmonyPatch(typeof(CombatDomain), "HasNeedTrick")]
        [HarmonyPostfix]
        public static void RemoveTrickCost(CombatCharacter character, ref bool __result)
        {
            if (character.GetId() != TaiwuID) return;
            if (!FreeSkills) return;
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
            if (!Helpers.HasCustomFeature()) return;
            List<CharacterItem> characters = ModConfigData.GetModConfigData<CharacterItem>("CharacterItem");
            if (characters[0] == null) return;
            __result = characters[0].TemplateId; //the first entry in the .Json will be the protagonist template ID.
        }

        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetTitles")]
        [HarmonyPrefix]
        public static void TemplateID(GameData.Domains.Character.Character __instance, ref List<short> titleIds)
        {
            if (DomainManager.Taiwu.GetTaiwu() == null) return;
            UpdatePlayerChar();
            if (TaiwuID != __instance.GetId()) return;
            List<CharacterTitleItem> modTitles = ModConfigData.GetModConfigData<CharacterTitleItem>("CharacterTitleItem");
            if (modTitles[0] != null && __instance.GetXiangshuType() == 4)
            {
                //FileLogger.Info($"Adding Title ID: {modTitles[0].Name}");
                titleIds.Add(modTitles[0].TemplateId);
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
        

        //patch for defense skills
        [HarmonyPatch(typeof(CombatDomain), "ApplyAgileOrDefenseSkill")]
        [HarmonyPrefix]

        public static void ApplyDefenseEffect(CombatDomain __instance, CombatCharacter character, CombatSkillItem skillConfig)
        {
            if (character.GetId() != TaiwuID) return;
            if (!character.IsAlly) return;
            if (skillConfig.EquipType != 3) return;
            short skillTemplateId = skillConfig.TemplateId;
            DataContext context = __instance.Context;
            short charId = (short)TaiwuID;
            CombatSkillKey skillKey = new CombatSkillKey(charId, skillTemplateId);
            if (!__instance.CombatSkillDataExist(skillKey) && skillTemplateId > 0)
            {
                CombatSkillData skillData = new CombatSkillData(skillKey);
                Helpers.Call(__instance, "AddElement_SkillDataDict", skillKey, skillData);
                skillData.SetLeftCdFrame(0, context);
            }
            if (DomainManager.SpecialEffect != null)
            {
                CombatSkillItem combatSkillItem = Config.CombatSkill.Instance[skillTemplateId];
                SpecialEffectItem specialEffectItem = SpecialEffect.Instance[(short)combatSkillItem.ReverseEffectID];
                foreach (CustomEffect effect in CustomEffectDict)
                {
                    if (specialEffectItem.ClassName == effect.ClassName)
                    {
                        foreach (var kv in effect.Effects)
                        {
                            if (SpecialEffect.Instance[kv.Key].EffectActiveType == 0)
                            {
                                Helpers.ApplySpecialEffect(DomainManager.SpecialEffect, context, skillKey, kv.Value, 0);
                            }
                        }
                        break;
                    }
                }
            }

        }
        

        //need patch for agile skills
        [HarmonyPatch(typeof(CombatCharacter), "OnFrameBegin")]
        [HarmonyPrefix]
        public static void AddSpecialEffect(CombatCharacter __instance)
        {
            //this is a custom function for agile skills with custom class to work!
            if (__instance.GetId() != TaiwuID) return;
            if (!__instance.IsAlly) return;
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
                    foreach (CustomEffect effect in CustomEffectDict)
                    {
                        if (specialEffectItem.ClassName == effect.ClassName)
                        {
                            foreach (var kv in effect.Effects)
                            {
                                if (combatSkillItem.EquipType == 2)
                                {
                                    Helpers.ApplySpecialEffect(DomainManager.SpecialEffect, context, skillKey, kv.Value, SpecialEffect.Instance[kv.Key].EffectActiveType);
                                }
                            }
                            __instance.NeedAddEffectAgileSkillId = -1;
                            break;
                        }
                    }
                    
                }

            }


        }
        [HarmonyPatch(typeof(CombatCharacterStatePrepareSkill), "OnEnter")]
        [HarmonyPrefix]
        public static void AddSpecialEffect(CombatCharacterStatePrepareSkill __instance)
        {
            CombatCharacter combatChar = Helpers.GetPrivateField<CombatCharacter>(__instance, "CombatChar");
            if (!combatChar.IsAlly) return;
            if (combatChar == null) return;
            short charId = (short)combatChar.GetId();
            DataContext context = combatChar.GetDataContext();
            if (charId != TaiwuID) return;
            short skillTemplateId = (combatChar.NeedUseSkillFreeId >= 0 ? combatChar.NeedUseSkillFreeId : combatChar.NeedUseSkillId);
            if (skillTemplateId >= 0)
            {
                CombatSkillKey skillKey = new CombatSkillKey(charId, skillTemplateId);
                if (!DomainManager.Combat.CombatSkillDataExist(skillKey) && skillTemplateId > 0)
                {
                    CombatSkillData skillData = new CombatSkillData(skillKey);
                    Helpers.Call(DomainManager.Combat, "AddElement_SkillDataDict", skillKey, skillData);
                    skillData.SetLeftCdFrame(0, context);
                }
                if (DomainManager.SpecialEffect != null)
                {
                    if (Config.CombatSkill.Instance[skillTemplateId] != null)
                    {
                        CombatSkillItem combatSkillItem = Config.CombatSkill.Instance[skillTemplateId];
                        SpecialEffectItem specialEffectItem = SpecialEffect.Instance[(short)combatSkillItem.ReverseEffectID];
                        foreach (CustomEffect effect in CustomEffectDict)
                        {
                            if (specialEffectItem.ClassName == effect.ClassName)
                            {
                                foreach (var kv in effect.Effects)
                                {
                                    if (SpecialEffect.Instance[kv.Key].EffectActiveType == 0 && combatSkillItem.EquipType != 2)
                                    {
                                        Helpers.ApplySpecialEffect(DomainManager.SpecialEffect, context, skillKey, kv.Value, 0);
                                    }
                                }
                                break;
                            }
                        }

                    }

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
            if (!__instance.IsAlly) return;
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
                        foreach (CustomEffect effect in CustomEffectDict)
                        {
                            if (specialEffectItem.ClassName == effect.ClassName)
                            {
                                foreach(var kv in effect.Effects)
                                {
                                    if (SpecialEffect.Instance[kv.Key].EffectActiveType == 1 && combatSkillItem.EquipType != 2)
                                    {
                                        Helpers.ApplySpecialEffect(DomainManager.SpecialEffect, context, skillKey, kv.Value, 1);
                                    }
                                }
                                break;
                            }
                        }
                    }

                }


            }
        }

        [HarmonyPatch(typeof(CombatCharacter), "Init")]
        [HarmonyPrefix]
        public static void InitCombatCharacter(CombatCharacter __instance, CombatDomain combatDomain, int characterId, DataContext context)
        {
            UpdatePlayerChar();
            MergeModData();
            if (characterId != TaiwuID) return;
            GameData.Domains.Character.Character _character = DomainManager.Character.GetElement_Objects(characterId);
            
            if (IsBossEnabled())
            {
                
                AppendTemplateId2BossID(_character.GetTemplateId());
                BossItem bossConfig = Boss.Instance[BossID];
                CharacterItem charConfig = Config.Character.Instance[bossConfig.CharacterIdList[bossConfig.CharacterIdList.Length - 1]];
                if (IsBoss(__instance, characterId))
                {
                    InitializeBoss(characterId, context);
                }
            }          
            else
            {
                RefreshCharId2BossId();
                if (CustomWeapon)
                {
                    Helpers.RefreshModWeapons(_character, context);
                }
                
            }
        }


        //also handles boss skills!
        [HarmonyPatch(typeof(CombatCharacter), "EnableEnterCombatSkillEffect")]
        [HarmonyPrefix]
        public static void SetupTaiwu(CombatCharacter __instance, DataContext context, IEnumerable<short> skillListInCombat)
        {
            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            if (taiwu == null || __instance.GetId() != taiwu.GetId()) return;
            if (!Helpers.HasCustomFeature()) return;
            // Check if the trigger skill (724), modified energizing practice neigong
            foreach (short id in skillListInCombat)
            {
                if (Config.CombatSkill.Instance[id] == null) continue;
                if (Config.CombatSkill.Instance[id].EquipType != 0) return;
            }
            var featureIds = taiwu.GetFeatureIds();

            // =========================
            // Remove negative features
            // =========================
            if (RemoveNegFeatures)
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


            // =========================
            // Teach mod skills
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
            if (LearnBossSkills)
            {
                SetBossSkills(__instance, context);
            }
            
            LearnSectSkills(__instance, context);

            if (IsBoss(__instance, __instance.GetId()) && IsBossEnabled())
            {
                ReplaceBossSkills(__instance, __instance.GetId(), context);
            }

            //redistribute so that the elements are in harmony
            if (SetHarmony)
            {
                taiwu.SetBaseNeiliProportionOfFiveElements(new NeiliProportionOfFiveElements(20, 20, 20, 20, 20), context);
            }
            

        }
        

        public static bool IsBossEnabled()
        {
            if (BossID < 0 || BossID >= Boss.Instance.Count || Boss.Instance[BossID] == null) return false;
            return true;
        }

        public static bool IsBoss(short id)
        {
            if (CombatDomain.CharId2BossId.ContainsKey(id)) return true;
            return false;
        }

        public static bool IsBoss(CombatCharacter combatCharacter, int characterId)
        {
            short templateId = DomainManager.Character.GetElement_Objects(characterId).GetTemplateId();
            if (CombatDomain.CharId2BossId.ContainsKey(templateId)) return true;
            return false;
        }

        public static void InitializeBoss(int characterId, DataContext context)
        {

            GameData.Domains.Character.Character _character = DomainManager.Character.GetElement_Objects(characterId);
            short templateId = _character.GetTemplateId();
            BossItem bossConfig = Boss.Instance[BossID];
            CharacterItem charConfig = Config.Character.Instance[bossConfig.CharacterIdList[bossConfig.CharacterIdList.Length - 1]];
            InitializeBossWeapons(_character, context, charConfig);
        }
        public static void ReplaceBossSkills(CombatCharacter combatCharacter, int characterId, DataContext context)
        {
           
            GameData.Domains.Character.Character _character = DomainManager.Character.GetElement_Objects(characterId);
            BossItem bossConfig = Boss.Instance[BossID];
            CharacterItem charConfig = Config.Character.Instance[bossConfig.CharacterIdList[bossConfig.CharacterIdList.Length - 1]];
            List<short> attackSkills = bossConfig.PhaseAttackSkills[0].ToList();
            attackSkills.AddRange(bossConfig.PhaseAttackSkills[1].ToList());
            List<short> neigongSkills = new List<short>();
            List<short> agileSkills = new List<short>();
            List<short> defenseSkills = new List<short>();
            foreach (PresetCombatSkill skill in charConfig.PresetCombatSkills)
            {
                short skillId = skill.SkillTemplateId;
                CombatSkillItem skillConfig = Config.CombatSkill.Instance[skillId];
                switch (skillConfig.EquipType)
                {
                    case 0: neigongSkills.Add(skillId); break;
                    case 2: agileSkills.Add(skillId); break;
                    case 3: defenseSkills.Add(skillId); break;
                    default: break;
                }
            }
            FileLogger.Info($"[ReplaceBossSkills] attempting to set boss skills...");
            FileLogger.Info($"Setting Attack Skills...");
            Helpers.ReplaceAttackSkillList(combatCharacter, context, attackSkills);
            FileLogger.Info($"Successfully Set Attack Skills!");
            FileLogger.Info($"Setting Movement Skills...");
            Helpers.ReplaceAgileSkillList(combatCharacter, context, agileSkills);
            FileLogger.Info($"Successfully Set Movement Skills!");
            FileLogger.Info($"Setting Defense Skills...");
            Helpers.ReplaceDefenseSkillList(combatCharacter, context, defenseSkills);
            FileLogger.Info($"Successfully Set Defense Skills!");
            FileLogger.Info($"Setting Neigong Skills...");
            Helpers.AppendNeigongSkills(combatCharacter, context, neigongSkills);
            FileLogger.Info($"Successfully Set Neigong Skills!");
        }

        public static void InitializeBossWeapons(GameData.Domains.Character.Character character, DataContext context, CharacterItem charConfig)
        {
            var equipment = character.GetEquipment();
            var inventory = character.GetInventory();
            sbyte[] weaponSlots = EquipmentSlot.EquipmentType2Slots[0];
            List<short> weaponIds = new List<short>();
            foreach (PresetEquipmentItem presetEquipment in charConfig.PresetEquipment)
            {
                if (presetEquipment.Type == 0)
                {
                    weaponIds.Add(presetEquipment.TemplateId);
                }
            }
            int weaponCount = weaponIds.Count() > 3 ? 3 : weaponIds.Count();
            if (weaponIds.Count > 0)
            {
                int id = weaponIds[0];

                for (int i = 0; i < weaponSlots.Length; i++)
                {
                    if (equipment[(int)weaponSlots[i]].IsValid())
                    {
                        //remove currently equipped equipment
                        ItemKey weapon = equipment[(int)weaponSlots[i]];
                        character.ChangeEquipment(context, weaponSlots[i], -1, ItemKey.Invalid);
                        character.RemoveInventoryItem(context, weapon, 1, true);
                    }
                }
                for (int i = 0; i < weaponCount; i++)
                {
                    ItemKey weapon = DomainManager.Item.CreateWeapon(context, weaponIds[i], 1);
                    character.AddInventoryItem(context, weapon, 1);
                    character.ChangeEquipment(context, -1, weaponSlots[i], weapon);
                }

            }
        }

        public static void RefreshCharId2BossId()
        {
            //reset
            CombatDomain.CharId2BossId.Clear();
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

        public static void AppendTemplateId2BossID(short templateId)
        {
            //reset
            CombatDomain.CharId2BossId.Clear();
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
            CombatDomain.CharId2BossId[templateId] = (sbyte)BossID;
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

        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "CalcCurrInventoryLoad")]
        [HarmonyPostfix]
        public static void CharInventory(GameData.Domains.Character.Character __instance, ref int __result)
        {
            if (!Helpers.HasCustomFeature(__instance)) return;
            if (!NoWeight) return;
            __result = 0;
        }

        [HarmonyPatch(typeof(CombatDomain), "GetDamageStepCollection")]
        [HarmonyPostfix]
        public static void BodyPartHealth(CombatDomain __instance, int charId, ref DamageStepCollection __result)
        {
            if (DomainManager.Taiwu.GetTaiwu == null) return;
            if (!HealthBuff) return;
            if (charId != TaiwuID) return;
            if (!Helpers.HasCustomFeature()) return;
            for (int i = 0; __result.OuterDamageSteps.Length > i; i++)
            {
                __result.OuterDamageSteps[i] = (int)(__result.OuterDamageSteps[i] * 1.5);
            }
            for (int i = 0; __result.InnerDamageSteps.Length > i; i++)
            {
                __result.InnerDamageSteps[i] = (int)(__result.InnerDamageSteps[i] * 1.5);
            }
            __result.FatalDamageStep = 9999;
            __result.MindDamageStep = (int)(__result.MindDamageStep * 1.5);
        }

        // patch needed for mirror battle
        [HarmonyPatch(typeof(AiController), "Init")]
        [HarmonyPrefix]
        public static bool AI_Init(AiController __instance, DataContext context)
        {
            Helpers.Call(__instance, "InitHazard");
            __instance.Environment.RegisterCallbacks();
            __instance.Memory.RegisterCallbacks();

            CombatCharacter combatCharacter = Helpers.GetPrivateField<CombatCharacter>(__instance, "_combatCharacter");

            if (combatCharacter != null && __instance.IsCombatDifficultyLevel2)
            {
                List<short> selfLearnedSkills = combatCharacter.GetCharacter().GetLearnedCombatSkills();
                int[] enemyTeam = combatCharacter.IsAlly
                    ? DomainManager.Combat.GetEnemyTeam()
                    : DomainManager.Combat.GetSelfTeam();

                foreach (int enemyId in enemyTeam)
                {
                    if (enemyId < 0)
                        continue;

                    List<short> attackSkillList = DomainManager.Combat
                        .GetElement_CombatCharacterDict(enemyId)
                        .GetAttackSkillList();

                    foreach (short skillId in attackSkillList)
                    {
                        if (!selfLearnedSkills.Contains(skillId))
                            continue;

                        var skillRecord = __instance.Memory.EnemyRecordDict[enemyId].SkillRecord;

                        if (!skillRecord.ContainsKey(skillId))
                        {
                            skillRecord.Add(skillId, new ValueTuple<int, int>(400, 0));
                        }
                    }
                }
            }
            MethodInfo method = AccessTools.Method(typeof(AiController), "OnCombatBegin");
            var handler = (Events.OnCombatBegin)Delegate.CreateDelegate(
                typeof(Events.OnCombatBegin),
                __instance,
                method
            );

            Events.RegisterHandler_CombatBegin(new Events.OnCombatBegin(handler));

            return false;
        }

        [HarmonyPatch(typeof(GameData.Domains.CombatSkill.CombatSkill), "PowerMatchAffectRequire")]
        [HarmonyPrefix]
        public static bool PowerMatchAffectRequire_Patch(GameData.Domains.CombatSkill.CombatSkill __instance, ref bool __result, int power, int index)
        {
            int i = 0;
            foreach (int requirePower in __instance.GetAffectRequirePower())
            {
                bool flag = i++ == index;
                if (flag)
                {
                    return true;
                }
            }
            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(CombatCharacterStateAttack), "StartAttack")]
        [HarmonyPrefix]
        public static void AttackPrepareAniPatch3(CombatCharacterStateAttack __instance)
        {
            if (IsBossEnabled()) return;
            CombatCharacter CombatChar = Helpers.GetPrivateField<CombatCharacter>(__instance, "CombatChar");
            if (CombatChar.GetId() != TaiwuID) return;
            sbyte _trickType = Helpers.GetPrivateField<sbyte>(__instance, "_trickType");
            if (_trickType == 21)
            {
                TrickTypeItem trick = Config.TrickType.Instance[_trickType];
                Helpers.SetPrivateField<sbyte[]>(trick, "AttackDistance", Config.TrickType.Instance[_currentTrick].AttackDistance);
            }
        }

        [HarmonyPatch(typeof(CombatDomain), "GetPrepareAttackAni")]
        [HarmonyPrefix]
        public static void AttackPrepareAniPatch(CombatCharacter character, ref sbyte trickType)
        {
            if (IsBossEnabled()) return;
            if (character.GetId() == TaiwuID)
            {
                List<sbyte> tricks = new List<sbyte>() { 3, 4, 5, 9, 10, 11, 12 };
                if (trickType == 21)
                {
                    Random rng = new Random();
                    int idx = rng.Next(0, tricks.Count());
                    trickType = tricks[idx];
                    _currentTrick = trickType;
                }
            }

        }

        private static sbyte _currentTrick;

        [HarmonyPatch(typeof(CombatDomain), "GetAttackEffect")]
        [HarmonyPrefix]
        public static void AttackPrepareAniPatch2(CombatCharacter character, ref sbyte trickType)
        {
            if (IsBossEnabled()) return;
            if (character.GetId() == TaiwuID)
            {
                trickType = _currentTrick;
            }
        }

        [HarmonyPatch(typeof(GameData.Domains.CombatSkill.CombatSkill), "CalcCurrInnerRatio")]
        [HarmonyPostfix]
        public static void ModifyInnerRatio(GameData.Domains.CombatSkill.CombatSkill __instance, ref sbyte __result)
        {
            if (__instance.GetId().CharId != TaiwuID) return;
            if (!ModifyRatio) return;
            if (!Helpers.HasCustomFeature()) return;
            __result = (sbyte)InnerRatio;

        }

        public static void LearnSectSkills(CombatCharacter character, DataContext context)
        {
            if (SectRef == 0) return;
            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            if (taiwu == null) return;
            if (character.GetId() != TaiwuID) return;
            List<short> sectSkills = new List<short>();
            foreach (CombatSkillItem skill in Config.CombatSkill.Instance)
            {
                if (skill.SectId == SectRef)
                {
                    sectSkills.Add(skill.TemplateId);
                }
            }
            Helpers.LearnSkillList(character, context, sectSkills);
        }

        public static void SetBossSkills(CombatCharacter character, DataContext context)
        {
            GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
            if (taiwu == null) return;
            if (character.GetId() != TaiwuID) return;
            List<short> bossSkills = new List<short>();
            for (int i = 0; i < Boss.Instance.Count; i++)
            {
                BossItem bossConfig = Boss.Instance[i];
                foreach (short skill in bossConfig.PlayerCastSkills)
                {
                    if (bossConfig.PhaseAttackSkills[1].Contains(skill))
                    {
                        bossSkills.Add(skill);
                    }
                    
                }         
            }
            Helpers.LearnSkillList(character, context, bossSkills);
        }

        public static void MergeModData()
        {
            ModConfigData.Initialize();
            DataFileHandler.LoadAndMergeAll();
        }
    }
}
