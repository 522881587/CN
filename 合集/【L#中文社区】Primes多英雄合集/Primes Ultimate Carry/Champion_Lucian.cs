using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Menu = LeagueSharp.Common.Menu;
using MenuItem = LeagueSharp.Common.MenuItem;

namespace Primes_Ultimate_Carry
{
	// ReSharper disable once InconsistentNaming
	class Champion_Lucian : Champion
	{
		public Spell Q2;
		public bool HavePassiveUp;
		public int Delay = 150;
		public int DelayTick = 0;

		public bool RActive;
		public int SpellCastetTick;
		public bool CanUseSpells = true;
		public bool WaitingForBuff = false;
		public bool GainBuff = false;

		public Champion_Lucian()
		{
			SetSpells();
			LoadMenu();

			Game.OnGameUpdate += OnUpdate;
			Drawing.OnDraw += OnDraw;
			Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

			PluginLoaded();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q, 675);
			Q.SetTargetted(0.5f, float.MaxValue);

			Q2 = new Spell(SpellSlot.Q, 1100);
			Q2.SetSkillshot(0.5f, 5f, float.MaxValue, true, SkillshotType.SkillshotLine);

			W = new Spell(SpellSlot.W, 1000);
			W.SetSkillshot(0.3f, 80f, 1600, true, SkillshotType.SkillshotLine);

			E = new Spell(SpellSlot.E, 475);

			R = new Spell(SpellSlot.R, 1400);
			R.SetSkillshot(0.01f, 110, 2800f, true, SkillshotType.SkillshotLine);
		}

