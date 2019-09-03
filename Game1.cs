using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Media;

namespace Tetris
{
    public class Game1 : Game
    {
        // Game constantes
        player player;
        Vector2 position1 = new Vector2(160*3/2, (640 - 32)*3/2);
        Rectangle BoardLocation = new Rectangle(320*3/2, 0, 480 * 3 / 2, 640 * 3 / 2);
        Rectangle[] nextBlockBoardsLocation = new Rectangle[]
        {
            new Rectangle(100*3/2, 88*3/2, 120*3/2, 120*3/2),
            new Rectangle(100*3/2, 168*3/2, 120*3/2, 120*3/2)
        };

        // Game state control
        const int STATE_MENU = 0;
        const int STATE_PLAYING = 1;
        const int STATE_PAUSED = 2;
        const int STATE_GAMEOVER = 3;
        const int STATE_GAMEWON = 4;
        int GameState = STATE_MENU;

        // Status
        bool Restart = false;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Textures
        Dictionary<char, Texture2D> BlockTextures;
        Texture2D boardRect;
        Texture2D texture1px;
        Texture2D background;
        Texture2D ground;
        Texture2D ground1;
        Texture2D goal;
        Texture2D startPlace;
        Texture2D logo;
        Texture2D nextPiece;
        Texture2D StartBackground;
        Texture2D skipBoard;
        Texture2D skip1;
        Texture2D skip2;
        Texture2D skip3;

        // Menu textures
        MenuObject buttonStart, buttonExit, buttonResume, buttonOutline, buttonRestart;

        // Fonts
        SpriteFont GameFont;

        // Game Objects
        public Board gameBoard;
        Board[] nextBlockBoards;

        Tetromino currentTetromino;
        Random randomGenerator;

        // Game parameters
        int Lines = 0;
        float FallSpeed = 4;
        int endHeight = 0;
        int skip = 0;

        double lastActionTime = 0; // lastUpdate time in ms
        double lastGravityEffectTime = 0;
        double ActionDelay = 150; // delay bewteen two actions in ms
        double lastSkipTime = 0;
<<<<<<< Updated upstream

=======
        double startTime = 0;
        double endTime = 0;
        double timeInterval = 0;
        double totalTime = 0;
        bool timeCount = false;
        double dieTime = 0;
        double winTime = 0;
        bool winTimeCount = false;
>>>>>>> Stashed changes

        Queue<char> nextTetrominos = new Queue<char>();
        string CHARLIST = "IOJLZTS";

        // Player related
        float speed = 4f;
        float jumpStrength = 15f;
        float gravity = 0.6f;
        bool hasJumped;
        bool outOfStart;
        Vector2 temptVelocity;
        Vector2 nextPosition;

<<<<<<< Updated upstream
=======
        Animation idleAnimation;
        Animation runAnimation;
        Animation dieAnimation;
        Animation jumpAnimation;
        Animation winAnimation;

        //Main Directory
        string path = (new FileInfo(AppDomain.CurrentDomain.BaseDirectory)).Directory.Parent.Parent.Parent.Parent.FullName;
        //Audio
        public static SoundEffect playerDefeat;
        SoundEffect playerVictory;
        SoundEffect playerJump;
        SoundEffect lineCompleted;
        //Song gameplayMusic;
        SoundPlayer menuMusic = null;
        SoundPlayer gamePlayMusic = null;
        int currentMusic = 100;

>>>>>>> Stashed changes
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1120*3/2;
            graphics.PreferredBackBufferHeight = 640*3/2;
            //graphics.IsFullScreen = true;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            BlockTextures = new Dictionary<char, Texture2D>();
            randomGenerator = new Random();
            gameBoard = new Board(20, 15);
            nextBlockBoards = new Board[2];
            for (int k = 0; k < 2; k++)
            {
                char nextTetrominoTag = GetRandomCharacter(CHARLIST, randomGenerator);
                nextTetrominos.Enqueue(nextTetrominoTag);
                nextBlockBoards[k] = new Board(4, 4);
            }
            this.IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // menu textures
            Texture2D start = Content.Load<Texture2D>("menu/button_start");
            Texture2D exit = Content.Load<Texture2D>("menu/button_exit");
            Texture2D resume = Content.Load<Texture2D>("menu/button_resume");
            Texture2D restart = Content.Load<Texture2D>("menu/button_restart");
            Texture2D outline = Content.Load<Texture2D>("menu/button_outline");
            buttonStart = new MenuObject(start, HAlignedTextureRectangle(start, 260*3/2));
            buttonExit = new MenuObject(exit, HAlignedTextureRectangle(exit, 360 * 3 / 2));
            buttonResume = new MenuObject(resume, HAlignedTextureRectangle(resume, 260 * 3 / 2));
            buttonRestart = new MenuObject(restart, HAlignedTextureRectangle(restart, 460 * 3 / 2));
            buttonOutline = new MenuObject(outline, HAlignedTextureRectangle(outline, 0));

