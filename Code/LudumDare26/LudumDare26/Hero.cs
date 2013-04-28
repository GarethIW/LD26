using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Spine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TiledLib;

namespace LudumDare26
{
    class Hero
    {
        public Vector2 Position;
        public Vector2 Speed;

        public int Layer = 0;

        public float Scale = 0.6f;

        Vector2 gravity = new Vector2(0f, 0.25f);

        Rectangle collisionRect = new Rectangle(0, 0, 85, 150);

        Texture2D blankTex;

        SkeletonRenderer skeletonRenderer;
        Skeleton skeleton;

        Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();

        float animTime;

        int faceDir = 1;

        bool walking = false;
        bool jumping = false;
        bool crouching = false;
        bool falling = false;
        bool grabbed = false;
        bool climbing = false;

        bool teleporting = false;
        int teleportingDir = 0;
        float teleportScale = 1f;
        public bool teleportFinished = false;

        bool oppositeDirPushed = false;

        bool justUngrabbed = false;

        Vector2 grabbedPosition;

        public Hero(Vector2 spawnPosition)
        {
            Position = spawnPosition;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            blankTex = content.Load<Texture2D>("blank");

            skeletonRenderer = new SkeletonRenderer(graphicsDevice);
            Atlas atlas = new Atlas(graphicsDevice, Path.Combine(content.RootDirectory, "spinegirl.atlas"));
            SkeletonJson json = new SkeletonJson(atlas);
            skeleton = new Skeleton(json.readSkeletonData("spinegirl", File.ReadAllText(Path.Combine(content.RootDirectory, "spinegirl.json"))));
            skeleton.SetSkin("default");
            skeleton.SetSlotsToBindPose();
            Animations.Add("walk", skeleton.Data.FindAnimation("walk"));
            Animations.Add("jump", skeleton.Data.FindAnimation("jump"));
            Animations.Add("crawl", skeleton.Data.FindAnimation("crawl"));
            Animations.Add("fall", skeleton.Data.FindAnimation("fall"));
            Animations.Add("grab", skeleton.Data.FindAnimation("grab"));
            Animations.Add("climb", skeleton.Data.FindAnimation("climb"));

            skeleton.RootBone.X = Position.X;
            skeleton.RootBone.Y = Position.Y;
            skeleton.RootBone.ScaleX = Scale;
            skeleton.RootBone.ScaleY = Scale;

            skeleton.UpdateWorldTransform();
        }

        public void Update(GameTime gameTime, Camera gameCamera, Map gameMap)
        {
            if (!walking && !jumping && !crouching && !grabbed)
            {
                skeleton.SetToBindPose();
            }

            if (walking && !jumping && !grabbed)
            {
                animTime += gameTime.ElapsedGameTime.Milliseconds / 1000f;
                if (!crouching)
                {
                    Animations["walk"].Mix(skeleton, animTime, true, 0.3f);
                }
                else
                {
                    Animations["crawl"].Mix(skeleton, animTime, true, 0.5f);
                }
            }

            if (jumping)
            {
                animTime += gameTime.ElapsedGameTime.Milliseconds / 1000f;
                Animations["jump"].Mix(skeleton, animTime, false, 0.5f);
            }

            if (crouching && !jumping)
            {
                collisionRect.Width = 100;
                collisionRect.Height = 110;

                if (!walking)
                {
                    animTime = 0;
                    Animations["crawl"].Mix(skeleton, animTime, false, 0.5f);
                }
            }
            else
            {
                collisionRect.Width = 85;
                collisionRect.Height = 150;
            }

            if (falling)
            {
                Speed += gravity;

                if (Speed.Y > 1)
                {
                    animTime += gameTime.ElapsedGameTime.Milliseconds / 1000f;
                    Animations["fall"].Mix(skeleton, animTime, true, 0.75f);
                }
            }

            if (grabbed)
            {
                Position = Vector2.Lerp(Position, grabbedPosition, 0.1f);
                animTime += gameTime.ElapsedGameTime.Milliseconds / 1000f;
                Animations["grab"].Mix(skeleton, animTime, true, 0.3f);
            }

            if (climbing)
            {
                //Position = Vector2.Lerp(Position, grabbedPosition, 0.1f);
                animTime += gameTime.ElapsedGameTime.Milliseconds / 500f;
                Animations["climb"].Apply(skeleton, animTime, false);

                Position = Vector2.Lerp(Position, grabbedPosition - new Vector2(20*(-faceDir), 185), (0.08f/Animations["grab"].Duration) * animTime);

                if ((Position - (grabbedPosition - new Vector2(20 * (-faceDir), 185))).Length() < 5f)
                    climbing = false;
            }

            if (teleporting)
            {
                teleportScale = MathHelper.Lerp(teleportScale, 0f, 0.1f);
                if (teleportScale < 0.02f)
                {
                    Layer = teleportingDir;
                    teleporting = false;
                    teleportFinished = false;
                }
            }
            else
            {
                if(teleportFinished)
                    teleportScale = MathHelper.Lerp(teleportScale, 1f, 0.1f);
            }

            skeleton.RootBone.ScaleX = Scale * teleportScale;
            skeleton.RootBone.ScaleY = Scale * teleportScale;

    
            Position += Speed;
            collisionRect.Location = new Point((int)Position.X - (collisionRect.Width / 2), (int)Position.Y - (collisionRect.Height));
            CheckCollision(gameMap);

            Position.X = MathHelper.Clamp(Position.X, 0, gameMap.Width * gameMap.TileWidth);

            skeleton.RootBone.X = Position.X;
            skeleton.RootBone.Y = Position.Y;
    

            if (faceDir == -1) skeleton.FlipX = true; else skeleton.FlipX = false;

            skeleton.UpdateWorldTransform();

            walking = false;
            oppositeDirPushed = false;
            Speed.X = 0f;
        }

        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Camera gameCamera)
        {
            if (teleportScale < 0.02f) return;

            skeletonRenderer.Begin(gameCamera.CameraMatrix);
            skeletonRenderer.Draw(skeleton);
            skeletonRenderer.End();

            // Draw collision box
            //spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, gameCamera.CameraMatrix);
            //spriteBatch.Draw(blankTex, collisionRect, Color.White * 0.3f);
            //spriteBatch.End();
        }


