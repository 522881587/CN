using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Primes_Ultimate_Carry
{
	// ReSharper disable once InconsistentNaming
	class Champion_Orianna : Champion 
	{
		public BallControl Ball= new BallControl();
		public Vector3 CastRPosition;
		public int RInterruptTry = 0;

		public Champion_Orianna()
		{
			SetSpells();
			LoadMenu();


			Game.OnGameUpdate += OnUpdate;
			Drawing.OnDraw += OnDraw;
			Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
			Game.OnGameSendPacket += Game_OnSendPacket;
			PluginLoaded();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q, 825);
			Q.SetSkillshot(0, 135, 1150, false, SkillshotType.SkillshotLine);
		
			W = new Spell(SpellSlot.W, 220);
			
			E = new Spell(SpellSlot.E, 1095);
			E.SetTargetted(0.25f, 1700);
		
			R = new Spell(SpellSlot.R, 300);
		}

		private void LoadMenu()
		{
			ChampionMenu.AddSubMenu(new Menu("杩炴嫑", PUC.Player.ChampionName + "Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep0", "====== 杩炴嫑"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo", "= 浣跨敤 W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo", "= 浣跨敤 R KillSecure").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("楠氭壈", PUC.Player.ChampionName + "Harass"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== 楠氭壈"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass"),"ManaManager_Harass",40);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useW_Harass", "= 浣跨敤 W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useHarass_Auto", "= 鑷姩楠氭壈").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("娓呭叺", PUC.Player.ChampionName + "LaneClear"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== 娓呭叺"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear"),"ManaManager_LaneClear",20);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useQ_LaneClear", "= 浣跨敤 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useW_LaneClear", "= 浣跨敤 W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("Lasthit", PUC.Player.ChampionName + "Lasthit"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("sep0", "====== Lasthit"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("useQ_Lasthit", "= Use Q").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("杩芥潃", PUC.Player.ChampionName + "RunLikeHell"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep0", "====== 杩芥潃"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useW_RunLikeHell", "= 浣跨敤 W 鍔犻€熴劎").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("鏉傞」", PUC.Player.ChampionName + "Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep0", "====== 鏉傞」"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useR_Interrupt", "= R 涓柇鎶€鑳姐劎").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useW_Auto", "= 鑷姩 W").SetValue(new Slider(2, 0, 5)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useR_Auto", "= 鑷姩 R").SetValue(new Slider(3, 0, 5)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useE_Auto", "= 浣跨敤 E % 琛€閲忋劎").SetValue(new Slider(40, 100, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("缁樺埗", PUC.Player.ChampionName + "Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep0", "====== 缁樺埗"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Disabled", "绂佹缁樺埗").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Q", "缁樺埗 Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_W", "缁樺埗 W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_E", "缁樺埗 E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_R", "缁樺埗 R").SetValue(true));

			var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage").SetValue(true);
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

			if(ChampionMenu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_W").GetValue<bool>())
				if(W.Level > 0)
					Utility.DrawCircle(Ball.BallPosition, W.Range, W.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(Ball.BallPosition, R.Range, R.IsReady() ? Color.Green : Color.Red);
		}

		private void OnUpdate(EventArgs args)
		{
			CastE();
			CastW();
			CastR();
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
						CastQ();
					break;
				case Orbwalker.Mode.Harass:
					if(!ManamanagerAllowCast("ManaManager_Harass"))
						return;
					if(ChampionMenu.Item("useQ_Harass").GetValue<bool>())
						CastQ();
					break;
				case Orbwalker.Mode.LaneClear:
						LaneClear();
					break;
				case Orbwalker.Mode.RunlikeHell:
					RunlikeHell();
					break;			
			}

			if(ChampionMenu.Item("useHarass_Auto").GetValue<KeyBind>().Active )
				CastQ();
		}

		private void LaneClear()
		{
			if(!ManamanagerAllowCast("ManaManager_LaneClear"))
				return;
			var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All,MinionTeam.NotAlly,MinionOrderTypes.MaxHealth );

			var useQ = ChampionMenu.Item("useQ_LaneClear").GetValue<bool>();
			var useW = ChampionMenu.Item("useW_LaneClear").GetValue<bool>();

			var hit = 0;

			if(useQ && Q.IsReady())
			{
				foreach(var enemy in allMinionsW)
				{
					Q.UpdateSourcePosition(Ball.BallPosition, PUC.Player.Position);
					if(!Q.IsReady() || !(PUC.Player.Distance(enemy) <= Q.Range))
						continue;
					hit += allMinionsW.Count(enemy2 => enemy2.Distance(Q.GetPrediction(enemy).CastPosition) < Q.Width);
					if(hit < 1)
						continue;
					if(Q.GetPrediction(enemy).Hitchance >= HitChance.High)
						Q.Cast(Q.GetPrediction(enemy).CastPosition, true);
				}
			}

			if(!useW || !W.IsReady())
				return;
			hit = allMinionsW.Count(enemy => enemy.Distance(Ball.BallPosition) < W.Range);
			if(hit >= 1)
				W.Cast();
		}

		private void RunlikeHell()
		{
			if (Ball.IsonMe && W.IsReady())
				W.Cast();
			else if(E.IsReady() && !Ball.IsonMe)
				E.Cast(PUC.Player, UsePackets());

		}

		private void CastQ()
		{
			if(Ball.IsMoving || !Q.IsReady())
				return;
			var target = TargetSelector.GetTarget(Q.Range);
			if (target == null) 
				return;
			Q.UpdateSourcePosition(Ball.BallPosition, PUC.Player.Position);
			if (Q.GetPrediction(target).Hitchance < HitChance.High) 
				return;
			Ball.IsMoving = true;
			Q.Cast(target, UsePackets());
		}

		private void CastW()
		{
			if(Ball.IsMoving || !W.IsReady())
				return;
			W.UpdateSourcePosition(Ball.BallPosition, PUC.Player.Position);
			switch (Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(EnemysinRange(W.Range, 1, Ball.BallPosition))
						W.Cast();
					break;
				case Orbwalker.Mode.Harass:
					if(!ManamanagerAllowCast("ManaManager_Harass"))
						return;
					if(EnemysinRange(W.Range, 1, Ball.BallPosition))
						W.Cast();
					break;
				default:
					if(EnemysinRange(W.Range, ChampionMenu.Item("useW_Auto").GetValue<Slider>().Value, Ball.BallPosition))
						W.Cast();
					break;
			}
			if(ChampionMenu.Item("useHarass_Auto").GetValue<KeyBind>().Active)
				if(ManamanagerAllowCast("ManaManager_Harass"))
					if(EnemysinRange(W.Range, 1, Ball.BallPosition))
						W.Cast();
		}

		private void CastE()
		{
			if(Ball.IsMoving || !E.IsReady())
				return;
			var healhpercentuse = ChampionMenu.Item("useE_Auto").GetValue<Slider>().Value;
			Obj_AI_Hero[]  lowestFriend = {null};
			foreach(var friend in PUC.AllHerosFriend.Where(
				hero =>
					hero.Health / hero.MaxHealth * 100 < healhpercentuse && hero.IsValid && !hero.IsDead &&
					hero.Distance(PUC.Player) < E.Range).Where(friend => lowestFriend[0] == null || lowestFriend[0].Health / lowestFriend[0].MaxHealth * 100 > friend.Health / friend.MaxHealth * 100))
			{
				lowestFriend[0] = friend;
			}
			if(lowestFriend[0] != null && Orbwalker.CurrentMode == Orbwalker.Mode.Combo )
				E.Cast(lowestFriend[0], UsePackets());
		}

		private void CastR()
		{
			if(Ball.IsMoving || !R.IsReady())
				return;
			if(EnemysinRange(R.Range, ChampionMenu.Item("useR_Auto").GetValue<Slider>().Value, Ball.BallPosition))
				R.Cast();
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if (PUC.AllHerosEnemy.Any(hero => hero.IsValidTarget() && hero.Position.Distance(Ball.BallPosition) < R.Range && (PUC.Player.GetSpellDamage(hero, SpellSlot.R) > hero.Health || PUC.Player.GetSpellDamage(hero, SpellSlot.R) + PUC.Player.GetSpellDamage(hero, SpellSlot.W) > hero.Health && W.IsReady())))
						R.Cast();								
					break;
			}
		}

		private void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
		{
			if(!PUC.Menu.Item("useR_Interrupt").GetValue<bool>())
				return;
			if(Ball.BallPosition.Distance(unit.Position) < R.Range && !Ball.IsMoving)
			{
				R.Cast();
			}
			else if (Ball.BallPosition.Distance(unit.Position) < Q.Range)
			{
				if (EnoughManaFor(SpellSlot.Q, SpellSlot.R) && Q.IsReady() && R.IsReady())
				{
					Q.Cast(unit.Position, UsePackets());
					Utility.DelayAction.Add(100, CastRonReachPosition);
					CastRPosition = unit.Position;
				}
			}
		}

		private void CastRonReachPosition()
		{
			if(Ball.BallPosition.Distance(CastRPosition) < 50)
				if(R.Cast())
				{
					CastRPosition = new Vector3();
					RInterruptTry = 0;
				}
			if(RInterruptTry < 15)
			{
				RInterruptTry += 1;
				Utility.DelayAction.Add(100, CastRonReachPosition);
			}
			else
			{
				RInterruptTry = 0;
				CastRPosition = new Vector3();
			}
		}

		private void Game_OnSendPacket(GamePacketEventArgs args)
		{
			if (args.PacketData[0] != Packet.C2S.Cast.Header) 
				return;
			var decodedPacket = Packet.C2S.Cast.Decoded(args.PacketData);
			if (decodedPacket.Slot.ToString() != "131") 
				return;
			if(!EnemysinRange(R.Range,1,Ball.BallPosition))
				args.Process = false;
			
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
			return (float)damage;
		}

		internal class BallControl
		{
			public Vector3 BallPosition;
			public bool IsMoving;
			public bool IsonMe;
			public BallControl()
			{
				Drawing.OnDraw  += CheckBallLocation;
				Obj_AI_Base.OnProcessSpellCast += OnSpellcast;
			}

			private void CheckBallLocation(EventArgs args)
			{
				if(ObjectManager.Player.HasBuff("orianaghostself", true))
				{
					BallPosition = ObjectManager.Player.ServerPosition;
					IsMoving = false;
					IsonMe = true;
					return;
				}

				foreach(var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && !ally.IsDead && ally.HasBuff("orianaghost", true)))
				{
					BallPosition = ally.ServerPosition;
					IsMoving = false;
					IsonMe = false;
					return;
				}
			}

			private void OnSpellcast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
			{
				var castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name, false);
				if(!sender.IsMe)
					return;

				if(castedSlot == SpellSlot.Q)
				{
					IsMoving = true;
					Utility.DelayAction.Add((int)Math.Max(1, 1000 * args.End.Distance(BallPosition) / 1200), () =>
					{
						BallPosition = args.End;
						IsMoving = false;
					});
				}

				if(castedSlot == SpellSlot.E)
				{
					IsMoving = true;
					Utility.DelayAction.Add((int)Math.Max(1, 1000 * args.End.Distance(BallPosition) / 1700), () =>
					{
						IsMoving = false;
					});
				}
				//if (castedSlot != SpellSlot.E) 
				//	return;
				//if (!args.Target.IsMe && args.Target.IsAlly)
				//	IsMoving = true;
				//if (args.Target.IsMe && BallPosition != ObjectManager.Player.ServerPosition)
				//	IsMoving = true;
			}
		}
	}
}
