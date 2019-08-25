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
        Vector2 position1;
        Rectangle BoardLocation = new Rectangle(250, 200, 320, 768);
        Rectangle nextBlockBoardsLocation = new Rectangle(575, 100, 80, 80);

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Textures
        Dictionary<char, Texture2D> BlockTextures;
        Texture2D boardRect;
        Texture2D texture1px;
        Texture2D background;
        
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

        //double 
        double lastActionTime = 0; // lastUpdate time in ms
        double lastGravityEffectTime = 0;
        double ActionDelay = 150; // delay bewteen two actions in ms


        Queue<char> nextTetrominos = new Queue<char>();
        string CHARLIST = "IOJLZTS";

        // Status
        bool GameOver = false;

        // Player related
        float speed = 3f;
        float jumpStrength = 10f;
        float gravity = 0.4f;
        bool hasJumped;
        bool falling;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 820;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 1010;   // set this value to the desired height of your window
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
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

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
            player = new player(Content.Load<Texture2D>("Images/donekindof"), new Vector2(100, 700));

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
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            // Exit check
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (GameOver)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    // Restart the game
                    Score = 0;
                    Lines = 0;

                    // Reset the queue of next tetromino
                    nextTetrominos = new Queue<char>();
                    nextTetrominos.Enqueue(GetRandomCharacter(CHARLIST, new Random()));
                    // Reset the board
                    gameBoard.Reset();
                    GameOver = false;
                }    
                return;
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
                // -----------------------------------------
                // Teleportation to ghost position
                // -----------------------------------------
                /* if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    currentTetromino?.MoveTo(currentTetromino.Xghost, currentTetromino.Yghost);
                    if (currentTetromino != null)
                        currentTetromino.IsFalling = false;
                    lastActionTime = gameTime.TotalGameTime.TotalMilliseconds;
                } */
            }

            currentTetromino?.Update(gameTime);
            // Row check
            if (currentTetromino != null && !currentTetromino.IsFalling)
            {
                // If the tetromino is outside 
                if (currentTetromino.Y >= 20)
                    GameOver = true;
          
                // Get the row to remove
                int rowCleared = gameBoard.ClearRow();
                if (rowCleared > 0)
                {
                    // Increase Score
                    Score +=  (Level + 1) * 100 * (int) Math.Pow(2, rowCleared);
                    // Update Lines
                    Lines += rowCleared;
                }
            }

            // player movement
            Console.WriteLine(player.IsColliding(player.nextPosition, gameBoard));
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if (player.IsColliding(player.nextPosition, gameBoard) == false && player.position.X < 538)
                {
                    player.velocity.X = speed;
                    player.nextPosition = new Vector2(player.position.X + player.velocity.X, player.position.Y);
                }
                else if (player.IsColliding(player.nextPosition, gameBoard) == true)
                {
                    player.position.X = 250 + gameBoard.Blocks[player.collideBlock].X * 32 - 32;
                    player.velocity.X = 0;
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.A) && player.position.X > 250)
            {
                player.velocity.X = -speed;
                player.nextPosition = new Vector2(player.position.X + player.velocity.X, player.position.Y);
            }
            else
                player.velocity.X = 0f;
            if (Keyboard.GetState().IsKeyDown(Keys.W) && hasJumped == false)
            {
                player.nextPosition = new Vector2(player.position.X, player.position.Y - jumpStrength);
                player.position.Y -= jumpStrength;
                player.velocity.Y = -5f;
                hasJumped = true;
                falling = true;
            }

            if (falling == true)
            {
                player.velocity.Y += gravity;
                player.nextPosition = new Vector2(player.position.X, player.position.Y + player.velocity.Y);
            }

            if (Math.Ceiling(player.position.Y + player.texture.Height + player.velocity.Y) >= 968)
            {
                falling = false;
                hasJumped = false;
                player.position.Y = 936;
            }

            if (falling == false)
            {
                player.velocity.Y = 0f;
            }
            player.position += player.velocity;
            // Console.WriteLine(nextPosition);

            base.Update(gameTime);
        }

        protected override void Draw (GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            // Draw the background
            spriteBatch.Draw(background, new Rectangle(250, 200, 320, 768), Color.White);
            // Draw the ghost piece
            // currentTetromino?.DrawGhost(spriteBatch, BoardLocation);
            // Draw the board
            gameBoard.Draw(spriteBatch, BoardLocation, texture1px);
            nextBlockBoards.Draw(spriteBatch, nextBlockBoardsLocation, texture1px);
            // Draw Game Info
        
            // Lines Cleared
            spriteBatch.DrawString(GameFont, String.Format("Cleared: {0}", Lines), new Vector2(50, 160), Color.White);

            // Draw the player
            player.Draw(spriteBatch, position1);


            if (GameOver)
            {
                // Draw game over screen
                spriteBatch.DrawString(GameFont, "Game Over!\nPress Enter to restart.", new Vector2(50, 210), Color.Red);
            }

            // Display the debug Window
            // DrawDebugWindow(spriteBatch);

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
            int x = 5, y = 20;
            return new Tetromino(gameBoard, x, y, name, BlockTextures[name]);
        }

        public void DrawDebugWindow(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(
                GameFont,
                String.Format("Tetromino: {1}{0}X: {2}, Y: {3}{0}Rotation Center: {4}{0}Rotation State: {5}{0}IsFalling: {6}{0}Level: {7}{0}Score: {8}{0}Lines: {9}{0}Speed: {10}{0}Next: {11}{0}Game over: {12}",
                Environment.NewLine,
                currentTetromino?.Tag,
                currentTetromino?.X,
                currentTetromino?.Y,
                currentTetromino?.RotCenter.ToString(),
                currentTetromino?.RotStatus,
                currentTetromino?.IsFalling,
                Level,
                Score,
                Lines,
                Speed,
                string.Join(" ", nextTetrominos.ToArray()),
                GameOver),
                new Vector2(10, 300),
                Color.GreenYellow);
        }

        /* public bool collideBelow(Vector2 position, Block Block)
        {
            if (player.IsColliding(player.nextPosition, gameBoard) == true && player.velocity.Y > 0)
                return true;
            else
                return false;
        } */
    }

}
