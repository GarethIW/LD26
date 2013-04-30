using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace LudumDare26
{
    class Hud
    {
        public static Hud Instance;

        public bool ReadyForRestart;

        public int SoulsPerished = 0;

        public bool ShowingSouls = false;
        public bool ShowingWater = false;

        float waterAlpha = 0f;
        float soulsAlpha = 0f;

        float waterLevelHeight = 0f;

        int waterAnimFrame;
        double waterAnimTime = 0;

        Texture2D texHud;
        SpriteFont smallFont;
        SpriteFont largeFont;

        float soulsToCenterAmount = 0f;

        bool endPromptsDone = false;

        public Hud()
        {
            Instance = this;
        }

        public void LoadContent(ContentManager content)
        {
            texHud = content.Load<Texture2D>("hud");
            smallFont = content.Load<SpriteFont>("hudfont-small");
            largeFont = content.Load<SpriteFont>("hudfont-large");
        }

        public void Reset()
        {
            soulsToCenterAmount = 0;
            endPromptsDone = false;
            ReadyForRestart = false;
            SoulsPerished = 0;
            ShowingSouls = false;
            ShowingWater = false;
            waterAlpha = 0f;
            soulsAlpha = 0f;
        }

        public void Update(GameTime gameTime, int waterLevel, Hero gameHero, Map gameMap)
        {
            if (ShowingSouls)
            {
                soulsAlpha = MathHelper.Lerp(soulsAlpha, 1f, 0.05f);
            }


            if (ShowingWater)
            {
                waterAlpha = MathHelper.Lerp(waterAlpha, 1f, 0.05f);
            }

            waterLevelHeight = MathHelper.Clamp((1f / ((gameMap.Height * gameMap.TileHeight) - (gameHero.Position.Y - 300f))) * (float)waterLevel, 0f, 1f); //((float)texHud.Height / (gameHero.Position.Y - 200f)) * (float)waterLevel;

            waterAnimTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (waterAnimTime >= 200)
            {
                waterAnimTime = 0;
                waterAnimFrame = 1 - waterAnimFrame;
            }

            if (gameHero.Dead)
            {
                ShowingSouls = true;
                soulsToCenterAmount = MathHelper.Lerp(soulsToCenterAmount, 1f, 0.05f);
                if (!endPromptsDone)
                {
                    PromptController.Instance.ClearPrompts();
                    PromptController.Instance.AddPrompt("dead1", PromptController.PromptType.Text, " ", false, 0, 0);
                    PromptController.Instance.AddPrompt("dead2", PromptController.PromptType.Text, " ", false, 0, 0);
                    PromptController.Instance.AddPrompt("dead3", PromptController.PromptType.Text, "...and one.", false, 0, 4000);
                    PromptController.Instance.AddPrompt("dead4", PromptController.PromptType.Image, "use", false, 0, 8000);
                    endPromptsDone = true;
                }

                if (soulsToCenterAmount > 0.98f) ReadyForRestart = true;
            }

            if (gameHero.Complete)
            {
                ShowingSouls = true;
                soulsToCenterAmount = MathHelper.Lerp(soulsToCenterAmount, 1f, 0.05f);
                if (!endPromptsDone)
                {
                    PromptController.Instance.ClearPrompts();
                    PromptController.Instance.AddPrompt("comp1", PromptController.PromptType.Text, " ", false, 0, 0);
                    PromptController.Instance.AddPrompt("comp2", PromptController.PromptType.Text, " ", false, 0, 0);
                    PromptController.Instance.AddPrompt("comp3", PromptController.PromptType.Text, "...but Gerde had saved untold numbers", false, 0, 4000);
                    PromptController.Instance.AddPrompt("comp4", PromptController.PromptType.Text, "of her people. She was happy.", false, 0, 4000);
                    PromptController.Instance.AddPrompt("comp5", PromptController.PromptType.Image, "use", false, 0, 8000);
                    endPromptsDone = true;
                }

                if (soulsToCenterAmount > 0.98f) ReadyForRestart = true;
            }
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb)
        {
            Rectangle bounds = gd.Viewport.Bounds;

            Vector2 meterPosition = new Vector2(bounds.Right - 100, ((bounds.Height/2) - (texHud.Height/2)) +10);
            Vector2 soulsPosition1 = new Vector2(bounds.Left + 100, bounds.Height - 110);
            Vector2 soulsPosition2 = new Vector2(bounds.Left + 100, bounds.Height - 85);

            soulsPosition1 = Vector2.Lerp(new Vector2(bounds.Left + 100, bounds.Height - 110), new Vector2(bounds.Center.X - (smallFont.MeasureString("Souls Perished").X / 2), (bounds.Center.Y / 2) - 100f), soulsToCenterAmount);
            soulsPosition2 = Vector2.Lerp(new Vector2(bounds.Left + 100, bounds.Height - 85), new Vector2(bounds.Center.X - (largeFont.MeasureString(SoulsPerished.ToString("N0")).X / 2), (bounds.Center.Y / 2) - 75f), soulsToCenterAmount);


            // Water level
            if (waterAlpha > 0f)
            {
                sb.Draw(texHud, meterPosition, new Rectangle(75, 0, 75, texHud.Height), Color.White * waterAlpha, 0f, new Vector2(75 / 2, 0), 1f, SpriteEffects.None, 1);
                sb.Draw(texHud, meterPosition + new Vector2(0, texHud.Height) + new Vector2(2, 2), new Rectangle(150, 0, 75, texHud.Height), Color.Black * waterAlpha, 0f, new Vector2(75 / 2, texHud.Height), new Vector2(1f, waterLevelHeight), SpriteEffects.None, 1);
                sb.Draw(texHud, meterPosition + new Vector2(0, texHud.Height), new Rectangle(150, 0, 75, texHud.Height), Color.White * waterAlpha, 0f, new Vector2(75 / 2, texHud.Height), new Vector2(1f, waterLevelHeight), SpriteEffects.None, 1);
                sb.Draw(texHud, meterPosition + new Vector2(0, (texHud.Height - (texHud.Height * waterLevelHeight)) + 5f), new Rectangle(0, 78 + (75*waterAnimFrame), 75, 72), Color.White * waterAlpha, 0f, new Vector2(75 / 2, 75), 1f, SpriteEffects.None, 1);
                sb.Draw(texHud, meterPosition + new Vector2(0, -20) + new Vector2(2, 2), new Rectangle(0, 0, 75, 75), Color.Black * waterAlpha, 0f, new Vector2(75 / 2, 0), 1f, SpriteEffects.None, 1);
                sb.Draw(texHud, meterPosition + new Vector2(0, -20), new Rectangle(0, 0, 75, 75), Color.White * waterAlpha, 0f, new Vector2(75 / 2, 0), 1f, SpriteEffects.None, 1);
                if (waterLevelHeight > 0.8f)
                {
                    sb.Draw(texHud, meterPosition + new Vector2(0, -20), new Rectangle(0, 225, 75, 75), Color.White * (1f - (1f / 0.2f) * (1f - waterLevelHeight)), 0f, new Vector2(75 / 2, 0), 1f, SpriteEffects.None, 1);
                }
            }

            if (soulsAlpha > 0f)
            {
                Helper.ShadowText(sb, smallFont, "Souls Perished", soulsPosition1, Color.White * soulsAlpha, Vector2.Zero, 1f);
                Helper.ShadowText(sb, largeFont, SoulsPerished.ToString("N0"), soulsPosition2, Color.White * soulsAlpha, Vector2.Zero, 1f);    
            }
        }

    }
}
