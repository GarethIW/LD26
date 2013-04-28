using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TiledLib;

namespace LudumDare26
{
    public class Water
    {
        struct WaterColumn
        {
            public float TargetHeight;
            public float Height;
            public float Speed;

            public void Update(float dampening, float tension)
            {
                float x = TargetHeight - Height;
                Speed += tension * x - Speed * dampening;
                Height += Speed;
            }
        }

        PrimitiveBatch pb;
        WaterColumn[] columns = new WaterColumn[201];
        static Random rand = new Random();

        public float Tension = 0.0025f;
        public float Dampening = 0.025f;
        public float Spread = 0.05f;

        public float Alpha = 1f;

        RenderTarget2D metaballTarget, particlesTarget;
        SpriteBatch spriteBatch;
        AlphaTestEffect alphaTest;
        Texture2D particleTexture;

        public int actualHeight = 900;

        int rippleX = 0;

        Map gameMap;

        private float ScaleWidth { get { return (bounds.Width) / (columns.Length - 1f); } }

        public float Scale;

        Color topColor;
        Color bottomColor;

        public Rectangle bounds;

        List<Particle> particles = new List<Particle>();
        class Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Orientation;

            public Particle(Vector2 position, Vector2 velocity, float orientation)
            {
                Position = position;
                Velocity = velocity;
                Orientation = orientation;
            }
        }

        public Water(GraphicsDevice device, Map map, Rectangle dest, Color top, Color bottom, float scale)//, Texture2D particleTexture)
        {
            Scale = scale;

            pb = new PrimitiveBatch(device);
            //this.particleTexture = particleTexture;
            spriteBatch = new SpriteBatch(device);
            gameMap = map;

            topColor = top * 0.8f;
            bottomColor = bottom;

            bounds = dest;

            //metaballTarget = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height);
            //particlesTarget = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height);
            //alphaTest = new AlphaTestEffect(device);
            //alphaTest.ReferenceAlpha = 175;

            //var view = device.Viewport;
            //alphaTest.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) *
            //    Matrix.CreateOrthographicOffCenter(0, view.Width, view.Height, 0, 0, 1);

            columns = new WaterColumn[bounds.Width / 30];

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new WaterColumn()
                {
                    Height = bounds.Height,
                    TargetHeight = bounds.Height,
                    Speed = 0
                };
            }

