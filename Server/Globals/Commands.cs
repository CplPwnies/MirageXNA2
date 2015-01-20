using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MirageXNA.Core;

namespace MirageXNA.Globals
{
    public static class Commands
    {
        public static string getPlayerName(int Index)
        {
            return clsGlobals.Players[Index].Name;
        }
        public static int getPlayerSprite(int Index)
        {
            return clsGlobals.Players[Index].Sprite;
        }
        public static int getPlayerMap(int Index)
        {
            return clsGlobals.Players[Index].Map;
        }
        public static int getPlayerX(int Index)
        {
            return clsGlobals.Players[Index].X;
        }
        public static int getPlayerY(int Index)
        {
            return clsGlobals.Players[Index].Y;
        }
    }
}
