using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Spine;
using TiledLib;

namespace LudumDare26
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class LudumDareGame : Microsoft.Xna.Framework.Game
    {
        static Random rand = new Random();

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Map gameMap;
        Camera gameCamera;
        Hero gameHero;
        Hud gameHud;
        TriggerController gameTriggerController;
        PromptController gamePromptController;

        KeyboardState lks;

        Texture2D blankTex;
        Texture2D skyGradient;
        Texture2D cloudTexture;
        Texture2D valveTexture;

        List<Water> Waters = new List<Water>();

        List<Vector4> Clouds = new List<Vector4>();

        float[] LayerDepths;
        Color[] LayerColors;

        double waterRiseTime;
        int waterLevel = 500;

        int highestWaterLevel = 500;

        bool emptying = false;

        bool resetting = false;
        float fadeAlpha = 1f;

        SoundEffectInstance ambient1;
        SoundEffectInstance ambient2;
        SoundEffectInstance water;

        public LudumDareGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Window.AllowUserResizing = false;
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            AudioController.LoadContent(Content);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            gameMap = Content.Load<Map>("map");

            gameHud = new Hud();
            gameHud.LoadContent(Content);

            gameTriggerController = new TriggerController(gameMap);
            gamePromptController = new PromptController();
            gamePromptController.LoadContent(Content);

            blankTex = Content.Load<Texture2D>("blank");
            skyGradient = Content.Load<Texture2D>("sky-gradient");
            cloudTexture = Content.Load<Texture2D>("cloud-test");
            valveTexture = Content.Load<Texture2D>("valve");

            int layerCount = 0;
            foreach (Layer ml in gameMap.Layers)
                if (ml is TileLayer) layerCount++;

            LayerDepths = new float[layerCount];
            LayerColors = new Color[layerCount];
            float scale = 1f;
            for (int i = 0; i < LayerDepths.Length; i++)
            {
                LayerDepths[i] = scale;
                LayerColors[i] = new Color((1f - (scale * 0.5f)) * 0.4f, (1f - (scale * 0.5f)) * 0.5f, (1f - (scale * 0.5f)) * 0.9f);//Color.White * (scale * 0.5f);
                if (scale > 0f) scale -= 0.33333f;
            }

            gameHero = new Hero(Helper.PtoV((gameMap.GetLayer("Spawn") as MapObjectLayer).Objects[0].Location.Center));
            gameHero.LoadContent(Content, GraphicsDevice);

            gameCamera = new Camera(GraphicsDevice.Viewport, gameMap);
            gameCamera.Position = gameHero.Position;
            gameCamera.Target = gameHero.Position;

            for (scale = 1.5f; scale > -5f; scale -= 0.1f)
            {
                Waters.Add(new Water(GraphicsDevice, gameMap, new Rectangle(-GraphicsDevice.Viewport.Bounds.Width, (gameMap.Height * gameMap.TileHeight) - waterLevel, ((gameMap.Width * gameMap.TileWidth) * 2) + GraphicsDevice.Viewport.Bounds.Width, 400 + waterLevel), new Color(50, 128, 255), Color.Black, scale));
                
            }

            for (scale = 1.5f; scale > -5f; scale -= 0.1f)
            {
                Clouds.Add(new Vector4(rand.Next(1920), 1000f, scale, 0f));
            }

            ambient1 = Content.Load<SoundEffect>("sfx/ambient1").CreateInstance();
            ambient2 = Content.Load<SoundEffect>("sfx/ambient2").CreateInstance();
            water = Content.Load<SoundEffect>("sfx/water").CreateInstance();

            ambient1.Volume = 0f;
            ambient2.Volume = 0f;
            water.Volume = 0f;
            ambient1.IsLooped = true;
            ambient2.IsLooped = true;
            water.IsLooped = true;
            ambient1.Play();
            ambient2.Play();
            water.Play();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Left)) gameHero.MoveLeftRight(-1f);
            else if (ks.IsKeyDown(Keys.Right)) gameHero.MoveLeftRight(1f);

            if (ks.IsKeyDown(Keys.Up)) gameHero.Jump();
            if (ks.IsKeyDown(Keys.Down)) gameHero.Crouch();

            if (ks.IsKeyDown(Keys.Space) && !lks.IsKeyDown(Keys.Space))
            {
                if (gameHero.Complete && !resetting && Hud.Instance.ReadyForRestart) resetting = true;
                if (gameHero.Dead && !resetting && Hud.Instance.ReadyForRestart) resetting = true;
                gameHero.UseObject(gameMap);
            }

            gameHero.waterLevel = (gameMap.Height * gameMap.TileHeight) - waterLevel;
            gameHero.Update(gameTime, gameCamera, gameMap);

            gameCamera.Target = gameHero.Position;
            gameCamera.Update(GraphicsDevice.Viewport.Bounds);

            gameTriggerController.Update(gameTime, gameHero);
            gamePromptController.Update(gameTime);

            gameHud.Update(gameTime, waterLevel, gameHero, gameMap);

            // Scale layers according to player's layer
            float targetScale = 1f;
            for (int l = gameHero.Layer; l < LayerDepths.Length; l++)
            {
                LayerDepths[l] = MathHelper.Lerp(LayerDepths[l], targetScale, 0.1f);
                LayerColors[l] = Color.Lerp(LayerColors[l], new Color((1f - (targetScale * 0.5f)) * 0.4f, (1f - (targetScale * 0.5f)) * 0.5f, (1f - (targetScale * 0.5f)) * 0.9f), 0.1f); //* (targetScale * 0.5f)
                if (targetScale > 0f) targetScale -= 0.333f;
                else LayerColors[l] = Color.Lerp(LayerColors[l], new Color((1f - targetScale) * 0.4f, (1f - targetScale) * 0.5f, (1f - targetScale) * 0.9f) * 0f, 0.1f);
            }
            if (gameHero.Layer > 0)
            {
                targetScale = 1.5f;
                for (int l = gameHero.Layer-1; l >=0; l--)
                {
                    LayerDepths[l] = MathHelper.Lerp(LayerDepths[l], targetScale, 0.1f);
                    if (gameHero.Layer - l == 1) LayerColors[l] = Color.Lerp(LayerColors[l], new Color(targetScale * 0.01f, targetScale * 0.02f, targetScale * 0.1f) * 0.85f, 0.1f);
                    else if (gameHero.Layer == l) LayerColors[l] = Color.Lerp(LayerColors[l], Color.White * 1f, 0.1f);
                    else LayerColors[l] = Color.Lerp(LayerColors[l], new Color(targetScale * 0.01f, targetScale * 0.02f, targetScale * 0.1f) * 0f, 0.1f);
                    targetScale += 0.5f;
                }
            }

            if (LayerDepths[gameHero.Layer] > 0.98f && LayerDepths[gameHero.Layer] < 1.02f && !gameHero.teleportFinished)
            {
                gameHero.teleportFinished = true;
                AudioController.PlaySFX("teleport_out", 0.6f, 0f, 0f);
            }

            lks = ks;

            waterRiseTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (waterRiseTime >= 50)
            {
                waterRiseTime = 0;

                if (TriggerController.Instance.WaterTriggered)
                {
                    waterLevel+=2;

                    if (waterLevel > highestWaterLevel)
                    {
                        highestWaterLevel = waterLevel;
                        if (!gameHero.Complete) gameHud.SoulsPerished += 100;
                    }
                    else if (!gameHero.Complete) gameHud.SoulsPerished += 10;

                    foreach (Water w in Waters)
                    {
                        w.bounds.Offset(new Point(0, -2));
                        w.bounds.Height+=2;
                    }
                }
            }

            if ((gameMap.Height * gameMap.TileHeight) - waterLevel < gameHero.Position.Y - 200f)
                gameHero.UnderWater = true;
                //emptying = true;

            if (gameHero.usingValve || emptying || gameHero.Complete)
            {
                waterLevel-=4;
                foreach (Water w in Waters)
                {
                    w.bounds.Offset(new Point(0, 4));
                    w.bounds.Height -= 4;
                }
                
            }

            if (waterLevel < 200) emptying = false;

            

            float startScale = 1.5f;
            foreach (Water w in Waters.OrderByDescending(wat => wat.Scale))
            {
                w.Scale = MathHelper.Lerp(w.Scale, startScale + (gameHero.Layer * 0.25f), 0.1f);

                if (w.Scale > 0.25f) w.Alpha = MathHelper.Lerp(w.Alpha, w.Scale, 0.1f);
                else w.Alpha = MathHelper.Lerp(w.Alpha, 0f, 0.1f);

                if(w.Scale>0f) w.Update(gameTime);
                startScale -= 0.1f;
            }

            startScale = 1.5f;
            for(int c = 0; c<Clouds.Count;c++)
            {
                Vector4 cl = Clouds[c];
                cl.Z = MathHelper.Lerp(cl.Z, startScale + (gameHero.Layer * 0.25f), 0.01f);
                cl.Y = ((((gameCamera.Position.Y) - (GraphicsDevice.Viewport.Height/2)) - ((((float)GraphicsDevice.Viewport.Height/2) / (float)(gameMap.Height * gameMap.TileHeight)) * (gameHero.Position.Y*3)))  +100*cl.Z);
                //cl.Y = ((gameCamera.Position.Y) - (GraphicsDevice.Viewport.Height/2)) * -(cl.Z * 1.5f);
                cl.X -= 0.1f;
                if (cl.X <= -cloudTexture.Width) cl.X = 0;

                if (cl.Z > 0.5f) cl.W = MathHelper.Lerp(cl.W, 1f, 0.01f);
                else if (cl.Z > 0f) cl.W = MathHelper.Lerp(cl.W, cl.Z, 0.01f);
                else cl.W = MathHelper.Lerp(cl.W, 0f, 0.01f);

                Clouds[c] = cl;
                startScale -= 0.1f;
            }


            if (!resetting)
            {
                fadeAlpha = MathHelper.Lerp(fadeAlpha, 0f, 0.05f);
            }
            else
            {
                fadeAlpha = MathHelper.Lerp(fadeAlpha, 1f, 0.05f);
                if (fadeAlpha > 0.99f)
                    Reset();
            }

            if (gameHero.Position.Y > (gameMap.Height * gameMap.TileHeight)/2)
            {
                ambient1.Volume = MathHelper.Lerp(ambient1.Volume, 1f, 0.001f);
                ambient2.Volume = MathHelper.Lerp(ambient2.Volume, 0f, 0.001f);
            }
            else
            {
                ambient1.Volume = MathHelper.Lerp(ambient1.Volume, 0f, 0.001f);
                ambient2.Volume = MathHelper.Lerp(ambient2.Volume, 1f, 0.001f);
            }

            float wdistance = ((gameMap.Height * gameMap.TileHeight) - waterLevel) - gameHero.Position.Y;
            float vol = 0.5f - ((0.5f / 800f) * wdistance);
            water.Volume = MathHelper.Clamp(vol, 0f, 0.5f);


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.Draw(skyGradient, GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.End();

            // TODO: Add your drawing code here
            for (int l = LayerDepths.Length-1; l >=0; l--)
            {
                foreach (Water w in Waters.OrderBy(wat => wat.Scale))
                    if (w.Scale < LayerDepths[l] && w.Scale>0.25f)
                    {
                        if (l == LayerDepths.Length - 1) w.Draw(gameCamera);
                        else if (w.Scale >= LayerDepths[l + 1]) w.Draw(gameCamera);
                    }


                
                foreach (Vector4 cloud in Clouds.OrderBy(cl => cl.Z))
                    if (cloud.Z < LayerDepths[l] && cloud.Z>0f && cloud.Z<1f)
                    {
                        if (l == LayerDepths.Length - 1) DrawCloud(spriteBatch, cloud);
                        else if (cloud.Z >= LayerDepths[l + 1]) DrawCloud(spriteBatch, cloud);
                    }


                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, gameCamera.CameraMatrix * Matrix.CreateScale(MathHelper.Clamp(LayerDepths[l], 0.25f, 2f)) * Matrix.CreateTranslation(new Vector3(0f, 300f - (300f * LayerDepths[l]), 0f)));
                //spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, gameCamera.CameraMatrix);// * Matrix.CreateScale(MathHelper.Clamp(LayerDepths[l], 0.25f, 2f)) * Matrix.CreateTranslation(new Vector3(0f, 300f - (300f * LayerDepths[l]), 0f)));

                gameMap.DrawLayer(spriteBatch, l.ToString() + "Decal1", gameCamera, l != gameHero.Layer ? LayerColors[l] : Color.White, (l != gameHero.Layer) ? true : false, LayerDepths[l]);
                gameMap.DrawLayer(spriteBatch, l.ToString() + "Decal", gameCamera, l != gameHero.Layer ? LayerColors[l] : Color.White, (l != gameHero.Layer) ? true : false, LayerDepths[l]);
                gameMap.DrawLayer(spriteBatch, l.ToString(), gameCamera, l != gameHero.Layer ? LayerColors[l] : Color.White, (l != gameHero.Layer) ? true : false, LayerDepths[l]);
                TriggerController.Instance.DrawValves(spriteBatch, l, valveTexture, l != gameHero.Layer ? LayerColors[l] : Color.White, (l != gameHero.Layer) ? true : false, gameHero);
                spriteBatch.End();

                

                if (l > gameHero.Layer)
                {
                    //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, dss, null, ate, gameCamera.CameraMatrix);// * Matrix.CreateScale(LayerDepths[l]));
                    //gameMap.DrawLayer(spriteBatch, l.ToString() + "Decal1", gameCamera, l < gameHero.Layer ? LayerColors[l] : Color.White * 0f);
                    //gameMap.DrawLayer(spriteBatch, l.ToString() + "Decal", gameCamera, l < gameHero.Layer ? LayerColors[l] : Color.White * 0f);
                    //gameMap.DrawLayer(spriteBatch, l.ToString(), gameCamera, l < gameHero.Layer ? LayerColors[l] : Color.White * 0f);
                    //spriteBatch.End();

                    //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, dss2, null, null);
                    //spriteBatch.Draw(blankTex, GraphicsDevice.Viewport.Bounds, new Color(255, 255, 255, 1));//LayerColors[l]);
                    //spriteBatch.End();
                }

                if(l==gameHero.Layer)
                    gameHero.Draw(GraphicsDevice, spriteBatch, gameCamera);
            }

            foreach (Water w in Waters.OrderBy(wat => wat.Scale))
                if (w.Scale >= LayerDepths[0] && w.Scale<1.6f && w.Scale>0f) w.Draw(gameCamera);

            foreach (Vector4 cloud in Clouds.OrderBy(cl => cl.Z))
                if (cloud.Z >= LayerDepths[0] && cloud.Z < 1f && cloud.Z>0f) DrawCloud(spriteBatch, cloud);

            spriteBatch.Begin();
            gamePromptController.Draw(GraphicsDevice, spriteBatch);
            gameHud.Draw(GraphicsDevice, spriteBatch);

            if (fadeAlpha > 0.05f)
            {
                spriteBatch.Draw(blankTex, GraphicsDevice.Viewport.Bounds, Color.Black * fadeAlpha);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        void DrawCloud(SpriteBatch sb, Vector4 cloud)
        {
            sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, gameCamera.CameraMatrix * Matrix.CreateScale(cloud.Z) * Matrix.CreateTranslation(new Vector3(0f, MathHelper.Clamp(100f/cloud.Z,0f,200f), 0f)));
            for (int x = -cloudTexture.Width; x < (gameMap.Width * gameMap.TileWidth) + cloudTexture.Width; x += (int)((float)cloudTexture.Width))
            {
                 sb.Draw(cloudTexture, new Vector2((x + cloud.X)/cloud.Z , -100f+MathHelper.Clamp(cloud.Y, 0f, gameMap.Height * gameMap.TileHeight)), null, Color.White * cloud.W, 0f, new Vector2(cloudTexture.Width, cloudTexture.Height)/2, 1f/cloud.Z, SpriteEffects.None, 1);
            }
            sb.End();
        }

        void Reset()
        {
            waterLevel = 500;
            highestWaterLevel = 500;

            foreach (Water w in Waters)
                w.bounds = new Rectangle(-GraphicsDevice.Viewport.Bounds.Width, (gameMap.Height * gameMap.TileHeight) - waterLevel, ((gameMap.Width * gameMap.TileWidth) * 2) + GraphicsDevice.Viewport.Bounds.Width, 400 + waterLevel);

            TriggerController.Instance.Reset();
            PromptController.Instance.Reset();
            gameHud.Reset();
            gameHero.Reset();

            gameCamera.Position = gameHero.Position;
            gameCamera.Target = gameHero.Position;

            float scale = 1f;
            for (int i = 0; i < LayerDepths.Length; i++)
            {
                LayerDepths[i] = scale;
                LayerColors[i] = new Color((1f - (scale * 0.5f)) * 0.4f, (1f - (scale * 0.5f)) * 0.5f, (1f - (scale * 0.5f)) * 0.9f);//Color.White * (scale * 0.5f);
                if (scale > 0f) scale -= 0.33333f;
            }

            scale = 1.5f;
            for (int i = 0; i < Waters.Count; i++)
            {
                Waters[i] = new Water(GraphicsDevice, gameMap, new Rectangle(-GraphicsDevice.Viewport.Bounds.Width, (gameMap.Height * gameMap.TileHeight) - waterLevel, ((gameMap.Width * gameMap.TileWidth) * 2) + GraphicsDevice.Viewport.Bounds.Width, 400 + waterLevel), new Color(50, 128, 255), Color.Black, scale);
                scale -= 0.1f;
            }

            scale = 1.5f;
            for (int i = 0; i < Clouds.Count; i++)
            {
                Clouds[i] = new Vector4(rand.Next(1920), 1000f, scale, 0f);
                scale -= 0.1f;
            }

        
            

            resetting = false;
        }
    }
}
