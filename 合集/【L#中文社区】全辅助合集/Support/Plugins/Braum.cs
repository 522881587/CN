#region LICENSE

// Copyright 2014 - 2014 Support
// Braum.cs is part of Support.
// Support is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// Support is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with Support. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Support.Evade;
using SpellData = LeagueSharp.SpellData;

#endregion

namespace Support.Plugins
{
    public class Braum : PluginBase
    {
        public Braum()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 1200);

            Q.SetSkillshot(0.25f, 60f, 1700f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 115f, 1400f, false, SkillshotType.SkillshotLine);
            Protector.OnSkillshotProtection += ProtectorOnSkillshotProtection;
            Protector.OnTargetedProtection += ProtectorOnTargetedProtection;
        }

        private bool IsShieldActive { get; set; }

        private void CastShield(Vector3 v)
        {
            if (!E.IsReady())
                return;

            E.Cast(v, UsePackets);
            IsShieldActive = true;
            Utility.DelayAction.Add(4000, () => IsShieldActive = false);
        }

        private void ProtectorOnTargetedProtection(Obj_AI_Base caster, Obj_AI_Hero target, SpellData spell)
        {
            try
            {
                if (!ConfigValue<bool>("Misc.Shield.Target"))
                    return;

                if (Orbwalking.IsAutoAttack(spell.Name) &&
                    target.HealthPercent() > ConfigValue<Slider>("Misc.Shield.Health").Value)
                    return;

                if (spell.MissileSpeed > 2000 || spell.MissileSpeed == 0)
                    return;

                // TODO: blacklist FiddleQ, FioraQ/R, LeonaE, VladQ, ZileanQ

                if (target.IsMe && E.IsReady())
                {
                    CastShield(caster.Position);
                }

                if (!target.IsMe && W.IsReady() && W.IsInRange(target) && (IsShieldActive || E.IsReady()))
                {
                    var jumpTime = (Player.Distance(target)*1000/W.Instance.SData.MissileSpeed) +
                                   (W.Instance.SData.SpellCastTime*1000);
                    var missileTime = caster.Distance(target)*1000/spell.MissileSpeed;

                    if (jumpTime > missileTime)
                    {
                        Console.WriteLine("Abort Jump - Missile too Fast: {0} {1}", jumpTime, missileTime);
                        return;
                    }

                    W.CastOnUnit(target, UsePackets);
                    Utility.DelayAction.Add((int) jumpTime, () => CastShield(caster.Position));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ProtectorOnSkillshotProtection(Obj_AI_Hero target, List<Skillshot> skillshots)
        {
            try
            {
                if (!ConfigValue<bool>("Misc.Shield.Skill"))
                    return;

                // get most dangerous skillshot
                var max = skillshots.First();
                foreach (
                    var spell in
                        skillshots.Where(
                            s =>
                                s.SpellData.Type == SkillShotType.SkillshotMissileLine ||
                                s.SpellData.Type == SkillShotType.SkillshotMissileCone))
                {
                    if (spell.Unit.GetSpellDamage(target, spell.SpellData.SpellName) >
                        max.Unit.GetSpellDamage(target, max.SpellData.SpellName))
                    {
                        max = spell;
                    }
                }

                if (target.IsMe && E.IsReady())
                {
                    CastShield(max.Start.To3D());
                }

                if (!target.IsMe && W.IsReady() && W.IsInRange(target) && (IsShieldActive || E.IsReady()))
                {
                    var jumpTime = (Player.Distance(target)*1000/W.Instance.SData.MissileSpeed) +
                                   (W.Instance.SData.SpellCastTime*1000);
                    var missileTime = target.Distance(max.MissilePosition)*1000/max.SpellData.MissileSpeed;

                    if (jumpTime > missileTime)
                    {
                        Console.WriteLine("Abort Jump - Missile too Fast: {0} {1}", jumpTime, missileTime);
                        return;
                    }

                    W.CastOnUnit(target, UsePackets);
                    Utility.DelayAction.Add((int) jumpTime, () => CastShield(max.Start.To3D()));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target, "Combo.Q"))
                {
                    Q.Cast(Target, UsePackets);
                }

                if (R.CastCheck(Target, "Combo.R"))
                {
                    R.CastIfWillHit(Target, ConfigValue<Slider>("Combo.R.Count").Value, true);
                }
            }

            if (HarassMode)
            {
                if (Q.CastCheck(Target, "Harass.Q"))
                {
                    Q.Cast(Target, UsePackets);
                }
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Q.CastCheck(gapcloser.Sender, "Gapcloser.Q"))
            {
                Q.Cast(gapcloser.Sender, UsePackets);
            }

            if (R.CastCheck(gapcloser.Sender, "Gapcloser.R"))
            {
                R.Cast(gapcloser.Sender, UsePackets);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel < InterruptableDangerLevel.High || unit.IsAlly)
                return;

            if (R.CastCheck(unit, "Interrupt.R"))
            {
                R.Cast(unit, UsePackets);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "浣跨敤 Q", true);
            config.AddBool("Combo.R", "浣跨敤 R", true);
            config.AddSlider("Combo.R.Count", "鏁屼汉鏁伴噺浣跨敤 R", 2, 1, 5);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "浣跨敤 Q", true);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddBool("Misc.Shield.Skill", "鐩炬姷鎸″皠鍑汇劎", true);
            config.AddBool("Misc.Shield.Target", "鐩剧殑鐩爣", true);
            config.AddSlider("Misc.Shield.Health", "鐩続A涓嬮潰 HP", 30, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.Q", "浣跨敤 Q 闃茬獊杩涖劎", true);
            config.AddBool("Gapcloser.R", "浣跨敤 R 闃茬獊杩涖劎", false);

            config.AddBool("Interrupt.R", "浣跨敤 R 鎵撴柇", true);
        }
    }
}