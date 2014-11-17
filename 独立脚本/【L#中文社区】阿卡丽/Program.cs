using System;
using System.Collections.Generic;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using Color = System.Drawing.Color;

namespace RoyalAkali
{
    //TODO
    /*
     * Use W if < % HP and 鈩?enemies around //Panic mode - CHECK
     * Use W for bush vision(configure ward\W) //WIP
     * Smart R killsteal
     *      To remove - Dont dive with ulti under towers unless you can kill enemy with R so you could get out with the stack you gain
    */
    class Program
    {
        //////////////////////////////
        static readonly Obj_AI_Hero player = ObjectManager.Player;
        static readonly string localVersion = "1.07";

        static Menu menu = new Menu("Akali", "Akali", true);

        static Spell E;
        static Spell Q;
        static Spell R;
        static Spell W;
        static SpellSlot IgniteSlot = player.GetSpellSlot("SummonerDot");
        static bool packetCast = false;

        static Obj_AI_Hero rektmate = default(Obj_AI_Hero);
        static float assignTime = 0f;

        static List<Spell> SpellList;
        //////////////////////////////

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        static void OnGameLoad(EventArgs args)
        {
            if (player.ChampionName != "Akali")
                return;

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 800);

            SpellList = new List<Spell>() { Q, W, E, R };

            try
            {
                LoadMenu();
            }
            catch (Exception ex)
            {
                Game.PrintChat("Mistake occurred when loading menu");
            }

            UpdateChecks();
            Console.WriteLine("\a \a \a");
            Drawing.OnDraw += OnDraw;
            Game.OnGameUpdate += OnUpdate;
            //Obj_AI_Hero.OnProcessSpellCast += OnCast;
        }

