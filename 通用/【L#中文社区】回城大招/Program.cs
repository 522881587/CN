using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace BaseUlt2
{
    class Program
    {
        public static Helper Helper;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Helper = new Helper();
            var baseult = new BaseUlt();
        }
    }
}
