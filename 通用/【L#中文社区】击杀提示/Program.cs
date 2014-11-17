/*
    Copyright (C) 2014 h3h3

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using Killability.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Killability
{
    public class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += a => new KillDrawer();
        }
    }

    internal class KillDrawer
    {
        private readonly List<Spell> _spells;
        private readonly List<Items.Item> _items;
        private readonly SpellDataInst _ignite;
        private readonly Menu _config;

        public KillDrawer()
        {
            _ignite = ObjectManager.Player.SummonerSpellbook.Spells.FirstOrDefault(x => x.Name.ToLower() == "summonerdot");
            _spells = new List<Spell>();
            _items = new List<Items.Item>
            {
                new Items.Item(3128, 750), // Deathfire Grasp
                new Items.Item(3077, 400), // Tiamat
                new Items.Item(3074, 400), // Ravenous Hydra
                new Items.Item(3146, 700), // Hextech Gunblade
                new Items.Item(3144, 450), // Bilgewater Cutlass
                new Items.Item(3153, 450)  // Blade of the Ruined King
            };

            try
            {
                new Spell(SpellSlot.Q).GetDamage(ObjectManager.Player);
                _spells.Add(new Spell(SpellSlot.Q));
            }
            catch (Exception)
            {
            }

            try
            {
                new Spell(SpellSlot.W).GetDamage(ObjectManager.Player);
                _spells.Add(new Spell(SpellSlot.W));
            }
            catch (Exception)
            {
            }

            try
            {
                new Spell(SpellSlot.E).GetDamage(ObjectManager.Player);
                _spells.Add(new Spell(SpellSlot.E));
            }
            catch (Exception)
            {
            }

            try
            {
                new Spell(SpellSlot.R).GetDamage(ObjectManager.Player);
                _spells.Add(new Spell(SpellSlot.R));
            }
            catch (Exception)
            {
            }

            _config = new Menu("L#涓枃绀惧尯-鍑绘潃鎻愮ず", "Killability", true);
            _config.AddItem(new MenuItem("icon", "鏄剧ず鍥炬爣").SetValue(true));
            _config.AddItem(new MenuItem("text", "鏄剧ず鏂囨湰").SetValue(true));


            _config.AddToMainMenu();

            InitDrawing();

            Game.PrintChat("Killability by h3h3 loaded.");
        }

        private void InitDrawing()
        {
            foreach (var h in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
            {
                var hero = h;
                var sprite = new Render.Sprite(Resources.Skull, hero.HPBarPosition);
                sprite.Scale = new Vector2(0.08f, 0.08f);
                sprite.PositionUpdate += () => new Vector2(hero.HPBarPosition.X + 140, hero.HPBarPosition.Y + 10);
                sprite.VisibleCondition += s =>
                    Render.OnScreen(Drawing.WorldToScreen(hero.Position)) &&
                    GetComboResult(hero).IsKillable &&
                    _config.Item("icon").GetValue<bool>();
                sprite.Add();

                var text = new Render.Text("", hero, new Vector2(20, 50), 18, new ColorBGRA(255, 255, 255, 255));
                text.VisibleCondition += s => Render.OnScreen(Drawing.WorldToScreen(hero.Position)) && _config.Item("text").GetValue<bool>();
                text.TextUpdate += () =>
                {
                    var result = GetComboResult(hero);
                    return result.Text;
                };
                text.OutLined = true;
                text.Add();
            }
        }

        private class ComboResult
        {
            public List<SpellSlot> Spells { get; set; }
            public bool IsKillable { get; set; }
            public double ManaCost { get; set; }
            public string Text { get; set; }

            public ComboResult()
            {
                Spells = new List<SpellSlot>();
            }
        }

        private ComboResult GetComboResult(Obj_AI_Hero target)
        {
            if (!target.IsValidTarget())
                return new ComboResult();

            var player = ObjectManager.Player;
            var result = new ComboResult();
            var comboMana = 0f;
            var comboDmg = 0d;

            foreach (var item in _items.Where(item => item.IsReady()))
            {
                switch (item.Id)
                {
                    case 3128: comboDmg += player.GetItemDamage(target, Damage.DamageItems.Dfg); break;
                    case 3077: comboDmg += player.GetItemDamage(target, Damage.DamageItems.Tiamat); break;
                    case 3074: comboDmg += player.GetItemDamage(target, Damage.DamageItems.Hydra); break;
                    case 3146: comboDmg += player.GetItemDamage(target, Damage.DamageItems.Hexgun); break;
                    case 3144: comboDmg += player.GetItemDamage(target, Damage.DamageItems.Bilgewater); break;
                    case 3153: comboDmg += player.GetItemDamage(target, Damage.DamageItems.Botrk); break;
                }

                if (comboDmg > target.Health)
                    break;
            }

            foreach (var spell in _spells.Where(spell => spell.Level > 0))
            {
                try
                {
                    comboDmg += spell.GetDamage(target, 1);
                    comboMana += spell.Instance.ManaCost;
                    result.Spells.Add(spell.Slot);

                    if (comboDmg > target.Health)
                        break;
                }
                catch
                {
                }
            }

            if (_ignite != null && _ignite.State == SpellState.Ready && target.Health >  comboDmg)
            {
                comboDmg += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }

            result.IsKillable = comboDmg > target.Health && player.Mana > comboMana;
            result.ManaCost = comboMana;

            if (result.IsKillable)
                result.Text = string.Join("/", result.Spells);

            if (player.Mana < comboMana)
                result.Text = "LOW MANA";

            return result;
        }
    }
}