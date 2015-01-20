///////////////////////////////////////////////////
////// MIRAGE XNA - DEVELOPED BY MARK MORRIS //////
//////          WWW.RPGCREATION.COM          //////
///////////////////////////////////////////////////

using System;
using Microsoft.Xna.Framework;

namespace MirageXNA.Global
{
    public class Types
    {
        public static Player_Struct[] Players = new Player_Struct[Constant.MAX_PLAYERS + 1];
        public static Map_Struct Map;

        // Tile Cache //
        public static TileLayer_Struct[] TileLayer; //32x32 Cache

        //////////////////////
        // Window Strutures //
        /// //////////////////
        public struct Grh
        {
            public int X;
            public int Y;
            public int W;
            public int H;
            public int sX;
            public int sY;
            public int sW;
            public int sH;
            public int TEXTURE;
            public Grh_Text Text;
        }
        public struct Grh_Button
        {
            public int X;
            public int Y;
            public int W;
            public int H;
            public int TEXTURE;
            public int STATE;
        }
        public struct Grh_Text
        {
            public string CAPTION;
            public int X;
            public int Y;
            public int W;
            public int H;
            public Color COLOUR;
            public int FONT;
        }

        //////////////////////
        // Player Strutures //
        /// //////////////////
        public struct Player_Struct
        {
            // Player Name //
            public string Name;

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

            // Temp - Movement //
            public int OffsetX;
            public int OffsetY;
            public int Moving;
            public int Step;

            // Temp - Attacking //
            public byte Attacking;
            public int AttackSpeed;
            public int AttackTimer;
        }

        ///////////////////
        // Map Strutures //
        /// ///////////////
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

        ///////////////////////////////////////////////////////////////
        // Used to hold the graphic layers in a quick-to-draw format //
        ///////////////////////////////////////////////////////////////
        public struct CachedTile_Struct
        {
            public int dx;
            public int dy;
            public int sX;
            public int sY;
            public int sW;
            public int sH;
            public int PixelPosX;
            public int PixelPosY;
        }

        public struct TileLayer_Struct
        {
            public CachedTile_Struct[] Tile;
            public int NumTiles;
        }

        ////////////////////////////////
        ////// Autotile Structure //////
        ////////////////////////////////
        public struct Destination_Struct
        {
            public int X;
            public int Y;
        }

        public struct QuarterTile_Struct
        {
            public Destination_Struct[] QuarterTile;    //4
            public byte RenderState;
            public int[] sX;    //4
            public int[] sY;    //4
        }

        public struct Autotile_Struct
        {
            public QuarterTile_Struct[] Layer; //5
        }
    }
}
