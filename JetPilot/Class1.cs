using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using InfinityScript;

namespace JetPilot
{
    public class JetPilot : BaseScript
    {

        public string JetModel = "vehicle_b2_bomber";
        public int Third = 0;


        public JetPilot()
            : base()
        {
            changeGametype(".^1JetPilot^7.");
            Call("precacheshader", "remote_turret_overlay_mp");
            PlayerConnected += new Action<Entity>(ent =>
            {
               
                ent.SetField("did", 0);
                ent.SetField("minigun", 1);
                ent.SetField("missile", 0);
                ent.SetField("blast", 0);
                ent.SetField("doattack", 0);
                ent.SetField("ThirdPerson", "0");
                ent.SetField("surri", 0);
                ent.SetField("cooling", 0);
                ent.Call("notifyonplayercommand", "changed", "weapnext");
                ent.Call("notifyonplayercommand", "attack", "+attack");
                ent.Call("notifyonplayercommand", "Nattack", "-attack");
                ent.Call("notifyonplayercommand", "tab", "+scores");
                ent.Call("notifyonplayercommand", "-tab", "-scores");
                ent.Call("notifyonplayercommand", "3rd", "+frag");
                MakeHUD(ent);
                makeWeapons(ent);
                ent.SetField("Rsens", ent.Call<string>("getdvar", "sensitivity"));
                ent.SpawnedPlayer += new Action(() =>
                {
                    PlayerSpawned(ent);

                });

            });

            PlayerDisconnected += new Action<Entity>(ent => {
                ent.SetClientDvar("sensitivity", ent.GetField<string>("Rsens"));
                
            });

        }

        void PlayerSpawned(Entity player)
        {
            // Call("setdvar" ,"scr_player_sprinttime", "800");

            player.Origin = new Vector3(player.Origin.X, player.Origin.Y, player.Origin.Z + 5000);
            JetStart(player, 100, false, 0);
        }

        void JetStart(Entity player, int Speed, bool silenced, int ThirdPerson)
        {
            
            player.SetClientDvar("sensitivity", "1");
            Call("setdvar", "sensitivity", "1");
            player.TakeAllWeapons();
            MoveSpeed(player, Speed);
            if (ThirdPerson == 1)
            {
                player.SetClientDvar("cg_thirdPerson", "1");
                player.SetClientDvar("cg_thirdPersonRange", "750");
            }
            player.OnNotify("3rd", (ent) =>
            {
                string value = ent.GetField<string>("ThirdPerson") == "0" ? "1" : "0";
                ent.SetField("ThirdPerson", value);
                ent.SetClientDvar("cg_thirdperson", value);
                ent.SetClientDvar("cg_thirdPersonRange", "750");
            });
            UseMinigun(player);

            //FixBug(player);
            
            //remote_uav_mp
            //vehicle_av8b_harrier_jet_mp
            if (player.GetField<int>("did") == 0)
            {
                WeaponSwitch(player);
                player.Call("visionsetnightforplayer", "thermal_mp", 1.0f);
                Call("playfxontag", Call<int>("loadfx", "smoke/jet_contrail"), player, "tag_right_wingtip");
                Call("playfxontag", Call<int>("loadfx", "smoke/jet_contrail"), player, "tag_left_wingtip");
                Call("playfxontag", Call<int>("loadfx", "fire/jet_afterburner"), player, "tag_engine_right");
                Call("playfxontag", Call<int>("loadfx", "fire/jet_afterburner"), player, "tag_engine_left");
                player.Call("attach", "vehicle_av8b_harrier_jet_mp", "tag_weapon_left", true);
                player.Call("playloopsound", "veh_aastrike_flyover_loop");
                player.SetField("did", 1);
            }
            // player.Call("setmodel", "vehicle_av8b_harrier_jet_mp");


        }

        void FixBug(Entity player)
        {
            player.OnNotify("death", (ent, assw) =>
            {
                UseMinigun(player);
            });

        }
        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            UseMinigun(player);
            int fxid = Call<int>("loadfx", "explosions/aerial_explosion_ac130_coop");
            Call("playfx", fxid, player.Origin);
            player.Call("stopLoopSound");
        }

