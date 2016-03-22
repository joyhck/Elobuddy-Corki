using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using GuTenTak.Corki;
using SharpDX;

namespace GuTenTak.Corki
{
    internal class Program
    {
        public const string ChampionName = "Corki";
        public static Menu Menu, ModesMenu1, ModesMenu2, ModesMenu3, DrawMenu;
        public static int SkinBase;
        public static Item Youmuu = new Item(ItemId.Youmuus_Ghostblade);
        public static Item Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
        public static Item Cutlass = new Item(ItemId.Bilgewater_Cutlass);
        public static Item Tear = new Item(ItemId.Tear_of_the_Goddess);
        public static Item Qss = new Item(ItemId.Quicksilver_Sash);
        public static Item Simitar = new Item(ItemId.Mercurial_Scimitar);
        public static Item hextech = new Item(ItemId.Hextech_Gunblade, 700);

        public static AIHeroClient PlayerInstance
        {
            get { return Player.Instance; }
        }
        private static float HealthPercent()
        {
            return (PlayerInstance.Health / PlayerInstance.MaxHealth) * 100;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool AutoQ { get; protected set; }
        public static float Manaah { get; protected set; }
        public static object GameEvent { get; private set; }

        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Active E;
        public static Spell.Skillshot R;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Game_OnStart;
        }


        static void Game_OnStart(EventArgs args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Obj_AI_Base.OnBuffGain += Common.OnBuffGain;
            Game.OnTick += OnTick;
            Gapcloser.OnGapcloser += Common.Gapcloser_OnGapCloser;
            Game.OnUpdate += Common.zigzag;
            SkinBase = Player.Instance.SkinId;
            // Item
            try
            {
                if (ChampionName != PlayerInstance.BaseSkinName)
                {
                    return;
                }

                Q = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Circular, 300, 1000, 250);
                Q.AllowedCollisionCount = int.MaxValue;
                W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Linear, 1800, 1500, 200);
                W.AllowedCollisionCount = int.MaxValue;
                E = new Spell.Active(SpellSlot.E, 600);
                R = new Spell.Skillshot(SpellSlot.R, 1300, SkillShotType.Linear, 200, 1950, 40);
                R.AllowedCollisionCount = 0;



                Bootstrap.Init(null);
                Chat.Print("GuTenTak Addon Loading Success", Color.Green);


                Menu = MainMenu.AddMenu("GuTenTak Corki", "Corki");
                Menu.AddSeparator();
                Menu.AddLabel("GuTenTak Corki Addon");

                var Enemies = EntityManager.Heroes.Enemies.Where(a => !a.IsMe).OrderBy(a => a.BaseSkinName);
                ModesMenu1 = Menu.AddSubMenu("Menu", "Modes1Corki");
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("Combo Configs");
                ModesMenu1.Add("ComboQ", new CheckBox("Use Q on Combo", true));
                ModesMenu1.Add("ComboA", new CheckBox("Use AA => Q Combo", false));
                ModesMenu1.Add("ComboE", new CheckBox("Use E on Combo", true));
                ModesMenu1.Add("ComboR", new CheckBox("Use R on Combo", true));
                ModesMenu1.Add("ManaCE", new Slider("Use E Mana %", 30));
                ModesMenu1.AddSeparator();
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("AutoHarass Configs");
                ModesMenu1.Add("AutoHarass", new CheckBox("Use R on AutoHarass", false));

                ModesMenu1.Add("ManaAuto", new Slider("Mana %", 80));
                ModesMenu1.AddLabel("Harass Configs");
                ModesMenu1.Add("HarassQ", new CheckBox("Use Q on Harass", true));
                ModesMenu1.Add("ManaHQ", new Slider("Mana %", 40));
                ModesMenu1.Add("HarassR", new CheckBox("Use R on Harass", true));
                ModesMenu1.Add("ManaHR", new Slider("Mana %", 60));
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("Kill Steal Configs");
                ModesMenu1.Add("KS", new CheckBox("Use KillSteal", true));
                ModesMenu1.Add("KQ", new CheckBox("Use Q on KillSteal", true));
                ModesMenu1.Add("KR", new CheckBox("Use R on KillSteal", true));

                ModesMenu2 = Menu.AddSubMenu("Farm", "Modes2Corki");
                ModesMenu2.AddLabel("Lane Clear Config");
                ModesMenu2.Add("ManaL", new Slider("Mana %", 40));
                ModesMenu2.Add("FarmQ", new CheckBox("Use Q on LaneClear", true));
                ModesMenu2.Add("ManaLR", new Slider("Mana %", 40));
                ModesMenu2.Add("FarmR", new CheckBox("Use R on LaneClear", true));
                ModesMenu2.AddLabel("Jungle Clear Config");
                ModesMenu2.Add("ManaJ", new Slider("Mana %", 40));
                ModesMenu2.Add("JungleQ", new CheckBox("Use Q on JungleClear", true));
                ModesMenu2.Add("ManaJR", new Slider("Mana %", 40));
                ModesMenu2.Add("JungleR", new CheckBox("Use R on JungleClear", true));

                ModesMenu3 = Menu.AddSubMenu("Misc", "Modes3Corki");
                ModesMenu3.AddLabel("Misc Configs");
                ModesMenu3.Add("AntiGap", new CheckBox("Use W for Anti-Gapcloser", true));
                ModesMenu3.AddLabel("Flee Configs");
                ModesMenu3.Add("FleeR", new CheckBox("Use R on Flee", true));
                ModesMenu3.Add("FleeW", new CheckBox("Use W on Flee", true));
                ModesMenu3.Add("ManaFlR", new Slider("R Mana %", 35));

