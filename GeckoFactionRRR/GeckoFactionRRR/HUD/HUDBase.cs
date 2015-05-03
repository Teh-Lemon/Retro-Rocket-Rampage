using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GeckoFactionRRR
{
    class HUDBase
    {
        public Texture2D hud;
        public GameWindow window;
        public List<Player> playerList { get; set; }
        public SpriteBatch spriteBatch { get; set; }
        public Race race { get; set; }
        Texture2D DPSFLOGO;
        Texture2D TitleImage;
        float titleScale = 1.2f;
        float offsetTime = 0f;
        float startMenuCurrentTime = 0;
        const float START_MENU_TIMER = 1f;
        bool drawStartMenuFont = false;

        // Player status above their HUD bar
        Texture2D flagIcon;
        Texture2D bombIcon;
        Texture2D firstIcon;
        Texture2D secondIcon;
        Texture2D thirdIcon;
        Texture2D fourthIcon;

        // Fonts
        SpriteFont debugFont;
        SpriteFont CDFont;
        SpriteFont KONotifierFont;

        public float timer = 0f;
        float prevTime = 0f;
        public float CDScale = 0.1f;
        public float CDScaleIncrease = 0.05f;

        Color BoostBarColour;

        float HUDItemSpriteScale = 2f;

        float _currentTime = 0.0f;
        float DT = 0.0f;
        bool finishedOverallAnimation = false;

        public Player OverallWinner = null;

        // Variables necessary for Start Menu Stat rectangles
        #region Car stats
        float P1CurrentWeightVal = 0;
        float P2CurrentWeightVal = 0;
        float P3CurrentWeightVal = 0;
        float P4CurrentWeightVal = 0;
        float currentWeight = 0;

        float P1CurrentShuntVal = 0;
        float P2CurrentShuntVal = 0;
        float P3CurrentShuntVal = 0;
        float P4CurrentShuntVal = 0;
        float currentShunt = 0;

        float P1CurrentStabilityVal = 0;
        float P2CurrentStabilityVal = 0;
        float P3CurrentStabilityVal = 0;
        float P4CurrentStabilityVal = 0;
        float currentStability = 0;
        #endregion

        // Rectangle Resize weight, between 0 and 1
        float STAmount = 0.3f;

        public HUDBase(Texture2D spriteTexture, ContentManager Content,
            GraphicsDevice gd, GraphicsDeviceManager gdm, float spriteScale = 1.0f)
        {
            //hud = spriteTexture;

            hud = Content.Load<Texture2D>("Materials/HUD/BoostBarOutlineSmall");

            debugFont = Content.Load<SpriteFont>("Fonts/Segoe11");
            CDFont = Content.Load<SpriteFont>("Fonts/Segoe72");
            KONotifierFont = Content.Load<SpriteFont>("Fonts/Segoe17");


            flagIcon = Content.Load<Texture2D>("Materials/HUD/Flag_Icon");
            bombIcon = Content.Load<Texture2D>("Materials/HUD/Bomb_Icon");

            firstIcon = Content.Load<Texture2D>("Materials/HUD/1stPlace");
            secondIcon = Content.Load<Texture2D>("Materials/HUD/2ndPlace");
            thirdIcon = Content.Load<Texture2D>("Materials/HUD/3rdPlace");
            fourthIcon = Content.Load<Texture2D>("Materials/HUD/4thPlace");

            DPSFLOGO = Content.Load<Texture2D>("Textures/DPSFLogo");
            TitleImage = Content.Load<Texture2D>("Materials/RRRtitle");
        }

        public void initialise()
        {
            CDScale = 0.1f;
            CDScaleIncrease = 0.05f;
            timer = 0;
            prevTime = 0;

            startMenuCurrentTime = 0;
            drawStartMenuFont = false;

            _currentTime = 0;
            finishedOverallAnimation = false;
        }

        public void update(float dt)
        {
            DT = dt;
        }

        public void draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            switch (GameState.Current)
            {
                case GameState.States.START_SCREEN:

                    // Draw Randomise Message
                    string startMessage = "Press boost to start the game!";
                    // Centre text on screen
                    Vector2 startMessageSize = font.MeasureString(startMessage);
                    Vector2 startMessageCentre = new Vector2(graphicsDevice.Viewport.Width / 2 - (font.MeasureString(startMessage).X),
                        window.ClientBounds.Height / 18 * 15);

                    startMenuCurrentTime += DT;

                    if (startMenuCurrentTime >= START_MENU_TIMER)
                    {
                        startMenuCurrentTime = 0f;
                        if (drawStartMenuFont)
                        {
                            drawStartMenuFont = false;
                        }
                        else
                        {
                            drawStartMenuFont = true;
                        }
                    }

                    if (drawStartMenuFont)
                    {
                        spriteBatch.DrawString(font, startMessage,
                            startMessageCentre, Color.White,
                            0, Vector2.Zero, 2, SpriteEffects.None, 0);
                    }

                    spriteBatch.Draw(DPSFLOGO,
                        new Vector2(
                            (window.ClientBounds.Width / 20 * 17),
                            window.ClientBounds.Height - 110 - (hud.Height / 4)),
                        null, Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);

                    titleScale = (float)window.ClientBounds.Width / 1200;
                    offsetTime += 0.05f;
                    if (offsetTime >= 6.3f) offsetTime = 0;

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);

                    spriteBatch.Draw(TitleImage,
                        new Vector2(window.ClientBounds.Width/2, window.ClientBounds.Height /2.5f),
                            null, Color.White, (float)Math.Sin(offsetTime)/20, new Vector2(
                            TitleImage.Bounds.Center.X, TitleImage.Bounds.Center.Y), titleScale, SpriteEffects.None, 0f);

                    break;

                case GameState.States.START:
                    break;

                case GameState.States.MENU:
                    // Draw Stat Meters
                    drawStats(graphicsDevice, spriteBatch, font);

                    #region Help and Veto Message
                    int vetoCount = 0;
                    string vetoMessage = "";

                    foreach (Player player in playerList)
                    {
                        if (player.vetoesTrack)
                        {
                            vetoCount++;
                        }
                    }

                    if (vetoCount == 1)
                    {
                        vetoMessage = "1 player vetoes the track!";
                    }
                    if (vetoCount > 1)
                    {
                        vetoMessage = vetoCount.ToString() + " players veto the track!";
                    }
                    if (vetoCount == playerList.Count())
                    {
                        vetoMessage = "All players veto the track! Resetting...";
                    }

                    // Draw Randomise Message
                    string boostMessage = "Press boost to randomise your vehicle!";
                    // Centre text on screen
                    Vector2 boostMessageSize = font.MeasureString(boostMessage);
                    Vector2 boostMessageCentre = new Vector2(graphicsDevice.Viewport.Width / 2 - (font.MeasureString(boostMessage).X),
                        window.ClientBounds.Height / 18 * 6);


                    // Centre text on screen
                    Vector2 vetoMessageSize = font.MeasureString(vetoMessage);
                    Vector2 vetoMessageCentre = new Vector2(graphicsDevice.Viewport.Width / 2 - (font.MeasureString(vetoMessage).X) / 2,
                        window.ClientBounds.Height / 18 * 7);

                    spriteBatch.DrawString(font, boostMessage,
                        boostMessageCentre, Color.White,
                        0, Vector2.Zero, 2, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, vetoMessage,
                        vetoMessageCentre - (vetoMessageSize / 2), Color.White,
                        0, Vector2.Zero, 2, SpriteEffects.None, 0);
                    #endregion

                    drawCountdown(graphicsDevice, spriteBatch, CDFont);

                    break;

                case GameState.States.MENU_TIMEOUT:
                    break;

                case GameState.States.PLAYING:
                    drawMidRaceHUD(graphicsDevice, spriteBatch, font);

#if DEBUG
                    foreach (Player player in playerList)
                    {
                        // DEBUG TEXT
                        string debugText = "For dir " + player.myCar.forwardDirection.X.ToString()
                            + ", " + player.myCar.forwardDirection.Y.ToString()
                            + ", " + player.myCar.forwardDirection.Z.ToString()
                            + "\n"
                            + "HasTouched " + player.HasTouchedControls.ToString()
                            + "\n"
                            + "Car/track angle " + player.myCar.debugAngle.ToString()
                            + "\n"
                            + "OnGround " + player.myCar.OnGround.ToString()
                            + "\n"
                            + "Last hit: " + player.myCar.LastCollidedPlyrID.ToString()
                        + "\n" + player.myCar.weight.ToString();

                        int heightPlace = window.ClientBounds.Height - 250;

                        spriteBatch.DrawString(debugFont, debugText
                            , new Vector2((window.ClientBounds.Width / 5) * player.playerID - hud.Width / 2, heightPlace)
                            , player.playerColor);
                    }
#endif

                    break;

                case GameState.States.VICTORY_LAP:
                    drawVictoryLap(graphicsDevice, spriteBatch, font);
                    break;

                case GameState.States.OVERALL_WINNER:
                    drawOverallWinner(graphicsDevice, spriteBatch, font);
                    break;

                case GameState.States.RACE_OVER:
                    break;

                default:
                    break;

            }
        }

        public void drawStats(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            int playerXmulti = 1;
            int playerYmulti = 1;
            float desiredWeight = 0;
            float desiredShunt = 0;
            float desiredStability = 0;

            foreach (Player player in playerList)
            {
                desiredWeight = player.myCar.weight;
                desiredShunt = player.myCar.boostStrength;
                desiredStability = player.myCar.stability;

                if (player.playerID == 1)
                {
                    playerXmulti = 1;
                    playerYmulti = 5;

                    currentWeight = P1CurrentWeightVal;
                    currentShunt = P1CurrentShuntVal;
                    currentStability = P1CurrentStabilityVal;

                    if (currentWeight != desiredWeight)
                    {
                        currentWeight = MathHelper.SmoothStep(currentWeight, desiredWeight, STAmount);
                    }

                    if (currentShunt != desiredShunt)
                    {
                        currentShunt = MathHelper.SmoothStep(currentShunt, desiredShunt, STAmount);
                    }

                    if (currentStability != desiredStability)
                    {
                        currentStability = MathHelper.SmoothStep(currentStability, desiredStability, STAmount);
                    }

                    P1CurrentWeightVal = currentWeight;
                    P1CurrentShuntVal = currentShunt;
                    P1CurrentStabilityVal = currentStability;
                }
                else if (player.playerID == 2)
                {
                    playerXmulti = 11;
                    playerYmulti = 5;

                    currentWeight = P2CurrentWeightVal;
                    currentShunt = P2CurrentShuntVal;
                    currentStability = P2CurrentStabilityVal;

                    if (currentWeight != desiredWeight)
                    {
                        currentWeight = MathHelper.SmoothStep(currentWeight, desiredWeight, STAmount);
                    }

                    if (currentShunt != desiredShunt)
                    {
                        currentShunt = MathHelper.SmoothStep(currentShunt, desiredShunt, STAmount);
                    }

                    if (currentStability != desiredStability)
                    {
                        currentStability = MathHelper.SmoothStep(currentStability, desiredStability, STAmount);
                    }

                    P2CurrentWeightVal = currentWeight;
                    P2CurrentShuntVal = currentShunt;
                    P2CurrentStabilityVal = currentStability;
                }
                else if (player.playerID == 3)
                {
                    playerXmulti = 1;
                    playerYmulti = 18;

                    currentWeight = P3CurrentWeightVal;
                    currentShunt = P3CurrentShuntVal;
                    currentStability = P3CurrentStabilityVal;

                    if (currentWeight != desiredWeight)
                    {
                        currentWeight = MathHelper.SmoothStep(currentWeight, desiredWeight, STAmount);
                    }

                    if (currentShunt != desiredShunt)
                    {
                        currentShunt = MathHelper.SmoothStep(currentShunt, desiredShunt, STAmount);
                    }

                    if (currentStability != desiredStability)
                    {
                        currentStability = MathHelper.SmoothStep(currentStability, desiredStability, STAmount);
                    }

                    P3CurrentWeightVal = currentWeight;
                    P3CurrentShuntVal = currentShunt;
                    P3CurrentStabilityVal = currentStability;
                }
                else if (player.playerID == 4)
                {
                    playerXmulti = 11;
                    playerYmulti = 18;

                    currentWeight = P4CurrentWeightVal;
                    currentShunt = P4CurrentShuntVal;
                    currentStability = P4CurrentStabilityVal;

                    if (currentWeight != desiredWeight)
                    {
                        currentWeight = MathHelper.SmoothStep(currentWeight, desiredWeight, STAmount);
                    }

                    if (currentShunt != desiredShunt)
                    {
                        currentShunt = MathHelper.SmoothStep(currentShunt, desiredShunt, STAmount);
                    }

                    if (currentStability != desiredStability)
                    {
                        currentStability = MathHelper.SmoothStep(currentStability, desiredStability, STAmount);
                    }

                    P4CurrentWeightVal = currentWeight;
                    P4CurrentShuntVal = currentShunt;
                    P4CurrentStabilityVal = currentStability;
                }


                // Draw Text
                spriteBatch.DrawString(font, string.Format("Player " + player.playerID.ToString() + ": " + player.myCar.name),
                        new Vector2((window.ClientBounds.Width / 20) * playerXmulti, (window.ClientBounds.Height / 20) * (playerYmulti - 3)), player.playerColor,
                        0, Vector2.Zero, 2, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Weight: ",//+ player.car.weight.ToString()),
                        new Vector2((window.ClientBounds.Width / 20) * playerXmulti, (window.ClientBounds.Height / 20) * (playerYmulti - 2)), player.playerColor,
                        0, Vector2.Zero, 2, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Shunt Strength: ",// + player.car.boostStrength.ToString()),
                        new Vector2((window.ClientBounds.Width / 20) * playerXmulti, (window.ClientBounds.Height / 20) * (playerYmulti - 1)), player.playerColor,
                        0, Vector2.Zero, 2, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Stability: ",//string.Format("{00000}", "Stability: " + player.car.stability.ToString()),
                        new Vector2((window.ClientBounds.Width / 20) * playerXmulti, (window.ClientBounds.Height / 20) * (playerYmulti)), player.playerColor,
                        0, Vector2.Zero, 2, SpriteEffects.None, 0);

                // Draw Rectangles
                //spriteBatch.Draw(hud, new Rectangle((window.ClientBounds.Width / 20) * (playerXmulti + 4) /*-(hud.Width / 2)*/,
                //      ((window.ClientBounds.Height / 20) * (playerYmulti - 2)), (int)(hud.Width * (player.car.weight / 200)), 22),
                //        new Rectangle(0, 22, (int)player.car.weight, 22), player.playerColor);                
                //spriteBatch.Draw(hud, new Rectangle((window.ClientBounds.Width / 20) * (playerXmulti + 4) /*-(hud.Width / 2)*/,
                //        ((window.ClientBounds.Height / 20) * (playerYmulti - 1))/* - 370)*/, (int)(hud.Width * ((double)player.car.boostStrength / 200)), 22),
                //        new Rectangle(0, 22, (int)player.car.boostStrength, 22), player.playerColor);
                //spriteBatch.Draw(hud, new Rectangle((window.ClientBounds.Width / 20) * (playerXmulti + 4) /*-(hud.Width / 2)*/,
                //        ((window.ClientBounds.Height / 20) * (playerYmulti))/* - 330)*/, (int)(hud.Width * ((double)player.car.stability / 200)), 22),
                //        new Rectangle(0, 22, (int)player.car.stability, 22), player.playerColor);

                spriteBatch.Draw(hud, new Rectangle((window.ClientBounds.Width / 20) * (playerXmulti + 4) /*-(hud.Width / 2)*/,
                        ((window.ClientBounds.Height / 20) * (playerYmulti - 2)), (int)(hud.Width * (currentWeight / 200)), 22),
                        new Rectangle(0, 22, (int)currentWeight, 22), player.playerColor);
                spriteBatch.Draw(hud, new Rectangle((window.ClientBounds.Width / 20) * (playerXmulti + 4) /*-(hud.Width / 2)*/,
                        ((window.ClientBounds.Height / 20) * (playerYmulti - 1))/* - 370)*/, (int)(hud.Width * (currentShunt / 200)), 22),
                        new Rectangle(0, 22, (int)currentShunt, 22), player.playerColor);
                spriteBatch.Draw(hud, new Rectangle((window.ClientBounds.Width / 20) * (playerXmulti + 4) /*-(hud.Width / 2)*/,
                        ((window.ClientBounds.Height / 20) * (playerYmulti))/* - 330)*/, (int)(hud.Width * (currentStability / 200)), 22),
                        new Rectangle(0, 22, (int)currentStability, 22), player.playerColor);

            }
        }

        void drawMidRaceHUD(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            // Sort race position order
            List<Player> scoreSortedPlayers = playerList.OrderByDescending(px => px.score).ToList<Player>();

            for (int i = 0; i < scoreSortedPlayers.Count(); i++)
            {
                scoreSortedPlayers[i].racePolePosition = i + 1;

                if (i != 0 && scoreSortedPlayers[i - 1].score == scoreSortedPlayers[i].score)
                {
                    scoreSortedPlayers[i].racePolePosition = scoreSortedPlayers[i - 1].racePolePosition;
                }
            }

            foreach (Player player in playerList)
            {
                float boostpercentage = player.myCar.BOOST_LIFE / 100 * player.myCar.currentBoost;
                BoostBarColour = player.playerColor;
                float Alpha = MathHelper.Lerp(255, 0, boostpercentage);
                BoostBarColour.A = (byte)Alpha;

                if (boostpercentage == 100)
                {
                    BoostBarColour.A = 255;
                }

                //Draw negative space
                spriteBatch.Draw(hud, new Rectangle((window.ClientBounds.Width / 5) * player.playerID - hud.Width / 2,
                    (window.ClientBounds.Height - 70), hud.Width, 22), new Rectangle(0, 22, hud.Width, 22), Color.Gray);

                if (player.IsAlive && player.canBoost)
                {
                    //Draw Boost bar according to current boost, and place according to player number
                    spriteBatch.Draw(hud, new Rectangle((window.ClientBounds.Width / 5) * player.playerID - hud.Width / 2,
                        (window.ClientBounds.Height - 70), (int)(hud.Width * ((double)player.myCar.currentBoost / 100)), 22),
                        new Rectangle(0, 22, hud.Width, 22), BoostBarColour);
                }
                else
                {
                    //Draw Boost Bar dark to indicate boost is currently unavailable
                    spriteBatch.Draw(hud, new Rectangle((window.ClientBounds.Width / 5) * player.playerID - hud.Width / 2,
                        (window.ClientBounds.Height - 70), (int)(hud.Width * ((double)player.myCar.currentBoost / 100)), 22),
                        new Rectangle(0, 22, hud.Width, 22), Color.DarkSlateGray);
                }

                //Draw Bar outline
                spriteBatch.Draw(hud, new Rectangle((window.ClientBounds.Width / 5) * player.playerID - hud.Width / 2,
                    (window.ClientBounds.Height - 70), hud.Width, 22), new Rectangle(0, 0, hud.Width, 22), Color.White);

                //Draw Score for current player                
                //spriteBatch.DrawString(font, string.Format("{00000}", player.score),
                //new Vector2((window.ClientBounds.Width / 5) * player.playerID, window.ClientBounds.Height - 110), player.playerColor);

                //Draw Score for current player
                string scoreString = player.score.ToString();
                Vector2 scoreStringSize = font.MeasureString(scoreString);

                spriteBatch.DrawString(font, scoreString /*string.Format("{00000}", player.score)*/,
                new Vector2(((window.ClientBounds.Width / 5) * player.playerID) + (hud.Width / 2) - (scoreStringSize.X * 2), window.ClientBounds.Height - 110),
                player.playerColor, 0, Vector2.Zero, 2, SpriteEffects.None, 0);


                // Draw Pole Position Icon

                Texture2D currentPolePos = null;
                // Find current Position
                switch (player.racePolePosition)
                {
                    case 1:
                        currentPolePos = firstIcon;
                        break;
                    case 2:
                        currentPolePos = secondIcon;
                        break;
                    case 3:
                        currentPolePos = thirdIcon;
                        break;
                    case 4:
                        currentPolePos = fourthIcon;
                        break;
                    default:
                        break;
                }
                // Draw Position Icon

                spriteBatch.Draw(currentPolePos,
                        new Vector2(
                            (window.ClientBounds.Width / 5) * player.playerID - hud.Width / 2,
                            window.ClientBounds.Height - 110 - (hud.Height / 4)),
                        null, Color.White, 0f, Vector2.Zero, HUDItemSpriteScale, SpriteEffects.None, 0f);

                // Draw Flag icon
                if (player.myCar.hasFlag)
                {
                    spriteBatch.Draw(flagIcon,
                        new Vector2(
                            ((window.ClientBounds.Width / 5) * player.playerID - hud.Width / 2)
                            + (currentPolePos.Width * HUDItemSpriteScale),
                            window.ClientBounds.Height - 110),
                        null, Color.White, 0f, Vector2.Zero, HUDItemSpriteScale, SpriteEffects.None, 0f);
                }

                // Draw Bomb Icon
                if (player.myCar.hasBomb)
                {
                    spriteBatch.Draw(bombIcon,
                        new Vector2(
                            ((window.ClientBounds.Width / 5) * player.playerID - hud.Width / 2)
                            + (currentPolePos.Width * HUDItemSpriteScale) + (bombIcon.Width * HUDItemSpriteScale),
                            window.ClientBounds.Height - 110),
                        null, Color.White, 0f, Vector2.Zero, HUDItemSpriteScale, SpriteEffects.None, 0f);
                }
            }
        }

        void drawCountdown(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            timer = (float)Math.Truncate(timer);

            if (prevTime != timer)
            {
                CDScale += CDScaleIncrease;
                CDScaleIncrease += 0.01f;
            }

            prevTime = timer;

            string timeLeft = timer.ToString();
            Vector2 CDMessageSize = font.MeasureString(timeLeft);
            Vector2 CDMessageCentre = new Vector2(graphicsDevice.Viewport.Width / 2,
                window.ClientBounds.Height / 2);

            //spriteBatch.DrawString(font, timeLeft,
            //  CDMessageCentre - (CDMessageSize / 2), Color.White);
            spriteBatch.DrawString(font, timeLeft,
                CDMessageCentre /*- (CDMessageSize / 2)*/, Color.White, 0, (CDMessageSize / 2), CDScale, SpriteEffects.None, 0);
        }

        void drawVictoryLap(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            const float GAME_FONT_SCALE = 5f;

            if (race.Winner != null)
            {
                string winText = "Player " + race.Winner.playerID.ToString() + " wins race "
                    + race.ID.ToString() + "/3";
                Vector2 winSize = font.MeasureString(winText) * GAME_FONT_SCALE;
                Vector2 winCentre = new Vector2(graphicsDevice.Viewport.Width / 2 - (winSize.X / 2),
                    window.ClientBounds.Height / 5 - (winSize.Y));

                string bonusPointsText = "+" + Race.RACE_LEADER_BONUS_SCORE.ToString() + " bonus points!";
                Vector2 bonusPointsSize = font.MeasureString(bonusPointsText) * GAME_FONT_SCALE;
                Vector2 bonusPointsCentre = new Vector2(graphicsDevice.Viewport.Width / 2 - (bonusPointsSize.X / 2),
                    winCentre.Y + (bonusPointsSize.Y ));

                string runningText = "Running Total";
                Vector2 runningSize = font.MeasureString(runningText) * GAME_FONT_SCALE;
                Vector2 runningCentre = new Vector2(graphicsDevice.Viewport.Width / 2 - (runningSize.X / 2),
                   bonusPointsCentre.Y + (runningSize.Y));

                string playerTally = "";
                for (int i = 0; i < playerList.Count; i++)
                {
                    playerTally += "Player " + (i + 1).ToString() 
                        + ": " + playerList[i].totalScore.ToString() + "\n";
                }
                Vector2 playerTallySize = font.MeasureString(playerTally) * GAME_FONT_SCALE;
                Vector2 playerTallyCentre = new Vector2(graphicsDevice.Viewport.Width / 2f - (playerTallySize.X / 2),
                    runningCentre.Y + (runningSize.Y));

                spriteBatch.DrawString(font, winText,
                    winCentre, Color.White,
                    0, Vector2.Zero, GAME_FONT_SCALE, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, bonusPointsText,
                    bonusPointsCentre, new Color(38, 255, 38),
                    0, Vector2.Zero, GAME_FONT_SCALE, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, runningText,
                    runningCentre, Color.White,
                    0, Vector2.Zero, GAME_FONT_SCALE, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, playerTally,
                    playerTallyCentre, Color.White,
                    0, Vector2.Zero, GAME_FONT_SCALE, SpriteEffects.None, 0);
                /*
                spriteBatch.DrawString(KONotifierFont, playerTallyScores,
                    playerTallyScoresCentre, Color.White,
                    0, Vector2.Zero, 2, SpriteEffects.None, 0);
                 * */

            }

            _currentTime = 0;
        }

        void drawOverallWinner(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            const float GAME_FONT_SCALE = 7f;

            float animationScale = 150.0f;
            _currentTime += DT;

            int lineNumber = 1;

            // Sort race position order
            List<Player> scoreSortedPlayers = playerList.OrderByDescending(px => px.totalScore).ToList<Player>();

            for (int i = 0; i < scoreSortedPlayers.Count(); i++)
            {
                scoreSortedPlayers[i].racePolePosition = i + 1;

                if (i != 0 && scoreSortedPlayers[i - 1].totalScore == scoreSortedPlayers[i].totalScore)
                {
                    scoreSortedPlayers[i].racePolePosition = scoreSortedPlayers[i - 1].racePolePosition;
                }
            }

            OverallWinner = scoreSortedPlayers[0];

            string finalRankingText = "Final Ranking!";
            Vector2 finalRankingSize = font.MeasureString(finalRankingText) * GAME_FONT_SCALE;
            Vector2 finalRankingCentre = new Vector2(graphicsDevice.Viewport.Width / 2 - (finalRankingSize.X / 2),
                window.ClientBounds.Height + (finalRankingSize.Y / 2f) - (_currentTime * animationScale));

            // Add the list of players to the final ranking list
            List<String> playersString = new List<string>();
            List<Vector2> playersSize = new List<Vector2>();
            List<Vector2> playersCentre = new List<Vector2>();
            //List<Vector2> playersOrigin = new List<Vector2>();
            List<float> playersScale = new List<float>();

            for (int i = 0; i < playerList.Count; i++)
            {
                playersString.Add("Player " + scoreSortedPlayers[i].playerID.ToString());

                playersScale.Add(GAME_FONT_SCALE / (i + 1));

                playersSize.Add(font.MeasureString(playersString[i]) * playersScale[i]);

                if (i == 0)
                {
                    playersCentre.Add(new Vector2(graphicsDevice.Viewport.Width / 2 - (playersSize[i].X / 2),
                //finalRankingCentre.Y + (playersSize[i].Y * 2)));
                finalRankingCentre.Y + finalRankingSize.Y));
                }
                else
                {
                    playersCentre.Add(new Vector2(graphicsDevice.Viewport.Width / 2 - (playersSize[i].X / 2),
                        playersCentre[i - 1].Y + finalRankingSize.Y));
                }

                //playersOrigin.Add(playersSize[i] / 2f);
            }

            // Draw "Final Rankings!"
            spriteBatch.DrawString(font, finalRankingText,
                finalRankingCentre, Color.White, 0, Vector2.Zero, (GAME_FONT_SCALE), SpriteEffects.None, 0);

            // Draw the list of players
            //float newScale = GAME_FONT_SCALE * 1.5f / (float)lineNumber;
            for (int i = 0; i < playerList.Count; i++)
            {
                //newScale = 1.5f / (float)lineNumber;
                spriteBatch.DrawString(font, playersString[i],
                    playersCentre[i], Color.White, 0, Vector2.Zero, playersScale[i], SpriteEffects.None, 0);
                //lineNumber++;
            }

            // Restart game once the last player reaches the top 25% height point
            if (playersCentre[playersCentre.Count - 1].Y < window.ClientBounds.Height / 4)
            {
                finishedOverallAnimation = true;
            }

        }

        public bool FinishedOverallAnimation
        {
            get
            {
                return finishedOverallAnimation;
            }
        }
    }
}
