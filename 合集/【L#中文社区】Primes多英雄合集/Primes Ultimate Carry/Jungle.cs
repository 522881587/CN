//using System;
//using System.Collections.Generic;
//using System.Linq;
//using LeagueSharp;
//using LeagueSharp.Common;
//using SharpDX;
//using Color = System.Drawing.Color;

//namespace Primes_Ultimate_Carry
//{
//	internal class Jungle
//	{
//		private static readonly List<JungleCamp> JungleCamps = new List<JungleCamp>
//		{
//			new JungleCamp //Baron
//			{
//				SpawnTime = TimeSpan.FromSeconds(900),
//				RespawnTimer = TimeSpan.FromSeconds(420),
//				Position = new Vector3(4549.126f, 10126.66f, -63.11666f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("Worm12.1.1")
//				}
//			},
//			new JungleCamp //Dragon
//			{
//				SpawnTime = TimeSpan.FromSeconds(150),
//				RespawnTimer = TimeSpan.FromSeconds(360),
//				Position = new Vector3(9606.835f, 4210.494f, -60.30991f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("Dragon6.1.1")
//				}
//			},
//			new JungleCamp //Wight
//			{
//				SpawnTime = TimeSpan.FromSeconds(125),
//				RespawnTimer = TimeSpan.FromSeconds(50),
//				Position = new Vector3(1859.131f, 8246.272f, 54.92376f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("GreatWraith13.1.1")
//				}
//			},
//			new JungleCamp //Blue
//			{
//				SpawnTime = TimeSpan.FromSeconds(115),
//				RespawnTimer = TimeSpan.FromSeconds(300),
//				Position = new Vector3(3388.156f, 7697.175f, 55.21874f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("AncientGolem1.1.1"),
//					new JungleMinion("YoungLizard1.1.2"),
//					new JungleMinion("YoungLizard1.1.3")
//				}
//			},
//			new JungleCamp //Wolfs
//			{
//				SpawnTime = TimeSpan.FromSeconds(125),
//				RespawnTimer = TimeSpan.FromSeconds(50),
//				Position = new Vector3(3415.77f, 6269.637f, 55.60973f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("GiantWolf2.1.1"),
//					new JungleMinion("Wolf2.1.2"),
//					new JungleMinion("Wolf2.1.3")
//				}
//			},
//			new JungleCamp //Wraith
//			{
//				SpawnTime = TimeSpan.FromSeconds(125),
//				RespawnTimer = TimeSpan.FromSeconds(50),
//				Position = new Vector3(6447.0f, 5384.0f, 60.0f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("Wraith3.1.1"),
//					new JungleMinion("LesserWraith3.1.2"),
//					new JungleMinion("LesserWraith3.1.3"),
//					new JungleMinion("LesserWraith3.1.4")
//				}
//			},
//			new JungleCamp //Red
//			{
//				SpawnTime = TimeSpan.FromSeconds(115),
//				RespawnTimer = TimeSpan.FromSeconds(300),
//				Position = new Vector3(7509.412f, 3977.053f, 56.867f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("LizardElder4.1.1"),
//					new JungleMinion("YoungLizard4.1.2"),
//					new JungleMinion("YoungLizard4.1.3")
//				}
//			},
//			new JungleCamp //Golems
//			{
//				SpawnTime = TimeSpan.FromSeconds(125),
//				RespawnTimer = TimeSpan.FromSeconds(50),
//				Position = new Vector3(8042.148f, 2274.269f, 54.2764f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("Golem5.1.2"),
//					new JungleMinion("SmallGolem5.1.1")
//				}
//			},
//			new JungleCamp //Golems
//			{
//				SpawnTime = TimeSpan.FromSeconds(125),
//				RespawnTimer = TimeSpan.FromSeconds(50),
//				Position = new Vector3(6005.0f, 12055.0f, 39.62551f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("Golem11.1.2"),
//					new JungleMinion("SmallGolem11.1.1")
//				}
//			},
//			new JungleCamp //Red
//			{
//				SpawnTime = TimeSpan.FromSeconds(115),
//				RespawnTimer = TimeSpan.FromSeconds(300),
//				Position = new Vector3(6558.157f, 10524.92f, 54.63499f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("LizardElder10.1.1"),
//					new JungleMinion("YoungLizard10.1.2"),
//					new JungleMinion("YoungLizard10.1.3")
//				}
//			},
//			new JungleCamp //Wraith
//			{
//				SpawnTime = TimeSpan.FromSeconds(125),
//				RespawnTimer = TimeSpan.FromSeconds(50),
//				Position = new Vector3(7534.319f, 9226.513f, 55.50048f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("Wraith9.1.1"),
//					new JungleMinion("LesserWraith9.1.2"),
//					new JungleMinion("LesserWraith9.1.3"),
//					new JungleMinion("LesserWraith9.1.4")
//				}
//			},
//			new JungleCamp //Wolfs
//			{
//				SpawnTime = TimeSpan.FromSeconds(125),
//				RespawnTimer = TimeSpan.FromSeconds(50),
//				Position = new Vector3(10575.0f, 8083.0f, 65.5235f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("GiantWolf8.1.1"),
//					new JungleMinion("Wolf8.1.2"),
//					new JungleMinion("Wolf8.1.3")
//				}
//			},
//			new JungleCamp //Blue
//			{
//				SpawnTime = TimeSpan.FromSeconds(115),
//				RespawnTimer = TimeSpan.FromSeconds(300),
//				Position = new Vector3(10439.95f, 6717.918f, 54.8691f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("AncientGolem7.1.1"),
//					new JungleMinion("YoungLizard7.1.2"),
//					new JungleMinion("YoungLizard7.1.3")
//				}
//			},
//			new JungleCamp //Wight
//			{
//				SpawnTime = TimeSpan.FromSeconds(125),
//				RespawnTimer = TimeSpan.FromSeconds(50),
//				Position = new Vector3(12287.0f, 6205.0f, 54.84151f),
//				Minions = new List<JungleMinion>
//				{
//					new JungleMinion("GreatWraith14.1.1")
//				}
//			}
//		};

