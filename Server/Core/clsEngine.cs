using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.AccessControl;

using MySql.Data;
using MySql.Data.MySqlClient;

using MirageXNA;
using MirageXNA.Core;
using MirageXNA.Network;

namespace MirageXNA.Core
{
    class clsEngine
    {

        public static void Main(string[] args)
        {
            int Time1 = Environment.TickCount;
            int Time2 = 0;

            // Map Loader Class
            clsDatabase Map = new clsDatabase();

            Console.Title = "RPGCreation.com | Game Server";
            Console.WriteLine("Initisalising Server...");

            Console.WriteLine("Setting Player Arrays...");
            clsGlobals.Players = new clsGlobals.Player_Struct[clsGlobals.MAX_PLAYERS + 1];

            // Init Socket
            Console.WriteLine("Loading Sockets...");
            ServerTCP.initServerTCP();

            // Clear Maps
            Console.WriteLine("Loading Maps...");
            //clsTypes.Init_MapStruct();
            Map.loadMaps();

            Console.WriteLine("Starting Server Loop...");
            Application.Idle += new EventHandler(Serverloop.Loop);

            // Return Time Took To Start Server //
            Time2 = Environment.TickCount;
            Console.WriteLine("Initialization complete. Server loaded in " + (Time2 - Time1) + "ms.");
            Application.Run();
        }

    }
}
