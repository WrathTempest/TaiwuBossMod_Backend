using Config;
using GameData.Utilities.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuBossMod;

namespace TaiwuBossMod_Backend
{
    public class CustomEffect
    {
        public string ClassName;
        public Dictionary<short, Type> Effects;

        public CustomEffect(string className)
        {
            this.ClassName = className;
            this.Effects = new Dictionary<short, Type>();
        }

        public CustomEffect(string className, List<Type> effects)
        {
            this.ClassName = className;
            this.Effects = new Dictionary<short, Type>();
            foreach (Type effect in effects)
            {
                AddEffect(effect);
            }       
        }

        public void AddEffect(Type className)
        {
            if (className == null) return;
            string name = className.Name;
            //FileLogger.Info($"CustomEffect Name: {name}");
            short templateId = -1;
            foreach (SpecialEffectItem effect in Config.SpecialEffect.Instance)
            {
                if (effect.ClassName == null) continue;
                if (name.Contains(effect.ClassName) || effect.ClassName.Contains(name))
                {
                    templateId = effect.TemplateId;
                    break;
                }
            }
            if (templateId != -1)
            {
                this.Effects.Add(templateId, className);
            }
            
        }

    }

}
