using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

using MirageXNA;
using MirageXNA.Network;
using MirageXNA.Globals;

namespace MirageXNA.Core
{
    class clsGlobals
    {

        #region Player Controls
        // Direction constants
        public const byte DIR_NORTH = 0;
        public const byte DIR_SOUTH = 1;
        public const byte DIR_WEST = 2;
        public const byte DIR_EAST = 3;

        // Constants for player movement: Tiles per Second
        public const byte MOVING_WALKING = 1;
        public const byte MOVING_RUNNING = 2;
        #endregion

        public static Player_Struct[] Players;

        public struct Player_Struct
        {
            public string Name;
            public string Password;

            // Paperdoll //
            public int Sprite;

            // Movement //
            public int Map;
            public int X;
            public int Y;
            public int Dir;

            // Stats //
            public int Stat1;
            public int Stat2;
            public int Stat3;
            public int Stat4;

            // Leveling //
            public int Level;
            public int Exp;
            public int Points;

            // Temp - Not Saved //
            public bool InGame;
            public NetIncomingMessage Buffer;
        }

        public const byte MAX_PLAYERS = 100;

        public static void Log(string Msg)
        {
            //frmServer.lstBox.Items[]
        }

        public static bool IsPlaying(Int32 Index)
        {
            //if (Static.connectedClients.IsConnected(Index))
            //{
                if (Players[Index].InGame)
                {
                    return true;
                }
            //}
            return false;
        }

        public static void PlayerWarp(Int32 Index, Int32 mapNum, Int32 X, Int32 Y)
        {
            // Check for subscript out of range
            if ((!IsPlaying(Index)) | (mapNum <= 0) | (mapNum > Constant.MAX_MAPS))
            {
                return;
            }

            // Check if you are out of bounds
            if (X > clsTypes.Map[mapNum].MaxX) { X = clsTypes.Map[mapNum].MaxX; }
            if (Y > clsTypes.Map[mapNum].MaxY) { Y = clsTypes.Map[mapNum].MaxY; }
        
            //TempPlayer(Index).TargetIndex = 0
            //TempPlayer(Index).TargetType = TARGET_TYPE_NONE
            //Call SendTarget(Index)
        
            // Save old map to send erase player data to //
            Int32 OldMap = Players[Index].Map;

            if (OldMap != mapNum)
            {
                //SendLeaveMap(Index, OldMap);
            }

            // Set New Map //
            Players[Index].Map = mapNum;
            Players[Index].X = X;
            Players[Index].Y = Y;

            // send equipment of all people on new map
            //If GetTotalMapPlayers(mapNum) > 0 Then
            //    For i = 1 To Player_HighIndex
            //        If IsPlaying(i) Then
            //            If GetPlayerMap(i) = mapNum Then
            //                Call SendMapEquipmentTo(i, Index)
            //            End If
            //        End If
            //    Next
            //End If

            // Now we check if there were any players left on the map the player just left, and if not stop processing npcs
            //If GetTotalMapPlayers(OldMap) = 0 Then
            //    PlayersOnMap(OldMap) = NO
            //    // Regenerate all NPCs' health
            //    For i = 1 To MAX_MAP_NPCS
            //        If MapNpc(OldMap).Npc(i).Num > 0 Then
            //            MapNpc(OldMap).Npc(i).Vital(Vitals.HP) = GetNpcMaxVital(MapNpc(OldMap).Npc(i).Num, Vitals.HP)
            //        End If
            //    Next
            // End If

            // Sets it so we know to process npcs on the map
            //PlayersOnMap(mapNum) = YES
            //TempPlayer(Index).GettingMap = YES
    
            //Buffer.WriteInteger SCheckForMap
            //Buffer.WriteLong mapNum
            //Buffer.WriteLong Map(mapNum).Revision
            //SendDataTo Index, Buffer.ToArray()
    
        }

