﻿using System;
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
    class TriggerController
    {
        class Trigger
        {
            public MapObject Object;
            public bool HasTriggered;
            public Vector2 Speed;
            public Vector2 Position;
        }

        public static TriggerController Instance;

        static Random rand = new Random();

        List<Trigger> triggers = new List<Trigger>();
        List<Trigger> valves = new List<Trigger>();

        public bool WaterTriggered = false;

        public bool AtValve = false;

        public TriggerController(Map gameMap)
        {
            Instance = this;

            MapObjectLayer triggerLayer = gameMap.GetLayer("Triggers") as MapObjectLayer;

            foreach (MapObject o in triggerLayer.Objects)
            {
                triggers.Add(new Trigger()
                {
                    Object = o,
                    HasTriggered = false
                });
            }

            triggerLayer = gameMap.GetLayer("Valves") as MapObjectLayer;

            foreach (MapObject o in triggerLayer.Objects)
            {
                valves.Add(new Trigger()
                {
                    Object = o,
                    HasTriggered = false,
                    Position = new Vector2(o.Location.Center.X, o.Location.Center.Y),
                    Speed = new Vector2(3f, 0.1f)
                });
            }
        }

        public void Update(GameTime gameTime, Hero gameHero)
        {
            foreach (Trigger trig in triggers)
            {
                if(!trig.HasTriggered)
                    if (trig.Object.Location.Contains(Helper.VtoP(gameHero.Position)))
                    {
                        int layer = 0;
                        if (trig.Object.Properties.Contains("Layer")) layer = Convert.ToInt16(trig.Object.Properties["Layer"]);
                        if (gameHero.Layer == layer)
                        {
                            trig.HasTriggered = true;
                            ActivateTrigger(Convert.ToInt16(trig.Object.Name));
                        }
                    }
            }

            AtValve = false;
            foreach (Trigger trig in valves)
            {
                if (!trig.HasTriggered)
                {
                    if (trig.Object.Location.Contains(Helper.VtoP(gameHero.Position)))
                    {
                        int layer = 0;
                        if (trig.Object.Properties.Contains("Layer")) layer = Convert.ToInt16(trig.Object.Properties["Layer"]);
                        if (gameHero.Layer == layer)
                        {
                            AtValve = true;
                        }
                    }
                }
                else
                {
                    trig.Position += trig.Speed;
                    trig.Speed.Y += 0.1f;
                    trig.Speed.X -=0.02f;
                    if (trig.Position.Y > (trig.Object.Location.Y + trig.Object.Location.Height) - 40f)
                    {
                        trig.Speed.Y = -(trig.Speed.Y / 2);
                        trig.Position += trig.Speed;
                    }

                    trig.Speed = Vector2.Clamp(trig.Speed, new Vector2(0, -10f), new Vector2(2f, 10f));
                }
            }
        }

        public void DeactivateValve(Hero gameHero)
        {
            if (!AtValve) return;

            foreach (Trigger trig in valves)
            {
                if (!trig.HasTriggered)
                    if (trig.Object.Location.Contains(Helper.VtoP(gameHero.Position)))
                    {
                        int layer = 0;
                        if (trig.Object.Properties.Contains("Layer")) layer = Convert.ToInt16(trig.Object.Properties["Layer"]);
                        if (gameHero.Layer == layer)
                        {
                            trig.HasTriggered = true;
                            if (trig.Object.Properties.Contains("Master")) gameHero.Complete = true;
                        }
                    }
            }
        }

        public void Reset()
        {
            foreach (Trigger t in triggers) t.HasTriggered = false;
            foreach (Trigger v in valves)
            {
                v.HasTriggered = false;
                v.Position = new Vector2(v.Object.Location.Center.X, v.Object.Location.Center.Y);
                v.Speed = new Vector2(3f, 0.1f);
            }

            WaterTriggered = false;
        }

        public void DrawValves(SpriteBatch sb, int layer, Texture2D tex, Color col, bool sil, Hero gameHero)
        {
            foreach (Trigger v in valves.Where(valve => Convert.ToInt16(valve.Object.Properties["Layer"]) == layer))
            {
                sb.Draw(tex, v.Position, new Rectangle(gameHero.usingValve?rand.Next(2) * (tex.Width/2):0, sil?tex.Height/2:0,tex.Width/2,tex.Height/2), col, 0f, new Vector2(tex.Width, tex.Height) / 4, 1f, SpriteEffects.None, 0); //v.Position
            }
        }

        void ActivateTrigger(int num)
        {
            switch (num)
            {
                case 1:
                    PromptController.Instance.AddPrompt("intro1", PromptController.PromptType.Text, "This is Gerde.", false, 0, 0);
                    PromptController.Instance.AddPrompt("intro2", PromptController.PromptType.Text, "Move her with the cursor keys.", false, 0, 1000);
                    PromptController.Instance.AddPrompt("intro3", PromptController.PromptType.Image, "move", false, 0, 1000);
                    break;
                case 2:
                    PromptController.Instance.RemovePrompt("intro1");
                    PromptController.Instance.RemovePrompt("intro2");
                    PromptController.Instance.RemovePrompt("intro3");

                    break;
                case 3:
                    PromptController.Instance.AddPrompt("jump1", PromptController.PromptType.Text, "Gerde can jump.", false, 0, 0);
                    PromptController.Instance.AddPrompt("jump2", PromptController.PromptType.Image, "jump", false, 0, 0);
                    break;
                case 4:
                    PromptController.Instance.RemovePrompt("jump1");
                    PromptController.Instance.RemovePrompt("jump2");
                    break;
                case 5:
                    PromptController.Instance.AddPrompt("climb1", PromptController.PromptType.Text, "Gerde can grab and climb onto ledges.", false, 0, 0);
                    PromptController.Instance.AddPrompt("climb2", PromptController.PromptType.Image, "climb", false, 0, 0);

                    break;
                case 6:
                    PromptController.Instance.RemovePrompt("climb1");
                    PromptController.Instance.RemovePrompt("climb2");
                    break;
                case 7:
                    PromptController.Instance.AddPrompt("crouch1", PromptController.PromptType.Text, "Gerde can crawl to fit through small spaces.", false, 0, 0);
                    PromptController.Instance.AddPrompt("crouch2", PromptController.PromptType.Image, "crawl", false, 0, 0);

                    break;
                case 8:
                    PromptController.Instance.RemovePrompt("crouch1");
                    PromptController.Instance.RemovePrompt("crouch2");
                    break;
                case 9:
                    PromptController.Instance.AddPrompt("story1", PromptController.PromptType.Text, "Gerde is sad.", true, 7000, 0);
                    PromptController.Instance.AddPrompt("story2", PromptController.PromptType.Text, "The flood is coming, and her world is dying.", true, 6000, 1000);
                    PromptController.Instance.AddPrompt("story3", PromptController.PromptType.Text, "The water level is rising fast.", true, 5000, 2000);
                    break;
                case 10:
                    PromptController.Instance.AddPrompt("story4", PromptController.PromptType.Text, "Gerde is part of an advanced civilization.", true, 7000, 0);
                    PromptController.Instance.AddPrompt("story5", PromptController.PromptType.Text, "Among other accomplishments, they have mastered", true, 6000, 1000);
                    PromptController.Instance.AddPrompt("story6", PromptController.PromptType.Text, "the science of teleportation.", true, 6000, 1000);
                    PromptController.Instance.AddPrompt("teleport1", PromptController.PromptType.Text, "Gerde can use the teleporters.", false, 0, 8000);
                    PromptController.Instance.AddPrompt("teleport2", PromptController.PromptType.Image, "use", false, 0, 8000);
                    break;
                case 11:
                    PromptController.Instance.RemovePrompt("story4");
                    PromptController.Instance.RemovePrompt("story5");
                    PromptController.Instance.RemovePrompt("story6");
                    PromptController.Instance.RemovePrompt("teleport1");
                    PromptController.Instance.RemovePrompt("teleport2");
                    break;
                case 12:
                    PromptController.Instance.RemovePrompt("story1");
                    PromptController.Instance.RemovePrompt("story2");
                    PromptController.Instance.RemovePrompt("story3");
                    break;
                case 13:
                    PromptController.Instance.AddPrompt("story7", PromptController.PromptType.Text, "Yet despite their technological prowess,", true, 7000, 0);
                    PromptController.Instance.AddPrompt("story8", PromptController.PromptType.Text, "Gerde's people cannot reverse the damage", true, 6000, 1000);
                    PromptController.Instance.AddPrompt("story9", PromptController.PromptType.Text, "that they have wrought on their world.", true, 6000, 1000);
                    break;
                case 14:
                    PromptController.Instance.AddPrompt("story10", PromptController.PromptType.Text, "The water is rising.", true, 10000, 0);
                    PromptController.Instance.AddPrompt("story11", PromptController.PromptType.Text, "And only Gerde can stop it.", true, 8000, 2000);
                    WaterTriggered = true;
                    Hud.Instance.ShowingWater = true;
                    break;
                case 15:
                    PromptController.Instance.AddPrompt("story12", PromptController.PromptType.Text, "Gerde can buy her people precious time.", true, 7000, 0);
                    PromptController.Instance.AddPrompt("story13", PromptController.PromptType.Text, "There are valves that will stem the tide.", true, 5000, 2000);
                    Hud.Instance.ShowingSouls = true;
                    break;
                case 16:
                    PromptController.Instance.AddPrompt("valve1", PromptController.PromptType.Text, "Gerde can open drainage valves.", false, 0, 0);
                    PromptController.Instance.AddPrompt("valve2", PromptController.PromptType.Image, "use", false, 0, 0);
                    break;

                case 17:
                    PromptController.Instance.RemovePrompt("valve1");
                    PromptController.Instance.RemovePrompt("valve2");
                    PromptController.Instance.AddPrompt("story14", PromptController.PromptType.Text, "Turning off the valves will give Gerde's", true, 7000, 0);
                    PromptController.Instance.AddPrompt("story15", PromptController.PromptType.Text, "people time to escape, minimizing casualties.", true, 7000, 0);
                    break;

                case 18:
                    PromptController.Instance.AddPrompt("story16", PromptController.PromptType.Text, "Gerde hopes she can reach the Master Valve", true, 7000, 0);
                    PromptController.Instance.AddPrompt("story17", PromptController.PromptType.Text, "before many more perish.", true, 7000, 0);
                    break;

                case 19:
                    PromptController.Instance.AddPrompt("story18", PromptController.PromptType.Text, "Gerde seems to recall that there are", true, 7000, 0);
                    PromptController.Instance.AddPrompt("story19", PromptController.PromptType.Text, "two more valves to find.", true, 7000, 0);
                    break;

                case 20:
                    PromptController.Instance.AddPrompt("story20", PromptController.PromptType.Text, "Gerde allows herself a flicker of hope", true, 7000, 0);
                    PromptController.Instance.AddPrompt("story21", PromptController.PromptType.Text, "before returning to the task at hand.", true, 7000, 0);
                    break;

                case 21:
                    PromptController.Instance.AddPrompt("story22", PromptController.PromptType.Text, "Just a few more steps...", true, 7000, 0);
                    break;

                
            }
        }

    }
}
