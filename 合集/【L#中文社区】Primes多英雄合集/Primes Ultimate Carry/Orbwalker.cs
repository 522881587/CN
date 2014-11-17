using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace Primes_Ultimate_Carry
{
	class Orbwalker
	{
		public static bool AllowDrawing = true;

		private static readonly string[] AttackResets = { "dariusnoxiantacticsonh", "fioraflurry", "garenq", "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "luciane", "lucianq", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral" };
		private static readonly string[] NoAttacks = { "jarvanivcataclysmattack", "monkeykingdoubleattack", "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack", "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire" };
		private static readonly string[] Attacks = { "caitlynheadshotmissile", "frostarrow", "garenslash2", "kennenmegaproc", "lucianpassiveattack", "masteryidoublestrike", "quinnwenhanced", "renektonexecute", "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "xenzhaothrust2", "xenzhaothrust3" };
		private static Spell _movementPrediction;
		private static readonly Obj_AI_Hero Player = PUC.Player;
		private static bool _attack = true;
		private static bool _disableNextAttack;
		private static bool _moving = true;
		private static int _lastAATick;
		private static Obj_AI_Base _lastTarget;
		private const float LaneClearWaitTimeMod = 2f;
		public static int LastMovement = 0;
		public static bool CustomOrbwalkMode;

		public delegate void BeforeAttackEvenH(BeforeAttackEventArgs args);
		public delegate void OnTargetChangeH(Obj_AI_Base oldTarget, Obj_AI_Base newTarget);
		public delegate void AfterAttackEvenH(Obj_AI_Base unit, Obj_AI_Base target);
		public delegate void OnAttackEvenH(Obj_AI_Base unit, Obj_AI_Base target);

		public static event BeforeAttackEvenH BeforeAttack;
		public static event OnTargetChangeH OnTargetChange;
		public static event AfterAttackEvenH AfterAttack;
		public static event OnAttackEvenH OnAttack;
		
		public enum Mode
		{
			Combo,
			Harass,
			LaneClear,
			Lasthit,
			RunlikeHell,
			None,
		}

		internal static void AddtoMenu(Menu menu)
		{
			_movementPrediction = new Spell(SpellSlot.Unknown, GetAutoAttackRangeto());
			_movementPrediction.SetTargetted(Player.BasicAttack.SpellCastTime, Player.BasicAttack.MissileSpeed);
			
			var tempMenu = menu;
			tempMenu.AddItem(new MenuItem("orb_sep0", "====== 璁剧疆 "));
			tempMenu.AddItem(new MenuItem("orb_HoldPos", "= 淇濇寔浣嶇疆").SetValue(new Slider(0, 100, 0)));
			tempMenu.AddItem(new MenuItem("orb_ExtraWindup", "= 棰濆寤惰繜").SetValue(new Slider(80, 200, 0)));
			tempMenu.AddItem(new MenuItem("orb_AutoWindup", "= 鑷姩璁剧疆缁堢粨").SetValue(false));
			tempMenu.AddItem(new MenuItem("orb_farmdelay", "= 琛ュ叺寤惰繜").SetValue(new Slider(0, 200, 0)));
			tempMenu.AddItem(new MenuItem("orb_Priority", "= 浼樺厛鍗曚綅").SetValue(new StringList(new[] { "Minion", "Hero" })));
			tempMenu.AddItem(new MenuItem("orb_MeleePrediction", "= 鍥㈡垬棰勫垽 ").SetValue(false));
			tempMenu.AddItem(new MenuItem("orb_nomove", "= 绂佺敤绉诲姩 ").SetValue(false));
			tempMenu.AddItem(new MenuItem("orb_noattack", "= 绂佺敤鏀诲嚮 ").SetValue(false));
			tempMenu.AddItem(new MenuItem("orb_draw", "= 鐢籄A鑼冨洿 ").SetValue(true));
			tempMenu.AddItem(new MenuItem("orb_sep1", "=== 鎸夐敭"));
			tempMenu.AddItem(new MenuItem("orbkey_Combo", "= 杩炴嫑鎸夐敭").SetValue(new KeyBind(32, KeyBindType.Press)));
			tempMenu.AddItem(new MenuItem("orbkey_Harass", "= 楠氭壈鎸夐敭").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
			tempMenu.AddItem(new MenuItem("orbkey_LaneClear", "= 娓呭叺鎸夐敭").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
			tempMenu.AddItem(new MenuItem("orbkey_Lasthit", "= 琛ュ叺鎸夐敭").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
			tempMenu.AddItem(new MenuItem("orbkey_Runlikehell", "= 閫冭窇鎸夐敭").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
			tempMenu.AddItem(new MenuItem("orb_sep2", "========="));
			PUC.Menu.AddSubMenu(tempMenu);

			Game.OnGameUpdate += Game_OnGameUpdate;
			Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
		}

		internal static void Draw()
		{
			if(PUC.Menu.Item("orb_draw").GetValue<bool>() && AllowDrawing)
			Utility.DrawCircle(Player.Position, GetAutoAttackRangeto(), Color.Lavender);
		}

		private static void Game_OnGameUpdate(EventArgs args)
		{
			if (PUC.Menu.Item("orb_AutoWindup").GetValue<bool>())
			{
				var additional = 0;
				if (Game.Ping >= 100)
					additional = Game.Ping / 100 * 10;
				else if (Game.Ping > 40 && Game.Ping < 100)
					additional = Game.Ping / 100 * 20;
				else if (Game.Ping <= 40)
					additional = + 20;
				var windUp = Game.Ping + additional;
				if (windUp < 40)
					windUp = 40;

				PUC.Menu.Item("orb_ExtraWindup").SetValue(windUp < 200 ? new Slider(windUp, 200, 0) : new Slider(200, 200, 0));
			}

			if(CurrentMode == Mode.None || Player.IsChannelingImportantSpell() || MenuGUI.IsChatOpen)
				return;
			//player have buff return todo
			if (CustomOrbwalkMode) 
				return;
			var target = GetPossibleTarget();
			Orbwalk(Game.CursorPos, target);
		}

		private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
		{
			if(IsAutoAttackReset(spell.SData.Name) && unit.IsMe)
				Utility.DelayAction.Add(250, ResetAutoAttackTimer);

			if(!IsAutoAttack(spell.SData.Name))
				return;
			if(unit.IsMe)
			{
				_lastAATick = Environment.TickCount ;
				// ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
				if(spell.Target is Obj_AI_Base)
				{
					FireOnTargetSwitch((Obj_AI_Base)spell.Target);
					_lastTarget = (Obj_AI_Base)spell.Target;
				}
				if(unit.IsMelee() || unit.BaseSkinName == "Thresh" )
					Utility.DelayAction.Add(
						(int)(unit.AttackCastDelay * 1000 + Game.Ping * 0.5), () => FireAfterAttack(unit, _lastTarget));
			}
			FireOnAttack(unit, _lastTarget);
		}

		public static bool IsAutoAttack(string name)
		{
			return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower())) ||
			Attacks.Contains(name.ToLower());
		}

		public static void ResetAutoAttackTimer()
		{
			_lastAATick = 0;
		}

		public static bool IsAutoAttackReset(string name)
		{
			return AttackResets.Contains(name.ToLower());
		}

		public static void Orbwalk(Vector3 goalPosition, Obj_AI_Base target)
		{
			if(target != null && CanAttack() && !PUC.Menu.Item("orb_noattack").GetValue<bool>())
			{
				_disableNextAttack = false;
				FireBeforeAttack(target);
				if(!_disableNextAttack)
				{
					if(!Player.IssueOrder(GameObjectOrder.AttackUnit, target))
						Utility.DelayAction.Add(250, ResetAutoAttackTimer);
					else
						_lastAATick = Environment.TickCount + Game.Ping / 2;

					return;
				}
			}
			if(!CanMove() || PUC.Menu.Item("orb_nomove").GetValue<bool>())
				return;
			if(Player.IsMelee() && target != null && target.Distance(Player) < GetAutoAttackRangeto(target) &&
				PUC.Menu.Item("orb_MeleePrediction").GetValue<bool>() && target is Obj_AI_Hero && Game.CursorPos.Distance(target.Position) < 300)
			{
				_movementPrediction.Delay = Player.BasicAttack.SpellCastTime;
				_movementPrediction.Speed = Player.BasicAttack.MissileSpeed;
				MoveTo(_movementPrediction.GetPrediction(target).UnitPosition, 1);
			}
			else
				MoveTo(goalPosition);
		}

		private static void MoveTo(Vector3 position, float holdAreaRadius = -1)
		{
			var rnd = new Random();
			var delay = rnd.Next(50, 100);
			if(Environment.TickCount -LastMovement < delay)
				return;
			LastMovement = Environment.TickCount;

			if (!CanMove())
				return;
			if(holdAreaRadius < 0)
				holdAreaRadius = PUC.Menu.Item("orb_HoldPos").GetValue<Slider>().Value;
			if(Player.ServerPosition.Distance(position) < holdAreaRadius)
			{
				if(Player.Path.Count() > 1)
					Player.IssueOrder(GameObjectOrder.HoldPosition, Player.ServerPosition);
				return;
			}
			if (position.Distance( PUC.Player.Position) < 200)
				Player.IssueOrder(GameObjectOrder.MoveTo, position);
			else
			{
				var point = Player.ServerPosition +
				200 * (position.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();
				Player.IssueOrder(GameObjectOrder.MoveTo, point);
			}
			
		}

		public static Obj_AI_Base GetPossibleTarget()
		{
			Obj_AI_Base tempTarget = null;

			if(PUC.Menu.Item("orb_Priority").GetValue<StringList>().SelectedIndex == 1 &&
				(CurrentMode == Mode.Harass || CurrentMode == Mode.LaneClear))
			{
				tempTarget = TargetSelector.GetAATarget();
				if(tempTarget != null)
					return tempTarget;
			}

			if(CurrentMode == Mode.Harass || CurrentMode == Mode.Lasthit || CurrentMode == Mode.LaneClear)
			{
				foreach(var minion in from minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(GetAutoAttackRangeto(minion)) && minion.Health > 0)
									  let time = (int)(Player.AttackCastDelay * 1000)  + Game.Ping / 2 - 100 +
												 (int)(1000 * Player.Distance(minion) / (Player.ChampionName == "Thresh" ? Player.BasicAttack.MissileSpeed :(Player.IsMelee() ? float.MaxValue : Player.BasicAttack.MissileSpeed)))
									  let predHealth = HealthPrediction.GetHealthPrediction(minion, time, GetFarmDelay)
									  where minion.Team != GameObjectTeam.Neutral &&
											predHealth > 0 &&
											predHealth <= Player.GetAutoAttackDamage(minion, true)
									  select minion)
				{
					return minion;
				}
			}

			if(CurrentMode == Mode.Harass || CurrentMode == Mode.LaneClear)
			{
				foreach(var turret in ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(GetAutoAttackRangeto(turret))))
				{
					return turret;
				}
			}

			if(CurrentMode != Mode.Lasthit)
			{
				tempTarget = TargetSelector.GetAATarget();
				if(tempTarget != null)
					return tempTarget;
			}

			float[] maxhealth;
			if(CurrentMode == Mode.LaneClear || CurrentMode == Mode.Harass)
			{
				maxhealth = new float[] {0};
				var maxhealth1 = maxhealth;
				foreach(var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(GetAutoAttackRangeto(minion)) && minion.Team == GameObjectTeam.Neutral).Where(minion => minion.MaxHealth >= maxhealth1[0] || Math.Abs(maxhealth1[0] - float.MaxValue) < float.Epsilon))
				{
					tempTarget = minion;
					maxhealth[0] = minion.MaxHealth;
				}
				if(tempTarget != null)
					return tempTarget;
			}

			if(CurrentMode != Mode.LaneClear || ShouldWait()) 
				return null;
			maxhealth = new float[] {0};
			foreach (var minion in from minion in ObjectManager.Get<Obj_AI_Minion>()
				.Where(minion => minion.IsValidTarget(GetAutoAttackRangeto(minion))) let predHealth = HealthPrediction.LaneClearHealthPrediction(
					minion, (int)((Player.AttackDelay * 1000) * LaneClearWaitTimeMod), GetFarmDelay ) where predHealth >=
					                                                                                        2 *
																											 Player.GetAutoAttackDamage(minion, true) ||
					                                                                                        Math.Abs(predHealth - minion.Health) < float.Epsilon where minion.Health >= maxhealth[0] || Math.Abs(maxhealth[0] - float.MaxValue) < float.Epsilon select minion)
			{
				tempTarget = minion;
				maxhealth[0] = minion.MaxHealth;
			}
			return tempTarget;
		}

		private static bool ShouldWait()
		{
			return
			ObjectManager.Get<Obj_AI_Minion>()
			.Any(
			minion =>
			minion.IsValidTarget( GetAutoAttackRangeto(minion)) &&
			HealthPrediction.LaneClearHealthPrediction(
			minion, (int)((Player.AttackDelay * 1000) * LaneClearWaitTimeMod), GetFarmDelay ) <=
			Player.GetAutoAttackDamage(minion, true));
		}

		public static float GetAutoAttackRangeto(Obj_AI_Base target = null)
		{
			var ret = Player.AttackRange + Player.BoundingRadius;
			if(target != null)
				ret += target.BoundingRadius;
			return ret;
		}

		public static Mode CurrentMode
		{
			get
			{
				if(PUC.Menu.Item("orbkey_Combo").GetValue<KeyBind>().Active)
				{
					return Mode.Combo;
				}
				if(PUC.Menu.Item("orbkey_Harass").GetValue<KeyBind>().Active)
				{
					return Mode.Harass;
				}
				if(PUC.Menu.Item("orbkey_LaneClear").GetValue<KeyBind>().Active)
				{
					return Mode.LaneClear;
				}
				if(PUC.Menu.Item("orbkey_Lasthit").GetValue<KeyBind>().Active)
				{
					return Mode.Lasthit;
				}
				return PUC.Menu.Item("orbkey_Runlikehell").GetValue<KeyBind>().Active ? Mode.RunlikeHell : Mode.None;
			}
		}

		public static void DisableAttack()
		{
			_attack = false;
		}

		public static void EnableAttack()
		{
			_attack = true;
		}

		public static void DisableMoving()
		{
			_moving = false;
		}

		public static void EnableMoving()
		{
			_moving = true;
		}

		private static int GetFarmDelay
		{
			get
			{
				return Player.ChampionName == "Azir"
					? PUC.Menu.Item("orb_farmdelay").GetValue<Slider>().Value + 125 // with Azir Additional Delay
					: PUC.Menu.Item("orb_farmdelay").GetValue<Slider>().Value;
			}
		}

		public static bool CanAttack()
		{
			if(_lastAATick <= Environment.TickCount)
				return Environment.TickCount + Game.Ping / 2  >= _lastAATick + Player.AttackDelay * 1000 && _attack;
			return false;
		}

		public static bool CanMove()
		{
			var extraWindup = PUC.Menu.Item("orb_ExtraWindup").GetValue<Slider>().Value;
			if(_lastAATick <= Environment.TickCount)
				return Environment.TickCount + Game.Ping / 2  >= _lastAATick + Player.AttackCastDelay * 1000 + extraWindup && _moving;
			return false;
		}

		internal class BeforeAttackEventArgs
		{
			public Obj_AI_Base Target;
			public Obj_AI_Base Unit = ObjectManager.Player;
			private bool _process = true;
			public bool Process
			{
				get
				{
					return _process;
				}
				set
				{
					_disableNextAttack = !value;
					_process = value;
				}
			}
		}

		private static void FireBeforeAttack(Obj_AI_Base target)
		{
			if(BeforeAttack != null)
			{
				BeforeAttack(new BeforeAttackEventArgs
				{
					Target = target
				});
			}
			else
			{
				_disableNextAttack = false;
			}
		}

		private static void FireOnTargetSwitch(Obj_AI_Base newTarget)
		{
			if (OnTargetChange != null && (_lastTarget == null || _lastTarget.NetworkId != newTarget.NetworkId))
			{
				OnTargetChange(_lastTarget, newTarget);
			}
		}

		private static void FireAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
		{
			if(AfterAttack != null)
			{
				AfterAttack(unit, target);
			}
		}

		private static void FireOnAttack(Obj_AI_Base unit, Obj_AI_Base target)
		{
			if(OnAttack != null)
			{
				OnAttack(unit, target);
			}
		}
	}
}
