#region LICENSE

// Copyright 2014 - 2014 Support
// Template.cs is part of Support.
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
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Support
{
    public class Template : PluginBase
    {
        public Template()
        {
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 0);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target, "ComboQ"))
                {
                }

                if (W.CastCheck(Target, "ComboW"))
                {
                }

                if (E.CastCheck(Target, "ComboE"))
                {
                }

                if (R.CastCheck(Target, "ComboR"))
                {
                }
            }

            if (HarassMode)
            {
                if (Q.CastCheck(Target, "HarassQ"))
                {
                }

                if (W.CastCheck(Target, "HarassW"))
                {
                }

                if (E.CastCheck(Target, "HarassE"))
                {
                }
            }
        }

        public override void OnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
        }

        public override void OnAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Q.CastCheck(gapcloser.Sender, "GapcloserQ"))
            {
            }

            if (W.CastCheck(gapcloser.Sender, "GapcloserW"))
            {
            }

            if (E.CastCheck(gapcloser.Sender, "GapcloserE"))
            {
            }

            if (R.CastCheck(gapcloser.Sender, "GapcloserR"))
            {
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel < InterruptableDangerLevel.High || unit.IsAlly)
                return;

            if (Q.CastCheck(unit, "InterruptQ"))
            {
            }

            if (W.CastCheck(unit, "InterruptW"))
            {
            }

            if (E.CastCheck(unit, "InterruptE"))
            {
            }

            if (R.CastCheck(unit, "InterruptR"))
            {
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "浣跨敤 Q", true);
            config.AddBool("ComboW", "浣跨敤 W", true);
            config.AddBool("ComboE", "浣跨敤 E", true);
            config.AddBool("ComboR", "浣跨敤 R", true);
            config.AddSlider("ComboCountR", "鍑犱釜鏁屼汉浣跨敤澶ф嫑", 2, 0, 5);
            config.AddSlider("ComboHealthR", "鍓╀綑琛€閲忓紑鍚ぇ鎷涖劎", 20, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "浣跨敤 Q", true);
            config.AddBool("HarassW", "浣跨敤 W", true);
            config.AddBool("HarassE", "浣跨敤 E", true);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddBool("GapcloserQ", "浣跨敤 Q 闃茬獊杩涖劎", true);
            config.AddBool("GapcloserW", "浣跨敤 W 闃茬獊杩涖劎", true);
            config.AddBool("GapcloserE", "浣跨敤 E 闃茬獊杩涖劎", true);
            config.AddBool("GapcloserR", "浣跨敤 R 闃茬獊杩涖劎", true);

            config.AddBool("InterruptQ", "浣跨敤 Q 鎵撴柇娉曟湳", true);
            config.AddBool("InterruptW", "浣跨敤 W 鎵撴柇娉曟湳", true);
            config.AddBool("InterruptE", "浣跨敤 E 鎵撴柇娉曟湳", true);
            config.AddBool("InterruptR", "浣跨敤 R 鎵撴柇娉曟湳", true);
        }
    }
}