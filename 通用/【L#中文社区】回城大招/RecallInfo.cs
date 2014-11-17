using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using SharpDX;

namespace BaseUlt2
{
    class RecallInfo
    {
        public EnemyInfo EnemyInfo;
        public Dictionary<int, float> IncomingDamage;
        public Packet.S2C.Recall.Struct Recall;

        public RecallInfo(EnemyInfo enemyInfo)
        {
            EnemyInfo = enemyInfo;
            Recall = new Packet.S2C.Recall.Struct(EnemyInfo.Player.NetworkId, Packet.S2C.Recall.RecallStatus.Unknown, Packet.S2C.Recall.ObjectType.Player, 0);
            IncomingDamage = new Dictionary<int, float>();
        }

        public EnemyInfo UpdateRecall(Packet.S2C.Recall.Struct newRecall)
        {
            Recall = newRecall;
            return EnemyInfo;
        }

        public int GetRecallStart()
        {
            switch ((int)Recall.Status)
            {
                case (int)Packet.S2C.Recall.RecallStatus.RecallStarted:
                case (int)Packet.S2C.Recall.RecallStatus.TeleportStart:
                    return BaseUlt.RecallT[Recall.UnitNetworkId];

                default:
                    return 0;
            }
        }

        public int GetRecallEnd()
        {
            return GetRecallStart() + Recall.Duration;
        }

        public int GetRecallCountdown()
        {
            var countdown = GetRecallEnd() - Environment.TickCount;
            return countdown < 0 ? 0 : countdown;
        }

        public override string ToString()
        {
            var drawtext = EnemyInfo.Player.ChampionName + ": " + Recall.Status; //change to better string

            var countdown = GetRecallCountdown() / 1000f;

            if (countdown > 0)
                drawtext += " (" + countdown.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "s)";

            return drawtext;
        }
    }
}
