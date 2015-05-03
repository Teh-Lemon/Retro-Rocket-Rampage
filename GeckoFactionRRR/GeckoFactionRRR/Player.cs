using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GeckoFactionRRR
{
    class Player : VBGameObjectBasic//IGameObject
    {
        public Car myCar { get; set; }
        public int playerID { get; set; }
        public int score { get; set; }
        bool isPlaying = false;
        public bool vetoesTrack = false;
        public int racePolePosition = 0;
        public bool LeftRocketOn = false;
        public bool RightRocketOn = false;
        public int totalScore = 0;

        public bool HasTouchedControls { get; set; }


        //  Controls
        KeyboardState previousKBState = Keyboard.GetState();
        GamePadState previousGPState = GamePad.GetState(PlayerIndex.One);
        //  KB
        Keys LeftRocketKey;
        Keys RightRocketKey;
        Keys BoostKey;
        //  Gamepad
        PlayerIndex playerIndex;
        Buttons BoostButton;
        public const float TRIGGER_DEADZONE = 0.05f;
        //  TrailColour
        public Color playerColor;
        //  Boosting
        const int BOOST_LIFE = 100;
        const int SCORE_ADDITION = 100;
        //public float currentBoost = 0;
        bool isBoosting = false;
        public bool canBoost = true;
        public bool boostPressed = false;
        float TotalDT = 0f;

        public Player(int playerNo, GraphicsDevice gd, GraphicsDeviceManager gdm,
            string fileName = "", ContentManager content = null)
            : base(gd, gdm, fileName, content)
        {
            playerID = playerNo;
            score = 0;
            switch (playerID)
            {
                case 1:
                    playerIndex = PlayerIndex.One;
                    playerColor = new Color(255, 0, 0);
                    break;
                case 2:
                    playerIndex = PlayerIndex.Two;
                    playerColor = new Color(0, 255, 0);
                    break;
                case 3:
                    playerIndex = PlayerIndex.Three;
                    playerColor = new Color(0, 100, 255);
                    break;
                case 4:
                    playerIndex = PlayerIndex.Four;
                    playerColor = new Color(255, 255, 0);
                    break;
                default:
                    break;
            }

            previousKBState = Keyboard.GetState();
            previousGPState = GamePad.GetState(playerIndex);

            HasTouchedControls = false;

            //_gameState = 0;
        }

        public void ResetTotalScore()
        {
            totalScore = 0;
        }

        public void AddToTotalScore(int newScore)
        {
            totalScore += newScore;
        }

        public void SetKeys(string kbControls, string gamepadControls)
        {
            BoostButton = (Buttons)Enum.Parse(typeof(Buttons), gamepadControls, false);

            string[] keyBinds = kbControls.Split(',');
            LeftRocketKey = (Keys)Enum.Parse(typeof(Keys), keyBinds[0], false);
            BoostKey = (Keys)Enum.Parse(typeof(Keys), keyBinds[1], false);
            RightRocketKey = (Keys)Enum.Parse(typeof(Keys), keyBinds[2], false);

        }

        //  If start of game, create new car
        //  Else randomize car parts
        public void GenerateCar(GraphicsDevice gd, GraphicsDeviceManager gdm, ContentManager content)
        {
            myCar = new Car(gd, gdm, "", content);
            myCar.Position = Position;
            myCar.PlyrID = playerID;
            NewCarModel();
        }

        void NewCarModel()
        {
            myCar.randomizeParts(playerColor);
            myCar.BuildCollisionModels();

            if (GameState.Current == GameState.States.MENU)
            {
                SoundManager.PlayMenuBeep(myCar.SoundEmitter);
            }
        }

        void handleControls(float dt)
        {
            KeyboardState kbState = Keyboard.GetState();
            GamePadState gpState = GamePad.GetState(playerIndex);

            switch (GameState.Current)
            {
                case GameState.States.START_SCREEN:
                    if (kbState.IsKeyDown(BoostKey))
                    {
                        boostPressed = true;
                    }

                    if (gpState.IsConnected)
                    {
                        if (gpState.IsButtonDown(BoostButton))
                        {
                            boostPressed = true;
                        }
                    }

                    break;

                case GameState.States.START:
                    break;

                case GameState.States.MENU:

                    // Have to keep button down to veto
                    vetoesTrack = false;

                    // Randomise car
                    if (previousKBState.IsKeyUp(BoostKey))
                    {
                        if (kbState.IsKeyDown(BoostKey))
                        {
                            NewCarModel();
                        }
                    }

                    if (gpState.IsConnected)
                    {
                        if (previousGPState.IsButtonUp(BoostButton))
                        {
                            if (gpState.IsButtonDown(BoostButton))
                            {
                                NewCarModel();
                            }
                        }
                    }

                    // Keyboard vetoing
                    //if (kbState.IsKeyDown(BoostKey) && kbState.IsKeyDown(RightRocketKey)
                    //    && kbState.IsKeyDown(LeftRocketKey))
                    if (kbState.IsKeyDown(RightRocketKey) || previousKBState.IsKeyDown(LeftRocketKey))
                    {
                        vetoesTrack = true;
                    }

                    // Gamepad vetoing
                    if (gpState.IsConnected)
                    {
                        //if (gpState.IsButtonUp(BoostButton) && gpState.Triggers.Left > TRIGGER_DEADZONE
                        //    && gpState.Triggers.Right > TRIGGER_DEADZONE)
                        if (gpState.Triggers.Left > TRIGGER_DEADZONE || gpState.Triggers.Right > TRIGGER_DEADZONE)
                        {
                            vetoesTrack = true;
                        }
                    }
                    break;

                case GameState.States.MENU_TIMEOUT:
                    vetoesTrack = false;
                    HasTouchedControls = false;
                    break;

                case GameState.States.START_PLAYING:
                    
                    break;

                case GameState.States.PLAYING:

                    LeftRocketOn = false;
                    RightRocketOn = false;

                    //  Keyboard handling
                    if (kbState.IsKeyUp(BoostKey))
                    {
                        boostPressed = false;
                    }

                    if (kbState.IsKeyDown(LeftRocketKey))
                    {
                        myCar.applyLeftRocket(dt, 1.0f);
                        LeftRocketOn = true;
                        HasTouchedControls = true;
                    }

                    if (kbState.IsKeyDown(RightRocketKey))
                    {
                        myCar.applyRightRocket(dt, 1.0f);
                        RightRocketOn = true;
                        HasTouchedControls = true;
                    }

                    if (kbState.IsKeyDown(BoostKey) &&
                        canBoost && !isBoosting && !boostPressed && !myCar.BoostInvincibility)
                    {
                        isBoosting = true;
                        myCar.applyMainRocket(LeftRocketOn, RightRocketOn);
                        myCar.currentBoost-=20;
                        boostPressed = true;
                        HasTouchedControls = true;
                    }                   

                    //  Gamepad handling
                    if (gpState.IsConnected)
                    {

                        //  ADD ROCKET CONTROLS HERE
                        if (gpState.Triggers.Left > TRIGGER_DEADZONE)
                        {
                            myCar.applyLeftRocket(dt, gpState.Triggers.Left);
                            LeftRocketOn = true;
                            HasTouchedControls = true;
                        }
                        if (gpState.Triggers.Right > TRIGGER_DEADZONE)
                        {
                            myCar.applyRightRocket(dt, gpState.Triggers.Right);
                            RightRocketOn = true;
                            HasTouchedControls = true;
                        }

                        if (previousGPState.IsButtonUp(BoostButton))
                        {
                            if (gpState.IsButtonDown(BoostButton) &&
                                canBoost)
                            {
                                isBoosting = true;
                                myCar.applyMainRocket(LeftRocketOn, RightRocketOn);
                                myCar.currentBoost -= 20;
                                boostPressed = true;
                                HasTouchedControls = true;
                            }
                        }
                    }
                    break;

                case GameState.States.RACE_OVER:
                    HasTouchedControls = false;
                    break;

                default:
                    break;
            }
            //  Check if the player is active in the game
            if ((previousKBState != kbState || previousGPState != gpState) && !isPlaying)
            {
                isPlaying = true;
            }
            previousGPState = gpState;
            previousKBState = kbState;
        }

        public override void update(float dt)
        {
            // && myCar.IsAlive
            if (myCar != null)
            {
                handleControls(dt);

                if (myCar.IsAlive)
                {
                    myCar.update(dt);
                }
            }

            switch (GameState.Current)
            {
                case GameState.States.START_SCREEN:
                    handleControls(dt);
                    break;

                case GameState.States.START:
                    myCar.AffectedByGravity = false;
                    myCar.EngineOn = false;
                    break;

                case GameState.States.NEW_RACE:
                    myCar.AffectedByGravity = false;
                    myCar.EngineOn = false;
                    break;

                case GameState.States.MENU:
                    myCar.AffectedByGravity = false;
                    myCar.EngineOn = false;
                    break;

                case GameState.States.MENU_TIMEOUT:
                    myCar.AffectedByGravity = true;
                    break;

                case GameState.States.START_PLAYING:
                    myCar.AffectedByGravity = true;
                    myCar.EngineOn = true;
                    break;

                case GameState.States.PLAYING:

                    if (!myCar.IsAlive)
                    {
                        IsAlive = false;
                    }

                    if (IsAlive)
                    {
                        if (myCar.hasFlag)
                        {
                            TotalDT += dt;
                            if (TotalDT >= 1)
                            {
                                score += SCORE_ADDITION;
                                TotalDT = 0;
                            }
                        }
                        else
                        {
                            TotalDT = 0;
                        }

                        if (!canBoost)
                        {
                            isBoosting = false;
                        }

                        if (myCar.currentBoost >= BOOST_LIFE / 5)
                        {
                            canBoost = true;
                        }

                        if (myCar.currentBoost <= 0)
                        {
                            isBoosting = false;
                            canBoost = false;
                        }

                        if (isBoosting)
                        {
                            //car.applyMainRocket(kbState, LeftRocketKey, RightRocketKey);
                            isBoosting = false;
                        }
                        else
                        {
                            if (myCar.currentBoost < BOOST_LIFE && IsAlive)
                            {
                                myCar.currentBoost += 3 * dt;
                            }
                        }
                        myCar.currentBoost = MathHelper.Clamp(myCar.currentBoost, 0, BOOST_LIFE);
                    }
                    break;

                case GameState.States.RACE_OVER:
                    break;

                default:
                    break;
            }
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            if (myCar != null)
            {
                //if (camera.OnScreen(car))
                {
                    myCar.draw(graphicsDevice, camera);
                }
            }
        }
    }
}
