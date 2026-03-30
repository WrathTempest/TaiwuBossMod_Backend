using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill.Common.Agile;
using GameData.GameDataBridge;
using System;
using System.Runtime.CompilerServices;
using TaiwuBossMod;
using TaiwuBossMod_Backend.Utils;

namespace TaiwuBossMod_Backend.Passives
{
    internal class HeavenlyDemonAgile : AgileSkillBase
    {
        public HeavenlyDemonAgile()
        {
        }

        public HeavenlyDemonAgile(CombatSkillKey skillKey) : base(skillKey, 69421)
        {
            this.ListenCanAffectChange = true;
        }

        // Token: 0x0600001E RID: 30 RVA: 0x00002EA0 File Offset: 0x000010A0
        public override void OnEnable(DataContext context)
        {
            base.OnEnable(context);
            this.AffectDatas = new Dictionary<AffectedDataKey, EDataModifyType>();
            this.AffectDatas = new Dictionary<AffectedDataKey, EDataModifyType>();
            this.AffectDatas.Add(new AffectedDataKey(base.CharacterId, 156, -1, -1, -1, -1), EDataModifyType.Add);
            this.AffectDatas.Add(new AffectedDataKey(base.CharacterId, 157, -1, -1, -1, -1), EDataModifyType.Add);
            this._affecting = false;
            this.OnMoveSkillCanAffectChanged(context, default(DataUid));
            //FileLogger.Info($"Enabled Agile Skill!");
            
        }


        // Token: 0x060050E6 RID: 20710 RVA: 0x00A4FA4E File Offset: 0x00A4DC4E
        public override void OnDisable(DataContext context)
        {
            base.OnDisable(context);
            DomainManager.Combat.DisableJumpMove(context, base.CombatChar, base.SkillTemplateId);
        }
        protected override void OnMoveSkillChanged(DataContext context, DataUid dataUid)
        {
            //FileLogger.Info($"Affecting Move Skill ID: {base.CombatChar.GetAffectingMoveSkillId()}");
            //FileLogger.Info($"Is MoveSkillID == SkillTemplateID? {base.CombatChar.GetAffectingMoveSkillId() == base.SkillTemplateId}");
            base.OnMoveSkillChanged(context, dataUid);
        }

        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            bool flag = dataKey.CharId != base.CharacterId;
            int result;
            if (flag)
            {
                result = 0;
            }
            else
            {
                ushort fieldId = dataKey.FieldId;
                bool flag2 = fieldId - 156 <= 1;
                bool flag3 = flag2;
                if (flag3)
                {
                    result = 10000;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }

        // Token: 0x060050E7 RID: 20711 RVA: 0x00A4FA74 File Offset: 0x00A4DC74
        protected override void OnMoveSkillCanAffectChanged(DataContext context, DataUid dataUid)
        {
            //FileLogger.Info($"In OnMoveSkillCanAffectChanged!");
            bool canAffect = base.CanAffect;
            bool flag = this._affecting == canAffect;
            if (!flag)
            {
                this._affecting = canAffect;
                bool flag2 = canAffect;
                if (flag2)
                {
                    DomainManager.Combat.EnableJumpMove(base.CombatChar, base.SkillTemplateId);
                }
                else
                {
                    DomainManager.Combat.DisableJumpMove(context, base.CombatChar, base.SkillTemplateId);
                }
            }
        }

        // Token: 0x04004FC0 RID: 20416
        private bool _affecting;
    }
}
