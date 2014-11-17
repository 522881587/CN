using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;
using Color = System.Drawing.Color;

namespace TheHatinEvelynn
{
    internal class Program
    {
        public static string ChampionName = "Evelynn";
        public static Orbwalking.Orbwalker Orbwalker;
        public static string WelcomeMsg = ("<font color = '#6600cc'>TheHatin'瀵″鈥愁洭 </font><font color='#FFFFFF'>L#涓枃绀惧尯.</font> <font color = '#ff0000'>  www.loll35.com </font> ");
        private static Obj_AI_Hero Player;
        // Spells
        #region
        public static List<Spell> SpellList = new List<Spell>();
        public static SpellSlot IgniteSlot;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        #endregion

        // Items
        #region
        public static Items.Item DFG;
        public static Items.Item BotRK;
        public static Items.Item HexGunBlade;
        public static Items.Item QuickS;
        public static Items.Item Cutlass;
        public static Items.Item Scimitar;
        //public static Items.Item Omen;
        //public static Items.Item Zhonya;

        #endregion

        //Menu
        public static Menu Menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;
            Game.PrintChat(WelcomeMsg);

            //Create the spells
            #region
            Q = new Spell(SpellSlot.Q, 500f);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 225f);
            R = new Spell(SpellSlot.R, 650f);
            R.SetSkillshot(0.25f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            #endregion

            //Create the items
            #region
            DFG = new Items.Item(3128, 750f);
            BotRK = new Items.Item(3153, 450f);
            HexGunBlade = new Items.Item(3146, 700f);
            QuickS = new Items.Item(3140, 0f);
            Cutlass = new Items.Item(3144, 450f);
            Scimitar = new Items.Item(3139, 0f);
            #endregion

            //Create the menu
            #region
            Menu = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("|鐩爣閫夋嫨|", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);

            Menu.AddSubMenu(new Menu("|璧扮爫|", "Orbwalker Menu"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker Menu"));
            #endregion

            //Add Combo SubMenu
            #region
            Menu.AddSubMenu(new Menu("|杩炴嫑|", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "浣跨敤 Q").SetValue(true));
            //Menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "浣跨敤 W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "浣跨敤 E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "浣跨敤 R").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseItemsCombo", "|浣跨敤閫夐」|").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseIgniteCombo", "浣跨敤鐐圭噧").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "杩炴嫑").SetValue(new KeyBind(32, KeyBindType.Press)));
            #endregion

            //Add LaneClear SubMenu
            #region
            Menu.AddSubMenu(new Menu("|娓呯嚎|", "LaneClear"));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "浣跨敤 Q").SetValue(true));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "浣跨敤 E").SetValue(false));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "娓呯嚎").SetValue(new KeyBind("V".ToArray()[0], KeyBindType.Press)));
            #endregion

            //Add JungleFarm SubMenu
            #region
            Menu.AddSubMenu(new Menu("|娓呴噹|", "JungleFarm"));
            Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJungleFarm", "浣跨敤 Q").SetValue(true));
            Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJungleFarm", "浣跨敤 E").SetValue(true));
            Menu.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "娓呴噹").SetValue(new KeyBind("V".ToArray()[0], KeyBindType.Press)));
            #endregion

            //Add Items SubMenu
            #region
            Menu.AddSubMenu(new Menu("|鑷姩鐗╁搧閫夐」榛樿鍗冲彲|", "Items"));
            Menu.SubMenu("Items").AddItem(new MenuItem("UseDFGItems", "Use DFG").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("UseBotRKItems", "Use BotRK").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("UseHexGunBladeItems", "Use Hextech Gunblade").SetValue(true));
            //Menu.SubMenu("Items").AddItem(new MenuItem("UseQuickSItems", "Use Quicksilver Sash").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("UseCutlassItems", "Use Bilgewater cutlass").SetValue(true));
            #endregion

            //Add Drawing SubMenu
            #region
            Menu.AddSubMenu(new Menu("鎶€鑳借寖鍥撮€夐」", "Drawings"));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Q鑼冨洿").SetValue(new Circle(true, Color.FromArgb(255, 0, 255, 0))));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "E鑼冨洿").SetValue(new Circle(true, Color.FromArgb(255, 0, 255, 0))));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "R鑼冨洿").SetValue(new Circle(true, Color.FromArgb(255, 0, 255, 0))));
            #endregion

            //Add Misc SubMenu
            #region
            Menu.AddSubMenu(new Menu("|鏉傞」|", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("UsePackets", "|浣跨敤灏佸寘|").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("SmartW", "|鏅鸿兘W|").SetValue(true));
            Menu.SubMenu("Misc").AddSubMenu(new Menu("Smart Quicksilver Sash", "SQS"));
            Menu.SubMenu("Misc").SubMenu("SQS").AddItem(new MenuItem("ActiveQSS", "Active").SetValue(true));
            Menu.SubMenu("Misc").SubMenu("SQS").AddItem(new MenuItem("Quick%Poison", "On % HP when poisoned").SetValue(new Slider(10, 1, 100)));
            //Menu.SubMenu("SmartQuickS1").AddItem(new MenuItem("Quick%Poison", "On % HP when poisoned").SetValue(10));
            #endregion
            Menu.AddItem(new MenuItem("by Da'ath.", "by Da'ath"));
			Menu.AddSubMenu(new Menu("L#涓枃绀惧尯", "AD"));
				Menu.SubMenu("AD").AddItem(new MenuItem("wangzhan", "www.loll35.com"));
				Menu.SubMenu("AD").AddItem(new MenuItem("qunhao", "姹夊寲缇わ細397983217"));
            //Make visable
            Menu.AddToMainMenu();

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;


        }
        //Drawings
        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItemQ = Menu.Item("Draw" + Q.Slot).GetValue<Circle>();
            if (menuItemQ.Active && Q.IsReady())
                Utility.DrawCircle(Player.Position, Q.Range, menuItemQ.Color);
            else if (menuItemQ.Active)
                Utility.DrawCircle(Player.Position, Q.Range, Color.DarkRed);

            var menuItemE = Menu.Item("Draw" + E.Slot).GetValue<Circle>();
            if (menuItemE.Active && E.IsReady())
                Utility.DrawCircle(Player.Position, E.Range, menuItemE.Color);
            else if (menuItemE.Active)
                Utility.DrawCircle(Player.Position, E.Range, Color.DarkRed);

            var menuItemR = Menu.Item("Draw" + R.Slot).GetValue<Circle>();
            if (menuItemR.Active && R.IsReady())
                Utility.DrawCircle(Player.Position, R.Range, menuItemR.Color);
            else if (menuItemR.Active)
                Utility.DrawCircle(Player.Position, R.Range, Color.DarkRed);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            Orbwalker.SetMovement(true);

            if (Menu.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();

            if (Menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                LaneClear();
            if (Menu.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                JungleFarm();

            if (Menu.Item("ActiveQSS").GetValue<bool>() && QuickS.IsReady() || Scimitar.IsReady())
                SmartQuickS();
            if (ObjectManager.Player.HasBuffOfType(BuffType.Slow) && Menu.Item("SmartW").GetValue<bool>() && W.IsReady())
                W.Cast();

        }


        private static void Combo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            if (target != null)
            {

                if (ObjectManager.Player.Distance(target) <= Cutlass.Range && Cutlass.IsReady() && Menu.Item("UseCutlassItems").GetValue<bool>())
                    Cutlass.Cast(target);
                if (ObjectManager.Player.Distance(target) <= BotRK.Range && BotRK.IsReady() && Menu.Item("UseBotRKItems").GetValue<bool>())
                    BotRK.Cast(target);
                if (ObjectManager.Player.Distance(target) <= HexGunBlade.Range && HexGunBlade.IsReady() && Menu.Item("UseHexGunBladeItems").GetValue<bool>())
                    HexGunBlade.Cast(target);
                if (ObjectManager.Player.Distance(target) <= DFG.Range && DFG.IsReady() && Menu.Item("UseDFGItems").GetValue<bool>() && target.Health > GetComboDamage(target))
                    DFG.Cast(target);

                if (ObjectManager.Player.Distance(target) < Q.Range && Q.IsReady() && Menu.Item("UseQCombo").GetValue<bool>())
                    Q.Cast(Menu.Item("UsePackets").GetValue<bool>()); 
                if (ObjectManager.Player.Distance(target) < E.Range && E.IsReady() && Menu.Item("UseECombo").GetValue<bool>())
                    E.CastOnUnit(target, Menu.Item("UsePackets").GetValue<bool>());
                if (ObjectManager.Player.Distance(target) < R.Range && R.IsReady() && GetComboDamage(target) > target.Health && Menu.Item("UseRCombo").GetValue<bool>());
                    R.Cast(target, Menu.Item("UsePackets").GetValue<bool>(), true);
                if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && Menu.Item("UseIgniteCombo").GetValue<bool>())
                    if(GetComboDamage(target) > target.Health)
                      Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                /*if(ObjectManager.Player.HasBuffOfType(BuffType.Slow) && Menu.Item("SmartW").GetValue<bool>() && W.IsReady())
                    W.Cast();*/


            }

        }
        private static void LaneClear()
        {

            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

            foreach (var minion in minions.Where(minion => minion.IsValidTarget(Q.Range)))
            {
                if (Menu.Item("UseQLaneClear").GetValue<bool>() && Q.IsReady())
                    Q.Cast();
                if (Menu.Item("UseELaneClear").GetValue<bool>() && E.IsReady())
                    E.CastOnUnit(minion);
            }


        }

        private static void JungleFarm()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (minions.Count > 0)
            {
                if (Menu.Item("UseQJungleFarm").GetValue<bool>() && Q.IsReady())
                    Q.Cast();
                if (Menu.Item("UseEJungleFarm").GetValue<bool>() && E.IsReady())
                    E.CastOnUnit(minions[0]);

            }
        }




        private static float GetPlayerHP(float HP)
        {
            return (float) (Player.MaxHealth * (HP / 100));

        }

        private static void SmartQuickS()
        {

            if (ObjectManager.Player.HasBuffOfType(BuffType.Slow) && W.IsReady()) return;
            if (ObjectManager.Player.HasBuffOfType(BuffType.Slow) || ObjectManager.Player.HasBuffOfType(BuffType.Blind) || ObjectManager.Player.HasBuffOfType(BuffType.Fear) || ObjectManager.Player.HasBuffOfType(BuffType.Stun) ||
                ObjectManager.Player.HasBuffOfType(BuffType.Charm) || ObjectManager.Player.HasBuffOfType(BuffType.Silence) || ObjectManager.Player.HasBuffOfType(BuffType.Snare) || ObjectManager.Player.HasBuffOfType(BuffType.Taunt)
                || ObjectManager.Player.HasBuffOfType(BuffType.Sleep) || ObjectManager.Player.HasBuffOfType(BuffType.Shred) || ObjectManager.Player.HasBuffOfType(BuffType.Polymorph) || ObjectManager.Player.HasBuffOfType(BuffType.Knockup)
                || ObjectManager.Player.HasBuffOfType(BuffType.Knockback) || ObjectManager.Player.HasBuffOfType(BuffType.Disarm) || ObjectManager.Player.HasBuffOfType(BuffType.Poison))
            {
                if (ObjectManager.Player.HasBuffOfType(BuffType.Poison))
                {

                    if (Player.Health <=  GetPlayerHP(Menu.Item("Quick%Poison").GetValue<Slider>().Value))
                        QuickS.Cast();
                        if(Scimitar.IsReady())
                            Scimitar.Cast();

                }
                else if (!(ObjectManager.Player.HasBuffOfType(BuffType.Poison)))
                    QuickS.Cast();
                    if(Scimitar.IsReady())
                        Scimitar.Cast();


            }



        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            if (DFG.IsReady())
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;
            if (Cutlass.IsReady())
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
            if (BotRK.IsReady())
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            if (HexGunBlade.IsReady())
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Hexgun);
            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            if(DFG.IsReady())
                damage = damage * 1.2;
            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float) damage; 
           

        }
    }
}