//		internal static void AddtoMenu(Menu menu)
//		{
//			if (Activator.SummonerList.Any(spell => spell.IsActive() && spell.Summoner == Activator.Summoner.Smite))
//			{
//				var tempMenu = menu;
//				menu.AddSubMenu(new Menu("Smite", "jun_smite"));
//				menu.SubMenu("jun_smite").AddItem(new MenuItem("jun_smite_sep0", "====== Conditions "));
//				menu.SubMenu("jun_smite")
//					.AddItem(
//						new MenuItem("jun_smite_use", "= Smite").SetValue(
//							new StringList(new[] {"No", "Spezial Objects", "HighPriority", "Allways", "Custom"})));
//				menu.SubMenu("jun_smite").AddItem(new MenuItem("jun_smite_sep1", "=== Condition 1 "));
//				menu.SubMenu("jun_smite").AddItem(new MenuItem("jun_smite_LaneClear", "= just Smite on LaneClear").SetValue(true));
//				menu.SubMenu("jun_smite").AddItem(new MenuItem("jun_smite_sep2", "=== Condition 2"));
//				menu.SubMenu("jun_smite").AddItem(new MenuItem("jun_smite_smart", "= Smart Smite ").SetValue(true));
//				menu.SubMenu("jun_smite").AddItem(new MenuItem("jun_smite_sep3", "========="));

//				menu.AddSubMenu(new Menu("Smite Custom", "jun_smite_Custom"));
//				menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_sep4", "====== Smite Objects "));

//				menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_sep5", "=== High Priority "));
//				if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.SummonersRift)
//				{
//					menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_obj_Baron", "= Baron").SetValue(true));
//					menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_obj_Dragon", "= Dragon").SetValue(true));
//					menu.SubMenu("jun_smite_Custom")
//						.AddItem(new MenuItem("jun_smite_obj_AncientGolem", "= (Blue) Ancient Golem ").SetValue(true));
//					menu.SubMenu("jun_smite_Custom")
//						.AddItem(new MenuItem("jun_smite_obj_LizardElder", "= (Red) Lizard Elder").SetValue(true));
//				}
//				if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline)
//					menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_obj_TTSpiderboss", "= Spiderboss").SetValue(true));

//				menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_sep6", "=== Low Priority "));
//				if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.SummonersRift)
//				{
//					menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_obj_Golem", "= Golem").SetValue(true));
//					menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_obj_GiantWolf", "= GiantWolf").SetValue(true));
//					menu.SubMenu("jun_smite_Custom")
//						.AddItem(new MenuItem("jun_smite_obj_GreatWraith", "= Great Wraith").SetValue(true));
//					menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_obj_Wraith", "= Wraith").SetValue(true));
//				}
//				if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline)
//				{
//					menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_obj_TTGolem", "= Golem").SetValue(true));
//					menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_obj_TTWolf", "= Wolf").SetValue(true));
//					menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_obj_TTWraith", "= Wraith").SetValue(true));
//				}

