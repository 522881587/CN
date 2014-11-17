using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Primes_Ultimate_Carry
{
	// ReSharper disable once InconsistentNaming
	class Champion_Thresh : Champion
	{
		public int QFollowTick = 0;
		public const int QFollowTime = 5000;

		public Champion_Thresh()
		{
			SetSpells();
			LoadMenu();

			Game.OnGameUpdate += OnUpdate;
			Drawing.OnDraw += OnDraw;
			Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
			AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

			PluginLoaded();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q, 1000);
			Q.SetSkillshot(0.5f, 50f, 1900, true, SkillshotType.SkillshotLine);

			W = new Spell(SpellSlot.W, 950);

			E = new Spell(SpellSlot.E, 400);

			R = new Spell(SpellSlot.R, 400);
		}

		private void LoadMenu()
		{
			ChampionMenu.AddSubMenu(new Menu("杩炴嫑", PUC.Player.ChampionName + "Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep0", "====== 杩炴嫑"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo_follow", "= 璺熼殢 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo_Shield", "= 浣跨敤 W Shield at x%").SetValue(new Slider(40, 100, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo_Engage", "= 浣跨敤 W for Engage").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo", "= E 鐩村埌锛呭仴搴枫劎").SetValue(new Slider(10, 100, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo_minHit", "= 濡傛灉鍛戒腑浣跨敤 R ").SetValue(new Slider(2, 5, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("楠氭壈", PUC.Player.ChampionName + "Harass"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass"), "ManaManager_Harass", 40);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== 楠氭壈"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useW_Harass_safe", "= 浣跨敤 W 淇濇姢闃熷弸").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useE_Harass", "= E 鑷虫垜鍒帮紖鍋ュ悍").SetValue(new Slider(90, 100, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("娓呭叺", PUC.Player.ChampionName + "LaneClear"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear"), "ManaManager_LaneClear", 20);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== 娓呭叺"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear", "= 浣跨敤 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("RunLikeHell", PUC.Player.ChampionName + "RunLikeHell"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep0", "====== RunLikeHell"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useE_RunLikeHell", "= E to Mouse").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("鏉傞」", PUC.Player.ChampionName + "Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep0", "====== Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useQ_Interrupt", "= Q 鎵撴柇鎶€鑳姐劎").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useE_Interrupt", "= E 鎵撴柇鎶€鑳姐劎").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useE_GapCloser", "= E 鍙嶇獊杩涖劎").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep1", "========="));

			AddSupportmode(ChampionMenu);

			ChampionMenu.AddSubMenu(new Menu("缁樺埗", PUC.Player.ChampionName + "Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep0", "====== Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Disabled", "鍏ㄩ儴绂佺敤").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Q", "缁樺埗 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_W", "缁樺埗 W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_E", "缁樺埗 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_R", "缁樺埗 R").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep1", "========="));

			PUC.Menu.AddSubMenu(ChampionMenu);
		}

		private void OnDraw(EventArgs args)
		{
			Orbwalker.AllowDrawing = !ChampionMenu.Item("Draw_Disabled").GetValue<bool>();

			if(ChampionMenu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(ChampionMenu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_W").GetValue<bool>())
				if(W.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
		}

		private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
		{
			if(!ChampionMenu.Item("useE_GapCloser").GetValue<bool>())
				return;
			if(!(gapcloser.End.Distance(PUC.Player.ServerPosition) <= 100))
				return;
			E.CastIfHitchanceEquals(gapcloser.Sender, HitChance.Medium, UsePackets());
		}

		private void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
		{
			if(ChampionMenu.Item("useE_Interrupt").GetValue<bool>())
			{
				E.CastIfHitchanceEquals(unit, HitChance.Medium, UsePackets());
				return;
			}
			if (!ChampionMenu.Item("useQ_Interrupt").GetValue<bool>() || Environment.TickCount - QFollowTick <= QFollowTime)
				return;
			if(Q.CastIfHitchanceEquals(unit, HitChance.Medium, UsePackets()))
				QFollowTick = Environment.TickCount;
		}


		private void OnUpdate(EventArgs args)
		{

			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(ChampionMenu.Item("useQ_Combo").GetValue<bool>() && Environment.TickCount - QFollowTick >= QFollowTime)
						if(Cast_BasicSkillshot_Enemy(Q) != null)
							QFollowTick = Environment.TickCount;
					if(ChampionMenu.Item("useQ_Combo_follow").GetValue<bool>() && QFollowTarget.ShouldCast() && Q.IsReady())
						Q.Cast();
					Cast_Shield_onFriend(W, ChampionMenu.Item("useW_Combo_Shield").GetValue<Slider>().Value, true);
					if(ChampionMenu.Item("useW_Combo_Engage").GetValue<bool>())
						EngageFriendLatern();
					if (ChampionMenu.Item("useE_Combo").GetValue<Slider>().Value > 0)
						if (PUC.Player.Health/PUC.Player.MaxHealth*100 > ChampionMenu.Item("useE_Combo").GetValue<Slider>().Value)
							Cast_E("ToMe");
						else
							Cast_E();
					if(ChampionMenu.Item("useR_Combo_minHit").GetValue<Slider>().Value >= 1)
						if(EnemysinRange(R.Range, ChampionMenu.Item("useR_Combo_minHit").GetValue<Slider>().Value))
							R.Cast();
					break;
				case Orbwalker.Mode.Harass:
					if(ChampionMenu.Item("useQ_Harass").GetValue<bool>() && Environment.TickCount - QFollowTick >= QFollowTime && ManamanagerAllowCast("ManaManager_Harass"))
						if(Cast_BasicSkillshot_Enemy(Q) != null)
							QFollowTick = Environment.TickCount;
					if(ChampionMenu.Item("useE_Harass").GetValue<Slider>().Value > 0)
						if(PUC.Player.Health / PUC.Player.MaxHealth * 100 > ChampionMenu.Item("useE_Harass").GetValue<Slider>().Value)
							Cast_E("ToMe");
						else
							Cast_E();
					if(ChampionMenu.Item("useW_Harass_safe").GetValue<bool>())
						SafeFriendLatern();
					break;
				case Orbwalker.Mode.LaneClear:
					if(ChampionMenu.Item("useE_LaneClear").GetValue<bool>())
						Cast_BasicSkillshot_AOE_Farm(E);
					break;
			}
		}

		private void Cast_E(string mode = "")
		{
			if(!E.IsReady())
				return;
			var target = TargetSelector.GetTarget(E.Range - 10);
			if(target == null)
				return;
			E.Cast(mode == "ToMe" ? GetReversePosition(PUC.Player.Position, target.Position) : target.Position,UsePackets());
		}

		private void SafeFriendLatern()
		{
			if(!W.IsReady())
				return;
			var bestcastposition = new Vector3(0f, 0f, 0f);
			foreach(
				var friend in
					PUC.AllHerosFriend 
						.Where(
							hero =>
								hero.Distance(PUC.Player) <= W.Range - 200 && hero.Health / hero.MaxHealth * 100 <= 20 && !hero.IsDead))
			{
				foreach(var enemy in PUC.AllHerosEnemy )
				{
					if(friend == null)
						continue;
					var center = ObjectManager.Player.Position;
					const int points = 36;
					var radius = W.Range;

					const double slice = 2 * Math.PI / points;
					for(var i = 0; i < points; i++)
					{
						var angle = slice * i;
						var newX = (int)(center.X + radius * Math.Cos(angle));
						var newY = (int)(center.Y + radius * Math.Sin(angle));
						var p = new Vector3(newX, newY, 0);
						if(p.Distance(friend.Position) <= bestcastposition.Distance(friend.Position))
							bestcastposition = p;
					}
					if(friend.Distance(ObjectManager.Player) <= W.Range)
					{
						W.Cast(W.GetPrediction( friend).CastPosition, UsePackets());
						return;
					}
				}
				if(bestcastposition.Distance(new Vector3(0f, 0f, 0f)) >= 100)
					W.Cast(bestcastposition, UsePackets());
			}
		}

		private void EngageFriendLatern()
		{
			if(!W.IsReady())
				return;
			var bestcastposition = new Vector3(0f, 0f, 0f);
			foreach(var friend in PUC.AllHerosFriend.Where(hero => !hero.IsMe && hero.Distance(PUC.Player) <= W.Range + 300 && hero.Distance(PUC.Player) <= W.Range - 300 && hero.Health / hero.MaxHealth * 100 >= 20 && EnemysinRange(150)))
			{
				var center = PUC.Player.Position;
				const int points = 36;
				var radius = W.Range;

				const double slice = 2 * Math.PI / points;
				for(var i = 0; i < points; i++)
				{
					var angle = slice * i;
					var newX = (int)(center.X + radius * Math.Cos(angle));
					var newY = (int)(center.Y + radius * Math.Sin(angle));
					var p = new Vector3(newX, newY, 0);
					if(p.Distance(friend.Position) <= bestcastposition.Distance(friend.Position))
						bestcastposition = friend.Position;
				}
				if(!(friend.Distance(PUC.Player) <= W.Range))
					continue;
				W.Cast(bestcastposition, UsePackets());
				return;
			}
			if(bestcastposition.Distance(new Vector3(0f, 0f, 0f)) >= 100)
				W.Cast(bestcastposition, UsePackets());
		}

		internal class QFollowTarget
		{
			public static bool Exist()
			{
				return ObjectManager.Get<Obj_AI_Base>().Any(unit => unit.HasBuff("ThreshQ") && !unit.IsMe);
			}

			public static Obj_AI_Base Get()
			{
				return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(unit => unit.HasBuff("ThreshQ") && !unit.IsMe);
			}

			public bool InTower()
			{
				return IsInsideEnemyTower(Get().Position);
			}

			public static bool ShouldCast()
			{
				if(!Exist())
					return false;
				var buff = Get().Buffs.FirstOrDefault(buf => buf.Name == "ThreshQ");
				if(buff == null)
					return false;
				return buff.EndTime - Game.Time < 0.5;
			}
		}
	}
}
