using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Primes_Ultimate_Carry
{
	class AutoLevel
	{
		public static List<Levelsequence> LevelsequenceList;
		public static int Delay = 150;
		public static int DelayTick = 0;

		private static void CreateList()
		{
			LevelsequenceList = new List<Levelsequence>();
			// Aatrox
			LevelsequenceList.Add(new Levelsequence("Aatrox", "UC Top", new List<int> { 3, 2, 1, 2, 3, 4, 2, 3, 2, 3, 4, 2, 3, 1, 1, 4, 1, 1, }));
			LevelsequenceList.Add(new Levelsequence("Aatrox", "UC Mid", new List<int> { 3, 2, 1, 2, 2, 4, 3, 2, 3, 2, 4, 3, 3, 1, 1, 4, 1, 1, }));
			LevelsequenceList.Add(new Levelsequence("Aatrox", "UC Jungle", new List<int> { 2, 1, 3, 2, 2, 4, 2, 3, 2, 3, 4, 3, 3, 1, 1, 4, 1, 1, }));
			
			// Caitlyn
			LevelsequenceList.Add(new Levelsequence("Caitlyn", "WannabeCaitlyn Adc", new List<int> { 1, 3, 1, 2, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2, })); // from Wannabe 
			
			// Ezreal
			LevelsequenceList.Add(new Levelsequence("Ezreal", "BluEZreal Poke Escape Adc", new List<int> { 1, 3, 1, 3, 1, 4, 1, 3, 1, 3, 4, 3, 2, 3, 2, 4, 2, 2, })); // from Qtkzx 
			
			// Khazix
			LevelsequenceList.Add(new Levelsequence("Khazix", "WannabeKhazix Jungle", new List<int> { 2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3, })); // from Wannabe 
			
			// LeeSin
			LevelsequenceList.Add(new Levelsequence("LeeSin", "WannabeLeeSin Jungle", new List<int> { 1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3, })); // from Wannabe 
			
			// Leona
			LevelsequenceList.Add(new Levelsequence("Leona", "WannabeLeona Sup", new List<int> { 1, 3, 2, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3, })); // from Wannabe 
			
			// Lucian
			LevelsequenceList.Add(new Levelsequence("Lucian", "WannabeLucian Adc", new List<int> { 1, 3, 1, 2, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2, })); // from Wannabe  

			// Morgana
			LevelsequenceList.Add(new Levelsequence("Morgana", "WannabeMorgana Sup", new List<int> { 1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2, })); // from Wannabe 
			
			// Thresh
			LevelsequenceList.Add(new Levelsequence("Thresh", "WannabeThresh Sup", new List<int> { 3, 1, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2, })); // from Wannabe 
			
			// Tristana
			LevelsequenceList.Add(new Levelsequence("Tristana", "WannabeTristana Adc", new List<int> { 3, 2, 1, 3, 3, 4, 1, 1, 1, 1, 4, 2, 2, 2, 2, 4, 3, 3, })); // from Wannabe 
			
			// Zed
			LevelsequenceList.Add(new Levelsequence("Zed", "WannabeZed Mid", new List<int> { 1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2, })); // from Wannabe 




			
			
		}

		internal static void AddtoMenu(Menu menu)
		{
			CreateList();
			var tempMenu = menu;
			tempMenu.AddItem(new MenuItem("lvl_sep0", "====== AutoLevel "));
			foreach (var lvlsequence in LevelsequenceList.Where(sequence => sequence.ChampionName == PUC.Player.ChampionName))
			{
				tempMenu.AddItem(new MenuItem("lvl_sequence_" + lvlsequence.SequenceName.Replace(" ", "_"), "= " + lvlsequence.SequenceName).SetValue(false).DontSave());
				tempMenu.Item("lvl_sequence_" + lvlsequence.SequenceName.Replace(" ", "_")).ValueChanged += SwitchSequence;
			}
			tempMenu.AddItem(new MenuItem("lvl_sep2", "========="));
			PUC.Menu.AddSubMenu(tempMenu);

			Game.OnGameUpdate += OnUpdate;
		}

		private static void OnUpdate(EventArgs args)
		{
			if(Delay > Environment.TickCount - DelayTick)
				return;
			DelayTick = Environment.TickCount;
			foreach (var lvlsequence in LevelsequenceList.Where(sequence => sequence.ChampionName == PUC.Player.ChampionName).Where(lvlsequence => PUC.Menu.Item("lvl_sequence_" + lvlsequence.SequenceName.Replace(" ", "_")).GetValue<bool>()))
			{
				LevelUpSpell(lvlsequence);
				return;
			}
		}

		private static void LevelUpSpell(Levelsequence lvlsequence)
		{
			var myLvl = PUC.Player.Level;
			var q = PUC.Player.Spellbook.GetSpell(SpellSlot.Q);
			var w = PUC.Player.Spellbook.GetSpell(SpellSlot.W);
			var e = PUC.Player.Spellbook.GetSpell(SpellSlot.E);
			var r = PUC.Player.Spellbook.GetSpell(SpellSlot.R);
			var lvlQ = 0;
			var lvlW = 0;
			var lvlE = 0;
			var lvlR = 0;
			foreach (var level in lvlsequence.Sequence)
			{
				if(lvlQ + lvlW + lvlE + lvlR >= myLvl + lvlsequence.LvlOffset)
					return;
				switch(level)
				{
					case 1:
						lvlQ += 1;
						if(q.Level >= lvlQ)
							continue;
						PUC.Player.Spellbook.LevelUpSpell(SpellSlot.Q);
						return;
					case 2:
						lvlW += 1;
						if(w.Level >= lvlW)
							continue;
						PUC.Player.Spellbook.LevelUpSpell(SpellSlot.W);
						return;
					case 3:
						lvlE += 1;
						if(e.Level >= lvlE)
							continue;
						PUC.Player.Spellbook.LevelUpSpell(SpellSlot.E);
						return;
					case 4:
						lvlR += 1;
						if(r.Level >= lvlR)
							continue;
						PUC.Player.Spellbook.LevelUpSpell(SpellSlot.R);
						return;
				}
			}

		}

		private static void SwitchSequence(object sender, OnValueChangeEventArgs e)
		{
			if (!e.GetNewValue<bool>()) 
				return;
			var item = (MenuItem)sender;
			foreach (var lvlsequence in LevelsequenceList.Where(sequence => sequence.ChampionName == PUC.Player.ChampionName).Where(lvlsequence => item.Name != "lvl_sequence_" + lvlsequence.SequenceName.Replace(" ", "_")))
				PUC.Menu.Item("lvl_sequence_" + lvlsequence.SequenceName.Replace(" ", "_")).SetValue(false);		
		}

		internal class Levelsequence
		{
			public string ChampionName;
			public string SequenceName;
			public List<int> Sequence;
			public int LvlOffset;

			public Levelsequence(string championName, string sequenceName, List<int> sequence, int lvlOffset = 0)
			{
				ChampionName = championName;
				SequenceName = sequenceName;
				Sequence = sequence;
				LvlOffset = lvlOffset;
			}
		}
	}
}
