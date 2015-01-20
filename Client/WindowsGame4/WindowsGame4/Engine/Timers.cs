///////////////////////////////////////////////////
////// MIRAGE XNA - DEVELOPED BY MARK MORRIS //////
//////          WWW.RPGCREATION.COM          //////
///////////////////////////////////////////////////

using System;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using MirageXNA.Global;
using MirageXNA.Network;

namespace MirageXNA.Engine
{
    public sealed class Timers
    {
        //////////
        // Menu //
        //////////

        // Routine Timers //
        private const long UpdateTime_Credits = 50;

        // Update Routines Checks //
        private long LastUpdateTime_Credits;

        //////////
        // Game //
        //////////

        // Routine Timers //
        private const long UpdateTime_GameInput = 50;
        private const long UpdateTime_WalkTimer = 30;
        private const long UpdateTime_Ping = 500;

        // Update Routines Checks //
        private long LastUpdateTime_GameInput;
        private long LastUpdateTime_WalkTimer;
        private long LastUpdateTime_Ping;

        //////////////////////
        // Update Game Loop //
        //////////////////////
        public void updateGameLoop(long tickCount)
        {
            // Check Movement //
            if (LastUpdateTime_GameInput < tickCount)
            {
                Logic.checkMovement();
                LastUpdateTime_GameInput = tickCount + UpdateTime_GameInput;
            }

            // Process Every Players Movement //
            if (LastUpdateTime_WalkTimer < tickCount)
            {
                for (int LoopI = 0; LoopI <= 9; LoopI++)
                {
                    if (ClientTCP.IsPlaying(LoopI)) { Logic.processMovement(LoopI); }
                }
                LastUpdateTime_WalkTimer = (long)tickCount + UpdateTime_WalkTimer;
            }

            // Send Ping Request //
            if (LastUpdateTime_Ping < tickCount)
            {
                ClientTCP.SendGetPing();
                LastUpdateTime_Ping = tickCount + UpdateTime_Ping;
            }

            if (Mirage.client.ConnectionStatus == NetConnectionStatus.Disconnected)
            {
                GC.Collect();   // Collect Garbage
                Static.shutDown = true;    // Exit Game
            }

        }

        //////////////////////
        // Update Menu Loop //
        //////////////////////
        public void updateMenuLoop(long tickCount)
        {

            // Send Ping Request //
            if (LastUpdateTime_Credits < tickCount)
            {
                if (Static.showCredits)
                {
                    Static.creditsAlpha = Static.creditsAlpha + 15;
                    if (Static.creditsAlpha > 255) { Static.creditsAlpha = 255; }
                }
                else
                {
                    Static.creditsAlpha = Static.creditsAlpha - 15;
                    if (Static.creditsAlpha < 0) { Static.creditsAlpha = 0; }
                }
                LastUpdateTime_Credits = tickCount + UpdateTime_Credits;
            }

            // Erase Menu Alert Message //
            if (Static.menuAlertTimer < tickCount)
            {
                Static.menuAlertMessage = String.Empty;
                Static.menuAlertColour = 0;
                Static.menuAlertTimer = 0;
            }

            // Update Parallax //
            if (Mirage.ParallaxX == -800) 
            { Mirage.ParallaxX = 0; } 
            else 
            { Mirage.ParallaxX = Mirage.ParallaxX - 1; }

        }
    }
}
