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

        public Point Direction => direction;

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
            for (int i = 0; i < segments.Count; i++)
            {
                Point segment = segments[i];
                Rectangle rect = new Rectangle(
                    segment.X * gridSize + 1,
                    segment.Y * gridSize + 1,
                    gridSize - 2,
                    gridSize - 2
                );

                // Hlava má jinou barvu a je větší
                if (i == 0)
                {
                    spriteBatch.Draw(texture, rect, Color.YellowGreen);

                    // Oči
                    int eyeSize = 3;
                    int eyeOffset = 5;
                    Rectangle leftEye, rightEye;

                    if (direction.X == 1) // Doprava
                    {
                        leftEye = new Rectangle(rect.Right - eyeOffset, rect.Top + 4, eyeSize, eyeSize);
                        rightEye = new Rectangle(rect.Right - eyeOffset, rect.Bottom - 7, eyeSize, eyeSize);
                    }
                    else if (direction.X == -1) // Doleva
                    {
                        leftEye = new Rectangle(rect.Left + eyeOffset - eyeSize, rect.Top + 4, eyeSize, eyeSize);
                        rightEye = new Rectangle(rect.Left + eyeOffset - eyeSize, rect.Bottom - 7, eyeSize, eyeSize);
                    }
                    else if (direction.Y == -1) // Nahoru
                    {
                        leftEye = new Rectangle(rect.Left + 4, rect.Top + eyeOffset - eyeSize, eyeSize, eyeSize);
                        rightEye = new Rectangle(rect.Right - 7, rect.Top + eyeOffset - eyeSize, eyeSize, eyeSize);
                    }
                    else // Dolů
                    {
                        leftEye = new Rectangle(rect.Left + 4, rect.Bottom - eyeOffset, eyeSize, eyeSize);
                        rightEye = new Rectangle(rect.Right - 7, rect.Bottom - eyeOffset, eyeSize, eyeSize);
                    }

                    spriteBatch.Draw(texture, leftEye, Color.Black);
                    spriteBatch.Draw(texture, rightEye, Color.Black);
                }
                else
                {
                    // Tělo - gradient efekt
                    float intensity = 1f - (i / (float)segments.Count) * 0.5f;
                    Color bodyColor = new Color(
                        (int)(34 * intensity),
                        (int)(139 * intensity),
                        (int)(34 * intensity)
                    );
                    spriteBatch.Draw(texture, rect, bodyColor);
                }
            }
        }
    }
}