        public void MoveLeftRight(float dir)
        {
            if (teleporting) return;

            if (grabbed)
            {
                if ((int)dir != faceDir) oppositeDirPushed = true;
            }

            if (grabbed || climbing) return;
            if (dir > 0) faceDir = 1; else faceDir = -1;

            Speed.X = dir * 4f;
            walking = true;
            
        }

        public void UsePortal(Map gameMap)
        {
            if (teleporting) return;

            MapObjectLayer portalLayer = gameMap.GetLayer("Portals") as MapObjectLayer;

            foreach (MapObject o in portalLayer.Objects)
            {
                if (o.Location.Contains(Helper.VtoP(Position)))
                {
                    if (Convert.ToInt16(o.Properties["In"]) == Layer)
                    {
                        teleportingDir = Convert.ToInt16(o.Properties["Out"]);
                        teleporting = true;
                        teleportScale = 1f;
                        return;
                    }
                    if (Convert.ToInt16(o.Properties["Out"]) == Layer)
                    {
                        teleportingDir = Convert.ToInt16(o.Properties["In"]);
                        teleporting = true;
                        teleportScale = 1f;
                        return;
                    }
                }
            }
        }

       

        public void Jump()
        {
            if (teleporting) return;

            if (grabbed && (Position - grabbedPosition).Length()<5f && !oppositeDirPushed)
            {
                climbing = true;
                grabbed = false;
                animTime = 0;
                return;
            }

            if (!jumping && !crouching && !falling && !climbing && (!grabbed || oppositeDirPushed))
            {
                if (oppositeDirPushed)
                {
                    faceDir = -faceDir;
                    Speed.X = (faceDir) * 4f;
                    Position.X += (faceDir * 40f);
                    grabbed = false;
                    jumping = true;
                    animTime = Animations["jump"].Duration * 0.3f;
                }
                else
                {
                    jumping = true;
                    animTime = 0;
                }
                Speed.Y = -9f;
            }
        }

        public void Crouch()
        {
            if (teleporting) return;

            if (grabbed)
            {
                grabbed = false;
                falling = true;
                justUngrabbed = true;
                Position.X += (-faceDir * 40f);
            }
            else
                if(!falling && !climbing && !justUngrabbed) crouching = true;
        }

