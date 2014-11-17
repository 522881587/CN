using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;
using Color = System.Drawing.Color;

namespace BaseUlt2
{
    /*
     * - Exclude specific champiosn from being baseulted
     * - baseult friends menu doesnt work as Karthus
     * */

    class BaseUlt
    {
        private static Menu _menu;
        private static Vector3 _enemySpawnPos;
        private static int _ultCasted;
        private static Spell _ult;

        public static Utility.Map.MapType Map;
        public static Dictionary<int, int> RecallT = new Dictionary<int, int>();

        private static readonly Dictionary<String, UltData> UltInfo = new Dictionary<string, UltData>
        {
            {"Jinx", new UltData { SpellStage = 1, DamageMultiplicator = 1f, Width = 140f, Delay = 600f/1000f, Speed = 1700f, Range = 20000f, Collision = true}},
            {"Ashe", new UltData { SpellStage = 0, DamageMultiplicator = 1f, Width = 130f, Delay = 250f/1000f, Speed = 1600f, Range = 20000f, Collision = true}},
            {"Draven", new UltData { SpellStage = 0, DamageMultiplicator = 0.7f, Width = 160f, Delay = 400f/1000f, Speed = 2000f, Range = 20000f, Collision = true}}, //change to single axe damage later
            {"Ezreal", new UltData { SpellStage = 0, DamageMultiplicator = 0.7f, Width = 160f, Delay = 1000f/1000f, Speed = 2000f, Range = 20000f, Collision = false}},
            {"Karthus", new UltData { SpellStage = 0, DamageMultiplicator = 1f, Width = 0f, Delay = 3125f/1000f, Speed = 0f, Range = 20000f, Collision = false}}
        };