        void MakeHUD(Entity player)
        {


            HudElem minigun = HudElem.CreateFontString(player, "default", 1.6f);
            minigun.SetPoint("RIGHT", "RIGHT", -10, 0);
            minigun.SetText("MINIGUN");
            minigun.GlowColor = new Vector3(0f, 1f, 0f);
            minigun.GlowAlpha = 0.8f;
            minigun.HideWhenInMenu = true;

            HudElem missile = HudElem.CreateFontString(player, "default", 1.5f);
            missile.SetPoint("RIGHT", "RIGHT", -10, 40);
            missile.SetText("MISSILE");
            missile.GlowColor = new Vector3(0f, 1f, 0f);
            missile.GlowAlpha = 0.0f;
            missile.HideWhenInMenu = true;


            HudElem credits = HudElem.CreateFontString(player, "hudbig", 1.2f);
            credits.SetPoint("CENTER", "BOTTOM", 0, -70);
            credits.Call("settext", "JetPilot Mod by bingo007");
            credits.Alpha = 0f;
            credits.SetField("glowcolor", new Vector3(0.2f, 1f, 0.8f));
            credits.GlowAlpha = 1f;
            HudElem credits1 = HudElem.CreateFontString(player, "hudbig", 0.8f);
            credits1.SetPoint("CENTER", "BOTTOM", 0, -55);
            credits1.Call("settext", "tuttocarbonio.altervista.org");
            credits1.Alpha = 0f;
            credits1.SetField("glowcolor", new Vector3(1f, 0f, 0.5f));
            credits1.GlowAlpha = 1f;

            HudElem instr = HudElem.CreateFontString(player, "default", 1.1f);
            instr.SetPoint("TOP", "TOP", 300, 420);
            instr.Call("settext", "Press ^3[{weapnext}] ^7to change weapon.");
            instr.HideWhenInMenu = true;
            HudElem instr1 = HudElem.CreateFontString(player, "default", 1.1f);
            instr1.SetPoint("TOP", "TOP", 300, 410);
            instr1.Call("settext", "Press ^3[{+frag}] ^7to toggle 3rd person");
            instr1.HideWhenInMenu = true;
            player.Call("VisionSetMissilecamForPlayer", "black_bw", 1.0f);
            HudElem stati = HudElem.NewClientHudElem(player);
            stati.HorzAlign = "fullscreen";
            stati.VertAlign = "fullscreen";
            stati.SetShader("ac130_overlay_grain", 640, 480);
            stati.Archived = true;
            stati.Sort = 20;
            stati.Alpha = 0.3f;


            //HudElem stati1 = HudElem.NewClientHudElem(player);
            //stati1.HorzAlign = "fullscreen";
            //stati1.VertAlign = "fullscreen";
            //stati1.SetShader("predator2xhair", 256, 256);
            //stati1.Archived = true;
            HudElem overlay = HudElem.NewClientHudElem(player);
            overlay.X = 0;
            overlay.Y = 0;
            overlay.AlignX = "left";
            overlay.AlignY = "top";
            overlay.HorzAlign = "fullscreen";
            overlay.VertAlign = "fullscreen";
            overlay.SetShader("remote_turret_overlay_mp", 640, 480);
            overlay.Sort = -10;
            overlay.Archived = true;

            HudElem firebar = HudElem.NewClientHudElem(player);
            firebar.X = 0;
            firebar.Y = 10;
            firebar.AlignX = "center";
            firebar.AlignY = "bottom";
            firebar.HorzAlign = "center";
            firebar.VertAlign = "bottom";
            firebar.SetShader("white", 200, 8);
            firebar.Alpha = 0.3f;
            firebar.HideWhenInMenu = true;
            firebar.Foreground = false;

            HudElem firebar1 = HudElem.NewClientHudElem(player);
            firebar1.X = 0;
            firebar1.Y = 10;
            firebar1.AlignX = "center";
            firebar1.AlignY = "bottom";
            firebar1.HorzAlign = "center";
            firebar1.VertAlign = "bottom";
            firebar1.SetShader("white", 0, 8);
            firebar1.Alpha = 0.7f;
            firebar1.Color = new Vector3(1, 0, 0);
            firebar1.HideWhenInMenu = true;

            HudElem heat = HudElem.CreateFontString(player, "default", 1.8f);
            heat.SetPoint("BOTTOM", "BOTTOM", 0, 0);
            heat.SetText("WEAPONS IN COOLING");
            heat.GlowAlpha = 0.5f;
            heat.GlowColor = new Vector3(1f, 0f, 0f);

            heat.Alpha = 0f;
            heat.HideWhenInMenu = true;


            player.OnInterval(80, (ent) =>
            {
                int newsurri = player.GetField<int>("surri") - 4;
                if (newsurri < 0) { newsurri = 0; player.SetField("cooling", 0); }
                if (newsurri > 200) { newsurri = 200; player.SetField("cooling", 1); }
                player.SetField("surri", newsurri);
                if (player.GetField<int>("cooling") == 0)
                {
                    heat.Alpha = 0f;
                }
                //200 : 200 = x : newsurri
                // firebar1.Width = (200 * newsurri) / 200;
                firebar1.SetShader("white", ((200 * newsurri) / 200), 8);
                return true;
            });



            player.OnNotify("tab", (ent) => { credits.Alpha = 1f; credits1.Alpha = 1f; });
            player.OnNotify("-tab", (ent) => { credits.Alpha = 0f; credits1.Alpha = 0f; });

            player.OnInterval(200, (ent) =>
            {
                if (player.GetField<int>("minigun") == 1)
                {
                    minigun.GlowAlpha = 0.8f;
                    minigun.Alpha = 1f;
                    missile.GlowAlpha = 0.0f;

                    minigun.FontScale = 1.6f;
                    missile.FontScale = 1.5f;
                   // missile.Call("FadeOverTime", 1);
                    missile.Alpha = 0.1f;

                }
                if (player.GetField<int>("missile") == 1)
                {
                    minigun.GlowAlpha = 0.0f;
                    missile.GlowAlpha = 0.8f;
                    missile.Alpha = 1f;

                    minigun.FontScale = 1.5f;
                    missile.FontScale = 1.6f;
                    //minigun.Call("FadeOverTime", 1);
                    minigun.Alpha = 0.1f;

                }
                return true;
            });


        }

