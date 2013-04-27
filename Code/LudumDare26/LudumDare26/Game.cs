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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Map gameMap;
        Camera gameCamera;
        Hero gameHero;

        KeyboardState lks;

        float[] LayerDepths;
        Color[] LayerColors;

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
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            gameMap = Content.Load<Map>("map");

            int layerCount = 0;
            foreach (Layer ml in gameMap.Layers)
                if (ml is TileLayer) layerCount++;

            LayerDepths = new float[layerCount];
            LayerColors = new Color[layerCount];
            float scale = 1f;
            for (int i = 0; i < LayerDepths.Length; i++)
            {
                LayerDepths[i] = scale;
                LayerColors[i] = Color.White * scale;
                if (scale > 0f) scale -= 0.25f;
            }

            gameHero = new Hero(Helper.PtoV((gameMap.GetLayer("Spawn") as MapObjectLayer).Objects[0].Location.Center));
            gameHero.LoadContent(Content, GraphicsDevice);

            gameCamera = new Camera(GraphicsDevice.Viewport, gameMap);
            gameCamera.Position = gameHero.Position;
            gameCamera.Target = gameHero.Position;

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

            if (ks.IsKeyDown(Keys.Space) && !lks.IsKeyDown(Keys.Space)) gameHero.UsePortal(gameMap);

            gameHero.Update(gameTime, gameCamera, gameMap);

            gameCamera.Target = gameHero.Position;
            gameCamera.Update(GraphicsDevice.Viewport.Bounds);


            // Scale layers according to player's layer
            float targetScale = 1f;
            for (int l = gameHero.Layer; l < LayerDepths.Length; l++)
            {
                LayerDepths[l] = MathHelper.Lerp(LayerDepths[l], targetScale, 0.1f);
                LayerColors[l] = Color.Lerp(LayerColors[l], Color.White * targetScale, 0.1f);
                if (targetScale > 0f) targetScale -= 0.25f;
            }
            if (gameHero.Layer > 0)
            {
                targetScale = 1.5f;
                for (int l = gameHero.Layer-1; l >=0; l--)
                {
                    LayerDepths[l] = MathHelper.Lerp(LayerDepths[l], targetScale, 0.1f);
                    if (gameHero.Layer - l == 1) LayerColors[l] = Color.Lerp(LayerColors[l], Color.Black * 1f, 0.1f);
                    else LayerColors[l] = Color.Lerp(LayerColors[l], Color.Black * 0f, 0.1f);
                    targetScale += 0.5f;
                }
            }

            lks = ks;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here
            for (int l = LayerDepths.Length-1; l >=0; l--)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, gameCamera.CameraMatrix * Matrix.CreateScale(LayerDepths[l]));
                gameMap.DrawLayer(spriteBatch, l.ToString() + "Decal", gameCamera, LayerColors[l]);
                gameMap.DrawLayer(spriteBatch, l.ToString(), gameCamera, LayerColors[l]);
                spriteBatch.End();

                if(l==gameHero.Layer)
                    gameHero.Draw(GraphicsDevice, spriteBatch, gameCamera);
            }

            

            base.Draw(gameTime);
        }
    }
}