            // Load block sprites
            BlockTextures.Add('?', Content.Load<Texture2D>("Images/block_white"));
            BlockTextures.Add('S', Content.Load<Texture2D>("Images/block_red"));
            BlockTextures.Add('I', Content.Load<Texture2D>("Images/block_cyan"));
            BlockTextures.Add('O', Content.Load<Texture2D>("Images/block_yellow"));
            BlockTextures.Add('L', Content.Load<Texture2D>("Images/block_blue"));
            BlockTextures.Add('J', Content.Load<Texture2D>("Images/block_orange"));
            BlockTextures.Add('Z', Content.Load<Texture2D>("Images/block_green"));
            BlockTextures.Add('T', Content.Load<Texture2D>("Images/block_purple"));

            background = Content.Load<Texture2D>("Images/background");

            // Player
            player = new player(Content.Load<Texture2D>("Images/donekindof"), position1);

            // Texture 1px
            texture1px = new Texture2D(GraphicsDevice, 1, 1);
            texture1px.SetData(new Color[] { Color.White });

            // boardTexture
            boardRect = new Texture2D(graphics.GraphicsDevice, BoardLocation.Width, BoardLocation.Height);
            Color[] data = new Color[BoardLocation.Width * BoardLocation.Height];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.AliceBlue;
            boardRect.SetData(data);
            
            // Load Fonts
            GameFont = Content.Load<SpriteFont>("Fonts/MyFont");

            //load endimage
            goal = Content.Load<Texture2D>("Images/end_flag_96x96");
            ground = Content.Load<Texture2D>("Images/ground_320x96");
            ground1 = Content.Load<Texture2D>("Images/ground_96x37");
            StartBackground = Content.Load<Texture2D>("Images/start_96x96");
            startPlace = Content.Load<Texture2D>("Images/StartingArea");
            logo = Content.Load<Texture2D>("Images/Logo_820");
            nextPiece = Content.Load<Texture2D>("Images/Nextboxbg");
            skipBoard = Content.Load<Texture2D>("Images/skipboard");
            skip1 = Content.Load<Texture2D>("Images/skip1");
            skip2 = Content.Load<Texture2D>("Images/skip2");
            skip3 = Content.Load<Texture2D>("Images/skip3");
<<<<<<< Updated upstream
=======
            win = Content.Load<Texture2D>("Images/win");
            lose = Content.Load<Texture2D>("Images/lose");

            // player textures
            idleAnimation = new Animation(Content.Load<Texture2D>("Images/donekindof"), 0.1f, true);
            runAnimation = new Animation(Content.Load<Texture2D>("Images/runanimation"), 0.1f, true);
            dieAnimation = new Animation(Content.Load<Texture2D>("Images/deathanimation"), 0.1f, false);
            jumpAnimation = new Animation(Content.Load<Texture2D>("Images/jumpanimation"), 0.1f, true);
            winAnimation = new Animation(Content.Load<Texture2D>("Images/winanimation"), 0.1f, false);
            player.sprite.PlayAnimation(idleAnimation);

            //Audio
            playerDefeat = Content.Load<SoundEffect>("Sound/SFX/Player_Defeat");
            playerVictory = Content.Load<SoundEffect>("Sound/SFX/Player_Victory");
            playerJump = Content.Load<SoundEffect>("Sound/SFX/Player_Jump_A");
            lineCompleted = Content.Load<SoundEffect>("Sound/SFX/Line_Completed");

            // MediaPlayer.Play(gameplayMusic);
            menuMusic = new SoundPlayer(path + @"\Content\Main_Menu_Music.wav");
            gamePlayMusic = new SoundPlayer(path + @"\Content\GamePlay.wav");

            PlayMusic(0);
>>>>>>> Stashed changes
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            if (ButtonIntersects(ref mouseState, buttonStart) || ButtonIntersects(ref mouseState, buttonExit) || ButtonIntersects(ref mouseState, buttonResume) || ButtonIntersects(ref mouseState, buttonRestart))
                buttonOutline.Enable(spriteBatch);
            else
                buttonOutline.Disable(spriteBatch);

