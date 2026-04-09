using System;
using System.Runtime.CompilerServices;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Utilities;
using TaiwuBossMod;

namespace TaiwuBossMod_Backend.Passives
{
    public class HeavenlyDemonAttack3 : CombatSkillEffectBase
    {
        // Token: 0x0600002E RID: 46 RVA: 0x000035E7 File Offset: 0x000017E7
        public HeavenlyDemonAttack3()
        {
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00003603 File Offset: 0x00001803
        public HeavenlyDemonAttack3(CombatSkillKey skillKey) : base(skillKey, 69423, -1)
        {
        }

        public override void OnEnable(DataContext context)
        {
            Events.RegisterHandler_CombatStateMachineUpdateEnd(new Events.OnCombatStateMachineUpdateEnd(this.OnStateMachineUpdateEnd));
            Events.RegisterHandler_AttackSkillAttackEnd(new Events.OnAttackSkillAttackEnd(this.OnSkillAttackEnd));
            Events.RegisterHandler_CastSkillEnd(new Events.OnCastSkillEnd(this.OnCastSkillEnd));
        }

        // Token: 0x06004EF5 RID: 20213 RVA: 0x00A46CCF File Offset: 0x00A44ECF
        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CombatStateMachineUpdateEnd(new Events.OnCombatStateMachineUpdateEnd(this.OnStateMachineUpdateEnd));
            Events.UnRegisterHandler_AttackSkillAttackEnd(new Events.OnAttackSkillAttackEnd(this.OnSkillAttackEnd));
            Events.UnRegisterHandler_CastSkillEnd(new Events.OnCastSkillEnd(this.OnCastSkillEnd));
        }

        // Token: 0x06004EF6 RID: 20214 RVA: 0x00A46D08 File Offset: 0x00A44F08
        private void OnStateMachineUpdateEnd(DataContext context, CombatCharacter combatChar)
        {
            bool flag = base.CombatChar != combatChar || DomainManager.Combat.Pause;
            if (!flag)
            {
                this._frameCounter++;
                bool flag2 = this._frameCounter < 60 || !DomainManager.Combat.InAttackRange(base.CombatChar);
                if (!flag2)
                {
                    this._frameCounter = 0;
                    CombatCharacter enemyChar = base.CurrEnemyChar;
                    bool flag3 = enemyChar.GetCharacter().GetXiangshuInfection() < 200;
                    if (flag3)
                    {
                        enemyChar.GetCharacter().ChangeXiangshuInfection(context, 10 + enemyChar.OriginXiangshuInfection * 10 / 100);
                    }
                    FileLogger.Info($"Current Infection: {enemyChar.GetCharacter().GetXiangshuInfection()}");
                    base.ShowSpecialEffectTips(2);
                }
            }
        }

        // Token: 0x06004EF7 RID: 20215 RVA: 0x00A46DB0 File Offset: 0x00A44FB0
        private void OnSkillAttackEnd(DataContext context, CombatCharacter attacker, CombatCharacter defender, short skillId, int index, bool hit)
        {
            bool flag = attacker != base.CombatChar || skillId != base.SkillTemplateId || !base.CombatCharPowerMatchAffectRequire(0) || index != 3;
            if (!flag)
            {
                bool flag2 = defender.GetCharacter().GetXiangshuInfection() >= 200 && !defender.Immortal;
                FileLogger.Info($"defender.Immortal: {defender.Immortal}");
                if (flag2)
                {
                    defender.ForceDefeat = true;
                    base.ShowSpecialEffectTips(3);
                }
            }
        }

        // Token: 0x06004EF8 RID: 20216 RVA: 0x00A46E1C File Offset: 0x00A4501C
        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            bool flag = charId != base.CharacterId || skillId != base.SkillTemplateId;
            if (!flag)
            {
                base.RemoveSelf(context);
            }
        }

        // Token: 0x04004F3C RID: 20284
        private const sbyte AffectFrameCount = 60;

        // Token: 0x04004F3D RID: 20285
        private int _frameCounter;
    }
}
