using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LudumDare26
{
    class PromptController
    {
        public enum PromptType
        {
            Text,
            Image
        }

        public class Prompt
        {
            public PromptType Type;
            public string Name;
            public string Text;
            public bool IsActive;
            public double Delay;
            public bool IsTimed;
            public double Time;
            public float Alpha;
            public bool HasDisplayed = false;
        }

        public static PromptController Instance;

        SpriteFont font;

        List<Prompt> prompts = new List<Prompt>();

        Dictionary<string, Texture2D> promptImages = new Dictionary<string, Texture2D>();

        public PromptController()
        {
            Instance = this;

        }

        public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("font");

            promptImages.Add("move", content.Load<Texture2D>("promptimages/move"));
            promptImages.Add("jump", content.Load<Texture2D>("promptimages/jump"));
            promptImages.Add("crawl", content.Load<Texture2D>("promptimages/crawl"));
            promptImages.Add("climb", content.Load<Texture2D>("promptimages/climb"));
            promptImages.Add("use", content.Load<Texture2D>("promptimages/use"));
        }

        public void Reset()
        {
            ClearPrompts();
            prompts.Clear();
        }

        public void Update(GameTime gameTime)
        {
            foreach (Prompt p in prompts)
            {
                if (p.IsActive)
                {
                    p.HasDisplayed = true;

                    p.Alpha = MathHelper.Lerp(p.Alpha, 1f, 0.05f);
                    if (p.IsTimed)
                    {
                        p.Time -= gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (p.Time <= 0) p.IsActive = false;
                    }
                }
                else
                {
                    if (p.Delay > 0 && !p.HasDisplayed)
                    {
                        p.Delay -= gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (p.Delay <= 0) p.IsActive = true;
                    }
                    p.Alpha = MathHelper.Lerp(p.Alpha, 0f, 0.1f);
                }
                               
            }
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb)
        {
            Vector2 pos = new Vector2(gd.Viewport.Bounds.Center.X, (gd.Viewport.Bounds.Center.Y / 2)-100f);
            foreach (Prompt p in prompts)
            {
                if (p.Alpha > 0.05f)
                {
                    switch (p.Type)
                    {
                        case PromptType.Text:
                            Vector2 size = font.MeasureString(p.Text);
                            Helper.ShadowText(sb, font, p.Text, pos, Color.Salmon * p.Alpha, size / 2, 1f);
                            pos.Y += (size.Y-5);
                            break;
                        case PromptType.Image:
                            pos.Y += 10;
                            sb.Draw(promptImages[p.Text], pos, null, Color.White * p.Alpha, 0f, new Vector2(promptImages[p.Text].Width, promptImages[p.Text].Height) / 2, 0.8f, SpriteEffects.None, 1);
                            pos.Y += (promptImages[p.Text].Height);
                            break;
                    }
                }
            }
        }

        public void AddPrompt(string name, PromptType type, string text, bool isTimed, double time, double delay)
        {
            if (prompts.Find(p => p.Name == name) == null)
            {
                prompts.Add(new Prompt()
                {
                    Name = name,
                    Type = type,
                    Text = text,
                    Delay = delay,
                    IsTimed = isTimed,
                    Time = time,
                    IsActive = (delay>0)?false:true,
                    Alpha = 0f,
                    HasDisplayed = false
                });
            }
        }

        public void RemovePrompt(string name)
        {
            try
            {
                prompts.First(p => p.Name == name).IsActive = false;
                prompts.First(p => p.Name == name).HasDisplayed = true;
            }
            catch (Exception ex) { }
        }

        public void ClearPrompts()
        {
            foreach (Prompt p in prompts)
            {
                p.IsActive = false;
                p.HasDisplayed = true;
            }
        }

    }
}
