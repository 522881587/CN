using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;

namespace Skin_Changer
{
    class Program
    {
        private static Menu Config;
        private static Dictionary<int, string> pSkins;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            try
            {
                pSkins = GetSkins(ObjectManager.Player.ChampionName);

                Config = new Menu("L#涓枃绀惧尯-鐗规晥鎹㈣偆", "Skin Changer", true);
                var SelectedSkin = Config.AddItem(new MenuItem("currentSkin", " ").SetValue(new StringList(pSkins.Select(item => item.Value).ToArray())).DontSave());
                var SwitchSkin = Config.AddItem(new MenuItem("keySwitch", "寮€鍏崇毊鑲ゃ劎").SetValue(new KeyBind("9".ToCharArray()[0], KeyBindType.Press)));

                SelectedSkin.ValueChanged += (object sender, OnValueChangeEventArgs vcArgs) =>
                {
                    Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(ObjectManager.Player.NetworkId, vcArgs.GetNewValue<StringList>().SelectedIndex, ObjectManager.Player.ChampionName)).Process(PacketChannel.S2C);
                };

                SwitchSkin.ValueChanged += (object sender, OnValueChangeEventArgs vcArgs) =>
                {
                    if (vcArgs.GetOldValue<KeyBind>().Active) return;

                    var currentSkin = Config.Item("currentSkin");
                    var OldValues = currentSkin.GetValue<StringList>();
                    var newSkinId = OldValues.SelectedIndex + 1 >= OldValues.SList.Count() ? 0 : OldValues.SelectedIndex + 1;
                    currentSkin.SetValue(new StringList(OldValues.SList, newSkinId));
                };

                Config.AddToMainMenu();

                Game.PrintChat("<font color=\"#0066FF\">[<font color=\"#FFFFFF\">madk</font>]</font><font color=\"#FFFFFF\"> Skin Changer loaded!</font>");
            }
            catch(Exception ex)
            {
                Game.PrintChat("<font color=\"#0066FF\">[<font color=\"#FFFFFF\">madk</font>]</font><font color=\"#FFFFFF\"> An error ocurred loading Skin Changer.</font>");
                
                Console.WriteLine("~~ Skin Changer exception found ~~");
                Console.WriteLine(ex);
                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            }

        }

        private static Dictionary<int, string> GetSkins(string ChampionName)
        {
            Dictionary<int, string> SkinList = new Dictionary<int, string>();
            var client = new WebClient();
            var JsonChamp = client.DownloadString(string.Format("http://ddragon.leagueoflegends.com/cdn/{0}/data/en_US/champion/{1}.json", DataDragonVersion, ChampionName));
            
            var JSSerializer = new JavaScriptSerializer();
            Dictionary<string, object> deserializedJson = JSSerializer.Deserialize<Dictionary<string, object>>(JsonChamp);
            Dictionary<string, object> temp = deserializedJson["data"] as Dictionary<string, object>;
            Dictionary<string, object> ChampionData = temp[ChampionName] as Dictionary<string, object>;

            var Skins = ChampionData["skins"] as ArrayList;
            var SkinId = 0;

            foreach (Dictionary<string, object> Skin in Skins)
            {
                SkinList.Add(SkinId, Skin["name"].ToString());
                SkinId++;
            }

            return SkinList;
        }

        private static string DataDragonVersion
        {
            get
            {
                var client = new WebClient();
                var JsonRealmV = client.DownloadString("http://ddragon.leagueoflegends.com/realms/na.json");
                var JSSerializer = new JavaScriptSerializer();
                Dictionary<string, object> deserializedJson = JSSerializer.Deserialize<Dictionary<string, object>>(JsonRealmV);
                Dictionary<string, object> n = deserializedJson["n"] as Dictionary<string, object>;

                return n["champion"] as string;
            }
        }
    }
}