        //TODO: Remove
        static void OnCast(LeagueSharp.Obj_AI_Base sender, LeagueSharp.GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsEnemy) return;
            if (args.SData.Name == "TrinketTotemLvl3B" || args.SData.Name == "VisionWard" && menu.SubMenu("misc").Item("antipink").GetValue<bool>())
            {
                if (args.End.Distance(player.Position) < 1200) 
                    Utility.DelayAction.Add(200, () => AntiPink(args.End));
            }
        }
        //

        static void OnUpdate(EventArgs args)
        {
            packetCast = menu.Item("packets").GetValue<bool>();
            LXOrbwalker.SetAttack(true);
            if(menu.Item("RKillsteal").GetValue<bool>())
                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    if (enemy.IsEnemy && enemy.Distance(player) <= R.Range && player.GetSpellDamage(enemy, SpellSlot.R) > enemy.Health && ultiCount() > 0 && R.IsReady())
                        R.CastOnUnit(enemy);

            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    RapeTime();
                    break;

                case LXOrbwalker.Mode.Harass:
                    if (menu.SubMenu("harass").Item("useQ").GetValue<bool>())
                        CastQ(true);
                    if (menu.SubMenu("harass").Item("useE").GetValue<bool>())
                        CastE(true);
                    break;

                case LXOrbwalker.Mode.LaneClear:
                    if (menu.SubMenu("laneclear").Item("useQ").GetValue<bool>())
                        CastQ(false);
                    if (menu.SubMenu("laneclear").Item("useE").GetValue<bool>())
                        CastE(false);
                    break;
            }
            if (menu.SubMenu("misc").Item("escape").GetValue<KeyBind>().Active) Escape();
        }

        private static void OnDraw(EventArgs args)
        {
            if (menu.SubMenu("misc").Item("escape").GetValue<KeyBind>().Active)
            {
                Utility.DrawCircle(Game.CursorPos, 200, W.IsReady() ? Color.Blue : Color.Red, 3);
                Utility.DrawCircle(player.Position, R.Range, menu.Item("Rrange").GetValue<Circle>().Color, 13);
            }
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(player.Position, spell.Range, menuItem.Color);
            }
            if (menu.SubMenu("drawings").Item("RAPE").GetValue<bool>() && rektmate != default(Obj_AI_Hero))
                Utility.DrawCircle(rektmate.Position, 70, Color.ForestGreen, 8);
            /*
            Drawing.DrawLine(Drawing.WorldToScreen(debugTarget), Drawing.WorldToScreen(debugJump), 3, Color.AliceBlue);
            Drawing.DrawLine(Drawing.WorldToScreen(debugTarget), Drawing.WorldToScreen(debugPlayer), 3, Color.Aquamarine);
            Drawing.DrawText(Drawing.WorldToScreen(debugTarget).X, Drawing.WorldToScreen(debugTarget).Y, Color.PowderBlue, debugTargetDist.ToString());
            Drawing.DrawText(Drawing.WorldToScreen(debugJump).X, Drawing.WorldToScreen(debugJump).Y, Color.PowderBlue, debugJumpDist.ToString());
            */
        }

        static void AntiPink(Vector3 position)
        {
            float pd = player.Distance(position);
            foreach (var item in player.InventoryItems)
                switch (item.Name)
                {
                    case "TrinketSweeperLvl1":
                        if (pd < 800)
                            item.UseItem(V2E(player.Position, position, 400).To3D());
                        break;
                    case "TrinketSweeperLvl2":
                        if (pd < 1200)
                            item.UseItem(V2E(player.Position, position, 600).To3D());
                        break;
                    case "TrinketSweeperLvl3":
                        if (pd < 1200)
                            item.UseItem(V2E(player.Position, position, 600).To3D());
                        break;
                }
        }

        private static void CastQ(bool mode)
        {
            if (!Q.IsReady()) return;
            if (mode)
            {
                Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
                if (!target.IsValidTarget(Q.Range)) return;
                Q.Cast(target);
            }
            else
            {
                foreach (
                    Obj_AI_Base minion in
                        MinionManager.GetMinions(player.Position, Q.Range, MinionTypes.All, MinionTeam.Enemy,
                            MinionOrderTypes.Health))
                    if (HasBuff(minion, "AkaliMota") &&
                        Orbwalking.GetRealAutoAttackRange(player) >= player.Distance(minion))
                        LXOrbwalker.ForcedTarget = minion;

                foreach (
                    Obj_AI_Base minion in
                        MinionManager.GetMinions(player.Position, Q.Range, MinionTypes.All, MinionTeam.Enemy,
                            MinionOrderTypes.Health))
                    if (
                        HealthPrediction.GetHealthPrediction(minion,
                            (int) (E.Delay + (minion.Distance(player)/E.Speed))*1000) <
                        player.GetSpellDamage(minion, SpellSlot.Q) &&
                        HealthPrediction.GetHealthPrediction(minion,
                            (int) (E.Delay + (minion.Distance(player)/E.Speed))*1000) > 0 &&
                        player.Distance(minion) > Orbwalking.GetRealAutoAttackRange(player))
                        Q.Cast(minion);

                foreach (Obj_AI_Base minion in MinionManager.GetMinions(player.ServerPosition, Q.Range,
                    MinionTypes.All,
                    MinionTeam.Neutral, MinionOrderTypes.MaxHealth))
                    if (player.Distance(minion) <= Q.Range)
                        Q.Cast(minion);


            }
        }

        static void CastE(bool mode)
        {
            if (!E.IsReady()) return;
            if (mode)
            {
                Obj_AI_Hero target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
                if (target == null || !target.IsValidTarget(E.Range)) return;
                if (HasBuff(target, "AkaliMota") && !E.IsReady() && Orbwalking.GetRealAutoAttackRange(player) >= player.Distance(target))
                    LXOrbwalker.ForcedTarget = target;
                else
                    E.Cast();
            }
            else
            {   //Minions in E range                                                                            >= Value in menu
                if (MinionManager.GetMinions(player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy).Count >= menu.SubMenu("laneclear").Item("hitCounter").GetValue<Slider>().Value) E.Cast();
                foreach (Obj_AI_Base minion in MinionManager.GetMinions(player.ServerPosition, Q.Range,
                      MinionTypes.All,
                      MinionTeam.Neutral, MinionOrderTypes.MaxHealth))
                    if (player.Distance(minion) <= E.Range)
                        E.Cast();
            }
        }

        static void RapeTime()
        {
            Obj_AI_Hero possibleVictim = SimpleTs.GetTarget(R.Range * 2 + Orbwalking.GetRealAutoAttackRange(player), SimpleTs.DamageType.Magical);
            try
            {
                if (rektmate.IsDead || Game.Time - assignTime > 1.5)
                {
                    //Console.WriteLine("Unassign - " + rektmate.ChampionName + " dead: " + rektmate.IsDead + "\n\n");
                    rektmate = default(Obj_AI_Hero);
                }
            }
            catch (Exception ex) { }
            try
            {
                if (rektmate == default(Obj_AI_Hero) && IsRapeble(possibleVictim) > possibleVictim.Health)
                {
                    rektmate = possibleVictim;
                    assignTime = Game.Time;
                    //Console.WriteLine("Assign - " + rektmate.ChampionName + " time: " + assignTime+"\n\n");
                }
            }
            catch (Exception ex) { }
            if (rektmate != default(Obj_AI_Hero))
            {
                //!(menu.SubMenu("misc").Item("TowerDive").GetValue<Slider>().Value < player.Health/player.MaxHealth && Utility.UnderTurret(rektmate, true)) && 
                if (player.Distance(rektmate) < R.Range * 2 + Orbwalking.GetRealAutoAttackRange(player) && player.Distance(rektmate) > Q.Range)
                    CastR(rektmate.Position);
                else if (player.Distance(rektmate) < Q.Range)
                    RaperinoCasterino(rektmate);
                else rektmate = default(Obj_AI_Hero);//Target is out of range. Unassign.
            }
            else
            {
                LXOrbwalker.SetAttack(!Q.IsReady() && !E.IsReady());
                if (menu.SubMenu("combo").Item("useQ").GetValue<bool>())
                    CastQ(true);
                if (menu.SubMenu("combo").Item("useE").GetValue<bool>())
                    CastE(true);
                if (menu.SubMenu("combo").Item("useW").GetValue<bool>())
                    CastW();
                if (menu.SubMenu("combo").Item("useR").GetValue<bool>())
                {
                    Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
                    if ((target.IsValidTarget(R.Range) && target.Distance(player) > Orbwalking.GetRealAutoAttackRange(player)) || R.IsKillable(target))
                        R.Cast(target, packetCast);
                }
            }
        }

        static void CastW()
        {
            //
            //menu.SubMenu("misc").AddItem(new MenuItem("PanicW", "In combo if 鈩?of enemies around").SetValue(new Slider(0, 0, 5)));
            //menu.SubMenu("misc").AddItem(new MenuItem("PanicWN", "In combo in %HP < ").SetValue(new Slider(25, 0, 100)));
            //
            byte enemiesAround = 0;
            foreach(Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                if(enemy.IsEnemy && enemy.Distance(player) < 400) enemiesAround++;
            if (menu.Item("PanicW").GetValue<Slider>().Value > enemiesAround && menu.Item("PanicWN").GetValue<Slider>().Value < (int)(player.Health / player.MaxHealth * 100))
                return;
            W.Cast(player.Position, packetCast);
        }

        static void RaperinoCasterino(Obj_AI_Hero victim)
        {
            LXOrbwalker.SetAttack(!Q.IsReady() && !E.IsReady() && player.Distance(victim) < 800f);
            LXOrbwalker.ForcedTarget = victim;
            foreach (var item in player.InventoryItems)
                switch ((int)item.Id)
                {
                    case 3144:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready) item.UseItem(victim);
                        break;
                    case 3146:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready) item.UseItem(victim);
                        break;
                    case 3128:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready) item.UseItem(victim);
                        break;
                }
            if (Q.IsReady() && Q.InRange(victim.Position) && !HasBuff(victim, "AkaliMota")) Q.Cast(victim, packetCast);
            if (E.IsReady() && E.InRange(victim.Position)) E.Cast();
            if (W.IsReady() && W.InRange(victim.Position) && !(HasBuff(victim, "AkaliMota") && player.Distance(victim) > Orbwalking.GetRealAutoAttackRange(player))) W.Cast(V2E(player.Position, victim.Position, player.Distance(victim) + W.Width * 2 - 20), packetCast);
            if (R.IsReady() && R.InRange(victim.Position)) R.Cast(victim, packetCast);
            if (IgniteSlot != SpellSlot.Unknown && player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready) player.SummonerSpellbook.CastSpell(IgniteSlot, victim);
        }

        static double IsRapeble(Obj_AI_Hero victim)
        {
            int UC = ultiCount();
            int jumpCount = (UC - (int)(victim.Distance(player.Position) / R.Range));
            double comboDamage = 0d;
            if (Q.IsReady()) comboDamage += player.GetSpellDamage(victim, SpellSlot.Q) + player.CalcDamage(victim, Damage.DamageType.Magical, (45 + 35 * Q.Level + 0.5 * player.FlatMagicDamageMod));
            if (E.IsReady()) comboDamage += player.GetSpellDamage(victim, SpellSlot.E);

            if (HasBuff(victim, "AkaliMota")) comboDamage += player.CalcDamage(victim, Damage.DamageType.Magical, (45 + 35 * Q.Level + 0.5 * player.FlatMagicDamageMod));
            //comboDamage += player.GetAutoAttackDamage(victim, true);

            comboDamage += player.CalcDamage(victim, Damage.DamageType.Magical, CalcPassiveDmg());
            comboDamage += player.CalcDamage(victim, Damage.DamageType.Magical, CalcItemsDmg(victim));

            foreach (var item in player.InventoryItems)
                if ((int)item.Id == 3128)
                    if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                        comboDamage *= 1.2;
            if (HasBuff(victim, "deathfiregraspspell")) comboDamage *= 1.2;

            if (UC > 0) comboDamage += jumpCount > 0 ? player.GetSpellDamage(victim, SpellSlot.R) * jumpCount : player.GetSpellDamage(victim, SpellSlot.R);
            if (IgniteSlot != SpellSlot.Unknown && player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                comboDamage += ObjectManager.Player.GetSummonerSpellDamage(victim, Damage.SummonerSpell.Ignite);
            return comboDamage;
        }

        static double CalcPassiveDmg()
        {
            return (0.06 + 0.01 * (player.FlatMagicDamageMod / 6)) * (player.FlatPhysicalDamageMod + player.BaseAttackDamage);
        }

        static double CalcItemsDmg(Obj_AI_Hero victim)
        {
            double result = 0d;
            foreach (var item in player.InventoryItems)
                switch ((int)item.Id)
                {
                    case 3100: // LichBane
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            result += player.BaseAttackDamage * 0.75 + player.FlatMagicDamageMod * 0.5;
                        break;
                    case 3057://Sheen
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            result += player.BaseAttackDamage;
                        break;
                    case 3144:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            result += 100d;
                        break;
                    case 3146:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            result += 150d + player.FlatMagicDamageMod * 0.4;
                        break;
                    case 3128:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            result += victim.MaxHealth * 0.15;
                        break;
                }

            return result;
        }

        static void Escape()
        {
            Vector3 cursorPos = Game.CursorPos;
            Vector2 pos = V2E(player.Position, cursorPos, R.Range);
            Vector2 pass = V2E(player.Position, cursorPos, 120);
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(pass.X, pass.Y)).Send();
            if (menu.SubMenu("misc").Item("RCounter").GetValue<Slider>().Value > ultiCount()) return;
            if (!IsWall(pos) && IsPassWall(player.Position, pos.To3D()) && MinionManager.GetMinions(cursorPos, 300, MinionTypes.All, MinionTeam.NotAlly).Count < 1)
                if (W.IsReady()) W.Cast(V2E(player.Position, cursorPos, W.Range));
            CastR(cursorPos, true);
        }

        static void CastR(Vector3 position, bool mouseJump = false)
        {
            Obj_AI_Base target = player;
            foreach (Obj_AI_Base minion in ObjectManager.Get<Obj_AI_Base>())
                if (minion.IsValidTarget(R.Range, true) && player.Distance(position, true) > minion.Distance(position, true) && minion.Distance(position, true) < target.Distance(position, true))
                    if (mouseJump)
                    {
                        if (minion.Distance(position) < 200)
                            target = minion;
                    }
                    else 
                        target = minion;
            if (R.IsReady() && R.InRange(target.Position) && !target.IsMe)
                if (mouseJump && target.Distance(position) < 200)
                        R.CastOnUnit(target, packetCast);
                else if (player.Distance(position, true) > target.Distance(position, true))
                    R.CastOnUnit(target, packetCast);

        }

        static bool IsPassWall(Vector3 start, Vector3 end)
        {
            double count = Vector3.Distance(start, end);
            for (uint i = 0; i <= count; i += 10)
            {
                Vector2 pos = V2E(start, end, i);
                if (IsWall(pos)) return true;
            }
            return false;
        }

        static int ultiCount()
        {
            foreach (BuffInstance buff in player.Buffs)
                if (buff.Name == "AkaliShadowDance")
                    return buff.Count;
            return 0;
        }

        static bool IsWall(Vector2 pos)
        {
            return (NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Wall ||
                    NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Building);
        }

        static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
        {
            return from.To2D() + distance * Vector3.Normalize(direction - from).To2D();
        }
        static bool HasBuff(Obj_AI_Base target, string buffName)
        {
            foreach (BuffInstance buff in target.Buffs)
                if (buff.Name == buffName) return true;
            return false;
        }

        static bool ableToGapclose(Obj_AI_Base target)
        {

            return false;
        }

        static void LoadMenu()
        {
            /*
            Menu targetSelector = new Menu("Target Selector", "ts");
            SimpleTs.AddToMenu(targetSelector);
            menu.AddSubMenu(targetSelector);
            */

            Menu SOW = new Menu("璧扮爫", "orbwalker");
            menu.AddSubMenu(SOW);
            LXOrbwalker.AddToMenu(SOW);

            menu.AddSubMenu(new Menu("杩炴嫑", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQ", "浣跨敤Q").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useW", "浣跨敤W").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useE", "浣跨敤E").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useR", "浣跨敤R").SetValue(true));

            menu.AddSubMenu(new Menu("楠氭壈", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQ", "浣跨敤Q").SetValue(false));
            menu.SubMenu("harass").AddItem(new MenuItem("useE", "浣跨敤E").SetValue(true));

            menu.AddSubMenu(new Menu("娓呯嚎", "laneclear"));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQ", "浣跨敤Q琛ュ叺").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useE", "浣跨敤E娓呯嚎").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("hitCounter", "X灏忓叺浣跨敤E").SetValue(new Slider(3, 1, 6)));

            menu.AddSubMenu(new Menu("鏉傞」", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("0", "澶ф嫑:"));
            menu.SubMenu("misc").AddItem(new MenuItem("escape", "閫冭窇").SetValue(new KeyBind('G', KeyBindType.Press)));
            menu.SubMenu("misc").AddItem(new MenuItem("RCounter", "R>X 涓嶉€冭窇").SetValue(new Slider(1, 1, 3)));
            menu.SubMenu("misc").AddItem(new MenuItem("RKillsteal", "鐢≧鍑绘潃").SetValue(false));
            menu.SubMenu("misc").AddItem(new MenuItem("1", "W"));
            menu.SubMenu("misc").AddItem(new MenuItem("PanicW", "鏁屼汉>X").SetValue(new Slider(1, 1, 5)));
            menu.SubMenu("misc").AddItem(new MenuItem("PanicWN", "HP<X%").SetValue(new Slider(25, 0, 100)));
            menu.SubMenu("misc").AddItem(new MenuItem("2", "鍏朵粬"));
            menu.SubMenu("misc").AddItem(new MenuItem("packets", "浣跨敤灏佸寘").SetValue(true));
            //menu.SubMenu("misc").AddItem(new MenuItem("antipink", "Cast red trinket to diasble enemy pink").SetValue(true));

			
				
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "鏄剧ず浼ゅ").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit += hero => (float)IsRapeble(hero);
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            Menu drawings = new Menu("鏄剧ず", "drawings");
            menu.AddSubMenu(drawings);
            drawings.AddItem(new MenuItem("Qrange", "Q鑼冨洿").SetValue(new Circle(true, Color.FromArgb(150, Color.IndianRed))));
            drawings.AddItem(new MenuItem("Wrange", "W鑼冨洿").SetValue(new Circle(true, Color.FromArgb(150, Color.IndianRed))));
            drawings.AddItem(new MenuItem("Erange", "E鑼冨洿").SetValue(new Circle(false, Color.FromArgb(150, Color.DarkRed))));
            drawings.AddItem(new MenuItem("Rrange", "R鑼冨洿").SetValue(new Circle(false, Color.FromArgb(150, Color.DarkRed))));
            drawings.AddItem(new MenuItem("RAPE", "鏄剧ず鍙潃鐩爣").SetValue<bool>(true));
            drawings.AddItem(dmgAfterComboItem);

			menu.AddSubMenu(new Menu("L#涓枃绀惧尯", "AD"));
				menu.SubMenu("AD").AddItem(new MenuItem("WANGZHAN", "www.loll35.com"));
				menu.SubMenu("AD").AddItem(new MenuItem("qunhao", "姹夊寲缇わ細397983217"));
				
            menu.AddToMainMenu();
        }

        static void UpdateChecks()
        {
            Game.PrintChat("-----------------------");
            WebClient client = new WebClient();
            string version = client.DownloadString("https://raw.github.com/princer007/LeagueSharp/master/RoyalRapistAkali/version");
            if (version.Remove(4).Equals(localVersion))
                Game.PrintChat("== 鑴氭湰鏃犻渶鏇存柊! ==");
            else
                Game.PrintChat("== 鑴氭湰宸茬粡鏇存柊璇蜂笅杞芥柊鐨勮剼鏈瑋 ==");

            Utility.DelayAction.Add(300, () => Game.PrintChat("-----------------------"));
            Utility.DelayAction.Add(100, () => Game.PrintChat("Akali by princer007 Loaded. "));
        }
    }
}
