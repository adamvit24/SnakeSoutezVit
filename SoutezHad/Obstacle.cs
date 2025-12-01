using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoutezHad
{
    public class Obstacle : IRenderable
    {
        public Point Position { get; private set; }
        public Color Color { get; private set; } = new Color(90, 90, 90); // šedá překážka

        public Obstacle(Point position)
        {
            Position = position;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel, int gridSize)
        {
            Rectangle rect = new Rectangle(
                Position.X * gridSize,
                Position.Y * gridSize,
                gridSize,
                gridSize
            );

            spriteBatch.Draw(pixel, rect, Color);
        }
    }
}