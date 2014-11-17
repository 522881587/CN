using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Primes_Ultimate_Carry
{

	// ReSharper disable once InconsistentNaming
	class Champion_Gnar : Champion 
	{
		public Spell QMini;
		public Spell QMega;
		public Spell EMini;
		public Spell EMega;

		public string TransformSoon = "gnartransformsoon";
		public string Transformed = "gnartransform";
		public int GnarState = 1;

		public Champion_Gnar()
		{
			SetSpells();
			LoadMenu();

			Game.OnGameUpdate += OnUpdate;
			Drawing.OnDraw += OnDraw;

			PluginLoaded();
		}

		private void SetSpells()
		{
			QMini = new Spell(SpellSlot.Q, 1100f);
			QMini.SetSkillshot(0.066f, 60f, 1400f, true, SkillshotType.SkillshotLine);
			
			QMega = new Spell(SpellSlot.Q, 1100f);
			QMega.SetSkillshot(0.60f, 90f, 2100f, true, SkillshotType.SkillshotLine);
			
			W = new Spell(SpellSlot.W, 525f);
			W.SetSkillshot(0.25f, 80f, 1200f, false, SkillshotType.SkillshotLine);
			
			EMini = new Spell(SpellSlot.E, 475f);
			EMini.SetSkillshot(0.695f, 150f, 2000f, false, SkillshotType.SkillshotCircle);
			
			EMega = new Spell(SpellSlot.E, 475f);
			EMega.SetSkillshot(0.695f, 350f, 2000f, false, SkillshotType.SkillshotCircle);
			
			R = new Spell(SpellSlot.R, 1f);
			R.SetSkillshot(0.066f, 400f, 1400f, false, SkillshotType.SkillshotCircle);
		}

		private void LoadMenu()
		{
			ChampionMenu.AddSubMenu(new Menu("杩炴嫑", PUC.Player.ChampionName + "Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep0", "====== 杩炴嫑"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo", "= 浣跨敤 W ").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo", "= 浣跨敤 E ").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo_collision", "= 浣跨敤 R 纰版挒").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("楠氭壈", PUC.Player.ChampionName + "Harass"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== 楠氭壈"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useW_Harass", "= 浣跨敤 W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("娓呭叺", PUC.Player.ChampionName + "LaneClear"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== 娓呭叺"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useQ_LaneClear", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useW_LaneClear", "= 浣跨敤 W ").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear", "= 浣跨敤 E ").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("琛ュ叺", PUC.Player.ChampionName + "Lasthit"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("sep0", "====== 琛ュ叺"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("useQ_Lasthit", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("RunLikeHell", PUC.Player.ChampionName + "RunLikeHell"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep0", "====== RunLikeHell"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useE_RunLikeHell", "= E on Object").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep1", "========="));

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
			CheckState();
			switch(GnarState)
			{
				case 1:
					Q = QMini;
					E = EMini;
					break;
				default:
					Q = QMega;
					E = EMega;
					break;
			}

			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
						CastQEnemy();
					if(ChampionMenu.Item("useW_Combo").GetValue<bool>() && GnarState > 1)
						CastWEnemy();
					if(ChampionMenu.Item("useE_Combo").GetValue<bool>() && GnarState > 1)
						CastEEnemy();
					if(ChampionMenu.Item("useR_Combo_collision").GetValue<bool>() && GnarState > 1)
						CastREnemy();
					break;
				case Orbwalker.Mode.Harass:
					if(ChampionMenu.Item("useQ_Harass").GetValue<bool>())
						CastQEnemy();
					if(ChampionMenu.Item("useW_Harass").GetValue<bool>() && GnarState > 1)
						CastWEnemy();
					break;
				case Orbwalker.Mode.LaneClear:
					if(ChampionMenu.Item("useQ_LaneClear").GetValue<bool>())
					{
						CastQEnemy();
						CastQMinion();
					}
					if(ChampionMenu.Item("useW_LaneClear").GetValue<bool>() && GnarState > 1)
					{
						CastWEnemy();
						CastWMinion();
					}
					if(ChampionMenu.Item("useE_LaneClear").GetValue<bool>() && GnarState > 1)
						Cast_BasicSkillshot_AOE_Farm(E);
					break;
				case Orbwalker.Mode.Lasthit:
					if(ChampionMenu.Item("useQ_Lasthit").GetValue<bool>())
						CastQMinion();
					break;
			}
		}

		private void CheckState()
		{
			var tempState = 1;
			foreach(var buff in PUC.Player.Buffs)
			{
				if(buff.Name == TransformSoon)
					tempState = 2;
				if(buff.Name == Transformed)
					tempState = 3;
			}
			GnarState = tempState;
		}

		private void CastQEnemy()
		{
			if(!Q.IsReady())
				return;
			var target = TargetSelector.GetTarget(Q.Range);
			if(target == null)
				return;
			if(target.IsValidTarget(Q.Range) && Q.GetPrediction(target).Hitchance >= HitChance.High)
				Q.Cast(target, UsePackets() );
			if(Q.GetPrediction(target).Hitchance != HitChance.Collision)
				return;
			var qCollision = Q.GetPrediction(target).CollisionObjects;
			if((!qCollision.Exists(coll => coll.Distance(target) > 180 && GnarState == 1)) || (!qCollision.Exists(coll => coll.Distance(target) > 40)))
				Q.Cast(target.Position, UsePackets());
		}

		private void CastQMinion()
		{
			if(!Q.IsReady())
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
			foreach(var minion in allMinions)
			{
				if(!minion.IsValidTarget())
					continue;
				var minionInRangeAa = Orbwalker.GetAutoAttackRangeto(minion) <= minion.Distance(PUC.Player);
				var minionInRangeSpell = minion.Distance(ObjectManager.Player) <= Q.Range;
				var minionKillableAa = ObjectManager.Player.GetAutoAttackDamage(minion) >= minion.Health;
				var minionKillableSpell = ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health;
				var lastHit = Orbwalker.CurrentMode == Orbwalker.Mode.Lasthit;
				var laneClear = Orbwalker.CurrentMode == Orbwalker.Mode.LaneClear;

				if((lastHit && minionInRangeSpell && minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					Q.Cast(minion.Position, UsePackets());
				else if((laneClear && minionInRangeSpell && !minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					Q.Cast(minion.Position, UsePackets());
			}
		}

		private void CastWEnemy()
		{
			if(!W.IsReady())
				return;
			var target = TargetSelector.GetTarget(W.Range);
			if(target == null)
				return;
			if(target.IsValidTarget(W.Range) && W.GetPrediction(target).Hitchance >= HitChance.High)
				W.Cast(target, UsePackets());
		}

		private void CastWMinion()
		{
			if(!W.IsReady())
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);
			foreach(var minion in allMinions)
			{
				if(!minion.IsValidTarget())
					continue;
				var minionInRangeAa = Orbwalker.GetAutoAttackRangeto(minion) <= minion.Distance(PUC.Player);
				var minionInRangeSpell = minion.Distance(ObjectManager.Player) <= W.Range;
				var minionKillableAa = ObjectManager.Player.GetAutoAttackDamage(minion) >= minion.Health;
				var minionKillableSpell = ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W) >= minion.Health;
				var lastHit = Orbwalker.CurrentMode == Orbwalker.Mode.Lasthit;
				var laneClear = Orbwalker.CurrentMode == Orbwalker.Mode.LaneClear;

				if((lastHit && minionInRangeSpell && minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					W.Cast(minion.Position, UsePackets());
				else if((laneClear && minionInRangeSpell && !minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					W.Cast(minion.Position, UsePackets());
			}
		}

		private void CastEEnemy()
		{
			if(!E.IsReady())
				return;
			var target = TargetSelector.GetTarget(E.Range);
			if(target == null)
				return;
			if(target.IsValidTarget(Q.Range) && E.GetPrediction(target).Hitchance >= HitChance.High)
				E.Cast(target, UsePackets());
		}

		private void CastREnemy()
		{
			if(!R.IsReady())
				return;
			foreach(var target in PUC.AllHerosEnemy.Where(hero => hero.IsValidTarget(R.Width)))
				CastRToCollision(GetCollision(target));
		}

		private void CastRToCollision(int collisionId)
		{
			if (collisionId == -1)
				return;
			var center = PUC.Player.Position;
			const int points = 36;
			const int radius = 300;

			const double slice = 2*Math.PI/points;
			for (var i = 0; i < points; i++)
			{
				var angle = slice*i;
				var newX = (int) (center.X + radius*Math.Cos(angle));
				var newY = (int) (center.Y + radius*Math.Sin(angle));
				var p = new Vector3(newX, newY, 0);
				if (collisionId == i)
					R.Cast(p, UsePackets());
			}
		}

		private int GetCollision(Obj_AI_Hero enemy)
		{
			var center = enemy.Position;
			const int points = 36;
			const int radius = 300;
			var positionList = new List<Vector3>();

			const double slice = 2 * Math.PI / points;
			for(var i = 0; i < points; i++)
			{
				var angle = slice * i;
				var newX = (int)(center.X + radius * Math.Cos(angle));
				var newY = (int)(center.Y + radius * Math.Sin(angle));
				var p = new Vector3(newX, newY, 0);

				if(NavMesh.GetCollisionFlags(p) == CollisionFlags.Wall || NavMesh.GetCollisionFlags(p) == CollisionFlags.Building)
					return i;
			}
			return -1;
		}
	
	}
}
