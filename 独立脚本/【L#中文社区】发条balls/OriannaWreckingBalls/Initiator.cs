using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriannaWreckingBalls
{
    class Initiator
    {
        public string HeroName { get; set; }
        public string spellName { get; set; }
        public string SDataName { get; set; }

        public static List<Initiator> InitatorList = new List<Initiator>();

        static Initiator()
        {
            //aatrox
            InitatorList.Add(new Initiator
            {
                HeroName = "Aatrox",
                spellName = "Aatrox Q",
                SDataName = "AatroxQ"
            });

            //alistar
            InitatorList.Add(new Initiator
            {
                HeroName = "Alistar",
                spellName = "Alistar W",
                SDataName = "Headbutt"
            });

            //amumu
            InitatorList.Add(new Initiator
            {
                HeroName = "Amumu",
                spellName = "Amumu Q",
                SDataName = "BandageToss"
            });

            //elise
            InitatorList.Add(new Initiator
            {
                HeroName = "Elise",
                spellName = "Elise Spider E",
                SDataName = "elisespideredescent"
            });

            //fid
            InitatorList.Add(new Initiator
            {
                HeroName = "FiddleSticks",
                spellName = "FiddleSticks R",
                SDataName = "Crowstorm"
            });

            //fiora
            InitatorList.Add(new Initiator
            {
                HeroName = "Fiora",
                spellName = "Fiora Q",
                SDataName = "FioraQ"
            });

            //gragas
            InitatorList.Add(new Initiator
            {
                HeroName = "Gragas",
                spellName = "Gragas Q",
                SDataName = "GragasE"
            });

            //Hecarim
            InitatorList.Add(new Initiator
            {
                HeroName = "Hecarim",
                spellName = "Hecarim R",
                SDataName = "HecarimUlt"
            });

            //Irelia
            InitatorList.Add(new Initiator
            {
                HeroName = "Irelia",
                spellName = "Irelia Q",
                SDataName = "IreliaGatotsu"
            });

            //JarvanIV
            InitatorList.Add(new Initiator
            {
                HeroName = "JarvanIV",
                spellName = "JarvanIV EQ",
                SDataName = "JarvanIVDragonStrike"
            });

            
            InitatorList.Add(new Initiator
            {
                HeroName = "JarvanIV",
                spellName = "JarvanIV R",
                SDataName = "JarvanIVCataclysm"
            });

            //Jax
            InitatorList.Add(new Initiator
            {
                HeroName = "Jax",
                spellName = "Jax Q",
                SDataName = "JaxLeapStrike"
            });

            //Jayce
            InitatorList.Add(new Initiator
            {
                HeroName = "Jayce",
                spellName = "Jayce Q",
                SDataName = "JayceToTheSkies"
            });

            //Kassadin
            InitatorList.Add(new Initiator
            {
                HeroName = "Kassadin",
                spellName = "Kassadin Q",
                SDataName = "RiftWalk"
            });

            //Katarina
            InitatorList.Add(new Initiator
            {
                HeroName = "Katarina",
                spellName = "Katarina E",
                SDataName = "KatarinaE"
            });

            //Khazix
            InitatorList.Add(new Initiator
            {
                HeroName = "Khazix",
                spellName = "Khazix E",
                SDataName = "KhazixE"
            });

            
            InitatorList.Add(new Initiator
            {
                HeroName = "Khazix",
                spellName = "Khazix E(Evo)",
                SDataName = "khazixelong"
            });

            //Leblanc
            InitatorList.Add(new Initiator
            {
                HeroName = "Leblanc",
                spellName = "Leblanc W",
                SDataName = "LeblancSlide"
            });

            //LeeSin
            InitatorList.Add(new Initiator
            {
                HeroName = "LeeSin",
                spellName = "LeeSin 2nd Q",
                SDataName = "blindmonkqtwo"
            });

            //Leona
            InitatorList.Add(new Initiator
            {
                HeroName = "Leona",
                spellName = "Leona E",
                SDataName = "LeonaZenithBladeMissle"
            });

            //Lissandra
            InitatorList.Add(new Initiator
            {
                HeroName = "Lissandra",
                spellName = "Lissandra E",
                SDataName = "LissandraE"
            });

            //Malphite
            InitatorList.Add(new Initiator
            {
                HeroName = "Malphite",
                spellName = "Malphite R",
                SDataName = "UFSlash"
            });

            //Maokai
            InitatorList.Add(new Initiator
            {
                HeroName = "Maokai",
                spellName = "Maokai W",
                SDataName = "MaokaiUnstableGrowth"
            });

            //MonkeyKing
            InitatorList.Add(new Initiator
            {
                HeroName = "MonkeyKing",
                spellName = "MonkeyKing E",
                SDataName = "MonkeyKingNimbus"
            });


            InitatorList.Add(new Initiator
            {
                HeroName = "MonkeyKing",
                spellName = "MonkeyKing R",
                SDataName = "MonkeyKingSpinToWin"
            });

            //Nocturne
            InitatorList.Add(new Initiator
            {
                HeroName = "Nocturne",
                spellName = "Nocturne R",
                SDataName = "NocturneParanoia"
            });

            //Renekton
            InitatorList.Add(new Initiator
            {
                HeroName = "Renekton",
                spellName = "Renekton E",
                SDataName = "RenektonSliceAndDice"
            });

            //Rengar
            InitatorList.Add(new Initiator
            {
                HeroName = "Rengar",
                spellName = "Rengar R",
                SDataName = "RengarR"
            });

            //Rengar
            InitatorList.Add(new Initiator
            {
                HeroName = "Rengar",
                spellName = "Rengar R",
                SDataName = "RengarR"
            });

            //Sejuani
            InitatorList.Add(new Initiator
            {
                HeroName = "Sejuani",
                spellName = "Sejuani Q",
                SDataName = "SejuaniArcticAssault"
            });

            //Shaco
            InitatorList.Add(new Initiator
            {
                HeroName = "Shaco",
                spellName = "Shaco Q",
                SDataName = "Deceive"
            });

            //Shen
            InitatorList.Add(new Initiator
            {
                HeroName = "Shen",
                spellName = "Shen E",
                SDataName = "ShenShadowDash"
            });

            //Shyvana
            InitatorList.Add(new Initiator
            {
                HeroName = "Shyvana",
                spellName = "Shyvana R",
                SDataName = "ShyvanaTransformCast"
            });

            //Talon
            InitatorList.Add(new Initiator
            {
                HeroName = "Talon",
                spellName = "Talon E",
                SDataName = "TalonCutthroat"
            });

            //Thresh
            InitatorList.Add(new Initiator
            {
                HeroName = "Thresh",
                spellName = "Thresh Q",
                SDataName = "threshqleap"
            });

            //Tristana
            InitatorList.Add(new Initiator
            {
                HeroName = "Tristana",
                spellName = "Tristana W",
                SDataName = "RocketJump"
            });

            //Tryndamere
            InitatorList.Add(new Initiator
            {
                HeroName = "Tryndamere",
                spellName = "Tryndamere E",
                SDataName = "slashCast"
            });

            //Twitch
            InitatorList.Add(new Initiator
            {
                HeroName = "Twitch",
                spellName = "Twitch Q",
                SDataName = "HideInShadows"
            });

            //vi
            InitatorList.Add(new Initiator
            {
                HeroName = "Vi",
                spellName = "Vi Q",
                SDataName = "ViQ"
            });

            InitatorList.Add(new Initiator
            {
                HeroName = "Vi",
                spellName = "Vi R",
                SDataName = "ViR"
            });

            //Volibear
            InitatorList.Add(new Initiator
            {
                HeroName = "Volibear",
                spellName = "Volibear Q",
                SDataName = "VolibearQ"
            });

            //Xin Zhao
            InitatorList.Add(new Initiator
            {
                HeroName = "Xin Zhao",
                spellName = "Xin Zhao E",
                SDataName = "XenZhaoSweep"
            });

            //Xin Zac
            InitatorList.Add(new Initiator
            {
                HeroName = "Zac",
                spellName = "Zac E",
                SDataName = "ZacE"
            });

        }
    }
}
