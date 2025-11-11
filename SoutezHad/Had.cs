using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SoutezHad
{
    public class Snake
    {
        private List<Point> segments;
        private Point direction;
        private Point nextDirection;
        private int gridSize;
        private bool grow;

        public Snake(int startX, int startY, int gridSize)
        {
            this.gridSize = gridSize;
            segments = new List<Point>();

            // Začínáme se třemi segmenty
            segments.Add(new Point(startX, startY));
            segments.Add(new Point(startX - 1, startY));
            segments.Add(new Point(startX - 2, startY));

            direction = new Point(1, 0); // Pohyb doprava
            nextDirection = direction;
            grow = false;
        }

        public Point Head => segments[0];

        public IReadOnlyList<Point> Segments => segments;

        public void SetDirection(Point newDirection)
        {
            // Zabránit otočení o 180 stupňů
            if (newDirection.X != -direction.X || newDirection.Y != -direction.Y)
            {
                nextDirection = newDirection;
            }
        }

        public void Update()
        {
            direction = nextDirection;

            // Nová pozice hlavy
            Point newHead = new Point(Head.X + direction.X, Head.Y + direction.Y);
            segments.Insert(0, newHead);

            // Pokud had nemusí růst, odstraníme ocas
            if (!grow)
            {
                segments.RemoveAt(segments.Count - 1);
            }
            else
            {
                grow = false;
            }
        }

        public void Grow()
        {
            grow = true;
        }

        public bool CheckSelfCollision()
        {
            // Kontrola kolize hlavy s tělem
            for (int i = 1; i < segments.Count; i++)
            {
                if (segments[i] == Head)
                    return true;
            }
            return false;
        }

        public bool CheckWallCollision(int gridWidth, int gridHeight)
        {
            return Head.X < 0 || Head.X >= gridWidth ||
                   Head.Y < 0 || Head.Y >= gridHeight;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Color color)
        {
            foreach (Point segment in segments)
            {
                Rectangle rect = new Rectangle(
                    segment.X * gridSize,
                    segment.Y * gridSize,
                    gridSize,
                    gridSize
                );
                spriteBatch.Draw(texture, rect, color);
            }
        }
    }
}