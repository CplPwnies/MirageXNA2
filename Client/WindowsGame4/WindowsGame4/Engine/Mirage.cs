///////////////////////////////////////////////////
////// MIRAGE XNA - DEVELOPED BY MARK MORRIS //////
//////          WWW.RPGCREATION.COM          //////
///////////////////////////////////////////////////

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Lidgren.Network;

using MirageXNA;
using MirageXNA.Global;
using MirageXNA.Network;

namespace MirageXNA.Engine
{
    public struct Texture2D_struct
    {
        public Texture2D Texture;
        public int Width;
        public int Height;
        public string Path;
        public long UnloadTimer;
        public bool Loaded;
    }

    // XNA Window //
    public class Mirage : Microsoft.Xna.Framework.Game
    {
        public static GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Timers Time;
        private InputState inputState;
        public static SoundEffect soundEffect;
        public static NetClient client;

        // Routine Timers //
        private const long UpdateTime_Input = 60;
        private long LastUpdateTime_Input;

        
        public static int ParallaxX;

        public int mTextureNum;
        public int CurrentTexture;
        public Texture2D_struct[] TEXTURE;
        public static SpriteFont[] FONT_TEXTURE;

        // Game Time //
        public static GameTime _gameTime;

        public Mirage()
        {
            // Init Window //
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = false;
            graphics.PreferMultiSampling = true;

            // Set Content Directory //
            Content.RootDirectory = "Content";

            // Networking //
            NetPeerConfiguration config = new NetPeerConfiguration("MirageXNA");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.ConnectionTimeout = 5;
            client = new NetClient(config);
            client.Start();
            client.Socket.Blocking = false; // If a method is called it wont block

            client.DiscoverKnownPeer("127.0.0.1", 7001);
            //client.DiscoverKnownPeer("82.35.224.177", 7001);

            // Init //
            initTextures();
            initSounds();
        }

        protected override void Initialize()
        {
            // ###########
            // ## FONTS ##
            // ###########

            // Load Fonts //
            FONT_TEXTURE = new SpriteFont[2];
            FONT_TEXTURE[0] = Content.Load<SpriteFont>("Fonts\\1");
            FONT_TEXTURE[1] = Content.Load<SpriteFont>("Fonts\\2");

            // Static Class Init //
            UI_Menu.initGUI();              // Game Graphical User Interface

            // Private Class Init //
            inputState = new InputState();  // Game Input
            Time = new Timers();            // Game Timers

            // Local Class //
            // Sock socket = new Sock();       // Game Socket Connection

            // Force Tileset Update //
            Static.LastTileX = -131313;        // Force Map Update //
            Static.LastTileY = -131313;        // Force Map Update //

            // This will show the mouse //
            this.IsMouseVisible = true;     // Show Mouse - Temp (Render) //

            // Init Game //
            base.Initialize();              
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            // Unload Textures //
            TEXTURE = null;
            FONT_TEXTURE = null;

            // Unload Sounds //
            
        }

        protected override void Update(GameTime gameTime)
        {
            int Tick = (int)gameTime.TotalGameTime.TotalMilliseconds;
            _gameTime = gameTime;

            // Start Shutdown //
            if (Static.shutDown) { GC.Collect(); this.Exit(); }

            // Check Input //
            if (LastUpdateTime_Input < Tick)
            {
                if (inputState.justPressed(Keys.Escape)) this.Exit();
                inputState.Update();
                LastUpdateTime_Input = Tick + UpdateTime_Input;
            }

            // Update Logic //
            switch (Static.menuType)
            {
                case Constant.inMenu: Time.updateMenuLoop(Tick); break;
                case Constant.inGame: Time.updateGameLoop(Tick); break;
            }

            // READ INCOMING MESSAGE //
            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse: client.Connect(msg.SenderEndpoint); break;
                    case NetIncomingMessageType.Data: HandleTCP.HandleData(msg); break;
                }
                client.Recycle(msg);
            }

            General.calculateFPS(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Rectangle Offset;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            switch (Static.menuType)
            {

                // Render Menu //
                case Constant.inMenu:

                    // Render Menu //
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);

                    // ****** Parallax ****** // //Replace 0 with ParallaxX got effect
                    renderRect(Static.Tex_Gui[2], 0, 0, 768, 800, 0, 0, 768, 800, Color.White);
                    renderRect(Static.Tex_Gui[2], 0 + 800, 0, 768, 800, 0, 0, 768, 800, Color.White);

                    renderMenu();

                    renderRect(Static.Tex_Gui[9], 1024 - 300, 768 - 124, 114, 300, 0, 0, 229, 600, Color.White);

                    Vector2 Pos = new Vector2(10, 10);
                    // Draw the string
                    spriteBatch.DrawString(FONT_TEXTURE[0], "X: " + InputState.curMouse.X, Pos, Color.Black);

                    Pos = new Vector2(10, 30);
                    // Draw the string
                    spriteBatch.DrawString(FONT_TEXTURE[0], "Y: " + InputState.curMouse.Y, Pos, Color.Black);

                    // Render Alert Message //
                    if (Static.menuAlertTimer > 0) 
                    {
                        renderRect(Static.Tex_Gui[2], 0, 0, 768, 1024, 0, 0, 768, 1024, new Color(0, 0, 0, 100));
                        Pos = new Vector2(512 - (General.getStringWidth(0, Static.menuAlertMessage) / 2), 200);
                        spriteBatch.DrawString(FONT_TEXTURE[0], Static.menuAlertMessage, Pos, Color.Black);
                    }

                    spriteBatch.End();
                    break;

                // Render In Game //
                case Constant.inGame:

                    // Scrolling Map Algorithm //
                    Offset = nextFrame();

                    // Draw the sprite
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);

