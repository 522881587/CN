using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using Menu = LeagueSharp.Common.Menu;
using MenuItem = LeagueSharp.Common.MenuItem;

namespace Primes_Ultimate_Carry
{	
	// ReSharper disable once InconsistentNaming
	class Champion_Aatrox : Champion
	{
		public Champion_Aatrox()
		{
			SetSpells();
			LoadMenu();

			Game.OnGameUpdate += OnUpdate;
			Drawing.OnDraw += OnDraw;

			PluginLoaded();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q, 650);
			Q.SetSkillshot((float)0.27, 280, 1800, false, SkillshotType.SkillshotCircle);

			W = new Spell(SpellSlot.W);

			E = new Spell(SpellSlot.E,1000);
			E.SetSkillshot((float)0.27,80,1200,false,SkillshotType.SkillshotLine);

			R = new Spell(SpellSlot.R,300);			
		}

		private void LoadMenu()
		{
			ChampionMenu.AddSubMenu(new Menu("杩炴嫑", PUC.Player.ChampionName + "Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep0", "====== 杩炴嫑"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo", "= 浣跨敤 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo_Amount", "= R 鏁屼汉璺濈").SetValue(new Slider(2, 5, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo_EnemyHealh", "= R 瀵规晫浜虹殑锛呭仴搴枫劎 <").SetValue(new Slider(60, 100, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("楠氭壈", PUC.Player.ChampionName + "Harass"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== 楠氭壈"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass_dangerzones", "= 鍗遍櫓鍖哄煙鍐呯殑Q ?").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useE_Harass", "= 浣跨敤 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("娓呭叺", PUC.Player.ChampionName + "LaneClear"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== 娓呭叺"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useQ_LaneClear", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear", "= 浣跨敤 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("Lasthit", "Lasthit"));

			ChampionMenu.AddSubMenu(new Menu("杩芥潃", PUC.Player.ChampionName + "RunLikeHell"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep0", "====== 杩芥潃"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useQ_RunLikeHell", "= Q 璺熼殢榧犳爣").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useE_RunLikeHell", "= E 瀵规晫浜恒劎").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("鏉傞」", PUC.Player.ChampionName + "Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep0", "====== 鏉傞」"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useW_autoswitch", "= 鑷姩寮€鍏砏").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useW_autoswitch_health", "= 鍙樺寲鐨勭櫨鍒嗘瘮鍋ュ悍").SetValue(new Slider(60, 100, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useW_autoswitch_prioheal", "= 娌荤枟浼樺厛").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("缁樺埗", PUC.Player.ChampionName + "Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep0", "====== 缁樺埗"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Disabled", "绂佹缁樺埗").SetValue(false));
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

			if(ChampionMenu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
	
		}

		private void OnUpdate(EventArgs args)
		{
			AutomaticW();
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					Combo();
					break;
				case Orbwalker.Mode.Harass:
					Harass();
					break;
				case Orbwalker.Mode.LaneClear :
					LaneClear();
					break;
				case Orbwalker.Mode.RunlikeHell:
					RunlikeHell();
					break;
			}
		}

		private void AutomaticW()
		{
			if(!W.IsReady())
				return;
			if(!ChampionMenu.Item("useW_autoswitch").GetValue<bool>())
				return;
			if(Orbwalker.CurrentMode == Orbwalker.Mode.Combo)
			{
				if(PUC.Player.Health / PUC.Player.MaxHealth * 100 <
					ChampionMenu.Item("useW_autoswitch_health").GetValue<Slider>().Value)
				{
					WtoHeal();
					return;
				}
				WtoDamage();
				return;

			}
			if(ChampionMenu.Item("useW_autoswitch").GetValue<bool>())
			{
				if(PUC.Player.Health / PUC.Player.MaxHealth * 100 < 95)
					WtoHeal();
				else
					WtoDamage();
			}
			else
			{
				if(PUC.Player.Health / PUC.Player.MaxHealth * 100 <
					ChampionMenu.Item("useW_autoswitch_health").GetValue<Slider>().Value)
					WtoHeal();
				else
					WtoDamage();
			}

		}

		private void Combo()
		{
			if(ChampionMenu.Item("useE_Combo").GetValue<bool>())
				CastE();
			if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
				CastQ();
			CastR();
		}

		private void Harass()
		{
			if(ChampionMenu.Item("useE_Harass").GetValue<bool>())
				CastE();
			if(ChampionMenu.Item("useQ_Harass").GetValue<bool>())
				CastQ();
		}

		private void LaneClear()
		{
			if (ChampionMenu.Item("useQ_LaneClear").GetValue<bool>())
				Cast_BasicSkillshot_AOE_Farm(Q);
			if(ChampionMenu.Item("useE_LaneClear").GetValue<bool>())
				Cast_BasicSkillshot_AOE_Farm(E);
		}

		private void RunlikeHell()
		{
			if(ChampionMenu.Item("useE_RunLikeHell").GetValue<bool>())
				CastE();
			if(ChampionMenu.Item("useQ_RunLikeHell").GetValue<bool>())
				if (Game.CursorPos.Distance(PUC.Player.Position) > Q.Range && Q.IsReady())
					Q.Cast(Game.CursorPos, UsePackets());
		}

		private void WtoDamage()
		{
			if(GetSpellName(SpellSlot.W) == "aatroxw2" || !W.IsReady())
				return;
			W.Cast();
		}

		private void WtoHeal()
		{
			if(GetSpellName(SpellSlot.W) == "AatroxW" || !W.IsReady())
				return;
			W.Cast();
		}

		private void CastQ()
		{
			var target = TargetSelector.GetTarget(Q.Range + Q.Width / 2);
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(target != null && PUC.Player.Distance(target) > Orbwalker.GetAutoAttackRangeto(target))
						Cast_BasicSkillshot_Enemy(Q);
					break;
				case Orbwalker.Mode.Harass:
					if(target != null && PUC.Player.Distance(target) > Orbwalker.GetAutoAttackRangeto(target))
						if (!ChampionMenu.Item("useQ_Harass_dangerzones").GetValue<bool>())
						{
							if (!IsInsideEnemyTower(Q.GetPrediction(target).CastPosition))
								Cast_BasicSkillshot_Enemy(Q);
						}
						else
							Cast_BasicSkillshot_Enemy(Q);
			
					break;
			}
		}

		private void CastE()
		{
			var target = TargetSelector.GetTarget(E.Range);
			if (target != null)
				Cast_BasicSkillshot_Enemy(E);
		}

		private void CastR()
		{
			if(!R.IsReady())
				return;
			if(ChampionMenu.Item("useR_Combo_Amount").GetValue<Slider>().Value > 0)
				if(EnemysinRange(R.Range, ChampionMenu.Item("useR_Combo_Amount").GetValue<Slider>().Value))
				{
					R.Cast();
					return;
				}
			if(ChampionMenu.Item("useR_Combo_EnemyHealh").GetValue<Slider>().Value <= 0)
				return;
			if(!PUC.AllHerosEnemy.Any(hero => hero.IsValidTarget(R.Range) &&
											  hero.Health / hero.MaxHealth * 100 <
											  ChampionMenu.Item("useR_Combo_EnemyHealh").GetValue<Slider>().Value))
				return;
			R.Cast();
		}

	}
}
