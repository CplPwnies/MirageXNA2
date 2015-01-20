using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MirageXNA;
using MirageXNA.Core;
using MirageXNA.Network;

namespace MirageXNA.Core.Player
{
    public class Combat
    {

        public static bool CanPlayerAttackPlayer(Int32 Attacker, Int32 Victim)
        {
            // Check for subscript out of range
            if (clsGlobals.IsPlaying(Victim)) return false;

            // Make sure they not attacking ourselfs
            if (Attacker == Victim) return false;

            switch (clsGlobals.Players[Attacker].Dir)
            {
                case clsGlobals.DIR_NORTH:
                    if ((clsGlobals.Players[Victim].Y + 1 != clsGlobals.Players[Attacker].Y) & (clsGlobals.Players[Victim].X != clsGlobals.Players[Attacker].Y)) return false;
                    break;
                case clsGlobals.DIR_SOUTH:
                    if ((clsGlobals.Players[Victim].Y - 1 != clsGlobals.Players[Attacker].Y) & (clsGlobals.Players[Victim].X != clsGlobals.Players[Attacker].Y)) return false;
                    break;
                case clsGlobals.DIR_WEST:
                    if ((clsGlobals.Players[Victim].Y != clsGlobals.Players[Attacker].Y) & (clsGlobals.Players[Victim].X + 1 != clsGlobals.Players[Attacker].Y)) return false;
                    break;
                case clsGlobals.DIR_EAST:
                    if ((clsGlobals.Players[Victim].Y != clsGlobals.Players[Attacker].Y) & (clsGlobals.Players[Victim].X - 1 != clsGlobals.Players[Attacker].Y)) return false;
                    break;
            }

            return true;
        }

        // COMBAT SYSTEM
        public static void Combat_PlayerAttackPlayer(Int32 Attacker, Int32 Victim)
        {
            if (CanPlayerAttackPlayer(Attacker,Victim))
            {

            }
        }

    }
}
