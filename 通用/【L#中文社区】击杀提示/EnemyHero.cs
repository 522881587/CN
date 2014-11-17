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

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Killability.Properties;
//using LeagueSharp;
//using LeagueSharp.Common;
//using SharpDX;

//namespace Killability
//{
//    internal class RotationEntry
//    {
//        public SpellSlot Spell { get; set; }
//        public DamageLib.SpellType Type { get; set; }
//        public float Delay { get; set; }
//        public float Next { get; set; }

//        public RotationEntry(SpellSlot spell, DamageLib.SpellType type, float delay)
//        {
//            Spell = spell;
//            Type = type;
//            Delay = delay;
//        }
//    }

//    internal class EnemyHero
//    {
//        public Obj_AI_Hero Hero { get; set; }
//        public float Health { get { return Hero.Health; } }
//        public float Mana { get { return Hero.Mana; } }
//        public List<Items.Item> Items { get; set; }
//        public SpellDataInst Ignite { get; set; }

//        public List<RotationEntry> Rotation { get; set; }
//        public bool IsKillable { get; set; }
//        public double Damage { get; set; }
//        public float Time { get; set; }
//        public string Text { get; set; }

//        public EnemyHero(Obj_AI_Hero hero)
//        {
//            Hero = hero;
//            Items = new List<Items.Item>();
//            Rotation = new List<RotationEntry>();
//            Ignite = ObjectManager.Player.SummonerSpellbook.Spells.FirstOrDefault(x => x.Name.ToLower() == "summonerdot");

//            Rotation.Add(new RotationEntry(SpellSlot.Unknown, DamageLib.SpellType.AD, 100));

//            try
//            {
//                DamageLib.getDmg(ObjectManager.Player, DamageLib.SpellType.Q);
//                Rotation.Add(new RotationEntry(SpellSlot.Q, DamageLib.SpellType.Q, 150));
//            }
//            catch { }

//            try
//            {
//                DamageLib.getDmg(ObjectManager.Player, DamageLib.SpellType.W);
//                Rotation.Add(new RotationEntry(SpellSlot.W, DamageLib.SpellType.W, 150));
//            }
//            catch { }

//            try
//            {
//                DamageLib.getDmg(ObjectManager.Player, DamageLib.SpellType.E);
//                Rotation.Add(new RotationEntry(SpellSlot.E, DamageLib.SpellType.E, 150));
//            }
//            catch { }

//            try
//            {
//                DamageLib.getDmg(ObjectManager.Player, DamageLib.SpellType.R);
//                Rotation.Add(new RotationEntry(SpellSlot.R, DamageLib.SpellType.R, 150));
//            }
//            catch { }

//            // Observe Items
//            Game.OnGameNotifyEvent += Game_OnGameNotifyEvent;

//#if DEBUG
//            Console.WriteLine("Create: " + Hero.Name);

//            foreach (var item in Items)
//                Console.WriteLine("\t" + item.Id);

//            foreach (var item in Rotation)
//                Console.WriteLine("\t" + item.Type);

//            if (Ignite != null)
//                Console.WriteLine("\tIgnite");
//#endif

//            InitDrawing();
//            Task.Factory.StartNew(CalcTask);
//        }

//        private void CalcTask()
//        {
//            while (true)
//            {
//                var time = 0f;
//                var mana = 0f;
//                var dmg = 0d;
//                var target = ObjectManager.Player;
//                var combo = new List<DamageLib.SpellType>();

//                foreach (var item in Items.Where(item => item.IsReady()))
//                {
//                    switch (item.Id)
//                    {
//                        case 3128: combo.Add(DamageLib.SpellType.DFG); break;
//                        case 3077: combo.Add(DamageLib.SpellType.TIAMAT); break;
//                        case 3074: combo.Add(DamageLib.SpellType.HYDRA); break;
//                        case 3146: combo.Add(DamageLib.SpellType.HEXGUN); break;
//                        case 3144: combo.Add(DamageLib.SpellType.BILGEWATER); break;
//                        case 3153: combo.Add(DamageLib.SpellType.BOTRK); break;
//                    }

//                    time += 100;

//                    if (DamageLib.IsKillable(target, combo))
//                        break;
//                }