        // MAJOR OVERHAUL OF THIS SUB MUCH BETTER THAN BEFORE - MARK MORRIS
        public static void PlayerMove(Int32 Index, int Dir, int Movement)
        {
            int NewX = 0;
            int NewY = 0;
            int NewMapX = 0;
            int NewMapY = 0;

            bool isPlaying = IsPlaying(Index);

            // Check for subscript out of range
            if (isPlaying = false | Dir < DIR_NORTH | Dir > DIR_EAST | Movement < 1 | Movement > 2)
            {
                return;
            }

            Players[Index].Dir = Dir;
            //Moved = NO
            int mapNum = Players[Index].Map;
    
            switch (Dir)
            {
                case DIR_NORTH:
                    NewX = Players[Index].X;
                    NewY = Players[Index].Y - 1;
                    break;
                case DIR_SOUTH:
                    NewX = Players[Index].X;
                    NewY = Players[Index].Y + 1;
                    break;
                case DIR_WEST:
                    NewX = Players[Index].X - 1;
                    NewY = Players[Index].Y;
                    break;
                case DIR_EAST:
                    NewX = Players[Index].X + 1;
                    NewY = Players[Index].Y;
                    break;
            }

            // CHECK BOUNDRIES
            if (Dir == DIR_NORTH)
            {
                if (Players[Index].Y <= 0)
                {
                    if (clsTypes.Map[mapNum].Up > 0)
                    {
                        NewMapY = clsTypes.Map[clsTypes.Map[mapNum].Up].MaxY - 1;
                        General.playerWarp(Index, clsTypes.Map[Commands.getPlayerMap(Index)].Up, Commands.getPlayerX(Index), NewMapY);
                        //PlayerWarp(Index, Map(GetPlayerMap(Index)).Up, GetPlayerX(Index), NewMapY)
                        //Log GetPlayerName(Index) & " Warped to map" & GetPlayerMap(Index), CodeTracker '//\\LOGLINE//\\
                        return;
                    }
                }
            }
    
            if (Dir == DIR_SOUTH)
            {
                if (Players[Index].Y >= clsTypes.Map[mapNum].MaxY)
                {
                    if (clsTypes.Map[mapNum].Down > 0)
                    {
                        NewMapY = 0;
                        General.playerWarp(Index, clsTypes.Map[Commands.getPlayerMap(Index)].Down, Commands.getPlayerX(Index), NewMapY);
                        //PlayerWarp(Index, Map(GetPlayerMap(Index)).Up, GetPlayerX(Index), NewMapY)
                        //Log GetPlayerName(Index) & " Warped to map" & GetPlayerMap(Index), CodeTracker '//\\LOGLINE//\\
                        return;
                    }
                }
            }
    
            if (Dir == DIR_WEST)
            {
                if (Players[Index].X <= 0)
                {
                    if (clsTypes.Map[mapNum].Left > 0)
                    {
                        NewMapX = clsTypes.Map[clsTypes.Map[mapNum].Left].MaxX;
                        General.playerWarp(Index, clsTypes.Map[Commands.getPlayerMap(Index)].Left, NewMapX, Commands.getPlayerY(Index));
                        //Log GetPlayerName(Index) & " Warped to map" & GetPlayerMap(Index), CodeTracker '//\\LOGLINE//\\
                        return;
                    }
                }
            }
    
            if (Dir == DIR_EAST)
            {
                if (Players[Index].X >= clsTypes.Map[mapNum].MaxX)
                {
                    if (clsTypes.Map[mapNum].Right > 0)
                    {
                        NewMapX = 0;
                        General.playerWarp(Index, clsTypes.Map[Commands.getPlayerMap(Index)].Right, NewMapX, Commands.getPlayerY(Index));
                        //Log GetPlayerName(Index) & " Warped to map" & GetPlayerMap(Index), CodeTracker '//\\LOGLINE//\\
                        return;
                    }
                }
            }

            // Move Player On Free Tile //
            Players[Index].X = NewX;
            Players[Index].Y = NewY;
            ServerTCP.SendPlayerMovement(Index, Movement);
        }

    }
}
