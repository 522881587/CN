///*
//    Copyright (C) 2014 h3h3

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//*/

//using System.Collections.Generic;
//using System.Linq;
//using LeagueSharp;
//using LeagueSharp.Common;

//namespace Killability
//{
//    internal class Killability
//    {
//        #region Static
//        private readonly static Menu Config;

//        static Killability()
//        {
//            Config = new Menu("Killability", "Killability", true);
//            Config.AddItem(new MenuItem("icon", "Show Icon").SetValue(true));
//            Config.AddItem(new MenuItem("text", "Show Text").SetValue(true));
//            Config.AddToMainMenu();
//        }

//        public static List<Items.Item> Items = new List<Items.Item>
//        {
//            new Items.Item(3128, 750), // Deathfire Grasp
//            new Items.Item(3077, 400), // Tiamat
//            new Items.Item(3074, 400), // Ravenous Hydra
//            new Items.Item(3146, 700), // Hextech Gunblade
//            new Items.Item(3153, 450)  // Blade of the Ruined King
//        };
//        public static bool Icon { get { return Config.Item("icon").GetValue<bool>(); } }
//        public static bool Text { get { return Config.Item("text").GetValue<bool>(); } }
//        #endregion

//        private readonly List<EnemyHero> _enemys = new List<EnemyHero>(); 

//        public Killability()
//        {
//            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
//            {
//                _enemys.Add(new EnemyHero(hero));
//            }
//        }
//    }
//}
