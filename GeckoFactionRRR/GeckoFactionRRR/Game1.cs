using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DPSF;

namespace GeckoFactionRRR
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        BloomComponent bloom;
        int bloomSettingsIndex = 0;

        //  Global Variables    
        //  Change in time between frames
        static float dt;

        //  Used for state machine
        float _currentTime = 0;
        //int _state = 0;

        //  Used to get keyboard input
        KeyboardState previousKbState;
        KeyboardState kbState = Keyboard.GetState();

        //  Cameras
        ChaseCamera globalCamera;
        ChaseCamera carPreviewCamera;

        //  List of all game objects
        List<IGameObject> gameObjects = new List<IGameObject>();
        List<Player> playerList = new List<Player>();
        TrackManager trackManager;
        Flag flag;
        HUDBase hud;

        SpriteFont _spr_font;
        int _total_frames = 0;
        float _elapsed_time = 0.0f;
        float totalTime = 0f;
        int _fps = 0;

        int AspectRatio = 0;

        // World in which the physics simulation runs
        //public Space space;

        MouseState mouseState;
        MouseState previousMouseState;

        //  Constants
        int NUM_PLAYERS = 4;
        int ControlScheme = 0;

        // Time until Race start
        const float COUNTDOWN_TIME = 30;

        // Extra 5 seconds for 'time up'
        const float CD_TIMEUP = 5;

        //  Cap Z axis to stop random lurching
        //  forward during countdown
        const float MENU_SPEED_CAP = 0;

        const int KO_SCORE = 500;

        //stuff
        Bomb bomb;

        List<BasePowerUp> powerUpList;

        //Menu sprites
        SpriteFont playerFont;
        SpriteFont instructionFont;

        // DPSF Particle test
        ParticleSystemManager _particleSystemManager = new ParticleSystemManager();
        TrailParticleSystem trailP1 = null;
        TrailParticleSystem trailP2 = null;
        TrailParticleSystem trailP3 = null;
        TrailParticleSystem trailP4 = null;

        TrailParticleSystem trailP1LeftRocket = null;
        TrailParticleSystem trailP2LeftRocket = null;
        TrailParticleSystem trailP3LeftRocket = null;
        TrailParticleSystem trailP4LeftRocket = null;

        TrailParticleSystem trailP1RightRocket = null;
        TrailParticleSystem trailP2RightRocket = null;
        TrailParticleSystem trailP3RightRocket = null;
        TrailParticleSystem trailP4RightRocket = null;
        //ExplosionSmokeTrailsParticleSystem bombExplosion = null;
        ExplosionShockwaveParticleSystem bombExplosion = null;

        // How often the Particle Systems should be updated (zero = update as often as possible)
        const int PARTICLE_SYSTEM_UPDATES_PER_SECOND = 60;

        // Static Particle Settings
        float _staticParticleTimeStep = 1.0f / 30.0f;	// The Time Step between the drawing of each frame of the Static Particles (1 / # of fps, example, 1 / 30 = 30fps).
        float _staticParticleTotalTime = 3.0f;			// The number of seconds that the Static Particle should be drawn over.
        const int TRAIL_START_SIZE_NORMAL = 20;
        const int TRAIL_START_SIZE_BOOSTING = 50;

        //Skybox
        Skybox skybox;

        //Biome
        Biome city;
        Biome sun;
        Biome bouncy;
        Biome rapture;

        public float GameTimeInSeconds;

        public int raceNumber = 0;
        List<Race> raceList = new List<Race>();

        // Debug controls
        Keys skipKey;
        Keys restartKey;
        Keys quitKey;
        Buttons skipButton;
        Buttons restartButton;
        Buttons quitButton;

        Player OverallWinner;
        bool HasVetoed = false;

        const float CHANGE_MENU_BIOME_TIMER = 35f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if ARCADE
            IsMouseVisible = false;
#else
            IsMouseVisible = true;
#endif
            applyGeneralSettings(GetSettingsFromFile("Content/Settings/config.txt", "General"));
            double newRatio = (double)graphics.PreferredBackBufferWidth / (double)graphics.PreferredBackBufferHeight;
            AspectRatio = (int)Math.Truncate(newRatio * 100);

            bloom = new BloomComponent(this);
            Components.Add(bloom);
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


            mouseState = Mouse.GetState();
            previousMouseState = mouseState;

            SoundManager.Initialize(this.Content);
            GameState.Initialize(GameState.States.PRE_START_SCREEN);//START);

            //  Load settings
            //LoadSettingsFromFile("Content/Settings/config.txt", "General");
            applyAudioSettings(GetSettingsFromFile("Content/Settings/config.txt", "Sound"));

            // Set gamepad debug keys
            SetDebugControls(GetSettingsFromFile("Content/Settings/config.txt", "Gamepad Debug Keys"));

#if ARCADE            
            SetDebugControls(GetSettingsFromFile("Content/Settings/config.txt", "Arcade Debug Keys"));
#else
            SetDebugControls(GetSettingsFromFile("Content/Settings/config.txt", "Keyboard Debug Keys"));
