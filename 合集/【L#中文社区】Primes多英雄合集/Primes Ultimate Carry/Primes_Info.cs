using LeagueSharp.Common;

namespace Primes_Ultimate_Carry
{
	class PrimesInfo
	{
		internal static void AddtoMenu(Menu menu)
		{
			var tempMenu = menu;
			tempMenu.AddItem(new MenuItem("info_sep0", "====== 鍜ㄨ鍙般劎 ======"));
			tempMenu.AddItem(new MenuItem("info_Patchnotes", "= 琛ヤ竵璇存槑").SetValue((new KeyBind("U".ToCharArray()[0], KeyBindType.Press))));
			tempMenu.AddItem(new MenuItem("info_PUC", "= PUC 淇℃伅").SetValue((new KeyBind("I".ToCharArray()[0], KeyBindType.Press))));
			tempMenu.AddItem(new MenuItem("info_Champ", "= 鑻遍泟淇℃伅").SetValue((new KeyBind("L".ToCharArray()[0], KeyBindType.Press))));
			tempMenu.AddItem(new MenuItem("info_sep1", "===================="));
			PUC.Menu.AddSubMenu(tempMenu);

		}

		public static void Draw()
		{
			if(PUC.Menu.Item("info_Patchnotes").GetValue<KeyBind>().Active)
				InfoWindow.Patchnodes();
			if(PUC.Menu.Item("info_PUC").GetValue<KeyBind>().Active)
				InfoWindow.PUCInfo();
			if(PUC.Menu.Item("info_Champ").GetValue<KeyBind>().Active)
				InfoWindow.Champinfo();
		}
	}
}