            rDeltas = new float[columns.Length];
            lDeltas = new float[columns.Length];
        }

        // Returns the height of the water at a given x coordinate.
        public float GetHeight(float x)
        {
            if (x < 0 || x > bounds.Width)
                return bounds.Height;

            return columns[(int)(x / ScaleWidth)].Height;
        }

        void UpdateParticle(Particle particle)
        {
            const float Gravity = 0.3f;
            particle.Velocity.Y += Gravity;
            particle.Position += particle.Velocity;
            particle.Orientation = GetAngle(particle.Velocity);
        }

        public void Splash(float xPosition, float speed)
        {
            int index = (int)MathHelper.Clamp((xPosition - bounds.Left) / ScaleWidth, 0, columns.Length - 1);
            for (int i = Math.Max(0, index - 0); i < Math.Min(columns.Length - 1, index + 1); i++)
                columns[index].Speed = -speed;



        }



        private void CreateParticle(Vector2 pos, Vector2 velocity)
        {
            particles.Add(new Particle(pos, velocity, 0));
        }

        private Vector2 FromPolar(float angle, float magnitude)
        {
            return magnitude * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private float GetRandomFloat(float min, float max)
        {
            return (float)rand.NextDouble() * (max - min) + min;
        }

        private Vector2 GetRandomVector2(float maxLength)
        {
            return FromPolar(GetRandomFloat(-MathHelper.Pi, MathHelper.Pi), GetRandomFloat(0, maxLength));
        }

        private float GetAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }

        double rippleTime = 0;
        float[] lDeltas;
        float[] rDeltas;
        public void Update(GameTime gameTime)
        {
            rippleTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (rippleTime > 500)
            {
                rippleTime = 0;
                rippleX += 10;
                if (rippleX >= 500 || rippleX > bounds.Width) rippleX = 0;
                for (int x = rippleX; x < bounds.Width; x += 500) Splash((float)x + bounds.X, 10f);
            }
            //if (rippleX >= (gameMap.Width * gameMap.TileWidth)) rippleX -= (gameMap.Width * gameMap.TileWidth);

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i].Update(Dampening, Tension);
                columns[i].TargetHeight = bounds.Height;
            }



            // do some passes where columns pull on their neighbours
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    if (i > 0)
                    {
                        lDeltas[i] = Spread * (columns[i].Height - columns[i - 1].Height);
                        columns[i - 1].Speed += lDeltas[i];
                    }
                    if (i < columns.Length - 1)
                    {
                        rDeltas[i] = Spread * (columns[i].Height - columns[i + 1].Height);
                        columns[i + 1].Speed += rDeltas[i];
                    }
                }

                for (int i = 0; i < columns.Length; i++)
                {
                    if (i > 0)
                        columns[i - 1].Height += lDeltas[i];
                    if (i < columns.Length - 1)
                        columns[i + 1].Height += rDeltas[i];
                }
            }

            //forreach (var particle in particles)
            //	UpdateParticle(particle);

            //particles = particles.Where(x => x.Position.X >= 0 && x.Position.X <= 800 && x.Position.Y - 5 <= GetHeight(x.Position.X)).ToList();
        }

        //public void DrawToRenderTargets()
        //{
        //    GraphicsDevice device = spriteBatch.GraphicsDevice;
        //    device.SetRenderTarget(metaballTarget);
        //    device.Clear(Color.Transparent);

        //    // draw particles to the metaball render target
        //    spriteBatch.Begin(0, BlendState.Additive);
        //    foreach (var particle in particles)
        //    {
        //        Vector2 origin = new Vector2(particleTexture.Width, particleTexture.Height) / 2f;
        //        spriteBatch.Draw(particleTexture, particle.Position, null, Color.White, particle.Orientation, origin, 2f, 0, 0);
        //    }
        //    spriteBatch.End();

        //    // draw a gradient above the water so the metaballs will fuse with the water's surface.
        //    pb.Begin(PrimitiveType.TriangleList);

        //    const float thickness = 20;
        //    float scale = Scale;
        //    for (int i = 1; i < columns.Length; i++)
        //    {
        //        Vector2 p1 = new Vector2((i - 1) * scale, columns[i - 1].Height);
        //        Vector2 p2 = new Vector2(i * scale, columns[i].Height);
        //        Vector2 p3 = new Vector2(p1.X, p1.Y - thickness);
        //        Vector2 p4 = new Vector2(p2.X, p2.Y - thickness);

        //        pb.AddVertex(p2, Color.White);
        //        pb.AddVertex(p1, Color.White);
        //        pb.AddVertex(p3, Color.Transparent);

        //        pb.AddVertex(p3, Color.Transparent);
        //        pb.AddVertex(p4, Color.Transparent);
        //        pb.AddVertex(p2, Color.White);
        //    }

        //    pb.End();

        //    // save the results in another render target (in particlesTarget)
        //    device.SetRenderTarget(particlesTarget);
        //    device.Clear(Color.Transparent);
        //    spriteBatch.Begin(0, null, null, null, null, alphaTest);
        //    spriteBatch.Draw(metaballTarget, Vector2.Zero, Color.White);
        //    spriteBatch.End();

        //    // switch back to drawing to the backbuffer.
        //    device.SetRenderTarget(null);
        //}

        public void Draw(Camera gameCamera)
        {
            Color lightBlue = new Color(0.2f, 0.5f, 1f);

            // draw the particles 3 times to create a bevelling effect
            //spriteBatch.Begin();
            //spriteBatch.Draw(particlesTarget, -Vector2.One, new Color(0.8f, 0.8f, 1f));
            //spriteBatch.Draw(particlesTarget, Vector2.One, new Color(0f, 0f, 0.2f));
            //spriteBatch.Draw(particlesTarget, Vector2.Zero, lightBlue);
            //spriteBatch.End();

            // draw the waves
            pb.Begin(PrimitiveType.TriangleList, gameCamera.CameraMatrix * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(new Vector3(0f, 300f - (300f * Scale), 0f)));
            Color midnightBlue = new Color(0, 0, 0);// *0.9f;
            lightBlue *= 0.8f;

            float bottom = bounds.Bottom;// - actualHeight) + 700;
            float scale = ScaleWidth;
            for (int i = 1; i < columns.Length; i++)
            {
                Vector2 p1 = new Vector2(bounds.Left + ((i - 1) * scale), bottom - columns[i - 1].Height);
                Vector2 p2 = new Vector2(bounds.Left + (i * scale), bottom - columns[i].Height);
                Vector2 p3 = new Vector2(p2.X, bottom);
                Vector2 p4 = new Vector2(p1.X, bottom);

                pb.AddVertex(p1, topColor*Alpha);
                pb.AddVertex(p2, topColor * Alpha);
                pb.AddVertex(p3, bottomColor * Alpha);

                pb.AddVertex(p1, topColor * Alpha);
                pb.AddVertex(p3, bottomColor * Alpha);
                pb.AddVertex(p4, bottomColor * Alpha);
            }

            pb.End();
        }
    }
}
