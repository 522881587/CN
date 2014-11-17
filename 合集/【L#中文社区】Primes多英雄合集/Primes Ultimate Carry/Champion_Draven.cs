using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
namespace Primes_Ultimate_Carry
{
	// ReSharper disable once InconsistentNaming
	class Champion_Draven :Champion 
	{
		public static List<Axe> AxeList = new List<Axe>();

		public Champion_Draven()
		{
			SetSpells();
			LoadMenu();

			GameObject.OnCreate += OnCreateObject;
			GameObject.OnDelete += OnDeleteObject;
			Game.OnGameUpdate += OnUpdate;
			Orbwalker.BeforeAttack += BeforeAttach;
			Drawing.OnDraw += OnDraw;
			Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
			AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
			PluginLoaded();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q);

			W = new Spell(SpellSlot.W);

			E = new Spell(SpellSlot.E, 1100);
			E.SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);

			R = new Spell(SpellSlot.R, 20000);
			R.SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);
		}

		private void LoadMenu()
		{
			ChampionMenu.AddSubMenu(new Menu("杩炴嫑", PUC.Player.ChampionName + "Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep0", "====== 杩炴嫑"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo", "= 浣跨敤 W").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo", "= 浣跨敤 E").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("楠氭壈", PUC.Player.ChampionName + "Harass"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== 楠氭壈"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass"), "ManaManager_Harass", 40);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useW_Harass", "= 浣跨敤 W").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("娓呭叺", PUC.Player.ChampionName + "LaneClear"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== 娓呭叺"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear"), "ManaManager_LaneClear", 20);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useQ_LaneClear", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useW_LaneClear", "= 浣跨敤 W").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear", "= 浣跨敤 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("RunLikeHell", PUC.Player.ChampionName + "RunLikeHell"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep0", "====== RunLikeHell"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useW_RunLikeHell", "= W to speed up").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("鏉傞」", PUC.Player.ChampionName + "Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep0", "====== 鏉傞」"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useE_Interrupt", "= E 鎵撴柇鎶€鑳姐劎").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useE_GapCloser", "= E 鍙嶇獊杩涖劎").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useCatchAxe_Combo", "= 杩炴嫑鎺ヤ綇鏂уご鑼冨洿").SetValue(new Slider(300, 0, 1000)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useCatchAxe_Harass", "= 楠氭壈鎺ヤ綇鏂уご鑼冨洿").SetValue(new Slider(400, 0, 1000)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useCatchAxe_LaneClear", "= 娓呭叺鎺ヤ綇鏂уご鑼冨洿").SetValue(new Slider(700, 0, 1000)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useCatchAxe_Lasthit", "= 琛ュ垁鎺ヤ綇鏂уご鑼冨洿").SetValue(new Slider(500, 0, 1000)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useW_SpeecBuffCatch", "= 浣跨敤 W 璧朵笂鎺ユ枾").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("缁樺埗", PUC.Player.ChampionName + "Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep0", "====== 缁樺埗"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Disabled", "绂佺敤鎵€鏈夈劎").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_E", "缁樺埗 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_CatchRange", "缁樺埗鎺ユ枾鑼冨洿").SetValue(true));


			var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "缁樺埗鎹熶激").SetValue(true);
			Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
			Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
			dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
			{
				Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
			};
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(dmgAfterComboItem);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep1", "========="));

			PUC.Menu.AddSubMenu(ChampionMenu);
		}

		private void OnDraw(EventArgs args)
		{
			Orbwalker.AllowDrawing = !ChampionMenu.Item("Draw_Disabled").GetValue<bool>();

			if(ChampionMenu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(ChampionMenu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_CatchRange").GetValue<bool>())
				if (Q.Level > 0)
				{
					switch(Orbwalker.CurrentMode)
					{
						case Orbwalker.Mode.Combo:
							Utility.DrawCircle(Game.CursorPos, ChampionMenu.Item("useCatchAxe_Combo").GetValue<Slider>().Value, Color.Blue);	
							break;
						case Orbwalker.Mode.Harass:
							Utility.DrawCircle(Game.CursorPos, ChampionMenu.Item("useCatchAxe_Harass").GetValue<Slider>().Value, Color.Blue);
							break;
						case Orbwalker.Mode.LaneClear:
							Utility.DrawCircle(Game.CursorPos, ChampionMenu.Item("useCatchAxe_LaneClear").GetValue<Slider>().Value, Color.Blue);
							break;
						case Orbwalker.Mode.Lasthit:
							Utility.DrawCircle(Game.CursorPos, ChampionMenu.Item("useCatchAxe_Lasthit").GetValue<Slider>().Value, Color.Blue);
							break;
					}
				}
					
		}
		private float GetComboDamage(Obj_AI_Base enemy)
		{
			var damage = 0d;
			if(Q.IsReady())
				damage += PUC.Player.GetSpellDamage(enemy, SpellSlot.Q);
			if(W.IsReady())
				damage += PUC.Player.GetSpellDamage(enemy, SpellSlot.W);
			if(E.IsReady())
				damage += PUC.Player.GetSpellDamage(enemy, SpellSlot.E);
			if(R.IsReady())
				damage += PUC.Player.GetSpellDamage(enemy, SpellSlot.R);
			damage += PUC.Player.GetAutoAttackDamage(enemy)*2;
			return (float)damage;
		}

		private void OnUpdate(EventArgs args)
		{
			CatchAxe();
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
						CastQ();
					if(ChampionMenu.Item("useW_Combo").GetValue<bool>())
						CastW();
					if (ChampionMenu.Item("useE_Combo").GetValue<bool>())
						Cast_BasicSkillshot_Enemy(E);
					break;
				case Orbwalker.Mode.Harass:
					if(ChampionMenu.Item("useQ_Harass").GetValue<bool>() && ManamanagerAllowCast("ManaManager_Harass"))
						CastQ();
					if(ChampionMenu.Item("useW_Harass").GetValue<bool>() && ManamanagerAllowCast("ManaManager_Harass"))
						CastW();
					break;
				case Orbwalker.Mode.LaneClear:
					if(ChampionMenu.Item("useQ_LaneClear").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
						CastQ();
					if(ChampionMenu.Item("useW_LaneClear").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
						CastW();
					if(ChampionMenu.Item("useE_LaneClear").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
						Cast_BasicSkillshot_Enemy(E);
					break;
			}
		}

		private void CatchAxe()
		{
			if (AxeList.Count > 0)
			{
				Axe[] axe = {null};
				foreach (var obj in AxeList.Where(obj => axe[0] == null || obj.CreationTime < axe[0].CreationTime))
					axe[0] = obj;
				if (axe[0] != null)
				{
					var distanceNorm = Vector2.Distance(axe[0].Position.To2D(), PUC.Player.ServerPosition.To2D()) - PUC.Player.BoundingRadius;
					var distanceBuffed = PUC.Player.GetPath(axe[0].Position).ToList().To2D().PathLength();
					var canCatchAxeNorm = distanceNorm / PUC.Player.MoveSpeed + Game.Time < axe[0].EndTime;
					var canCatchAxeBuffed = distanceBuffed / (PUC.Player.MoveSpeed + (5 * W.Level + 35) * 0.01 * PUC.Player.MoveSpeed + Game.Time) < axe[0].EndTime;

					if (!ChampionMenu.Item("useW_SpeecBuffCatch").GetValue<bool>())
						if (!canCatchAxeNorm)
						{
							AxeList.Remove(axe[0]);
							return;
						}

					if ((!(axe[0].Position.Distance(Game.CursorPos) < ChampionMenu.Item("useCatchAxe_Combo").GetValue<Slider>().Value) ||
					     Orbwalker.CurrentMode != Orbwalker.Mode.Combo) &&
					    (!(axe[0].Position.Distance(Game.CursorPos) < ChampionMenu.Item("useCatchAxe_Harass").GetValue<Slider>().Value) ||
					     Orbwalker.CurrentMode != Orbwalker.Mode.Harass) &&
					    (!(axe[0].Position.Distance(Game.CursorPos) < ChampionMenu.Item("useCatchAxe_LaneClear").GetValue<Slider>().Value) ||
					     Orbwalker.CurrentMode != Orbwalker.Mode.LaneClear) &&
					    (!(axe[0].Position.Distance(Game.CursorPos) < ChampionMenu.Item("useCatchAxe_Lasthit").GetValue<Slider>().Value) ||
					     Orbwalker.CurrentMode != Orbwalker.Mode.Lasthit)) 
						return;
					if(canCatchAxeBuffed && !canCatchAxeNorm && W.IsReady() && !axe[0].Catching())
						W.Cast();
					Orbwalker.CustomOrbwalkMode = true;
					Orbwalker.Orbwalk(GetModifiedPosition(axe[0].Position, Game.CursorPos, 49 + PUC.Player.BoundingRadius / 2), Orbwalker.GetPossibleTarget());
				}
				
			}
			else
				Orbwalker.CustomOrbwalkMode = false;
		}

		private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
		{
			if(!ChampionMenu.Item("useE_GapCloser").GetValue<bool>())
				return;
			if (!(gapcloser.End.Distance(PUC.Player.ServerPosition) <= 100)) 
				return;
			E.CastIfHitchanceEquals(gapcloser.Sender, HitChance.Medium, UsePackets());
		}

		private void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
		{
			if (!ChampionMenu.Item("useE_Interrupt").GetValue<bool>())
				return;
			E.CastIfHitchanceEquals(unit,HitChance.Medium,UsePackets());			
		}

		private static void OnCreateObject(GameObject sender, EventArgs args)
		{
			if (!sender.Name.Contains("Q_reticle_self"))
				return;
			AxeList.Add(new Axe(sender));
		}

		private static void OnDeleteObject(GameObject sender, EventArgs args)
		{
			if (!sender.Name.Contains("Q_reticle_self"))
				return;
			foreach (var axe in AxeList.Where(axe => axe.NetworkId == sender.NetworkId))
			{
				AxeList.Remove(axe);
				return;
			}
		}

		private void BeforeAttach(Orbwalker.BeforeAttackEventArgs args)
		{
			
		}

		private void CastQ()
		{
			if(!Q.IsReady())
				return;
			if(GetQStacks() > 0 || AxeList.Count > 2)
				return;
			var target = TargetSelector.GetAATarget();
			if(target != null)
				Q.Cast();

			if(Orbwalker.CurrentMode != Orbwalker.Mode.LaneClear)
				return;
			var allMinion = MinionManager.GetMinions(PUC.Player.Position,
				Orbwalker.GetAutoAttackRangeto(), MinionTypes.All, MinionTeam.NotAlly);
			if(!allMinion.Any(minion => minion.IsValidTarget(Orbwalker.GetAutoAttackRangeto(minion))))
				return;
			Q.Cast();
		}

		private void CastW()
		{
			if(!W.IsReady())
				return;

			var target = TargetSelector.GetAATarget();
			if(target != null)
				W.Cast();

			if(Orbwalker.CurrentMode != Orbwalker.Mode.LaneClear)
				return;
			var allMinion = MinionManager.GetMinions(PUC.Player.Position,
				Orbwalker.GetAutoAttackRangeto(), MinionTypes.All, MinionTeam.NotAlly);
			if(!allMinion.Any(minion => minion.IsValidTarget(Orbwalker.GetAutoAttackRangeto(minion))))
				return;
			W.Cast();
		}

		public static int GetQStacks()
		{
			var buff = ObjectManager.Player.Buffs.FirstOrDefault(buff1 => buff1.Name.Equals("dravenspinningattack"));
			return buff != null ? buff.Count : 0;
		}

		internal class Axe
		{
			public GameObject AxeObject;
			public double CreationTime;
			public double EndTime;
			public int NetworkId;
			public Vector3 Position;

			public Axe(GameObject axeObject)
			{
				AxeObject = axeObject;
				NetworkId = axeObject.NetworkId;
				Position = axeObject.Position;
				CreationTime = Game.Time;
				EndTime = CreationTime + 1.2;
			}

			public float DistanceToPlayer()
			{
				return ObjectManager.Player.Distance(Position);
			}

			public bool Catching()
			{
				return PUC.Player.Position.Distance(Position) <= 49 + PUC.Player.BoundingRadius/2 + 50;
			}
		}
	}
}