        public BaseUlt()
        {
            (_menu = new Menu("L#涓枃绀惧尯-鍥炵▼澶ф嫑", "BaseUlt", true)).AddToMainMenu();
            _menu.AddItem(new MenuItem("showRecalls", "鏄剧ず鍥炲煄").SetValue(true));
            _menu.AddItem(new MenuItem("baseUlt", "鍩哄湴澶ф嫑").SetValue(true));
            _menu.AddItem(new MenuItem("extraDelay", "棰濆鐨勫欢杩熴劎").SetValue(new Slider(0, -2000, 2000)));
            _menu.AddItem(new MenuItem("panicKey", "绂佺敤澶ф嫑鎸夐敭").SetValue(new KeyBind(32, KeyBindType.Press))); //32 == space
            _menu.AddItem(new MenuItem("regardlessKey", "浠讳綍鏃堕棿(淇濇寔)").SetValue(new KeyBind(17, KeyBindType.Press))); //17 == ctrl

            var teamUlt = _menu.AddSubMenu(new Menu("Team Baseult Friends", "TeamUlt"));

            var compatibleChamp = IsCompatibleChamp(ObjectManager.Player.ChampionName);

            if (compatibleChamp)
                foreach (Obj_AI_Hero champ in Program.Helper.OwnTeam.Where(x => !x.IsMe && IsCompatibleChamp(x.ChampionName)))
                    teamUlt.AddItem(new MenuItem(champ.ChampionName, champ.ChampionName + " friend with Baseult?").SetValue(false).DontSave());

            _enemySpawnPos = ObjectManager.Get<GameObject>().First(x => x.Type == GameObjectType.obj_SpawnPoint && x.Team != ObjectManager.Player.Team).Position;

            Map = Utility.Map.GetMap()._MapType;

            _ult = new Spell(SpellSlot.R, 20000f);

            foreach (EnemyInfo enemyInfo in Program.Helper.EnemyInfo)
            {
                enemyInfo.RecallInfo = new RecallInfo(enemyInfo);
            }

            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Drawing.OnDraw += Drawing_OnDraw;

            if (compatibleChamp)
                Game.OnGameUpdate += Game_OnGameUpdate;

            Game.PrintChat("<font color=\"#1eff00\">BaseUlt2 by Beaving</font> - <font color=\"#00BFFF\">Loaded</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!_menu.Item("baseUlt").GetValue<bool>())
                return;

            foreach (EnemyInfo playerInfo in Program.Helper.EnemyInfo.Where(x =>
                x.Player.IsValid &&
                !x.Player.IsDead &&
                x.Player.IsEnemy &&
                x.RecallInfo.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted).OrderBy(x => x.RecallInfo.GetRecallEnd()).Where(playerInfo => Environment.TickCount - _ultCasted > 20000))
            {
                HandleRecallShot(playerInfo);
            }
        }

        private static void HandleRecallShot(EnemyInfo playerInfo)
        {
            var shoot = false;

            foreach (Obj_AI_Hero champ in Program.Helper.OwnTeam.Where(x =>
                            x.IsValid && ((x.IsMe && !x.IsStunned) || Helper.GetSafeMenuItem<bool>(_menu.Item(x.ChampionName))) &&
                            !x.IsDead &&
                            (x.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready ||
                            (x.Spellbook.GetSpell(SpellSlot.R).Level > 0 &&
                            x.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Surpressed &&
                            x.Mana >= x.Spellbook.GetSpell(SpellSlot.R).ManaCost)))) //use when fixed: champ.Spellbook.GetSpell(SpellSlot.R) = Ready or champ.Spellbook.GetSpell(SpellSlot.R).ManaCost)
            {
                if (UltInfo[champ.ChampionName].Collision && IsCollidingWithChamps(champ, _enemySpawnPos, UltInfo[champ.ChampionName].Width))
                    continue;

                //increase timeneeded if it should arrive earlier, decrease if later
                var timeneeded = GetSpellTravelTime(champ, UltInfo[champ.ChampionName].Speed, UltInfo[champ.ChampionName].Delay, _enemySpawnPos) - (_menu.Item("extraDelay").GetValue<Slider>().Value + 65);

                if (timeneeded - playerInfo.RecallInfo.GetRecallCountdown() > 60)
                    continue;

                playerInfo.RecallInfo.IncomingDamage[champ.NetworkId] = (float)Damage.GetSpellDamage(champ, playerInfo.Player, SpellSlot.R, UltInfo[champ.ChampionName].SpellStage) * UltInfo[champ.ChampionName].DamageMultiplicator;

                if (!(playerInfo.RecallInfo.GetRecallCountdown() <= timeneeded))
                    continue;
                if (champ.IsMe)
                    shoot = true;
            }

            var totalUltDamage = playerInfo.RecallInfo.IncomingDamage.Values.Sum();

            var targetHealth = Program.Helper.GetTargetHealth(playerInfo, playerInfo.RecallInfo.GetRecallCountdown());

            if (!shoot || _menu.Item("panicKey").GetValue<KeyBind>().Active)
                return;

            playerInfo.RecallInfo.IncomingDamage.Clear(); //wrong placement?

            var time = Environment.TickCount;

            if (time - playerInfo.LastSeen > 20000 && !_menu.Item("regardlessKey").GetValue<KeyBind>().Active)
            {
                if (totalUltDamage < playerInfo.Player.MaxHealth)
                    return;
            }
            else if (totalUltDamage < targetHealth)
                return;

            _ult.Cast(_enemySpawnPos, true);
            _ultCasted = time;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_menu.Item("showRecalls").GetValue<bool>())
                return;

            var index = -1;

            foreach (EnemyInfo playerInfo in Program.Helper.EnemyInfo.Where(x =>
                (x.RecallInfo.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted ||
                x.RecallInfo.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart) &&
                x.Player.IsValid &&
                !x.Player.IsDead &&
                x.RecallInfo.GetRecallCountdown() > 0 &&
                x.Player.IsEnemy).OrderBy(x => x.RecallInfo.GetRecallEnd()))
            {
                index++;

                //draw progress bar
                //show circle on minimap on recall

                Drawing.DrawText(Drawing.Width * 0.73f, Drawing.Height * 0.88f + (index * 15f), Color.Red, playerInfo.RecallInfo.ToString());
            }
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != Packet.S2C.Recall.Header)
                return;
            var newRecall = RecallDecode(args.PacketData);
            Program.Helper.EnemyInfo.Find(x => x.Player.NetworkId == newRecall.UnitNetworkId).RecallInfo.UpdateRecall(newRecall); //Packet.S2C.Recall.Decoded(args.PacketData)
        }

