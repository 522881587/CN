using System;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;


namespace D_Corki
{
    class Program
    {
        private const string ChampionName = "Corki";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _w, _e, _r, _r1, _r2;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static Int32 _lastSkin;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 825f);
            _w = new Spell(SpellSlot.W, 800f);
            _e = new Spell(SpellSlot.E, 600f);
            _r = new Spell(SpellSlot.R);
            _r1 = new Spell(SpellSlot.R, 1300f);
            _r2 = new Spell(SpellSlot.R, 1500f);

            _q.SetSkillshot(0.3f, 250f, 1250f, false, SkillshotType.SkillshotCircle);
            _e.SetSkillshot(0f, (float) (45*Math.PI/180), 1500, false, SkillshotType.SkillshotCone);
            _r.SetSkillshot(0.20f, 40f, 2000f, true, SkillshotType.SkillshotLine);


            //D Corki
            _config = new Menu("L#涓枃绀惧尯-搴撳", "D-Corki", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("鐩爣閫夋嫨", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("璧扮爫", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "浣跨敤Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "浣跨敤W")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("UseWHE", "鍦℉P锛呬娇鐢╓").SetValue(new Slider(65, 1, 100)));
            _config.SubMenu("Combo").AddItem(new MenuItem("EnemyC", "鏁屼汉R鑼冨洿 <").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "浣跨敤E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRC", "浣跨敤R")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "杩炴嫑").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass
            _config.AddSubMenu(new Menu("楠氭壈", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "浣跨敤Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseEH", "浣跨敤E")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseRH", "浣跨敤")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("RlimH", "R鏁伴噺>").SetValue(new Slider(3, 1, 7)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "鑷姩楠氭壈(鍒囨崲)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Harass")
                .AddItem(new MenuItem("Harrasmana", "鏈€浣庢硶鍔涳紖").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "楠氭壈").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            _config.AddSubMenu(new Menu("鍙戣偛", "Farm"));
            _config.SubMenu("Farm").AddItem(new MenuItem("UseQL", "Q 娓呯嚎")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseEL", "E 娓呯嚎")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseRL", "R 娓呯嚎")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseQLH", "Q 灏惧垁")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseELH", "E 灏惧垁")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseQJ", "Q 娓呴噹")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseEJ", "E 娓呴噹")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("UseRJ", "R 娓呴噹")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("RlimL", "R鏁伴噺>").SetValue(new Slider(3, 1, 7)));
            _config.SubMenu("Farm").AddItem(new MenuItem("Lanemana", "鏈€浣庢硶鍔涳紖").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("ActiveLast", "灏惧垁").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("ActiveLane", "娓呯嚎娓呴噹").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            //Misc
            _config.AddSubMenu(new Menu("鏉傞」", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("UseQM", "浣跨敤Q鍑绘潃")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseEM", "浣跨敤E鍑绘潃")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseRM", "浣跨敤R鍑绘潃")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("skinC", "浣跨敤鐨偆").SetValue(false));
            _config.SubMenu("Misc").AddItem(new MenuItem("skinCorki", "鐨偆").SetValue(new Slider(4, 1, 7)));
            _config.SubMenu("Misc").AddItem(new MenuItem("usePackets", "浣跨敤灏佸寘")).SetValue(true);

            //Misc
            _config.AddSubMenu(new Menu("鍛戒腑鍑犵巼", "HitChance"));

            _config.SubMenu("HitChance").AddSubMenu(new Menu("楠氭壈", "Harass"));
            _config.SubMenu("HitChance").SubMenu("Harass").AddItem(new MenuItem("QchangeHar", "Q鍛戒腑").SetValue(
                new StringList(new[] {"榛樿", "涓瓑", "绮惧噯", "闈炲父绮惧噯"})));
            _config.SubMenu("HitChance").SubMenu("Harass").AddItem(new MenuItem("EchangeHar", "E鍛戒腑").SetValue(
                new StringList(new[] {"榛樿", "涓瓑", "绮惧噯", "闈炲父绮惧噯"})));
            _config.SubMenu("HitChance").SubMenu("Harass").AddItem(new MenuItem("RchangeHar", "R鍛戒腑").SetValue(
                new StringList(new[] {"榛樿", "涓瓑", "绮惧噯", "闈炲父绮惧噯"})));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("杩炴嫑", "Combo"));
            _config.SubMenu("HitChance").SubMenu("Combo").AddItem(new MenuItem("Qchange", "Q鍛戒腑").SetValue(
                new StringList(new[] {"榛樿", "涓瓑", "绮惧噯", "闈炲父绮惧噯"})));
            _config.SubMenu("HitChance").SubMenu("Combo").AddItem(new MenuItem("Echange", "E鍛戒腑").SetValue(
                new StringList(new[] {"榛樿", "涓瓑", "绮惧噯", "闈炲父绮惧噯"})));
            _config.SubMenu("HitChance").SubMenu("Combo").AddItem(new MenuItem("Rchange", "R鍛戒腑").SetValue(
                new StringList(new[] {"榛樿", "涓瓑", "绮惧噯", "闈炲父绮惧噯"})));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("鍑绘潃", "KillSteal"));
            _config.SubMenu("HitChance").SubMenu("KillSteal").AddItem(new MenuItem("Qchangekil", "Q鍛戒腑").SetValue(
                new StringList(new[] { "榛樿", "涓瓑", "绮惧噯", "闈炲父绮惧噯" })));
            _config.SubMenu("HitChance").SubMenu("KillSteal").AddItem(new MenuItem("Echangekil", "E鍛戒腑").SetValue(
                new StringList(new[] { "榛樿", "涓瓑", "绮惧噯", "闈炲父绮惧噯" })));
            _config.SubMenu("HitChance").SubMenu("KillSteal").AddItem(new MenuItem("Rchangekil", "R鍛戒腑").SetValue(
                new StringList(new[] { "榛樿", "涓瓑", "绮惧噯", "闈炲父绮惧噯" })));

            //Drawings
            _config.AddSubMenu(new Menu("鎶€鑳借寖鍥撮€夐」", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Q鑼冨洿")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "W鑼冨洿")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "E鑼冨洿")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "R鑼冨洿")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "闄嶄綆FPS").SetValue(true));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));
_config.AddSubMenu(new Menu("L#涓枃绀惧尯", "AD"));

_config.SubMenu("AD").AddItem(new MenuItem("wangzhan", "www.loll35.com"));
_config.SubMenu("AD").AddItem(new MenuItem("qunhao2", "姹夊寲缇わ細397983217"));
            _config.AddToMainMenu();
            Game.PrintChat("<font color='#881df2'>D-Corki by Diabaths</font> Loaded.");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            if (_config.Item("skinC").GetValue<bool>())
            {
                GenModelPacket(_player.ChampionName, _config.Item("skinCorki").GetValue<Slider>().Value);
                _lastSkin = _config.Item("skinCorki").GetValue<Slider>().Value;
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_player.HasBuff("CorkiMissileBarrageCounterBig"))
                _r.Range = _r2.Range;
            else
                _r.Range = _r1.Range;
            if (_config.Item("skinC").GetValue<bool>() && SkinChanged())
            {
                GenModelPacket(_player.ChampionName, _config.Item("skinCorki").GetValue<Slider>().Value);
                _lastSkin = _config.Item("skinCorki").GetValue<Slider>().Value;
            }
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if ((_config.Item("ActiveHarass").GetValue<KeyBind>().Active || _config.Item("harasstoggle").GetValue<KeyBind>().Active) && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();

            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value)
            {
                Laneclear();
                JungleClear();

            }
            if (_config.Item("ActiveLast").GetValue<KeyBind>().Active && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value)
            {
                LastHit();
            }

            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);

            KillSteal();
        }

        private static int UltiStucks()
        {
            return _player.Spellbook.GetSpell(SpellSlot.R).Ammo;
        }
        static void GenModelPacket(string champ, int skinId)
        {
            Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(_player.NetworkId, skinId, champ)).Process();
        }

        static bool SkinChanged()
        {
            return (_config.Item("skinCorki").GetValue<Slider>().Value != _lastSkin);
        }

        private static void Combo()
        {
            var useQ = _config.Item("UseQC").GetValue<bool>();
            var useW = _config.Item("UseWC").GetValue<bool>();
            var useE = _config.Item("UseEC").GetValue<bool>();
            var useR = _config.Item("UseRC").GetValue<bool>();
            var usewhE = (100 * (_player.Health / _player.MaxHealth)) > _config.Item("UseWHE").GetValue<Slider>().Value;

            if (useQ && _q.IsReady())
            {
                var t = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
                if (t != null && t.Distance(_player.Position) < _q.Range && _q.GetPrediction(t).Hitchance >= Qchangecombo())
                    _q.Cast(t, Packets(), true);
            }
            if (useW && _w.IsReady() && usewhE &&
                Utility.CountEnemysInRange(1300) <= _config.Item("EnemyC").GetValue<Slider>().Value)
            {
                var t = SimpleTs.GetTarget(_w.Range, SimpleTs.DamageType.Magical);
                if (t != null && _player.Distance(t) > 500)
                    _w.Cast(t.Position, Packets());
            }
            if (useE && _e.IsReady())
            {
                var t = SimpleTs.GetTarget(_e.Range, SimpleTs.DamageType.Magical);
                if (t != null && _player.Distance(t) < _e.Range && _e.GetPrediction(t).Hitchance >= Echangecombo())
                    _e.Cast(t, Packets(), true);
            }
            if (useR && _r.IsReady())
            {
                var t = SimpleTs.GetTarget(_r.Range, SimpleTs.DamageType.Magical);
                if (t != null && t.Distance(_player.Position) < _r.Range && _r.GetPrediction(t).Hitchance >= Rchangecombo())
                    _r.Cast(t, Packets(), true);
            }
        }

        private static void Harass()
        {
            var useQ = _config.Item("UseQH").GetValue<bool>();
            var useE = _config.Item("UseEH").GetValue<bool>();
            var useR = _config.Item("UseRH").GetValue<bool>();
            var rlimH = _config.Item("RlimH").GetValue<Slider>().Value;
            if (useQ && _q.IsReady())
            {
                var t = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
                if (t != null && t.Distance(_player.Position) < _q.Range && _q.GetPrediction(t).Hitchance >= Qchangehar())
                    _q.Cast(t, Packets(), true);
            }
            if (useE && _e.IsReady())
            {
                var t = SimpleTs.GetTarget(_e.Range, SimpleTs.DamageType.Magical);
                if (t != null && _player.Distance(t) < _e.Range && _e.GetPrediction(t).Hitchance >= Echangehar())
                    _e.Cast(t, Packets(), true);
            }
            if (useR && _r.IsReady() && rlimH < UltiStucks())
            {
                var t = SimpleTs.GetTarget(_r.Range, SimpleTs.DamageType.Magical);
                if (t != null && t.Distance(_player.Position) < _r.Range && _r.GetPrediction(t).Hitchance >= Rchangehar())
                    _r.Cast(t, Packets(), true);
            }
        }

        private static void Laneclear()
        {
            if (!Orbwalking.CanMove(40)) return;

            var rangedMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
            MinionTypes.Ranged);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range + _q.Width + 30,
            MinionTypes.All);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range, MinionTypes.All);
            var allMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _r.Range, MinionTypes.All);
            var rangedMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _r.Range + _r.Width,
            MinionTypes.Ranged);
            var useQl = _config.Item("UseQL").GetValue<bool>();
            var useEl = _config.Item("UseEL").GetValue<bool>();
            var useRl = _config.Item("UseRL").GetValue<bool>();
            var rlimL = _config.Item("RlimL").GetValue<Slider>().Value;
            if (_q.IsReady() && useQl)
            {
                var fl1 = _q.GetCircularFarmLocation(rangedMinionsQ, _q.Width);
                var fl2 = _q.GetCircularFarmLocation(allMinionsQ, _q.Width);

                if (fl1.MinionsHit >= 3)
                {
                    _q.Cast(fl1.Position);
                }
                else if (fl2.MinionsHit >= 2 || allMinionsQ.Count == 1)
                {
                    _q.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                        minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                            _q.Cast(minion);
            }
            if (_e.IsReady() && useEl)
            {
                var fl2 = _w.GetLineFarmLocation(allMinionsE, _e.Width);

                if (fl2.MinionsHit >= 2 || allMinionsE.Count == 1)
                {
                    _e.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsE)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                        minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.E))
                            _e.Cast(minion);
            }
            if (_r.IsReady() && useRl && rlimL < UltiStucks() && allMinionsR.Count > 3 )
            {
                var fl1 = _w.GetLineFarmLocation(rangedMinionsR, _r.Width);
                var fl2 = _w.GetLineFarmLocation(allMinionsR, _r.Width);

                if (fl1.MinionsHit >= 3)
                {
                    _r.Cast(fl1.Position);
                }
                else if (fl2.MinionsHit >= 2 || allMinionsR.Count == 1)
                {
                    _r.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsR)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                        minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.R))
                            _r.Cast(minion);
            }
        }

        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLH").GetValue<bool>();
            var useE = _config.Item("UseELH").GetValue<bool>();
            if (allMinions.Count < 3) return;
            foreach (var minion in allMinions)
            {
                if (useQ && _q.IsReady() && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    _q.Cast(minion);
                }

                if (_w.IsReady() && useE && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.E))
                {
                    _e.Cast(minion);
                }
            }
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useQ = _config.Item("UseQJ").GetValue<bool>();
            var useE = _config.Item("UseEJ").GetValue<bool>();
            var useR = _config.Item("UseRJ").GetValue<bool>();
            var rlimL = _config.Item("RlimL").GetValue<Slider>().Value;
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useQ && _q.IsReady())
                {
                    _q.Cast(mob, Packets());
                }
                if (_e.IsReady() && useE)
                {
                    _e.Cast(mob, Packets());
                }
                if (_r.IsReady() && useR && rlimL < UltiStucks())
                {
                    _r.Cast(mob, Packets());
                }
            }
        }

        private static bool Packets()
        {
            return _config.Item("usePackets").GetValue<bool>();
        }
        private static bool HasBigRocket()
        {
            return ObjectManager.Player.Buffs.Any(buff => buff.DisplayName.ToLower() == "corkimissilebarragecounterbig");
        }

        private static void KillSteal()
        {
            if (_q.IsReady() && _config.Item("UseQM").GetValue<bool>())
            {
                var t = SimpleTs.GetTarget(_q.Range, SimpleTs.DamageType.Magical);
                if (_q.GetDamage(t) > t.Health && _player.Distance(t) <= _q.Range && _q.GetPrediction(t).Hitchance >= Qchangekil())
                    _q.Cast(t, Packets(), true);
            }
            if (_e.IsReady() && _config.Item("UseEM").GetValue<bool>())
            {
                var t = SimpleTs.GetTarget(_w.Range, SimpleTs.DamageType.Magical);
                if (_e.GetDamage(t) > t.Health && _player.Distance(t) <= _e.Range && _e.GetPrediction(t).Hitchance >= Echangekil())
                    _e.Cast(t, Packets(), true);
            }
            if (_r.IsReady() && _config.Item("UseRM").GetValue<bool>())
            {
                var t = SimpleTs.GetTarget(_r.Range, SimpleTs.DamageType.Magical);
                var bigRocket = HasBigRocket();
                foreach (
                    var hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                hero =>
                                    hero.IsValidTarget(bigRocket ? _r2.Range : _r1.Range) &&
                                    _r1.GetDamage(hero) * (bigRocket ? 1.5f : 1f) > hero.Health))
                                    if (_r.GetPrediction(t).Hitchance >= Rchangekil())
                    _r.Cast(t, Packets(), true);
            }
        }

        private static HitChance Qchangecombo()
        {
            switch (_config.Item("Qchange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance Echangecombo()
        {
            switch (_config.Item("Echange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance Rchangecombo()
        {
            switch (_config.Item("Rchange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }
        private static HitChance Qchangehar()
        {
            switch (_config.Item("QchangeHar").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }

        private static HitChance Echangehar()
        {
            switch (_config.Item("EchangeHar").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }

        private static HitChance Rchangehar()
        {
            switch (_config.Item("RchangeHar").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }
        private static HitChance Qchangekil()
        {
            switch (_config.Item("Qchangekil").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Low;
            }
        }

        private static HitChance Echangekil()
        {
            switch (_config.Item("Echangekil").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Low;
            }
        }

        private static HitChance Rchangekil()
        {
            switch (_config.Item("Rchangekil").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Low;
            }
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_config.Item("CircleLag").GetValue<bool>())
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
                }

                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.White);
                }

            }
        }
    }
}