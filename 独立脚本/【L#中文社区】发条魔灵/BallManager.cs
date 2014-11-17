//TODO: Add all spellcasts that move the ball, aka E

#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace Orianna
{
    internal class BallManager
    {
        private const int QSpeed = 1200;
        internal static Vector3 CurrentBallPosition;
        internal static Vector3 CurrentBallPositionDraw;
        internal static bool IsBallMoving;

        static BallManager()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.HasBuff("orianaghostself", true))
            {
                CurrentBallPosition = ObjectManager.Player.ServerPosition;
                CurrentBallPositionDraw = ObjectManager.Player.Position;
                IsBallMoving = false;
                return;
            }

            foreach (var ally in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(ally => ally.IsAlly && !ally.IsDead && ally.HasBuff("orianaghost", true)))
            {
                CurrentBallPosition = ally.ServerPosition;
                CurrentBallPositionDraw = ally.Position;
                IsBallMoving = false;
                return;
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || ObjectManager.Player.GetSpellSlot(args.SData.Name, false) != SpellSlot.Q)
            {
                return;
            }

            IsBallMoving = true;

            Utility.DelayAction.Add(
                (int) Math.Max(1, 1000 * (args.End.Distance(CurrentBallPosition) - Game.Ping / 2.0 - 0.1) / QSpeed),
                () =>
                {
                    CurrentBallPosition = args.End;
                    CurrentBallPositionDraw = args.End;
                    IsBallMoving = false;
                });
        }
    }
}