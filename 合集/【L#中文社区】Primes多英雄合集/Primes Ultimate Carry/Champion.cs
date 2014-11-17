using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Primes_Ultimate_Carry
{
	class Champion
	{
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;
		public Menu ChampionMenu;
		public bool HaveSupportMode;

		public Champion()
		{
			Chat.Print(PUC.Player.ChampionName  + " Plugin Loading ...");
			MenuBasics();
		}

		public void PluginLoaded()
		{
		Chat.Print(PUC.Player.ChampionName  + " Plugin Loadet!");	
		}
		private void MenuBasics()
		{
			ChampionMenu = new Menu("Primes " + PUC.Player.ChampionName, "Primes_Champion_" + PUC.Player.ChampionName);
			
			ChampionMenu.AddSubMenu(new Menu("鍒嗙粍璁剧疆", "Primes_Champion_Packets"));
			ChampionMenu.SubMenu("Primes_Champion_Packets").AddItem(new MenuItem("Primes_Champion_Packets_sep0", "===== 璁剧疆"));
			ChampionMenu.SubMenu("Primes_Champion_Packets").AddItem(new MenuItem("Primes_Champion_Packets_active", "= 浣跨敤鍒嗙粍").SetValue(true));
			ChampionMenu.SubMenu("Primes_Champion_Packets").AddItem(new MenuItem("Primes_Champion_Packets_sep1", "========="));
		}

		public void AddSupportmode(Menu menu)
		{
			menu.AddSubMenu(new Menu("鏀寔妯″紡", "SupportMode"));
			menu.SubMenu("SupportMode").AddItem(new MenuItem("sup_sep0", "===== 鏀寔妯″紡"));
			menu.SubMenu("SupportMode").AddItem(new MenuItem("SubMode", "鏀寔妯″紡").SetValue(false));
			menu.SubMenu("SupportMode").AddItem(new MenuItem("sup_sep1", "========="));
			
			HaveSupportMode = true;
			Game.OnGameSendPacket += GameSendPacker_Supportmode;
		}

		private void GameSendPacker_Supportmode(GamePacketEventArgs args)
		{
			if (!HaveSupportMode)
				return;
			if(args.PacketData[0] != Packet.C2S.Move.Header)
				return;
			var decodedPacket = Packet.C2S.Move.Decoded(args.PacketData);
			if (decodedPacket.MoveType != 3 || !Orbwalker.GetPossibleTarget().IsMinion ||
			    !ChampionMenu.Item("SubMode").GetValue<bool>() || (Orbwalker.Mode.Harass != Orbwalker.CurrentMode)) return;
			if (PUC.AllHerosFriend.Any(
					hero => !hero.IsMe && !hero.IsDead && hero.Distance(Orbwalker.GetPossibleTarget()) <= hero.AttackRange + 200))
				args.Process = false;
		}

		public MenuItem GetMenuItem(string name, string displayName)
		{
			return new MenuItem("Primes_Champion_Control_" + name, "= " + displayName);
		}

		public static bool IsInsideEnemyTower(Vector3 position)
		{
			return ObjectManager.Get<Obj_AI_Turret>()
									.Any(tower => tower.IsEnemy && tower.Health > 0 && tower.Position.Distance(position) < 775);
		}

		public Obj_AI_Hero Cast_BasicSkillshot_Enemy(Spell spell, TargetSelector.PriorityMode prio = TargetSelector.PriorityMode.AutoPriority, float extrarange = 0)
		{
			if(!spell.IsReady())
				return null;
			var target = TargetSelector.GetTarget(spell.Range, prio);
			if(target == null)
				return null;
			if (!target.IsValidTarget(spell.Range + extrarange) || spell.GetPrediction(target).Hitchance < HitChance.High)
				return null;
			spell.Cast(target, UsePackets());
			return target;
		}

		public void Cast_BasicSkillshot_AOE_Farm(Spell spell, int extrawidth = 0)
		{
			if(!spell.IsReady() )
				return;
			var minions = MinionManager.GetMinions(ObjectManager.Player.Position, spell.Type == SkillshotType.SkillshotLine ? spell.Range : spell.Range + ((spell.Width + extrawidth) / 2),MinionTypes.All , MinionTeam.NotAlly);
			if(minions.Count == 0)
				return;
			var castPostion = new MinionManager.FarmLocation();
			
			if(spell.Type == SkillshotType.SkillshotCircle)
				castPostion = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), spell.Width + extrawidth, spell.Range);
			if(spell.Type == SkillshotType.SkillshotLine)
				castPostion = MinionManager.GetBestLineFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), spell.Width, spell.Range);
			spell.Cast(castPostion.Position, UsePackets());
		}

		public void Cast_Basic_Farm(Spell spell, bool skillshot = false)
		{
			if(!spell.IsReady())
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spell.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
			foreach(var minion in allMinions)
			{
				if(!minion.IsValidTarget())
					continue;
				var minionInRangeAa = Orbwalking.InAutoAttackRange(minion);
				var minionInRangeSpell = minion.Distance(ObjectManager.Player) <= spell.Range;
				var minionKillableAa = ObjectManager.Player.GetAutoAttackDamage(minion) >= minion.Health;
				var minionKillableSpell = ObjectManager.Player.GetSpellDamage(minion, spell.Slot ) >= minion.Health;
				var lastHit = Orbwalker.CurrentMode == Orbwalker.Mode.Lasthit;
				var laneClear = Orbwalker.CurrentMode == Orbwalker.Mode.LaneClear;

				if((lastHit && minionInRangeSpell && minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					if(skillshot)
						spell.Cast(minion.Position, UsePackets());
					else
						spell.Cast(minion, UsePackets());
				else if((laneClear && minionInRangeSpell && !minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					if(skillshot)
						spell.Cast(minion.Position, UsePackets());
					else
						spell.Cast(minion, UsePackets());
				
			}
		}

		public bool UsePackets()
		{
			// todo
			// Tempfix Packets 4.18
			return false;
			return ChampionMenu.Item("Primes_Champion_Packets_active").GetValue<bool>();
		}

		public string GetSpellName(SpellSlot slot, Obj_AI_Hero unit = null)
		{
			return unit != null ? unit.Spellbook.GetSpell(slot).Name : PUC.Player.Spellbook.GetSpell(slot).Name;
		}

		public bool EnemysinRange(float range ,int min = 1, Obj_AI_Hero unit = null)
		{
			if (unit == null)
				unit = PUC.Player;
			return min <= PUC.AllHerosEnemy.Count(hero => hero.Distance(unit) < range && hero.IsValidTarget());
		}
		public bool EnemysinRange(float range, int min , Vector3  pos)
		{
			return min <= PUC.AllHerosEnemy.Count(hero => hero.Position.Distance(pos) < range && hero.IsValidTarget() && !hero.IsDead);
		}

		public void AddManaManager(Menu menu,string name,int basic)
		{
			menu.AddItem(new MenuItem(name, "= Mana-Manager").SetValue(new Slider(basic, 100, 0)));
		}

		public bool ManamanagerAllowCast(string name)
		{
			return PUC.Menu.Item(name).GetValue<Slider>().Value < PUC.Player.Mana/PUC.Player.MaxMana*100;
		}

		public bool EnoughManaFor(SpellSlot spell, SpellSlot spell2 = SpellSlot.Unknown, SpellSlot spell3 = SpellSlot.Unknown, SpellSlot spell4 = SpellSlot.Unknown)
		{
			var cost1 = PUC.Player.Spellbook.GetSpell(spell).ManaCost;
			var cost2 = 0f;
			var cost3 = 0f;
			var cost4 = 0f;
			if(spell2 != SpellSlot.Unknown)
				cost2 = PUC.Player.Spellbook.GetSpell(spell2).ManaCost;
			if(spell3 != SpellSlot.Unknown)
				cost3 = PUC.Player.Spellbook.GetSpell(spell3).ManaCost;
			if(spell4 != SpellSlot.Unknown)
				cost4 = PUC.Player.Spellbook.GetSpell(spell4).ManaCost;

			return cost1 + cost2 + cost3 + cost4 <= PUC.Player.Mana;
		}

		public Vector3 GetModifiedPosition(Vector3 from, Vector3 to, float range)
		{
			var newpos = to - from;
			newpos.Normalize();
			return from + (newpos * range);
		}

		public void Cast_Shield_onFriend(Spell spell, int percent, bool skillshot = false)
		{
			if(!spell.IsReady())
				return;
			foreach(var friend in PUC.AllHerosFriend.Where(hero => hero.Distance(PUC.Player) <= spell.Range).Where(friend => friend.Health / friend.MaxHealth * 100 <= percent && EnemysinRange(600, 1, friend)))
			{
				if(skillshot)
					spell.Cast(spell.GetPrediction(friend).CastPosition , UsePackets());
				else
					spell.CastOnUnit(friend, UsePackets());
				return;
			}
		}

		public Vector3 GetReversePosition(Vector3 positionMe, Vector3 positionEnemy)
		{
			var x = positionMe.X - positionEnemy.X;
			var y = positionMe.Y - positionEnemy.Y;
			return new Vector3(positionMe.X + x, positionMe.Y + y, positionMe.Z);
		}

	}
}
