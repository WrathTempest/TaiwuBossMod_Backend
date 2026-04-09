using System;
using System.Runtime.CompilerServices;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;
using GameData.Utilities;

namespace TaiwuBossMod_Backend.Passives
{
    public class HeavenlyDemonAttack : CombatSkillEffectBase
    {
        // Token: 0x0600002E RID: 46 RVA: 0x000035E7 File Offset: 0x000017E7
        public HeavenlyDemonAttack()
        {
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00003603 File Offset: 0x00001803
        public HeavenlyDemonAttack(CombatSkillKey skillKey) : base(skillKey, 69422, -1)
        {
        }

        // Token: 0x06000030 RID: 48 RVA: 0x00003626 File Offset: 0x00001826
        public override void OnEnable(DataContext context)
        {

            this._registeredStateMachineUpdateEnd = false;
            Events.RegisterHandler_PrepareSkillEnd(new Events.OnPrepareSkillEnd(this.OnPrepareSkillEnd));
            Events.RegisterHandler_CastSkillEnd(new Events.OnCastSkillEnd(this.OnCastSkillEnd));
            Events.RegisterHandler_SkillEffectChange(new Events.OnSkillEffectChange(this.OnSkillEffectChange));
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00003668 File Offset: 0x00001868
        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_PrepareSkillEnd(new Events.OnPrepareSkillEnd(this.OnPrepareSkillEnd));
            Events.UnRegisterHandler_CastSkillEnd(new Events.OnCastSkillEnd(this.OnCastSkillEnd));
            Events.UnRegisterHandler_SkillEffectChange(new Events.OnSkillEffectChange(this.OnSkillEffectChange));
            bool registeredStateMachineUpdateEnd = this._registeredStateMachineUpdateEnd;
            bool flag = registeredStateMachineUpdateEnd;
            if (flag)
            {
                Events.UnRegisterHandler_CombatStateMachineUpdateEnd(new Events.OnCombatStateMachineUpdateEnd(this.OnStateMachineUpdateEnd));
            }
        }

        // Token: 0x06000032 RID: 50 RVA: 0x000036CC File Offset: 0x000018CC
        private void OnPrepareSkillEnd(DataContext context, int charId, bool isAlly, short skillId)
        {
            bool flag = charId != this.CharacterId || skillId != base.SkillTemplateId;
            bool flag2 = !flag;
            if (flag2)
            {
                CombatCharacter combatCharacter = DomainManager.Combat.GetCombatCharacter(!base.CombatChar.IsAlly, false);
                DomainManager.Combat.ChangeAttackPrepareValue(context, combatCharacter, (int)(-(int)combatCharacter.GetMaxAttackPrepareValue()));
                combatCharacter.SetNormalAttackCd(new IntPair((int)this.AffectFrameCount2, (int)this.AffectFrameCount2), context);
                DomainManager.Combat.ShowSpecialEffectTips(this.CharacterId, base.EffectId, 0);
            }
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00003760 File Offset: 0x00001960
        private void OnCastSkillEnd(DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)
        {
            bool flag = charId != this.CharacterId || skillId != base.SkillTemplateId;
            bool flag2 = !flag;
            if (flag2)
            {
                bool flag3 = !this.IsSrcSkillPerformed;
                bool flag4 = flag3;
                if (flag4)
                {
                    bool flag5 = !interrupted;
                    bool flag6 = flag5;
                    if (flag6)
                    {
                        this.IsSrcSkillPerformed = true;
                        this._frameCounter = 0;
                        this._registeredStateMachineUpdateEnd = true;
                        DomainManager.Combat.AddSkillEffect(context, base.CombatChar, new SkillEffectKey(base.SkillTemplateId, this.IsDirect), base.MaxEffectCount, base.MaxEffectCount, true);
                        Events.RegisterHandler_CombatStateMachineUpdateEnd(new Events.OnCombatStateMachineUpdateEnd(this.OnStateMachineUpdateEnd));
                    }
                    else
                    {
                        base.RemoveSelf(context);
                    }
                }
                else
                {
                    bool flag7 = !interrupted;
                    bool flag8 = flag7;
                    if (flag8)
                    {
                        base.RemoveSelf(context);
                    }
                }
            }
        }

        // Token: 0x06000034 RID: 52 RVA: 0x00003838 File Offset: 0x00001A38
        private void OnSkillEffectChange(DataContext context, int charId, SkillEffectKey key, short oldCount, short newCount, bool removed)
        {
            bool flag = removed && this.IsSrcSkillPerformed && charId == this.CharacterId && key.SkillId == base.SkillTemplateId && key.IsDirect == this.IsDirect;
            bool flag2 = flag;
            if (flag2)
            {
                base.RemoveSelf(context);
            }
        }

        // Token: 0x06000035 RID: 53 RVA: 0x0000388C File Offset: 0x00001A8C
        private void OnStateMachineUpdateEnd(DataContext context, CombatCharacter combatChar)
        {
            bool flag = base.CombatChar != combatChar || base.CombatChar.StateMachine.GetCurrentState().IsUpdateOnPause;
            bool flag2 = !flag;
            if (flag2)
            {
                this._frameCounter++;
                bool flag3 = this._frameCounter < 300;
                bool flag4 = !flag3;
                if (flag4)
                {
                    this._frameCounter = 0;
                    CombatCharacter combatCharacter = DomainManager.Combat.GetCombatCharacter(!base.CombatChar.IsAlly, false);
                    for (int i = 0; i < (int)this.AcupointCount; i++)
                    {
                        DomainManager.Combat.AddAcupoint(context, combatCharacter, 2, this.SkillKey, -1, 1, true);
                    }
                    DomainManager.Combat.ChangeBreathValue(context, combatCharacter, -3900);
                    DomainManager.Combat.ChangeStanceValue(context, combatCharacter, -600, false);
                    DomainManager.Combat.ChangeMobilityValue(context, combatCharacter, -200, true, base.CombatChar);
                    bool flag5 = combatCharacter.GetAffectingMoveSkillId() >= 0;
                    bool flag6 = flag5;
                    if (flag6)
                    {
                        DomainManager.Combat.ChangeMobilityValue(context, combatCharacter, (int)(-GlobalConfig.Instance.AgileSkillNonJumpDirectionCostMobilityPercent * 20 / 100), true, base.CombatChar);
                    }
                    DomainManager.Combat.ShowSpecialEffectTips(this.CharacterId, base.EffectId, 0);
                    DomainManager.Combat.AddToCheckFallenSet(combatCharacter.GetId());
                    DomainManager.Combat.ShowSpecialEffectTips(this.CharacterId, base.EffectId, 0);
                    base.ReduceEffectCount(1);
                }
            }
        }

        // Token: 0x04000015 RID: 21
        private const short AffectFrameCount = 300;

        // Token: 0x04000016 RID: 22
        private const sbyte AcupointLevel = 2;

        // Token: 0x04000017 RID: 23
        private sbyte AcupointCount = 2;

        // Token: 0x04000018 RID: 24
        private int _frameCounter;

        // Token: 0x04000019 RID: 25
        private bool _registeredStateMachineUpdateEnd;

        // Token: 0x0400001A RID: 26
        protected short AffectFrameCount2 = 600;
    }
}
