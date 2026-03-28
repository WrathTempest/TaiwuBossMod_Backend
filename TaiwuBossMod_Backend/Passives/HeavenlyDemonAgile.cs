using System;
using System.Runtime.CompilerServices;
using GameData.Common;
using GameData.DomainEvents;
using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.CombatSkill;
using GameData.Domains.SpecialEffect.CombatSkill;

namespace TaiwuBossMod_Backend.Passives
{
    internal class HeavenlyDemonAgile : CombatSkillEffectBase
    {
        public HeavenlyDemonAgile()
        {
        }

        public HeavenlyDemonAgile(CombatSkillKey skillKey) : base(skillKey, 69421, -1)
        {
        }

        // Token: 0x0600001E RID: 30 RVA: 0x00002EA0 File Offset: 0x000010A0
        public override void OnEnable(DataContext context)
        {
            Events.RegisterHandler_CastSkillEnd(new Events.OnCastSkillEnd(this.OnCastSkillEnd));
            Events.RegisterHandler_DistanceChanged(new Events.OnDistanceChanged(this.OnDistanceChanged));
            Events.RegisterHandler_SkillEffectChange(new Events.OnSkillEffectChange(this.OnSkillEffectChange));
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00002ED9 File Offset: 0x000010D9
        public override void OnDisable(DataContext context)
        {
            Events.UnRegisterHandler_CastSkillEnd(new Events.OnCastSkillEnd(this.OnCastSkillEnd));
            Events.UnRegisterHandler_DistanceChanged(new Events.OnDistanceChanged(this.OnDistanceChanged));
            Events.UnRegisterHandler_SkillEffectChange(new Events.OnSkillEffectChange(this.OnSkillEffectChange));
        }

        // Token: 0x06000020 RID: 32 RVA: 0x00002F14 File Offset: 0x00001114
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
                    this.IsSrcSkillPerformed = true;
                    int[] characterList = DomainManager.Combat.GetCharacterList(!base.CombatChar.IsAlly);
                    for (int i = 0; i < characterList.Length; i++)
                    {
                        bool flag5 = characterList[i] < 0;
                        bool flag6 = !flag5;
                        if (flag6)
                        {
                            CombatCharacter element_CombatCharacterDict = DomainManager.Combat.GetElement_CombatCharacterDict(characterList[i]);
                            for (int j = 0; j < 3; j++)
                            {
                                DomainManager.Combat.AddWeaponExtraCd(context, element_CombatCharacterDict, j, 30000);
                            }
                        }
                    }
                    DomainManager.Combat.UpdateWeaponCanChange(context, DomainManager.Combat.GetCombatCharacter(!base.CombatChar.IsAlly, false));
                    DomainManager.Combat.AddSkillEffect(context, base.CombatChar, new SkillEffectKey(base.SkillTemplateId, this.IsDirect), base.MaxEffectCount, base.MaxEffectCount, true);
                }
                else
                {
                    base.RemoveSelf(context);
                }
            }
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00003050 File Offset: 0x00001250
        private void OnDistanceChanged(DataContext context, CombatCharacter mover, short distance, bool isMove, bool isForced)
        {
            bool flag = !this.IsSrcSkillPerformed || mover.IsAlly == base.CombatChar.IsAlly || !isMove || isForced;
            bool flag2 = !flag;
            if (flag2)
            {
                this._movedDistance += (int)Math.Abs(distance);
                bool flag3 = this._movedDistance >= RequireMoveDistance;
                bool flag4 = flag3;
                if (flag4)
                {
                    this._movedDistance -= RequireMoveDistance;
                    CombatCharacter combatCharacter = DomainManager.Combat.GetCombatCharacter(!base.CombatChar.IsAlly, false);
                    DomainManager.Combat.ChangeBreathValue(context, combatCharacter, -19500);
                    DomainManager.Combat.ChangeStanceValue(context, combatCharacter, -3000, false);
                    DomainManager.Combat.ChangeMobilityValue(context, combatCharacter, -1000, true, base.CombatChar);
                    DomainManager.Combat.ClearAffectingAgileSkill(context, combatCharacter);
                    DomainManager.Combat.ShowSpecialEffectTips(this.CharacterId, base.EffectId, 0);
                    base.ReduceEffectCount(1);
                }
            }
        }

        // Token: 0x06000022 RID: 34 RVA: 0x00003158 File Offset: 0x00001358
        private void OnSkillEffectChange(DataContext context, int charId, SkillEffectKey key, short oldCount, short newCount, bool removed)
        {
            bool flag = removed && this.IsSrcSkillPerformed && charId == this.CharacterId && key.SkillId == base.SkillTemplateId && key.IsDirect == this.IsDirect;
            bool flag2 = flag;
            if (flag2)
            {
                int[] characterList = DomainManager.Combat.GetCharacterList(!base.CombatChar.IsAlly);
                for (int i = 0; i < characterList.Length; i++)
                {
                    bool flag3 = characterList[i] < 0;
                    bool flag4 = !flag3;
                    if (flag4)
                    {
                        CombatCharacter element_CombatCharacterDict = DomainManager.Combat.GetElement_CombatCharacterDict(characterList[i]);
                        for (int j = 0; j < 3; j++)
                        {
                            DomainManager.Combat.ReduceWeaponExtraCd(context, element_CombatCharacterDict, j, 30000);
                        }
                    }
                }
                DomainManager.Combat.UpdateWeaponCanChange(context, DomainManager.Combat.GetCombatCharacter(!base.CombatChar.IsAlly, false));
                base.RemoveSelf(context);
            }
        }

        // Token: 0x0400000D RID: 13
        private const sbyte RequireMoveDistance = 20;

        // Token: 0x0400000E RID: 14
        private int _movedDistance;

        // Token: 0x0400000F RID: 15
        private int _frameCounter;

        // Token: 0x04000010 RID: 16
        private short AffectFrameCount = 300;
    }
}
