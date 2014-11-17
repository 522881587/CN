using LeagueSharp.Common;

namespace Primes_Ultimate_Carry
{
	class Program
	{

		// ReSharper disable once UnusedParameter.Local
		private static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad += Game_Start;
		}

		private static void Game_Start(System.EventArgs args)
		{
			// ReSharper disable once ObjectCreationAsStatement
			new PUC();
		}
	}
}