                    // RENDER LOWER MAP LAYER //
                    for (int LoopL = 0; LoopL <= 2; LoopL++)
                    {
                        for (int LoopT = 1; LoopT <= Types.TileLayer[LoopL].NumTiles; LoopT++)
                        {
                            int Text = LoopT - 1;

                            if (Autotiles.Autotile[Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy].Layer[LoopL].RenderState == Autotiles.RENDER_STATE_NORMAL)
                            {
                                renderRect
                                (Static.Tex_Tilesets[Types.Map.Tile[Types.TileLayer[LoopL].Tile[Text].dx,
                                    Types.TileLayer[LoopL].Tile[Text].dy].Layer[LoopL].Tileset],
                                    Types.TileLayer[LoopL].Tile[Text].PixelPosX - (Offset.X),
                                    Types.TileLayer[LoopL].Tile[Text].PixelPosY - (Offset.Y),
                                    32,
                                    32,
                                    Types.TileLayer[LoopL].Tile[Text].sX,
                                    Types.TileLayer[LoopL].Tile[Text].sY,
                                    32,
                                    32, 
                                    Color.White);
                            }
                            else if (Autotiles.Autotile[Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy].Layer[LoopL].RenderState == Autotiles.RENDER_STATE_AUTOTILE)
                            {
                                renderAutotile(LoopL, Types.TileLayer[LoopL].Tile[Text].PixelPosX - (Offset.X), Types.TileLayer[LoopL].Tile[Text].PixelPosY - (Offset.Y), 1, Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy);
                                renderAutotile(LoopL, Types.TileLayer[LoopL].Tile[Text].PixelPosX - (Offset.X) + 16, Types.TileLayer[LoopL].Tile[Text].PixelPosY - (Offset.Y), 2, Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy);
                                renderAutotile(LoopL, Types.TileLayer[LoopL].Tile[Text].PixelPosX - (Offset.X), Types.TileLayer[LoopL].Tile[Text].PixelPosY - (Offset.Y) + 16, 3, Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy);
                                renderAutotile(LoopL, Types.TileLayer[LoopL].Tile[Text].PixelPosX - (Offset.X) + 16, Types.TileLayer[LoopL].Tile[Text].PixelPosY - (Offset.Y) + 16, 4, Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy);
                            }
                        }
                    }

                    // RENDER PLAYERS //
                    for (int LoopI = 0; LoopI <= Constant.MAX_PLAYERS - 1; LoopI++)
                    {
                        if (ClientTCP.IsPlaying(LoopI))
                        {
                            if (Types.Players[Static.MyIndex].Map == Types.Players[LoopI].Map)
                            {
                                int PlayerX = ((Types.Players[LoopI].X * 32) + Types.Players[LoopI].OffsetX) - Offset.X;
                                int PlayerY = ((Types.Players[LoopI].Y * 32) + Types.Players[LoopI].OffsetY) - Offset.Y;
                                renderPlayer(LoopI, PlayerX, PlayerY);
                            }
                        }
                    }


                    // RENDER HIGHER MAP LAYER //
                    for (int LoopL = 3; LoopL <= 4; LoopL++)
                    {
                        for (int LoopT = 1; LoopT <= Types.TileLayer[LoopL].NumTiles; LoopT++)
                        {
                            int Text = LoopT - 1;

                            if (Autotiles.Autotile[Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy].Layer[LoopL].RenderState == Autotiles.RENDER_STATE_NORMAL)
                            {
                                renderRect
                                (Static.Tex_Tilesets[Types.Map.Tile[Types.TileLayer[LoopL].Tile[Text].dx,
                                    Types.TileLayer[LoopL].Tile[Text].dy].Layer[LoopL].Tileset],
                                    Types.TileLayer[LoopL].Tile[Text].PixelPosX - (Offset.X),
                                    Types.TileLayer[LoopL].Tile[Text].PixelPosY - (Offset.Y),
                                    32,
                                    32,
                                    Types.TileLayer[LoopL].Tile[Text].sX,
                                    Types.TileLayer[LoopL].Tile[Text].sY,
                                    32,
                                    32,
                                    Color.White);
                            }
                            else if (Autotiles.Autotile[Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy].Layer[LoopL].RenderState == Autotiles.RENDER_STATE_AUTOTILE)
                            {
                                renderAutotile(LoopL, Types.TileLayer[LoopL].Tile[Text].PixelPosX - (Offset.X), Types.TileLayer[LoopL].Tile[Text].PixelPosY - (Offset.Y), 1, Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy);
                                renderAutotile(LoopL, Types.TileLayer[LoopL].Tile[Text].PixelPosX - (Offset.X) + 16, Types.TileLayer[LoopL].Tile[Text].PixelPosY - (Offset.Y), 2, Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy);
                                renderAutotile(LoopL, Types.TileLayer[LoopL].Tile[Text].PixelPosX - (Offset.X), Types.TileLayer[LoopL].Tile[Text].PixelPosY - (Offset.Y) + 16, 3, Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy);
                                renderAutotile(LoopL, Types.TileLayer[LoopL].Tile[Text].PixelPosX - (Offset.X) + 16, Types.TileLayer[LoopL].Tile[Text].PixelPosY - (Offset.Y) + 16, 4, Types.TileLayer[LoopL].Tile[Text].dx, Types.TileLayer[LoopL].Tile[Text].dy);
                            }
                        }
                    }

                    string PingToDraw;

                    switch (Static.Ping)
                    {
                        case -1:
                            PingToDraw = "Latency: " + "Syncing";
                            break;
                        case 0:
                            PingToDraw = "Latency: " + "Local";
                            break;
                        default: 
                            PingToDraw = "Latency: " + Static.Ping;
                            break;
                    }

                    // Render Windows //
                    renderGameWindows(Constant.inventoryWindow);
                    renderGameWindows(Constant.spellWindow);
                    renderGameWindows(Constant.chatWindow);
                    renderGameWindows(Constant.charWindow);
                    renderGameWindows(Constant.menuWindow);

                    // RENDER PING //
                    Pos = new Vector2(10, 10);
                    spriteBatch.DrawString(FONT_TEXTURE[0], PingToDraw, Pos, Color.Black);

                    spriteBatch.End();