//                if (Ignite != null && Ignite.State == SpellState.Ready && !DamageLib.IsKillable(target, combo))
//                {
//                    combo.Add(DamageLib.SpellType.IGNITE);
//                    time += 100;
//                }

//                while (target.Health > dmg)
//                {
//                    foreach (var r in Rotation)
//                    {
//                        if (target.Health > dmg)
//                            break;

//                        if (time > r.Next)
//                        {
//                            if (r.Spell != SpellSlot.Unknown) // Spell
//                            {
//                                var cost = Hero.Spellbook.GetSpell(r.Spell).ManaCost;

//#if DEBUG
//                                Console.WriteLine("Test: {0} {1}", Hero.Name, r.Type);
//                                Console.WriteLine("Mana: {0} {1} {2}", Hero.Name, cost, Mana);
//#endif

//                                if (Hero.Spellbook.GetSpell(r.Spell).Level > 0 && (cost < 1 || Mana < 1 || Mana > mana + cost))
//                                {
//                                    mana += Hero.Spellbook.GetSpell(r.Spell).ManaCost;
//                                    r.Next = time + Hero.Spellbook.GetSpell(r.Spell).Cooldown * 1000;

//                                    combo.Add(r.Type);
//                                    time += r.Delay;
//                                    dmg += DamageLib.GetComboDamage(target, combo);

//#if DEBUG
//                                    Console.WriteLine("Cast: {0} {1}", Hero.Name, r.Type);
//                                    Console.WriteLine("Dmg:  {0} {1} {2}", Hero.Name, DamageLib.GetComboDamage(target, combo), dmg);
//                                    Console.WriteLine("Next: {0} {1}", Hero.Name, r.Next);
//#endif
//                                }
//                            }
//                            else // AA
//                            {
//                                r.Next = time + Hero.AttackDelay * 1000;

//                                combo.Add(r.Type);
//                                time += r.Delay;
//                                dmg += DamageLib.GetComboDamage(target, combo);

//#if DEBUG
//                                Console.WriteLine("Cast: {0} {1}", Hero.Name, r.Type);
//                                Console.WriteLine("Dmg:  {0} {1} {2}", Hero.Name, DamageLib.GetComboDamage(target, combo), dmg);
//                                Console.WriteLine("Next: {0} {1}", Hero.Name, r.Next);
//#endif
//                            }
//                        }

//                        time += 100;
//                    }
//                }

//                Damage = DamageLib.GetComboDamage(target, combo);
//                IsKillable = Damage > target.Health;
//                Time = time;
//#if DEBUG
//                Text = Math.Round(Damage) + " - " + Time + " - " + string.Join("/", combo);
//#endif

//                Thread.Sleep(5000);
//            }
//        }

//        private void Game_OnGameNotifyEvent(GameNotifyEventArgs args)
//        {
//            if (args.NetworkId != Hero.NetworkId)
//                return;

//            switch (args.EventId)
//            {
//                case GameEventId.OnItemPurchased:
//                    foreach (var item in Killability.Items.Where(item => LeagueSharp.Common.Items.HasItem(item.Id, Hero) && !Items.Contains(item)))
//                        Items.Add(item);
//                    break;

//                case GameEventId.OnItemRemoved:
//                    foreach (var item in Killability.Items.Where(item => !LeagueSharp.Common.Items.HasItem(item.Id, Hero) && Items.Contains(item)))
//                        Items.Remove(item);
//                    break;
//            }
//        }

//        private void InitDrawing()
//        {
//            var sprite = new Render.Sprite(Resources.Skull, Hero.HPBarPosition);
//            sprite.PositionUpdate += () => new Vector2(Hero.HPBarPosition.X + 140, Hero.HPBarPosition.Y + 10);
//            sprite.VisibleCondition += s => IsOnScreen() && IsKillable && Killability.Icon;
//            sprite.Scale = new Vector2(0.08f, 0.08f);
//            sprite.Add();

//            var text = new Render.Text("", Hero, new Vector2(20, 50), 18, new ColorBGRA(255, 255, 255, 255));
//            //text.VisibleCondition += s => IsOnScreen() && Killability.Text;
//            text.TextUpdate += () => Text;
//            text.OutLined = true;
//            text.Add();
//        }

//        private bool IsOnScreen()
//        {
//            return Render.OnScreen(Drawing.WorldToScreen(Hero.Position));
//        }
//    }
//}
