using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Primes_Ultimate_Carry
{
	class TargetSelector
	{
			public enum PriorityMode
		{
			AutoPriority
		}

		private static readonly string[] AP = {"Ahri", "Akali", "Anivia", "Annie", "Azir", "Brand", "Cassiopeia", "Diana", "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus", "Kassadin", "Katarina", "Kayle", "Kennen", "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna", "Ryze", "Sion", "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra", "Velkoz"};
		private static readonly string[] Sup = {"Blitzcrank", "Janna", "Karma", "Leona", "Lulu", "Nami", "Sona", "Soraka", "Thresh", "Zilean"};
		private static readonly string[] Tank = {"Amumu", "Chogath", "DrMundo", "Galio", "Hecarim", "Malphite", "Maokai", "Nasus", "Rammus", "Sejuani", "Shen", "Singed", "Skarner", "Volibear", "Warwick", "Yorick", "Zac", "Nunu", "Taric", "Alistar", "Garen", "Nautilus", "Braum"};
		private static readonly string[] AD = { "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Kogmaw", "MissFortune", "Quinn", "Sivir", "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Zed", "Jinx", "Yasuo", "Lucian" };
		private static readonly string[] Bruiser = {"Darius", "Elise", "Evelynn", "Fiora", "Gangplank", "Gnar", "Jayce", "Pantheon", "Irelia", "JarvanIV", "Jax", "Khazix", "LeeSin", "Nocturne", "Olaf", "Poppy", "Renekton", "Rengar", "Riven", "Shyvana", "Trundle", "Tryndamere", "Udyr", "Vi", "MonkeyKing", "XinZhao", "Aatrox", "Rumble", "Shaco", "MasterYi"};
		
		internal static void AddtoMenu(Menu menu)
		{
			var tempMenu = menu;
			tempMenu.AddItem(new MenuItem("ts_sep0", "===== 璁剧疆"));
			tempMenu.AddItem(new MenuItem("ts_info", "= 蹇潵 ( 鑷姩 )"));
			tempMenu.AddItem(new MenuItem("ts_sep1", "========="));
			PUC.Menu.AddSubMenu(tempMenu);
		}

		internal static void Draw()
		{
			//nodrawing right now todo
		}

		private static int GetPriorityForTarget(string championName)
		{
			if (AD.Contains(championName))
				return 1;
			if (AP.Contains(championName))
				return 2;
			if (Sup.Contains(championName))
				return 3;
			if(Bruiser.Contains(championName))
				return 4;
			if(Tank.Contains(championName))
				return 5;
			return  5;
		}

		public static Obj_AI_Hero GetAATarget()
		{
			var temprange = PUC.Player.AttackRange + PUC.Player.BoundingRadius;
			foreach (
				var enemy in
					PUC.AllHerosEnemy.Where(hero => hero.IsValidTarget(temprange + hero.BoundingRadius))
						.Where(enemy => PUC.Player.GetAutoAttackDamage(enemy)*5 > enemy.Health))
				return enemy;

			Obj_AI_Hero[] tempTarget = {null};
			int[] tempPrio = {6};
			foreach(var enemy in PUC.AllHerosEnemy.Where(hero => hero.IsValidTarget(temprange + hero.BoundingRadius)).Where(enemy => (tempTarget[0] == null) || (tempPrio[0] > GetPriorityForTarget(enemy.ChampionName)) || (tempPrio[0] == GetPriorityForTarget(enemy.ChampionName) && PUC.Player.CalcDamage(enemy, Damage.DamageType.Physical, 100) > PUC.Player.CalcDamage(tempTarget[0], Damage.DamageType.Physical, 100))))
			{
				tempTarget[0] = enemy;
				tempPrio[0] = GetPriorityForTarget(enemy.ChampionName);
			}
			return tempTarget[0];
		}

		public static Obj_AI_Hero GetTarget(float range, PriorityMode mode = PriorityMode.AutoPriority)
		{
			if (mode == PriorityMode.AutoPriority)
				return AutoPrio(range);
			return AutoPrio(range);
		}

		private static Obj_AI_Hero AutoPrio(float range)
		{
			if(range < 1)
				return null;
			int[] priority = { 6 };
			Obj_AI_Hero[] priorityTarget = { null };
			foreach(var enemy in PUC.AllHerosEnemy.Where(enemy => enemy.IsValidTarget(range + enemy.BoundingRadius)).Where(enemy =>
				!(priorityTarget[0] != null && priority[0] <= GetPriorityForTarget(enemy.ChampionName))))
			{
				priorityTarget[0] = enemy;
				priority[0] = GetPriorityForTarget(enemy.ChampionName);
			}
			return priorityTarget[0];
		}
	}
}