                break;
            }

            Window.Title = Constant.GAME_NAME + " FPS: " + Static.fps;

            base.Draw(gameTime);
        }

        //////////////////
        // SOUND ENGINE //
        //////////////////

        // Load All Sound Effects //
        private void initSounds()
        {
            // will make it unload when not used //
            Audio._mHover = Content.Load<SoundEffect>("Sounds\\1");
            //Audio._mClick = Content.Load<SoundEffect>("2");
        }

        ///////////////////
        // RENDER ENGINE //
        ///////////////////

        private void initTextures()
        {
            TEXTURE = new Texture2D_struct[1];
            string appPath = Content.RootDirectory;

            // ######################
            // ## Tileset Textures ##
            // ######################
            // Loop through folder scanning for textures to load
            string currentFolder = "Tiles\\";
            DirectoryInfo dir = new DirectoryInfo(Content.RootDirectory + "\\" + currentFolder);
            while (File.Exists(dir.FullName + Static.Tilesets_Count + ".xnb") == true)
            {
                Array.Resize<int>(ref Static.Tex_Tilesets, Static.Tilesets_Count + 1);
                Static.Tex_Tilesets[Static.Tilesets_Count] = setTexturePath(currentFolder + Static.Tilesets_Count);
                Static.Tilesets_Count++;
            }
            // decrement int due to 1 too many calls to the increment
            --Static.Tilesets_Count;

            // #####################
            // ## Sprite Textures ##
            // #####################
            // Loop through folder scanning for textures to load
            currentFolder = "Sprites\\";
            dir = new DirectoryInfo(Content.RootDirectory + "\\" + currentFolder);
            while (File.Exists(dir.FullName + Static.Sprites_Count + ".xnb") == true)
            {
                Array.Resize<int>(ref Static.Tex_Sprites, Static.Sprites_Count + 1);
                Static.Tex_Sprites[Static.Sprites_Count] = setTexturePath(currentFolder + Static.Sprites_Count);
                Static.Sprites_Count++;
            }
            // decrement int due to 1 too many calls to the increment
            --Static.Sprites_Count;

            // ####################
            // ## Faces Textures ##
            // ####################
            // Loop through folder scanning for textures to load
            currentFolder = "Faces\\";
            dir = new DirectoryInfo(Content.RootDirectory + "\\" + currentFolder);
            while (File.Exists(dir.FullName + Static.Faces_Count + ".xnb") == true)
            {
                Array.Resize<int>(ref Static.Tex_Faces, Static.Faces_Count + 1);
                Static.Tex_Faces[Static.Faces_Count] = setTexturePath(currentFolder + Static.Faces_Count);
                Static.Faces_Count++;
            }
            // decrement int due to 1 too many calls to the increment
            --Static.Faces_Count;

            // ###################
            // ## Hair Textures ##
            // ###################
            // Loop through folder scanning for textures to load
            currentFolder = "Hairs\\";
            dir = new DirectoryInfo(Content.RootDirectory + "\\" + currentFolder);
            while (File.Exists(dir.FullName + Static.Hairs_Count + ".xnb") == true)
            {
                Array.Resize<int>(ref Static.Tex_Hairs, Static.Hairs_Count + 1);
                Static.Tex_Hairs[Static.Hairs_Count] = setTexturePath(currentFolder + Static.Hairs_Count);
                Static.Hairs_Count++;
            }
            // decrement int due to 1 too many calls to the increment
            --Static.Hairs_Count;

            // #######################
            // ## Clothing Textures ##
            // #######################
            // Loop through folder scanning for textures to load
            currentFolder = "Clothing\\";
            dir = new DirectoryInfo(Content.RootDirectory + "\\" + currentFolder);
            while (File.Exists(dir.FullName + Static.Clothing_Count + ".xnb") == true)
            {
                Array.Resize<int>(ref Static.Tex_Clothing, Static.Clothing_Count + 1);
                Static.Tex_Clothing[Static.Clothing_Count] = setTexturePath(currentFolder + Static.Clothing_Count);
                Static.Clothing_Count++;
            }
            // decrement int due to 1 too many calls to the increment
            --Static.Clothing_Count;

            // ##################
            // ## Gui Textures ##
            // ##################

            // Loop through folder scanning for textures to load
            currentFolder = "GUI\\";
            dir = new DirectoryInfo(Content.RootDirectory + "\\" + currentFolder);
            while (File.Exists(dir.FullName + Static.Gui_Count + ".xnb") == true)
            {
                Array.Resize<int>(ref Static.Tex_Gui, Static.Gui_Count + 1);
                Static.Tex_Gui[Static.Gui_Count] = setTexturePath(currentFolder + Static.Gui_Count);
                Static.Gui_Count++;
            }
            // decrement int due to 1 too many calls to the increment
            --Static.Gui_Count;

            // #####################
            // ## Button Textures ##
            // #####################

            // Loop through folder scanning for textures to load
            currentFolder = "Buttons\\";
            dir = new DirectoryInfo(Content.RootDirectory + "\\" + currentFolder);
            while (File.Exists(dir.FullName + Static.Buttons_Count + ".xnb") == true)
            {
                Array.Resize<int>(ref Static.Tex_Buttons, Static.Buttons_Count + 1);
                Static.Tex_Buttons[Static.Buttons_Count] = setTexturePath(currentFolder + Static.Buttons_Count);
                Static.Buttons_Count++;
            }
            // decrement int due to 1 too many calls to the increment
            --Static.Buttons_Count;

            // ###########################
            // ## Button Click Textures ##
            // ###########################

            // Loop through folder scanning for textures to load
            currentFolder = "Buttons\\";
            dir = new DirectoryInfo(Content.RootDirectory + "\\" + currentFolder);
            while (File.Exists(dir.FullName + Static.Buttons_C_Count + "_C.xnb") == true)
            {
                Array.Resize<int>(ref Static.Tex_Buttons_C, Static.Buttons_C_Count + 1);
                Static.Tex_Buttons_C[Static.Buttons_C_Count] = setTexturePath(currentFolder + Static.Buttons_C_Count + "_C");
                Static.Buttons_C_Count++;
            }
            // decrement int due to 1 too many calls to the increment
            --Static.Buttons_C_Count;

            // ###########################
            // ## Button Hover Textures ##
            // ###########################

            // Loop through folder scanning for textures to load
            currentFolder = "Buttons\\";
            dir = new DirectoryInfo(Content.RootDirectory + "\\" + currentFolder);
            while (File.Exists(dir.FullName + Static.Buttons_H_Count + "_H.xnb") == true)
            {
                Array.Resize<int>(ref Static.Tex_Buttons_H, Static.Buttons_H_Count + 1);
                Static.Tex_Buttons_H[Static.Buttons_H_Count] = setTexturePath(currentFolder + Static.Buttons_H_Count + "_H");
                Static.Buttons_H_Count++;
            }
            // decrement int due to 1 too many calls to the increment
            --Static.Buttons_H_Count;
        }

        private int setTexturePath(string path)
        {
            mTextureNum = mTextureNum + 1;
            Array.Resize<Texture2D_struct>(ref TEXTURE, mTextureNum + 1);
            TEXTURE[mTextureNum].Path = path;
            TEXTURE[mTextureNum].Loaded = false;
            return mTextureNum;
        }

        private void loadTexture(long TextureNum)
        {
            
            TEXTURE[TextureNum].Texture = Content.Load<Texture2D>(TEXTURE[TextureNum].Path);
            TEXTURE[TextureNum].Height = TEXTURE[TextureNum].Width;
            TEXTURE[TextureNum].Width = TEXTURE[TextureNum].Height;
            TEXTURE[TextureNum].UnloadTimer = System.Environment.TickCount;
            TEXTURE[TextureNum].Loaded = true;
        }

        public void checkTexture(int TextureNumber)
        {
            if (TextureNumber != CurrentTexture)
            {
                if (TextureNumber > mTextureNum) { TextureNumber = mTextureNum; }
                if (TextureNumber < 0) { TextureNumber = 0; }

                // ###### Check if texture is loaded ######
                if (TextureNumber != -1)
                {
                    if (TEXTURE[TextureNumber].Loaded == false)
                    {
                        loadTexture(TextureNumber);
                    }
                }
                CurrentTexture = TextureNumber;
            }
        }

        private void renderRect(int TextureNum, int X, int Y, int H, int W, int sX, int sY, int sH, int sW, Color Colour)
        {
            Rectangle _dest = new Rectangle(X, Y, W, H);
            Rectangle _source = new Rectangle(sX, sY, sW, sH);

            // Faster Here //
            if (TextureNum == 0) { return; }

            // Check if we need to load texture //
            checkTexture(TextureNum);

            // Draw //
            spriteBatch.Draw(TEXTURE[TextureNum].Texture, _dest, _source, Colour);

            // Set Timer //
            TEXTURE[TextureNum].UnloadTimer = System.Environment.TickCount;
        }

        private void renderAutotile(int LayerNum, int dx, int dy, int quarterNum, int X, int Y)
        {
            int Yoffset = 0; int Xoffset = 0;

            // calculate the offset
            switch (Types.Map.Tile[X, Y].Autotile[LayerNum])
            {
                //case Constant.AUTOTILE_WATERFALL: Yoffset = (WaterfallAnim - 1) * 32;
                //case Constant.AUTOTILE_ANIM: Xoffset = AutoTileAnim * 64;
                case Constant.AUTOTILE_CLIFF: Yoffset = -32; break;
            }

            // Draw the quarter
            renderRect(Static.Tex_Tilesets[Types.Map.Tile[X, Y].Layer[LayerNum].Tileset], dx, dy, 16, 16, Autotiles.Autotile[X, Y].Layer[LayerNum].sX[quarterNum] + Xoffset, Autotiles.Autotile[X, Y].Layer[LayerNum].sY[quarterNum] + Yoffset, 16, 16, Color.White);
        }

        private void renderPlayer(Int32 Index, int dx, int dy)
        {
            int Anim = 1; int sX; int sY;
            int spriteTop = 0;

            // Walking Animation //
            switch (Types.Players[Index].Dir)
            {
                case Constant.DIR_NORTH: if (Types.Players[Index].OffsetY > 8) { Anim = Types.Players[Index].Step; } spriteTop = 3; break;
                case Constant.DIR_SOUTH: if (Types.Players[Index].OffsetY < -8) { Anim = Types.Players[Index].Step; } spriteTop = 0; break;
                case Constant.DIR_WEST: if (Types.Players[Index].OffsetX > 8) { Anim = Types.Players[Index].Step; } spriteTop = 1; break;
                case Constant.DIR_EAST: if (Types.Players[Index].OffsetX < -8) { Anim = Types.Players[Index].Step; } spriteTop = 2; break;
            }

            // Get The Source RECT //
            sX = Anim * 32;
            sY = spriteTop * 32;

            // Render Paperdoll //
            renderSprite(Types.Players[Index].Sprite, dx, dy, sX, sY);
            //renderHairStyle(Types.Players[Index].Hair, dx, dy, sX, sY);
            //renderClothing(Types.Players[Index].Clothing, dx, dy, sX, sY);
        }

        private void renderSprite(int spriteNum, int dx, int dy, int sX, int sY)
        {
            if (spriteNum < 1 | spriteNum > Static.Sprites_Count) return;
            renderRect(Static.Tex_Sprites[spriteNum], dx, dy, Constant.playerY, Constant.playerX, sX, sY, Constant.playerY, Constant.playerX, Color.White);
        }

        private void renderHairStyle(int spriteNum, int dx, int dy, int sX, int sY)
        {
            if (spriteNum < 1 | spriteNum > Static.Hairs_Count) return;
            renderRect(Static.Tex_Hairs[spriteNum], dx, dy, Constant.playerY, Constant.playerX, sX, sY, Constant.playerY, Constant.playerX, Color.White);
        }

        private void renderClothing(int spriteNum, int dx, int dy, int sX, int sY)
        {
            if (spriteNum < 1 | spriteNum > Static.Clothing_Count) return;
            renderRect(Static.Tex_Clothing[spriteNum], dx, dy, Constant.playerY, Constant.playerX, sX, sY, Constant.playerY, Constant.playerX, Color.White);
        }

        // Menu //
        public void renderMenu()
        {
            int LoopI;
            int var = Static.curMainMenu;
            Static.creditsAlpha = (byte)MathHelper.Clamp(Static.creditsAlpha, 0, 255);
            Color _TEMPCOLOR = new Color(255, 255, 255, Static.creditsAlpha);
            string rName = String.Empty;
            string rPass = String.Empty;

            switch (var)
            {

                /// Main Screen ///
                #region MainScreen Render
                case Constant.curMenuMain:

                    // MAIN
                    renderRect(UI_Menu.MenuWindow.mainwindow.Texture.TEXTURE,
                    UI_Menu.MenuWindow.mainwindow.Texture.X,
                    UI_Menu.MenuWindow.mainwindow.Texture.Y,
                    768,
                    UI_Menu.MenuWindow.mainwindow.Texture.W,
                    UI_Menu.MenuWindow.mainwindow.Texture.X,
                    UI_Menu.MenuWindow.mainwindow.Texture.Y,
                    768,
                    UI_Menu.MenuWindow.mainwindow.Texture.W,
                    Color.White);

                    // Render Menu Buttons
                    for (LoopI = 1; LoopI <= 5; LoopI++)
                    {
                        if (UI_Menu.MenuWindow.mainwindow.Button[LoopI].STATE == Constant.BUTTON_STATE_NORMAL)
                        {
                            renderRect(Static.Tex_Buttons[UI_Menu.MenuWindow.mainwindow.Button[LoopI].TEXTURE],
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].X,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].Y,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].W,
                            0,
                            0,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].W,
                            Color.White);
                        }
                        else if (UI_Menu.MenuWindow.mainwindow.Button[LoopI].STATE == Constant.BUTTON_STATE_HOVER)
                        {
                            renderRect(Static.Tex_Buttons_H[UI_Menu.MenuWindow.mainwindow.Button[LoopI].TEXTURE],
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].X,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].Y,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].W,
                            0,
                            0,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].W,
                            Color.White);
                        }
                        else if (UI_Menu.MenuWindow.mainwindow.Button[LoopI].STATE == Constant.BUTTON_STATE_CLICK)
                        {
                            renderRect(Static.Tex_Buttons_C[UI_Menu.MenuWindow.mainwindow.Button[LoopI].TEXTURE],
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].X,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].Y,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].W,
                            0,
                            0,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.mainwindow.Button[LoopI].W,
                            Color.White);
                        }

                    }
                    break;
                #endregion

                /// Login Screen ///
                #region LoginScreen Render
                case Constant.curMenuLogin:

                    // MAIN
                    renderRect(UI_Menu.MenuWindow.mainwindow.Texture.TEXTURE,
                    UI_Menu.MenuWindow.loginwindow.Texture.X,
                    UI_Menu.MenuWindow.loginwindow.Texture.Y,
                    768,
                    UI_Menu.MenuWindow.loginwindow.Texture.W,
                    UI_Menu.MenuWindow.loginwindow.Texture.X,
                    UI_Menu.MenuWindow.loginwindow.Texture.Y,
                    768,
                    UI_Menu.MenuWindow.loginwindow.Texture.W,
                    Color.White);

                    // Render Surfaces
                    for (LoopI = 1; LoopI <= 2; LoopI++)
                    {
                        renderRect(Static.Tex_Gui[UI_Menu.MenuWindow.loginwindow.Textures[LoopI].TEXTURE],
                        UI_Menu.MenuWindow.loginwindow.Textures[LoopI].X,
                        UI_Menu.MenuWindow.loginwindow.Textures[LoopI].Y,
                        UI_Menu.MenuWindow.loginwindow.Textures[LoopI].H,
                        UI_Menu.MenuWindow.loginwindow.Textures[LoopI].W,
                        0,
                        0,
                        UI_Menu.MenuWindow.loginwindow.Textures[LoopI].H,
                        UI_Menu.MenuWindow.loginwindow.Textures[LoopI].W,
                        Color.White);
                    }

                    Vector2 textPos = new Vector2(UI_Menu.MenuWindow.loginwindow.Textures[1].X, UI_Menu.MenuWindow.loginwindow.Textures[1].Y - 20);
                    spriteBatch.DrawString(FONT_TEXTURE[0], "Username", textPos, Color.Black);
                    textPos = new Vector2(UI_Menu.MenuWindow.loginwindow.Textures[2].X, UI_Menu.MenuWindow.loginwindow.Textures[2].Y - 20);
                    spriteBatch.DrawString(FONT_TEXTURE[0], "Password", textPos, Color.Black);

                    rName = Static.Username;
                    if (Static.loginInput == 1) { rName = rName + "|"; }
                    Vector2 Pos = new Vector2(UI_Menu.MenuWindow.loginwindow.Textures[1].X + 7, UI_Menu.MenuWindow.loginwindow.Textures[1].Y + 5);
                    spriteBatch.DrawString(FONT_TEXTURE[0], rName, Pos, Color.Black);

                    if (Static.Password.Length > 0)
                    {
                        for (LoopI = 1; LoopI <= Static.Password.Length; LoopI++)
                        {
                            rPass = rPass + "*";
                        }
                    }
                    if (Static.loginInput == 2) { rPass = rPass + "|"; }
                    Pos = new Vector2(UI_Menu.MenuWindow.loginwindow.Textures[2].X + 7, UI_Menu.MenuWindow.loginwindow.Textures[2].Y + 5);
                    spriteBatch.DrawString(FONT_TEXTURE[0], rPass, Pos, Color.Black);

                    // Render Menu Buttons
                    for (LoopI = 1; LoopI <= 2; LoopI++)
                    {
                        if (UI_Menu.MenuWindow.loginwindow.Button[LoopI].STATE == Constant.BUTTON_STATE_NORMAL)
                        {
                            renderRect(Static.Tex_Buttons[UI_Menu.MenuWindow.loginwindow.Button[LoopI].TEXTURE],
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].X,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].Y,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].W,
                            0,
                            0,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].W,
                            Color.White);
                        }
                        else if (UI_Menu.MenuWindow.loginwindow.Button[LoopI].STATE == Constant.BUTTON_STATE_HOVER)
                        {
                            renderRect(Static.Tex_Buttons_H[UI_Menu.MenuWindow.loginwindow.Button[LoopI].TEXTURE],
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].X,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].Y,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].W,
                            0,
                            0,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].W,
                            Color.White);
                        }
                        else if (UI_Menu.MenuWindow.loginwindow.Button[LoopI].STATE == Constant.BUTTON_STATE_CLICK)
                        {
                            renderRect(Static.Tex_Buttons_C[UI_Menu.MenuWindow.loginwindow.Button[LoopI].TEXTURE],
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].X,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].Y,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].W,
                            0,
                            0,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.loginwindow.Button[LoopI].W,
                            Color.White);
                        }

                    }
                    break;
                #endregion

                /// Register Screen ///
                #region RegisterScreen Render
                case Constant.curMenuRegister:

                    // MAIN
                    renderRect(UI_Menu.MenuWindow.registerwindow.Texture.TEXTURE,
                    UI_Menu.MenuWindow.registerwindow.Texture.X,
                    UI_Menu.MenuWindow.registerwindow.Texture.Y,
                    768,
                    UI_Menu.MenuWindow.registerwindow.Texture.W,
                    UI_Menu.MenuWindow.registerwindow.Texture.X,
                    UI_Menu.MenuWindow.registerwindow.Texture.Y,
                    768,
                    UI_Menu.MenuWindow.registerwindow.Texture.W,
                    Color.White);

                    // Render Selected Sprite //
                    renderRect(Static.Tex_Sprites[Static.rSprite], 496, 260, 32, 32, 32, 0, 32, 32, Color.White);

                    // Render Surfaces
                    for (LoopI = 1; LoopI <= UI_Menu.Max_Register_Textures; LoopI++)
                    {
                        renderRect(Static.Tex_Gui[UI_Menu.MenuWindow.registerwindow.Textures[LoopI].TEXTURE],
                        UI_Menu.MenuWindow.registerwindow.Textures[LoopI].X,
                        UI_Menu.MenuWindow.registerwindow.Textures[LoopI].Y,
                        UI_Menu.MenuWindow.registerwindow.Textures[LoopI].H,
                        UI_Menu.MenuWindow.registerwindow.Textures[LoopI].W,
                        0,
                        0,
                        UI_Menu.MenuWindow.registerwindow.Textures[LoopI].H,
                        UI_Menu.MenuWindow.registerwindow.Textures[LoopI].W,
                        Color.White);
                    }

                    textPos = new Vector2(UI_Menu.MenuWindow.registerwindow.Textures[1].X, UI_Menu.MenuWindow.registerwindow.Textures[1].Y - 20);
                    spriteBatch.DrawString(FONT_TEXTURE[0], "Username", textPos, Color.Black);
                    textPos = new Vector2(UI_Menu.MenuWindow.registerwindow.Textures[2].X, UI_Menu.MenuWindow.registerwindow.Textures[2].Y - 20);
                    spriteBatch.DrawString(FONT_TEXTURE[0], "Password", textPos, Color.Black);

                    rName = Static.rUsername;
                    if (Static.registerInput == 1) { rName = rName + "|"; }
                    Pos = new Vector2(UI_Menu.MenuWindow.registerwindow.Textures[1].X + 7, UI_Menu.MenuWindow.registerwindow.Textures[1].Y + 5);
                    spriteBatch.DrawString(FONT_TEXTURE[0], rName, Pos, Color.Black);


                    if (Static.rPassword.Length > 0)
                    {
                        for (LoopI = 1; LoopI <= Static.rPassword.Length; LoopI++)
                        {
                            rPass = rPass + "*";
                        }
                    }

                    if (Static.registerInput == 2) { rPass = rPass + "|"; }
                    Pos = new Vector2(UI_Menu.MenuWindow.registerwindow.Textures[2].X + 7, UI_Menu.MenuWindow.registerwindow.Textures[2].Y + 5);
                    spriteBatch.DrawString(FONT_TEXTURE[0], rPass, Pos, Color.Black);

                    // Render Menu Buttons
                    for (LoopI = 1; LoopI <= UI_Menu.Max_Register_Buttons; LoopI++)
                    {
                        if (UI_Menu.MenuWindow.registerwindow.Button[LoopI].STATE == Constant.BUTTON_STATE_NORMAL)
                        {
                            renderRect(Static.Tex_Buttons[UI_Menu.MenuWindow.registerwindow.Button[LoopI].TEXTURE],
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].X,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].Y,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].W,
                            0,
                            0,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].W,
                            Color.White);
                        }
                        else if (UI_Menu.MenuWindow.registerwindow.Button[LoopI].STATE == Constant.BUTTON_STATE_HOVER)
                        {
                            renderRect(Static.Tex_Buttons_H[UI_Menu.MenuWindow.registerwindow.Button[LoopI].TEXTURE],
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].X,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].Y,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].W,
                            0,
                            0,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].W,
                            Color.White);
                        }
                        else if (UI_Menu.MenuWindow.registerwindow.Button[LoopI].STATE == Constant.BUTTON_STATE_CLICK)
                        {
                            renderRect(Static.Tex_Buttons_C[UI_Menu.MenuWindow.registerwindow.Button[LoopI].TEXTURE],
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].X,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].Y,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].W,
                            0,
                            0,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].H,
                            UI_Menu.MenuWindow.registerwindow.Button[LoopI].W,
                            Color.White);
                        }                       

                    }

                    // Render Text
                    for (LoopI = 1; LoopI <= UI_Menu.Max_Register_Text; LoopI++)
                    {
                        spriteBatch.DrawString(FONT_TEXTURE[UI_Menu.MenuWindow.registerwindow.Text[LoopI].FONT],
                            UI_Menu.MenuWindow.registerwindow.Text[LoopI].CAPTION,
                            Pos = new Vector2(UI_Menu.MenuWindow.registerwindow.Text[LoopI].X, UI_Menu.MenuWindow.registerwindow.Text[LoopI].Y),
                            UI_Menu.MenuWindow.registerwindow.Text[LoopI].COLOUR);
                    }

                    break;
                #endregion

            }

            // Credits //
            renderRect(Static.Tex_Gui[5],
            0,
            0,
            768,
            1024,
            0,
            0,
            768,
            1024,
            _TEMPCOLOR);

        }

        // Game //
        public Rectangle nextFrame()
        {
            Rectangle Offset = new Rectangle(0,0,0,0);
            int OffsetX;
            int OffsetY;
            int StartX;
            int StartY;
            int EndX;
            int EndY;

            // Set Offset
            OffsetX = Types.Players[Static.MyIndex].OffsetX + 32;
            OffsetY = Types.Players[Static.MyIndex].OffsetY + 32;

            // PreCache
            StartX = Types.Players[Static.MyIndex].X - Static.HalfX;
            StartY = Types.Players[Static.MyIndex].Y - Static.HalfY;

            if (StartX < 0)
            {
                OffsetX = 0;
                if (StartX == -1)
                {
                    if (Types.Players[Static.MyIndex].OffsetX > 0)
                    {
                        OffsetX = Types.Players[Static.MyIndex].OffsetX;
                    }
                }
                StartX = 0;
            }

            if (StartY < 0)
            {
                OffsetY = 0;
                if (StartY == -1)
                {
                    if (Types.Players[Static.MyIndex].OffsetY > 0)
                    {
                        OffsetY = Types.Players[Static.MyIndex].OffsetY;
                    }
                }
                StartY = 0;
            }

            EndX = StartX + (Static.MAX_MAPX + 1) + 1;
            EndY = StartY + (Static.MAX_MAPY + 1) + 1;

            if (EndX > (Types.Map.MaxX - 1))
            {
                OffsetX = 32;

                if (EndX == Types.Map.MaxX)
                {
                    if (Types.Players[Static.MyIndex].OffsetX < 0)
                    {
                        OffsetX = Types.Players[Static.MyIndex].OffsetX + 32;
                    }
                }

                EndX = (Types.Map.MaxX - 1);
                StartX = EndX - Static.MAX_MAPX - 1;
            }

            if (EndY > (Types.Map.MaxY - 1))
            {
                OffsetY = 32;

                if (EndY == Types.Map.MaxY)
                {
                    if (Types.Players[Static.MyIndex].OffsetY < 0)
                    {
                        OffsetY = Types.Players[Static.MyIndex].OffsetY + 32;
                    }
                }

                EndY = (Types.Map.MaxY - 1);
                StartY = EndY - Static.MAX_MAPY - 1;
            }

            Static.ScreenMinY = OffsetY;
            Static.ScreenMaxY = Static.ScreenMinY + Static.ScreenY;
            Static.ScreenMinX = OffsetX;
            Static.ScreenMaxX = Static.ScreenMinX + Static.ScreenX;

            //Check if we need to update the graphics
            if (StartX != Static.LastTileX | StartY != Static.LastTileY)
            {
                Static.MinY = StartY;
                Static.MaxY = EndY;
                Static.MinX = StartX;
                Static.MaxX = EndX;

                //Update the last position
                Static.LastTileX = StartX;
                Static.LastTileY = StartY;

                //Re-create the tile layers
                General.cacheTiles();
            }

            Offset.X = (Static.MinX * 32) + OffsetX;
            Offset.Y = (Static.MinY * 32) + OffsetY;
            return Offset;

        }

        public void renderGameWindows(int windowIndex)
        {
            if (Static.ShowGameWindow[windowIndex])
            {
                switch (windowIndex)
                {
                    case Constant.inventoryWindow:
                        renderInvWindow();
                        break;
                    case Constant.spellWindow:
                        renderSpellWindow();
                        break;
                    case Constant.chatWindow:
                        renderChatWindow();
                        break;
                    case Constant.charWindow: 
                        renderCharWindow();
                        break;
                    case Constant.menuWindow:
                        renderMenuWindow(); 
                        break;
                }
            }
        }

        public void renderChatWindow()
        {

            // Main Window Background //
            renderRect(UI_Game.gameWindow.chatWindow.Texture.TEXTURE,
            UI_Game.gameWindow.chatWindow.Texture.X,
            UI_Game.gameWindow.chatWindow.Texture.Y,
            UI_Game.gameWindow.chatWindow.Texture.H,
            UI_Game.gameWindow.chatWindow.Texture.W,
            0,
            0,
            UI_Game.gameWindow.chatWindow.Texture.H,
            UI_Game.gameWindow.chatWindow.Texture.W,
            Color.White);

            // Set the position //
            if (Static.ChatBox_Chunk <= 1) { Static.ChatBox_Chunk = 1; }

            // Loop through each buffer string //
            for (int LoopC = (10 * Static.ChatBox_Chunk) - (9); LoopC <= 10 * Static.ChatBox_Chunk; LoopC++)
            {
                if (LoopC > Constant.maxChat) { break; }
                if (Static.ChatBox_Chunk * 10 > Constant.maxChat) { Static.ChatBox_Chunk = Static.ChatBox_Chunk - 1; }

                // Set the base position //
                int X = UI_Game.gameWindow.chatWindow.Texture.X + 5; //+ GameGUI.gameWindow.chatWindow.Texture.X;
                int Y = UI_Game.gameWindow.chatWindow.Texture.Y + 2; //+ GameGUI.gameWindow.chatWindow.Texture.Y;

                // Set the Y position to be used //
                Y = (Y - (LoopC * 10) + (10 * Static.ChatBox_Chunk * 10));

                // Don't bother with empty strings //
                if (UI_Game.chatBuffer[LoopC].Text.Length != 0)
                {
                    Vector2 Pos = new Vector2(X, Y);
                    spriteBatch.DrawString(FONT_TEXTURE[1], UI_Game.chatBuffer[LoopC].Text, Pos, Color.White);
                }
            }

            // Draw entered text
            if (Static.EnterText)
            {
                // Set the base position //
                int X = UI_Game.gameWindow.chatWindow.Texture.X + 10;
                int Y = UI_Game.gameWindow.chatWindow.Texture.Y + 106;

                if (Static.EnterTextBufferWidth == 0) { Static.EnterTextBufferWidth = 1; }   // Dividing by 0 is never good
                Vector2 Pos = new Vector2(X, Y);
                spriteBatch.DrawString(FONT_TEXTURE[1], "Text: " + Static.ShownText + "|", Pos, Color.White);
            }

        }

        public void renderInvWindow()
        {
            // Main Window Background //
            renderRect(UI_Game.gameWindow.invWindow.Texture.TEXTURE,
            UI_Game.gameWindow.invWindow.Texture.X,
            UI_Game.gameWindow.invWindow.Texture.Y,
            UI_Game.gameWindow.invWindow.Texture.H,
            UI_Game.gameWindow.invWindow.Texture.W,
            0,
            0,
            UI_Game.gameWindow.invWindow.Texture.H,
            UI_Game.gameWindow.invWindow.Texture.W,
            Color.White);

            int dX = UI_Game.gameWindow.invWindow.Texture.X;
            int dY = UI_Game.gameWindow.invWindow.Texture.Y;
        }

        public void renderSpellWindow()
        {
            // Main Window Background //
            renderRect(UI_Game.gameWindow.spellWindow.Texture.TEXTURE,
            UI_Game.gameWindow.spellWindow.Texture.X,
            UI_Game.gameWindow.spellWindow.Texture.Y,
            UI_Game.gameWindow.spellWindow.Texture.H,
            UI_Game.gameWindow.spellWindow.Texture.W,
            0,
            0,
            UI_Game.gameWindow.spellWindow.Texture.H,
            UI_Game.gameWindow.spellWindow.Texture.W,
            Color.White);

            int dX = UI_Game.gameWindow.spellWindow.Texture.X;
            int dY = UI_Game.gameWindow.spellWindow.Texture.Y;
        }

        public void renderCharWindow()
        {
            Vector2 Pos;

            // Main Window Background //
            renderRect(UI_Game.gameWindow.charWindow.Texture.TEXTURE,
            UI_Game.gameWindow.charWindow.Texture.X,
            UI_Game.gameWindow.charWindow.Texture.Y,
            UI_Game.gameWindow.charWindow.Texture.H,
            UI_Game.gameWindow.charWindow.Texture.W,
            0,
            0,
            UI_Game.gameWindow.charWindow.Texture.H,
            UI_Game.gameWindow.charWindow.Texture.W,
            Color.White);

            int dX = UI_Game.gameWindow.charWindow.Texture.X;
            int dY = UI_Game.gameWindow.charWindow.Texture.Y;

            // Set Face Sprite //
            UI_Game.gameWindow.charWindow.Textures[0].TEXTURE = Static.Tex_Faces[Types.Players[Static.MyIndex].Sprite];

            // Set Stat Caption //
            UI_Game.gameWindow.charWindow.Text[0].CAPTION = UI_Tweaks.getStatText(1);
            UI_Game.gameWindow.charWindow.Text[1].CAPTION = UI_Tweaks.getStatText(2);
            UI_Game.gameWindow.charWindow.Text[2].CAPTION = UI_Tweaks.getStatText(3);
            UI_Game.gameWindow.charWindow.Text[3].CAPTION = UI_Tweaks.getStatText(4);

            // Set Points Label //
            UI_Game.gameWindow.charWindow.Text[5].CAPTION = UI_Tweaks.getPointsText();

            // Set Player Name - Note: This should be done once //
            UI_Tweaks.setNameVector2();

            // Render Textures
            for (int LoopI = 0; LoopI <= 3; LoopI++)
            {
                renderRect(UI_Game.gameWindow.charWindow.Textures[LoopI].TEXTURE,
                dX + UI_Game.gameWindow.charWindow.Textures[LoopI].X,
                dY + UI_Game.gameWindow.charWindow.Textures[LoopI].Y,
                UI_Game.gameWindow.charWindow.Textures[LoopI].H,
                UI_Game.gameWindow.charWindow.Textures[LoopI].W,
                0,
                0,
                UI_Game.gameWindow.charWindow.Textures[LoopI].H,
                UI_Game.gameWindow.charWindow.Textures[LoopI].W,
                Color.White);
            }

            // Render Text
            for (int LoopI = 0; LoopI <= UI_Game.gameWindow.charWindow.Text.Length - 1; LoopI++)
            {
                int windowX = UI_Game.gameWindow.charWindow.Texture.X + UI_Game.gameWindow.charWindow.Text[LoopI].X;
                int windowY = UI_Game.gameWindow.charWindow.Texture.Y + UI_Game.gameWindow.charWindow.Text[LoopI].Y;
                Pos = new Vector2(windowX, windowY);
                spriteBatch.DrawString(FONT_TEXTURE[UI_Game.gameWindow.charWindow.Text[LoopI].FONT],
                    UI_Game.gameWindow.charWindow.Text[LoopI].CAPTION,
                    Pos,
                    UI_Game.gameWindow.charWindow.Text[LoopI].COLOUR);
            }
        }

        public void renderMenuWindow()
        {
            // Main Window Background //
            renderRect(UI_Game.gameWindow.menuWindow.Texture.TEXTURE,
            UI_Game.gameWindow.menuWindow.Texture.X,
            UI_Game.gameWindow.menuWindow.Texture.Y,
            UI_Game.gameWindow.menuWindow.Texture.H,
            UI_Game.gameWindow.menuWindow.Texture.W,
            0,
            0,
            UI_Game.gameWindow.menuWindow.Texture.H,
            UI_Game.gameWindow.menuWindow.Texture.W,
            Color.White);

            int dX = UI_Game.gameWindow.menuWindow.Texture.X;
            int dY = UI_Game.gameWindow.menuWindow.Texture.Y;

            // Render Menu Buttons
            for (int LoopI = 0; LoopI <= 5; LoopI++)
            {
                if (UI_Game.gameWindow.menuWindow.Button[LoopI].STATE == Constant.BUTTON_STATE_NORMAL)
                {
                    renderRect(Static.Tex_Buttons[UI_Game.gameWindow.menuWindow.Button[LoopI].TEXTURE],
                    dX + UI_Game.gameWindow.menuWindow.Button[LoopI].X,
                    dY + UI_Game.gameWindow.menuWindow.Button[LoopI].Y,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].H,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].W,
                    0,
                    0,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].H,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].W,
                    Color.White);
                }
                else if (UI_Game.gameWindow.menuWindow.Button[LoopI].STATE == Constant.BUTTON_STATE_HOVER)
                {
                    renderRect(Static.Tex_Buttons_H[UI_Game.gameWindow.menuWindow.Button[LoopI].TEXTURE],
                    dX + UI_Game.gameWindow.menuWindow.Button[LoopI].X,
                    dY + UI_Game.gameWindow.menuWindow.Button[LoopI].Y,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].H,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].W,
                    0,
                    0,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].H,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].W,
                    Color.White);
                }
                else if (UI_Game.gameWindow.menuWindow.Button[LoopI].STATE == Constant.BUTTON_STATE_CLICK)
                {
                    renderRect(Static.Tex_Buttons_C[UI_Game.gameWindow.menuWindow.Button[LoopI].TEXTURE],
                    dX + UI_Game.gameWindow.menuWindow.Button[LoopI].X,
                    dY + UI_Game.gameWindow.menuWindow.Button[LoopI].Y,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].H,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].W,
                    0,
                    0,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].H,
                    UI_Game.gameWindow.menuWindow.Button[LoopI].W,
                    Color.White);
                }

            }
        }

    }
}
