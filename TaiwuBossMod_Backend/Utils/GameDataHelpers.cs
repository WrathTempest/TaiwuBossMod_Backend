using Config;
using Config.Common;
using Config.ConfigCells.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaiwuBossMod_Backend.Utils
{
    internal class GameDataHelpers
    {
        public static T Get<T>() where T : class, IConfigData
        {
            foreach (var item in ConfigCollection.Items)
            {
                if (item is T t)
                    return t;
            }
            return null;
        }
    }
}
