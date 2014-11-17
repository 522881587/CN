#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Orianna
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (ObjectManager.Player.ChampionName == "Orianna")
                {
                    // ReSharper disable once ObjectCreationAsStatement
                    new Orianna();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Orianna.Program.Game_OnGameLoad: " + exception);
            }
        }
    }
}