#endif

            //  DEBUG
            DebugShapeRenderer.Initialize(GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Initialise the physics simulation
            //space = new Space();
            //space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);


            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //  Game objects
            _spr_font = Content.Load<SpriteFont>("Fonts/Segoe20");

            //Loading Menu sprites
            playerFont = Content.Load<SpriteFont>("Fonts/powerclear");
            instructionFont = Content.Load<SpriteFont>("Fonts/Segoe17");


            //Populate biome sprites
            List<Texture2D> cityList = new List<Texture2D>();
            cityList.Add(this.Content.Load<Texture2D>("Materials/Scenery/skyscraper"));
            cityList.Add(this.Content.Load<Texture2D>("Materials/Scenery/comtower"));
            List<Texture2D> sunList = new List<Texture2D>();
            sunList.Add(this.Content.Load<Texture2D>("Materials/Scenery/fireball"));
            sunList.Add(this.Content.Load<Texture2D>("Materials/Scenery/starburst"));
            List<Texture2D> bouncyList = new List<Texture2D>();
            bouncyList.Add(this.Content.Load<Texture2D>("Materials/Scenery/balloon"));
            bouncyList.Add(this.Content.Load<Texture2D>("Materials/Scenery/spring"));
            List<Texture2D> raptureList = new List<Texture2D>();
            raptureList.Add(this.Content.Load<Texture2D>("Materials/Scenery/skyscraper_ruined"));
            raptureList.Add(this.Content.Load<Texture2D>("Materials/Scenery/mountain"));



            // Create Biomes
            city = new Biome(globalCamera, cityList,
                new Color(0, 15, 30), new Color(0, 255, 200), new Color(0, 255, 255));
            sun = new Biome(globalCamera, sunList,
                new Color(100, 50, 0), new Color(255, 100, 0), new Color(255, 0, 0));
            bouncy = new Biome(globalCamera, bouncyList,
                new Color(50, 0, 0), new Color(255, 50, 200), new Color(255, 50, 200));
            rapture = new Biome(globalCamera, raptureList,
                new Color(10, 10, 10), new Color(200, 100, 100), new Color(100, 0, 0));

            //  Holds and handles all the track sections
            trackManager = new TrackManager(city, this, GraphicsDevice, graphics, "", Content);

            //  HUD necessaties
            hud = new HUDBase(this.Content.Load<Texture2D>("HUD Elements/BoostBarOutline"), Content, GraphicsDevice, graphics, 1.0f);
            hud.playerList = playerList;
            hud.window = this.Window;


            // test
            //scaletest = new ScalePowerUp(playerList, GraphicsDevice, graphics, "Content/Models/Power Ups/tempPowerUp.txt", Content);
            //physicstest = new PhysicsPowerUp(playerList, GraphicsDevice, graphics, "Content/Models/Power Ups/tempPowerUp.txt", Content);
            //thrusttest = new ThrustPowerUp(playerList, GraphicsDevice, graphics, "Content/Models/Power Ups/tempPowerUp.txt", Content);
            //terraintest = new TerrainPowerUp(trackManager, playerList, GraphicsDevice, graphics, "Content/Models/Power Ups/tempPowerUp.txt", Content);

            bomb = new Bomb(GraphicsDevice, graphics, /*null,*/ "Content/Models/bomb.txt", Content);

            //  List of key bindings
            string[] kbControls;
            string gamepadControls;

            //  Retrieve the appropriate key bindings
            switch (ControlScheme)
            {
                case 1:
                    kbControls = GetKBControls(GetSettingsFromFile("Content/Settings/config.txt", "Keyboard Keys"));
                    break;
                case 2:
                    kbControls = GetKBControls(GetSettingsFromFile("Content/Settings/config.txt", "Arcade Keys"));
                    break;
                default:
                    kbControls = new string[] { "Q,W,E", "Z,X,C", "M,OemComma,OemPeriod", "P,OemOpenBrackets,OemCloseBrackets" };
                    break;
            }
            gamepadControls = GetGamePadControls(GetSettingsFromFile("Content/Settings/config.txt", "Gamepad Keys"));

            gameObjects.Add(trackManager);

            //  Create all the players
            for (int i = 0; i < NUM_PLAYERS; i++)
            {
                playerList.Add(new Player(i + 1, GraphicsDevice, graphics, "", Content));
                //gameObjects.Add(playerList[i]);

                //  Set up key bindings
                playerList[i].SetKeys(kbControls[i], gamepadControls);                 
            }

            flag = new Flag(GraphicsDevice, graphics, null, "Content/Models/Car/flag.txt", Content);

            globalCamera = new ChaseCamera((float)(0.25 * Math.PI),
                (float)graphics.GraphicsDevice.Viewport.Width /
                (float)graphics.GraphicsDevice.Viewport.Height,
                1.0f, 15000.0f, playerList[0].Position, Vector3.Up);
            globalCamera.window = this.Window;
            globalCamera.playerList = playerList;
            globalCamera.track = trackManager;


            // Set up Skybox
            skybox = new Skybox(this.Content.Load<Texture2D>("Materials/Skybox/skybox"), globalCamera, Window);


            carPreviewCamera = new ChaseCamera((float)(0.25 * Math.PI),
                (float)graphics.GraphicsDevice.Viewport.Width /
                (float)graphics.GraphicsDevice.Viewport.Height,
                1.0f, 15000.0f, playerList[0].Position, Vector3.Up);
            carPreviewCamera.window = this.Window;
            carPreviewCamera.playerList = playerList;
            carPreviewCamera.track = trackManager;

            trackManager.camera = globalCamera;

            //  Add all objects to the list of gameObjects
            gameObjects.Add(globalCamera);
            //gameObjects.Add(carPreviewCamera);
            gameObjects.Add(flag);

            powerUpList = new List<BasePowerUp>();

            trackManager.PlayerList = playerList;

            bomb.playerList = playerList;
            #region particles
            if (playerList.Count >= 1)
            {
                trailP1 = new TrailParticleSystem(this);
                trailP1.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP1.TrailStartColor = playerList[0].playerColor;
                trailP1.TrailEndColor = Color.Yellow;//playerList[0].playerColor;
                trailP1.Enabled = false;

                trailP1LeftRocket = new TrailParticleSystem(this);
                trailP1LeftRocket.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP1LeftRocket.LoadParticleSystemShort();
                trailP1LeftRocket.TrailStartColor = playerList[0].playerColor;
                trailP1LeftRocket.TrailEndColor = Color.Yellow;
                trailP1LeftRocket.Enabled = false;

                trailP1RightRocket = new TrailParticleSystem(this);
                trailP1RightRocket.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP1RightRocket.LoadParticleSystemShort();
                trailP1RightRocket.TrailStartColor = playerList[0].playerColor;
                trailP1RightRocket.TrailEndColor = Color.Yellow;
                trailP1RightRocket.Enabled = false;

                _particleSystemManager.AddParticleSystem(trailP1);
                _particleSystemManager.AddParticleSystem(trailP1LeftRocket);
                _particleSystemManager.AddParticleSystem(trailP1RightRocket);
            }
            if (playerList.Count >= 2)
            {
                trailP2 = new TrailParticleSystem(this);
                trailP2.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP2.TrailStartColor = playerList[1].playerColor;
                trailP2.TrailEndColor = Color.RoyalBlue;//Color.Yellow;//playerList[1].playerColor;
                trailP2.Enabled = false;

                trailP2LeftRocket = new TrailParticleSystem(this);
                trailP2LeftRocket.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP2LeftRocket.LoadParticleSystemShort();
                trailP2LeftRocket.TrailStartColor = playerList[1].playerColor;
                trailP2LeftRocket.TrailEndColor = Color.RoyalBlue;
                trailP2LeftRocket.Enabled = false;

                trailP2RightRocket = new TrailParticleSystem(this);
                trailP2RightRocket.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP2RightRocket.LoadParticleSystemShort();
                trailP2RightRocket.TrailStartColor = playerList[1].playerColor;
                trailP2RightRocket.TrailEndColor = Color.RoyalBlue;
                trailP2RightRocket.Enabled = false;

                _particleSystemManager.AddParticleSystem(trailP2);
                _particleSystemManager.AddParticleSystem(trailP2LeftRocket);
                _particleSystemManager.AddParticleSystem(trailP2RightRocket);
            }
            if (playerList.Count >= 3)
            {
                trailP3 = new TrailParticleSystem(this);
                trailP3.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP3.TrailStartColor = playerList[2].playerColor;
                trailP3.TrailEndColor = Color.LightBlue;//Color.Yellow;//playerList[2].playerColor;
                trailP3.Enabled = false;

                trailP3LeftRocket = new TrailParticleSystem(this);
                trailP3LeftRocket.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP3LeftRocket.LoadParticleSystemShort();
                trailP3LeftRocket.TrailStartColor = playerList[2].playerColor;
                trailP3LeftRocket.TrailEndColor = Color.LightBlue;
                trailP3LeftRocket.Enabled = false;

                trailP3RightRocket = new TrailParticleSystem(this);
                trailP3RightRocket.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP3RightRocket.LoadParticleSystemShort();
                trailP3RightRocket.TrailStartColor = playerList[2].playerColor;
                trailP3RightRocket.TrailEndColor = Color.LightBlue;
                trailP3RightRocket.Enabled = false;

                _particleSystemManager.AddParticleSystem(trailP3);
                _particleSystemManager.AddParticleSystem(trailP3LeftRocket);
                _particleSystemManager.AddParticleSystem(trailP3RightRocket);
            }
            if (playerList.Count >= 4)
            {
                trailP4 = new TrailParticleSystem(this);
                trailP4.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP4.TrailStartColor = playerList[3].playerColor;
                trailP4.TrailEndColor = Color.SeaGreen;//Color.Yellow;//playerList[3].playerColor;
                trailP4.Enabled = false;

                trailP4LeftRocket = new TrailParticleSystem(this);
                trailP4LeftRocket.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP4LeftRocket.LoadParticleSystemShort();
                trailP4LeftRocket.TrailStartColor = playerList[3].playerColor;
                trailP4LeftRocket.TrailEndColor = Color.SeaGreen;
                trailP4LeftRocket.Enabled = false;

                trailP4RightRocket = new TrailParticleSystem(this);
                trailP4RightRocket.AutoInitialize(GraphicsDevice, Content, spriteBatch);
                trailP4RightRocket.LoadParticleSystemShort();
                trailP4RightRocket.TrailStartColor = playerList[3].playerColor;
                trailP4RightRocket.TrailEndColor = Color.SeaGreen;
                trailP4RightRocket.Enabled = false;

                _particleSystemManager.AddParticleSystem(trailP4);
                _particleSystemManager.AddParticleSystem(trailP4LeftRocket);
                _particleSystemManager.AddParticleSystem(trailP4RightRocket);
            }
            _particleSystemManager.Enabled = false;
            //bombExplosion = new ExplosionSmokeTrailsParticleSystem(this);
            bombExplosion = new ExplosionShockwaveParticleSystem(this);
            bombExplosion.AutoInitialize(GraphicsDevice, Content, spriteBatch);
            //bombExplosion.Enabled = false;
            //bombExplosion.Emitter.EmitParticlesAutomatically = false;
            _particleSystemManager.AddParticleSystem(bombExplosion);
            #endregion

            #region old
            /*
            trailP1 = new TrailParticleSystem(this);
            trailP1.AutoInitialize(GraphicsDevice, Content, spriteBatch);
            trailP1.TrailStartColor = playerList[0].playerColor;
            trailP2 = new TrailParticleSystem(this);
            trailP2.AutoInitialize(GraphicsDevice, Content, spriteBatch);
            trailP2.TrailStartColor = playerList[1].playerColor;
            trailP3 = new TrailParticleSystem(this);
            trailP3.AutoInitialize(GraphicsDevice, Content, spriteBatch);
            trailP3.TrailStartColor = playerList[2].playerColor;
            trailP4 = new TrailParticleSystem(this);
            trailP4.AutoInitialize(GraphicsDevice, Content, spriteBatch);
            trailP4.TrailStartColor = playerList[3].playerColor;

            trailP1.TrailStartColor = playerList[0].playerColor;//Color.White;
            trailP2.TrailStartColor = playerList[1].playerColor;//Color.White;
            trailP3.TrailStartColor = playerList[2].playerColor;//Color.White;
            trailP4.TrailStartColor = playerList[3].playerColor;//Color.White;

            trailP1.TrailEndColor = playerList[0].playerColor;
            trailP2.TrailEndColor = playerList[1].playerColor;
            trailP3.TrailEndColor = playerList[2].playerColor;
            trailP4.TrailEndColor = playerList[3].playerColor;

            _particleSystemManager.AddParticleSystem(trailP1);
            _particleSystemManager.AddParticleSystem(trailP2);
            _particleSystemManager.AddParticleSystem(trailP3);
            _particleSystemManager.AddParticleSystem(trailP4);
            */
            #endregion

            _particleSystemManager.UpdatesPerSecond = PARTICLE_SYSTEM_UPDATES_PER_SECOND;

            //icon = this.Content.Load<Texture2D>("GameThumbnail");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            _particleSystemManager.DestroyAllParticleSystems();
        }

        //  Check if cars are colliding with anything each session
        void CheckCollisions(float time)
        {
            _currentTime += time;
            //  Collision detection
            for (int i = 0; i < playerList.Count; i++)
            {
                Car car = playerList[i].myCar;

                //  If car not alive, don't check collisions
                if (!car.IsAlive)
                {
                    continue;
                }

                //  Check for other cars
                for (int j = i + 1; j < playerList.Count; j++)
                {
                    Car otherCar = playerList[j].myCar;

                    //  If other car not alive, don't check collisions
                    if (!otherCar.IsAlive)
                    {
                        continue;
                    }

                    if (GameState.Current != GameState.States.MENU_TIMEOUT)
                    {
                        //  Check collision with other cars
                        if (car.carCollisions(otherCar))
                        {
                            //otherCar.carCollisions(car);

                            car.SetLastHit(otherCar);
                            otherCar.SetLastHit(car);

                            if (_currentTime >= 1)
                            {
                                if (car.hasFlag)
                                {
                                    car.hasFlag = false;
                                    flag.SetParent(otherCar);
                                }
                                else if (otherCar.hasFlag)
                                {
                                    otherCar.hasFlag = false;
                                    flag.SetParent(car);
                                }

                                if (car.hasBomb)
                                {
                                    bomb.SetParent(playerList[j]);
                                }
                                else if (otherCar.hasBomb)
                                {
                                    bomb.SetParent(playerList[i]);
                                }
                                //playerList[i].currentBoost += 10;
                                _currentTime = 0;
                            }
                        }
                    }
                }

                //  Check with obstacles

                //  Collision with track
                trackManager.CheckCarCollisions(car);

                // Check if past finish line
                if (CurrentRace.CheckCollisions(playerList[i]))
                {
                    GameState.Current = GameState.States.PRE_VICTORY_LAP;
                    skybox.FadeOn(false);
                }
            }
        }

        //  
        void PlayerKOCheck()
        {

            for (int i = 0; i < playerList.Count; i++)
            {
                if (/*playerList[i].car.LastCollidedPlyrID != 0 &&*/ playerList[i].myCar.HasDied)
                {
                    globalCamera.Shake(10f, .5f);
                    playerList[i].score -= KO_SCORE;
                    bombExplosion.ShockwaveSize = 1500;
                    bombExplosion.ShockwaveColor = playerList[i].playerColor;
                    bombExplosion.Emitter.PositionData.Position = new Vector3(playerList[i].myCar.Position.X, playerList[i].myCar.Position.Y, playerList[i].myCar.Position.Z + 300);
                    bombExplosion.Emitter.BurstParticles = 3;
                    if (trackManager.CurrentBiome == rapture)
                    {
                        playerList[i].score -= KO_SCORE;
                    }
                    playerList[i].myCar.HasDied = false;
                    //hud.PlayerKOd()

                    if (playerList[i].myCar.LastCollidedPlyrID != 0)
                    {
                        int index = playerList[i].myCar.LastCollidedPlyrID - 1;
                        playerList[index].score += KO_SCORE;
                        if (trackManager.CurrentBiome == rapture)
                        {
                            playerList[index].score += KO_SCORE;
                        }
                        //hud.gainpoints()
                    }
                }
            }
        }

        //  Function that randomises the player who holds the flag
        void setFlagRandomPlayer()
        {
            //Random rand = new Random();
            // selected player outofbounds for playerlist maximum
            int selectedPlayer = 5;
            int capTest = 0;

            do
            {
                selectedPlayer = GlobalRandom.Next(0, playerList.Count);
                if (playerList[selectedPlayer].myCar.IsAlive)
                {
                    flag.SetParent(playerList[selectedPlayer].myCar);
                }
                capTest++;
            }
            while (!playerList[selectedPlayer].myCar.IsAlive && capTest <=10 );

                foreach (Player player in playerList)
                {
                    if (selectedPlayer != 5)
                    {
                        if (player != playerList[selectedPlayer])
                        {
                            player.myCar.hasFlag = false;
                        }
                    }
                    else
                    {
                        player.myCar.hasFlag = false;
                        flag.parentCar = null;
                    }
                }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            //  Exit the program when the esc key is hit
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(quitButton)
                || kbState.IsKeyDown(quitKey))
            {
                this.Exit();
            }

            // TODO: Add your update logic here
            dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            previousKbState = kbState;
            kbState = Keyboard.GetState();

            _elapsed_time += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            totalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            GameTimeInSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //1 second has passed
