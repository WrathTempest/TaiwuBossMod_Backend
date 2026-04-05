using Config;
using GameData.Utilities.Mod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuBossMod;
using TaiwuBossMod_Backend.Utils;

namespace TaiwuBossMod_Backend
{
    public static class ModConfigData
    {
        public static Dictionary<string, object> ModDict = new Dictionary<string, object>();

        public static void Initialize()
        {
            ModDict = DataFileHandler.LoadAllJsons();
        }

        public static List<T> GetModConfigData<T>(string type)
        {
            List<T> list = new List<T>();
            if (ModDict.ContainsKey(type))
            {
                list = DataFileHandler.ConvertToTypedList<T>(ModDict[type]);
            }
            
            return list;
        }
        public static List<short> GetTemplateIds<T>(List<T> Config)
        {
            List<short> modTemplates = new List<short>();
            foreach (T item in Config)
            {
                short id = Helpers.GetPrivateField<short>(item, "TemplateId");
                modTemplates.Add(id);
            }
            return modTemplates;
        }

        public static List<short> GetTemplateIds<T>(string type)
        {
            List<short> modTemplates = new List<short>();
            List<T> list = new List<T>();
            if (ModDict.ContainsKey(type))
            {
                list = DataFileHandler.ConvertToTypedList<T>(ModDict[type]);
                modTemplates = GetTemplateIds<T>(list);
            }
            return modTemplates;
        }

    }

}