            if (buttonStart.BoundingBox.Contains(mouseState.Position))
                buttonOutline.Position.Y = 260 * 3 / 2;
            if (buttonExit.BoundingBox.Contains(mouseState.Position))
                buttonOutline.Position.Y = 360 * 3 / 2;
            if (buttonResume.BoundingBox.Contains(mouseState.Position))
                buttonOutline.Position.Y = 260 * 3 / 2;
            if (buttonRestart.BoundingBox.Contains(mouseState.Position))
                buttonOutline.Position.Y = 460 * 3 / 2;

            switch (GameState)
            {
                case STATE_MENU:
                    buttonResume.Disable(spriteBatch);
                    buttonRestart.Disable(spriteBatch);
                    if (Clicked(ref mouseState, buttonStart))
                        GameState = STATE_PLAYING;
                    if (Clicked(ref mouseState, buttonExit))
                        Exit();
                    break;
                case STATE_PLAYING:
                    buttonStart.Disable(spriteBatch);
                    buttonExit.Disable(spriteBatch);
                    buttonResume.Disable(spriteBatch);
                    buttonRestart.Disable(spriteBatch);
                    if (keyboardState.IsKeyDown(Keys.P))
                        GameState = STATE_PAUSED;
                    break;
                case STATE_PAUSED:
                    buttonResume.Enable(spriteBatch);
                    buttonExit.Enable(spriteBatch);
                    buttonRestart.Enable(spriteBatch);
                    if (Clicked(ref mouseState, buttonRestart))
                    {
                        GameState = STATE_PLAYING;
                        Restart = true;
                    }
                    if (Clicked(ref mouseState, buttonResume))
                        GameState = STATE_PLAYING;
                    if (Clicked(ref mouseState, buttonExit))
                        Exit();
                    break;
                case STATE_GAMEOVER:
                    buttonRestart.Enable(spriteBatch);
                    buttonExit.Enable(spriteBatch);
                    if (Clicked(ref mouseState, buttonRestart))
                    {
                        GameState = STATE_PLAYING;
                        Restart = true;
                    }
                    if (Clicked(ref mouseState, buttonExit))
                        Exit();
                    break;
                case STATE_GAMEWON:
                    buttonRestart.Enable(spriteBatch);
                    buttonExit.Enable(spriteBatch);
<<<<<<< Updated upstream
=======
                    if (timeCount == false)
                    {
                        endTime = gameTime.TotalGameTime.TotalSeconds;
                        timeInterval = endTime - startTime;
                        totalTime += timeInterval;
                        timeCount = true;
                    }
>>>>>>> Stashed changes
                    if (Clicked(ref mouseState, buttonRestart))
                    {
                        GameState = STATE_PLAYING;
                        Restart = true;
                    }
                    if (Clicked(ref mouseState, buttonExit))
                        Exit();
                    break;
                default:
                    break;
            }

