using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace GeckoFactionRRR
{
    public static class SoundManager
    {
        /// <summary>
        ///  Available selection of music tracks
        /// </summary>
        public enum MusicTrack
        {
            None,
            MenuCountDown30,
            MenuCountDown60,
            ConceptMusic,
            FinalGameOver,
            TitleMusic
        }

        #region Sound list
        // sound files
        static SoundEffect silence;
        static SoundEffect menuBeep1;
        static SoundEffect trackPowerUp;
        static SoundEffect scalePowerUp;
        static SoundEffect boostPowerUp;
        static SoundEffect physicsPowerUp;

        // Groups
        static List<SoundEffect> CarCrashes = new List<SoundEffect>();
        static List<SoundEffect> CarDeaths = new List<SoundEffect>();
        static List<SoundEffect> CarBoosts = new List<SoundEffect>();

        // Music
        static Song titleMusic;
        static Song silentMusic;
        static Song menuCountDown60;
        static Song menuCountDown30;
        static Song conceptMusic;
        static Song finalGameOver;
        #endregion

        #region 3D audio
        // Used to adjust the volume based on how far away objects are
        const float WORLD_SCALE = 1000f;
        // Attached to the camera
        static AudioListener listener;
        #endregion

        // Settings
        static float MasterVolume = 1.0f;
        static float FXVolume = 1.0f;
        static float MusicVolume = 1.0f;

        static List<SoundEffectInstance> fxInstances = new List<SoundEffectInstance>();
        // Currently playing background music
        static MusicTrack currentMusicTrack = MusicTrack.None;
        // Whether audio hardware is installed
        static bool SafeToRun = true;


        // Load all the sound files
        public static void Initialize(ContentManager content)
        {
            // Test safe to run/load silent file to sound effect
            TestSafeToRun(content);

            if (SafeToRun)
            {
                try
                {
                    // Load music 
                    silentMusic = content.Load<Song>("Sounds/Music/Silence5");
                    menuCountDown30 = content.Load<Song>("Sounds/Music/RRR_Countdown_30");
                    menuCountDown60 = content.Load<Song>("Sounds/Music/RRR_Countdown_60");
                    conceptMusic = content.Load<Song>("Sounds/Music/RRR_firsttheme_concept");
                    finalGameOver = content.Load<Song>("Sounds/Music/breaks_fx_letmedown_012");
                    titleMusic = content.Load<Song>("Sounds/Music/GeckoFaction_Low");

                    // Load sound effects
                    menuBeep1 = content.Load<SoundEffect>("Sounds/FX/CLICK_S_146721__fins__menu-click");
                    LoadCarCrashes(content);
                    LoadCarDeaths(content);
                    LoadCarBoosts(content);
                    trackPowerUp = content.Load<SoundEffect>("Sounds/FX/PowerUp/breaks_fx_nastymetallicpad");
                    scalePowerUp = content.Load<SoundEffect>("Sounds/FX/PowerUp/breaks_fx_fruitmachine");
                    boostPowerUp = content.Load<SoundEffect>("Sounds/FX/PowerUp/SomethingGoingUp");                    
                    
                    SetMusic(MusicTrack.None, false);
                }
                catch
                {

                    throw new ContentLoadException("Missing sound files");
                }
            }

            listener = new AudioListener();

            SoundEffect.DistanceScale = WORLD_SCALE;
        }

        // Load car car crash sound files
        static void LoadCarCrashes(ContentManager content)
        {
            CarCrashes.Add(content.Load<SoundEffect>("Sounds/FX/Crash/FireEnemy1"));
            CarCrashes.Add(content.Load<SoundEffect>("Sounds/FX/Crash/FireEnemy2"));
            CarCrashes.Add(content.Load<SoundEffect>("Sounds/FX/Crash/FireEnemy3"));
            CarCrashes.Add(content.Load<SoundEffect>("Sounds/FX/Crash/FireEnemy4"));
            CarCrashes.Add(content.Load<SoundEffect>("Sounds/FX/Crash/WhiteDeath1"));
        }

        // Load car death sound files
        static void LoadCarDeaths(ContentManager content)
        {
            CarDeaths.Add(content.Load<SoundEffect>("Sounds/FX/Death/WhiteDeath1"));
            CarDeaths.Add(content.Load<SoundEffect>("Sounds/FX/Death/WhiteDeath2"));
            CarDeaths.Add(content.Load<SoundEffect>("Sounds/FX/Death/WhiteDeath3"));
            CarDeaths.Add(content.Load<SoundEffect>("Sounds/FX/Death/WhiteDeath4"));
        }

        // Load car boost sound files
        static void LoadCarBoosts(ContentManager content)
        {
            //CarBoosts.Add(content.Load<SoundEffect>("Sounds/FX/Boost/WhiteFizzyShrill1"));
            //CarBoosts.Add(content.Load<SoundEffect>("Sounds/FX/Boost/WhiteFizzyShrill2"));
            CarBoosts.Add(content.Load<SoundEffect>("Sounds/FX/Boost/FirePewPew1"));
        }

        /// <summary>
        /// Changes the current background music
        /// </summary>
        /// <param name="newTrack">Which track to play</param>
        /// <param name="loop">Whether to loop the music</param>
        public static void SetMusic(MusicTrack newTrack, bool loop, float volume = 1.0f, bool restart = false)
        {
            if (!SafeToRun) { return; }
            if (newTrack == currentMusicTrack && !restart) { return; }

            DisposeMusic();
            currentMusicTrack = newTrack;

            //Set the volume
            MediaPlayer.Volume = MusicVolume * volume * MasterVolume;
            // Set the loop property
            MediaPlayer.IsRepeating = loop;

            // Set the background music to the new music track
            switch (newTrack)
            {
                case MusicTrack.None:
                    MediaPlayer.Play(silentMusic);
                    break;
                case MusicTrack.MenuCountDown30:
                    MediaPlayer.Play(menuCountDown30);
                    break;
                case MusicTrack.MenuCountDown60:
                    MediaPlayer.Play(menuCountDown30);
                    break;
                case MusicTrack.ConceptMusic:
                    MediaPlayer.Play(conceptMusic);
                    break;
                case MusicTrack.FinalGameOver:
                    MediaPlayer.Play(finalGameOver);
                    break;
                case MusicTrack.TitleMusic:
                    MediaPlayer.Play(titleMusic);
                    break;
                default:
                    break;
            }            
        }        

        // Used for car generation
        public static void PlayMenuBeep(AudioEmitter emitter = null, float volume = 1.0f)
        {
            PlaySoundEffect(menuBeep1, emitter, volume);
        }

        // Used for car on car collisions
        public static void PlayCarCrash(AudioEmitter emitter = null, float volume = 1.0f)
        {
            PlayRandomFromList(CarCrashes, emitter, volume);
        }

        // Used for car on car collisions
        public static void PlayCarDeath(AudioEmitter emitter = null, float volume = 1.0f)
        {
            PlayRandomFromList(CarDeaths, emitter, volume);
        }

        public static void PlayBoost(AudioEmitter emitter = null, float volume = 1.0f)
        {
            PlayRandomFromList(CarBoosts, emitter, volume);
        }

        public static void PlayTrackPowerUp(AudioEmitter emitter = null, float volume = 1.0f)
        {
            PlaySoundEffect(trackPowerUp, emitter, volume);
        }

        public static void PlayScalePowerUp(AudioEmitter emitter = null, float volume = 1.0f)
        {
            PlaySoundEffect(scalePowerUp, emitter, volume);
        }

        public static void PlayBoostPowerUp(AudioEmitter emitter = null, float volume = 1.0f)
        {
            PlaySoundEffect(boostPowerUp, emitter, volume);
        }

        // Used for car bounce on track
        public static void PlayCarHitFloor(AudioEmitter emitter)
        {

        }

        public static void PlayRocketBoost(AudioEmitter emitter)
        {

        }

        public static void PauseAllSounds()
        {
        }

        public static void StopAllSounds()
        {
            
        }

        // Clean up all loaded audio instances
        public static void DisposeAll()
        {
            DisposeMusic();
            DisposeSoundEffects();
        }

        // Update the listener position every frame
        public static void Update(Vector3 position)
        {
            listener.Position = position;
        }

        // Set the volume levels
        public static void ApplyVolumeLevels(float master, float fx, float music)
        {
            MasterVolume = master;
            FXVolume = fx;
            MusicVolume = music;

            SoundEffect.MasterVolume = MasterVolume;
        }

        public static void OnQuit()
        {
            fxInstances = null;

                    silence = null;
        menuBeep1 = null;
          trackPowerUp = null;
          scalePowerUp = null;
          boostPowerUp = null;
          physicsPowerUp = null;

        // Groups
          CarCrashes = null;
          CarDeaths = null;
          CarBoosts = null;
        }

        // Properly dispose of all the sound effect instances
        static void DisposeSoundEffects()
        {
            
            for (int i = 0; i < fxInstances.Count; i++)
            {
                fxInstances[i].Dispose();
            }

            fxInstances.Clear();

            //fxInstances = new List<SoundEffectInstance>();
        }

        // Stop the music
        static void DisposeMusic()
        {
            MediaPlayer.Stop();
        }

        // Play an instance of a sound effect file with 3D positioning
        static void PlaySoundEffect(SoundEffect soundEffect, AudioEmitter emitter = null, float volume = 1.0f)
        {
            // If audio hardware is unavailable, stop
            if (!SafeToRun) { return; }
            
            // Create an instance of the audio file
            fxInstances.Add(soundEffect.CreateInstance());
            SoundEffectInstance instance = fxInstances[fxInstances.Count - 1];


            // Set volume levels
            instance.Volume = FXVolume * volume;
            if (instance.Volume > 1.0f)
            {
                instance.Volume = 1.0f;
            }

            if (emitter != null)
            {
                // Set 3D positioning
                instance.Apply3D(listener, emitter);
            }

            // Play the sound
            instance.Play();            
        }

        static void PlayRandomFromList(List<SoundEffect> list, AudioEmitter emitter = null, float volume = 1.0f)
        {
            // Pick a random sound effect from the list
            int index = GlobalRandom.Next(list.Count - 1);

            // Play that sound effect
            PlaySoundEffect(list[index], emitter, volume);
        }

        // Test whether audio hardware is installed
        static void TestSafeToRun(ContentManager content)
        {
            try
            {
                silence = content.Load<SoundEffect>("Sounds/FX/Silence5");
                PlaySoundEffect(silence);
                DisposeSoundEffects();
            }
            catch
            {
                SafeToRun = false;
            }
        }


    }
}
