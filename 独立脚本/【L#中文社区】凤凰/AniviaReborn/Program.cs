using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;
using Color = System.Drawing.Color;

namespace AniviaReborn
{
    internal class Program
    {
        public const string ChampionName = "Anivia";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static float[] wWidth = {400f, 500f, 600f, 700f, 800f};

        public static SpellSlot IgniteSlot;

        //mana manager
        public static int[] qMana = {80, 80, 90, 100, 110, 120};
        public static int[] wMana = {70, 70, 70, 70, 70, 70};
        public static int[] eMana = {80, 50, 60, 70, 80, 90};
        public static int[] rMana = {75, 75, 75, 75, 75};

        //Spell Obj
        //Q
        public static GameObject qMissle = null;
        public static bool qFirstCreated = false;
        public static Vector3 qPos;

        //E
        public static bool eCasted = false;

        //R
        public static GameObject rObj = null;
        public static bool rFirstCreated = false;
        public static bool rByMe = false;

        //Menu
        public static Menu menu;

        public static Obj_AI_Hero SelectedTarget = null;
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
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 625);

            Q.SetSkillshot(.5f, 110f, 750f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(.25f, 1f, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(.25f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            //Create the menu
            menu = new Menu(ChampionName, ChampionName, true);

            //Orbwalker submenu
            var orbwalkerMenu = new Menu("璧扮爫", "my_Orbwalker");
            LXOrbwalker.AddToMenu(orbwalkerMenu);
            menu.AddSubMenu(orbwalkerMenu);

            //Target selector
            var targetSelectorMenu = new Menu("鐩爣閫夋嫨", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);


            //key menu
            menu.AddSubMenu(new Menu("鐑敭", "Key"));
            menu.SubMenu("Key")
                .AddItem(
                    new MenuItem("ComboActive", "杩炴嫑").SetValue(
                        new KeyBind(menu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Key")
                .AddItem(
                    new MenuItem("HarassActive", "楠氭壈").SetValue(
                        new KeyBind(menu.Item("Harass_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
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
                    new MenuItem("snipe", "W-Q").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            menu.SubMenu("Key")
                .AddItem(
                    new MenuItem("escape", "閫冭窇").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Press)));

            //Combo menu:
            menu.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("selected", "閿佸畾鐩爣").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "浣跨敤Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "浣跨敤W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "浣跨敤E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "浣跨敤R").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("ignite", "鐐圭噧").SetValue(true));
            menu.SubMenu("Combo")
                .AddItem(new MenuItem("igniteMode", "妯″紡").SetValue(new StringList(new[] {"Combo", "KS"}, 0)));

            //Harass menu:
            menu.AddSubMenu(new Menu("楠氭壈", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "浣跨敤Q").SetValue(false));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "浣跨敤W").SetValue(false));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "浣跨敤E").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseRHarass", "浣跨敤R").SetValue(true));

            //Farming menu:
            menu.AddSubMenu(new Menu("琛ュ叺", "Farm"));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "浣跨敤Q").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "浣跨敤E").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseRFarm", "浣跨敤R").SetValue(false));

            //Misc Menu:
            menu.AddSubMenu(new Menu("鏉傞」", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "鎵撴柇").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseGap", "w闃茬獊").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("packet", "灏佸寘").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("checkR", "鑷姩鍏砇").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("detonateQ", "鑷姩寮曠垎Q").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("detonateQ2", "韬悗鎵擰").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("wallKill", "鍙潃鐢╓").SetValue(true));
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
                .AddItem(dmgAfterComboItem);
            menu.AddToMainMenu();
			
			menu.AddSubMenu(new Menu("L#涓枃绀惧尯", "AD"));
				menu.SubMenu("AD").AddItem(new MenuItem("WANGZHAN", "www.loll35.com"));
				menu.SubMenu("AD").AddItem(new MenuItem("qunhao", "姹夊寲缇わ細397983217"));
            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Game.PrintChat(ChampionName + " Loaded! --- by xSalice");
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (E.IsReady() & (Q.IsReady() || R.IsReady()))
                damage += Player.GetSpellDamage(enemy, SpellSlot.E)*2;
            else if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R)*3;

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
                menu.Item("UseEHarass").GetValue<bool>(), menu.Item("UseRHarass").GetValue<bool>(), "Harass");
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, string Source)
        {
            var range = Q.IsReady() ? Q.Range : W.Range;
            var focusSelected = menu.Item("selected").GetValue<bool>();
            Obj_AI_Hero target = SimpleTs.GetTarget(range, SimpleTs.DamageType.Magical);
            if (SimpleTs.GetSelectedTarget() != null)
                if (focusSelected && SimpleTs.GetSelectedTarget().Distance(Player.ServerPosition) < range)
                    target = SimpleTs.GetSelectedTarget();

            int IgniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;
            float dmg = GetComboDamage(target);
            bool hasMana = manaCheck();

            if (useE && target != null && E.IsReady() && Player.Distance(target) < E.Range && shouldE(target, Source))
            {
                E.CastOnUnit(target, packets());
            }

            if (useQ && Q.IsReady() && Player.Distance(target) <= Q.Range && target != null &&
                Q.GetPrediction(target).Hitchance >= HitChance.High && shouldQ(target))
            {
                Vector3 qPos2 = Q.GetPrediction(target).CastPosition;
                var vec = new Vector3(qPos2.X - Player.ServerPosition.X, 0, qPos2.Z - Player.ServerPosition.Z);
                Vector3 CastBehind = qPos2 + Vector3.Normalize(vec)*100;

                qPos = CastBehind;
                Q.Cast(target, packets());
            }

            //Ignite
            if (target != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && Source == "Combo" && hasMana)
            {
                if (IgniteMode == 0 && dmg > target.Health)
                {
                    Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                }
            }

            if (useW && target != null && W.IsReady() && Player.Distance(target) <= W.Range && shouldUseW(target))
            {
                castW(target);
            }

            if (useR && target != null && R.IsReady() && Player.Distance(target) < R.Range &&
                R.GetPrediction(target).Hitchance >= HitChance.High)
            {
                if (shouldR(target, Source))
                    R.Cast(target);
            }
        }

        public static bool manaCheck()
        {
            int totalMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] + rMana[R.Level];

            if (Player.Mana >= totalMana)
                return true;

            return false;
        }

