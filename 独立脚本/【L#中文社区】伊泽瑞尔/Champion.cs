using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

abstract class Champion
{
    protected Obj_AI_Hero Player;
    protected Menu Menu;
    protected Orbwalking.Orbwalker Orbwalker;
    protected SpellManager Spells;

    private int tick = 1000 / 20;
    private int lastTick = Environment.TickCount;
    private string ChampName;
    private SkinManager SkinManager;

    public Champion(string name)
    {
        ChampName = name;

        CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
    }

    private void Game_OnGameLoad(EventArgs args)
    {
        Player = ObjectManager.Player;

        if (ChampName.ToLower() != Player.ChampionName.ToLower())
            return;

        SkinManager = new SkinManager();
        Spells = new SpellManager();

        InitializeSpells(ref Spells);
        InitializeSkins(ref SkinManager);

        Menu = new Menu("Easy" + ChampName, "Easy" + ChampName, true);

        SkinManager.AddToMenu(ref Menu);

        Menu.AddSubMenu(new Menu("鐩爣閫夋嫨", "Target Selector"));
        SimpleTs.AddToMenu(Menu.SubMenu("Target Selector"));

        Menu.AddSubMenu(new Menu("璧扮爫", "Orbwalker"));
        Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

        InitializeMenu();

        Menu.AddItem(new MenuItem("Recall_block", "鍥炲煄鍋滄鑴氭湰").SetValue(true));
        Menu.AddToMainMenu();

        Game.OnGameUpdate += Game_OnGameUpdate;
        Game.OnGameEnd += Game_OnGameEnd;
        Drawing.OnDraw += Drawing_OnDraw;

        try
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string amount = wc.UploadString("http://niels-wouters.be/LeagueSharp/playcount.php", "assembly=" + ChampName);
                Game.PrintChat("Easy" + ChampName + " is loaded! This assembly has been played in " + amount + " games.");
            }
        }
        catch (Exception)
        {
            Game.PrintChat("Easy" + ChampName + " is loaded! Error trying to contact EasyServer!");
        }
    }

    private void Drawing_OnDraw(EventArgs args)
    {
        Draw();
    }

    private void Game_OnGameEnd(GameEndEventArgs args)
    {
        using (WebClient wc = new WebClient())
        {
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            wc.UploadString("http://niels-wouters.be/LeagueSharp/stats.php", "assembly=" + ChampName);
        }
    }

    private void Game_OnGameUpdate(EventArgs args)
    {
        if (Environment.TickCount < lastTick + tick) return;
        lastTick = Environment.TickCount;

        SkinManager.Update();

        Update();

        if ((Menu.Item("Recall_block").GetValue<bool>() && Player.HasBuff("Recall")) || Player.IsWindingUp)
            return;

        bool minionBlock = false;

        foreach (Obj_AI_Minion minion in MinionManager.GetMinions(Player.Position, Player.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
        {
            if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(Player, minion, false))
                minionBlock = true;
        }

        switch (Orbwalker.ActiveMode)
        {
            case Orbwalking.OrbwalkingMode.Combo:
                Combo();
                break;
            case Orbwalking.OrbwalkingMode.Mixed:
                if (!minionBlock) Harass();
                break;
            default:
                if (!minionBlock) Auto();
                break;
        }
    }

    protected virtual void InitializeSkins(ref SkinManager Skins) { }
    protected virtual void InitializeSpells(ref SpellManager Spells) { }
    protected virtual void InitializeMenu() { }

    protected virtual void Update() { }
    protected virtual void Draw() { }
    protected virtual void Combo() { }
    protected virtual void Harass() { }
    protected virtual void Auto() { }

    protected void DrawCircle(string menuItem, string spell)
    {
        Circle circle = Menu.Item(menuItem).GetValue<Circle>();
        if (circle.Active) Utility.DrawCircle(Player.Position, Spells.get(spell).Range, circle.Color);
    }
}