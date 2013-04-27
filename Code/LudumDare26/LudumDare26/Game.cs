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

        BlendState multiBlend;
        Effect tileEffect;

        Map gameMap;
        Camera gameCamera;
        Hero gameHero;

        float[] LayerDepths;

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

            tileEffect = Content.Load<Effect>("tileeffect");

            int layerCount = 0;
            foreach (Layer ml in gameMap.Layers)
                if (ml is TileLayer) layerCount++;

            LayerDepths = new float[layerCount];
            float scale = 1f;
            for (int i = 0; i < LayerDepths.Length; i++)
            {
                LayerDepths[i] = scale;
                if (scale > 0f) scale -= 0.25f;
            }

            gameHero = new Hero(Helper.PtoV((gameMap.GetLayer("Spawn") as MapObjectLayer).Objects[0].Location.Center));
            gameHero.LoadContent(Content, GraphicsDevice);

            gameCamera = new Camera(GraphicsDevice.Viewport, gameMap);
            gameCamera.Position = gameHero.Position;
            gameCamera.Target = gameHero.Position;

            multiBlend = new BlendState();
            multiBlend.ColorSourceBlend = Blend.BlendFactor;
            multiBlend.ColorDestinationBlend = Blend.Zero;
            multiBlend.BlendFactor = new Color(0.5f, 0.5f, 0.5f, 1f);
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

            gameHero.Update(gameTime, gameCamera, gameMap);

            gameCamera.Target = gameHero.Position;
            gameCamera.Update(GraphicsDevice.Viewport.Bounds);


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
            for (int l = 0; l < LayerDepths.Length; l++)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, tileEffect, gameCamera.CameraMatrix * Matrix.CreateScale(LayerDepths[l]));
                gameMap.DrawLayer(spriteBatch, l.ToString(), gameCamera, new Color(LayerDepths[l], LayerDepths[l], LayerDepths[l]));
                spriteBatch.End();
            }

            gameHero.Draw(GraphicsDevice, spriteBatch, gameCamera);

            base.Draw(gameTime);
        }
    }
}
