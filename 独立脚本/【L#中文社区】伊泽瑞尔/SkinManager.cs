using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class SkinManager
{
    private List<string> Skins = new List<string>();
    private Menu Menu;
    private int SelectedSkin;
    private bool Initialize = true;

    public SkinManager()
    {

    }

    public void AddToMenu(ref Menu menu)
    {
        Menu = menu;

        if (Skins.Count > 0)
        {
            Menu.AddSubMenu(new Menu("鎹㈣偆", "Skin Changer"));
            Menu.SubMenu("Skin Changer").AddItem(new MenuItem("Skin_" + ObjectManager.Player.ChampionName + "_enabled", "鎵撳紑鎹㈣偆").SetValue(false));
            Menu.SubMenu("Skin Changer").AddItem(new MenuItem("Skin_" + ObjectManager.Player.ChampionName + "_select", "Skins").SetValue(new StringList(Skins.ToArray())));
            SelectedSkin = Menu.Item("Skin_" + ObjectManager.Player.ChampionName + "_select").GetValue<StringList>().SelectedIndex;
        }
    }

    public void Add(string skin)
    {
        Skins.Add(skin);
    }

    public void Update()
    {
        if (Menu.Item("Skin_" + ObjectManager.Player.ChampionName + "_enabled").GetValue<bool>())
        {
            int skin = Menu.Item("Skin_" + ObjectManager.Player.ChampionName + "_select").GetValue<StringList>().SelectedIndex;
            if (Initialize || skin != SelectedSkin)
            {
                GenerateSkinPacket(skin);
                SelectedSkin = skin;
                Initialize = false;
            }
        }
    }

    private static void GenerateSkinPacket(int skinNumber)
    {
        int netID = ObjectManager.Player.NetworkId;
        GamePacket model = Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(ObjectManager.Player.NetworkId, skinNumber, ObjectManager.Player.ChampionName));
        model.Process(PacketChannel.S2C);
    }
}