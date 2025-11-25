using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SoutezHad
{
    public enum FruitType
    {
        Apple,      // 10 bodů
        Melon,      // 30 bodů
        Kiwi        // 20 bodů
    }

    public class Fruit
    {
        public Point Position { get; set; }
        public FruitType Type { get; private set; }
        public int Score { get; private set; }
        public Color Color { get; private set; }
        public Color HighlightColor { get; private set; }

        private float pulseTimer;
        private const float PULSE_SPEED = 3f;

        public Fruit(Point position, FruitType type)
        {
            Position = position;
            Type = type;
            pulseTimer = 0;

            switch (type)
            {
                case FruitType.Apple:
                    Score = 10;
                    Color = Color.Crimson;
                    HighlightColor = Color.Pink;
                    break;

                case FruitType.Melon:
                    Score = 30;
                    Color = new Color(0, 180, 0); // zelený meloun
                    HighlightColor = new Color(150, 255, 150);
                    break;

                case FruitType.Kiwi:
                    Score = 20;
                    Color = new Color(110, 200, 90); // kiwi zeleno-hnědá
                    HighlightColor = new Color(180, 255, 160);
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * PULSE_SPEED;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel, int gridSize)
        {
            float pulseScale = 0.8f + (float)Math.Sin(pulseTimer) * 0.2f;
            int size = (int)(gridSize * pulseScale);
            int offset = (gridSize - size) / 2;

            Rectangle rect = new Rectangle(
                Position.X * gridSize + offset,
                Position.Y * gridSize + offset,
                size,
                size
            );

            spriteBatch.Draw(pixel, rect, Color);

            // Highlight
            Rectangle highlight = new Rectangle(
                rect.X + 3,
                rect.Y + 3,
                size / 3,
                size / 3
            );
            spriteBatch.Draw(pixel, highlight, HighlightColor);

            // SPECIÁLNÍ MELON EFEKT (pruhování)
            if (Type == FruitType.Melon)
            {
                int stripeCount = 4;
                int stripeWidth = size / (stripeCount * 2);

                for (int i = 0; i < stripeCount; i++)
                {
                    Rectangle stripe = new Rectangle(
                        rect.X + i * stripeWidth * 2,
                        rect.Y,
                        stripeWidth,
                        size
                    );
                    spriteBatch.Draw(pixel, stripe, new Color(0, 100, 0));
                }
            }

            // SPECIÁLNÍ KIWI EFEKT (střed + semínka)
            if (Type == FruitType.Kiwi)
            {
                // Střed
                Rectangle center = new Rectangle(
                    rect.X + size / 4,
                    rect.Y + size / 4,
                    size / 2,
                    size / 2
                );
                spriteBatch.Draw(pixel, center, new Color(230, 255, 200));

                // "Semínka" (4 body)
                int seedSize = size / 8;
                spriteBatch.Draw(pixel, new Rectangle(center.X, center.Y, seedSize, seedSize), Color.Black);
                spriteBatch.Draw(pixel, new Rectangle(center.Right - seedSize, center.Y, seedSize, seedSize), Color.Black);
                spriteBatch.Draw(pixel, new Rectangle(center.X, center.Bottom - seedSize, seedSize, seedSize), Color.Black);
                spriteBatch.Draw(pixel, new Rectangle(center.Right - seedSize, center.Bottom - seedSize, seedSize, seedSize), Color.Black);
            }
        }
    }
}
