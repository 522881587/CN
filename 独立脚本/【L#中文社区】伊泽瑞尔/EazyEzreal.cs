using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyEzreal
{
    class EazyEzreal : Champion
    {
        static void Main(string[] args)
        {
            new EazyEzreal();
        }

        public EazyEzreal() : base("Ezreal")
        {

        }

        protected override void InitializeSkins(ref SkinManager Skins)
        {
           Skins.Add("Ezreal");
            Skins.Add("Nottingham Ezreal");
            Skins.Add("Striker Ezreal");
            Skins.Add("Frosted Ezreal");
            Skins.Add("Explorer Ezreal");
            Skins.Add("Pulsefire Ezreal");
            Skins.Add("TPA Ezreal");
            Skins.Add("Debonair Ezreal");
        }

        protected override void InitializeSpells(ref SpellManager Spells)
        {
            Spell Q = new Spell(SpellSlot.Q, 1200);
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

            Spell W = new Spell(SpellSlot.W, 1050);
            W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);

            Spell R = new Spell(SpellSlot.R, float.MaxValue);
            R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            Spells.Add("Q", Q);
            Spells.Add("W", W);
            Spells.Add("R", R);
        }

        protected override void InitializeMenu()
        {
            Menu.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_q", "浣跨敤Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo_w", "浣跨敤W").SetValue(true));

            Menu.AddSubMenu(new Menu("楠氭壈", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Harass_q", "浣跨敤Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Harass_w", "浣跨敤W").SetValue(false));

            Menu.AddSubMenu(new Menu("鑷姩", "Auto"));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_q", "鑷姩Q").SetValue(true));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_w", "鑷姩W").SetValue(false));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_r", "鑷姩R").SetValue(true));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_minrange", "R min").SetValue(new Slider(1050, 0, 1500)));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_maxrange", "R max").SetValue(new Slider(3000, 1500, 5000)));
            Menu.SubMenu("Auto").AddItem(new MenuItem("Auto_manaE", "鐣欒摑E").SetValue(true));

            Menu.AddSubMenu(new Menu("鏄剧ず", "Drawing"));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_q", "Q鑼冨洿").SetValue(new Circle(true, Color.FromArgb(100, 0, 255, 0))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_w", "W鑼冨洿").SetValue(new Circle(true, Color.FromArgb(100, 0, 255, 0))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Drawing_rdamage", "R浼ゅ鎸囩ず").SetValue(true));
			
			Menu.AddSubMenu(new Menu("L#涓枃绀惧尯", "AD"));
				Menu.SubMenu("AD").AddItem(new MenuItem("wangzhan", "www.loll35.com"));
				Menu.SubMenu("AD").AddItem(new MenuItem("qunhao", "姹夊寲缇わ細397983217"));
        }

        protected override void Combo()
        {
            if (Menu.Item("Combo_q").GetValue<bool>()) Spells.CastSkillshot("Q", SimpleTs.DamageType.Physical, HitChance.High);
            if (Menu.Item("Combo_w").GetValue<bool>()) Spells.CastSkillshot("W", SimpleTs.DamageType.Magical, HitChance.High);
        }
        protected override void Harass()
        {
            if (Menu.Item("Harass_q").GetValue<bool>()) Spells.CastSkillshot("Q", SimpleTs.DamageType.Physical);
            if (Menu.Item("Harass_w").GetValue<bool>()) Spells.CastSkillshot("W", SimpleTs.DamageType.Magical);
        }
        protected override void Auto()
        {
            if (Menu.Item("Auto_q").GetValue<bool>() && !(Menu.Item("Auto_manaE").GetValue<bool>() && Player.Spellbook.GetSpell(SpellSlot.E).Level >= 1 && Player.Mana < Player.Spellbook.GetSpell(SpellSlot.E).ManaCost + Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost)) Spells.CastSkillshot("Q", SimpleTs.DamageType.Physical);
            if (Menu.Item("Auto_w").GetValue<bool>() && !(Menu.Item("Auto_manaE").GetValue<bool>() && Player.Spellbook.GetSpell(SpellSlot.E).Level >= 1 && Player.Mana < Player.Spellbook.GetSpell(SpellSlot.E).ManaCost + Player.Spellbook.GetSpell(SpellSlot.W).ManaCost)) Spells.CastSkillshot("W", SimpleTs.DamageType.Magical);
        }

        protected override void Draw()
        {
            DrawCircle("Drawing_q", "Q");
            DrawCircle("Drawing_w", "W");

            Utility.HpBarDamageIndicator.DamageToUnit = UltimateDamage;
            Utility.HpBarDamageIndicator.Enabled = Menu.Item("Drawing_rdamage").GetValue<bool>();
        }
        protected override void Update()
        {
            if (Menu.Item("Auto_r").GetValue<bool>()) CastR();
        } 

        private void CastR()
        {
            Spell R = Spells.get("R");

            if (!R.IsReady()) return;

            Obj_AI_Hero target = SimpleTs.GetTarget(Menu.Item("Auto_maxrange").GetValue<Slider>().Value, SimpleTs.DamageType.Magical);
            if (target == null || Player.Distance(target) < Menu.Item("Auto_minrange").GetValue<Slider>().Value) return;

            float predictedHealth = HealthPrediction.GetHealthPrediction(target, (int)(R.Delay + (Player.Distance(target) / R.Speed) * 1000));

            if (UltimateDamage(target) < predictedHealth || predictedHealth <= 0) return;

            if (R.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
                R.Cast(target, true);
        }

        private float UltimateDamage(Obj_AI_Hero hero)
        {
            Spell R = Spells.get("R");

            if (!R.IsReady()) return 0;

            float reduction = 1f - ((R.GetCollision(Player.Position.To2D(), new List<SharpDX.Vector2> { hero.Position.To2D() }).Count) / 10f);
            reduction = (reduction < 0.3f ? 0.3f : reduction);

            return (float)Damage.CalcDamage(Player, hero, Damage.DamageType.Magical, Damage.GetSpellDamage(Player, hero, SpellSlot.R) * reduction);
        }
    }
}
