using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SoutezHad
{
    public enum AppleType
    {
        Red,
        Blue,
        Purple
    }

    public class Apple
    {
        public Point Position { get; private set; }
        public AppleType Type { get; private set; }

        private int gridSize;
        private static Random random = new Random();

        public Apple(int gridWidth, int gridHeight, int gridSize)
        {
            this.gridSize = gridSize;
            Respawn(gridWidth, gridHeight);
        }

        public void Respawn(int gridWidth, int gridHeight)
        {
            Position = new Point(random.Next(gridWidth), random.Next(gridHeight));

            // 70% red, 20% blue, 10% purple
            int r = random.Next(100);
            if (r < 70)
                Type = AppleType.Red;
            else if (r < 90)
                Type = AppleType.Blue;
            else
                Type = AppleType.Purple;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel, float pulse)
        {
            float pulseScale = 0.8f + (float)Math.Sin(pulse) * 0.2f;
            int foodSize = (int)(gridSize * pulseScale);
            int offset = (gridSize - foodSize) / 2;

            Rectangle rect = new Rectangle(
                Position.X * gridSize + offset,
                Position.Y * gridSize + offset,
                foodSize,
                foodSize
            );

            Color color = Type switch
            {
                AppleType.Red => Color.Crimson,
                AppleType.Blue => Color.CornflowerBlue,
                AppleType.Purple => Color.MediumPurple,
                _ => Color.White
            };

            spriteBatch.Draw(pixel, rect, color);

            // malý highlight efekt
            Rectangle highlight = new Rectangle(
                rect.X + 3, rect.Y + 3, foodSize / 3, foodSize / 3
            );
            spriteBatch.Draw(pixel, highlight, Color.White * 0.6f);
        }
    }
}
