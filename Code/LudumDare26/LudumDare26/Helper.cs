using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare26
{
    public static class Helper
    {
        public static float AngleBetween(Vector2 v1, Vector2 v2)
        {
            v1.Normalize();
            v2.Normalize();
            float Angle = (float)Math.Acos(Vector2.Dot(v1, v2));
            return Angle;
        }

        public static float TurnToFace(Vector2 position, Vector2 faceThis,
            float currentAngle, float direction, float turnSpeed)
        {
            // consider this diagram:
            //         C 
            //        /|
            //      /  |
            //    /    | y
            //  / o    |
            // S--------
            //     x
            // 
            // where S is the position of the spot light, C is the position of the cat,
            // and "o" is the angle that the spot light should be facing in order to 
            // point at the cat. we need to know what o is. using trig, we know that
            //      tan(theta)       = opposite / adjacent
            //      tan(o)           = y / x
            // if we take the arctan of both sides of this equation...
            //      arctan( tan(o) ) = arctan( y / x )
            //      o                = arctan( y / x )
            // so, we can use x and y to find o, our "desiredAngle."
            // x and y are just the differences in position between the two objects.
            float x = (faceThis.X - position.X) * direction;
            float y = (faceThis.Y - position.Y) * direction;

            // we'll use the Atan2 function. Atan will calculates the arc tangent of 
            // y / x for us, and has the added benefit that it will use the signs of x
            // and y to determine what cartesian quadrant to put the result in.
            // http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
            float desiredAngle = (float)Math.Atan2(y, x);

            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return desiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            float difference = WrapAngle(desiredAngle - currentAngle);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            return WrapAngle(currentAngle + difference);
        }

        /// <summary>
        /// Returns the angle expressed in radians between -Pi and Pi.
        /// </summary>
        public static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }

        public static Vector2 PointOnCircle(ref Vector2 C, int R, float A)
        {
            //A = A - 90;
            float endX = (C.X + (R * ((float)Math.Cos((float)A))));
            float endY = (C.Y + (R * ((float)Math.Sin((float)A))));
            return new Vector2(endX, endY);
        }

        public static Vector2 AngleToVector(float angle, float length)
        {
            Vector2 direction = Vector2.Zero;
            direction.X = (float)Math.Cos(angle) * length;
            direction.Y = (float)Math.Sin(angle) * length;
            return direction;
        }

        public static float V2ToAngle(Vector2 direction)
        {
            return (float)Math.Atan2(direction.Y, direction.X);
        }

        public static Vector2 PtoV(Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static Point VtoP(Vector2 v)
        {
            return new Point((int)v.X, (int)v.Y);
        }

        public static void ShadowText(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color col, Vector2 off, float scale)
        {
            sb.DrawString(font, text, pos + (Vector2.One * 2f), new Color(0, 0, 0, col.A), 0f, off, scale, SpriteEffects.None, 1);
            sb.DrawString(font, text, pos, col, 0f, off, scale, SpriteEffects.None, 1);
        }
    }
}

