using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

using MirageXNA.Globals;

namespace MirageXNA.Core
{
    class clsTypes
    {
        public static Map_Struct[] Map = new Map_Struct[Constant.MAX_MAPS + 1];
        public static Cached_Struct[] mapCache;

        public struct Cached_Struct
        {
            public NetOutgoingMessage Data;
        }

        public struct TileData_Struct
        {
            public byte X;
            public byte Y;
            public byte Tileset;
        }

        public struct Tile_Struct
        {
            public TileData_Struct[] Layer;
            public byte[] Autotile;
            public byte Type;
            public Int32 Data1;
            public Int32 Data2;
            public Int32 Data3;
            public byte DirBlock;
        }

        public struct Map_Struct
        {
            public string Name;
            public Int32 Revision;
            public byte Moral;
            public Int32 Tileset;
            public Int16 Up;
            public Int16 Down;
            public Int16 Left;
            public Int16 Right;
            public byte Music;
            public Int16 BootMap;
            public byte BootX;
            public byte BootY;
            public byte MaxX;
            public byte MaxY;
            public Tile_Struct[,] Tile;
            public Int16[] Npc;
            public byte Weather;
            public Int16[,] SoundID;
        }

    }
}
