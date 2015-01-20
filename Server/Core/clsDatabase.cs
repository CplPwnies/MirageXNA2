using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Security.AccessControl;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using MirageXNA.Globals;
using MirageXNA.Network;
using MirageXNA.Core;
using Lidgren.Network;

namespace MirageXNA.Core
{

    class clsDatabase
    {
        string appPath = Application.StartupPath;

        /////////////////
        // COMPRESSION //
        /////////////////

        public static byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        public static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0) { memory.Write(buffer, 0, count); }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        ////////////
        // PLAYER //
        ////////////

        // Save Player //
        public static void savePlayer(Int32 Index)
        {
            DirectoryInfo dir = new DirectoryInfo("Data\\Accounts\\");
            using (FileStream stream = new FileStream(dir.FullName + clsGlobals.Players[Index].Name.ToLower() + ".bin", FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // Player Information //
                    writer.Write(clsGlobals.Players[Index].Name);
                    writer.Write(clsGlobals.Players[Index].Password);
                    writer.Write(clsGlobals.Players[Index].Sprite);

                    // Player Position // 
                    writer.Write(clsGlobals.Players[Index].Map);
                    writer.Write(clsGlobals.Players[Index].X);
                    writer.Write(clsGlobals.Players[Index].Y);
                    writer.Write(clsGlobals.Players[Index].Dir);

                    // Player Stats //
                    writer.Write(clsGlobals.Players[Index].Stat1);
                    writer.Write(clsGlobals.Players[Index].Stat2);
                    writer.Write(clsGlobals.Players[Index].Stat3);
                    writer.Write(clsGlobals.Players[Index].Stat4);
                    writer.Write(clsGlobals.Players[Index].Level);
                    writer.Write(clsGlobals.Players[Index].Exp);
                    writer.Write(clsGlobals.Players[Index].Points);
                    writer.Close();

                }
            }
        }

