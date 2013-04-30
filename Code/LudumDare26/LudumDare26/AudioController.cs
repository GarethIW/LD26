using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;


namespace LudumDare26
{
    public static class AudioController
    {
        public static float sfxvolume = 1f;
        public static float musicvolume = 0.5f;

        public static Random randomNumber = new Random();

        public static Dictionary<string, SoundEffect> effects;

        public static Dictionary<string, SoundEffectInstance> songs;

        static string playingTrack = "";
        static bool isPlaying;

        public static string currentlyPlaying = "";

        public static int currentTrack = 0;

        public static void LoadContent(ContentManager content)
        {
            effects = new Dictionary<string, SoundEffect>();

            effects.Add("teleport_in", content.Load<SoundEffect>("sfx/teleport_in"));
            effects.Add("teleport_out", content.Load<SoundEffect>("sfx/teleport_out"));
            effects.Add("fstep-grass", content.Load<SoundEffect>("sfx/fstep-grass"));
            effects.Add("fstep-metal", content.Load<SoundEffect>("sfx/fstep-metal"));
            effects.Add("fstep-wood", content.Load<SoundEffect>("sfx/fstep-wood"));
            effects.Add("valve", content.Load<SoundEffect>("sfx/valve"));
            effects.Add("water", content.Load<SoundEffect>("sfx/water"));
            effects.Add("splash", content.Load<SoundEffect>("sfx/splash"));
            




            songs = new Dictionary<string, SoundEffectInstance>();
            //songs.Add("0", content.Load<SoundEffect>("music/1").CreateInstance());
            //songs.Add("1", content.Load<SoundEffect>("music/2").CreateInstance());
            //songs.Add("2", content.Load<SoundEffect>("music/3").CreateInstance());
            //songs.Add("3", content.Load<SoundEffect>("music/4").CreateInstance());
            //songs.Add("4", content.Load<SoundEffect>("music/5").CreateInstance());
        }

        public static void LoadMusic(string piece, ContentManager content)
        {
            //if (currentlyPlaying.ToLower() == piece.ToLower()) return;
            //currentlyPlaying = piece;

            //if (!MediaPlayer.GameHasControl) return;

            //if (MediaPlayer.State != MediaState.Stopped) MediaPlayer.Stop();
            ////if (musicInstance != null)
            ////{
            ////    musicInstance.Dispose();
            ////}

            //musicInstance = content.Load<Song>("audio/music/" + piece);
            //MediaPlayer.IsRepeating = true;
            //// MediaPlayer.Volume = musicvolume;
            //MediaPlayer.Play(musicInstance);

            //if (!OptionsMenuScreen.music) MediaPlayer.Pause();
        }

        public static void PlayMusic()
        {
            PlayMusic(currentTrack.ToString());
            currentTrack++;
            if (currentTrack == 5) currentTrack = 0;
        }

        public static void PlayMusic(string track)
        {
            playingTrack = track;
            isPlaying = true;
            songs[track].IsLooped = true;
            songs[track].Volume = 0f;
            songs[track].Play();
        }

        public static void StopMusic()
        {

            isPlaying = false;
        }

        public static void ToggleMusic()
        {

            //if (OptionsMenuScreen.music)
            //{
            //    MediaPlayer.Resume();
            //}
            //else
            //    MediaPlayer.Pause();
        }

        public static void PlaySFX(string name)
        {
            //if (OptionsMenuScreen.sfx)
            effects[name].Play(sfxvolume, 0f, 0f);
        }
        public static void PlaySFX(string name, float pitch)
        {
            //if (OptionsMenuScreen.sfx)
            effects[name].Play(sfxvolume, pitch, 0f);
        }
        public static void PlaySFX(string name, float volume, float pitch, float pan)
        {
            // if (OptionsMenuScreen.sfx)
            if (pan < -1f || pan > 1f) return;
            volume = MathHelper.Clamp(volume, 0f, 1f);
            effects[name].Play(volume * sfxvolume, pitch, pan);
        }
        public static void PlaySFX(string name, float minpitch, float maxpitch)
        {
            // if (OptionsMenuScreen.sfx)
            effects[name].Play(sfxvolume, minpitch + ((float)randomNumber.NextDouble() * (maxpitch - minpitch)), 0f);
        }

        internal static void PlaySFX(string name, float volume, float minpitch, float maxpitch, Vector2 Position)
        {
            //Vector2 screenPos = Vector2.Transform(Position, GameManager.Camera.CameraMatrix);
            //float pan = MathHelper.Clamp((screenPos.X - (GameManager.Camera.Width / 2)) / (GameManager.Camera.Width / 2), -1f, 1f);
            //effects[name].Play(volume * sfxvolume, minpitch + ((float)randomNumber.NextDouble() * (maxpitch - minpitch)), pan);
        }


        public static void Update(GameTime gameTime)
        {

            if (playingTrack == "") return;

            if (isPlaying)
                if (songs[playingTrack].Volume < musicvolume) songs[playingTrack].Volume += 0.01f;

            if (!isPlaying)
                if (songs[playingTrack].Volume > 0) songs[playingTrack].Volume -= 0.01f;
                else songs[playingTrack].Stop();

            // if (MediaPlayer.Volume > musicvolume) MediaPlayer.Volume = musicvolume;
        }

        public static void Unload()
        {

        }




    }
}