        void CheckCollision(Map gameMap)
        {
            //collisionRect.Offset(new Point((int)Speed.X, (int)Speed.Y));

            Rectangle? collRect;

            // Check for ledge grabs
            if ((jumping || falling) && !justUngrabbed)
            {
                if (Speed.X<0 && gameMap.CheckTileCollision(new Vector2(collisionRect.Left, collisionRect.Top), Layer))
                    if (!gameMap.CheckTileCollision(new Vector2(collisionRect.Left, collisionRect.Top - gameMap.TileHeight), Layer) &&
                       !gameMap.CheckTileCollision(new Vector2(collisionRect.Left + gameMap.TileWidth, collisionRect.Top - gameMap.TileHeight), Layer) &&
                       !gameMap.CheckTileCollision(new Vector2(collisionRect.Left + gameMap.TileWidth, collisionRect.Top), Layer))
                    {
                        grabbed = true;
                        jumping = false;
                        falling = false;
                        crouching = false;
                        Speed.Y = 0;
                        Speed.X = 0;
                        grabbedPosition = new Vector2((int)(collisionRect.Left / gameMap.TileWidth) * gameMap.TileWidth, (int)(collisionRect.Top / gameMap.TileHeight) * gameMap.TileHeight) + new Vector2(gameMap.TileWidth, collisionRect.Height + 30);
                        faceDir = -1;
                    }

                if (Speed.X > 0 && gameMap.CheckTileCollision(new Vector2(collisionRect.Right, collisionRect.Top), Layer))
                    if (!gameMap.CheckTileCollision(new Vector2(collisionRect.Right, collisionRect.Top - gameMap.TileHeight), Layer) &&
                       !gameMap.CheckTileCollision(new Vector2(collisionRect.Right - gameMap.TileWidth, collisionRect.Top - gameMap.TileHeight), Layer) &&
                       !gameMap.CheckTileCollision(new Vector2(collisionRect.Right - gameMap.TileWidth, collisionRect.Top), Layer))
                    {
                        grabbed = true;
                        jumping = false;
                        falling = false;
                        crouching = false;
                        Speed.Y = 0;
                        Speed.X = 0;
                        grabbedPosition = new Vector2((int)(collisionRect.Right / gameMap.TileWidth) * gameMap.TileWidth, (int)(collisionRect.Top / gameMap.TileHeight) * gameMap.TileHeight) + new Vector2(0, collisionRect.Height + 30);
                        faceDir = 1;
                    }
            }

            if (grabbed || climbing) return;


            
                collRect = CheckCollisionBottom(gameMap);
                if (collRect.HasValue)
                {
                    if (falling)
                    {
                        Speed.Y = 0f;
                        Position.Y -= collRect.Value.Height;
                        collisionRect.Offset(0, -collRect.Value.Height);
                        jumping = false;
                        falling = false;
                        justUngrabbed = false;
                    }
                    
                }
                else
                    falling = true;

                if (Speed.Y < 0f)
                {
                    collRect = CheckCollisionTop(gameMap);
                    if (collRect.HasValue)
                    {
                        Speed.Y = 0f;
                        Position.Y += collRect.Value.Height;
                        collisionRect.Offset(justUngrabbed ? (collRect.Value.Width * (-faceDir)) : 0, collRect.Value.Height);
                        falling = true;
                        jumping = false;
                    }
                }
            
            
            

            if (Speed.X > 0f)
            {
                collRect = CheckCollisionRight(gameMap);
                if (collRect.HasValue)
                {
                    Speed.X = 0f;
                    Position.X -= (collRect.Value.Width);
                    collisionRect.Offset(-collRect.Value.Width, 0);
                }
            }
            if (Speed.X < 0f)
            {
                collRect = CheckCollisionLeft(gameMap);
                if (collRect.HasValue)
                {
                    Speed.X = 0f;
                    Position.X += collRect.Value.Width;
                    collisionRect.Offset(collRect.Value.Width, 0);
                }
            }


            bool collided = false;
            for (int y = -1; y > -15; y--)
            {
                collisionRect.Offset(0, -1);
                collRect = CheckCollisionTop(gameMap);
                if (collRect.HasValue) collided = true;
            }
            if (!collided) crouching = false;

        }

        Rectangle? CheckCollisionTop(Map gameMap)
        {
            for (float x = collisionRect.Left+5; x < collisionRect.Right-5; x += 1)
            {
                Vector2 checkPos = new Vector2(x, collisionRect.Top);
                Rectangle? collRect = gameMap.CheckTileCollisionIntersect(checkPos, collisionRect, Layer);
                if (collRect.HasValue) return collRect;
            }

            return null;
        }
        Rectangle? CheckCollisionBottom(Map gameMap)
        {
            for (float x = collisionRect.Left+5; x < collisionRect.Right-5; x += 1)
            {
                Vector2 checkPos = new Vector2(x, collisionRect.Bottom);
                Rectangle? collRect = gameMap.CheckTileCollisionIntersect(checkPos, collisionRect, Layer);
                if (collRect.HasValue) return collRect;
            }

            return null;
        }
        Rectangle? CheckCollisionRight(Map gameMap)
        {
            for (float y = collisionRect.Top; y < collisionRect.Bottom; y += 1)
            {
                Vector2 checkPos = new Vector2(collisionRect.Right, y);
                Rectangle? collRect = gameMap.CheckTileCollisionIntersect(checkPos, collisionRect, Layer);
                if (collRect.HasValue) return collRect;
            }

            return null;
        }
        Rectangle? CheckCollisionLeft(Map gameMap)
        {
            for (float y = collisionRect.Top; y < collisionRect.Bottom; y += 1)
            {
                Vector2 checkPos = new Vector2(collisionRect.Left, y);
                Rectangle? collRect = gameMap.CheckTileCollisionIntersect(checkPos, collisionRect, Layer);
                if (collRect.HasValue) return collRect;
            }

            return null;
        }
    }
}