            if (GameState == STATE_PLAYING)
            {
                if (Restart == true)
                {
                    player.position = position1;
                    player.velocity = Vector2.Zero;
                    endHeight = 0;
                    Lines = 0;
                    gameBoard.Reset();
                    currentTetromino?.MoveTo(currentTetromino.Xghost, currentTetromino.Yghost);
                    nextBlockBoards[0].Reset();
                    nextBlockBoards[1].Reset();
                    nextTetrominos = new Queue<char>();
                    for (int k = 0; k < 2; k++)
                        nextTetrominos.Enqueue(GetRandomCharacter(CHARLIST, new Random()));
                    hasJumped = false;
                    outOfStart = false;
                    Restart = false;
                    lastActionTime = 0;
                    lastGravityEffectTime = 0;
                    lastSkipTime = 0;
                    skip = 0;
<<<<<<< Updated upstream
=======
                    dieTime = 0;
                    winTime = 0;
                    winTimeCount = false;
                    player.sprite.PlayAnimation(idleAnimation);
>>>>>>> Stashed changes
                }

                // Tetromino generation
                if ((currentTetromino == null || !currentTetromino.IsFalling) && outOfStart == true)
                {
                    currentTetromino = GenerateNewTetromino(nextTetrominos.Dequeue());
                    nextTetrominos.Enqueue(GetRandomCharacter(CHARLIST, randomGenerator));

                    // Reset the nextBlockBoards
                    for (int k = 0; k < 2; k++)
                    {
                        nextBlockBoards[k].Reset();
                        // add a tetromino in the board
                        new Tetromino(nextBlockBoards[k], 2, 1, nextTetrominos.ElementAt(k), BlockTextures[nextTetrominos.ElementAt(k)]);
                    }
                }

                // Apply gravity
                if (gameTime.TotalGameTime.TotalMilliseconds - lastGravityEffectTime > 1000 / FallSpeed)
                {
                    currentTetromino?.MoveTo(currentTetromino.X, currentTetromino.Y - 1);
                    lastGravityEffectTime = gameTime.TotalGameTime.TotalMilliseconds;
                }

                // Check for last action / update
                bool actionIsAllowed = false;
                if (gameTime.TotalGameTime.TotalMilliseconds - lastActionTime > ActionDelay)
                    actionIsAllowed = true;

                if (actionIsAllowed)
                {
                    // -----------------------------------------
                    // Movement
                    // -----------------------------------------
                    if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    {
                        currentTetromino?.MoveTo(currentTetromino.X - 1, currentTetromino.Y);
                        lastActionTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    {
                        currentTetromino?.MoveTo(currentTetromino.X + 1, currentTetromino.Y);
                        lastActionTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    {
                        currentTetromino?.MoveTo(currentTetromino.X, currentTetromino.Y - 1);
                        lastActionTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    // -----------------------------------------
                    // Rotation
                    // -----------------------------------------
                    if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    {
                        currentTetromino?.Rotate(1); // clock wise rotation
                        lastActionTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }

                currentTetromino?.Update(gameTime);
                // Row check
                if (currentTetromino != null && !currentTetromino.IsFalling)
                {
                    // If the tetromino is outside 
                    if (currentTetromino.Y >= 18)
                    {
                        GameState = STATE_GAMEOVER;
                    }
                        
                    // Get the row to remove
                    int rowCleared = gameBoard.ClearRow(lineCompleted);
                    if (rowCleared > 0)
                    {
                        // Update Lines
                        Lines += rowCleared;
                        // decrease end goal
                        endHeight += 64 * 3 / 2 * rowCleared;
                        if (endHeight >= (640 - 160) * 3 / 2)
                            endHeight = (640 - 160) * 3 / 2;
                    }
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    if (gameTime.TotalGameTime.TotalMilliseconds - lastSkipTime > ActionDelay && skip < 3)
                    {
                        nextTetrominos.Dequeue();
                        nextTetrominos.Enqueue(GetRandomCharacter(CHARLIST, randomGenerator));
                        for (int k = 0; k < 2; k++)
                        {
                            nextBlockBoards[k].Reset();
                            // add a tetromino in the board
                            new Tetromino(nextBlockBoards[k], 2, 1, nextTetrominos.ElementAt(k), BlockTextures[nextTetrominos.ElementAt(k)]);
                        }
                        skip++;
                        lastSkipTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }


                // player movement
                if (Keyboard.GetState().IsKeyDown(Keys.D) && player.sprite.Animation != dieAnimation)
                {
                    temptVelocity.X = speed;
                    nextPosition = new Vector2(player.position.X + temptVelocity.X, player.position.Y);
                    if (player.IsColliding(nextPosition, gameBoard) == false)
                    {
                        player.velocity.X = temptVelocity.X;
                    }
                    else if (player.IsColliding(nextPosition, gameBoard) == true)
                    {
                        player.position.X = (320 + gameBoard.Blocks[player.collideBlock].X * 32 - 32) * 3 / 2;
                        player.velocity.X = 0;
                    }
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.A) && player.sprite.Animation != dieAnimation)
                {
                    temptVelocity.X = -speed;
                    nextPosition = new Vector2(player.position.X + temptVelocity.X, player.position.Y);
                    // Console.WriteLine(player.IsColliding(nextPosition, gameBoard));
                    if (player.IsColliding(nextPosition, gameBoard) == false)
                    {
                        player.velocity.X = temptVelocity.X;
                    }
                    else if (player.IsColliding(nextPosition, gameBoard) == true)
                    {
                        player.position.X = (320 + gameBoard.Blocks[player.collideBlock].X * 32 + 32) * 3 / 2;
                        player.velocity.X = 0;
                    }
                }
<<<<<<< Updated upstream
                else
=======
                else if (player.sprite.Animation != dieAnimation && player.sprite.Animation != winAnimation)
                {
>>>>>>> Stashed changes
                    player.velocity.X = 0;
                player.position.X += player.velocity.X;

                if (player.position.X >= 320 * 3 / 2)
                    outOfStart = true;

                if (Keyboard.GetState().IsKeyDown(Keys.W) && player.sprite.Animation != dieAnimation)
                {
                    nextPosition = new Vector2(player.position.X, player.position.Y - jumpStrength);
                    // Console.WriteLine("{0},{1},{2}",player.IsColliding(nextPosition, gameBoard), hasJumped, nextPosition);
                    if (player.IsColliding(nextPosition, gameBoard) == false && hasJumped == false)
                        if (nextPosition.Y <= 0)
                        {
                            player.position.Y = 0;
                            player.velocity.Y = -8f;
                            hasJumped = true;
                        }
                        else
                        {
                            player.position.Y -= jumpStrength;
                            player.velocity.Y = -8f;
                            hasJumped = true;
                            // Console.WriteLine("jumped");
                        }
<<<<<<< Updated upstream
=======
                        playerJump.Play();
                        player.sprite.PlayAnimation(jumpAnimation);
                    }
>>>>>>> Stashed changes
                    else if (player.IsColliding(nextPosition, gameBoard) == true && hasJumped == false)
                    {
                        playerJump.Play();
                        player.position.Y = (20 - gameBoard.Blocks[player.collideBlock].Y) * 32 * 3 / 2;
                        player.velocity.Y = 0;
                        hasJumped = true;
                        if (belongToCurrent(gameBoard.Blocks[player.collideBlock]) && currentTetromino.IsFalling == true)
                        {
                            GameState = STATE_GAMEOVER;
                        }
                    }
                }

                temptVelocity.Y = player.velocity.Y + gravity;
                nextPosition = new Vector2(player.position.X, player.position.Y + temptVelocity.Y);
                // Console.WriteLine(nextPosition);
                if (player.IsColliding(nextPosition, gameBoard) == false)
                {
                    player.velocity.Y = temptVelocity.Y;
                }
                else if (player.IsColliding(nextPosition, gameBoard) == true && player.velocity.Y < 0)
                {
                    player.position.Y = (20 - gameBoard.Blocks[player.collideBlock].Y) * 32 * 3 / 2;
                    player.velocity.Y = 0;
                }
                else if (player.IsColliding(nextPosition, gameBoard) == true && player.velocity.Y > 0)
                {
                    hasJumped = false;
                    player.position.Y = (20 - gameBoard.Blocks[player.collideBlock].Y - 2) * 32 * 3 / 2;
                    player.velocity.Y = 0;
                }
                player.position.Y += player.velocity.Y;

                if (player.position.Y + 32 * 3 / 2 <= endHeight)
                {
                    if (player.position.X >= (1120 - 320 - 32) * 3 / 2)
                    {
                        player.position.X = (1120 - 320 - 32) * 3 / 2;
                        player.velocity.X = 0;
                    }
                    else if (player.position.X <= 320 * 3 / 2)
                    {
                        player.position.X = 320 * 3 / 2;
                        player.velocity.X = 0;
                    }
                }

                if (endHeight + 160 * 3 / 2 <= (640 - 160) * 3 / 2)
                {
                    if (player.position.Y >= endHeight + 160 * 3 / 2 && player.position.Y + 32 * 3 / 2 <= (640 - 160) * 3 / 2)
                    {
                        if (player.position.X >= (1120 - 320 - 32) * 3 / 2)
                        {
                            player.position.X = (1120 - 320 - 32) * 3 / 2;
                            player.velocity.X = 0;
                        }
                        else if (player.position.X <= 320 * 3 / 2)
                        {
                            player.position.X = 320 * 3 / 2;
                            player.velocity.X = 0;
                        }
                    }
                    else if (player.position.Y >= (640 - 160) * 3 / 2)
                    {
                        if (player.position.X >= (1120 - 320 - 32) * 3 / 2)
                        {
                            player.position.X = (1120 - 320 - 32) * 3 / 2;
                            player.velocity.X = 0;
                        }
                        else if (player.position.X <= 160 * 3 / 2 && outOfStart == false)
                        {
                            player.position.X = 160 * 3 / 2;
                            player.velocity.X = 0;
                        }
                        else if (player.position.X <= 320 * 3 / 2 && outOfStart == true)
                        {
                            player.position.X = 320 * 3 / 2;
                            player.velocity.X = 0;
                        }
                    }
                }
                else if (endHeight + 160 * 3 / 2 > (640 - 160) * 3 / 2)
                {
                    if (player.position.Y + 32 * 3 / 2 <= (640 - 160) * 3 / 2 && player.position.Y >= endHeight)
                    {
                        if (player.position.X >= (1120 - 160 - 32) * 3 / 2)
                        {
                            player.position.X = (1120 - 160 - 32) * 3 / 2;
                            player.velocity.X = 0;
                        }
                        else if (player.position.X <= 320 * 3 / 2)
                        {
                            player.position.X = 320 * 3 / 2;
                            player.velocity.X = 0;
                        }
                    }
                    else if (player.position.Y + 32 * 3 / 2 < (640 - 160) * 3 / 2 && player.position.Y >= endHeight + 160 * 3 / 2)
                    {
                        if (player.position.X >= (1120 - 160 - 32) * 3 / 2)
                        {
                            player.position.X = (1120 - 160 - 32) * 3 / 2;
                            player.velocity.X = 0;
                        }
                        else if (player.position.X <= 160 * 3 / 2 && outOfStart == false)
                        {
                            player.position.X = 160 * 3 / 2;
                            player.velocity.X = 0;
                        }
                        else if (player.position.X <= 320 * 3 / 2 && outOfStart == true)
                        {
                            player.position.X = 320 * 3 / 2;
                            player.velocity.X = 0;
                        }
                    }
                    else if (player.position.Y >= endHeight + 160 * 3 / 2)
                    {
                        if (player.position.X >= (1120 - 320 - 32) * 3 / 2)
                        {
                            player.position.X = (1120 - 320 - 32) * 3 / 2;
                            player.velocity.X = 0;
                        }
                        else if (player.position.X <= 160 * 3 / 2 && outOfStart == false)
                        {
                            player.position.X = 160 * 3 / 2;
                            player.velocity.X = 0;
                        }
                        else if (player.position.X <= 320 * 3 / 2 && outOfStart == true)
                        {
                            player.position.X = 320 * 3 / 2;
                            player.velocity.X = 0;
                        }
                    }
                }

                if (player.position.Y + player.velocity.Y >= (640 - 32) * 3 / 2)
                {
                    hasJumped = false;
                    player.position.Y = (640 - 32) * 3 / 2;
                    player.velocity.Y = 0;
                }

                if (player.position.X + 32 * 3 / 2 > (320+480)*3/2 && player.position.Y + player.velocity.Y >= endHeight + (160 - 32) * 3 / 2)
                {
                    hasJumped = false;
                    player.position.Y = endHeight + (160 - 32) * 3 / 2;
                    player.velocity.Y = 0;
                }

                SoundEffectInstance Defeat = playerDefeat.CreateInstance();
                Defeat.IsLooped = false;

                if (player.TopColliding(player.position.X, player.position.Y, gameBoard) == true)
                {
                    if (belongToCurrent(gameBoard.Blocks[player.collideIndex]) && currentTetromino.IsFalling == true)
                    {
<<<<<<< Updated upstream
                        GameState = STATE_GAMEOVER;
                    }
                }
                else if (player.position.X > (320 + 480) * 3 / 2 && player.position.X < (320 + 480 + 160) * 3 / 2 && player.position.Y > endHeight && player.position.Y < endHeight + (160 - 32) * 3 / 2)
=======
                        gamePlayMusic.Stop();
                        Defeat.Play();
                        player.sprite.PlayAnimation(dieAnimation);
                        dieTime = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }

                if (player.sprite.Animation == dieAnimation && gameTime.TotalGameTime.TotalMilliseconds - dieTime > 300)
                {
                    GameState = STATE_GAMEOVER;
                }

                if (player.position.X >= (320 + 480) * 3 / 2 && player.position.X <= (320 + 480 + 160 - 32) * 3 / 2 && player.position.Y >= endHeight && player.position.Y <= endHeight + (160 - 32) * 3 / 2)
                {
                    player.velocity.X = 0;
                    player.sprite.PlayAnimation(winAnimation);
                    if (winTimeCount == false)
                    {
                        playerVictory.Play();
                        winTime = gameTime.TotalGameTime.TotalMilliseconds;
                        winTimeCount = true;
                    }
                    // GameState = STATE_GAMEWON;
                }

                Console.WriteLine(winTime);
                if (player.sprite.Animation == winAnimation && gameTime.TotalGameTime.TotalMilliseconds - winTime > 100)
>>>>>>> Stashed changes
                {
                    GameState = STATE_GAMEWON;
                    // player.position.Y = endHeight+40;
                    // player.velocity.Y = 0;
                    // hasJumped = false;
                }
            }

            if (GameState == STATE_PAUSED)
            {
                player.velocity = Vector2.Zero;
            }

            base.Update(gameTime);
        }

        protected override void Draw (GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            if (GameState == STATE_MENU)
                spriteBatch.Draw(logo, new Rectangle(104, 80, 1472, 224), Color.White);
            buttonStart.Draw(spriteBatch);
            buttonExit.Draw(spriteBatch);
            buttonResume.Draw(spriteBatch);
            buttonOutline.Draw(spriteBatch);
            buttonRestart.Draw(spriteBatch);

            if (GameState == STATE_PLAYING)
            {
                spriteBatch.Draw(background, new Rectangle(320 * 3 / 2, 0, 480 * 3 / 2, 640 * 3 / 2), Color.White);
                spriteBatch.Draw(goal, new Rectangle((1120 - 320) * 3 / 2, endHeight, 160 * 3 / 2, 160 * 3 / 2), Color.White);
                // spriteBatch.Draw(ground, new Vector2(250, 940), Color.White);
                // spriteBatch.Draw(ground, new Vector2(0, 940), Color.White);
                // spriteBatch.Draw(ground1, new Vector2(570, 808), Color.White);
                spriteBatch.Draw(StartBackground, new Rectangle(160 * 3 / 2, (640 - 160) * 3 / 2, 160 * 3 / 2, 160 * 3 / 2), Color.White);
                spriteBatch.Draw(startPlace, new Rectangle(160 * 3 / 2, (640 - 160) * 3 / 2, 160 * 3 / 2, 160 * 3 / 2), Color.White);
                spriteBatch.Draw(nextPiece, new Rectangle(32 * 3 / 2, 32 * 3 / 2, 256 * 3 / 2, 256 * 3 / 2), Color.White);
                spriteBatch.Draw(skipBoard, new Rectangle(32 * 3 / 2, 320 * 3 / 2, 256 * 3 / 2, 128 * 3 / 2), Color.White);
                if (skip == 0)
                    spriteBatch.Draw(skip1, new Rectangle(32 * 3 / 2, 320 * 3 / 2, 256 * 3 / 2, 128 * 3 / 2), Color.White);
                if (skip <= 1)
                    spriteBatch.Draw(skip2, new Rectangle(32 * 3 / 2, 320 * 3 / 2, 256 * 3 / 2, 128 * 3 / 2), Color.White);
                if (skip <= 2)
                    spriteBatch.Draw(skip3, new Rectangle(32 * 3 / 2, 320 * 3 / 2, 256 * 3 / 2, 128 * 3 / 2), Color.White);

                // Draw the board
                gameBoard.Draw(spriteBatch, BoardLocation, texture1px);
                for (int k = 0; k < nextBlockBoards.Length; k++)
                    nextBlockBoards[k].Draw(spriteBatch, nextBlockBoardsLocation[k], texture1px);

                // Draw Game Info
                // Lines Cleared
                // spriteBatch.DrawString(GameFont, String.Format("Lines Cleared: {0}", Lines), new Vector2(30, 450), Color.White);

                // Draw the player
                player.Draw(spriteBatch);
                /* if (playerShouldRemove == false)
                {
                    player.Draw(spriteBatch);
                } */
            }

            if (GameState == STATE_GAMEOVER)
            {
                // Draw game over screen
<<<<<<< Updated upstream
                spriteBatch.DrawString(GameFont, "Game Over!", new Vector2(250, 210), Color.Red);
=======
                spriteBatch.DrawString(GameFont, "You Lose!", new Vector2(450*3/2, 30*3/2), Color.Red);
                if (totalTime % 60 < 10)
                    spriteBatch.DrawString(GameFont, "Time: " + Math.Truncate(totalTime / 60) + " : 0" + Math.Round(totalTime % 60, 2, MidpointRounding.ToEven), new Vector2(450 * 3 / 2, 110 * 3 / 2), Color.White);
                else if (totalTime % 60 >= 10)
                    spriteBatch.DrawString(GameFont, "Time: " + Math.Truncate(totalTime / 60) + " : " + Math.Round(totalTime % 60, 2, MidpointRounding.ToEven), new Vector2(450 * 3 / 2, 110 * 3 / 2), Color.White);
                spriteBatch.DrawString(GameFont, "Lines: " + Lines, new Vector2(450 * 3 / 2, 190 * 3 / 2), Color.White);
                spriteBatch.Draw(lose, new Rectangle(480 * 3 / 2, 260 * 3 / 2, 160 * 3 / 2, 160 * 3 / 2), Color.White);
>>>>>>> Stashed changes
            }

            // Display the debug Window
            // DrawDebugWindow(spriteBatch);

            if (GameState == STATE_GAMEWON)
            {
                // Draw game over screen
<<<<<<< Updated upstream
                spriteBatch.DrawString(GameFont, "Game Won!", new Vector2(250, 210), Color.Yellow);
=======
                spriteBatch.DrawString(GameFont, "You Win!", new Vector2(450 * 3 / 2, 30 * 3 / 2), Color.Yellow);
                if (totalTime % 60 < 10)
                    spriteBatch.DrawString(GameFont, "Time: " + Math.Truncate(totalTime / 60) + " : 0" + Math.Round(totalTime % 60, 2, MidpointRounding.ToEven), new Vector2(450 * 3 / 2, 110 * 3 / 2), Color.White);
                else if (totalTime % 60 >= 10)
                    spriteBatch.DrawString(GameFont, "Time: " + Math.Truncate(totalTime / 60) + " : " + Math.Round(totalTime % 60, 2, MidpointRounding.ToEven), new Vector2(450 * 3 / 2, 110 * 3 / 2), Color.White);
                spriteBatch.DrawString(GameFont, "Lines: " + Lines, new Vector2(450 * 3 / 2, 190 * 3 / 2), Color.White);
                spriteBatch.Draw(win, new Rectangle(480 * 3 / 2, 260 * 3 / 2, 160 * 3 / 2, 160 * 3 / 2), Color.White);
>>>>>>> Stashed changes
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public static char GetRandomCharacter(string text, Random rng)
        {
            int index = rng.Next(text.Length);
            return text[index];
        }

        public Tetromino GenerateNewTetromino(char name)
        {
            int x = 5, y = 18;
            return new Tetromino(gameBoard, x, y, name, BlockTextures[name]);
        }

        public void DrawDebugWindow(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(
                GameFont,
                String.Format("Tetromino: {1}{0}X: {2}, Y: {3}{0}PlayerX: {4}{0}PlayerY: {5}{0}nextX:{6}{0}nextY:{7}{0}velocityX:{8}{0}velocityY:{9}{0}hasJumped:{10}{0}IsFalling: {11}{0}Next: {12}",
                Environment.NewLine,
                currentTetromino?.Tag,
                currentTetromino?.X,
                currentTetromino?.Y,
                player?.position.X,
                player?.position.Y,
                nextPosition.X,
                nextPosition.Y,
                player?.velocity.X,
                player?.velocity.Y,
                hasJumped,
                currentTetromino?.IsFalling,
                string.Join(" ", nextTetrominos.ToArray())),
                new Vector2(10, 30),
                Color.GreenYellow);
        }

        public bool belongToCurrent(Block Block)
        {
            foreach (Block b in currentTetromino.Blocks)
            {
                if (b.X == Block.X && b.Y == Block.Y)
                    return true;
            }
            return false;
        }

        private bool Clicked(ref MouseState mouseState, MenuObject button)
        {
            return ButtonIntersects(ref mouseState, button) && (mouseState.LeftButton == ButtonState.Pressed);
        }

        private bool ButtonIntersects(ref MouseState mouseState, MenuObject button)
        {
            if (GameState == STATE_MENU || GameState == STATE_PAUSED || GameState == STATE_GAMEOVER || GameState == STATE_GAMEWON)
                return button.BoundingBox.Contains(mouseState.Position);
            return false;
        }

        private void UpdateState()
        {
            switch (GameState)
            {
                case STATE_MENU:
                    break;
                default:
                    break;
            }
        }

        private Vector2 HAlignedTextureRectangle(Texture2D texture, int height)
        {
            return new Vector2(
                (GraphicsDevice.Viewport.Width - texture.Width) / 2,
                height
            );
        }

        protected void PlayMusic(int i)
        {
            if (currentMusic == i)
            {
                return;
            }
            switch (i)
            {
                case 0:
                    currentMusic = 0;
                    menuMusic.PlayLooping();
                    break;
                case 1:
                    currentMusic = 1;
                    gamePlayMusic.PlayLooping();
                    break;
                case 2:
                    currentMusic = 2;
                    gamePlayMusic.Stop();
                    break;
            }
        }

    }

}
