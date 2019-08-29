using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetris
{
    public class Game1 : Game
    {
        // Game constantes
        player player;
        Vector2 position1 = new Vector2(128, 800 - 16 - 32);
        Rectangle BoardLocation = new Rectangle(224, 16, 320, 768);
        Rectangle nextBlockBoardsLocation = new Rectangle(16+36, 480+50, 120, 120);

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

        // Menu textures
        GameObject buttonStart, buttonExit, buttonResume, buttonOutline, buttonRestart;

        // Fonts
        SpriteFont GameFont;

        // Game Objects
        public Board gameBoard;
        Board nextBlockBoards;

        Tetromino currentTetromino;
        Random randomGenerator;

        // Game parameters
        int Score = 0;
        int Lines = 0;
        float Speed => 5 + Level; 
        int Level => (int) Math.Floor((double)Lines / 10);
        int endHeight = 432;

        //double 
        double lastActionTime = 0; // lastUpdate time in ms
        double lastGravityEffectTime = 0;
        double ActionDelay = 150; // delay bewteen two actions in ms


        Queue<char> nextTetrominos = new Queue<char>();
        string CHARLIST = "IOJLZTS";

        // Player related
        float speed = 3f;
        float jumpStrength = 10f;
        float gravity = 0.4f;
        bool hasJumped;
        Vector2 temptVelocity;
        Vector2 nextPosition;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 656;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 800;   // set this value to the desired height of your window
            //graphics.IsFullScreen = true;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            BlockTextures = new Dictionary<char, Texture2D>();
            randomGenerator = new Random();
            gameBoard = new Board(24, 10);
            // Preview of next tetromino
            nextBlockBoards = new Board(4,4);
            char nextTetrominoTag = GetRandomCharacter(CHARLIST, randomGenerator);
            nextTetrominos.Enqueue(nextTetrominoTag);
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
            Texture2D outline = Content.Load<Texture2D>("menu/button_outline");
            Texture2D restart = Content.Load<Texture2D>("menu/button_restart");
            buttonStart = new GameObject(start, HAlignedTextureRectangle(start, 450));
            buttonExit = new GameObject(exit, HAlignedTextureRectangle(exit, 550));
            buttonResume = new GameObject(resume, HAlignedTextureRectangle(resume, 450));
            buttonOutline = new GameObject(outline, HAlignedTextureRectangle(outline, 0));
            buttonRestart = new GameObject(restart, HAlignedTextureRectangle(restart, 650));
            // buttonOutline.Disable(spriteBatch);
            // buttonRestart.Disable(spriteBatch);

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
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            if (ButtonIntersects(ref mouseState, buttonStart) || ButtonIntersects(ref mouseState, buttonExit) || ButtonIntersects(ref mouseState, buttonResume))
                buttonOutline.Enable(spriteBatch);
            else
                buttonOutline.Disable(spriteBatch);

            if (buttonStart.BoundingBox.Contains(mouseState.Position))
                buttonOutline.Position.Y = 450;
            if (buttonExit.BoundingBox.Contains(mouseState.Position))
                buttonOutline.Position.Y = 550;
            if (buttonResume.BoundingBox.Contains(mouseState.Position))
                buttonOutline.Position.Y = 450;
            if (buttonRestart.BoundingBox.Contains(mouseState.Position))
                buttonOutline.Position.Y = 650;

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
                    // Restart the game
                    Lines = 0;
                    // Reset the queue of next tetromino
                    nextTetrominos = new Queue<char>();
                    nextTetrominos.Enqueue(GetRandomCharacter(CHARLIST, new Random()));
                    // Reset the board
                    gameBoard.Reset();
                    hasJumped = false;
                    Restart = false;
                }

                // Tetromino generation
                if (currentTetromino == null || !currentTetromino.IsFalling)
                {
                    currentTetromino = GenerateNewTetromino(nextTetrominos.Dequeue());
                    nextTetrominos.Enqueue(GetRandomCharacter(CHARLIST, randomGenerator));

                    // Reset the nextBlockBoards
                    nextBlockBoards.Reset();
                    // add a tetromino in the board
                    new Tetromino(nextBlockBoards, 2, 1, nextTetrominos.ElementAt(0), BlockTextures[nextTetrominos.ElementAt(0)]);
                }

                // Apply gravity
                if (gameTime.TotalGameTime.TotalMilliseconds - lastGravityEffectTime > 1000 / Speed)
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
                    if (currentTetromino.Y >= 22)
                        GameState = STATE_GAMEOVER;

                    // Get the row to remove
                    int rowCleared = gameBoard.ClearRow();
                    if (rowCleared > 0)
                    {
                        // Increase Score
                        Score += (Level + 1) * 100 * (int)Math.Pow(2, rowCleared);
                        // Update Lines
                        Lines += rowCleared;
                    }
                }

                // player movement
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    temptVelocity.X = speed;
                    nextPosition = new Vector2(player.position.X + temptVelocity.X, player.position.Y);
                    if (player.IsColliding(nextPosition, gameBoard) == false)
                    {
                        player.velocity.X = temptVelocity.X;
                    }
                    else if (player.IsColliding(nextPosition, gameBoard) == true)
                    {
                        player.position.X = 224 + gameBoard.Blocks[player.collideBlock].X * 32 - 32;
                        player.velocity.X = 0;
                    }
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.A))
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
                        player.position.X = 224 + gameBoard.Blocks[player.collideBlock].X * 32 + 32;
                        player.velocity.X = 0;
                    }
                }
                else
                    player.velocity.X = 0;
                player.position.X += player.velocity.X;

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    nextPosition = new Vector2(player.position.X, player.position.Y - jumpStrength);
                    // Console.WriteLine("{0},{1},{2}",player.IsColliding(nextPosition, gameBoard), hasJumped, nextPosition);
                    if (player.IsColliding(nextPosition, gameBoard) == false && hasJumped == false)
                        if (nextPosition.Y <= 16)
                        {
                            player.position.Y = 16;
                            player.velocity.Y = -5f;
                            hasJumped = true;
                        }
                        else
                        {
                            player.position.Y -= jumpStrength;
                            player.velocity.Y = -5f;
                            hasJumped = true;
                            // Console.WriteLine("jumped");
                        }
                    else if (player.IsColliding(nextPosition, gameBoard) == true && hasJumped == false)
                    {
                        player.position.Y = 16 + (24 - gameBoard.Blocks[player.collideBlock].Y) * 32;
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
                    player.position.Y = 16 + (24 - gameBoard.Blocks[player.collideBlock].Y) * 32;
                    player.velocity.Y = 0;
                }
                else if (player.IsColliding(nextPosition, gameBoard) == true && player.velocity.Y > 0)
                {
                    hasJumped = false;
                    player.position.Y = 16 + (24 - gameBoard.Blocks[player.collideBlock].Y) * 32 - 64;
                    player.velocity.Y = 0;
                }
                player.position.Y += player.velocity.Y;

                if (player.position.Y <= endHeight)
                {
                    if (player.position.X >= 656 - 112 - 32)
                    {
                        player.position.X = 656 - 112 - 32;
                        player.velocity.X = 0;
                    }
                    else if (player.position.X <= 224)
                    {
                        player.position.X = 224;
                        player.velocity.X = 0;
                    }
                }

                if (player.position.Y >= endHeight + 96 && player.position.Y <= 800 - 16 - 96)
                {
                    if (player.position.X >= 656 - 112 - 32)
                    {
                        player.position.X = 656 - 112 - 32;
                        player.velocity.X = 0;
                    }
                    else if (player.position.X <= 224)
                    {
                        player.position.X = 224;
                        player.velocity.X = 0;
                    }

                }
                else if (player.position.Y >= 800 - 16 - 96)
                {
                    if (player.position.X >= 656 - 112 - 32)
                    {
                        player.position.X = 656 - 112 - 32;
                        player.velocity.X = 0;
                    }
                    else if (player.position.X <= 224 - 96)
                    {
                        player.position.X = 224 - 96;
                        player.velocity.X = 0;
                    }
                }

                if (player.position.Y + player.velocity.Y >= 800 - 16 - 32)
                {
                    hasJumped = false;
                    player.position.Y = 800 - 16 - 32;
                    player.velocity.Y = 0;
                }

                if (player.TopColliding(player.position.X, player.position.Y, gameBoard) == true)
                {
                    if (belongToCurrent(gameBoard.Blocks[player.collideIndex]) && currentTetromino.IsFalling == true)
                    { GameState = STATE_GAMEOVER; }
                }
                else if (player.position.X > 224 + 320 && player.position.X < 224 + 320 + 96 && player.position.Y > endHeight && player.position.Y < endHeight + 96 - 32)
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
                spriteBatch.Draw(logo, new Vector2(3, 50), Color.White);
            buttonStart.Draw(spriteBatch);
            buttonExit.Draw(spriteBatch);
            buttonResume.Draw(spriteBatch);
            buttonOutline.Draw(spriteBatch);
            buttonRestart.Draw(spriteBatch);

            if (GameState == STATE_PLAYING)
            {
                spriteBatch.Draw(background, new Rectangle(224, 16, 320, 768), Color.White);
                spriteBatch.Draw(goal, new Vector2(656 - 112, endHeight), Color.White);
                // spriteBatch.Draw(ground, new Vector2(250, 940), Color.White);
                // spriteBatch.Draw(ground, new Vector2(0, 940), Color.White);
                // spriteBatch.Draw(ground1, new Vector2(570, 808), Color.White);
                spriteBatch.Draw(StartBackground, new Vector2(128, 800 - 16 - 96), Color.White);
                spriteBatch.Draw(startPlace, new Vector2(128, 800 - 16 - 96), Color.White);
                spriteBatch.Draw(nextPiece, new Vector2(16, 480), Color.White);

                // Draw the board
                gameBoard.Draw(spriteBatch, BoardLocation, texture1px);
                nextBlockBoards.Draw(spriteBatch, nextBlockBoardsLocation, texture1px);

                // Draw Game Info
                // Lines Cleared
                spriteBatch.DrawString(GameFont, String.Format("Lines Cleared: {0}", Lines), new Vector2(30, 450), Color.White);

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
                spriteBatch.DrawString(GameFont, "Game Over!", new Vector2(250, 210), Color.Red);
            }

            // Display the debug Window
            // DrawDebugWindow(spriteBatch);

            if (GameState == STATE_GAMEWON)
            {
                // Draw game over screen
                spriteBatch.DrawString(GameFont, "Game Won!", new Vector2(30, 210), Color.Yellow);
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
            int x = 5, y = 22;
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

        private bool Clicked(ref MouseState mouseState, GameObject button)
        {
            return ButtonIntersects(ref mouseState, button) && (mouseState.LeftButton == ButtonState.Pressed);
        }

        private bool ButtonIntersects(ref MouseState mouseState, GameObject button)
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

    }

}
