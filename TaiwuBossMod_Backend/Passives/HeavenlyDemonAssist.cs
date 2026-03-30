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
        }

        // Token: 0x06000027 RID: 39 RVA: 0x000033BD File Offset: 0x000015BD
        public override void OnDisable(DataContext context)
        {
            GameDataBridge.RemovePostDataModificationHandler(this._defeatMarkUid, base.DataHandlerKey);
        }

        private void OnMarkChanged(DataContext context, DataUid dataUid)
        {
            this._addPower = AddPowerUnit * (base.CombatChar.GetDefeatMarkCollection().OuterInjuryMarkList.Sum() + base.CombatChar.GetDefeatMarkCollection().InnerInjuryMarkList.Sum() + base.CombatChar.GetDefeatMarkCollection().FatalDamageMarkCount);
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
            else
            {
                bool flag3 = dataKey.FieldId == 142 || dataKey.FieldId == 137;
                result = (!flag3 && dataValue);
            }
            return result;
        }

        // Token: 0x0600002B RID: 43 RVA: 0x00003548 File Offset: 0x00001748
        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            bool flag = dataKey.CharId != base.CharacterId || !base.CanAffect;
            int result;
            if (flag)
            {
                result = 0;
            }
            else
            {
                bool flag2 = dataKey.FieldId == 211;
                if (flag2)
                {
                    result = this._addPower;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
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