        // Load Player //
        public static bool loadPlayer(Int32 Index, String Name)
        {
            DirectoryInfo dir = new DirectoryInfo("Data\\Accounts\\");
            string accountDir = dir.FullName + Name.ToLower() + ".bin";
            if (File.Exists(accountDir))
            {
                using (FileStream stream = new FileStream(accountDir, FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        // Player Information //
                        clsGlobals.Players[Index].Name = reader.ReadString();
                        clsGlobals.Players[Index].Password = reader.ReadString();
                        clsGlobals.Players[Index].Sprite = reader.ReadInt32();

                        // Player Position //
                        clsGlobals.Players[Index].Map = reader.ReadInt32();
                        clsGlobals.Players[Index].X = reader.ReadInt32();
                        clsGlobals.Players[Index].Y = reader.ReadInt32();
                        clsGlobals.Players[Index].Dir = reader.ReadInt32();
                        
                        // Player Stats //
                        clsGlobals.Players[Index].Stat1 = reader.ReadInt32();
                        clsGlobals.Players[Index].Stat2 = reader.ReadInt32();
                        clsGlobals.Players[Index].Stat3 = reader.ReadInt32();
                        clsGlobals.Players[Index].Stat4 = reader.ReadInt32();
                        clsGlobals.Players[Index].Level = reader.ReadInt32();
                        clsGlobals.Players[Index].Exp = reader.ReadInt32();
                        clsGlobals.Players[Index].Points = reader.ReadInt32();
                        reader.Close();
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /////////
        // MAP //
        /////////
        #region Map IO
        public void checkMaps()
        {
            for (int i = 1; i <= Constant.MAX_MAPS; i++)
            {
                DirectoryInfo dir = new DirectoryInfo("data\\Maps\\");
                if (!File.Exists(dir.FullName + i + ".bin"))
                {
                    clearMap(i);
                    saveMap(i);
                }
            }
        }

        public void saveMap(Int32 MapNum)
        {
            int MaxX = clsTypes.Map[MapNum].MaxX;
            int MaxY = clsTypes.Map[MapNum].MaxY;

            DirectoryInfo dir = new DirectoryInfo("Data\\Maps\\");
            using (FileStream stream = new FileStream(dir.FullName + MapNum + ".bin", FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(clsTypes.Map[MapNum].Name);
                    writer.Write(clsTypes.Map[MapNum].Revision);
                    writer.Write(clsTypes.Map[MapNum].Music);
                    writer.Write(clsTypes.Map[MapNum].Moral);
                    writer.Write(clsTypes.Map[MapNum].Weather);
                    writer.Write(clsTypes.Map[MapNum].Tileset);
                    writer.Write(clsTypes.Map[MapNum].Up);
                    writer.Write(clsTypes.Map[MapNum].Down);
                    writer.Write(clsTypes.Map[MapNum].Left);
                    writer.Write(clsTypes.Map[MapNum].Right);
                    writer.Write(clsTypes.Map[MapNum].BootMap);
                    writer.Write(clsTypes.Map[MapNum].BootX);
                    writer.Write(clsTypes.Map[MapNum].BootY);
                    writer.Write(clsTypes.Map[MapNum].MaxX);
                    writer.Write(clsTypes.Map[MapNum].MaxY);

                    for (int X = 0; X <= MaxX - 1; X++)
                    {
                        for (int Y = 0; Y <= MaxY - 1; Y++)
                        {
                            for (int I = 0; I <= 4; I++)
                            {
                                writer.Write(clsTypes.Map[MapNum].Tile[X, Y].Layer[I].Tileset);
                                writer.Write(clsTypes.Map[MapNum].Tile[X, Y].Layer[I].X);
                                writer.Write(clsTypes.Map[MapNum].Tile[X, Y].Layer[I].Y);

                                writer.Write(clsTypes.Map[MapNum].Tile[X, Y].Autotile[I]);

                                writer.Write(clsTypes.Map[MapNum].Tile[X, Y].Type);
                                writer.Write(clsTypes.Map[MapNum].Tile[X, Y].Data1);
                                writer.Write(clsTypes.Map[MapNum].Tile[X, Y].Data2);
                                writer.Write(clsTypes.Map[MapNum].Tile[X, Y].Data3);
                                writer.Write(clsTypes.Map[MapNum].Tile[X, Y].DirBlock);
                            }
                        }
                    }

                    for (int X = 0; X <= 20 - 1; X++)
                    {
                        writer.Write(clsTypes.Map[MapNum].Npc[X]);
                    }

                    for (int X = 0; X <= MaxX - 1; X++)
                    {
                        for (int Y = 0; Y <= MaxY - 1; Y++)
                        {
                            writer.Write(clsTypes.Map[MapNum].SoundID[X, Y]);
                        }
                    }

                    writer.Close();

                }
            }
        }

        public void loadMap(Int32 MapNum)
        {

            DirectoryInfo dir = new DirectoryInfo("Data\\Maps\\");
            using (FileStream stream = new FileStream(dir.FullName +  MapNum + ".bin", FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    clsTypes.Map[MapNum].Name = reader.ReadString();
                    clsTypes.Map[MapNum].Revision = reader.ReadInt32();
                    clsTypes.Map[MapNum].Music = reader.ReadByte();
                    clsTypes.Map[MapNum].Moral = reader.ReadByte();
                    clsTypes.Map[MapNum].Weather = reader.ReadByte();
                    clsTypes.Map[MapNum].Tileset = reader.ReadInt32();
                    clsTypes.Map[MapNum].Up = reader.ReadInt16();
                    clsTypes.Map[MapNum].Down = reader.ReadInt16();
                    clsTypes.Map[MapNum].Left = reader.ReadInt16();
                    clsTypes.Map[MapNum].Right = reader.ReadInt16();
                    clsTypes.Map[MapNum].BootMap = reader.ReadInt16();
                    clsTypes.Map[MapNum].BootX = reader.ReadByte();
                    clsTypes.Map[MapNum].BootY = reader.ReadByte();
                    clsTypes.Map[MapNum].MaxX = reader.ReadByte();
                    clsTypes.Map[MapNum].MaxY = reader.ReadByte();

                    byte MaxX = clsTypes.Map[MapNum].MaxX;
                    byte MaxY = clsTypes.Map[MapNum].MaxY;

                    resizeMap(MapNum, MaxX, MaxY);

                    for (int X = 0; X <= MaxX - 1; X++)
                    {
                        for (int Y = 0; Y <= MaxY - 1; Y++)
                        {
                            for (int I = 0; I <= 4; I++)
                            {
                                clsTypes.Map[MapNum].Tile[X, Y].Layer[I].Tileset = reader.ReadByte();
                                clsTypes.Map[MapNum].Tile[X, Y].Layer[I].X = reader.ReadByte();
                                clsTypes.Map[MapNum].Tile[X, Y].Layer[I].Y = reader.ReadByte();
                                clsTypes.Map[MapNum].Tile[X, Y].Autotile[I] = reader.ReadByte();
                                clsTypes.Map[MapNum].Tile[X, Y].Type = reader.ReadByte();
                                clsTypes.Map[MapNum].Tile[X, Y].Data1 = reader.ReadInt32();
                                clsTypes.Map[MapNum].Tile[X, Y].Data2 = reader.ReadInt32();
                                clsTypes.Map[MapNum].Tile[X, Y].Data3 = reader.ReadInt32();
                                clsTypes.Map[MapNum].Tile[X, Y].DirBlock = reader.ReadByte();
                            }
                        }
                    }

                    for (int X = 0; X <= 19; X++)
                    {
                        clsTypes.Map[MapNum].Npc[X] = reader.ReadInt16();
                    }

                    for (int X = 0; X <= MaxX - 1; X++)
                    {
                        for (int Y = 0; Y <= MaxY - 1; Y++)
                        {
                            clsTypes.Map[MapNum].SoundID[X, Y] = reader.ReadInt16();
                        }
                    }

                    reader.Close();
                }
            }
        }

        public void resizeMap(Int32 MapNum, byte MaxX, byte MaxY)
        {
            clsTypes.Map[MapNum].Tileset = 1;
            clsTypes.Map[MapNum].Name = "Map";
            clsTypes.Map[MapNum].MaxX = MaxX;
            clsTypes.Map[MapNum].MaxY = MaxY;

            // Restructure the Array!
            clsTypes.Map[MapNum].Tile = new clsTypes.Tile_Struct[clsTypes.Map[MapNum].MaxX, clsTypes.Map[MapNum].MaxY];
            clsTypes.Map[MapNum].SoundID = new Int16[clsTypes.Map[MapNum].MaxX, clsTypes.Map[MapNum].MaxY];
            clsTypes.Map[MapNum].Npc = new Int16[20];

            // Resize all Layers
            for (int x = 0; x <= clsTypes.Map[MapNum].MaxX - 1; x++)
            {
                for (int y = 0; y <= clsTypes.Map[MapNum].MaxY - 1; y++)
                {
                    Array.Resize<clsTypes.TileData_Struct>(ref clsTypes.Map[MapNum].Tile[x, y].Layer, 5);
                }
            }

            // Resize all Layers
            for (int x = 0; x <= clsTypes.Map[MapNum].MaxX - 1; x++)
            {
                for (int y = 0; y <= clsTypes.Map[MapNum].MaxY - 1; y++)
                {
                    Array.Resize<byte>(ref clsTypes.Map[MapNum].Tile[x, y].Autotile, 5);
                }
            }

            Array.Clear(clsTypes.Map[MapNum].SoundID, 0, clsTypes.Map[MapNum].SoundID.Length);

            // Clear Map Cache //
            clsTypes.mapCache[MapNum].Data = null;
        }

        public void clearMap(Int32 MapNum)
        {
            clsTypes.Map[MapNum].Tileset = 1;
            clsTypes.Map[MapNum].Name = "Map";
            clsTypes.Map[MapNum].MaxX = 50;
            clsTypes.Map[MapNum].MaxY = 50;

            // Restructure the Array!
            clsTypes.Map[MapNum].Tile = new clsTypes.Tile_Struct[clsTypes.Map[MapNum].MaxX, clsTypes.Map[MapNum].MaxY];
            clsTypes.Map[MapNum].SoundID = new Int16[clsTypes.Map[MapNum].MaxX, clsTypes.Map[MapNum].MaxY];
            clsTypes.Map[MapNum].Npc = new Int16[20];

            // Resize all Layers
            for (int x = 0; x <= clsTypes.Map[MapNum].MaxX - 1; x++)
            {
                for (int y = 0; y <= clsTypes.Map[MapNum].MaxY - 1; y++)
                {
                    Array.Resize<clsTypes.TileData_Struct>(ref clsTypes.Map[MapNum].Tile[x, y].Layer, 5);
                }
            }

            // Resize all Layers
            for (int x = 0; x <= clsTypes.Map[MapNum].MaxX - 1; x++)
            {
                for (int y = 0; y <= clsTypes.Map[MapNum].MaxY - 1; y++)
                {
                    Array.Resize<byte>(ref clsTypes.Map[MapNum].Tile[x, y].Autotile, 5);
                }
            }

            Array.Clear(clsTypes.Map[MapNum].SoundID, 0, clsTypes.Map[MapNum].SoundID.Length);

            // Clear Map Cache //
            clsTypes.mapCache[MapNum].Data = null;
        }

        public void loadMaps()
        {
            // Load Map Cache //
            clsTypes.mapCache = new clsTypes.Cached_Struct[Constant.MAX_MAPS + 1];

            checkMaps();

            for (int LoopI = 1; LoopI <= Constant.MAX_MAPS; LoopI++)
            {
                loadMap(LoopI);
                Application.DoEvents();
            }

            for (int LoopI = 1; LoopI <= Constant.MAX_MAPS; LoopI++)
            {
                cacheMap(LoopI);
                Application.DoEvents();
            }

            // Cache All Maps Loaded //

        }

        public void cacheMap(Int32 mapNum)
        {
            NetOutgoingMessage TempBuffer = ServerTCP.sSock.CreateMessage();

            int X; int Y;
            int MaxX = clsTypes.Map[mapNum].MaxX;
            int MaxY = clsTypes.Map[mapNum].MaxY;

            // ****** PreAllocate Buffer ******
            int mapSize = Marshal.SizeOf(clsTypes.Map[mapNum]);
            int tileSize = Marshal.SizeOf(clsTypes.Map[mapNum].Tile[0,0]);
            int nLength = mapSize + ((tileSize * MaxX) * MaxY);

            // Must Preallocate //
            TempBuffer.Write(mapNum);

            // ****** Map Info ******
            TempBuffer.Write(clsTypes.Map[mapNum].Name);
            TempBuffer.Write(clsTypes.Map[mapNum].Music);
            TempBuffer.Write(clsTypes.Map[mapNum].Revision);
            TempBuffer.Write(clsTypes.Map[mapNum].Moral);
            TempBuffer.Write(clsTypes.Map[mapNum].Weather);
            TempBuffer.Write(clsTypes.Map[mapNum].Tileset);
            TempBuffer.Write(clsTypes.Map[mapNum].Up);
            TempBuffer.Write(clsTypes.Map[mapNum].Down);
            TempBuffer.Write(clsTypes.Map[mapNum].Left);
            TempBuffer.Write(clsTypes.Map[mapNum].Right);
            TempBuffer.Write(clsTypes.Map[mapNum].BootMap);
            TempBuffer.Write(clsTypes.Map[mapNum].BootX);
            TempBuffer.Write(clsTypes.Map[mapNum].BootY);
            TempBuffer.Write(clsTypes.Map[mapNum].MaxX);
            TempBuffer.Write(clsTypes.Map[mapNum].MaxY);

            // Faster Maybe? //
            clsTypes.Tile_Struct[,] tempTile = clsTypes.Map[mapNum].Tile;

            // ****** Tiles ******
            for (X = 0; X <= MaxX - 1; X++)
            {
                for (Y = 0; Y <= MaxY - 1; Y++)
                {
                    for (int I = 0; I <= 4; I++)
                    {
                        TempBuffer.Write(tempTile[X, Y].Layer[I].X);
                        TempBuffer.Write(tempTile[X, Y].Layer[I].Y);
                        TempBuffer.Write(tempTile[X, Y].Layer[I].Tileset);
                        TempBuffer.Write(tempTile[X, Y].Autotile[I]);
                    }
                    TempBuffer.Write(tempTile[X, Y].Type);
                    TempBuffer.Write(tempTile[X, Y].Data1);
                    TempBuffer.Write(tempTile[X, Y].Data2);
                    TempBuffer.Write(tempTile[X, Y].Data3);
                    TempBuffer.Write(tempTile[X, Y].DirBlock);
                    Application.DoEvents();
                }
            }

            // ****** Map Npc's ******
            for (X = 0; X <= 19; X++)
            {
                TempBuffer.Write(clsTypes.Map[mapNum].Npc[X]);
            }

            // ****** Send Map SoundID ******
            for (X = 0; X <= MaxX - 1; X++)
            {
                for (Y = 0; Y <= MaxY - 1; Y++)
                {
                    TempBuffer.Write(clsTypes.Map[mapNum].SoundID[X, Y]);
                }
            }
    
            // ****** Compress Using 7Zip.dll ******
            //TempBuffer.CompressBuffer();

            clsTypes.mapCache[mapNum].Data = TempBuffer;
        }


        #endregion
    }

}
