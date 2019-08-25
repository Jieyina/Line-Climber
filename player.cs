using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    class player
    {
        public Texture2D texture;
        public Vector2 position;
        public Vector2 nextPosition;
        public Vector2 velocity;
        public int collideBlock;

        public player(Texture2D playerTexture, Vector2 playerPosition)
        {
            texture = playerTexture;
            position = playerPosition;
        }
        /* public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                velocity.X = speed;
                nextPosition = new Vector2(position.X + velocity.X, position.Y);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.A) && position.X > 250)
            {
                velocity.X = -speed;
                nextPosition = new Vector2(position.X + velocity.X, position.Y);
            }
            else
                velocity.X = 0f;
            if (Keyboard.GetState().IsKeyDown(Keys.W) && hasJumped == false)
            {
                nextPosition = new Vector2(position.X, position.Y - jumpStrength);
                position.Y -= jumpStrength;
                velocity.Y = -5f;
                hasJumped = true;
                falling = true;
            }

            if (falling == true)
            {
                velocity.Y += gravity;
                nextPosition = new Vector2(position.X, position.Y + velocity.Y);
            }

            if (Math.Ceiling(position.Y + texture.Height + velocity.Y) >= 968)
            {
                falling = false;
                hasJumped = false;
                position.Y = 936;
            }

            if (falling == false)
            {
                velocity.Y = 0f;
            }
            position += velocity;
            // Console.WriteLine(nextPosition);
            
        } */

        public bool IsColliding(Vector2 nextPosition, Board Board)
        {
            float rightX = nextPosition.X + 32;
            float leftX = nextPosition.X;
            float bottomY = nextPosition.Y + 32;
            float topY = nextPosition.Y;
            for (int i = 0; i < Board.Blocks.Count; i++)
            {
                float rightEdge = 250 + Board.Blocks[i].X * 32 + 32;
                float leftEdge = 250 + Board.Blocks[i].X * 32;
                float bottomEdge = 200 + (24 - Board.Blocks[i].Y) * 32;
                float topEdge = 200 + (24 - Board.Blocks[i].Y) * 32 - 32;
                if (rightX <= leftEdge || leftX >= rightEdge || bottomY <= topEdge || topY >= bottomEdge)
                {
                    int check = i + 1;
                }
                else
                {
                    collideBlock = i;
                    return true;
                }
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position1)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }
    }
}

