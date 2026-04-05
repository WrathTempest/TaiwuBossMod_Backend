using GameData.Common;
using GameData.Domains;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill.Common.Assist;
using GameData.GameDataBridge;
using GameData.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TaiwuBossMod;
using TaiwuBossMod_Backend.Utils;


namespace TaiwuBossMod_Backend.Passives
{
    public class HeavenlyDemonAssist : AssistSkillBase
    {
        // Token: 0x06000023 RID: 35 RVA: 0x00003252 File Offset: 0x00001452
        public HeavenlyDemonAssist()
        {
        }

        // Token: 0x06000024 RID: 36 RVA: 0x0000325C File Offset: 0x0000145C
        public HeavenlyDemonAssist(CombatSkillKey skillKey) : base(skillKey, 69420)
        {
            this.SetConstAffectingOnCombatBegin = true;
        }

        // Token: 0x06000025 RID: 37 RVA: 0x00003273 File Offset: 0x00001473
        public HeavenlyDemonAssist(int charId)
        {
            this.SetConstAffectingOnCombatBegin = true;
        }

        // Token: 0x06000026 RID: 38 RVA: 0x00003284 File Offset: 0x00001484
        public override void OnEnable(DataContext context)
        {
            this._addPower = 0;
            this._defeatMarkUid = base.ParseCombatCharacterDataUid(54);
            GameDataBridge.AddPostDataModificationHandler(this._defeatMarkUid, base.DataHandlerKey, new Action<DataContext, DataUid>(this.OnMarkChanged));
            base.CreateAffectedData(142, EDataModifyType.Custom, -1);
            base.CreateAffectedData(137, EDataModifyType.Custom, -1);
            base.CreateAffectedData(211, EDataModifyType.AddPercent, -1);
            FileLogger.Info($"Enabled HeavenlyDemonAssist! Current Power: {this._addPower}");
            //immune to certain stuff
            this.AffectDatas.Add(new AffectedDataKey(base.CharacterId, 159, -1, -1, -1, -1), EDataModifyType.Custom);
            this.AffectDatas.Add(new AffectedDataKey(base.CharacterId, 229, -1, -1, -1, -1), EDataModifyType.Custom);
            this.AffectDatas.Add(new AffectedDataKey(base.CharacterId, 227, -1, -1, -1, -1), EDataModifyType.Custom);
            this.AffectDatas.Add(new AffectedDataKey(base.CharacterId, 93, -1, -1, -1, -1), EDataModifyType.Custom);
            this.AffectDatas.Add(new AffectedDataKey(base.CharacterId, 83, -1, -1, -1, -1), EDataModifyType.Custom);
            this.AffectDatas.Add(new AffectedDataKey(base.CharacterId, 84, -1, -1, -1, -1), EDataModifyType.Custom);
            this.AffectDatas.Add(new AffectedDataKey(base.CharacterId, 156, -1, -1, -1, -1), EDataModifyType.Add);
            this.AffectDatas.Add(new AffectedDataKey(base.CharacterId, 157, -1, -1, -1, -1), EDataModifyType.Add);
        }

        // Token: 0x06000027 RID: 39 RVA: 0x000033BD File Offset: 0x000015BD
        public override void OnDisable(DataContext context)
        {
            GameDataBridge.RemovePostDataModificationHandler(this._defeatMarkUid, base.DataHandlerKey);
        }

        private void OnMarkChanged(DataContext context, DataUid dataUid)
        {
            this._addPower = AddPowerUnit * (base.CombatChar.GetDefeatMarkCollection().GetTotalCount());
            DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 211);
            FileLogger.Info($"Mark Changed! Current Power: {this._addPower}");
        }

        protected override void OnCanUseChanged(DataContext context, DataUid dataUid)
        {
            base.SetConstAffecting(context, base.CanAffect);
            DomainManager.SpecialEffect.InvalidateCache(context, base.CharacterId, 211);
        }
        public override bool GetModifiedValue(AffectedDataKey dataKey, bool dataValue)
        {
            bool flag = dataKey.CharId != this.CharacterId || !base.CanAffect;
            bool flag2 = flag;
            bool result;
            if (flag2)
            {
                result = dataValue;
            }
            //force values you want to be true
            else if (dataKey.FieldId == 83 || dataKey.FieldId == 84)
            {
                result = true;
            }
            //force it to be false
            else if (dataKey.FieldId == 227 || dataKey.FieldId == 142 || dataKey.FieldId == 137 || dataKey.FieldId == 159 || dataKey.FieldId == 229 || dataKey.FieldId == 93)
            {
                result = false;
            }
            else
            {
                result = dataValue;
            }
            return result;
        }

        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            // Only affect the target character while the effect is active
            if (dataKey.CharId != base.CharacterId || !base.CanAffect)
                return 0;

            switch (dataKey.FieldId)
            {
                // Apply +10000 to field IDs 156 and 157
                case 156:
                case 157:
                    return 10000;

                // Apply custom power bonus to field ID 211
                case 211:
                    return this._addPower;

                // No change for all other fields
                default:
                    return 0;
            }
        }

        // Token: 0x04000011 RID: 17
        private const sbyte ChangeDamage = 50;

        // Token: 0x04000012 RID: 18
        private const sbyte AddPowerUnit = 15;

        // Token: 0x04000013 RID: 19
        private DataUid _defeatMarkUid;

        // Token: 0x04000014 RID: 20
        private int _addPower;
    }
}
