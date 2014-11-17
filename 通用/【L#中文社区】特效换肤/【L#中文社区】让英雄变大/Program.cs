using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace MakeMyMonsterGrow
{
    internal class Program
    {
        public static Menu Config;
        public static Int32 SequenceId = 1;
        public static Int32 previousNetId = 0x40000019;
        public static Int32 newNetId = 0x40000019;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("让英雄变大只", "MakeMyMonsterGrow", true);
            Game.OnGameSendPacket += Game_OnGameSendPacket;
            Config.AddItem(new MenuItem("Size", "变大尺寸��").SetValue(new Slider(150, 50, 200)));
            Config.AddToMainMenu();
			
			Config.AddSubMenu(new Menu("L#中文社区汉化", "LOLL35.Com"));
				Config.SubMenu("LOLL35.Com").AddItem(new MenuItem("wangzhi", "www.loll35.com"));
				Config.SubMenu("LOLL35.Com").AddItem(new MenuItem("qunhao2", "汉化群：123456"));
        }
        static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.C2S.SetTarget.Header)
            {
                ChangeSize(previousNetId, 1.0f);
                newNetId = BitConverter.ToInt32(args.PacketData, 9);
                ChangeSize(newNetId, (Single)(Config.Item("Size").GetValue<Slider>().Value) / 100.0f);
                previousNetId = newNetId;
            }
            else if (args.PacketData[0] == Packet.C2S.UpdateConfirm.Header)
                SequenceId = BitConverter.ToInt32(args.PacketData, 5) + 1;
        }
        private static void ChangeSize(Int32 netId, Single size)
        {
            Obj_AI_Hero hero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(netId);
            if (hero != null)
            {
                GamePacket p = new GamePacket(0xC4);
                p.WriteInteger(0);
                p.WriteInteger(SequenceId++);
                p.WriteByte(0x1);
                p.WriteByte(0x8);
                p.WriteInteger(netId);
                p.WriteInteger(0x800);
                p.WriteByte(0x8);
                p.WriteFloat(size);
                p.Process();
                SequenceId++;
            }
        }
    }
}