//				menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_sep7", "=== Special Objects "));
//				// Zac Blop passive
//				// tibbers
//				// H-28G Evolution Turret
//				// Summon Voidling
//				// Jack In The Box
//				// Rampant Growth (plant) 
//				menu.SubMenu("jun_smite_Custom").AddItem(new MenuItem("jun_smite_sep8", "========="));
//				PUC.Menu.AddSubMenu(tempMenu);
//			}
//			Game.OnGameUpdate += OnUpdate;
//			GameObject.OnCreate += ObjectOnCreate;
//			GameObject.OnDelete += ObjectOnDelete;
//		}

//		private static void OnUpdate(EventArgs args)
//		{
//			if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.SummonersRift)
//				UpdateCamps();
//		}

//		internal static void Draw()
//		{
//			if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.SummonersRift)
//			{
//				foreach (var minionCamp in JungleCamps)
//				{
//					if (minionCamp.State != JungleCampState.Dead)
//						continue;

//					TimeSpan time;
//					Vector2 pos;
//					string display;
//					if(Game.ClockTime < minionCamp.SpawnTime.TotalSeconds)
//					{
//						time = TimeSpan.FromSeconds(minionCamp.SpawnTime.TotalSeconds - Game.ClockTime);
//						pos = Drawing.WorldToMinimap(minionCamp.Position);
//						display = string.Format("{0}:{1:D2}", time.Minutes, time.Seconds);
//						Drawing.DrawText(pos.X - display.Length * 3, pos.Y - 5, Color.White, display);
//						continue;
//					}
//					var delta = Game.Time - minionCamp.ClearTick;
//					if (!(delta < minionCamp.RespawnTimer.TotalSeconds)) 
//						continue;
//					time = TimeSpan.FromSeconds(minionCamp.RespawnTimer.TotalSeconds - delta);
//					pos = Drawing.WorldToMinimap(minionCamp.Position);
//					display = string.Format("{0}:{1:D2}", time.Minutes, time.Seconds);
//					Drawing.DrawText(pos.X - display.Length*3, pos.Y - 5, Color.White, display);
//				}

//				foreach (var camp in ObjectManager.Get<NeutralMinionCamp>())
//				{
//					var pos = GetCampModifiedPosition(camp);
//					Utility.DrawCircle(pos, GetCampRange(camp), Color.Blue);
//					Drawing.DrawText(
//						Drawing.WorldToScreen(pos).X,
//						Drawing.WorldToScreen(pos).Y,
//						Color.White, GetCampName(camp));
//				}
//			}
//		}

//		private static void UpdateCamps()
//		{
//			foreach (var camp in JungleCamps)
//			{
//				var allAlive = true;
//				var allDead = true;
//				foreach (var minion in camp.Minions)
//				{
//					if (minion.Unit != null)
//						minion.Dead = minion.Unit.IsDead;
//					if (minion.Dead)
//						allAlive = false;
//					else
//						allDead = false;
//				}

//				if(Game.ClockTime < camp.SpawnTime.TotalSeconds)
//				{
//					camp.State = JungleCampState.Dead;
//					camp.ClearTick = 0.0f;
//					continue;
//				}

//				switch (camp.State)
//				{
//					case JungleCampState.Unknown:
//						if (allAlive)
//						{
//							camp.State = JungleCampState.Alive;
//							camp.ClearTick = 0.0f;
//						}
//						break;
//					case JungleCampState.Dead:
//						if (allAlive)
//						{
//							camp.State = JungleCampState.Alive;
//							camp.ClearTick = 0.0f;
//						}
//						break;
//					case JungleCampState.Alive:
//						if (allDead)
//						{
//							camp.State = JungleCampState.Dead;
//							camp.ClearTick = Game.Time;
//						}
//						break;
//				}
//			}
//		}

//		private static void ObjectOnDelete(GameObject sender, EventArgs args)
//		{
//			try
//			{
//				if (sender.Type != GameObjectType.obj_AI_Minion)
//					return;
//				var neutral = (Obj_AI_Minion) sender;
//				if (neutral.Name.Contains("Minion") || !neutral.IsValid)
//					return;
//				foreach (
//					var minion in
//						from camp in JungleCamps
//						from minion in camp.Minions
//						where minion.Name == neutral.Name
//						select minion)
//				{
//					minion.Dead = neutral.IsDead;
//					minion.Unit = null;
//				}
//			}
//			catch (Exception ex)
//			{
//				Console.WriteLine(ex.ToString());
//			}
//		}