        void MoveSpeed(Entity player, int Speed)
        {
            Entity Jet = Call<Entity>("spawn", "script_origin", player.Origin);
            // Entity JetM = Call<Entity>("spawnHelicopter", player, player.Origin, Call<Vector3>("anglestoforward", player.Call<Vector3>("getplayerangles")), "harrier_mp", "vehicle_av8b_harrier_jet_mp");
            player.OnInterval(100, (ent) =>
            {
                if (!player.IsAlive) return false;
                Jet.Origin = player.Origin;
                // player.Origin = Jet.Origin;
                player.Call("playerlinkto", Jet);
                // Jet.Call("linkto", player);
                Vector3 vec = Call<Vector3>("anglestoforward", player.Call<Vector3>("getplayerangles")); ; ;
                Vector3 vecGuess = new Vector3(vec.X * Speed, vec.Y * Speed, vec.Z * Speed);
                //Jet.Call("setorigin", new Vector3(vec.X + vecGuess.X, vec.Y + vecGuess.Y, vec.Z + vecGuess.Z));
                Jet.Origin += vecGuess;
                // JetM.Origin = player.Origin;
                return true;
            });
        }

        void makeWeapons(Entity player)
        {
            player.OnNotify("attack", (ent) =>
            {
                if (player.GetField<int>("minigun") == 1)
                {
                    player.SetField("doattack", 1);

                    player.OnInterval(113, (enti) =>
                    {
                        if (player.GetField<int>("doattack") == 0) return false;
                        if (!player.IsAlive) return false;
                        if (!player.IsPlayer) return false;
                        if (player.GetField<int>("cooling") == 1) { return false; }
                        Vector3 irepos = GetCursorPos(player);
                        Call("magicbullet", "ac130_25mm_mp", player.Origin, irepos, player);
                        int newsurri = player.GetField<int>("surri") + 10;
                        player.SetField("surri", newsurri);
                        return true;
                    });

                }
                if (player.GetField<int>("missile") == 1)
                {
                    Vector3 irepos = GetCursorPos(player);
                    if (player.GetField<int>("cooling") != 1 && player.IsAlive && player.IsPlayer)
                    {
                        int newsurri = player.GetField<int>("surri") + 20;
                        player.SetField("surri", newsurri);
                        Call("magicbullet", "stinger_mp", player.Origin, irepos, player);
                    }
                }
            });
            player.OnNotify("Nattack", (ent) => { player.SetField("doattack", 0); });
        }

        void WeaponSwitch(Entity player)
        {
            player.OnNotify("changed", (ent) =>
            {

                if (player.GetField<int>("minigun") == 1)
                {
                    UseMissile(player);
                    return;
                }
                if (player.GetField<int>("missile") == 1)
                {
                    UseMinigun(player);
                    return;
                }


            });
        }

        void UseMinigun(Entity player)
        {
            player.SetField("minigun", 1);
            player.SetField("missile", 0);
            player.SetField("blast", 0);
        }
        void UseMissile(Entity player)
        {
            player.SetField("minigun", 0);
            player.SetField("missile", 1);
            player.SetField("blast", 0);
        }
        Vector3 GetCursorPos(Entity player)
        {
            Vector3 forw = player.Call<Vector3>("gettagorigin", "tag_eye");
            Vector3 asd = Call<Vector3>("anglestoforward", player.Call<Vector3>("getplayerangles"));
            Vector3 end = new Vector3(asd.X * 1000000, asd.Y * 1000000, asd.Z * 1000000);
            //Vector3 location = Call<Vector3>("bullettrace", forw, end, 0, player);
            return end;
        }

        bool _changed = false;
        IntPtr memory;
        private unsafe void changeGametype(string gametype)
        {
            byte[] gametypestring;
            if (_changed)
            {
                gametypestring = new System.Text.UTF8Encoding().GetBytes(gametype);
                if (gametypestring.Length >= 64) gametypestring[64] = 0x0;
                Marshal.Copy(gametypestring, 0, memory, gametype.Length > 64 ? 64 : gametype.Length);
                return;
            }
            memory = alloc(64);
            gametypestring = new System.Text.UTF8Encoding().GetBytes(gametype);
            if (gametypestring.Length >= 64) gametypestring[64] = 0x0;
            Marshal.Copy(gametypestring, 0, memory, gametype.Length > 64 ? 64 : gametype.Length);
            *(byte*)0x4EB983 = 0x68;
            *(int*)0x4EB984 = (int)memory;
            *(byte*)0x4EB988 = 0x90;
            *(byte*)0x4EB989 = 0x90;
            *(byte*)0x4EB98A = 0x90;
            *(byte*)0x4EB98B = 0x90;
            _changed = true;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);
        public IntPtr alloc(int size)
        {
            return VirtualAlloc(IntPtr.Zero, (UIntPtr)size, 0x3000, 0x40);
        }

    }
}
