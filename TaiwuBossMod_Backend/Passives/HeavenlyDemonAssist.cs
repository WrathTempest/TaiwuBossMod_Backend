using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill.Common.Assist;
using GameData.GameDataBridge;


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
            this._defeatMarkUid = new DataUid(8, 16, (ulong)((long)this.CharacterId), 58U);
            GameDataBridge.AddPostDataModificationHandler(this._defeatMarkUid, base.DataHandlerKey, new Action<DataContext, DataUid>(this.OnDefeatMarkChanged));
            this.AffectDatas = new Dictionary<AffectedDataKey, EDataModifyType>();
            this.AffectDatas.Add(new AffectedDataKey(this.CharacterId, 147, -1, -1, -1, -1), EDataModifyType.Custom);
            this.AffectDatas.Add(new AffectedDataKey(this.CharacterId, 211, -1, -1, -1, -1), EDataModifyType.AddPercent);
            this.AffectDatas.Add(new AffectedDataKey(this.CharacterId, 110, -1, -1, -1, -1), EDataModifyType.AddPercent);
            //this.AffectDatas.Add(new AffectedDataKey(this.CharacterId, 164, -1, -1, -1, -1), EDataModifyType.Custom);
            //this.AffectDatas.Add(new AffectedDataKey(this.CharacterId, 241, -1, -1, -1, -1), EDataModifyType.Custom);
            //this.AffectDatas.Add(new AffectedDataKey(this.CharacterId, 239, -1, -1, -1, -1), EDataModifyType.Custom);
            this.AffectDatas.Add(new AffectedDataKey(this.CharacterId, 142, -1, -1, -1, -1), EDataModifyType.Custom);
        }

        // Token: 0x06000027 RID: 39 RVA: 0x000033BD File Offset: 0x000015BD
        public override void OnDisable(DataContext context)
        {
            GameDataBridge.RemovePostDataModificationHandler(this._defeatMarkUid, base.DataHandlerKey);
        }

        // Token: 0x06000028 RID: 40 RVA: 0x000033D4 File Offset: 0x000015D4
        private void OnDefeatMarkChanged(DataContext context, DataUid dataUid)
        {
            int num = 0;
            foreach (byte b in DomainManager.Combat.GetElement_CombatCharacterDict(this.CharacterId).GetDefeatMarkCollection().OuterInjuryMarkList)
            {
                num += (int)b;
            }
            foreach (byte b2 in DomainManager.Combat.GetElement_CombatCharacterDict(this.CharacterId).GetDefeatMarkCollection().InnerInjuryMarkList)
            {
                num += (int)b2;
            }
            num += (int)DomainManager.Combat.GetElement_CombatCharacterDict(this.CharacterId).GetDefeatMarkCollection().FatalDamageMarkCount;
            this._addPower = AddPowerUnit * num;
            DomainManager.SpecialEffect.InvalidateCache(context, this.CharacterId, 211);
        }

        // Token: 0x06000029 RID: 41 RVA: 0x00003494 File Offset: 0x00001694
        protected override void OnCanUseChanged(DataContext context, DataUid dataUid)
        {
            base.SetConstAffecting(context, base.CanAffect);
            DomainManager.SpecialEffect.InvalidateCache(context, this.CharacterId, 211);
        }

        // Token: 0x0600002A RID: 42 RVA: 0x000034BC File Offset: 0x000016BC
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
                bool flag3 = dataKey.FieldId == 147 || dataKey.FieldId == 142;
                result = (!flag3 && dataValue);
            }
            return result;
        }

        // Token: 0x0600002B RID: 43 RVA: 0x00003548 File Offset: 0x00001748
        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            bool flag = dataKey.CharId != this.CharacterId || !base.CanAffect;
            bool flag2 = flag;
            int result;
            if (flag2)
            {
                result = 0;
            }
            else
            {
                bool flag3 = dataKey.FieldId == 211;
                bool flag4 = flag3;
                if (flag4)
                {
                    result = this._addPower;
                }
                else
                {
                    bool flag5 = dataKey.FieldId == 110;
                    bool flag6 = flag5;
                    if (flag6)
                    {
                        result = -20;
                    }
                    else
                    {
                        result = 0;
                    }
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