#if DEBUG
            if (_elapsed_time >= 1000.0f)
            {
                _fps = _total_frames;
                _total_frames = 0;
                _elapsed_time = 0;
            }
#endif
            

            switch (GameState.Current)
            {
                // Game initialise
                case GameState.States.PRE_START_SCREEN:
                    trackManager.initialize(raceNumber);
                    trackManager.setBiome(city);
                    trackManager.GenerateTrackModel();
                    globalCamera.initialize();
                    globalCamera.Position = trackManager.TrackPoints[GlobalRandom.Next(5, trackManager.TrackPoints.Count - 5)].Position;
                    globalCamera.chasePosition = globalCamera.Position;
                    flag.initialize();
                    bomb.initialize();
                    hud.initialise();
                    _particleSystemManager.Enabled = false;
                    raceNumber = 0;
                    raceNumber++;
                    raceList.Add(new Race(this, trackManager, GraphicsDevice, graphics, raceNumber));
                    foreach (Player Pr in playerList)
                    {
                        Pr.GenerateCar(GraphicsDevice, graphics, Content);
                        Pr.myCar.InitialiseEmitter(Pr.playerID, Content, GraphicsDevice);
                        Pr.myCar.AffectedByGravity = false;
                        PlacePlayersInSpawn(Pr.playerID);
                    }

                    for (int i = 18; i < trackManager.TrackPoints.Count; i += 9)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            PlacePowerupsBySpawn(j + 1, i);
                        }
                    }

                    skybox.FadeOn(true);

                    GameState.Current = GameState.States.START_SCREEN;

                    break;

                case GameState.States.START_SCREEN:
                    _currentTime += dt;
                    hud.update(dt);

                    for (int i = 0; i < playerList.Count; i++)
                    {
                        if(playerList[i].boostPressed)
                        {
                            _currentTime = 0f;
                            GameState.Current = GameState.States.START;
                            break;
                        }
                        SoundManager.SetMusic(SoundManager.MusicTrack.TitleMusic, true, 1.0f, false);
                    }


                    #region Menu Biome Change
                    if (_currentTime >= CHANGE_MENU_BIOME_TIMER)
                    {
                        int ran = GlobalRandom.Next(1, 4);
                        Biome newBiome;

                        switch (ran)
                        {
                            case 0:
                                newBiome = city;
                                trackManager.setBiome(newBiome);
                                for (int i = 0; i < playerList.Count; i++)
                                {
                                    playerList[i].myCar.IsBouncy = false;
                                    playerList[i].myCar.ConstantFullBoost = false;
                                }
                                break;
                            case 1:
                                newBiome = bouncy;
                                trackManager.setBiome(newBiome);
                                for (int i = 0; i < playerList.Count; i++)
                                {
                                    playerList[i].myCar.IsBouncy = true;
                                    playerList[i].myCar.ConstantFullBoost = false;
                                }
                                break;
                            case 2:
                                newBiome = sun;
                                trackManager.setBiome(newBiome);
                                for (int i = 0; i < playerList.Count; i++)
                                {
                                    playerList[i].myCar.IsBouncy = false;
                                    playerList[i].myCar.ConstantFullBoost = true;
                                }
                                break;
                            case 3:
                                newBiome = rapture;
                                trackManager.setBiome(newBiome);
                                for (int i = 0; i < playerList.Count; i++)
                                {
                                    playerList[i].myCar.IsBouncy = false;
                                    playerList[i].myCar.ConstantFullBoost = false;
                                }
                                break;
                            default:
                                break;
                        }
                        _currentTime = 0f;
                    }
                    #endregion
                    break;

                case GameState.States.START:
                    _currentTime = 0;
                    raceNumber = 0;

                    foreach (Player Pr in playerList)
                    {
                        //Pr.GenerateCar(GraphicsDevice, graphics, Content);
                        //Pr.myCar.InitialiseEmitter(Pr.playerID, Content, GraphicsDevice);
                        Pr.myCar.AffectedByGravity = false;
                        PlacePlayersInSpawn(Pr.playerID);
                    }

                    powerUpList.Clear();

                    GameState.Current = GameState.States.NEW_RACE;
                    break;

                case (GameState.States.NEW_RACE):

                    _currentTime += dt;

                    SoundManager.DisposeAll();
                    trackManager.initialize(raceNumber);
                    trackManager.setBiome(city);
                    for (int i = 0; i < playerList.Count; i++)
                    {
                        playerList[i].myCar.IsBouncy = false;
                        playerList[i].myCar.ConstantFullBoost = false;
                    }
                    flag.initialize();
                    bomb.initialize();
                    hud.initialise();

                    globalCamera.initialize();
                    carPreviewCamera.initialize();
                    carPreviewCamera.CurrentState = Camera.States.CAR_PREVIEW;

                    if (raceNumber == 0)
                    {
                        GameState.Current = GameState.States.MENU;

                        foreach (Player Pr in playerList)
                        {
                            Pr.ResetTotalScore();
                        }
                    }
                    else
                    {
                        GameState.Current = GameState.States.PRE_MENU_TIMEOUT;
                    }

                    foreach (Player player in playerList)
                    {
                        player.IsAlive = true;
                        player.score = 0;
                        player.myCar.currentBoost = 100;
                        //Pr.GenerateCar(GraphicsDevice, graphics, Content);
                        //player.myCar.InitialiseEmitter(player.playerID, Content, GraphicsDevice);
                        player.myCar.initialize();
                        player.myCar.BuildCollisionModels();

                        player.myCar.IsDeadTimer = 0f;
                        player.myCar.NeedsRespawn = false;
                        player.myCar.HasDied = false;
                        player.myCar.IsAlive = true;
                        player.IsAlive = true;
                        player.myCar.BoostInvincibility = true;
                        player.myCar.currentBInvTime = 0f;
                        player.myCar.LastCollidedPlyrID = 0;
                        player.myCar.LCPCurrentTime = 0;
                        player.myCar.Velocity = Vector3.Zero;
                        PlacePlayersInSpawn(player.playerID);
                    }

                    raceNumber++;
                    raceList.Add(new Race(this, trackManager, GraphicsDevice, graphics, raceNumber));

                    for (int i = 18; i < trackManager.TrackPoints.Count; i += 9)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            PlacePowerupsBySpawn(j + 1, i);
                        }
                    }
                    hud.race = CurrentRace;

                    // Turn the skybox back on5
                    skybox.FadeOn(true);

                    _currentTime = 0;
                    break;

                // Menu and vehicle/terrain generation
                case (GameState.States.MENU):
                    OverallWinner = null;

                    // Play the menu music
                    if (HasVetoed)
                    {
                        SoundManager.SetMusic(SoundManager.MusicTrack.MenuCountDown30, false, 1.0f, true);
                        HasVetoed = false;
                    }
                    else
                    {
                        SoundManager.SetMusic(SoundManager.MusicTrack.MenuCountDown30, false, 1.0f, false);
                    }

                    _currentTime += dt;

                    // How many players have vetoes the current track
                    int vetoCount = 0;

                    #region Disable Trails in Menu
                    if (playerList.Count >= 1)
                    {
                        if (trailP1.Enabled)
                        {
                            trailP1.Enabled = false;
                        }
                    }
                    if (playerList.Count >= 2)
                    {
                        if (trailP2.Enabled)
                        {
                            trailP2.Enabled = false;
                        }
                    }
                    if (playerList.Count >= 3)
                    {
                        if (trailP3.Enabled)
                        {
                            trailP3.Enabled = false;
                        }
                    }
                    if (playerList.Count >= 4)
                    {
                        if (trailP4.Enabled)
                        {
                            trailP4.Enabled = false;
                        }
                    }
                    #endregion

                    //cap forward speed during menu
                    foreach (Player player in playerList)
                    {
                        if (player.myCar.Velocity.Length() > MENU_SPEED_CAP)
                        {
                            player.myCar.Velocity = Vector3.Normalize(player.myCar.Velocity) * MENU_SPEED_CAP;
                        }

                        if (player.vetoesTrack)
                        {
                            vetoCount++;
                        }
                    }

                    // If everyone vetoes, start again
                    if (vetoCount == playerList.Count())
                    {
                        raceNumber = 0;
                        GameState.Current = GameState.States.RACE_OVER;
                        HasVetoed = true;

                        _currentTime = 0;
                    }

                    hud.timer = COUNTDOWN_TIME - _currentTime;

                    if (_currentTime >= COUNTDOWN_TIME
                        || kbState.IsKeyDown(skipKey)
                        || GamePad.GetState(PlayerIndex.One).IsButtonDown(skipButton))
                    {
                        GameState.Current = GameState.States.PRE_MENU_TIMEOUT;

                        for (int i = 0; i < playerList.Count; i++)
                        {
                            PlacePlayersInSpawn(i + 1);
                        }

                        _currentTime = 0;
                    }

                    carPreviewCamera.update(dt);
                    break;

                case GameState.States.PRE_MENU_TIMEOUT:
                    for (int i = 0; i < playerList.Count; i++)
                    {
                        playerList[i].myCar.Velocity = Vector3.Zero;
                        playerList[i].myCar.Acceleration = Vector3.Zero;
                    }

                    trackManager.GenerateTrackModel();

                    _particleSystemManager.Enabled = true;
                    GameState.Current = GameState.States.MENU_TIMEOUT;
                    break;

                // Buffer between menu and the race start
                case GameState.States.MENU_TIMEOUT:
                    _currentTime += dt;

                    #region Enable Trail Particles
                    if (playerList.Count >= 1)
                    {
                        if (!trailP1.Enabled)
                        {
                            trailP1.Enabled = true;
                            trailP1LeftRocket.Enabled = true;
                            trailP1RightRocket.Enabled = true;
                        }
                    }
                    if (playerList.Count >= 2)
                    {
                        if (!trailP2.Enabled)
                        {
                            trailP2.Enabled = true;
                            trailP2LeftRocket.Enabled = true;
                            trailP2RightRocket.Enabled = true;
                        }
                    }
                    if (playerList.Count >= 3)
                    {
                        if (!trailP3.Enabled)
                        {
                            trailP3.Enabled = true;
                            trailP3LeftRocket.Enabled = true;
                            trailP3RightRocket.Enabled = true;
                        }
                    }
                    if (playerList.Count >= 4)
                    {
                        if (!trailP4.Enabled)
                        {
                            trailP4.Enabled = true;
                            trailP4LeftRocket.Enabled = true;
                            trailP4RightRocket.Enabled = true;
                        }
                    }
                    #endregion

                    bomb.SetPosition(trackManager.TrackPoints[TrackManager.ROLLING_LENGTH * 2].Position + new Vector3(0, bomb.HitSpheres[0].Radius, 0));

                    //  Run collision check and handling
                    CheckCollisions(dt);

                    for (int i = 0; i < playerList.Count; i++)
                    {
                        playerList[i].myCar.Velocity = new Vector3(0, playerList[i].myCar.Velocity.Y, 0);
                    }

                    if (_currentTime >= CD_TIMEUP
                        || (kbState.IsKeyDown(skipKey) && previousKbState.IsKeyUp(skipKey))
                        || GamePad.GetState(PlayerIndex.One).IsButtonDown(skipButton))
                    {
                        // Play the game music
                        SoundManager.SetMusic(SoundManager.MusicTrack.None, true);

                        _currentTime = 0;
                        GameState.Current = GameState.States.START_PLAYING;
                    }

                    break;

                case GameState.States.START_PLAYING:
                    GameState.Current = GameState.States.PLAYING;

                    SoundManager.SetMusic(SoundManager.MusicTrack.ConceptMusic, true, 1.0f, true);

                    for (int i = 0; i < playerList.Count; i++)
                    {
                        //PlacePlayersInSpawn(i + 1);
                    }

                    break;

                // Race running
                case GameState.States.PLAYING:

                    #region Camera Shake Demo
                    //  if the camera should shake (demo functionality)
                    bool shake = false;
                    bool longShake = false;

                    if (kbState.IsKeyDown(Keys.D))
                    {
                        shake = true;
                    }

                    if (kbState.IsKeyDown(Keys.F))
                    {
                        longShake = true;
                    }

                    if (longShake)
                    {
                        globalCamera.Shake(15f, 2f);
                    }

                    // If we're performing a short shake, call the Shake method with a .4 second length
                    else if (shake)
                    {
                        globalCamera.Shake(15f, .4f);
                    }

                    #endregion

                    #region Side Rocket Particle Effects
                    if (playerList.Count >= 1)
                    {
                        if (playerList[0].LeftRocketOn)
                        {
                            trailP1LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[0].RightRocketOn)
                        {
                            trailP1RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    if (playerList.Count >= 2)
                    {
                        if (playerList[1].LeftRocketOn)
                        {
                            trailP2LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[1].RightRocketOn)
                        {
                            trailP2RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    if (playerList.Count >= 3)
                    {
                        if (playerList[2].LeftRocketOn)
                        {
                            trailP3LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[2].RightRocketOn)
                        {
                            trailP3RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    if (playerList.Count >= 4)
                    {
                        if (playerList[3].LeftRocketOn)
                        {
                            trailP4LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[3].RightRocketOn)
                        {
                            trailP4RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    #endregion

                    //  If no one has the flag, start a countdown
                    if (flag.parentCar == null)
                    {
                        _currentTime += dt;

                        //  Give the flag to a random player after the countdown
                        if (_currentTime >= 5)
                        {
                            setFlagRandomPlayer();

                            if (flag.parentCar == null || !flag.parentCar.IsAlive)
                            {
                                setFlagRandomPlayer();
                            }

                            _currentTime = 0;
                        }
                    }
                    //  If someone does have the flag, check if they're alive
                    else if (!flag.parentCar.IsAlive)
                    {
                        //  If they're dead, remove the flag
                        flag.parentCar.hasFlag = false;
                        flag.parentCar = null;
                        setFlagRandomPlayer();
                    }

                    if (bomb.parentPlayer != null && !bomb.parentPlayer.IsAlive)
                    {
                        bomb.parentPlayer.myCar.hasBomb = false;
                        bomb.parentPlayer = null;
                        bomb.initialize();
                    }

                    if (bomb.HasExploded)
                    {
                        //bombExplosion.Emitter.BurstTime = 1;
                        bombExplosion.ShockwaveSize = 300;
                        bombExplosion.Emitter.BurstParticles = 3;
                        globalCamera.Shake(15f, .4f);
                    }

                    if (bomb.FinishedExploding)
                    {
                        bomb.initialize();
                        if (trackManager.TrackPoints.Count > trackManager.CurrentCheckPoint.ID + 10)
                        {
                            bomb.SetPosition(trackManager.TrackPoints[trackManager.CurrentCheckPoint.ID + 10].Position
                                + new Vector3((float)(GlobalRandom.NextDouble() * 200 - GlobalRandom.NextDouble() * 200), bomb.HitSpheres[0].Radius, 0));
                        }
                        bomb.FinishedExploding = false;
                    }

                    if ((globalCamera.Position.Z - bomb.Position.Z) >= 750)
                    {
                        if (trackManager.TrackPoints.Count > trackManager.CurrentCheckPoint.ID + 10)
                        {
                            bomb.SetPosition(trackManager.TrackPoints[trackManager.CurrentCheckPoint.ID + 10].Position
                                + new Vector3((float)(GlobalRandom.NextDouble() * 200 - GlobalRandom.NextDouble() * 200), bomb.HitSpheres[0].Radius, 0));
                        }
                    }

                    // Respawn players
                    for (int i = 0; i < playerList.Count; i++)
                    {
                        if (!playerList[i].myCar.IsAlive && playerList[i].HasTouchedControls)
                        {
                            playerList[i].myCar.NeedsRespawn = true;
                        }
                        if (playerList[i].myCar.NeedsRespawn)
                        {
                            playerList[i].myCar.IsDeadTimer += dt;
                            if (playerList[i].myCar.IsDeadTimer >= playerList[i].myCar.DEATH_TIME_UP)
                            {
                                playerList[i].myCar.IsDeadTimer = 0f;
                                playerList[i].myCar.NeedsRespawn = false;
                                playerList[i].myCar.HasDied = false;
                                playerList[i].myCar.IsAlive = true;
                                playerList[i].IsAlive = true;
                                playerList[i].myCar.BoostInvincibility = true;
                                playerList[i].myCar.currentBInvTime = 0f;
                                playerList[i].myCar.LastCollidedPlyrID = 0;
                                playerList[i].myCar.LCPCurrentTime = 0;
                                playerList[i].myCar.Velocity = Vector3.Zero;
                                playerList[i].myCar.Momentum = Vector3.Zero;
                                playerList[i].myCar.Acceleration = Vector3.Zero;
                                PlacePlayersInSpawn(playerList[i].playerID);
                                playerList[i].HasTouchedControls = false;
                            }
                        }
                    }

                    //  If reset button is hit, restart game
                    if (kbState.IsKeyDown(restartKey) || GamePad.GetState(PlayerIndex.One).IsButtonDown(restartButton))
                    {
                        GameState.Current = GameState.States.RACE_OVER;
                    }

                    // Track Color Changing
                    if (kbState.IsKeyDown(Keys.Delete) && previousKbState.IsKeyUp(Keys.Delete))
                    {
                        //trackManager.changeColorRandom();
                        //trackManager.setBiome(bouncy);
                    }
                         
                    //  Run collision check and handling
                    CheckCollisions(dt);

                    //  Check if a player has KO'd an opponent
                    PlayerKOCheck();
                    break;



                case GameState.States.PRE_VICTORY_LAP:
                #region Side Rocket Particle Effects
                    if (playerList.Count >= 1)
                    {
                        if (playerList[0].LeftRocketOn)
                        {
                            trailP1LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[0].RightRocketOn)
                        {
                            trailP1RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    if (playerList.Count >= 2)
                    {
                        if (playerList[1].LeftRocketOn)
                        {
                            trailP2LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[1].RightRocketOn)
                        {
                            trailP2RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    if (playerList.Count >= 3)
                    {
                        if (playerList[2].LeftRocketOn)
                        {
                            trailP3LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[2].RightRocketOn)
                        {
                            trailP3RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    if (playerList.Count >= 4)
                    {
                        if (playerList[3].LeftRocketOn)
                        {
                            trailP4LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[3].RightRocketOn)
                        {
                            trailP4RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    #endregion

                    // Circle the winner of the current race
                    globalCamera.Winner = CurrentRace.CheckWinners();
                    GameState.Current = GameState.States.VICTORY_LAP;

                    for (int i = 0; i < playerList.Count; i++)
                    {
                        playerList[i].AddToTotalScore(playerList[i].score);
                    }

                    _currentTime = 0;

                    break;

                case GameState.States.VICTORY_LAP:
                    #region Side Rocket Particle Effects
                    if (playerList.Count >= 1)
                    {
                        if (playerList[0].LeftRocketOn)
                        {
                            trailP1LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[0].RightRocketOn)
                        {
                            trailP1RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    if (playerList.Count >= 2)
                    {
                        if (playerList[1].LeftRocketOn)
                        {
                            trailP2LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[1].RightRocketOn)
                        {
                            trailP2RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    if (playerList.Count >= 3)
                    {
                        if (playerList[2].LeftRocketOn)
                        {
                            trailP3LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[2].RightRocketOn)
                        {
                            trailP3RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    if (playerList.Count >= 4)
                    {
                        if (playerList[3].LeftRocketOn)
                        {
                            trailP4LeftRocket.Emitter.BurstTime = dt;
                        }
                        if (playerList[3].RightRocketOn)
                        {
                            trailP4RightRocket.Emitter.BurstTime = dt;
                        }
                    }
                    #endregion

                    _currentTime += dt;

                    if (_currentTime > 5)
                    {
                        _currentTime = 0;
                        GameState.Current = GameState.States.RACE_OVER;
                    }
                    break;

                // Race over
                case GameState.States.RACE_OVER:
                    GameState.Current = GameState.States.NEW_RACE;
                    powerUpList.Clear();

                    if (raceNumber >= 3)
                    {
                        _particleSystemManager.Enabled = false;
                        SoundManager.SetMusic(SoundManager.MusicTrack.FinalGameOver, false, 1.0f, false);
                        _currentTime = 0;
                        GameState.Current = GameState.States.OVERALL_WINNER;
                        raceNumber = 0;

                        // Remove all players when the final scoreboard shows
                        for (int i = 0; i < playerList.Count; i++)
                        {
                            if (playerList[i].IsAlive)
                            {
                                playerList[i].myCar.Die(false);
                            }
                        }
                    }

                    break;
                case GameState.States.OVERALL_WINNER:
                    _currentTime += dt;

                    hud.update(dt);

                    if (hud.FinishedOverallAnimation)
                    {
                        _currentTime = 0;
                        GameState.Current = GameState.States.PRE_START_SCREEN;
                    }                    
                    break;

                default:
                    break;
            }

            bool anyCarsAlive = false;
            //  Check if all cars have died
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].myCar != null && playerList[i].myCar.IsAlive)
                {
                    anyCarsAlive = true;
                    break;
                }
            }

            bomb.update(dt);


            for (int i = 0; i < powerUpList.Count; i++)
            {
                powerUpList[i].TotalGameTime = totalTime;
                powerUpList[i].update(dt);
            }

            for (int i = 0; i < playerList.Count; i++)
            {
                playerList[i].myCar.TotalTime = totalTime;
            }

            //  Update all game objects
            foreach (IGameObject go in gameObjects)
            {
                if (go.IsAlive)
                {
                    go.update(dt);
                }
            }

            for (int i = 0; i < playerList.Count; i++)
            {
                playerList[i].update(dt);
            }

            if (raceList.Count > 0)
            {
                CurrentRace.update(dt);
            }

            // Update sound manager
            SoundManager.Update(globalCamera.Position);

            // TEST Chase Camera direction change
            if (kbState.IsKeyDown(Keys.Add))
            {
                globalCamera.ChangeDirection(true);
            }

            #region Trail Particles
            // Particle System updates
            _particleSystemManager.SetCameraPositionForAllParticleSystems(globalCamera.Position);
            _particleSystemManager.SetWorldViewProjectionMatricesForAllParticleSystems(globalCamera.worldMat, globalCamera.viewMat, globalCamera.projMat);

            if (playerList.Count >= 1)
            {
                trailP1.Emitter.PositionData.Position = playerList[0].myCar.Position
                    - (playerList[0].myCar.forwardDirection * playerList[0].myCar.HitSpheres[0].Radius);

                trailP1LeftRocket.Emitter.PositionData.Position = playerList[0].myCar.Position 
                    + new Vector3((playerList[0].myCar.HitSpheres[0].Radius / 2), 0, 0);
                trailP1RightRocket.Emitter.PositionData.Position = playerList[0].myCar.Position
                    - new Vector3((playerList[0].myCar.HitSpheres[0].Radius / 2), 0, 0);

                if (playerList[0].myCar.BoostInvincibility)
                {
                    trailP1.TrailStartSize = TRAIL_START_SIZE_BOOSTING;
                }
                else if (trailP1.TrailStartSize != TRAIL_START_SIZE_NORMAL)
                {
                    trailP1.TrailStartSize = TRAIL_START_SIZE_NORMAL;
                }
            }
            if (playerList.Count >= 2)
            {
                trailP2.Emitter.PositionData.Position = playerList[1].myCar.Position
                    - (playerList[1].myCar.forwardDirection * playerList[1].myCar.HitSpheres[0].Radius);

                trailP2LeftRocket.Emitter.PositionData.Position = playerList[1].myCar.Position
                    + new Vector3((playerList[1].myCar.HitSpheres[0].Radius / 2), 0, 0);
                trailP2RightRocket.Emitter.PositionData.Position = playerList[1].myCar.Position
                    - new Vector3((playerList[1].myCar.HitSpheres[0].Radius / 2), 0, 0);

                if (playerList[1].myCar.BoostInvincibility)
                {
                    trailP2.TrailStartSize = TRAIL_START_SIZE_BOOSTING;
                }
                else if (trailP2.TrailStartSize != TRAIL_START_SIZE_NORMAL)
                {
                    trailP2.TrailStartSize = TRAIL_START_SIZE_NORMAL;
                }
            }
            if (playerList.Count >= 3)
            {
                trailP3.Emitter.PositionData.Position = playerList[2].myCar.Position
                    - (playerList[2].myCar.forwardDirection * playerList[2].myCar.HitSpheres[0].Radius);

                trailP3LeftRocket.Emitter.PositionData.Position = playerList[2].myCar.Position
                    + new Vector3((playerList[2].myCar.HitSpheres[0].Radius / 2), 0, 0);
                trailP3RightRocket.Emitter.PositionData.Position = playerList[2].myCar.Position
                    - new Vector3((playerList[2].myCar.HitSpheres[0].Radius / 2), 0, 0);

                if (playerList[2].myCar.BoostInvincibility)
                {
                    trailP3.TrailStartSize = TRAIL_START_SIZE_BOOSTING;
                }
                else if (trailP3.TrailStartSize != TRAIL_START_SIZE_NORMAL)
                {
                    trailP3.TrailStartSize = TRAIL_START_SIZE_NORMAL;
                }
            }
            if (playerList.Count >= 4)
            {
                trailP4.Emitter.PositionData.Position = playerList[3].myCar.Position
                    - (playerList[3].myCar.forwardDirection * playerList[3].myCar.HitSpheres[0].Radius);

                trailP4LeftRocket.Emitter.PositionData.Position = playerList[3].myCar.Position
                    + new Vector3((playerList[3].myCar.HitSpheres[0].Radius / 2), 0, 0);
                trailP4RightRocket.Emitter.PositionData.Position = playerList[3].myCar.Position
                    - new Vector3((playerList[3].myCar.HitSpheres[0].Radius / 2), 0, 0);

                if (playerList[3].myCar.BoostInvincibility)
                {
                    trailP4.TrailStartSize = TRAIL_START_SIZE_BOOSTING;
                }
                else if (trailP4.TrailStartSize != TRAIL_START_SIZE_NORMAL)
                {
                    trailP4.TrailStartSize = TRAIL_START_SIZE_NORMAL;
                }
            }

            //trailP1.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            //if (trailP1 != null && trailP1.IsInitialized)
            //{
            _particleSystemManager.UpdateAllParticleSystems(GameTimeInSeconds);//(float)gameTime.ElapsedGameTime.TotalSeconds);
            //}
            #endregion

            #region Bomb Particles
            if (bomb.parentPlayer != null)
            {
                bombExplosion.Emitter.PositionData.Position = bomb.parentPlayer.myCar.Position;
                bombExplosion.ShockwaveColor = bomb.parentPlayer.playerColor;
            }
            #endregion

            // TEST Chase Camera direction change
            if (kbState.IsKeyDown(Keys.Add))
            {
                globalCamera.ChangeDirection(true);
            }


            skybox.update(dt);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _total_frames++;

            bloom.BeginDraw();

            GraphicsDevice.Clear(trackManager.CurrentBiome.skyBoxColor * skybox.Opacity);

            //First spritebatch - Background objects
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);

            //if (GameState.Current != GameState.States.VICTORY_LAP)
            {
                skybox.draw(spriteBatch);
            }

            if (GameState.Current == GameState.States.MENU)
            {
                trackManager.drawCurve(spriteBatch, new Vector2(0, Window.ClientBounds.Height / 2),
                    ((float)Window.ClientBounds.Width / trackManager.TrackPoints.Count) / 730);
            }
            spriteBatch.End();



            //Resetting the depth mode
            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            trackManager.drawSprites(GraphicsDevice, globalCamera);

            switch (GameState.Current)
            {
                case (GameState.States.MENU):
                    for (int j = 0; j < playerList.Count; j++)
                    {
                        playerList[j].myCar.draw(GraphicsDevice, carPreviewCamera);
                    }
                    break;

                default:
                    for (int i = 0; i < gameObjects.Count; i++)
                    {
                        gameObjects[i].draw(GraphicsDevice, globalCamera);
                    }
                    for (int i = 0; i < playerList.Count; i++)
                    {
                        playerList[i].draw(GraphicsDevice, globalCamera);
                    }
                    break;
            }

            for (int i = 0; i < powerUpList.Count; i++)
            {
                powerUpList[i].draw(GraphicsDevice, globalCamera);
            }
            
            bomb.draw(GraphicsDevice, globalCamera);

            CurrentRace.draw(GraphicsDevice, globalCamera);
            DebugShapeRenderer.Draw(gameTime, globalCamera.viewMat, globalCamera.projMat);



            _particleSystemManager.DrawAllParticleSystems();


            base.Draw(gameTime);
            // Particle System needs to be called after base draw (maybe)

            // Second spritebatch - Sprites and HUD objects
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            // FPS Counter
#if DEBUG
            spriteBatch.DrawString(_spr_font, "FPS = " + _fps,
                new Vector2(10.0f, 20.0f), Color.White);
#endif

            hud.draw(GraphicsDevice, spriteBatch, playerFont);
            if (GameState.Current == GameState.States.OVERALL_WINNER)
            {
                if (OverallWinner == null)
                {
                    OverallWinner = hud.OverallWinner;
                    globalCamera.Winner = OverallWinner;
                }
            }

            spriteBatch.End();

            // Resetting GraphicsDevice after drawing menu and text
            GraphicsDevice.BlendFactor = Color.White;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        #region Settings

        //  Retrieves the relevant section from the settings file
        //  TODO: Add error checking for files with incorrect syntax
        string[] GetSettingsFromFile(string filename, string section)
        {
            if (filename != "")
            {
                //  File to be read in
                string[] settingsFile = System.IO.File.ReadAllLines(filename);


                //  Read only the lines for given section
                int startIndex = startIndex = Array.IndexOf(settingsFile, "[" + section + "]");
                int endIndex = Array.FindIndex(settingsFile, startIndex + 1
                                                    , element => element.StartsWith("["));

                return settingsFile.Skip(startIndex).Take(endIndex - startIndex).ToArray();
            }

            //  If file does not exist
            return new string[0];
        }

        //  Apply the general game settings
        //  TODO: Add error checking for files with incorrect syntax
        void applyGeneralSettings(string[] settingsFile)
        {
            if (settingsFile.Length > 0)
            {
                //  Setting name and value
                string[] keyPair = new string[2];

                //  Loop through each line in the section
                //for (int i = 0; i < settingsFile.Length; i++)
                foreach (String line in settingsFile)
                {
                    //  If line not empty and not commented and not start of section header
                    if (!line.StartsWith("//")
                            && line != ""
                            && !line.StartsWith("["))
                    {
                        keyPair[0] = "";
                        keyPair[1] = "";
                        //  Seperate the command and value from each line
                        keyPair = line.Split('=');

                        //  Change appropriate setting
                        switch (keyPair[0])
                        {
                            case "NumberOfPlayers":
                                NUM_PLAYERS = int.Parse(keyPair[1]);
                                break;

                            case "GameWindowWidth":
                                SetResolutionWidth(int.Parse(keyPair[1]));
                                break;

                            case "GameWindowHeight":
                                SetResolutionHeight(int.Parse(keyPair[1]));
                                break;

                            case "FullScreen":
                                SetFullScreen(bool.Parse(keyPair[1]));
                                break;

                            case "VSync":
                                graphics.SynchronizeWithVerticalRetrace = bool.Parse(keyPair[1]);
                                break;

                            case "FixedTimeStep":
                                this.IsFixedTimeStep = bool.Parse(keyPair[1]);
                                break;

                            case "ControlScheme":
                                SetControlScheme(int.Parse(keyPair[1]));
                                break;


                            default:
                                break;
                        }
                    }
                }
            }
        }

        //  Apply the sound settings
        void applyAudioSettings(string[] settingsFile)
        {
            float master = 1.0f;
            float fx = 1.0f;
            float music = 1.0f;

            if (settingsFile.Length > 0)
            {
                //  Setting name and value
                string[] keyPair = new string[2];

                //  Loop through each line in the section
                //for (int i = 0; i < settingsFile.Length; i++)
                foreach (String line in settingsFile)
                {
                    //  If line not empty and not commented and not start of section header
                    if (!line.StartsWith("//")
                            && line != ""
                            && !line.StartsWith("["))
                    {
                        keyPair[0] = "";
                        keyPair[1] = "";
                        //  Seperate the command and value from each line
                        keyPair = line.Split('=');

                        //  Change appropriate setting
                        switch (keyPair[0])
                        {
                            case "MasterVolume":
                                master = float.Parse(keyPair[1]);
                                break;

                            case "FXVolume":
                                fx = float.Parse(keyPair[1]);
                                break;

                            case "MusicVolume":
                                music = float.Parse(keyPair[1]);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }

            SoundManager.ApplyVolumeLevels(master, fx, music);
        }

        string[] GetKBControls(string[] settingsFile)
        {
            if (settingsFile.Length > 0)
            {
                //  Controls for all 4 players
                string[] playerControls = new string[4];
                //  Setting name and value
                string[] keyPair = new string[2];

                //  Loop through each line in the section
                foreach (String line in settingsFile)
                {
                    //  If line not empty and not commented and not start of section header
                    if (!line.StartsWith("//")
                            && line != ""
                            && !line.StartsWith("["))
                    {
                        keyPair[0] = "";
                        keyPair[1] = "";
                        //  Seperate the command and value from each line
                        keyPair = line.Split('=');

                        //  Change appropriate setting
                        switch (keyPair[0])
                        {
                            case "P1":
                                playerControls[0] = keyPair[1];
                                break;

                            case "P2":
                                playerControls[1] = keyPair[1];
                                break;

                            case "P3":
                                playerControls[2] = keyPair[1];
                                break;
                            case "P4":
                                playerControls[3] = keyPair[1];
                                break;

                            default:
                                break;
                        }
                    }
                }

                return playerControls;
            }

            //  If empty file
            return new string[0];
        }

        //  Retrieves gamepad controls
        string GetGamePadControls(string[] settingsFile)
        {
            if (settingsFile.Length > 0)
            {
                //  Controls for all 4 players
                string playerControls = "";
                //  Setting name and value
                string[] keyPair = new string[2];

                //  Loop through each line in the section
                foreach (String line in settingsFile)
                {
                    //  If line not empty and not commented and not start of section header
                    if (!line.StartsWith("//")
                            && line != ""
                            && !line.StartsWith("["))
                    {
                        keyPair[0] = "";
                        keyPair[1] = "";
                        //  Seperate the command and value from each line
                        keyPair = line.Split('=');

                        //  Change appropriate setting
                        switch (keyPair[0])
                        {
                            case "Boost":
                                playerControls = keyPair[1];
                                break;

                            default:
                                break;
                        }
                    }
                }

                return playerControls;
            }

            //  If empty file
            return "B";
        }

        void SetDebugControls(string[] settingsFile)
        {
            if (settingsFile.Length > 0)
            {
                //  Setting name and value
                string[] keyPair = new string[2];

                //  Loop through each line in the section
                foreach (String line in settingsFile)
                {
                    //  If line not empty and not commented and not start of section header
                    if (!line.StartsWith("//")
                            && line != ""
                            && !line.StartsWith("["))
                    {
                        keyPair[0] = "";
                        keyPair[1] = "";
                        //  Seperate the command and value from each line
                        keyPair = line.Split('=');

                        //  Change appropriate setting
                        switch (keyPair[0])
                        {
                            case "Skip":
                                skipKey = (Keys)Enum.Parse(typeof(Keys), keyPair[1], true);
                                break;

                            case "Restart":
                                restartKey = (Keys)Enum.Parse(typeof(Keys), keyPair[1], true);
                                break;

                            case "Quit":
                                quitKey = (Keys)Enum.Parse(typeof(Keys), keyPair[1], true);
                                break;

                            case "GPSkip":
                                skipButton = (Buttons)Enum.Parse(typeof(Buttons), keyPair[1], true);
                                break;

                            case "GPRestart":
                                restartButton = (Buttons)Enum.Parse(typeof(Buttons), keyPair[1], true);
                                break;

                            case "GPQuit":
                                quitButton = (Buttons)Enum.Parse(typeof(Buttons), keyPair[1], true);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        // Sets the keyboard control scheme
        // If arcade build, set to arcade scheme
        // Else set to config file
        void SetControlScheme(int value)
        {
#if ARCADE
            ControlScheme = 2;
#else
            ControlScheme = value;
#endif
        }

        // Sets the full screen setting
        // Arcade, full screen is always on
        // Else pull setting from config file
        void SetFullScreen(bool isOn)
        {
#if ARCADE
            graphics.IsFullScreen = true;
#else
            graphics.IsFullScreen = isOn;

            if (graphics.IsFullScreen)
            {
                IsMouseVisible = false;
            }
            else
            {
                IsMouseVisible = true;
            }

#endif
        }

        void SetResolutionWidth(int width)
        {
#if ARCADE
            graphics.PreferredBackBufferWidth = 1024;
#else
            graphics.PreferredBackBufferWidth = width;
#endif
        }

        void SetResolutionHeight(int height)
        {
#if ARCADE
            graphics.PreferredBackBufferHeight = 768;
#else
            graphics.PreferredBackBufferHeight = height;
#endif
        }
        #endregion

        // Run when the game closes
        protected override void OnExiting(object sender, EventArgs args)
        {
            // Clean up all loaded audio instances
            SoundManager.OnQuit();

            base.OnExiting(sender, args);
        }

        void PlacePlayersInSpawn(int ID)
        {
            //  Create all the players
            const float START_X_POS = -60f;
            float startXPos = 60f;
            const float START_Y_POS = 50f;
            const float START_Z_TOP_POS = 25f;
            const float START_Z_BOTTOM_POS = 35f;
            const float SPAWN_GAP = 50f;

            startXPos -= (SPAWN_GAP * (ID - 1));
            Player player = playerList[ID - 1];

            float MENU_TOP_Y = 0;
            float MENU_BOTTOM_Y = 0;
            float MENU_RIGHT_X = 0;
            float MENU_LEFT_X = 0;

            // Change menu countdown positions based on screen resolution
            switch (AspectRatio)
            {
                // 16:9
                case (177):
                    MENU_TOP_Y = 35f;
                    MENU_BOTTOM_Y = 0f;
                    MENU_RIGHT_X = 65f;
                    MENU_LEFT_X = -65;
                    break;
                // 4:3
                case (133):
                    MENU_TOP_Y = 40f;
                    MENU_BOTTOM_Y = 0f;
                    MENU_RIGHT_X = 50f;
                    MENU_LEFT_X = -50;
                    break;
                // 3:2
                case (150):
                    MENU_TOP_Y = 40f;
                    MENU_BOTTOM_Y = 0f;
                    MENU_RIGHT_X = 50f;
                    MENU_LEFT_X = -50;
                    break;

                // 16:10
                case (160):
                    MENU_TOP_Y = 35f;
                    MENU_BOTTOM_Y = 0f;
                    MENU_RIGHT_X = 55f;
                    MENU_LEFT_X = -55;

                    break;

                default:
                    MENU_TOP_Y = 40f;
                    MENU_BOTTOM_Y = 0f;
                    MENU_RIGHT_X = 50f;
                    MENU_LEFT_X = -50;
                    break;
            }

            switch (GameState.Current)
            {
                case GameState.States.MENU:
                    // If odd, place car on left
                    if (ID % 2 == 1)
                    {
                        if (ID < 3)
                        {
                            player.Position = new Vector3(MENU_LEFT_X, MENU_TOP_Y, START_Z_TOP_POS);
                        }
                        else
                        {
                            player.Position = new Vector3(MENU_LEFT_X, MENU_BOTTOM_Y, START_Z_BOTTOM_POS);
                        }
                    }
                    else
                    {
                        if (ID < 3)
                        {
                            player.Position = new Vector3(MENU_RIGHT_X, MENU_TOP_Y, START_Z_TOP_POS);
                        }
                        else
                        {
                            player.Position = new Vector3(MENU_RIGHT_X, MENU_BOTTOM_Y, START_Z_BOTTOM_POS);
                        }
                    }
                    break;

                case GameState.States.MENU_TIMEOUT:
                    //  Place cars along starting line
                    player.Position = new Vector3(startXPos, START_Y_POS, START_Z_TOP_POS);
                    break;

                case GameState.States.PLAYING:
                    int pointID = trackManager.CurrentCheckPoint.ID;

                    if (pointID == trackManager.TrackPoints.Count - 1)
                    {
                        pointID-=2;
                    }

                    TrackPoint trackPoint = trackManager.TrackPoints[pointID];
                    Vector3[] positions = trackPoint.GetOutsidePoints(START_X_POS + (SPAWN_GAP * (ID - 1)));
                    Vector3 halfwayToNext = trackManager.TrackPoints[pointID + 1].Position - trackManager.TrackPoints[pointID].Position;
                    halfwayToNext /= 2;

                    //  Place cars along starting line
                    player.Position = positions[0] + (Vector3.Up * 100) + halfwayToNext;
                    player.myCar.angle = trackPoint.Angle;
                    break;

                default:
                    //  Place cars along starting line
                    player.Position = new Vector3(startXPos, START_Y_POS, START_Z_TOP_POS);
                    break;
            }

            player.myCar.SetPosition(player.Position);
        }

        Race CurrentRace
        {
            get
            {
                return raceList[raceList.Count - 1];
            }
        }

        void PlacePowerupsBySpawn(int ID, int trackpointID)
        {
            //  Create all the players
            const float START_X_POS = -100f;
            float startXPos = 70f;
            const float SPAWN_GAP = 70f;

            startXPos -= (SPAWN_GAP * (ID - 1));
            BasePowerUp powerup = null;

            int powerupSwitch = GlobalRandom.Next(0, 4);

            switch (powerupSwitch)
            {
                case 0:
                    powerup = new PhysicsPowerUp(playerList, GraphicsDevice, graphics, "Content/Models/Power Ups/tempPowerUp.txt", Content);
                    break;
                case 1:
                    powerup = new ScalePowerUp(playerList, GraphicsDevice, graphics, "Content/Models/Power Ups/tempPowerUp.txt", Content);
                    break;
                case 2:
                    powerup = new TerrainPowerUp(trackManager, playerList, GraphicsDevice, graphics, "Content/Models/Power Ups/tempPowerUp.txt", Content);
                    powerup.City = city;
                    powerup.Bouncy = bouncy;
                    powerup.Sun = sun;
                    powerup.Rapture = rapture;
                    break;
                case 3:
                    powerup = new ThrustPowerUp(playerList, GraphicsDevice, graphics, "Content/Models/Power Ups/tempPowerUp.txt", Content);
                    break;
                default:
                    break;
            }

            powerUpList.Add(powerup);


            TrackPoint trackPoint = trackManager.TrackPoints[trackpointID];
            Vector3[] positions = trackPoint.GetOutsidePoints(START_X_POS + (SPAWN_GAP * (ID - 1)));

            powerup.Position = positions[0] + (Vector3.Up * 15);
            powerup.BuildCollisionModels();
            powerup.SetPosition(powerup.Position);
        }
    }
}
