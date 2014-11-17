using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;
namespace Primes_Ultimate_Carry
{
	class InfoWindow
	{
		private const int GuiHeight = 500;
		private const int GuiWidth = 700;
		private static readonly Vector2 StartPos = new Vector2(Drawing.Width / 2 - GuiWidth / 2, Drawing.Height / 2 - GuiHeight / 2 - 50);
		private static readonly Vector2 EndPos = new Vector2(Drawing.Width / 2 - GuiWidth / 2 + GuiWidth, StartPos.Y);

		public static void Patchnodes()
		{
			Drawing.DrawLine(StartPos, EndPos, GuiHeight, Color.DimGray);
		}

		public static void PUCInfo()
		{
			Drawing.DrawLine(StartPos, EndPos, GuiHeight, Color.DimGray);
		}

		public static void Champinfo()
		{
			Drawing.DrawLine(StartPos, EndPos, GuiHeight, Color.DimGray);
		}
	}
}