                ModesMenu3.AddLabel("Item Usage on Combo");
                ModesMenu3.Add("useYoumuu", new CheckBox("Use Youmuu", true));
                ModesMenu3.Add("usehextech", new CheckBox("Use Hextech", true));
                ModesMenu3.Add("useBotrk", new CheckBox("Use Botrk & Cutlass", true));
                ModesMenu3.Add("useQss", new CheckBox("Use QuickSilver", true));
                ModesMenu3.Add("minHPBotrk", new Slider("Min health to use Botrk %", 80));
                ModesMenu3.Add("enemyMinHPBotrk", new Slider("Min enemy health to use Botrk %", 80));

                ModesMenu3.AddLabel("QSS Configs");
                ModesMenu3.Add("Qssmode", new ComboBox(" ", 0, "Auto", "Combo"));
                ModesMenu3.Add("Stun", new CheckBox("Stun", true));
                ModesMenu3.Add("Blind", new CheckBox("Blind", true));
                ModesMenu3.Add("Charm", new CheckBox("Charm", true));
                ModesMenu3.Add("Suppression", new CheckBox("Suppression", true));
                ModesMenu3.Add("Polymorph", new CheckBox("Polymorph", true));
                ModesMenu3.Add("Fear", new CheckBox("Fear", true));
                ModesMenu3.Add("Taunt", new CheckBox("Taunt", true));
                ModesMenu3.Add("Silence", new CheckBox("Silence", false));
                ModesMenu3.Add("QssDelay", new Slider("Use QSS Delay(ms)", 250, 0, 1000));

                ModesMenu3.AddLabel("QSS Ult Configs");
                ModesMenu3.Add("ZedUlt", new CheckBox("Zed R", true));
                ModesMenu3.Add("VladUlt", new CheckBox("Vladimir R", true));
                ModesMenu3.Add("FizzUlt", new CheckBox("Fizz R", true));
                ModesMenu3.Add("MordUlt", new CheckBox("Mordekaiser R", true));
                ModesMenu3.Add("PoppyUlt", new CheckBox("Poppy R", true));
                ModesMenu3.Add("QssUltDelay", new Slider("Use QSS Delay(ms) for Ult", 250, 0, 1000));

                ModesMenu3.AddLabel("Skin Hack");
                ModesMenu3.Add("skinhack", new CheckBox("Activate Skin hack", false));
                ModesMenu3.Add("skinId", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4", "5", "6", "7"));

                DrawMenu = Menu.AddSubMenu("Draws", "DrawCorki");
                DrawMenu.Add("drawQ", new CheckBox(" Draw Q", true));
                DrawMenu.Add("drawW", new CheckBox(" Draw W", true));
                DrawMenu.Add("drawR", new CheckBox(" Draw R", false));
                DrawMenu.Add("drawXR", new CheckBox(" Draw Don't Use R", true));
                DrawMenu.Add("drawXFleeQ", new CheckBox(" Draw Don't Use Flee Q", false));

            }

            catch (Exception e)
            {

            }

        }
        private static void Game_OnDraw(EventArgs args)
        {

            try
            {
                if (DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue)
                {
                    if (Q.IsReady() && Q.IsLearned)
                    {
                        Circle.Draw(Color.White, Q.Range, Player.Instance.Position);
                    }
                }
                if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
                {
                    if (W.IsReady() && W.IsLearned)
                    {
                        Circle.Draw(Color.White, W.Range, Player.Instance.Position);
                    }
                }
                if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
                {
                    if (R.IsReady() && R.IsLearned)
                    {
                        Circle.Draw(Color.White, R.Range, Player.Instance.Position);
                    }
                }
                if (DrawMenu["drawXR"].Cast<CheckBox>().CurrentValue)
                {
                    if (R.IsReady() && R.IsLearned)
                    {
                        Circle.Draw(Color.Red, 700, Player.Instance.Position);
                    }
                }
                if (DrawMenu["drawXFleeQ"].Cast<CheckBox>().CurrentValue)
                {
                    if (Q.IsReady() && Q.IsLearned)
                    {
                        Circle.Draw(Color.Red, 400, Player.Instance.Position);
                    }
                }

                if (R.IsReady() && R.IsLearned)
                {
                    Circle.Draw(Color.Black, R.Range, Player.Instance.Position);
                }
            }
            catch (Exception e)
            {

            }
        }
        static void Game_OnUpdate(EventArgs args)
        {
            try
            {
                var AutoHarass = ModesMenu1["AutoHarass"].Cast<CheckBox>().CurrentValue;
                var ManaAuto = ModesMenu1["ManaAuto"].Cast<Slider>().CurrentValue;

                Common.Skinhack();

            if (AutoHarass && ManaAuto <= _Player.ManaPercent)
            {
                Common.AutoR();
            }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Common.Combo();
                    Common.ItemUsage();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    Common.Harass();
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {

                    Common.LaneClear();

                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {

                    Common.JungleClear();
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    Common.LastHit();

                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                {
                    Common.Flee();

                }
            }
            catch (Exception e)
            {

            }
        }
        public static void OnTick(EventArgs args)
        {

            if (ModesMenu1["ComboA"].Cast<CheckBox>().CurrentValue)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        Orbwalker.OnPostAttack += Common.Orbwalker_OnPostAttack;
                    }
                    else
                    {
                        Orbwalker.OnPostAttack -= Common.Orbwalker_OnPostAttack;
                    }
                }
                else
                {
                    Orbwalker.OnPostAttack -= Common.Orbwalker_OnPostAttack;
                }
            Common.KillSteal();
        }
    }
}
