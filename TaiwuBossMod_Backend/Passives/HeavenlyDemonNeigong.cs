using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect;
using GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Neigong.Boss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaiwuBossMod_Backend.Passives
{
    internal class HeavenlyDemonNeigong : BossNeigongBase
    {
        // Token: 0x06000036 RID: 54 RVA: 0x00003A15 File Offset: 0x00001C15
        public HeavenlyDemonNeigong()
        {
        }

        // Token: 0x06000037 RID: 55 RVA: 0x00003A1F File Offset: 0x00001C1F
        public HeavenlyDemonNeigong(CombatSkillKey skillKey) : base(skillKey, 16107)
        {
        }

        protected override void ActivePhase2Effect(DataContext context)
        {
            base.AppendAffectedData(context, this.CharacterId, 75, EDataModifyType.TotalPercent, -1);
            base.AppendAffectedData(context, this.CharacterId, 110, EDataModifyType.TotalPercent, -1);
            base.CombatChar.OuterInjuryAutoHealSpeeds.Add(1);
            base.CombatChar.InnerInjuryAutoHealSpeeds.Add(1);
            base.AppendAffectedAllEnemyData(context, 212, EDataModifyType.Custom, -1);
            base.AppendAffectedAllEnemyData(context, 213, EDataModifyType.Custom, -1);
            NeiliAllocation originNeiliAllocation = base.CombatChar.GetOriginNeiliAllocation();
            for (byte b = 0; b < 4; b += 1)
            {
                base.CombatChar.ChangeNeiliAllocation(context, b, (int)((double)(originNeiliAllocation.GetTotal() / 4) * 1.0), false);
            }
            Events.RegisterHandler_NormalAttackEnd(new Events.OnNormalAttackEnd(this.OnNormalAttackEnd));
            Events.RegisterHandler_CombatStateMachineUpdateEnd(new Events.OnCombatStateMachineUpdateEnd(this.OnStateMachineUpdateEnd));
        }
        public override void OnDisable(DataContext context)
        {
            base.OnDisable(context);
            Events.UnRegisterHandler_NormalAttackEnd(new Events.OnNormalAttackEnd(this.OnNormalAttackEnd));
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(new Events.OnCombatStateMachineUpdateEnd(this.OnStateMachineUpdateEnd));
        }
        // Token: 0x06000039 RID: 57 RVA: 0x00003AE8 File Offset: 0x00001CE8
        public override bool GetModifiedValue(AffectedDataKey dataKey, bool dataValue)
        {
            return !DomainManager.Combat.InAttackRange(base.CombatChar);
        }
        private void OnStateMachineUpdateEnd(DataContext context, CombatCharacter combatChar)
        {
            bool flag = base.CombatChar != combatChar || DomainManager.Combat.Pause;
            if (!flag)
            {
                this._frameCounter++;
                bool flag2 = this._frameCounter < (int)this.AddNeiliAllocationFrame;
                if (!flag2)
                {
                    this._frameCounter = 0;
                    for (byte type = 0; type < 4; type += 1)
                    {
                        base.CombatChar.ChangeNeiliAllocation(context, type, 1, true, true);
                    }
                }
            }
        }

        //Normal attacks can interrupt
        private void OnNormalAttackEnd(DataContext context, CombatCharacter attacker, CombatCharacter defender, sbyte trickType, int pursueIndex, bool hit, bool isFightBack)
        {
            bool flag = !hit || pursueIndex != 0 || attacker != base.CombatChar || base.CurrEnemyChar.GetPreparingSkillId() < 0;
            if (!flag)
            {
                bool flag2 = DomainManager.Combat.InterruptSkill(context, base.CurrEnemyChar, 100);
                if (flag2)
                {
                    base.CurrEnemyChar.SetAnimationToPlayOnce(DomainManager.Combat.GetHittedAni(base.CurrEnemyChar, 2), context);
                    DomainManager.Combat.SetProperLoopAniAndParticle(context, base.CurrEnemyChar, false);
                    base.ShowSpecialEffectTips(1);
                }
            }
        }

        // Token: 0x0600003A RID: 58 RVA: 0x00003B10 File Offset: 0x00001D10
        public override int GetModifyValue(AffectedDataKey dataKey, int currModifyValue)
        {
            bool flag = dataKey.CharId != this.CharacterId;
            bool flag2 = flag;
            int result;
            if (flag2)
            {
                result = 0;
            }
            else
            {
                result = ((dataKey.FieldId == 75) ? AddDamage : ReduceDamage);
            }
            return result;
        }

        // Token: 0x0400001B RID: 27
        private const int AddDamage = 150;

        // Token: 0x0400001C RID: 28
        private const int ReduceDamage = -60;

        private sbyte AddNeiliAllocationFrame = 60;

        // Token: 0x04004EF3 RID: 20211
        private int _frameCounter;
    }
}
