using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System;
using Game1;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using Microsoft.Xna.Framework;           // <--- REQUIRED for Vector2, Rectangle, Color
using Microsoft.Xna.Framework.Graphics;  // <--- REQUIRED for Texture2D, SpriteBatch
using Microsoft.Xna.Framework.Input;

namespace Game1
{
    public class Camera
    {
        public Matrix Transform { get; private set; }

        public void Follow(Vector2 target, int screenW, int screenH, int worldW, int worldH)
        {
            // 1. Center on target
            float x = target.X - (screenW / 2);
            float y = target.Y - (screenH / 2);

            // 2. Clamp to world bounds (The "Rabbit Hole" preventer)
            // Ensures camera doesn't show black space outside background
            x = MathHelper.Clamp(x, 0, worldW - screenW);
            y = MathHelper.Clamp(y, 0, worldH - screenH);

            // 3. Create Matrix
            Transform = Matrix.CreateTranslation(-x, -y, 0);
        }
    }

}
