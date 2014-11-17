using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace Primes_Ultimate_Carry
{
	class SideBar
	{

		private static int _menuX;
		private static int _menuY;
		private static Render.RenderObject _box;
		//private static Render.RenderObject Champion1;
		//private static Render.RenderObject Champion2;
		//private static Render.RenderObject Champion3;
		//private static Render.RenderObject Champion4;
		//private static Render.RenderObject Champion5;

		internal static void AddtoMenu(Menu menu)
		{
			_menuX = Drawing.Width - 320;
			_menuY = 90;

			_box = new Render.Sprite(Properties.Resources.RightBar2, new Vector2(_menuX, _menuY));
			_box.Add();


			//Champion1 = new Render.Sprite(Properties.Resources.Aatrox , new Vector2(_menuX +17, _menuY +12));
			//Champion1.Add();
			//Champion2 = new Render.Sprite(Properties.Resources.Ahri, new Vector2(_menuX + 17, _menuY + 41));
			//Champion2.Add();
			//Champion3 = new Render.Sprite(Properties.Resources.Caitlyn , new Vector2(_menuX + 17, _menuY + 70));
			//Champion3.Add();
			//Champion4 = new Render.Sprite(Properties.Resources.Gnar, new Vector2(_menuX + 17, _menuY + 99));
			//Champion4.Add();
			//Champion5 = new Render.Sprite(Properties.Resources.DrMundo , new Vector2(_menuX + 17, _menuY + 128));
			//Champion5.Add();

			var tempMenu = menu;
			tempMenu.AddItem(new MenuItem("sb_sep0", "====== 璁剧疆"));
			tempMenu.AddItem(new MenuItem("sb_show", "= 鏄剧ず杈规爮").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
			tempMenu.AddItem(new MenuItem("sb_sep1", "========="));
			PUC.Menu.AddSubMenu(tempMenu);

		}

		internal static void Draw()
		{
			if (!PUC.Menu.Item("sb_show").GetValue<KeyBind>().Active)
			{
				_box.Visible = false;
				//Champion1.Visible  = false;
				//Champion2.Visible = false;
				//Champion3.Visible = false;
				//Champion4.Visible = false;
				//Champion5.Visible = false;
				return;
			}
			_box.Visible = true;
			//Champion1.Visible = true;
			//Champion2.Visible = true;
			//Champion3.Visible = true;
			//Champion4.Visible = true;
			//Champion5.Visible = true;
			DrawGui();
		}

		private static void DrawGui()
		{
			//DrawBox(_menuX + 10, _menuY + 10, 280, 150, Color.Black);
			//Drawing.DrawText(_menuX + 20, _menuY + 15, Color.White, "Annie");
			//Drawing.DrawText(_menuX + 20, _menuY + 45, Color.White, "Anivia");
			//Drawing.DrawText(_menuX + 20, _menuY + 75, Color.White, "Zed");
			//Drawing.DrawText(_menuX + 20, _menuY + 105, Color.White, "Jumbo");
			//Drawing.DrawText(_menuX + 20, _menuY + 135, Color.White, "Godzilla");
		}

		private static void DrawBox(int x, int y, int witdth, int height, Color color)
		{
			Drawing.DrawLine(new Vector2(x, y), new Vector2(x + witdth, y), height, color);
		}
	}
}
