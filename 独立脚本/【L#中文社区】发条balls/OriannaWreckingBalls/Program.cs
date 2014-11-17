using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;
using Color = System.Drawing.Color;

namespace OriannaWreckingBalls
{
    internal class Program
    {
        public const string ChampionName = "Orianna";

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static SpellSlot IgniteSlot;

        public static Obj_AI_Hero SelectedTarget = null;

        //ball manager
        public static bool IsBallMoving = false;
        public static Vector3 CurrentBallPosition;
        public static Vector3 allyDraw;
        public static int ballStatus = 0;

        //Menu
        public static Menu menu;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Thanks to Esk0r
            Player = ObjectManager.Player;

            //check to see if correct champ
            if (Player.BaseSkinName != ChampionName) return;

            //intalize spell
            Q = new Spell(SpellSlot.Q, 825);
            W = new Spell(SpellSlot.W, 250);
            E = new Spell(SpellSlot.E, 1095);
            R = new Spell(SpellSlot.R, 370);

            Q.SetSkillshot(0.25f, 80, 1300, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0f, 250, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 145, 1700, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.60f, 370, float.MaxValue, false, SkillshotType.SkillshotCircle);

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

            //Keys
            menu.AddSubMenu(new Menu("鎸夐敭璁剧疆", "Keys"));
            menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("ComboActive", "杩炴嫑").SetValue(
                        new KeyBind(menu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("HarassActive", "楠氭壈").SetValue(
                        new KeyBind(menu.Item("Harass_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("HarassActiveT", "楠氭壈 (閿佸畾)").SetValue(new KeyBind("Y".ToCharArray()[0],
                        KeyBindType.Toggle)));
            menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("LastHitQQ", "Q琛ュ叺").SetValue(new KeyBind("A".ToCharArray()[0],
                        KeyBindType.Press)));
            menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("LaneClearActive", "琛ュ叺").SetValue(
                        new KeyBind(menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("escape", "閫冭窇").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Press)));
            //Spell Menu
            menu.AddSubMenu(new Menu("娉曟湳", "Spell"));
            //Q Menu
            menu.SubMenu("Spell").AddSubMenu(new Menu("Q", "QSpell"));
            menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("qHit", "杩炴嫑Q min鍑讳腑").SetValue(new Slider(3, 1, 3)));
            menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("qHit2", "楠氭壈Q min鍑讳腑").SetValue(new Slider(3, 1, 4)));
            //W
            menu.SubMenu("Spell").AddSubMenu(new Menu("W", "WSpell"));
            menu.SubMenu("Spell").SubMenu("WSpell").AddItem(new MenuItem("autoW", "W min鍑讳腑").SetValue(new Slider(2, 1, 5)));
            //E
            menu.SubMenu("Spell").AddSubMenu(new Menu("E", "ESpell"));
            menu.SubMenu("Spell").SubMenu("ESpell").AddItem(new MenuItem("UseEDmg", "鑷姩E").SetValue(true));
            menu.SubMenu("Spell").SubMenu("ESpell").AddSubMenu(new Menu("E闃熷弸", "shield"));
            menu.SubMenu("Spell").SubMenu("ESpell").SubMenu("shield").AddItem(new MenuItem("eAllyIfHP", "HP < %").SetValue(new Slider(40, 0, 100)));

            foreach (Obj_AI_Hero ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly))
                menu.SubMenu("Spell").SubMenu("ESpell")
                    .SubMenu("shield")
                    .AddItem(new MenuItem("shield" + ally.BaseSkinName, ally.BaseSkinName).SetValue(false));
            //R
            menu.SubMenu("Spell").AddSubMenu(new Menu("R", "RSpell"));
            menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("autoR", "R min鍑讳腑").SetValue(new Slider(3, 1, 5)));
            menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("blockR", "娌′汉涓峈").SetValue(true));
            menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("overK", "绉掍汉").SetValue(true));
            menu.SubMenu("Spell").SubMenu("RSpell").AddItem(
                    new MenuItem("killR", "R鐑敭").SetValue(new KeyBind("T".ToCharArray()[0],
                        KeyBindType.Toggle)));

            //Combo menu:
            menu.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("selected", "閿佸畾鐩爣").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "浣跨敤Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "浣跨敤W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "浣跨敤E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "浣跨敤R").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("autoRCombo", "R min鍑讳腑").SetValue(new Slider(2, 1, 5)));
            menu.SubMenu("Combo").AddItem(new MenuItem("ignite", "浣跨敤鐐圭噧").SetValue(true));

            //Harass menu:
            menu.AddSubMenu(new Menu("楠氭壈", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "浣跨敤Q").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "浣跨敤W").SetValue(false));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "浣跨敤E").SetValue(true));

            //Farming menu:
            menu.AddSubMenu(new Menu("琛ュ叺", "Farm"));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "浣跨敤Q").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "浣跨敤W").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("qFarm", "Q/W 灏忓叺> ").SetValue(new Slider(3, 0, 5)));

            //intiator list:
            menu.AddSubMenu((new Menu("E绐佽繘闃熷弸", "Initiator")));

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                foreach (Initiator intiator in Initiator.InitatorList)
                {
                    if (intiator.HeroName == hero.BaseSkinName)
                    {
                        menu.SubMenu("Initiator")
                            .AddItem(new MenuItem(intiator.spellName, intiator.spellName))
                            .SetValue(false);
                    }
                }
            }

            //Misc Menu:
            menu.AddSubMenu(new Menu("鏉傞」", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "R鎵撴柇").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("packet", "灏佸寘").SetValue(true));

            menu.SubMenu("Misc").AddSubMenu(new Menu("鑷姩R鎵撴柇", "intR"));

            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                menu.SubMenu("Misc")
                    .SubMenu("intR")
                    .AddItem(new MenuItem("intR" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));

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
                    new MenuItem("rModeDraw", "R妯″紡").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
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
            Game.OnGameSendPacket += Game_OnSendPacket;
            Game.PrintChat(ChampionName + " Loaded! --- by xSalice");
        }

        public static PredictionOutput GetP(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {
            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
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

        public static PredictionOutput GetPCircle(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {
            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
                Radius = 1,
                Speed = float.MaxValue,
                From = pos,
                Range = float.MaxValue,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = Player.ServerPosition,
                Aoe = aoe,
            });
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            //if (Q.IsReady())
            damage += Player.GetSpellDamage(enemy, SpellSlot.Q)*1.5;

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) - 25;

            return (float) damage;
        }

        private static void Combo()
        {
            //Orbwalker.SetAttacks(!(Q.IsReady()));
            UseSpells(menu.Item("UseQCombo").GetValue<bool>(), menu.Item("UseWCombo").GetValue<bool>(),
                menu.Item("UseECombo").GetValue<bool>(), menu.Item("UseRCombo").GetValue<bool>(), "Combo");
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, String source)
        {
            var focusSelected = menu.Item("selected").GetValue<bool>();
            var range = E.IsReady() ? E.Range : Q.Range;
            Obj_AI_Hero target = SimpleTs.GetTarget(range, SimpleTs.DamageType.Magical);
            if (SimpleTs.GetSelectedTarget() != null)
                if (focusSelected && SimpleTs.GetSelectedTarget().Distance(Player.ServerPosition) < range)
                    target = SimpleTs.GetSelectedTarget();

            if (useQ && Q.IsReady())
            {
                castQ(target, source);
            }

            if (IsBallMoving)
                return;

            if (useW && target != null && W.IsReady())
            {
                castW(target);
            }

            //Ignite
            if (target != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && source == "Combo")
            {
                if (GetComboDamage(target) > target.Health)
                {
                    Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                }
            }

            if (useE && target != null && E.IsReady())
            {
                castE(target);
            }

            if (useR && target != null && R.IsReady())
            {
                if (menu.Item("intR" + target.BaseSkinName) != null)
                {
                    foreach (
                        Obj_AI_Hero enemy in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(x => Player.Distance(x) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
                    {
                        if (enemy != null && !enemy.IsDead && menu.Item("intR" + enemy.BaseSkinName).GetValue<bool>())
                        {
                            castR(enemy);
                            return;
                        }
                    }
                }

                if (!(menu.Item("killR").GetValue<KeyBind>().Active)) //check if multi
                {
                    if (menu.Item("overK").GetValue<bool>() &&
                        (Player.GetSpellDamage(target, SpellSlot.Q)*2) >= target.Health)
                    {
                    }
                    if (GetComboDamage(target) >= target.Health - 100)
                        castR(target);
                }
            }
        }

        public static bool packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        public static void castW(Obj_AI_Base target)
        {
            if (IsBallMoving) return;

            PredictionOutput prediction = GetPCircle(CurrentBallPosition, W, target, true);

            if (W.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) < W.Width)
            {
                W.Cast();
            }

        }

        public static void castR(Obj_AI_Base target)
        {
            if (IsBallMoving) return;

            PredictionOutput prediction = GetPCircle(CurrentBallPosition, R, target, true);

            if (R.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) <= R.Width)
            {
                R.Cast();
            }
        }

        public static void castE(Obj_AI_Base target)
        {
            if (IsBallMoving) return;

            Obj_AI_Hero etarget = Player;

            switch (ballStatus)
            {
                case 0:
                    if (target != null)
                    {
                        float TravelTime = target.Distance(Player.ServerPosition)/Q.Speed;
                        float MinTravelTime = 10000f;

                        foreach (
                            Obj_AI_Hero ally in
                                ObjectManager.Get<Obj_AI_Hero>()
                                    .Where(x => x.IsAlly && Player.Distance(x.ServerPosition) <= E.Range && !x.IsMe))
                        {
                            if (ally != null)
                            {
                                //dmg enemy with E
                                if (menu.Item("UseEDmg").GetValue<bool>())
                                {
                                    PredictionOutput prediction3 = GetP(Player.ServerPosition, E, target, true);
                                    Object[] obj = VectorPointProjectionOnLineSegment(Player.ServerPosition.To2D(),
                                        ally.ServerPosition.To2D(), prediction3.UnitPosition.To2D());
                                    var isOnseg = (bool) obj[2];
                                    var PointLine = (Vector2) obj[1];

                                    if (E.IsReady() && isOnseg &&
                                        prediction3.UnitPosition.Distance(PointLine.To3D()) < E.Width)
                                    {
                                        //Game.PrintChat("Dmg 1");
                                        E.CastOnUnit(ally, packets());
                                        return;
                                    }
                                }

                                float allyRange = target.Distance(ally.ServerPosition)/Q.Speed +
                                                  ally.Distance(Player.ServerPosition)/E.Speed;
                                if (allyRange < MinTravelTime)
                                {
                                    etarget = ally;
                                    MinTravelTime = allyRange;
                                }
                            }
                        }

                        if (MinTravelTime < TravelTime && Player.Distance(etarget.ServerPosition) <= E.Range &&
                            E.IsReady())
                        {
                            E.CastOnUnit(etarget, packets());
                        }
                    }
                    break;
                case 1:
                    //dmg enemy with E
                    if (menu.Item("UseEDmg").GetValue<bool>())
                    {
                        PredictionOutput prediction = GetP(CurrentBallPosition, E, target, true);
                        Object[] obj = VectorPointProjectionOnLineSegment(CurrentBallPosition.To2D(),
                            Player.ServerPosition.To2D(), prediction.UnitPosition.To2D());
                        var isOnseg = (bool) obj[2];
                        var PointLine = (Vector2) obj[1];

                        if (E.IsReady() && isOnseg && prediction.UnitPosition.Distance(PointLine.To3D()) < E.Width)
                        {
                            //Game.PrintChat("Dmg 2");
                            E.CastOnUnit(Player, packets());
                            return;
                        }
                    }

                    float TravelTime2 = target.Distance(CurrentBallPosition) / Q.Speed;
                    float MinTravelTime2 = target.Distance(Player.ServerPosition)/Q.Speed +
                                            Player.Distance(CurrentBallPosition) / E.Speed;

                    if (MinTravelTime2 < TravelTime2 && target.Distance(Player.ServerPosition) <= Q.Range + Q.Width &&
                        E.IsReady())
                    {
                        E.CastOnUnit(Player, packets());
                    }
                    
                    break;
                case 2:
                    float TravelTime3 = target.Distance(CurrentBallPosition) / Q.Speed;
                    float MinTravelTime3 = 10000f;

                    foreach (
                        Obj_AI_Hero ally in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(x => x.IsAlly && Player.Distance(x.ServerPosition) <= E.Range && !x.IsMe))
                    {
                        if (ally != null)
                        {
                            //dmg enemy with E
                            if (menu.Item("UseEDmg").GetValue<bool>())
                            {
                                PredictionOutput prediction2 = GetP(CurrentBallPosition, E, target, true);
                                Object[] obj = VectorPointProjectionOnLineSegment(CurrentBallPosition.To2D(),
                                    ally.ServerPosition.To2D(), prediction2.UnitPosition.To2D());
                                var isOnseg = (bool) obj[2];
                                var PointLine = (Vector2) obj[1];

                                if (E.IsReady() && isOnseg &&
                                    prediction2.UnitPosition.Distance(PointLine.To3D()) < E.Width)
                                {
                                    //Game.PrintChat("Dmg 3");
                                    E.CastOnUnit(ally, packets());
                                    return;
                                }
                            }

                            float allyRange2 = target.Distance(ally.ServerPosition)/Q.Speed +
                                                ally.Distance(CurrentBallPosition)/E.Speed;

                            if (allyRange2 < MinTravelTime3)
                            {
                                etarget = ally;
                                MinTravelTime3 = allyRange2;
                            }
                        }
                    }

                    if (MinTravelTime3 < TravelTime3 && Player.Distance(etarget.ServerPosition) <= E.Range &&
                        E.IsReady())
                    {
                        E.CastOnUnit(etarget, packets());
                    }
                    
                    break;
            }
        }

        public static void castQ(Obj_AI_Base target, String Source)
        {
            if (IsBallMoving) return;

            var hitC = HitChance.High;
            int qHit = menu.Item("qHit").GetValue<Slider>().Value;
            int harassQHit = menu.Item("qHit2").GetValue<Slider>().Value;

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

            PredictionOutput prediction = GetP(CurrentBallPosition, Q, target, true);

            if (Q.IsReady() && prediction.Hitchance >= hitC && Player.Distance(target) <= Q.Range + Q.Width)
            {
                Q.Cast(prediction.CastPosition, packets());
            }
        }

        public static void checkWMec()
        {
            if (!W.IsReady() || IsBallMoving)
                return;

            int hit = 0;
            int minHit = menu.Item("autoW").GetValue<Slider>().Value;

            foreach (
                Obj_AI_Hero enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => Player.Distance(x) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (enemy != null)
                {
                    PredictionOutput prediction = GetPCircle(CurrentBallPosition, W, enemy, true);

                    if (W.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) < W.Width)
                    {
                        hit++;
                    }
                }
            }

            if (hit >= minHit && W.IsReady())
                W.Cast();
        }

        public static void checkRMec()
        {
            if (!R.IsReady() || IsBallMoving)
                return;

            int hit = 0;
            int minHit = menu.Item("autoRCombo").GetValue<Slider>().Value;

            foreach (
                Obj_AI_Hero enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => Player.Distance(x) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (enemy != null)
                {
                    PredictionOutput prediction = GetPCircle(CurrentBallPosition, R, enemy, true);

                    if (R.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) <= R.Width)
                    {
                        hit++;
                    }
                }
            }

            if (hit >= minHit && R.IsReady())
                R.Cast();
        }

        public static void checkRMecGlobal()
        {
            if (!R.IsReady() || IsBallMoving)
                return;

            int hit = 0;
            int minHit = menu.Item("autoR").GetValue<Slider>().Value;

            foreach (
                Obj_AI_Hero enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => Player.Distance(x) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (enemy != null)
                {
                    PredictionOutput prediction = GetPCircle(CurrentBallPosition, R, enemy, true);

                    if (R.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) <= R.Width)
                    {
                        hit++;
                    }
                }
            }

            if (hit >= minHit && R.IsReady())
                R.Cast();
        }

        //credit to dien
        public static Object[] VectorPointProjectionOnLineSegment(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float cx = v3.X;
            float cy = v3.Y;
            float ax = v1.X;
            float ay = v1.Y;
            float bx = v2.X;
            float by = v2.Y;
            float rL = ((cx - ax)*(bx - ax) + (cy - ay)*(by - ay))/
                       ((float) Math.Pow(bx - ax, 2) + (float) Math.Pow(by - ay, 2));
            var pointLine = new Vector2(ax + rL*(bx - ax), ay + rL*(by - ay));
            float rS;
            if (rL < 0)
            {
                rS = 0;
            }
            else if (rL > 1)
            {
                rS = 1;
            }
            else
            {
                rS = rL;
            }
            bool isOnSegment;
            if (rS.CompareTo(rL) == 0)
            {
                isOnSegment = true;
            }
            else
            {
                isOnSegment = false;
            }
            var pointSegment = new Vector2();
            if (isOnSegment)
            {
                pointSegment = pointLine;
            }
            else
            {
                pointSegment = new Vector2(ax + rS*(bx - ax), ay + rS*(by - ay));
            }
            return new object[3] {pointSegment, pointLine, isOnSegment};
        }

        public static int countR()
        {
            if (!R.IsReady())
                return 0;

            int hit = 0;
            foreach (
                Obj_AI_Hero enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => Player.Distance(x) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (enemy != null)
                {
                    PredictionOutput prediction = GetPCircle(CurrentBallPosition, R, enemy, true);

                    if (R.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) <= R.Width)
                    {
                        hit++;
                    }
                }
            }

            return hit;
        }

        public static void lastHit()
        {
            if (!Orbwalking.CanMove(40)) return;

            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (Obj_AI_Base minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                        HealthPrediction.GetHealthPrediction(minion, (int) (Player.Distance(minion)*1000/1400)) <
                        Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        PredictionOutput prediction = GetP(CurrentBallPosition, Q, minion, true);

                        if (prediction.Hitchance >= HitChance.High && Q.IsReady())
                            Q.Cast(prediction.CastPosition, packets());
                    }
                }
            }
        }

        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                Q.Range + Q.Width, MinionTypes.All);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                Q.Range + Q.Width, MinionTypes.All);

            var useQ = menu.Item("UseQFarm").GetValue<bool>();
            var useW = menu.Item("UseWFarm").GetValue<bool>();
            int min = menu.Item("qFarm").GetValue<Slider>().Value;

            int hit = 0;

            if (useQ && Q.IsReady())
            {
                foreach (Obj_AI_Base enemy in allMinionsW)
                {
                    Q.From = CurrentBallPosition;

                    MinionManager.FarmLocation pred = Q.GetCircularFarmLocation(allMinionsQ, Q.Width + 15);

                    if (pred.MinionsHit >= min)
                        Q.Cast(pred.Position, packets());
                }
            }

            hit = 0;
            if (useW && W.IsReady())
            {
                foreach (Obj_AI_Base enemy in allMinionsW)
                {
                    if (enemy.Distance(CurrentBallPosition) < W.Range)
                        hit++;
                }

                if (hit >= min && W.IsReady())
                    W.Cast();
            }
        }

        private static void Harass()
        {
            UseSpells(menu.Item("UseQHarass").GetValue<bool>(), menu.Item("UseWHarass").GetValue<bool>(),
                menu.Item("UseEHarass").GetValue<bool>(), false, "Harass");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (Spell spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if ((spell.Slot == SpellSlot.R && menuItem.Active) || (spell.Slot == SpellSlot.W && menuItem.Active))
                {
                    if(ballStatus == 0)
                        Utility.DrawCircle(Player.Position, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
                    else if(ballStatus == 2)
                        Utility.DrawCircle(allyDraw, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
                    else
                    Utility.DrawCircle(CurrentBallPosition, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
                }
                else if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
            }
            if (menu.Item("rModeDraw").GetValue<Circle>().Active)
            {
                if (menu.Item("killR").GetValue<KeyBind>().Active)
                {
                    Vector2 wts = Drawing.WorldToScreen(Player.Position);
                    Drawing.DrawText(wts[0], wts[1], Color.White, "R Multi On");
                }
                else
                {
                    Vector2 wts = Drawing.WorldToScreen(Player.Position);
                    Drawing.DrawText(wts[0], wts[1], Color.Red, "R Multi Off");
                }
            }
        }

        public static void onGainBuff()
        {
            if (Player.HasBuff("OrianaGhostSelf"))
            {
                ballStatus = 0;
                CurrentBallPosition = Player.ServerPosition;
                IsBallMoving = false;
                return;
            }

            foreach (Obj_AI_Hero ally in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(ally => ally.IsAlly && !ally.IsDead && ally.HasBuff("orianaghost", true)))
            {
                ballStatus = 2;
                CurrentBallPosition = ally.ServerPosition;
                allyDraw = ally.Position;
                IsBallMoving = false;
                return;
            }

            ballStatus = 1;
        }

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            //Shield Ally
            if (unit.IsEnemy && unit.Type == GameObjectType.obj_AI_Hero && E.IsReady())
            {
                foreach (
                    Obj_AI_Hero ally in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(x => Player.Distance(x) < E.Range&& Player.Distance(unit) < 1500 && x.IsAlly && !x.IsDead).OrderBy(x => x.Distance(args.End)))
                {
                    if (menu.Item("shield" + ally.BaseSkinName) != null)
                    {
                        if (ally != null && menu.Item("shield" + ally.BaseSkinName).GetValue<bool>())
                        {
                            int hp = menu.Item("eAllyIfHP").GetValue<Slider>().Value;
                            float hpPercent = ally.Health / ally.MaxHealth * 100;

                            if (ally.Distance(args.End) < 500 && hpPercent <= hp)
                            {
                                //Game.PrintChat("shielding");
                                E.CastOnUnit(ally, packets());
                                IsBallMoving = true;
                                return;
                            }
                        }
                    }
                }
            }
            
            //intiator
            if (unit.IsAlly)
            {
                foreach (Initiator spell in Initiator.InitatorList)
                {
                    if (args.SData.Name == spell.SDataName)
                    {
                        if (menu.Item(spell.spellName).GetValue<bool>())
                        {
                            if (E.IsReady() && Player.Distance(unit) < E.Range)
                            {
                                E.CastOnUnit(unit, packets());
                                IsBallMoving = true;
                                return;
                            }
                        }
                    }
                }
            }

            if (!unit.IsMe) return;

            SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name, false);

            if (castedSlot == SpellSlot.Q)
            {
                IsBallMoving = true;
                Utility.DelayAction.Add(
                    (int) Math.Max(1, 1000*(args.End.Distance(CurrentBallPosition) - Game.Ping - 0.1)/Q.Speed), () =>
                    {
                        CurrentBallPosition = args.End;
                        ballStatus = 1;
                        IsBallMoving = false;
                        //Game.PrintChat("Stopped");
                    });
            }
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!menu.Item("UseInt").GetValue<bool>()) return;

            if (Player.Distance(unit) < R.Range && unit != null)
            {
                castR(unit);
            }
            else
            {
                castQ(unit, "Combo");
            }
        }

        private static void Game_OnSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.C2S.Cast.Header)
            {
                Packet.C2S.Cast.Struct decodedPacket = Packet.C2S.Cast.Decoded(args.PacketData);
                if (decodedPacket.Slot == SpellSlot.R)
                {
                    if (countR() == 0 && menu.Item("blockR").GetValue<bool>())
                    {
                        //Block packet if enemies hit is 0
                        args.Process = false;
                    }
                }
            }
        }

        public static void escape()
        {
            if (ballStatus == 0 && W.IsReady())
                W.Cast();
            else if(E.IsReady() && ballStatus != 0)
                E.CastOnUnit(Player, packets());
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            onGainBuff();

            checkRMecGlobal();

            if (menu.Item("escape").GetValue<KeyBind>().Active)
            {
                escape();
            }
            else if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                checkRMec();
                Combo();
            }
            else
            {
                if (menu.Item("HarassActive").GetValue<KeyBind>().Active ||
                    menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    Farm();
                }

                if (menu.Item("LastHitQQ").GetValue<KeyBind>().Active)
                {
                    lastHit();
                }
            }

            checkWMec();
        }
    }
}