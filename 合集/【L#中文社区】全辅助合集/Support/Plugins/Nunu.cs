#region LICENSE

// Copyright 2014 - 2014 Support
// Nunu.cs is part of Support.
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
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Support.Plugins
{
    public class Nunu : PluginBase
    {
        private int LastLaugh { get; set; }

        public Nunu()
        {
            Q = new Spell(SpellSlot.Q, 125);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 650);
        }

        public override void OnProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.S2C.PlayEmote.Header &&
                ConfigValue<StringList>("Misc.Laugh").SelectedIndex == 2)
            {
                var p = Packet.S2C.PlayEmote.Decoded(args.PacketData);
                if (p.NetworkId == Player.NetworkId && p.EmoteId == (byte) Packet.Emotes.Laugh)
                    args.Process = false;
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.IsReady() && ConfigValue<bool>("Combo.Q") &&
                    Player.HealthPercent() < ConfigValue<Slider>("Combo.Q.Health").Value)
                {
                    var minion = MinionManager.GetMinions(Player.Position, Q.Range).FirstOrDefault();
                    if (minion.IsValidTarget(Q.Range))
                        Q.CastOnUnit(minion, UsePackets);
                }

                var allys = Helpers.AllyInRange(W.Range).OrderByDescending(h => h.FlatPhysicalDamageMod).ToList();
                if (W.IsReady() && allys.Count > 0 && ConfigValue<bool>("Combo.W"))
                {
                    W.CastOnUnit(allys.FirstOrDefault(), UsePackets);
                }

                if (W.IsReady() && Target.IsValidTarget(AttackRange) && ConfigValue<bool>("Combo.W"))
                {
                    W.CastOnUnit(Player, UsePackets);
                }

                if (E.IsReady() && Target.IsValidTarget(E.Range) && ConfigValue<bool>("Combo.E"))
                {
                    E.CastOnUnit(Target, UsePackets && ConfigValue<bool>("Misc.E.NoFace"));
                }
            }

            if (HarassMode)
            {
                if (Q.IsReady() && ConfigValue<bool>("Harass.Q") &&
                    Player.HealthPercent() < ConfigValue<Slider>("Harass.Q.Health").Value)
                {
                    var minion = MinionManager.GetMinions(Player.Position, Q.Range).FirstOrDefault();
                    if (minion.IsValidTarget(Q.Range))
                        Q.CastOnUnit(minion, UsePackets);
                }

                var allys = Helpers.AllyInRange(W.Range).OrderByDescending(h => h.FlatPhysicalDamageMod).ToList();
                if (W.IsReady() && allys.Count > 0 && ConfigValue<bool>("Harass.W"))
                {
                    W.CastOnUnit(allys.FirstOrDefault(), UsePackets);
                }

                if (W.IsReady() && Target.IsValidTarget(AttackRange) && ConfigValue<bool>("Harass.W"))
                {
                    W.CastOnUnit(Player, UsePackets);
                }

                if (E.IsReady() && Target.IsValidTarget(E.Range) && ConfigValue<bool>("Harass.E"))
                {
                    E.CastOnUnit(Target, UsePackets && ConfigValue<bool>("Misc.E.NoFace"));
                }
            }

            // most import part!!!
            if (Environment.TickCount > LastLaugh + 4000 && Target.IsValidTarget(2000) &&
                ConfigValue<StringList>("Misc.Laugh").SelectedIndex > 0)
            {
                Packet.C2S.Emote.Encoded(new Packet.C2S.Emote.Struct((byte) Packet.Emotes.Laugh)).Send();
                LastLaugh = Environment.TickCount;
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
                return;

            if (E.CastCheck(gapcloser.Sender, "Gapcloser.E"))
            {
                E.CastOnUnit(gapcloser.Sender, UsePackets);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "浣跨敤 Q", true);
            config.AddBool("Combo.W", "浣跨敤 W", true);
            config.AddBool("Combo.E", "浣跨敤 E", true);
            config.AddSlider("Combo.Q.Health", "Consume below %HP", 50, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "浣跨敤 Q", true);
            config.AddBool("Harass.W", "浣跨敤 W", false);
            config.AddBool("Harass.E", "浣跨敤 E", true);
            config.AddSlider("Harass.Q.Health", "娑堣€楄閲忎綆浜?", 50, 1, 100);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddList("Misc.Laugh", "绗戠殑鍔ㄤ綔", new[] { "OFF", "ON", "ON + Mute" });
            config.AddBool("Misc.E.NoFace", "E NoFace寮€鍙戙劎NoFace Exploit", false);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.E", "浣跨敤 E 闃茬獊杩涖劎", true);
        }
    }
}