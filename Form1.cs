using Memory; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;  
using System.Windows.Forms;


namespace AssaultCubeEternal
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);
        #region Offsets
        string LocalPlayerHealth = "ac_client.exe+0x109B74,F8";
        string LocalPlayerAmmo = "ac_client.exe+0x00109B74,FC";
        string PlayerBase = "ac_client.exe+0x109B74";
        string EntityList = "ac_client.exe+0x110D90";
        string Health = ",0xF8";
        string X = ",0x4";
        string Y = ",0x8";
        string Z = ",0xC";
        string ViewY = ",0x44";
        string ViewX = ",0x40";

        #endregion

        Mem m = new Mem();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            int PID = m.GetProcIdFromName("ac_client");
            if (PID > 0)
            {
                m.OpenProcess(PID);
                Thread AB = new Thread(Aimbot) { IsBackground = true };
                AB.Start();

                Thread HD = new Thread(HealthDrain);
                HD.Start();
            }
        }
        void Aimbot()
        {
            while (true)
            {
                if (GetAsyncKeyState(Keys.XButton2) < 0)
                {
                    var LocalPlayer = GetLocal();
                    var Players = GetPlayers(LocalPlayer);

                    Players = Players.OrderBy(o => o.Magnitude).ToList();

                    if (Players.Count != 0)
                    {
                        Aim(LocalPlayer, Players[0]);
                    }
                    Thread.Sleep(1);
                }
            }
        }

        void HealthDrain()
        {
            while (true)
            {
                m.WriteMemory(LocalPlayerAmmo,"int","999");
                m.WriteMemory(LocalPlayerHealth,"int","1337");
            }
        }

        Player GetLocal()
        {
            var Player = new Player
            {
                X = m.ReadFloat(PlayerBase + X),
                Y = m.ReadFloat(PlayerBase + Y),
                Z = m.ReadFloat(PlayerBase + Z)
            };
            return Player;
        }

        float GetMag(Player player, Player entity)
        {
            float mag;

            mag = (float)Math.Sqrt(Math.Pow(entity.X - player.X, 2) + Math.Pow(entity.Y - player.Y, 2) + Math.Pow(entity.Z - player.Z, 2));

            return mag;
        }
      
        

        void Aim(Player Player, Player Enemy)
        {
            float deltaX = Enemy.X - Player.X;
            float deltaY = Enemy.Y - Player.Y;

            float viewX = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI) + 90;

            float deltaZ = Enemy.Z - Player.Z;

            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            float viewY = (float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);


            m.WriteMemory(PlayerBase + ViewX, "float", viewX.ToString());

            m.WriteMemory(PlayerBase + ViewY, "float", viewY.ToString());

        }

        List<Player> GetPlayers(Player local)
        {
            var players = new List<Player>();

            for (int i = 0; i < 20; i++)
            {
                var CurrentStr = EntityList + ",0x" + (i * 0x4).ToString("X");

                var Player = new Player
                {
                    X = m.ReadFloat(CurrentStr + X),
                    Y = m.ReadFloat(CurrentStr + Y),
                    Z = m.ReadFloat(CurrentStr + Z),
                    Health = m.ReadInt(CurrentStr + Health)
                };
                Player.Magnitude = GetMag(local, Player);

                if (Player.Health > 0 && Player.Health < 102)
                    players.Add(Player);
            }

            return players;
        }

              
    }   
}