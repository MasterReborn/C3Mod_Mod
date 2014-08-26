using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace C3Mod.GameTypes
{
    internal class Apocalypse
    {
        public static bool Running = false;
        public static bool Intermission = false;
        public static int Wave = 1;
        public static int VoteCount = 0;
        public static DateTime countDownTick = DateTime.UtcNow;
        public static DateTime voteCountDown = DateTime.UtcNow;
        public static DateTime scoreNotify = DateTime.UtcNow;
        public static DateTime WaveEndTime = DateTime.UtcNow;
        public static int MonstersLeft = -1;
        public static NPC CurMonster = null;
        public static Vector2 SpectatorArea = new Vector2();
        public static Vector2 PlayerSpawn = new Vector2();
        public static Vector2 MonsterSpawn = new Vector2();
        public static List<NPC> MonsterWhoAmI = new List<NPC>();
        public static int StartCount = 3;
        public static int LastMonster = 0;
        public static int playersdead = 0;
        public static int apocplayers = 0;

        //NEW CODE TO EXPAND THE APOCAYLPSE
        public static DateTime monsterIntermission = DateTime.UtcNow;
        public static int MonsterApocalypseIntermissionTimeCache = 0;
        //NEW CODE TO EXPAND THE APOCAYLPSE

		public static void OnUpdate(EventArgs args)
        {
            if (C3Mod.VoteRunning && C3Mod.VoteType == "apoc")
            {
                int VotedPlayers = 0;
                int TotalPlayers = 0;

                lock (C3Mod.C3Players)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "" || player.GameType == "apoc")
                            TotalPlayers++;
                        if (player.GameType == "apoc")
                            VotedPlayers++;
                    }
                }

                if (VotedPlayers == TotalPlayers)
                {
                    C3Tools.BroadcastMessageToGametype("apoc", "Vote to play Monster Apocalypse passed! Teleporting to start positions!", Color.DarkCyan);
                    bool[] playersDead = new bool[Main.maxNetPlayers];                    
                    TpToSpawnPoint();
                    countDownTick = DateTime.UtcNow;

                    //NEW CODE TO EXPAND THE APOCAYLPSE //Must reset all after NEW vote!
                    Wave = 1;
                    VoteCount = 0;
                    MonstersLeft = -1;
                    CurMonster = null;
                    MonsterWhoAmI.Clear();
                    StartCount = 3;
                    LastMonster = 0;
                    playersdead = 0;
                    monsterIntermission = DateTime.UtcNow;
                    MonsterApocalypseIntermissionTimeCache = 0;
                    //NEW CODE TO EXPAND THE APOCAYLPSE

                    Intermission = true;
                    C3Mod.VoteType = "";
                    C3Mod.VoteRunning = false;
                    ChooseNPC();
                    return;
                }

                double tick = (DateTime.UtcNow - voteCountDown).TotalMilliseconds;
                if (tick > (C3Mod.C3Config.VoteNotifyInterval * 1000) && VoteCount > 0)
                {
                    if (VoteCount != 1 && VoteCount < (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval))
                    {
                        C3Tools.BroadcastMessageToGametype("apoc", "Vote still in progress, please be patient!", Color.Cyan);
                        C3Tools.BroadcastMessageToGametype("", "Vote to play Monster Apocalypse in progress, type /join to join the lobby!", Color.Cyan);
                    }

                    VoteCount--;
                    voteCountDown = DateTime.UtcNow;
                }
                else if (VoteCount == 0)
                {
                    C3Mod.VoteRunning = false;

                    int players = 0;

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "apoc")
                            players++;
                    }

                    if (players >= C3Mod.C3Config.MonsterApocalypseMinimumPlayers)
                    {
                        C3Tools.BroadcastMessageToGametype("apoc", "Vote to play Monster Apocalypse passed! Teleporting to start positions!", Color.DarkCyan);
                        bool[] playersDead = new bool[Main.maxNetPlayers];

                        //NEW CODE TO EXPAND THE APOCAYLPSE //Must reset all after NEW vote!
                        Wave = 1;
                        VoteCount = 0;
                        MonstersLeft = -1;
                        CurMonster = null;
                        MonsterWhoAmI.Clear();
                        StartCount = 3;
                        LastMonster = 0;
                        playersdead = 0;
                        monsterIntermission = DateTime.UtcNow;
                        MonsterApocalypseIntermissionTimeCache = 0;
                        //NEW CODE TO EXPAND THE APOCAYLPSE

                        TpToSpawnPoint();
                        countDownTick = DateTime.UtcNow;
                        Intermission = true;
                        C3Mod.VoteType = "";
                        C3Mod.VoteRunning = false;
                        ChooseNPC();
                        return;
                    }
                    else
                        C3Tools.BroadcastMessageToGametype("apoc", "Vote to play Monster Apocalypse failed. Not enough players!", Color.DarkCyan);
                }
            }

            if (Intermission)
            {
                double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
                if (tick > 1000 && StartCount > -1)
                {
                    if (TpToSpawnPoint() > 0)
                    {
                        if (StartCount == 0)
                        {
                            C3Tools.BroadcastMessageToGametype("apoc", "Fight!!!", Color.Cyan);
                            StartCount = 3;

                            //NEW CODE TO EXPAND THE APOCAYLPSE
                            C3Tools.BroadcastMessageToGametype("apoc", "Wave: " + Wave.ToString() + ": " + CurMonster.name, Color.Red);
                            MonsterApocalypseIntermissionTimeCache = C3Mod.C3Config.MonsterApocalypseIntermissionTime;
                            //NEW CODE TO EXPAND THE APOCAYLPSE

                            Intermission = false;
                            Running = true;
                            SpawnMonsters();
                            playersdead = 0;

                            foreach (C3Player player in C3Mod.C3Players)
                            {
                                player.Dead = false;
                                player.LivesUsed = 0;
                            }
                        }
                        else
                        {
                            Random r = new Random();

                            switch (r.Next(2) + 1) //Change the 2 into the number of your variations! And add the same number of cases!
                            {
                                case 1:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Apocalypse in: " + StartCount.ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 2:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "You will die in: " + StartCount.ToString() + "...", Color.Cyan);
                                        break;
                                    }
                            }
                            //C3Tools.BroadcastMessageToGametype("apoc", "Nächste Monster sind: " + CurMonster.name, Color.Cyan); //Removed because WRONG Line //ADDED some lines above
                            countDownTick = DateTime.UtcNow;
                            StartCount--;
                        }
                    }
                    else
                    {
                        StartCount = 3;
                        C3Tools.ResetGameType("apoc");
                        return;
                    }
                }
            }

            if (Running)
            {
                double tick = (DateTime.UtcNow - scoreNotify).TotalMilliseconds;
                if (tick > (C3Mod.C3Config.MonsterApocalypseScoreNotifyInterval * 1000))
                {
                    C3Tools.BroadcastMessageToGametype("apoc",  MonstersLeft.ToString() + " Monsters left: ", Color.Cyan);
                    scoreNotify = DateTime.UtcNow;
                }

                //NEW CODE TO EXPAND THE APOCAYLPSE
                double tickmonsterIntermission = (DateTime.UtcNow - monsterIntermission).TotalMilliseconds;
                if (tickmonsterIntermission > 1000)
                {
                    MonsterApocalypseIntermissionTimeCache--;
                    monsterIntermission = DateTime.UtcNow;

                    if (C3Mod.C3Config.MonsterApocalypseIntermissionTime >= 3 && MonsterApocalypseIntermissionTimeCache <= 10 && MonsterApocalypseIntermissionTimeCache > 0) //MonsterApocalypseIntermissionTime must be larger then 3 or it makes no sence ;-)
                    {
                        C3Tools.BroadcastMessageToGametype("apoc", MonsterApocalypseIntermissionTimeCache.ToString() + " seconds left!", Color.Yellow);
                    }
                }
                //NEW CODE TO EXPAND THE APOCAYLPSE
                
                foreach (NPC npc in MonsterWhoAmI)
                {
                    if (!npc.active)
                    {
                        MonsterWhoAmI.Remove(npc);
                        MonstersLeft--;

                        if (MonstersLeft == 0)
                        {
                            Running = false;
                            Intermission = true;
                            Wave++;
                            ChooseNPC();
                            return;
                        }

                        break;
                    }
                }

                if (MonstersLeft == 0)
                {
                    Running = false;
                    Intermission = true;
                    ChooseNPC();
                    Wave++;
                    List<C3Player> AlivePlayers = new List<C3Player>();
                    List<C3Player> SpecatingPlayers = new List<C3Player>();

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "apoc")
                        {
                            if (player.Spectator)
                                SpecatingPlayers.Add(player);
                            else
                                AlivePlayers.Add(player);
                        }
                    }

                    C3Events.WaveAdvance(AlivePlayers, SpecatingPlayers, Wave);

                    return;
                }


                apocplayers = 0;

                lock(C3Mod.C3Players)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "apoc")
                        {
                            apocplayers++;

                            player.TSPlayer.TPlayer.hostile = false;
                            NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", player.TSPlayer.Index);

                            if (player.TerrariaTeam != 3)
                                player.TSPlayer.SetTeam(3);

                            if (!player.TSPlayer.TpLock)
                                if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }

                            if (Main.player[player.Index].dead)
                            {
                                player.LivesUsed++;
                                player.TSPlayer.TpLock = false;

                                if (player.LivesUsed >= C3Mod.C3Config.MonsterApocalypseLivesPerWave && !player.Dead)
                                {
                                    playersdead++;
                                    if (playersdead == apocplayers)
                                    {
                                        //NEW CODE TO EXPAND THE APOCAYLPSE
                                        ApoLost(player, 0); //0 = All death
                                        //NEW CODE TO EXPAND THE APOCAYLPSE
                                    }
                                    else
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", player.PlayerName + " is out!", Color.Cyan);
                                        player.TSPlayer.Teleport((int)SpectatorArea.X*16, (int)SpectatorArea.Y*16);
                                        player.Dead = true;
                                    }
                                }
                                else
                                {
                                    player.SendMessage("Lives left: " + (C3Mod.C3Config.MonsterApocalypseLivesPerWave - player.LivesUsed).ToString(), Color.Cyan);
                                    player.TSPlayer.Teleport((int)PlayerSpawn.X*16, (int)PlayerSpawn.Y*16);
                                }
                            }

                            //NEW CODE TO EXPAND THE APOCAYLPSE
                            if (MonsterApocalypseIntermissionTimeCache < 0)
                            {
                                ApoLost(player, 1); //1 = Not in time
                            }
                            //NEW CODE TO EXPAND THE APOCAYLPSE
                        }
                    }
                }
            }
        }

        public static void ApoLost(C3Player player, int reason)
        {
            switch (reason)
            {
                case 0:
                    {
                        C3Tools.BroadcastMessageToGametype("apoc", "You lost! All died!", Color.Cyan);
                        C3Tools.BroadcastMessageToGametype("", "The Apocalypse failed, because all are dead! Wimps!", Color.Cyan);
                        break;
                    }
                case 1:
                    {
                        C3Tools.BroadcastMessageToGametype("apoc", "Lost! Idiots! Your not fast enough!", Color.Cyan);
                        C3Tools.BroadcastMessageToGametype("", "The Apocalypse was not defeated in time! Wimps!", Color.Cyan);
                        break;
                    }
            }

            TpToSpawns(false);
            C3Tools.ResetGameType("apoc");
            Running = false;
            Intermission = false;
            StartCount = 3;

            //NEW CODE TO EXPAND THE APOCAYLPSE
            MonsterApocalypseIntermissionTimeCache = C3Mod.C3Config.MonsterApocalypseIntermissionTime;
            //NEW CODE TO EXPAND THE APOCAYLPSE

            List<C3Player> Players = new List<C3Player>();

            foreach (C3Player player1 in C3Mod.C3Players)
            {
                if (player1.GameType == "apoc")
                {
                    Players.Add(player);
                }
            }

            C3Events.GameEnd(new List<C3Player>(), Players, "apoc", 0, Wave);
        }

        public static void SpawnMonsters()
        {
            #region Calculating Monstercount
            //MonstersLeft = (((Wave / 3) + 1) * 10) * C3Mod.C3Config.MonsterApocalypseMultiplicator; //OLD Calculation of Monstercount
            //NEW CODE TO EXPAND THE APOCAYLPSE
            int max = 24;

            double multiplier = Wave / 5.0;
            if (multiplier < 1.0)
                multiplier = 1.0;

            if (Wave >= 10)
                multiplier *= (Wave * 0.15);

            int playerNum = (apocplayers / 5);

            if (playerNum <= 0)
            {
                max += (int)(3 * multiplier);
            }
            else
            {
                max += (int)((playerNum * 6) * multiplier);
            }

            double difficutly = C3Mod.C3Config.MonsterApocalypseDifficulty;
            if (Wave < 10)
                difficutly *= (Wave * 0.1);
            if (difficutly < 0.5)
                difficutly = 0.5;
            max = (int)(max * difficutly);

            if (Wave <= 1)
            {
                max = (int)(max * 0.2);
            }
            else if (Wave < 3)
            {
                max = (int)(max * 0.4);
            }
            else if (Wave < 4)
            {
                max = (int)(max * 0.6);
            }
            else if (Wave < 5)
            {
                max = (int)(max * 0.8);
            }

            MonstersLeft = max;

            //if (MonstersLeft >= C3Mod.C3Config.MonsterApocalypseMaxMonster)
            //{
            //    MonstersLeft = C3Mod.C3Config.MonsterApocalypseMaxMonster;
            //}
            //NEW CODE TO EXPAND THE APOCAYLPSE
            #endregion

            #region Calculation Monsterlife
            if (CurMonster.boss)
            {
                var npc = new NPC();
                Random r = new Random();
                int amount = r.Next(3) + 1;

                for (int i = 0; i < amount; i++)
                {
                    int npcid = NPC.NewNPC((int)(MonsterSpawn.X * 16) - r.Next(-32, 32), (int)MonsterSpawn.Y * 16, CurMonster.type);
                    Main.npc[npcid].SetDefaults(CurMonster.name);

                    //Main.npc[npcid].life = Main.npc[npcid].life * (Wave / 3) + 1; //OLD Calculation of Life
                    //NEW CODE TO EXPAND THE APOCAYLPSE
                    int life = (int)(16 * C3Mod.C3Config.MonsterApocalypseDifficulty * (Wave / 5.0));
                    double multiplier2 = (apocplayers / 2.0);
                    if (multiplier2 < 1.0) multiplier2 = 1.0;
                    life = (int)(life * multiplier2);

                    Main.npc[npcid].life = life;
                    //NEW CODE TO EXPAND THE APOCAYLPSE

                    MonsterWhoAmI.Add(Main.npc[npcid]);
                }
                MonstersLeft = amount;
            }
            else
            {
                Random r = new Random();
                //int amount = r.Next(Wave * 10) + Wave * 10; //Not used
                
                for (int i = 0; i < MonstersLeft; i++)
                {
                    int npcid = NPC.NewNPC((int)(MonsterSpawn.X * 16) - r.Next(-48, 48), (int)MonsterSpawn.Y * 16, CurMonster.type);
                    Main.npc[npcid].SetDefaults(CurMonster.name);

                    //Main.npc[npcid].life = Main.npc[npcid].life * (Wave / 3) + 1; //OLD Calculation of Life
                    //NEW CODE TO EXPAND THE APOCAYLPSE
                    int life = (int)(16 * C3Mod.C3Config.MonsterApocalypseDifficulty * (Wave / 5.0));
                    double multiplier2 = (apocplayers / 2.0);
                    if (multiplier2 < 1.0) multiplier2 = 1.0;
                    life = (int)(life * multiplier2);

                    Main.npc[npcid].life = life;
                    //NEW CODE TO EXPAND THE APOCAYLPSE

                    MonsterWhoAmI.Add(Main.npc[npcid]);
                }
            }
            #endregion
        }

        public static void ChooseNPC()
        {
            var npc = new NPC();
            Random r = new Random();
            int type = r.Next(ApocalypseMonsters.Monsters.Count);
            npc.SetDefaults(ApocalypseMonsters.Monsters[type]);
            CurMonster = npc;
        }

        public static int TpToSpawnPoint()
        {
            int apocplayers = 0;
            for (int i = 0; i < C3Mod.C3Players.Count; i++)
            {
                if (C3Mod.C3Players[i].GameType == "apoc")
                {
                    apocplayers++;
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    if (C3Mod.C3Players[i].tileX != (int)(PlayerSpawn.X) || C3Mod.C3Players[i].tileY != (int)(PlayerSpawn.Y - 3))
                    {
                        TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)PlayerSpawn.X*16, (int)PlayerSpawn.Y*16);
                    }
                }
            }
            if (apocplayers < C3Mod.C3Config.MonsterApocalypseMinimumPlayers)
            {
                C3Tools.BroadcastMessageToGametype("apoc", "Ending game, because not enough players to continue!", Color.DarkCyan);
                Running = false;
                Intermission = false;
                return 0;
            }
            return 1;
        }

        public static void TpToSpawns(bool pvpstate)
        {
            for (int i = 0; i < C3Mod.C3Players.Count; i++)
            {
                if (C3Mod.C3Players[i].GameType == "apoc")
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                }
            }

            Item heart = TShock.Utils.GetItemById(58);
            Item star = TShock.Utils.GetItemById(184);

            foreach (C3Player player in C3Mod.C3Players)
            {
                if (player.GameType == "apoc")
                {
                    player.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                    player.GiveItem(star.type, star.name, star.width, star.height, 20);
                }
            }
        }

        public static void SpawnSet(int posX, int posY)
        {
            PlayerSpawn.X = (posX / 16);
            PlayerSpawn.Y = (posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("SpawnX", (int)(posX / 16)));
            values.Add(new SqlValue("SpawnY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("Apocalypse", values, new List<SqlValue>());
        }

        public static void MonsterSpawnSet(int posX, int posY)
        {
            MonsterSpawn.X = (posX / 16);
            MonsterSpawn.Y = (posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("MonsterSpawnX", (int)(posX / 16)));
            values.Add(new SqlValue("MonsterSpawnY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("Apocalypse", values, new List<SqlValue>());
        }

        public static void SpectatorSpawnSet(int posX, int posY)
        {
            SpectatorArea.X = (posX / 16);
            SpectatorArea.Y = (posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("SpectatorSpawnX", (int)(posX / 16)));
            values.Add(new SqlValue("SpectatorSpawnY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("Apocalypse", values, new List<SqlValue>());
        }
    }
}