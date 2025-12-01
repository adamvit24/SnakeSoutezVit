using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SoutezHad
{
    public class PowerUp : IRenderable
    {
        public Point Position { get; set; }
        public bool IsActive { get; set; }

        private float rotationTimer;
        private float pulseTimer;
        private const float ROTATION_SPEED = 2f;
        private const float PULSE_SPEED = 4f;

        public PowerUp(Point position)
        {
            Position = position;
            IsActive = true;
            rotationTimer = 0;
            pulseTimer = 0;
        }

        public void Update(GameTime gameTime)
        {
            rotationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * ROTATION_SPEED;
            pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * PULSE_SPEED;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel, int gridSize)
        {
            if (!IsActive) return;

            float pulseScale = 0.7f + (float)Math.Sin(pulseTimer) * 0.3f;
            int size = (int)(gridSize * pulseScale);
            int offset = (gridSize - size) / 2;

            Rectangle rect = new Rectangle(
                Position.X * gridSize + offset,
                Position.Y * gridSize + offset,
                size,
                size
            );

            // Vnější záře
            int glowSize = size + 6;
            int glowOffset = (gridSize - glowSize) / 2;
            Rectangle glow = new Rectangle(
                Position.X * gridSize + glowOffset,
                Position.Y * gridSize + glowOffset,
                glowSize,
                glowSize
            );
            spriteBatch.Draw(pixel, glow, Color.Gold * 0.3f);

            // Hlavní power-up (hvězda efekt)
            spriteBatch.Draw(pixel, rect, Color.Gold);

            // Rotující křížek uvnitř
            float rotation = rotationTimer;
            int crossSize = size / 2;
            int centerX = Position.X * gridSize + gridSize / 2;
            int centerY = Position.Y * gridSize + gridSize / 2;

            // Horizontální čára
            Rectangle hLine = new Rectangle(
                centerX - crossSize / 2,
                centerY - 1,
                crossSize,
                3
            );
            spriteBatch.Draw(pixel, hLine, Color.White);

            // Vertikální čára
            Rectangle vLine = new Rectangle(
                centerX - 1,
                centerY - crossSize / 2,
                3,
                crossSize
            );
            spriteBatch.Draw(pixel, vLine, Color.White);

            // Malé hvězdičky kolem
            for (int i = 0; i < 4; i++)
            {
                float angle = rotation + (i * MathHelper.PiOver2);
                int starX = centerX + (int)(Math.Cos(angle) * (gridSize / 2 + 3));
                int starY = centerY + (int)(Math.Sin(angle) * (gridSize / 2 + 3));
                Rectangle star = new Rectangle(starX - 1, starY - 1, 2, 2);
                spriteBatch.Draw(pixel, star, Color.Yellow);
            }
        }
    }
}