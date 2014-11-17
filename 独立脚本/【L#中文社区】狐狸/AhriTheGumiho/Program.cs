using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LX_Orbwalker;

namespace AhriTheGumiho
{
    internal class Program
    {
        public const string ChampionName = "Ahri";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static bool rOn;
        public static int rTimer;
        public static int rTimeLeft;

        public static SpellSlot IgniteSlot;
        public static Obj_AI_Hero SelectedTarget = null;
        //mana 
        public static int[] qMana = {55, 55, 60, 65, 70, 75};
        public static int[] wMana = {50, 50, 50, 50, 50, 50};
        public static int[] eMana = {85, 85, 85, 85, 85, 85};
        public static int[] rMana = {100, 100, 100, 100, 100, 100};
        //items
        public static Items.Item DFG;

        //Menu
        public static Menu menu;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //check to see if correct champ
            if (Player.BaseSkinName != ChampionName) return;

            //intalize spell
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 875);
            R = new Spell(SpellSlot.R, 850);

            Q.SetSkillshot(0.5f, 100, 1100, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 60, 1200, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline
                ? new Items.Item(3188, 750)
                : new Items.Item(3128, 750);

            //Create the menu
            menu = new Menu(ChampionName, ChampionName, true);

            //Orbwalker submenu
            var orbwalkerMenu = new Menu("璧扮爫", "my_Orbwalker");
            LXOrbwalker.AddToMenu(orbwalkerMenu);
            menu.AddSubMenu(orbwalkerMenu);

            //Target selector
            var targetSelectorMenu = new Menu("鐩爣鐜板湪", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);

            //key
            menu.AddSubMenu(new Menu("鐑敭", "Key"));
            menu.SubMenu("Key")
                .AddItem(
                    new MenuItem("ComboActive", "杩炴嫑").SetValue(
                        new KeyBind(menu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Key")
                .AddItem(
                    new MenuItem("HarassActive", "楠氭壈").SetValue(
                        new KeyBind(menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Key")
                .AddItem(
                    new MenuItem("HarassActiveT", "楠氭壈 (閿佸畾)").SetValue(new KeyBind("Y".ToCharArray()[0],
                        KeyBindType.Toggle)));
            menu.SubMenu("Key")
                .AddItem(
                    new MenuItem("LaneClearActive", "琛ュ叺").SetValue(
                        new KeyBind(menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Key")
                .AddItem(
                    new MenuItem("charmCombo", "榄呮儜鍚嶲").SetValue(new KeyBind("I".ToCharArray()[0],
                        KeyBindType.Toggle)));

            //Combo menu:
            menu.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("selected", "閿佸畾鐩爣").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "浣跨敤Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("qHit", "Q/E min鍑讳腑").SetValue(new Slider(3, 1, 4)));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "浣跨敤W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "浣跨敤E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "浣跨敤R").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("rSpeed", "浣跨敤涓夋R绉掍汉").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("ignite", "浣跨敤鐐圭噧").SetValue(true));
            menu.SubMenu("Combo")
                .AddItem(new MenuItem("igniteMode", "妯″紡").SetValue(new StringList(new[] { "Combo", "KS" }, 0)));

            //Harass menu:
            menu.AddSubMenu(new Menu("楠氭壈", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "浣跨敤Q").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("qHit2", "Q/E  min鍑讳腑").SetValue(new Slider(3, 1, 4)));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "浣跨敤W").SetValue(false));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "浣跨敤E").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("longQ", "杩滆窛绂籕").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("charmHarass", "榄呮儜鍚庡彧Q").SetValue(true));

            //Farming menu:
            menu.AddSubMenu(new Menu("琛ュ叺", "Farm"));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "浣跨敤Q").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "浣跨敤W").SetValue(false));

            //Misc Menu:
            menu.AddSubMenu(new Menu("鏉傞」", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "E鎵撴柇").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseGap", "E闃茬獊").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("packet", "灏佸寘").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("mana", "R鍓嶆鏌ヨ摑").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("dfgCharm", "榄呮儜+鍐ョ伀").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("EQ", "EQ").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("smartKS", "鏅鸿兘鎶㈠ご").SetValue(true));

            //Damage after combo:
            MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "鏄剧ず浼ゅ").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            //Drawings menu:
            menu.AddSubMenu(new Menu("鏄剧ず", "Drawings"));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W鑼冨洿").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("cursor", "鏄剧ずR绌垮").SetValue(new Circle(false,
                        Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
            menu.AddToMainMenu();

			menu.AddSubMenu(new Menu("L#涓枃绀惧尯", "AD"));
				menu.SubMenu("AD").AddItem(new MenuItem("WANGZHAN", "www.loll35.com"));
				menu.SubMenu("AD").AddItem(new MenuItem("qunhao", "姹夊寲缇わ細397983217"));
            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.PrintChat(ChampionName + " Loaded! --- by xSalice");
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q, 1);
            }

            if (DFG.IsReady())
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg)/1.2;

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R)*2; //* Player.Spellbook.GetSpell(SpellSlot.R).Ammo;

            if (DFG.IsReady() && E.IsReady())
                damage = damage*1.44;
            else if (DFG.IsReady() && enemy.HasBuffOfType(BuffType.Charm))
                damage = damage*1.44;
            else if (E.IsReady())
                damage = damage*1.2;
            else if (DFG.IsReady())
                damage = damage*1.2;
            else if (enemy.HasBuffOfType(BuffType.Charm))
                damage = damage*1.2;

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            return (float) damage;
        }

        private static void Combo()
        {
            UseSpells(menu.Item("UseQCombo").GetValue<bool>(), menu.Item("UseWCombo").GetValue<bool>(),
                menu.Item("UseECombo").GetValue<bool>(), menu.Item("UseRCombo").GetValue<bool>(), "Combo");
        }

        private static void Harass()
        {
            UseSpells(menu.Item("UseQHarass").GetValue<bool>(), menu.Item("UseWHarass").GetValue<bool>(),
                menu.Item("UseEHarass").GetValue<bool>(), false, "Harass");
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, string Source)
        {
            var range = Q.Range;
            var focusSelected = menu.Item("selected").GetValue<bool>();
            Obj_AI_Hero eTarget = SimpleTs.GetTarget(range, SimpleTs.DamageType.Magical);
            if (SimpleTs.GetSelectedTarget() != null)
                if (focusSelected && SimpleTs.GetSelectedTarget().Distance(Player.ServerPosition) < range)
                    eTarget = SimpleTs.GetSelectedTarget();

            Obj_AI_Hero rETarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);

            int IgniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;

            var hitC = HitChance.High;
            int qHit = menu.Item("qHit").GetValue<Slider>().Value;
            int harassQHit = menu.Item("qHit2").GetValue<Slider>().Value;
            var dmg = GetComboDamage(eTarget);

            // HitChance.Low = 3, Medium , High .... etc..
            if (Source == "Combo")
            {
                switch (qHit)
                {
                    case 1:
                        hitC = HitChance.Low;
                        break;
                    case 2:
                        hitC = HitChance.Medium;
                        break;
                    case 3:
                        hitC = HitChance.High;
                        break;
                    case 4:
                        hitC = HitChance.VeryHigh;
                        break;
                }
            }
            else if (Source == "Harass")
            {
                switch (harassQHit)
                {
                    case 1:
                        hitC = HitChance.Low;
                        break;
                    case 2:
                        hitC = HitChance.Medium;
                        break;
                    case 3:
                        hitC = HitChance.High;
                        break;
                    case 4:
                        hitC = HitChance.VeryHigh;
                        break;
                }
            }

            //DFG
            if (eTarget != null && dmg > eTarget.Health - 300 && DFG.IsReady() && Source == "Combo" && Player.Distance(eTarget) <= 750 &&
                (eTarget.HasBuffOfType(BuffType.Charm) || !menu.Item("dfgCharm").GetValue<bool>()))
            {
                //Game.PrintChat("TRying!!!");
                DFG.Cast(eTarget);
            }

            //E
            if (useE && eTarget != null && E.IsReady() && Player.Distance(eTarget) < E.Range &&
                E.GetPrediction(eTarget).Hitchance >= hitC)
            {
                E.Cast(eTarget, packets());
                if (menu.Item("EQ").GetValue<bool>() && Q.IsReady())
                {
                    Q.Cast(eTarget, packets());
                }
                return;
            }

            //Ignite
            if (eTarget != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown && !E.IsReady() &&
                Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && Source == "Combo")
            {
                if (IgniteMode == 0 && GetComboDamage(eTarget) > eTarget.Health)
                {
                    Player.SummonerSpellbook.CastSpell(IgniteSlot, eTarget);
                }
            }

            //W
            if (useW && eTarget != null && W.IsReady() && Player.Distance(eTarget) <= W.Range &&
                shouldW(eTarget, Source))
            {
                W.Cast();
            }
            if (Source == "Harass" && menu.Item("longQ").GetValue<bool>())
            {
                if (useQ && Q.IsReady() && Player.Distance(eTarget) <= Q.Range && eTarget != null &&
                    shouldQ(eTarget, Source) && Player.Distance(eTarget) > 600)
                {
                    if (Q.GetPrediction(eTarget).Hitchance >= hitC)
                    {
                        Q.Cast(eTarget, packets(), true);
                        return;
                    }
                }
            }
            else if (useQ && Q.IsReady() && Player.Distance(eTarget) <= Q.Range && eTarget != null &&
                     shouldQ(eTarget, Source))
            {
                if (Q.GetPrediction(eTarget).Hitchance >= hitC)
                {
                    Q.Cast(eTarget, packets(), true);
                    return;
                }
            }

            //R
            if (useR && eTarget != null && R.IsReady() && Player.Distance(eTarget) < R.Range)
            {
                if (E.IsReady())
                {
                    if (checkREQ(rETarget))
                        E.Cast(rETarget, packets());
                }
                if (shouldR(eTarget) && R.IsReady())
                {
                    R.Cast(Game.CursorPos, packets());
                    rTimer = Environment.TickCount - 250;
                }
                if (rTimeLeft > 9500 && rOn && R.IsReady())
                {
                    R.Cast(Game.CursorPos, packets());
                    rTimer = Environment.TickCount - 250;
                }
            }
        }

        public static bool shouldQ(Obj_AI_Hero target, string Source)
        {
            if (Source == "Combo")
            {
                if ((Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1)) >
                    target.Health)
                    return true;

                if (rOn)
                    return true;

                if (!menu.Item("charmCombo").GetValue<KeyBind>().Active)
                    return true;

                if (target.HasBuffOfType(BuffType.Charm))
                    return true;
            }

            if (Source == "Harass")
            {
                if ((Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1)) >
                    target.Health)
                    return true;

                if (rOn)
                    return true;

                if (!menu.Item("charmHarass").GetValue<bool>())
                    return true;

                if (target.HasBuffOfType(BuffType.Charm))
                    return true;
            }

            return false;
        }

        public static bool shouldW(Obj_AI_Hero target, string Source)
        {
            if (Source == "Combo")
            {
                if (Player.GetSpellDamage(target, SpellSlot.W) > target.Health)
                    return true;

                if (rOn)
                    return true;

                if (!menu.Item("charmCombo").GetValue<KeyBind>().Active)
                    return true;

                if (target.HasBuffOfType(BuffType.Charm))
                    return true;
            }
            if (Source == "Harass")
            {
                if (Player.GetSpellDamage(target, SpellSlot.W) > target.Health)
                    return true;

                if (rOn)
                    return true;

                if (!menu.Item("charmHarass").GetValue<bool>())
                    return true;

                if (target.HasBuffOfType(BuffType.Charm))
                    return true;
            }

            return false;
        }

        public static bool shouldR(Obj_AI_Hero target)
        {
            if (!manaCheck())
                return false;

            Vector3 dashVector = Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position)*425;
            if (Player.Distance(Game.CursorPos) < 75 && target.Distance(dashVector) > 525)
                return false;

            if (menu.Item("rSpeed").GetValue<bool>() && countEnemiesNearPosition(Game.CursorPos, 1500) < 2 && GetComboDamage(target) > target.Health - 100)
                return true;

            if (GetComboDamage(target) > target.Health && !rOn)
            {
                if (target.HasBuffOfType(BuffType.Charm))
                    return true;
            }

            if (target.HasBuffOfType(BuffType.Charm) && rOn)
                return true;

            if (countAlliesNearPosition(Game.CursorPos, 1000) > 2 && rTimeLeft > 3500)
                return true;

            if (Player.GetSpellDamage(target, SpellSlot.R)*2 > target.Health)
                return true;

            if (rOn && rTimeLeft > 9500)
                return true;

            return false;
        }

        public static bool checkREQ(Obj_AI_Hero target)
        {
            if (Player.Distance(Game.CursorPos) < 75)
                return false;

            if (GetComboDamage(target) > target.Health && !rOn && countEnemiesNearPosition(Game.CursorPos, 1500) < 3)
            {
                if (target.Distance(Game.CursorPos) <= E.Range && E.IsReady())
                {
                    Vector3 dashVector = Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position)*425;
                    float addedDelay = Player.Distance(dashVector)/2200;

                    //Game.PrintChat("added delay: " + addedDelay);

                    PredictionOutput pred = GetP(Game.CursorPos, E, target, addedDelay, false);
                    if (pred.Hitchance >= HitChance.High && R.IsReady())
                    {
                        //Game.PrintChat("R-E Mode Intiate!");
                        R.Cast(Game.CursorPos, packets());
                        rTimer = Environment.TickCount - 250;
                        return true;
                    }
                }
            }

            return false;
        }

        public static int countEnemiesNearPosition(Vector3 pos, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>().Count(
                    hero => hero.IsEnemy && !hero.IsDead && hero.IsValid && hero.Distance(pos) <= range);
        }

        public static int countAlliesNearPosition(Vector3 pos, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>().Count(
                    hero => hero.IsAlly && !hero.IsDead && hero.IsValid && hero.Distance(pos) <= range);
        }

        public static void checkKS()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.Distance(x) < 1300 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (target != null)
                {
                    if (DFG.IsReady() && Player.GetItemDamage(target, Damage.DamageItems.Dfg) > target.Health &&
                        Player.Distance(target.ServerPosition) <= 750)
                    {
                        DFG.Cast(target);
                        return;
                    }

                    if (DFG.IsReady() && Player.Distance(target.ServerPosition) <= 750 && Q.IsReady() &&
                        (Player.GetItemDamage(target, Damage.DamageItems.Dfg) +
                         (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1))*
                         1.2) > target.Health)
                    {
                        DFG.Cast(target);
                        Q.Cast(target, packets());
                        return;
                    }

                    if (DFG.IsReady() && Player.Distance(target.ServerPosition) <= 750 && W.IsReady() &&
                        (Player.GetItemDamage(target, Damage.DamageItems.Dfg) +
                         Player.GetSpellDamage(target, SpellSlot.W)*1.2) > target.Health)
                    {
                        DFG.Cast(target);
                        W.Cast();
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= W.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1) +
                         Player.GetSpellDamage(target, SpellSlot.W)) > target.Health && Q.IsReady() && Q.IsReady())
                    {
                        Q.Cast(target, packets());
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= Q.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1)) >
                        target.Health && Q.IsReady())
                    {
                        Q.Cast(target, packets());
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= E.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health & E.IsReady())
                    {
                        E.Cast(target, packets());
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= W.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.W)) > target.Health && W.IsReady())
                    {
                        W.Cast();
                        return;
                    }

                    Vector3 dashVector = Player.Position +
                                         Vector3.Normalize(target.ServerPosition - Player.Position)*425;
                    if (Player.Distance(target.ServerPosition) <= R.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.R)) > target.Health && R.IsReady() && rOn &&
                        target.Distance(dashVector) < 425 && R.IsReady())
                    {
                        R.Cast(dashVector, packets());
                    }

                    //ignite
                    if (target != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                        Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                        Player.Distance(target.ServerPosition) <= 600)
                    {
                        int IgniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;
                        if (Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health + 20)
                        {
                            Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                        }
                    }
                }
            }
        }

        public static bool manaCheck()
        {
            int totalMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] + rMana[R.Level];
            var checkMana = menu.Item("mana").GetValue<bool>();

            if (Player.Mana >= totalMana || checkMana)
                return true;

            return false;
        }

        public static bool isRActive()
        {
            return Player.HasBuff("AhriTumble", true);
        }

        public static PredictionOutput GetP(Vector3 pos, Spell spell, Obj_AI_Base target, float delay, bool aoe)
        {
            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay + delay,
                Radius = spell.Width,
                Speed = spell.Speed,
                From = pos,
                Range = spell.Range,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = Player.ServerPosition,
                Aoe = aoe,
            });
        }

        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm").GetValue<bool>();
            var useW = menu.Item("UseWFarm").GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                MinionManager.FarmLocation qPos = Q.GetLineFarmLocation(allMinionsQ);
                if (qPos.MinionsHit >= 3)
                {
                    Q.Cast(qPos.Position, packets());
                }
            }

            if (useW && allMinionsW.Count > 0 && W.IsReady())
                W.Cast();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            rOn = isRActive();

            if (rOn)
                rTimeLeft = Environment.TickCount - rTimer;

            //ks check
            if (menu.Item("smartKS").GetValue<bool>())
                checkKS();

            if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        public static bool packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (Spell spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

            if (menu.Item("cursor").GetValue<Circle>().Active)
                Utility.DrawCircle(Player.Position, 475, Color.Aquamarine);
        }

        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap").GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender);
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!menu.Item("UseInt").GetValue<bool>()) return;

            if (Player.Distance(unit) < E.Range && unit != null)
            {
                if (E.GetPrediction(unit).Hitchance >= HitChance.High && E.IsReady())
                    E.Cast(unit, packets());
            }
        }

    }
}