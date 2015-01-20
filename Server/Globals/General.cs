using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MirageXNA.Core;
using MirageXNA.Network;

namespace MirageXNA.Globals
{
    class General
    {

        public static void joinGame(Int32 Index)
        {
            clsGlobals.Players[Index].InGame = true;
            //Packets.SendJoinMap(Index);

            // Make Room Here For Player Info //
            // SendInv
            // SendSpells

            // Warp the player to his saved location
            General.playerWarp(Index, Commands.getPlayerMap(Index), Commands.getPlayerX(Index), Commands.getPlayerY(Index));

            // Send Welcome Message //
            ServerTCP.SendMapMsg(Index, clsGlobals.Players[Index].Name + " has joined " + Constant.GAME_NAME);
            Console.WriteLine(clsGlobals.Players[Index].Name + " has enterd the game.");
        }

        public static void leftGame(int Index)
        {
            if (clsGlobals.Players[Index].InGame)
            {
                clsGlobals.Players[Index].InGame = false;

                // Check if player was the only player on the map and stop npc processing if so
                //if GetTotalMapPlayers(clsGlobals.Player[Index].Map) < 1
                //{
                //    PlayersOnMap(clsGlobals.Player[Index].Map) = NO
                //}

                clsDatabase.savePlayer(Index);
                //Call SaveBank(Index)
                //Call ClearBank(Index)

                // Send Welcome Message //
                ServerTCP.SendMapMsg(Index, clsGlobals.Players[Index].Name + " has left " + Constant.GAME_NAME);
                Console.WriteLine(clsGlobals.Players[Index].Name + " has left the game.");
                ServerTCP.SendLeftGame(Index);

                //TotalPlayersOnline = TotalPlayersOnline - 1
            }
            clearPlayer(Index);
        }

        public static void clearPlayer(int Index)
        {
            clsGlobals.Players[Index].Name = "";
            clsGlobals.Players[Index].Sprite = 0;
            clsGlobals.Players[Index].Map = 0;
            clsGlobals.Players[Index].X = 0;
            clsGlobals.Players[Index].Y = 0;
            clsGlobals.Players[Index].Dir = 0;
            clsGlobals.Players[Index].Level = 0;
            clsGlobals.Players[Index].Exp = 0;
            clsGlobals.Players[Index].InGame = false;
            clsGlobals.Players[Index].Buffer = null;
        }

        public static void playerWarp(int Index, int mapNum = 0, int X = 0, int Y = 0)
        {
            int oldMap;

            // Check for subscript out of range
            if ((!clsGlobals.IsPlaying(Index)) | (mapNum <= 0) | (mapNum > Constant.MAX_MAPS))
            {
                return;
            }

            // Check if you are out of bounds
            if (X > clsTypes.Map[mapNum].MaxX) { X = clsTypes.Map[mapNum].MaxX; }
            if (Y > clsTypes.Map[mapNum].MaxY) { Y = clsTypes.Map[mapNum].MaxY; }
        
            // Save old map to send erase player data to
            oldMap = clsGlobals.Players[Index].Map;

            if (oldMap != mapNum)
            {
                //SendLeaveMap(Index, OldMap)
            }

            // Set Player Position //
            clsGlobals.Players[Index].Map = mapNum;
            clsGlobals.Players[Index].X = X;
            clsGlobals.Players[Index].Y = Y;
    
            ServerTCP.SendCheckMap(Index, mapNum, clsTypes.Map[mapNum].Revision);
        }
    }
}
