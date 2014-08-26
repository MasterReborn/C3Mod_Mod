using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace C3Mod.GameTypes
{
    internal class ApocalypseMonsters
    {
        public static List<int> Monsters = new List<int>();

        public static void AddNPCs()
        {
            Monsters.Add(26);  //Goblin Peon
            Monsters.Add(27);  //Goblin Thief
            Monsters.Add(28);  //Goblin Warrior
        }
    }
}