		private void LoadMenu()
		{
			ChampionMenu.AddSubMenu(new Menu("妯″紡", PUC.Player.ChampionName + "Modes"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Modes").AddItem(new MenuItem("sep0", "====== 妯″紡"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Modes").AddItem(new MenuItem("use_Mode", "= 妯″紡").SetValue(new StringList(new[] { "Gangster", "Gentlemen", "Hybrid" })));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Modes").AddItem(new MenuItem("sep1", "========="));


			ChampionMenu.AddSubMenu(new Menu("杩炴嫑", PUC.Player.ChampionName + "Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep0", "====== 杩炴嫑"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo", "= 浣跨敤 W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo", "= 浣跨敤 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo_maxrange", "= 脣鏈€澶ц寖鍥村唴鐨勬晫浜恒劎").SetValue(new Slider(1100, 2000, 500)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo", "= 浣跨敤 R").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo_Filler", "= R 濡傛灉鍦–D").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("楠氭壈", PUC.Player.ChampionName + "Harass"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== 楠氭壈"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass"), "ManaManager_Harass", 40);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useW_Harass", "= 浣跨敤 W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("娓呭叺", PUC.Player.ChampionName + "LaneClear"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== 娓呭叺"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear"), "ManaManager_LaneClear", 20);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useQ_LaneClear", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useW_LaneClear", "= 浣跨敤 W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear", "= 浣跨敤 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear_maxrange", "= E 鏈€澶ц寖鍥寸殑灏忓叺").SetValue(new Slider(1100, 2000, 500)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear_dangerzones", "= E 鍗遍櫓鍖哄煙鍐??").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("杩芥潃", PUC.Player.ChampionName + "RunLikeHell"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep0", "====== 杩芥潃"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useE_RunLikeHell", "= 浣跨敤 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useE_RunLikeHell_passive", "= 蹇界暐琚姩").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("鏉傞」", PUC.Player.ChampionName + "Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep0", "====== 鏉傞」"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("usePassive_care", "= 浣跨敤琚姩").SetValue(true));
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

		private void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
		{
			if(!sender.IsMe)
				return;
			switch(args.SData.Name)
			{
				case "LucianQ":
					HavePassiveUp = true;
					Utility.DelayAction.Add(500, SetPassive);
					break;
				case "LucianW":
					HavePassiveUp = true;
					Utility.DelayAction.Add(500, SetPassive);
					break;
				case "LucianE":
					UsedSkill();
					HavePassiveUp = true;
					Utility.DelayAction.Add(500, SetPassive);
					break;
				case "LucianR":
					HavePassiveUp = true;
					Utility.DelayAction.Add(500, SetPassive);
					break;
				case "LucianPassiveAttack":
					Utility.DelayAction.Add(50, SetPassive);
					break;
			}
		}

		private void SetPassive()
		{
			if(PUC.Player.Buffs.Any(buff => buff.Name == "LucianR"))
			{
				Utility.DelayAction.Add(100, SetPassive);
				return;
			}
			if(PUC.Player.Buffs.All(buff => buff.Name != "lucianpassivebuff"))
				HavePassiveUp = false;
			else
				Utility.DelayAction.Add(100, SetPassive);
		}

		public Vector3 HitPosition(Vector3 unit, int range)
		{
			var me = ObjectManager.Player.Position;
			var mouse = unit;

			var newpos = mouse - me;
			newpos.Normalize();
			return unit + (newpos * range);
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

		private void OnUpdate(EventArgs args)
		{
			switch(PUC.Menu.Item("use_Mode").GetValue<StringList>().SelectedIndex)
			{
				case 1:
					switch(Orbwalker.CurrentMode)
					{
						case Orbwalker.Mode.Combo:
							ComboGentleMan();
							break;
						case Orbwalker.Mode.Harass:
							HarassGentleMan();
							break;
						case Orbwalker.Mode.LaneClear:
							LaneClearGentleMan();
							break;
						case Orbwalker.Mode.RunlikeHell:
							RunlikeHellGentleMan();
							break;
					}
					break;
				case 0:
					BuffCheck();
					UltCheck();

					switch(Orbwalker.CurrentMode)
					{
						case Orbwalker.Mode.Combo:
							if(ChampionMenu.Item("useE_Combo").GetValue<bool>())
								GansterCastE();
							if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
								GansterCastQEnemy();
							if(ChampionMenu.Item("useW_Combo").GetValue<bool>())
								GansterCastWEnemy();
							if(ChampionMenu.Item("useR_Combo").GetValue<bool>() ||
								ChampionMenu.Item("useR_Combo_Filler").GetValue<bool>())
								CastREnemy();
							break;
						case Orbwalker.Mode.Harass:
							if(ChampionMenu.Item("useQ_Harass").GetValue<bool>() && ManamanagerAllowCast("ManaManager_Harass"))
								GansterCastQEnemy();
							if(ChampionMenu.Item("useW_Harass").GetValue<bool>() && ManamanagerAllowCast("ManaManager_Harass"))
								GansterCastWEnemy();
							break;
						case Orbwalker.Mode.LaneClear:
							if(ChampionMenu.Item("useE_LaneClear").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
								GansterCastE();
							if(ChampionMenu.Item("useQ_LaneClear").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
							{
								GansterCastQEnemy();
								GansterCastQMinion();
							}
							if(ChampionMenu.Item("useW_LaneClear").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
							{
								GansterCastWEnemy();
								GansterCastWMinion();
							}
							break;
						case Orbwalker.Mode.RunlikeHell:
							RunlikeHellGentleMan();
							break;
					}
					break;
				case 2:
					BuffCheck();
					UltCheck();

					switch(Orbwalker.CurrentMode)
					{
						case Orbwalker.Mode.Combo:
							if(ChampionMenu.Item("useE_Combo").GetValue<bool>())
								GansterCastE();
							if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
								GansterCastQEnemy();
							if(ChampionMenu.Item("useW_Combo").GetValue<bool>())
								GansterCastWEnemy();
							if(ChampionMenu.Item("useR_Combo").GetValue<bool>() ||
								ChampionMenu.Item("useR_Combo_Filler").GetValue<bool>())
								CastREnemy();
							break;
						case Orbwalker.Mode.Harass:
							HarassGentleMan();
							break;
						case Orbwalker.Mode.LaneClear:
							LaneClearGentleMan();
							break;
						case Orbwalker.Mode.RunlikeHell:
							RunlikeHellGentleMan();
							break;
					}
					break;
			}
		}

		private void GansterCastQEnemy()
		{
			if(!Q.IsReady() || !CanUseSpells)
				return;
			var target = TargetSelector.GetTarget(Q.Range);

			if(target != null)
			{
				if((target.IsValidTarget(Q.Range)))
				{
					Q.CastOnUnit(target, UsePackets());
					UsedSkill();
				}
			}
			target = TargetSelector.GetTarget(Q2.Range);
			if(target == null)
				return;
			if((!target.IsValidTarget(Q2.Range)) || !CanUseSpells || !Q.IsReady())
				return;
			var qCollision = Q2.GetPrediction(target).CollisionObjects;
			foreach(var qCollisionChar in qCollision.Where(qCollisionChar => qCollisionChar.IsValidTarget(Q.Range)))
			{
				if(qCollisionChar is Obj_AI_Hero)
				{
					Q.CastOnUnit(qCollisionChar, UsePackets());
					UsedSkill();
					return;
				}
				for(var i = 10; i < 1070 - Q.Range; i = i + 10)
				{
					if(!(HitPosition(Q.GetPrediction(qCollisionChar).UnitPosition, i).Distance(Q2.GetPrediction(target).UnitPosition) < 35))
						continue;
					Q.CastOnUnit(qCollisionChar, UsePackets());
					UsedSkill();
					return;
				}
			}
		}

		private void GansterCastQMinion()
		{
			if(!Q.IsReady() || !CanUseSpells)
				return;
			var laneClear = Orbwalker.CurrentMode == Orbwalker.Mode.LaneClear;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
			if(!laneClear)
				return;
			var minion =
				allMinions.FirstOrDefault(
					minionn => minionn.Distance(ObjectManager.Player) <= Q.Range);
			if(minion == null)
				return;
			Q.CastOnUnit(minion, UsePackets());
			UsedSkill();
		}

		private void GansterCastWEnemy()
		{
			if(!W.IsReady() || !CanUseSpells)
				return;
			var target = TargetSelector.GetTarget(W.Range);
			if(target == null)
				return;
			if(target.IsValidTarget(W.Range) && W.GetPrediction(target).Hitchance >= HitChance.High)
			{
				W.Cast(target, UsePackets());
				UsedSkill();
			}
			else if(W.GetPrediction(target).Hitchance == HitChance.Collision)
			{
				var wCollision = W.GetPrediction(target).CollisionObjects;
				foreach(var wCollisionChar in wCollision.Where(wCollisionChar => wCollisionChar.Distance(target) <= 100))
				{
					W.Cast(wCollisionChar.Position, UsePackets());
					UsedSkill();
				}
			}
		}

		private void GansterCastE()
		{
			if(!E.IsReady() || !CanUseSpells)
				return;
			var combo = Orbwalker.CurrentMode == Orbwalker.Mode.Combo;
			var comboRange = ChampionMenu.Item("useE_Combo_maxrange").GetValue<Slider>().Value;

			var laneClear = Orbwalker.CurrentMode == Orbwalker.Mode.LaneClear;
			var laneClearRange = ChampionMenu.Item("useE_LaneClear_maxrange").GetValue<Slider>().Value;


			if(combo)
			{
				var target = TargetSelector.GetTarget(comboRange);
				if(!target.IsValidTarget(comboRange))
					return;
				E.Cast(Game.CursorPos, UsePackets());
				UsedSkill();
			}
			else if(laneClear)
			{
				var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, laneClearRange, MinionTypes.All, MinionTeam.NotAlly);
				foreach(var minion in allMinions.Where(minion => minion != null).Where(minion => minion.IsValidTarget(laneClearRange) && E.IsReady()))
				{
					E.Cast(Game.CursorPos, UsePackets());
					UsedSkill();
				}
			}
		}

		private void GansterCastWMinion()
		{
			if(!W.IsReady() || !CanUseSpells)
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, W.Range + 100, MinionTypes.All,
				MinionTeam.NotAlly);
			var minion = allMinions.FirstOrDefault(minionn => minionn.IsValidTarget(W.Range));
			if(minion != null)
			{
				W.Cast(minion.Position, UsePackets());
				UsedSkill();
			}
		}

		private void ComboGentleMan()
		{
			if(ChampionMenu.Item("useE_Combo").GetValue<bool>())
				CastE(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
			if(ChampionMenu.Item("useW_Combo").GetValue<bool>())
				CastW(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
			if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
				CastQ(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
			if(ChampionMenu.Item("useR_Combo").GetValue<bool>() || ChampionMenu.Item("useR_Combo_Filler").GetValue<bool>())
				CastREnemy();
		}

		private void CastREnemy()
		{
			if((ChampionMenu.Item("useR_Combo_Filler").GetValue<bool>() && (Q.IsReady() || W.IsReady() || E.IsReady())) || (!R.IsReady() || !CanUseSpells))
				return;
			var target = TargetSelector.GetTarget(R.Range);
			if(target.IsValidTarget(R.Range))
			{
				R.Cast(target, UsePackets());
				UsedSkill();
				RActive = true;
			}
		}

		private void UsedSkill()
		{
			if(!CanUseSpells)
				return;
			CanUseSpells = false;
			SpellCastetTick = Environment.TickCount;
		}

		private void UltCheck()
		{
			var tempultactive = false;
			foreach(var buff in PUC.Player.Buffs.Where(buff => buff.Name == "LucianR"))
				tempultactive = true;

			if(tempultactive)
			{
				Orbwalker.DisableAttack();
				RActive = true;
			}
			if(!tempultactive)
			{
				Orbwalker.EnableAttack();
				RActive = false;
			}
		}

		private void BuffCheck()
		{
			if(CanUseSpells == false && WaitingForBuff == false && GainBuff == false)
				WaitingForBuff = true;

			if(WaitingForBuff)
				foreach(var buff in PUC.Player.Buffs.Where(buff => buff.Name == "lucianpassivebuff"))
					GainBuff = true;

			if(GainBuff)
			{
				WaitingForBuff = false;
				var tempgotBuff = false;
				foreach(var buff in PUC.Player.Buffs.Where(buff => buff.Name == "lucianpassivebuff"))
					tempgotBuff = true;
				if(tempgotBuff == false)
				{
					GainBuff = false;
					CanUseSpells = true;
				}
			}

			if(SpellCastetTick >= Environment.TickCount - 1000 || WaitingForBuff != true)
				return;
			WaitingForBuff = false;
			GainBuff = false;
			CanUseSpells = true;
		}

		private void HarassGentleMan()
		{
			if(ChampionMenu.Item("useW_Harass").GetValue<bool>())
				CastW();
			if(ChampionMenu.Item("useQ_Harass").GetValue<bool>())
				CastQ(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
		}

		private void LaneClearGentleMan()
		{
			if(ChampionMenu.Item("useE_LaneClear").GetValue<bool>())
				CastE(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
			if(ChampionMenu.Item("useW_LaneClear").GetValue<bool>())
				CastW(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
			if(ChampionMenu.Item("useQ_LaneClear").GetValue<bool>())
				CastQ(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
		}

		private void RunlikeHellGentleMan()
		{
			if(ChampionMenu.Item("useE_RunLikeHell").GetValue<bool>())
				CastE(ChampionMenu.Item("useE_RunLikeHell_passive").GetValue<bool>());
		}

		private void CastQ(bool iggnorePassive = false)
		{
			if(!Q.IsReady() || (HavePassiveUp && !iggnorePassive))
				return;
			if(Delay >= Environment.TickCount - DelayTick)
				return;
			var targetNormal = TargetSelector.GetTarget(Q.Range);
			var targetExtended = TargetSelector.GetTarget(Q2.Range);
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					foreach(var obj in PUC.AllHerosEnemy.Where(hero => hero.IsValidTarget(Q2.Range)).SelectMany(enemy1 => ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.IsValidTarget(Q.Range) && (obj.ServerPosition.To2D().Distance(PUC.Player.ServerPosition.To2D(), Q2.GetPrediction(enemy1).UnitPosition.To2D(), true) < 35) || obj is Obj_AI_Hero)))
					{
						Q.CastOnUnit(obj,UsePackets() );
						DelayTick = Environment.TickCount;
						return;
					}
					break;
				case Orbwalker.Mode.Harass:
					if(!ManamanagerAllowCast("ManaManager_Harass"))
						return;
					foreach(var obj in PUC.AllHerosEnemy.Where(hero => hero.IsValidTarget(Q2.Range)).SelectMany(enemy1 => ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.IsValidTarget(Q.Range) && (obj.ServerPosition.To2D().Distance(PUC.Player.ServerPosition.To2D(), Q2.GetPrediction(enemy1).UnitPosition.To2D(), true) < 35) || obj is Obj_AI_Hero)))
					{
						Q.CastOnUnit(obj, UsePackets());
						DelayTick = Environment.TickCount;
						return;
					}
					break;
				case Orbwalker.Mode.LaneClear:
					if(!ManamanagerAllowCast("ManaManager_LaneClear"))
						return;
					var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
					var minion = allMinions.FirstOrDefault(minionn => minionn.Distance(ObjectManager.Player) <= Q.Range && HealthPrediction.LaneClearHealthPrediction(minionn, 500) > 0);
					if(minion == null)
						return;
					Q.CastOnUnit(minion, UsePackets());
					DelayTick = Environment.TickCount;
					break;
			}
		}

		private void CastW(bool iggnorePassive = false)
		{
			if(!W.IsReady() || (HavePassiveUp && !iggnorePassive))
				return;
			if(Delay >= Environment.TickCount - DelayTick)
				return;

			var target = TargetSelector.GetTarget(W.Range);
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(target.IsValidTarget(W.Range) && W.GetPrediction(target).Hitchance >= HitChance.High)
					{
						W.Cast(target, UsePackets());
						DelayTick = Environment.TickCount;
					}
					else if(W.GetPrediction(target).Hitchance == HitChance.Collision)
					{
						var wCollision = W.GetPrediction(target).CollisionObjects;
						if(!wCollision.Any(wCollisionChar => wCollisionChar.Distance(target) >= 100))
						{
							W.Cast(target, UsePackets());
							DelayTick = Environment.TickCount;
						}
					}
					break;
				case Orbwalker.Mode.Harass:
					if(!ManamanagerAllowCast("ManaManager_Harass"))
						return;
					if(target.IsValidTarget(W.Range) && W.GetPrediction(target).Hitchance >= HitChance.High)
					{
						W.Cast(target, UsePackets());
						DelayTick = Environment.TickCount;
					}
					else if(W.GetPrediction(target).Hitchance == HitChance.Collision)
					{
						var wCollision = W.GetPrediction(target).CollisionObjects;
						if(!wCollision.Any(wCollisionChar => wCollisionChar.Distance(target) >= 100))
						{
							W.Cast(target, UsePackets());
							DelayTick = Environment.TickCount;
						}
					}
					break;
				case Orbwalker.Mode.LaneClear:
					if(!ManamanagerAllowCast("ManaManager_LaneClear"))
						return;
					var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, W.Range, MinionTypes.All, MinionTeam.NotAlly);
					var minion = allMinions.FirstOrDefault(minionn => minionn.IsValidTarget(W.Range) && HealthPrediction.LaneClearHealthPrediction(minionn, 500) > 0);
					if(minion != null)
					{
						W.Cast(minion, UsePackets());
						DelayTick = Environment.TickCount;
					}
					break;
			}
		}

		private void CastE(bool iggnorePassive = false)
		{
			if(!E.IsReady() || (HavePassiveUp && !iggnorePassive))
				return;
			if(Delay >= Environment.TickCount - DelayTick)
				return;
			DelayTick = Environment.TickCount;
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(TargetSelector.GetTarget(PUC.Menu.Item("useE_Combo_maxrange").GetValue<Slider>().Value) != null)
					{
						E.Cast(Game.CursorPos, UsePackets());
						DelayTick = Environment.TickCount;
					}
					break;
				case Orbwalker.Mode.LaneClear:
					var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, PUC.Menu.Item("useE_LaneClear_maxrange").GetValue<Slider>().Value, MinionTypes.All,
						MinionTeam.NotAlly);
					if(allMinions.Where(minion => minion != null).Any(minion => minion.IsValidTarget(PUC.Menu.Item("useE_LaneClear_maxrange").GetValue<Slider>().Value) && E.IsReady()))
					{
						E.Cast(Game.CursorPos, UsePackets());
						DelayTick = Environment.TickCount;
					}
					break;
				case Orbwalker.Mode.RunlikeHell:
					if(Game.CursorPos.Distance(PUC.Player.Position) > E.Range && E.IsReady())
					{
						E.Cast(Game.CursorPos, UsePackets());
						DelayTick = Environment.TickCount;
					}
					break;
			}
		}
	}
}