//		private static void ObjectOnCreate(GameObject sender, EventArgs args)
//		{
//				if (sender.Type != GameObjectType.obj_AI_Minion)
//					return;
//				var neutral = (Obj_AI_Minion) sender;
//				if (neutral.Name.Contains("Minion") || !neutral.IsValid)
//					return;
//				foreach (
//					var minion in
//						from camp in JungleCamps
//						from minion in camp.Minions
//						where minion.Name == neutral.Name
//						select minion)
//				{
//					minion.Unit = neutral;
//					minion.Dead = neutral.IsDead;
//				}
//		}

//		private static string GetCampName(GameObject camp)
//		{
//			var teamOrder = "[Team: " + GameObjectTeam.Order + "] ";
//			var teamChaos = "[Team: " + GameObjectTeam.Chaos + "] ";
//			var teamNeutral = "[Team: " + GameObjectTeam.Neutral + "] ";
//			switch (camp.Name)
//			{
//				case "monsterCamp_1":
//					return teamOrder + "Blue";
//				case "monsterCamp_2":
//					return teamOrder + "Wolfs";
//				case "monsterCamp_3":
//					return teamOrder + "Wraiths";
//				case "monsterCamp_4":
//					return teamOrder + "Red";
//				case "monsterCamp_5":
//					return teamOrder + "Golems";
//				case "monsterCamp_6":
//					return teamNeutral + "Dragon";
//				case "monsterCamp_7":
//					return teamChaos + "Blue";
//				case "monsterCamp_8":
//					return teamChaos + "Wolfs";
//				case "monsterCamp_9":
//					return teamChaos + "Wraiths";
//				case "monsterCamp_10":
//					return teamChaos + "Red";
//				case "monsterCamp_11":
//					return teamChaos + "Golems";
//				case "monsterCamp_12":
//					return teamNeutral + "Baron";
//				case "monsterCamp_13":
//					return teamOrder + "Great Wraith";
//				case "monsterCamp_14":
//					return teamChaos + "Great Wraith";
//				default:
//					return camp.Name;
//			}
//		}

//		private static int GetCampRange(GameObject camp)
//		{
//			switch (camp.Name)
//			{
//				case "monsterCamp_6":
//					return 100;

//				case "monsterCamp_12":
//					return 100;

//				case "monsterCamp_13":
//					return 100;

//				case "monsterCamp_14":
//					return 100;

//				default:
//					return 220;
//			}
//		}

//		private static Vector3 GetCampModifiedPosition(NeutralMinionCamp camp)
//		{
//			switch (camp.Name)
//			{
//				case "monsterCamp_1":
//					return new Vector3(camp.Position.X - 50, camp.Position.Y + 50, camp.Position.Z);

//				case "monsterCamp_2":
//					return new Vector3(camp.Position.X + 50, camp.Position.Y + 50, camp.Position.Z);

//				case "monsterCamp_3":
//					return new Vector3(camp.Position.X + 250, camp.Position.Y - 50, camp.Position.Z);

//				case "monsterCamp_4":
//					return new Vector3(camp.Position.X - 50, camp.Position.Y - 50, camp.Position.Z);

//				case "monsterCamp_5":
//					return new Vector3(camp.Position.X + 120, camp.Position.Y, camp.Position.Z);

//				case "monsterCamp_7":
//					return new Vector3(camp.Position.X + 100, camp.Position.Y - 50, camp.Position.Z);

//				case "monsterCamp_8":
//					return new Vector3(camp.Position.X - 50, camp.Position.Y - 20, camp.Position.Z);

//				case "monsterCamp_9":
//					return new Vector3(camp.Position.X - 110, camp.Position.Y, camp.Position.Z);

//				case "monsterCamp_10":
//					return new Vector3(camp.Position.X + 50, camp.Position.Y + 50, camp.Position.Z);

//				case "monsterCamp_11":
//					return new Vector3(camp.Position.X + 170, camp.Position.Y + 50, camp.Position.Z);
//			}

//			return camp.Position;
//		}

//		internal class JungleCamp
//		{
//			public TimeSpan SpawnTime { get; set; }
//			public TimeSpan RespawnTimer { get; set; }
//			public Vector3 Position { get; set; }
//			public List<JungleMinion> Minions { get; set; }
//			public JungleCampState State { get; set; }
//			public float ClearTick { get; set; }
//		}

//		internal enum JungleCampState
//		{
//			Unknown,
//			Dead,
//			Alive
//		}

//		internal class JungleMinion
//		{
//			public JungleMinion(string name)
//			{
//				Name = name;
//			}

//			public string Name { get; set; }
//			public bool Dead { get; set; }
//			public GameObject Unit { get; set; }
//		}
//	}
//}