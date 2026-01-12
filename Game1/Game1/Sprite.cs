using System;
using Game1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;           // <--- REQUIRED for Vector2, Rectangle, Color
using Microsoft.Xna.Framework.Graphics;  // <--- REQUIRED for Texture2D, SpriteBatch
using Microsoft.Xna.Framework.Input;

namespace Game1
{
    public class Sprite
    {
        public Texture2D Texture;
        public Vector2 Position;
        public bool IsActive = true; // For removing items
        public int Value = 10;       // For scoring
        public float Alpha = 1.0f;   // For fading (2023/24 requirement)

        // Constructor
        public Sprite(Texture2D texture, Vector2 position)
        {
            Texture = texture;
            Position = position;
        }

        // Collision Box
        public Rectangle Bounds
        {
            get { return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height); }
        }

        public virtual void Update(GameTime gameTime)
        {
            // Fading Logic (If required by Q1d)
            // Alpha -= (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;
            // if (Alpha <= 0) IsActive = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
                spriteBatch.Draw(Texture, Position, Color.White * Alpha);
        }
    }
}

