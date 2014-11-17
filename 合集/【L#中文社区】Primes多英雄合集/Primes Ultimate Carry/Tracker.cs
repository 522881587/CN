using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace Primes_Ultimate_Carry
{
	internal class Tracker
	{
		public static Sprite Sprite;
		public static Texture TextureOther;
		public static Texture TextureMe;
		private static readonly Dictionary<string, Texture> SummonerTextures = new Dictionary<string, Texture>(StringComparer.InvariantCultureIgnoreCase);
		private static readonly Dictionary<string, Texture> SpellTextures = new Dictionary<string, Texture>(StringComparer.InvariantCultureIgnoreCase);
		public static int X = 0;
		public static int Y = 0;
		public static SpellSlot[] SummonerSpellSlots = { SpellSlot.Q, SpellSlot.W };
		public static SpellSlot[] SpellSlots = { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
		public static string[] SummonersNames = {"SummonerBarrier", "SummonerBoost", "SummonerClairvoyance", "SummonerDot", "SummonerExhaust", "SummonerFlash", "SummonerHaste", "SummonerHeal", "SummonerMana", "SummonerOdinGarrison", "SummonerRevive", "SummonerSmite", "SummonerTeleport"};

		internal static void AddtoMenu(Menu menu)
		{
			var tempMenu = menu;
			tempMenu.AddItem(new MenuItem("tb_sep0", "====== 璁剧疆"));
			tempMenu.AddItem(new MenuItem("tb_show_enemy", "= 鏄剧ず璺熻釜鐨勬晫浜恒劎").SetValue(true));
			tempMenu.AddItem(new MenuItem("tb_show_friend", "= 鏄剧ず璺熻釜鐨勬湅鍙嬨劎").SetValue(true));
			tempMenu.AddItem(new MenuItem("tb_show_me", "= 鏄剧ず璺熻釜鎴戙劎").SetValue(true));		
			tempMenu.AddItem(new MenuItem("tb_sep1", "========="));
			PUC.Menu.AddSubMenu(tempMenu);

			// ReSharper disable once ObjectCreationAsStatement
			new Tracker();
		}

		static Tracker()
		{
			foreach (var sName in SummonersNames)
				SummonerTextures.Add(sName.ToLower(), GetSummonerTexture(sName.ToLower()));
			foreach (var slot in SpellSlots)
				SpellTextures.Add(slot.ToString(), GetSpellTexture(slot.ToString()));

			Sprite = new Sprite(Drawing.Direct3DDevice);
			TextureOther = Texture.FromMemory(
			Drawing.Direct3DDevice,
			(byte[])new ImageConverter().ConvertTo(Properties.Resources.Healthbar_Tracker_Others, typeof(byte[])), 131, 17, 0,
			Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);
			TextureMe = Texture.FromMemory(
			Drawing.Direct3DDevice,
			(byte[])new ImageConverter().ConvertTo(Properties.Resources.Healthbar_Tracker2, typeof(byte[])), 131, 17, 0,
			Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);
			Drawing.OnPreReset += DrawingOnOnPreReset;
			Drawing.OnPostReset += DrawingOnOnPostReset;
			Drawing.OnDraw += Drawing_OnDraw;
			AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
			AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnDomainUnload;
		}

		private static Texture GetSpellTexture(string name)
		{
			Bitmap bitmap;
			switch (name)
			{
				case "Q":
					bitmap = Properties.Resources.Q_CD;
					break;
				case "W":
					bitmap = Properties.Resources.W_CD;
					break;
				case "E":
					bitmap = Properties.Resources.E_CD;
					break;
				case "R":
					bitmap = Properties.Resources.R_CD;
					break;
				default:
					bitmap = Properties.Resources.Q_CD;
					break;
			}
			return Texture.FromMemory(
			Drawing.Direct3DDevice, (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[])), 260, 13, 0,
			Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);
	

		}

		private static Texture GetSummonerTexture(string name)
		{
			Bitmap bitmap;
			switch(name)
			{
				case "summonerbarrier":
					bitmap = Properties.Resources.CD_barrier;
					break;
				case "summonerodingarrison":
					bitmap = Properties.Resources.CD_garrison;
					break;
				case "summonerrevive":
					bitmap = Properties.Resources.CD_revive;
					break;
				case "Summonerclairvoyance":
					bitmap = Properties.Resources.CD_clairvoyance;
					break;
				case "summonerboost":
					bitmap = Properties.Resources.CD_cleanse;
					break;
				case "Summonermana":
					bitmap = Properties.Resources.CD_mana_aswell_dunno_lol;
					break;
				case "summonerteleport":
					bitmap = Properties.Resources.Teleport_CD2;
					break;
				case "summonerheal":
					bitmap = Properties.Resources.CD_heal;
					break;
				case "summonerexhaust":
					bitmap = Properties.Resources.CD_exhaust;
					break;
				case "summonersmite":
					bitmap = Properties.Resources.CD_smite;
					break;
				case "summonerdot":
					bitmap = Properties.Resources.CD_ignite;
					break;
				case "summonerhaste":
					bitmap = Properties.Resources.CD_ghost;
					break;
				case "summonerflash":
					bitmap = Properties.Resources.CD_flash;
					break;
				default:
					bitmap = Properties.Resources.CD_ignite;
					break;
			}
			return Texture.FromMemory(
			Drawing.Direct3DDevice, (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[])), 260, 13, 0,
			Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);
		}

		private static void CurrentDomainOnDomainUnload(object sender, EventArgs eventArgs)
		{
			Sprite.Dispose();
		}
		
		private static void DrawingOnOnPostReset(EventArgs args)
		{
			Sprite.OnResetDevice();
		}
	
		private static void DrawingOnOnPreReset(EventArgs args)
		{
			Sprite.OnLostDevice();
		}

		private static void Drawing_OnDraw(EventArgs args)
		{
			if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
				return;
			try
			{
				if (Sprite.IsDisposed)
					return;
				foreach(var hero in
				ObjectManager.Get<Obj_AI_Hero>()
				.Where(hero => hero != null && hero.IsValid && hero.IsHPBarRendered && (hero.IsEnemy && PUC.Menu.Item("tb_show_enemy").GetValue<bool>() || !hero.IsMe && hero.IsAlly && PUC.Menu.Item("tb_show_friend").GetValue<bool>() || hero.IsMe  && PUC.Menu.Item("tb_show_me").GetValue<bool>() )))
				{
					Sprite.Begin();
					var indicatorsummoner = new HpBar {Unit = hero};
					X = (int)indicatorsummoner.Position.X;
					Y = (int)indicatorsummoner.Position.Y;
					var k = 0;
					foreach(var sSlot in SummonerSpellSlots)
					{
						var spell = hero.SummonerSpellbook.GetSpell(sSlot);
						var texture = SummonerTextures[spell.Name];
						var t = spell.CooldownExpires - Game.Time;
						var percent = (Math.Abs(spell.Cooldown) > float.Epsilon) ? t / spell.Cooldown : 1f;
						var n = (t > 0) ? (int)(19 * (1f - percent)) : 19;
						Sprite.Draw(
							texture, new ColorBGRA(255, 255, 255, 255), new SharpDX.Rectangle(13*n, 0, 13, 13),
							hero.IsMe ? new Vector3(-X - 3 - 13*k - 100, -Y - 1, 0) : new Vector3(-X - 3 - 13*k - 75, -Y - 1, 0));
						k++;
					}
					
					k = 0;
					foreach(var sSlot in SpellSlots)
					{
						var spell = hero.Spellbook.GetSpell(sSlot);
						var texture = SpellTextures[spell.Slot.ToString()];
						var t = spell.CooldownExpires - Game.Time;
						var percent = (Math.Abs(spell.Cooldown) > float.Epsilon) ? t / spell.Cooldown : 1f;
						var n = (t > 0) ? (int)(19 * (1f - percent)) : 19;
						Sprite.Draw(
							texture, new ColorBGRA(255, 255, 255, 255), new SharpDX.Rectangle(13*n, 0, 13, 13),
							hero.IsMe ? new Vector3(-X - 3 - 13*k - 24, -Y - 1, 0) : new Vector3(-X - 3 - 13*k  +1, -Y - 1, 0));
						k++;
					}

					Sprite.Draw(hero.IsMe ? TextureMe : TextureOther, new ColorBGRA(255, 255, 255, 255), null, new Vector3(-X, -Y, 0));
					Sprite.End();
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(@"/ff can't draw sprites: " + e);
			}
		}

		internal class HpBar
		{
			internal Obj_AI_Hero Unit
			{
				get;
				set;
			}
			private Vector2 Offset
			{
				get
				{
					if (Unit != null)
						return Unit.IsMe ? new Vector2(10, 23) : new Vector2(7, 31 );
					return new Vector2();
				}
			}
			internal Vector2 Position
			{
				get
				{
					return new Vector2(Unit.HPBarPosition.X + Offset.X, Unit.HPBarPosition.Y + Offset.Y);
				}
			}
		}

	}



	
}


		

	