        public static void smartKS()
        {
            if (!menu.Item("smartKS").GetValue<bool>())
                return;

            foreach (
                Obj_AI_Hero target in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => Player.Distance(x) < 1300 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (target != null)
                {
                    //ER
                    if (Player.Distance(target.ServerPosition) <= R.Range && !rFirstCreated && 
                        (Player.GetSpellDamage(target, SpellSlot.R) + Player.GetSpellDamage(target, SpellSlot.E)*2) >
                        target.Health + 50)
                    {
                        if (R.IsReady() && E.IsReady())
                        {
                            E.CastOnUnit(target, packets());
                            R.CastOnUnit(target, packets());
                            return;
                        }
                    }

                    //QR
                    if (Player.Distance(target.ServerPosition) <= R.Range && !qFirstCreated &&
                        (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.R)) >
                        target.Health + 30)
                    {
                        if (W.IsReady() && R.IsReady())
                        {
                            W.Cast(target, packets());
                            return;
                        }
                    }

                    //Q
                    if (Player.Distance(target.ServerPosition) <= Q.Range && !qFirstCreated &&
                        (Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 30)
                    {
                        if (Q.IsReady())
                        {
                            Q.Cast(target, packets());
                            return;
                        }
                    }

                    //E
                    if (Player.Distance(target.ServerPosition) <= E.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 30)
                    {
                        if (E.IsReady())
                        {
                            E.CastOnUnit(target, packets());
                            return;
                        }
                    }

                    //ignite
                    if (menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
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

        public static bool shouldQ(Obj_AI_Hero target)
        {
            if (qFirstCreated)
                return false;

            return true;
        }

        public static bool shouldR(Obj_AI_Hero target, string source)
        {
            if (rFirstCreated)
            {
                //Game.PrintChat("Bleh");
                return false;
            }
            if (rByMe)
            {
                Game.PrintChat("Bleh2");
                return false;
            }

            if (eCasted)
                return true;

            if (source == "Combo")
                return true;

            return false;
        }

        public static bool shouldE(Obj_AI_Hero target, string source)
        {
            if (checkChilled(target))
                return true;

            if (Player.GetSpellDamage(target, SpellSlot.E) > target.Health)
                return true;

            if (R.IsReady() && Player.Distance(target) <= R.Range - 25 && Player.Distance(target.ServerPosition) > 250)
                return true;

            return false;
        }

        public static bool shouldUseW(Obj_AI_Hero target)
        {
            if (GetComboDamage(target) >= target.Health - 20 && menu.Item("wallKill").GetValue<bool>())
                return true;

            if (rFirstCreated && rObj != null)
            {
                if (rObj.Position.Distance(target.ServerPosition) > 300)
                {
                    return true;
                }
            }

            return false;
        }

        public static void castW(Obj_AI_Hero target)
        {
            PredictionOutput pred = W.GetPrediction(target);
            var vec = new Vector3(pred.CastPosition.X - Player.ServerPosition.X, 0,
                pred.CastPosition.Z - Player.ServerPosition.Z);
            Vector3 CastBehind = pred.CastPosition + Vector3.Normalize(vec)*125;

            if (W.IsReady())
                W.Cast(CastBehind, packets());
        }

        /*public static void castWBetween()
        {
            var enemy = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champ.ServerPosition) < W.Range && champ.IsEnemy && champ.IsValid select champ).ToList();
            enemy.OrderBy(x => rObj.Position.Distance(x.ServerPosition));

            castW(enemy.FirstOrDefault());
        }*/

        public static void castWEscape(Obj_AI_Hero target)
        {
            PredictionOutput pred = W.GetPrediction(target);
            var vec = new Vector3(pred.CastPosition.X - Player.ServerPosition.X, 0,
                pred.CastPosition.Z - Player.ServerPosition.Z);
            Vector3 CastBehind = pred.CastPosition - Vector3.Normalize(vec)*125;

            if (W.IsReady())
                W.Cast(CastBehind, packets());
        }

        public static bool checkChilled(Obj_AI_Hero target)
        {
            return target.HasBuff("Chilled");
        }

        public static void detonateQ()
        {
            foreach (
                Obj_AI_Hero enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => Player.Distance(x) < 1200 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (enemy != null)
                {
                    if (qMissle != null && Q.IsReady())
                    {
                        if (shouldDetonate(enemy))
                        {
                            Q.Cast();
                        }
                    }
                }
            }
        }

        public static bool shouldDetonate(Obj_AI_Hero target)
        {
            var Q2 = menu.Item("detonateQ2").GetValue<bool>();
            if(Q2)
            if (target.ServerPosition.To2D().Distance(qPos.To2D()) < 50 || checkChilled(target))
                return true;

            if (target.ServerPosition.To2D().Distance(qMissle.Position.To2D()) < 110)
                return true;

            return false;
        }

        public static void snipe()
        {
            var range = Q.Range;
            var focusSelected = menu.Item("selected").GetValue<bool>();
            Obj_AI_Hero qTarget = SimpleTs.GetTarget(range, SimpleTs.DamageType.Magical);
            if (SimpleTs.GetSelectedTarget() != null)
                if (focusSelected && SimpleTs.GetSelectedTarget().Distance(Player.ServerPosition) < range)
                    qTarget = SimpleTs.GetSelectedTarget();

            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (W.IsReady() && Q.IsReady() && Player.Distance(qTarget.ServerPosition) < W.Range)
                castW(qTarget);

            if (!W.IsReady() && Q.IsReady() && Player.Distance(qTarget.ServerPosition) < Q.Range &&
                Q.GetPrediction(qTarget).Hitchance >= HitChance.High && !qFirstCreated)
            {
                Q.Cast(Q.GetPrediction(qTarget).CastPosition, packets());
                qFirstCreated = true;
            }
        }

        public static void checkR()
        {
            int hit = 0;
            foreach (
                Obj_AI_Hero enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            x =>
                                rObj.Position.Distance(x.ServerPosition) < 500 && x.IsValidTarget() && x.IsEnemy &&
                                !x.IsDead))
            {
                if (enemy != null)
                {
                    if (rObj != null && R.IsReady() && rObj.Position.Distance(enemy.ServerPosition) < 475)
                    {
                        hit++;
                    }
                }
            }

            if (hit < 1 && R.IsReady() && rObj != null && rFirstCreated && R.IsReady())
            {
                R.Cast();
            }
        }

        public static void escape()
        {
            if (LXOrbwalker.CanMove())
                LXOrbwalker.Orbwalk(Game.CursorPos, null);

            List<Obj_AI_Hero> enemy = (from champ in ObjectManager.Get<Obj_AI_Hero>()
                where Player.Distance(champ.ServerPosition) < 2500 && champ.IsEnemy && champ.IsValid
                select champ).ToList();
            enemy.OrderBy(x => rObj.Position.Distance(x.ServerPosition));

            if (Q.IsReady() && Player.Distance(enemy.FirstOrDefault()) <= Q.Range && enemy != null &&
                Q.GetPrediction(enemy.FirstOrDefault()).Hitchance >= HitChance.High && !qFirstCreated)
            {
                Q.Cast(enemy.FirstOrDefault(), packets());
                qFirstCreated = true;
            }

            if (enemy != null && W.IsReady() && Player.Distance(enemy.FirstOrDefault()) <= W.Range)
            {
                castWEscape(enemy.FirstOrDefault());
            }
        }

        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, R.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range,
                MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm").GetValue<bool>();
            var useE = menu.Item("UseEFarm").GetValue<bool>();
            var useR = menu.Item("UseRFarm").GetValue<bool>();

            int hit = 0;

            if (useQ && Q.IsReady() && !qFirstCreated)
            {
                MinionManager.FarmLocation qPos = Q.GetLineFarmLocation(allMinionsQ);
                if (qPos.MinionsHit >= 3)
                {
                    Q.Cast(qPos.Position, packets());
                }
            }

            if (useR & R.IsReady() && !rFirstCreated)
            {
                MinionManager.FarmLocation rPos = R.GetCircularFarmLocation(allMinionsR);
                if (Player.Distance(rPos.Position) < R.Range)
                    R.Cast(rPos.Position);
            }

            if (qFirstCreated)
            {
                if (useQ && Q.IsReady())
                {
                    foreach (Obj_AI_Base enemy in allMinionsQ)
                    {
                        if (enemy.Distance(qMissle.Position) < 110)
                            hit++;
                    }
                }

                if (hit > 2 && Q.IsReady())
                    Q.Cast();
            }

            if (rFirstCreated)
            {
                foreach (Obj_AI_Base enemy in allMinionsR)
                {
                    if (enemy.Distance(rObj.Position) < 400)
                        hit++;
                }

                if (hit < 2 && R.IsReady())
                    R.Cast();
            }

            if (useE && allMinionsE.Count > 0 && E.IsReady())
                E.Cast(allMinionsE[0]);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead)
            {
                //reset on death
                qMissle = null;
                qFirstCreated = false;
                eCasted = false;
                //rObj = null;
                //rFirstCreated = false;
                return;
            }

            //detonate Q check
            var detQ = menu.Item("detonateQ").GetValue<bool>();
            if (detQ && qFirstCreated)
                detonateQ();

            //checkR
            var rCheck = menu.Item("checkR").GetValue<bool>();
            if (rCheck && rFirstCreated && !menu.Item("LaneClearActive").GetValue<KeyBind>().Active && rByMe)
                checkR();


            //check ks
            smartKS();

            if (menu.Item("escape").GetValue<KeyBind>().Active)
            {
                escape();
            }
            else if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("snipe").GetValue<KeyBind>().Active)
                    snipe();

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
        }

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs attack)
        {
            if (unit.IsMe)
            {
                if (attack.SData.Name == "Frostbite")
                {
                    eCasted = true;
                }

                if (attack.SData.Name == "FlashFrost" && !qFirstCreated)
                {
                    //Game.PrintChat("woot");
                    qFirstCreated = true;
                }
            }
        }

        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap").GetValue<bool>()) return;

            if (W.IsReady() && gapcloser.Sender.IsValidTarget(W.Range))
            {
                Vector3 vec = Player.ServerPosition -
                              Vector3.Normalize(Player.ServerPosition - gapcloser.Sender.ServerPosition)*1;
                W.Cast(vec, packets());
            }
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!menu.Item("UseInt").GetValue<bool>()) return;

            if (Player.Distance(unit) < Q.Range && unit != null)
            {
                if (Q.GetPrediction(unit).Hitchance >= HitChance.High && Q.IsReady())
                    Q.Cast(unit, packets());
            }

            if (Player.Distance(unit) < W.Range && unit != null && W.IsReady())
            {
                W.Cast(unit, packets());
            }
        }

        private static void OnCreate(GameObject obj, EventArgs args)
        {
            //if(Player.Distance(obj.Position) < 300)
            //Game.PrintChat("OBJ: " + obj.Name);

            if (Player.Distance(obj.Position) < 1500)
            {
                //Q
                if (obj != null && obj.IsValid && obj.Name == "cryo_FlashFrost_Player_mis.troy")
                {
                    qMissle = obj;
                    qFirstCreated = true;
                }

                //R
                if (obj != null && obj.IsValid && obj.Name.Contains("cryo_storm"))
                {
                    if (menu.Item("ComboActive").GetValue<KeyBind>().Active ||
                        menu.Item("LaneClearActive").GetValue<KeyBind>().Active ||
                        menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                        rByMe = true;

                    rObj = obj;
                    rFirstCreated = true;
                }
            }
        }

        private static void OnDelete(GameObject obj, EventArgs args)
        {
            //if (Player.Distance(obj.Position) < 300)
            //Game.PrintChat("OBJ2: " + obj.Name);

            if (Player.Distance(obj.Position) < 1500)
            {
                //Q
                if (Player.Distance(obj.Position) < 1500)
                {
                    if (obj != null && obj.IsValid && obj.Name == "cryo_FlashFrost_Player_mis.troy")
                    {
                        qMissle = null;
                        qFirstCreated = false;
                    }

                    if (obj != null && obj.IsValid && obj.Name == "cryo_FrostBite_tar.troy")
                    {
                        eCasted = false;
                    }

                    //R
                    if (obj != null && obj.IsValid && obj.Name.Contains("cryo_storm"))
                    {
                        //Game.PrintChat("woot");
                        rObj = null;
                        rFirstCreated = false;
                        rByMe = false;
                    }
                }
            }
        }

    }
}