using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace KurisuNidalee
{
    /*  _____ _   _     _         
     * |   | |_|_| |___| |___ ___ 
     * | | | | | . | .'| | -_| -_|
     * |_|___|_|___|__,|_|___|___|
     * 
     * Revison 106-3 30/10/2014
     * + Can change hitchance in menu now
     * 
     * Revision 106-2 21/10/2014
     * + Spellchecks
     * + Fixed some spells casting with packets even when
     *   setting was off.
     * + Added enable/disable healengine
     * 
     * Revision: 106-1 16/10/2014
     * + Fixed autoheal healing when recalling
     * 
     * Revision: 106 - 11/10/2014
     * + Hitchance now adjusts based on range
     * + Lag free drawings
     * 
     * Revision: 105 - 09/30/2014
     * + DamageLib update
     * + Aspect of Cougar tweaks
     * 
     * Revision: 104 - 09/27/2014
     * + Added frost queens claims
     * + New Laneclear method
     * 
     * Revision: 103 - 09/24/2014
     * + HealEngine added
     * + Tweaks and Optimization
     * 
     * Revision: 102 - 09/24/2014
     * + Killsteal prediction fix
     * 
     * Revision: 100 - 09/24/2014
     * + Beta Release
     */

    internal class KurisuNidalee
    {
        public KurisuNidalee()
        {          
            Console.WriteLine("Kurisu assembly is loading...");
            CustomEvents.Game.OnGameLoad += Initialize;
        }

        #region Nidalee: Properties
        private static Menu Config;
        private static Obj_AI_Base Target;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;
        private static Orbwalking.Orbwalker Orbwalker;
        private static bool Kitty;

        private static Spell javelin = new Spell(SpellSlot.Q, 1500f);
        private static Spell bushwack = new Spell(SpellSlot.W, 900f);
        private static Spell primalsurge = new Spell(SpellSlot.E, 650f);
        private static Spell takedown = new Spell(SpellSlot.Q, 200f);
        private static Spell pounce = new Spell(SpellSlot.W, 375f);
        private static Spell swipe = new Spell(SpellSlot.E, 300f);
        private static Spell aspectofcougar = new Spell(SpellSlot.R, float.MaxValue);

        private static readonly SpellDataInst spellData = Me.Spellbook.GetSpell(SpellSlot.Q);
        private static readonly List<Spell> cougarList = new List<Spell>();
        private static readonly List<Spell> humanList = new List<Spell>();

        private static bool Packets() { return Config.Item("usepackets").GetValue<bool>(); }
        private static bool TargetHunted(Obj_AI_Base target) { return target.HasBuff("nidaleepassivehunted", true); }
        private static readonly string[] JungleMinions =
        {
            "AncientGolem", "GreatWraith", "Wraith", "LizardElder", "Golem", "Worm", "Dragon", "GiantWolf" 
        
        };

        #endregion

        #region Nidalee: Initialize
        private void Initialize(EventArgs args)
        {
            if (Me.BaseSkinName != "Nidalee") return;
            NidaMenu();

            cougarList.AddRange(new[] { takedown, pounce, swipe });
            humanList.AddRange(new[] { javelin, bushwack, primalsurge });

            javelin.SetSkillshot(0.50f, 70f, 1300f, true, SkillshotType.SkillshotLine);
            bushwack.SetSkillshot(0.50f, 100f, 1500f, false, SkillshotType.SkillshotCircle);
            swipe.SetSkillshot(0.50f, 375f, 1500f, false, SkillshotType.SkillshotCone);
            pounce.SetSkillshot(0.50f, 400f, 1500f, false, SkillshotType.SkillshotCone);

            Game.OnGameUpdate += NidaleeOnUpdate;
            Drawing.OnDraw += NidaleeOnDraw;
            Obj_AI_Base.OnProcessSpellCast += NidaleeTracker;
        }
        #endregion

        #region Nidalee: Menu
        private void NidaMenu()
        {
            Config = new Menu("|L#涓枃绀惧尯-濂堝痉涓絴", "nidalee", true);

            var nidaOrb = new Menu("璧扮爫", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(nidaOrb);
            Config.AddSubMenu(nidaOrb);

            var nidaTS = new Menu("鐩爣閫夋嫨", "target selecter");
            SimpleTs.AddToMenu(nidaTS);
            Config.AddSubMenu(nidaTS);

            var nidaKeys = new Menu("鐑敭", "keybindongs");
            nidaKeys.AddItem(new MenuItem("usecombo", "杩炴嫑")).SetValue(new KeyBind(32, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("useharass", "楠氭壈")).SetValue(new KeyBind(67, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("usejungle", "娓呴噹")).SetValue(new KeyBind(86, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("useclear", "娓呯嚎")).SetValue(new KeyBind(86, KeyBindType.Press));
            Config.AddSubMenu(nidaKeys);

            var nidaSpells = new Menu("娉曟湳", "spells");
            nidaSpells.AddItem(new MenuItem("hitchance", "鍛戒腑鏈轰細")).SetValue(new StringList(new[] { "Low", "Medium", "High" }, 2));
            nidaSpells.AddItem(new MenuItem("usehumanq", "浣跨敤浜哄舰鎬丵")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usehumanw", "浣跨敤浜哄舰鎬乄")).SetValue(true);
            nidaSpells.AddItem(new MenuItem(" ", " "));
            nidaSpells.AddItem(new MenuItem("usecougarq", "浣跨敤璞瑰舰鎬丵")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougarw", "浣跨敤璞瑰舰鎬乄")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("pouncerange", "鏈€灏忚窛绂汇劎")).SetValue(new Slider(125, 50, 300));
            nidaSpells.AddItem(new MenuItem("usecougare", "浣跨敤璞瑰舰鎬丒")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougarr", "鑷姩鍒囨崲")).SetValue(true);
            Config.AddSubMenu(nidaSpells);

            var nidaHeals = new Menu("浜哄舰鎬丒", "hengine");
            nidaHeals.AddItem(new MenuItem("usedemheals", "鎵撳紑")).SetValue(true);
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                nidaHeals.AddItem(new MenuItem("heal" + hero.SkinName, hero.SkinName)).SetValue(true);
                nidaHeals.AddItem(new MenuItem("healpct" + hero.SkinName, hero.SkinName + " heal %")).SetValue(new Slider(50));
            }
            nidaHeals.AddItem(new MenuItem("healmanapct", "鏈€灏忔硶鍔涘€笺劎")).SetValue(new Slider(40));
            Config.AddSubMenu(nidaHeals);


            var nidaHarass = new Menu("楠氭壈", "harass");
            nidaHarass.AddItem(new MenuItem("usehumanq2", "浣跨敤浜哄舰鎬丵")).SetValue(true);
            nidaHarass.AddItem(new MenuItem("humanqpct", "鏈€灏忔硶鍔涘€笺劎")).SetValue(new Slider(70));
            Config.AddSubMenu(nidaHarass);

            var nidaClear = new Menu("娓呯嚎", "laneclear");
            nidaClear.AddItem(new MenuItem("clearhumanq", "浣跨敤浜哄舰鎬丵")).SetValue(false);
            nidaClear.AddItem(new MenuItem(" ", " "));
            nidaClear.AddItem(new MenuItem("clearcougarq", "浣跨敤璞瑰舰鎬丵")).SetValue(true);
            nidaClear.AddItem(new MenuItem("clearcougarw", "浣跨敤璞瑰舰鎬乄")).SetValue(true);
            nidaClear.AddItem(new MenuItem("clearcougare", "浣跨敤璞瑰舰鎬丒")).SetValue(true);
            nidaClear.AddItem(new MenuItem("clearcougarr", "鑷姩鍒囨崲")).SetValue(false);
            nidaClear.AddItem(new MenuItem("clearpct", "鏈€灏忔硶鍔涘€笺劎")).SetValue(new Slider(55));
            Config.AddSubMenu(nidaClear);

            var nidaJungle = new Menu("娓呴噹", "jungleclear");
            nidaJungle.AddItem(new MenuItem("jghumanq", "浣跨敤浜哄舰鎬丵")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jghumanw", "浣跨敤浜哄舰鎬乄")).SetValue(true);
            nidaJungle.AddItem(new MenuItem(" ", " "));
            nidaJungle.AddItem(new MenuItem("jgcougarq", "浣跨敤璞瑰舰鎬丵")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougarw", "浣跨敤璞瑰舰鎬乄")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougare", "浣跨敤璞瑰舰鎬丒")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougarr", "鑷姩鍒囨崲")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgrpct", "鏈€灏忔硶鍔涘€笺劎")).SetValue(new Slider(55, 0, 100));
            Config.AddSubMenu(nidaJungle);

            var nidaMisc = new Menu("鏉傞」", "nidamisc");
            nidaMisc.AddItem(new MenuItem("usedfg", "浣跨敤鍐ョ伀")).SetValue(true);
            nidaMisc.AddItem(new MenuItem("usebork", "浣跨敤鐮磋触")).SetValue(true);
            nidaMisc.AddItem(new MenuItem("usebw", "浣跨敤灏忓集鍒€")).SetValue(true);
            nidaMisc.AddItem(new MenuItem("useclaim", "浣跨敤鍐伴湝濂崇殗")).SetValue(true);
            nidaMisc.AddItem(new MenuItem("useks", "鎶㈠ご")).SetValue(true);
            nidaMisc.AddItem(new MenuItem("swfks", "浜鸿惫鍒囨崲鎶㈠ご")).SetValue(false);
            Config.AddSubMenu(nidaMisc);

            var nidaD = new Menu("鏄剧ず", "drawings");
            nidaD.AddItem(new MenuItem("drawQ", "Q鑼冨洿")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawW", "W鑼冨洿")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawE", "E鑼冨洿")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawcds", "鏄剧ず鍐峰嵈")).SetValue(true);
            Config.AddSubMenu(nidaD);

            Config.AddItem(new MenuItem("useignote", "浣跨敤鐐圭噧")).SetValue(true);
            Config.AddItem(new MenuItem("usepackets", "浣跨敤灏佸寘")).SetValue(true);
            Config.AddToMainMenu();
			
			
			Config.AddSubMenu(new Menu("L#涓枃绀惧尯", "AD"));
				Config.SubMenu("AD").AddItem(new MenuItem("wangzhan", "www.loll35.com"));
				Config.SubMenu("AD").AddItem(new MenuItem("qunhao2", "缇ゅ彿锛?397983217"));

            Game.PrintChat("<font color=\"#FFAF4D\">[</font><font color=\"#FFA333\">Nidalee</font><font color=\"#FFAF4D\">]</font><font color=\"#FF8C00\"> - <u>the Bestial Huntress Rev106</u>  </font>- Kurisu");

        }
        #endregion

        #region Nidalee: OnTick
        private void NidaleeOnUpdate(EventArgs args)
        {
            Kitty = spellData.Name != "JavelinToss";
            Target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Magical);

            ProcessCooldowns();

            if (Me.IsStunned) return;
            PrimalSurge();
            Killsteal();

            if (Target != null && !Kitty)
                if (Target.Distance(Me) < 650f && TargetHunted(Target) && Config.Item("usecombo").GetValue<KeyBind>().Active)
                    if (Config.Item("usecougarr").GetValue<bool>() && aspectofcougar.IsReady())
                        aspectofcougar.Cast();

            if (Config.Item("usecombo").GetValue<KeyBind>().Active)
                UseCombo(Target);
            if (Config.Item("useharass").GetValue<KeyBind>().Active)
                UseHarass(Target);
            if (Config.Item("useclear").GetValue<KeyBind>().Active)
                UseLaneclear();
            if (Config.Item("usejungle").GetValue<KeyBind>().Active )
                UseJungleclear();
        }

        #endregion

        #region Nidalee: SBTW
        private void UseCombo(Obj_AI_Base target)
        {
            var ignote = Me.GetSpellSlot("summonerdot");
            var minPounce = Config.Item("pouncerange").GetValue<Slider>().Value;
            var hitchance = Config.Item("hitchance").GetValue<StringList>().SelectedIndex;

            if (Kitty)
            {
                // dfg, botrk, hydra, tiamat
                if ((Items.CanUseItem(3128) && Items.HasItem(3128) || Items.CanUseItem(3144) && Items.HasItem(3144) ||
                     Items.CanUseItem(3153) && Items.HasItem(3153)) && TargetHunted(target) && pounce.IsReady() && ComboDamage(target) > target.Health)
                {
                    if (Config.Item("usedfg").GetValue<bool>())
                        Items.UseItem(3128, target);
                    if (Config.Item("useignote").GetValue<bool>())
                        Me.SummonerSpellbook.CastSpell(ignote, target);
                    if (Config.Item("usebork").GetValue<bool>())
                        Items.UseItem(3153);
                    if (Config.Item("usebw").GetValue<bool>())
                        Items.UseItem(3144);
                }
                else if (TargetHunted(target) && pounce.IsReady() && ComboDamage(target) > target.Health)
                {
                    if (Config.Item("useignote").GetValue<bool>())
                        Me.SummonerSpellbook.CastSpell(ignote, target);
                }

                // frost claim
                if (Items.CanUseItem(3092) && Items.HasItem(3092) && Config.Item("useclaim").GetValue<bool>())
                    Items.UseItem(3092, target.Position);
                if (takedown.IsReady() && Config.Item("usecougarq").GetValue<bool>() && target.Distance(Me.Position) < takedown.Range)
                    takedown.CastOnUnit(Me, Packets());
                if (pounce.IsReady() && Config.Item("usecougarw").GetValue<bool>() && target.Distance(Me.Position) < 750f && target.Distance(Me.Position) > minPounce)
                    pounce.Cast(target.Position, Packets());
                if (swipe.IsReady() && Config.Item("usecougare").GetValue<bool>())
                {
                    var prediction = swipe.GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.Medium && target.Distance(Me.Position) <= swipe.Range)
                        swipe.Cast(prediction.CastPosition, Packets());
                }
                if (target.Distance(Me.Position) > pounce.Range && Config.Item("usecougarr").GetValue<bool>())
                    if (aspectofcougar.IsReady())
                        aspectofcougar.Cast();
                if (!pounce.IsReady() && javelin.IsReady() && target.Distance(Me.Position) < pounce.Range && Config.Item("usecougarr").GetValue<bool>())
                    if (aspectofcougar.IsReady())
                        aspectofcougar.Cast();
            }
            else
            {
                if (javelin.IsReady() && target.Distance(Me.Position) < javelin.Range && Config.Item("usehumanq").GetValue<bool>())
                {
                    var prediction = javelin.GetPrediction(target);

                    switch (hitchance)
                    {
                        case 0:
                            if (prediction.Hitchance >= HitChance.Low)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                        case 1:
                            if (prediction.Hitchance >= HitChance.Medium)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                        case 2:
                            if (prediction.Hitchance >= HitChance.High)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                    }
                }

                if (bushwack.IsReady() && Config.Item("usehumanw").GetValue<bool>() && target.Distance(Me.Position) <= bushwack.Range)
                    bushwack.Cast(target.Position, Packets());
            }
        }
        #endregion

        #region Nidalee: Harass
        private void UseHarass(Obj_AI_Base target)
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = Config.Item("humanqpct").GetValue<Slider>().Value;
            var hitchance = Config.Item("hitchance").GetValue<StringList>().SelectedIndex;
            if (!Kitty && javelin.IsReady() && Config.Item("usehumanq2").GetValue<bool>())
            {
                var prediction = javelin.GetPrediction(target);
                if (target.Distance(Me.Position) <= javelin.Range && actualHeroManaPercent > minPercent)
                {
                    switch (hitchance)
                    {
                        case 0:
                            if (prediction.Hitchance >= HitChance.Low)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                        case 1:
                            if (prediction.Hitchance >= HitChance.Medium)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                        case 2:
                            if (prediction.Hitchance >= HitChance.High)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                    }
                }                    
            }
        }

        #endregion

        #region Nidalee: PrimalSurge
        private void PrimalSurge()
        {
            if (!primalsurge.IsReady() || !Config.Item("usedemheals").GetValue<bool>()) return;
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var selfManaPercent = Config.Item("healmanapct").GetValue<Slider>().Value;
            foreach (
                var hero in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            hero =>
                                hero.IsAlly && hero.Distance(Me.Position) < primalsurge.Range && !hero.IsDead &&
                                hero.IsValid && hero.IsVisible)) 
            {

                if (!Kitty && Config.Item("heal" + hero.SkinName).GetValue<bool>() && !Me.HasBuff("Recall"))
                {
                    var needed = Config.Item("healpct" +hero.SkinName).GetValue<Slider>().Value;
                    var hp = (int)((hero.Health / hero.MaxHealth) * 100);
                    if (actualHeroManaPercent > selfManaPercent && hp < needed)
                        primalsurge.CastOnUnit(hero, Packets());
                }
            }
        }

        #endregion

        #region Nidalee: Jungleclear
        private void UseJungleclear()
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = Config.Item("jgrpct").GetValue<Slider>().Value;

            foreach (
                var m in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            m =>
                                m.Distance(Me) < 1500f && m.IsEnemy && m.IsValid && m.IsVisible &&
                                JungleMinions.Any(name => m.Name.StartsWith(name)))) 
            {
                if (Kitty)
                {
                    if (Config.Item("jgcougare").GetValue<bool>() && m.Distance(Me.Position) < swipe.Range)
                        if (swipe.IsReady())
                            swipe.Cast(m.Position);
                    if (Config.Item("jgcougarw").GetValue<bool>() && m.Distance(Me.Position) < pounce.Range)
                        if (pounce.IsReady())
                            pounce.Cast(m.Position);
                    if (Config.Item("jgcougarq").GetValue<bool>() && m.Distance(Me.Position) < takedown.Range)
                        if (takedown.IsReady())
                            takedown.CastOnUnit(Me);
                }
                else
                {
                    if (Config.Item("jghumanq").GetValue<bool>() && actualHeroManaPercent > minPercent)
                        if (javelin.IsReady())
                            javelin.Cast(m.Position);
                    if (Config.Item("jghumanw").GetValue<bool>() && m.Distance(Me.Position) < bushwack.Range && actualHeroManaPercent > minPercent)
                        if (bushwack.IsReady())
                            bushwack.Cast(m.Position);
                    if (!javelin.IsReady() && Config.Item("jgcougarr").GetValue<bool>() && m.Distance(Me.Position) < pounce.Range && actualHeroManaPercent > minPercent)
                        if (aspectofcougar.IsReady())
                            aspectofcougar.Cast();
                }
            }
        }

        #endregion

        #region Nidalee: Laneclear
        private void UseLaneclear()
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = Config.Item("clearpct").GetValue<Slider>().Value;

            foreach (
                var m in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            m =>
                                m.Distance(Me.Position) < 1500f && m.IsEnemy && !m.IsDead && m.IsValid && m.IsVisible &&
                                JungleMinions.Any(name => !m.Name.StartsWith(name)))) 
            {
                if (Kitty)
                {
                    if (Config.Item("clearcougare").GetValue<bool>() && m.Distance(Me.Position) < swipe.Range)
                        if (swipe.IsReady())
                            swipe.Cast(m);
                    if (Config.Item("clearcougarw").GetValue<bool>() && m.Distance(Me.Position) < pounce.Range)
                        if (pounce.IsReady())
                            pounce.Cast(m.Position);
                    if (Config.Item("clearcougarq").GetValue<bool>() && m.Distance(Me.Position) < takedown.Range)
                        if (takedown.IsReady())
                            takedown.CastOnUnit(Me);
                }
                else
                {
                    if (Config.Item("clearhumanq").GetValue<bool>() && actualHeroManaPercent > minPercent)
                        if (javelin.IsReady())
                            javelin.Cast(m.Position);
                    if ((!javelin.IsReady() || !Config.Item("clearhumanq").GetValue<bool>()) && Config.Item("clearcougarr").GetValue<bool>() && m.Distance(Me.Position) < pounce.Range)
                        aspectofcougar.Cast();
                }
            }
        }
        #endregion

        #region Nidalee: Killsteal
        private void Killsteal()
        {
            if (!Config.Item("useks").GetValue<bool>()) return;
            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            e =>
                                e.Distance(Me.Position) < 1500f && e.IsEnemy && !e.IsDead && e.IsValid &&
                                e.IsValid)) 
            {
                var qdmg = Me.GetSpellDamage(e, SpellSlot.Q);
                var wdmg = Me.GetSpellDamage(e, SpellSlot.W);
                var edmg = Me.GetSpellDamage(e, SpellSlot.E);


                if (takedown.IsReady() && e != null && e.Health < qdmg && e.Distance(Me.Position) < takedown.Range)
                    takedown.CastOnUnit(Me, Packets());
                if (javelin.IsReady() && e != null && e.Health < qdmg)
                {
                    var javelinPrediction = javelin.GetPrediction(e);
                    if (javelinPrediction.Hitchance == HitChance.Medium)
                        javelin.Cast(javelinPrediction.CastPosition, Packets());
                }
                if (pounce.IsReady() && e != null && e.Health < wdmg && e.Distance(Me.Position) < pounce.Range)
                    pounce.Cast(e.Position, Packets());
                if (swipe.IsReady() && e != null && e.Health < edmg && e.Distance(Me.Position) < swipe.Range)
                    swipe.Cast(e.Position, Packets());
                if (javelin.IsReady() && e.Health < qdmg  && e.Distance(Me.Position) <= javelin.Range &&
                    Config.Item("swfks").GetValue<bool>())
                    aspectofcougar.Cast();
            }
        }

        #endregion

        #region Nidalee: Tracker

        // timer trackers credits to detuks
        private void NidaleeTracker(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                GetCooldowns(args);
        }

        private static readonly float[] humanQcd = { 6, 6, 6, 6, 6 };
        private static readonly float[] humanWcd = { 13, 12, 11, 10, 9 };
        private static readonly float[] humanEcd = { 12, 12, 12, 12, 12 };
        private static readonly float[] cougarQcd, cougarWcd, cougarEcd = { 5, 5, 5, 5, 5 };

        private static float CQRem, CWRem, CERem;
        private static float HQRem, HWRem, HERem;
        private static float CQ, CW, CE;
        private static float HQ, HW, HE;

        private void ProcessCooldowns()
        {
            if (Me.IsDead) return;
            CQ = ((CQRem - Game.Time) > 0) ? (CQRem - Game.Time) : 0;
            CW = ((CWRem - Game.Time) > 0) ? (CWRem - Game.Time) : 0;
            CE = ((CERem - Game.Time) > 0) ? (CERem - Game.Time) : 0;
            HQ = ((HQRem - Game.Time) > 0) ? (HQRem - Game.Time) : 0;
            HW = ((HWRem - Game.Time) > 0) ? (HWRem - Game.Time) : 0;
            HE = ((HERem - Game.Time) > 0) ? (HERem - Game.Time) : 0;
        }

        private static float CalculateCd(float time)
        {
            return time + (time * Me.PercentCooldownMod);
        }

        private void GetCooldowns(GameObjectProcessSpellCastEventArgs spell)
        {
            if (Kitty)
            {
                if (spell.SData.Name == "Takedown")
                    CQRem = Game.Time + CalculateCd(cougarQcd[javelin.Level]);
                if (spell.SData.Name == "Pounce")
                    CWRem = Game.Time + CalculateCd(cougarWcd[bushwack.Level]);
                if (spell.SData.Name == "Swipe")
                    CERem = Game.Time + CalculateCd(cougarEcd[primalsurge.Level]);
            }
            else
            {
                if (spell.SData.Name == "JavelinToss")
                    HQRem = Game.Time + CalculateCd(humanQcd[javelin.Level]);
                if (spell.SData.Name == "Bushwhack")
                    HWRem = Game.Time + CalculateCd(humanWcd[bushwack.Level]);
                if (spell.SData.Name == "PrimalSurge")
                    HERem = Game.Time + CalculateCd(humanEcd[primalsurge.Level]);
            }
        }

        #endregion

        #region Nidalee: DamageLib
        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            var ignote = Me.GetSpellSlot("summonderdot");

            if (takedown.IsReady())
                damage += Me.GetSpellDamage(enemy, SpellSlot.Q);
            if (swipe.IsReady())
                damage += Me.GetSpellDamage(enemy, SpellSlot.E);
            if (pounce.IsReady())
                damage += Me.GetSpellDamage(enemy, SpellSlot.W);
            if (javelin.IsReady() && !Kitty)
                damage += Me.GetSpellDamage(enemy, SpellSlot.Q);
            if (Me.SummonerSpellbook.CanUseSpell(ignote) == SpellState.Ready )
                damage += Me.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            if (Items.HasItem(3128) && Items.CanUseItem(3128))
                damage += Me.GetItemDamage(enemy, Damage.DamageItems.Dfg); 
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += Me.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += Me.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
            return (float)damage;

        }

        #endregion

        #region Nidalee: On Draw
        private void NidaleeOnDraw(EventArgs args)
        {

            if (Target != null) Utility.DrawCircle(Target.Position, Target.BoundingRadius, Color.Red, 1, 1);

            foreach (var spell in cougarList)
            {
                var circle = Config.Item("draw" + spell.Slot.ToString()).GetValue<Circle>();
                if (circle.Active && Kitty && !Me.IsDead)
                    Utility.DrawCircle(Me.Position, spell.Range, circle.Color, 1, 1);
            }

            foreach (var spell in humanList)
            {
                var circle = Config.Item("draw" + spell.Slot.ToString()).GetValue<Circle>();
                if (circle.Active && !Kitty && !Me.IsDead)
                    Utility.DrawCircle(Me.Position, spell.Range, circle.Color, 1, 1);
            }

            if (!Config.Item("drawcds").GetValue<bool>()) return;

            var wts = Drawing.WorldToScreen(Me.Position);

            if (!Kitty) // lets show cooldown timers for the opposite form :)
            {
                if (Me.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Null");
                else if (CQ == 0)
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Ready");
                else
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.Orange, "Q: " + CQ.ToString("0.0"));
                if (Me.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Null");
                else if (CW == 0)
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Ready");
                else
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.Orange, "W: " + CW.ToString("0.0"));
                if (Me.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Null");
                else if (CE == 0)
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Ready");
                else
                    Drawing.DrawText(wts[0], wts[1], Color.Orange, "E: " + CE.ToString("0.0"));

            }
            else
            {
                if (Me.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Null");
                else if (HQ == 0)
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Ready");
                else
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.Orange, "Q: " + HQ.ToString("0.0"));
                if (Me.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Null");
                else if (HW == 0)
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Ready");
                else
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.Orange, "W: " + HW.ToString("0.0"));
                if (Me.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Null");
                else if (HE == 0)
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Ready");
                else
                    Drawing.DrawText(wts[0], wts[1], Color.Orange, "E: " + HE.ToString("0.0"));

            }
        }
        #endregion
    }
}