        public static float GetSpellTravelTime(Obj_AI_Hero source, float speed, float delay, Vector3 targetpos)
        {
            if (source.ChampionName == "Karthus")
                return delay * 1000;

            var distance = Vector3.Distance(source.ServerPosition, targetpos);

            var missilespeed = speed;

            if (source.ChampionName != "Jinx" || !(distance > 1350))
                return (distance / missilespeed + delay) * 1000;
            const float accelerationrate = 0.3f; //= (1500f - 1350f) / (2200 - speed), 1 unit = 0.3units/second

            var acceldifference = distance - 1350f;

            if (acceldifference > 150f) //it only accelerates 150 units
                acceldifference = 150f;

            var difference = distance - 1500f;

            missilespeed = (1350f * speed + acceldifference * (speed + accelerationrate * acceldifference) + difference * 2200f) / distance;

            return (distance / missilespeed + delay) * 1000;
        }

        public static bool IsCollidingWithChamps(Obj_AI_Hero source, Vector3 targetpos, float width)
        {
            var input = new PredictionInput
            {
                Radius = width,
                Unit = source,
            };

            input.CollisionObjects[0] = CollisionableObjects.Heroes;

            return Collision.GetCollision(new List<Vector3> { targetpos }, input).Any(); //x => x.NetworkId != targetnetid, hard to realize with teamult
        }

        public static Packet.S2C.Recall.Struct RecallDecode(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            var recall = new Packet.S2C.Recall.Struct();

            reader.ReadByte(); //PacketId
            reader.ReadInt32();
            recall.UnitNetworkId = reader.ReadInt32();
            reader.ReadBytes(66);

            recall.Status = Packet.S2C.Recall.RecallStatus.Unknown;

            var teleport = false;

            if (BitConverter.ToString(reader.ReadBytes(6)) != "00-00-00-00-00-00")
            {
                if (BitConverter.ToString(reader.ReadBytes(3)) != "00-00-00")
                {
                    recall.Status = Packet.S2C.Recall.RecallStatus.TeleportStart;
                    teleport = true;
                }
                else
                    recall.Status = Packet.S2C.Recall.RecallStatus.RecallStarted;
            }

            reader.Close();

            var champ = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recall.UnitNetworkId);

            if (champ == null)
                return recall;
            if (teleport)
                recall.Duration = 3500;
            else //use masteries to detect recall duration, because spelldata is not initialized yet when enemy has not been seen
            {
                recall.Duration = Map == Utility.Map.MapType.CrystalScar ? 4500 : 8000;

                if (champ.Masteries.Any(x => x.Page == MasteryPage.Utility && x.Id == 65 && x.Points == 1))
                    recall.Duration -= Map == Utility.Map.MapType.CrystalScar ? 500 : 1000; //phasewalker mastery
            }

            var time = Environment.TickCount - Game.Ping;

            if (!RecallT.ContainsKey(recall.UnitNetworkId))
                RecallT.Add(recall.UnitNetworkId, time); //will result in status RecallStarted, which would be wrong if the assembly was to be loaded while somebody recalls
            else
            {
                if (RecallT[recall.UnitNetworkId] == 0)
                    RecallT[recall.UnitNetworkId] = time;
                else
                {
                    if (time - RecallT[recall.UnitNetworkId] > recall.Duration - 75)
                        recall.Status = teleport ? Packet.S2C.Recall.RecallStatus.TeleportEnd : Packet.S2C.Recall.RecallStatus.RecallFinished;
                    else
                        recall.Status = teleport ? Packet.S2C.Recall.RecallStatus.TeleportAbort : Packet.S2C.Recall.RecallStatus.RecallAborted;

                    RecallT[recall.UnitNetworkId] = 0; //recall aborted or finished, reset status
                }
            }

            return recall;
        }

        public static bool IsCompatibleChamp(String championName)
        {
            switch (championName)
            {
                case "Ashe":
                case "Ezreal":
                case "Draven":
                case "Jinx":
                case "Karthus":
                    return true;

                default:
                    return false;
            }
        }

        private struct UltData
        {
            public float DamageMultiplicator;
            public float Delay;
            public float Range;
            public float Speed;
            public int SpellStage;
            public float Width;
            public bool Collision;
        }
    }
}
