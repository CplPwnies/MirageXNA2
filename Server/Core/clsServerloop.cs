using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

using MirageXNA.Network;
using MirageXNA.Globals;

namespace MirageXNA.Core
{
    class Serverloop
    {

        public static void Loop(object sender, EventArgs e)
        {
            bool ServerRunning = true;

            Console.WriteLine("Loop Started... Setup Complete.");
            while (ServerRunning)
            {
                // Incoming Data //
                HandleTCP.ReceivedData();

                // Let Windows Process Other Events //
                Application.DoEvents();
            }

        }

    }
}
