using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SoutezHad
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Snake snake;
        private Apple apple;
        private float slowMotionTimer;
        private float ghostTimer;
        private float currentMoveDelay;
        private Random random;

        private Texture2D pixel;

        private const int GRID_SIZE = 25;
        private const int GRID_WIDTH = 32;
        private const int GRID_HEIGHT = 24;

        private float moveTimer;
        private const float MOVE_DELAY = 0.12f;

        private int score;
        private bool gameOver;

        private KeyboardState previousKeyState;

        // Animace jídla
        private float foodPulse;
        private const float PULSE_SPEED = 3f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = GRID_WIDTH * GRID_SIZE;
            graphics.PreferredBackBufferHeight = GRID_HEIGHT * GRID_SIZE;
        }

        protected override void Initialize()
        {
            random = new Random();
            snake = new Snake(GRID_WIDTH / 2, GRID_HEIGHT / 2, GRID_SIZE);
            apple = new Apple(GRID_WIDTH, GRID_HEIGHT, GRID_SIZE);

            score = 0;
            gameOver = false;
            moveTimer = 0;
            foodPulse = 0;
            slowMotionTimer = 0;
            ghostTimer = 0;
            currentMoveDelay = MOVE_DELAY;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        private void SpawnFood()
        {
            bool validPosition;
            do
            {
                food = new Point(random.Next(GRID_WIDTH), random.Next(GRID_HEIGHT));
                validPosition = true;

                foreach (Point segment in snake.Segments)
                {
                    if (segment == food)
                    {
                        validPosition = false;
                        break;
                    }
                }
            } while (!validPosition);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                keyState.IsKeyDown(Keys.Escape))
                Exit();

            if (gameOver)
            {
                if (keyState.IsKeyDown(Keys.Space) && previousKeyState.IsKeyUp(Keys.Space))
                {
                    Initialize();
                }
                previousKeyState = keyState;
                return;
            }

            // Ovládání WASD
            if (keyState.IsKeyDown(Keys.W) && previousKeyState.IsKeyUp(Keys.W))
                snake.SetDirection(new Point(0, -1));
            else if (keyState.IsKeyDown(Keys.S) && previousKeyState.IsKeyUp(Keys.S))
                snake.SetDirection(new Point(0, 1));
            else if (keyState.IsKeyDown(Keys.A) && previousKeyState.IsKeyUp(Keys.A))
                snake.SetDirection(new Point(-1, 0));
            else if (keyState.IsKeyDown(Keys.D) && previousKeyState.IsKeyUp(Keys.D))
                snake.SetDirection(new Point(1, 0));

            previousKeyState = keyState;

            // Animace jídla
            foodPulse += (float)gameTime.ElapsedGameTime.TotalSeconds * PULSE_SPEED;

            moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (moveTimer >= currentMoveDelay)
            {
                moveTimer = 0;
                snake.Update();

                if (snake.CheckWallCollision(GRID_WIDTH, GRID_HEIGHT))
                {
                    gameOver = true;
                }

                if (snake.CheckSelfCollision() && ghostTimer <= 0)
                {
                    gameOver = true;
                }

                if (snake.Head == apple.Position)
                {
                    switch (apple.Type)
                    {
                        case AppleType.Red:
                            snake.Grow();
                            score += 10;
                            break;

                        case AppleType.Blue:
                            slowMotionTimer = 3f; // 3 sekundy zpomalení
                            currentMoveDelay = MOVE_DELAY * 2.0f;
                            break;

                        case AppleType.Purple:
                            ghostTimer = 3f; // 3 sekundy „ghost mode“
                            break;
                    }

                    apple.Respawn(GRID_WIDTH, GRID_HEIGHT);
                }
            }
            if (slowMotionTimer > 0)
            {
                slowMotionTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (slowMotionTimer <= 0)
                    currentMoveDelay = MOVE_DELAY;
            }

            if (ghostTimer > 0)
            {
                ghostTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(20, 20, 30));

            spriteBatch.Begin();

            // Mřížka na pozadí
            DrawGrid();

            if (!gameOver)
            {
                // Kreslení jídla s pulzujícím efektem
                float pulseScale = 0.8f + (float)Math.Sin(foodPulse) * 0.2f;
                int foodSize = (int)(GRID_SIZE * pulseScale);
                int offset = (GRID_SIZE - foodSize) / 2;

                Rectangle foodRect = new Rectangle(
                    food.X * GRID_SIZE + offset,
                    food.Y * GRID_SIZE + offset,
                    foodSize,
                    foodSize
                );
                apple.Draw(spriteBatch, pixel, foodPulse);

                // Světélko na jídle
                Rectangle highlight = new Rectangle(
                    foodRect.X + 3,
                    foodRect.Y + 3,
                    foodSize / 3,
                    foodSize / 3
                );
                spriteBatch.Draw(pixel, highlight, Color.Pink);

                // Kreslení hada
                snake.Draw(spriteBatch, pixel, Color.LimeGreen);

                // Skóre
                DrawText($"SKORE: {score}", 10, 10, Color.White, 2);
                DrawText($"DELKA: {snake.Segments.Count}", 10, 35, Color.LightGray, 2);
            }
            else
            {
                // Game Over overlay
                Rectangle overlay = new Rectangle(0, 0, GRID_WIDTH * GRID_SIZE, GRID_HEIGHT * GRID_SIZE);
                spriteBatch.Draw(pixel, overlay, Color.Black * 0.7f);

                // Game Over texty
                int centerX = GRID_WIDTH * GRID_SIZE / 2;
                int centerY = GRID_HEIGHT * GRID_SIZE / 2;

                DrawTextCentered("GAME OVER", centerX, centerY - 60, Color.Red, 4);
                DrawTextCentered($"FINALNI SKORE: {score}", centerX, centerY - 10, Color.White, 3);
                DrawTextCentered($"DELKA HADA: {snake.Segments.Count}", centerX, centerY + 25, Color.LightGray, 2);
                DrawTextCentered("STISKNI MEZERNK PRO RESTART", centerX, centerY + 70, Color.Yellow, 2);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGrid()
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    Rectangle cell = new Rectangle(x * GRID_SIZE, y * GRID_SIZE, GRID_SIZE, GRID_SIZE);
                    Color gridColor = ((x + y) % 2 == 0) ? new Color(25, 25, 35) : new Color(30, 30, 40);
                    spriteBatch.Draw(pixel, cell, gridColor);
                }
            }
        }

        private void DrawText(string text, int x, int y, Color color, int scale)
        {
            Dictionary<char, bool[,]> font = GetPixelFont();

            int currentX = x;
            foreach (char c in text.ToUpper())
            {
                if (c == ' ')
                {
                    currentX += 4 * scale;
                    continue;
                }

                if (font.ContainsKey(c))
                {
                    bool[,] charPixels = font[c];
                    for (int py = 0; py < charPixels.GetLength(0); py++)
                    {
                        for (int px = 0; px < charPixels.GetLength(1); px++)
                        {
                            if (charPixels[py, px])
                            {
                                Rectangle pixel = new Rectangle(
                                    currentX + px * scale,
                                    y + py * scale,
                                    scale,
                                    scale
                                );
                                spriteBatch.Draw(this.pixel, pixel, color);
                            }
                        }
                    }
                    currentX += (charPixels.GetLength(1) + 1) * scale;
                }
            }
        }

        private void DrawTextCentered(string text, int centerX, int y, Color color, int scale)
        {
            int width = GetTextWidth(text, scale);
            DrawText(text, centerX - width / 2, y, color, scale);
        }

        private int GetTextWidth(string text, int scale)
        {
            Dictionary<char, bool[,]> font = GetPixelFont();
            int width = 0;

            foreach (char c in text.ToUpper())
            {
                if (c == ' ')
                {
                    width += 4 * scale;
                }
                else if (font.ContainsKey(c))
                {
                    width += (font[c].GetLength(1) + 1) * scale;
                }
            }

            return width;
        }

        private Dictionary<char, bool[,]> GetPixelFont()
        {
            var font = new Dictionary<char, bool[,]>();

            // Jednoduchý pixel font 5x5
            font['A'] = new bool[,] { { false, true, true, true, false }, { true, false, false, false, true }, { true, true, true, true, true }, { true, false, false, false, true }, { true, false, false, false, true } };
            font['B'] = new bool[,] { { true, true, true, true, false }, { true, false, false, false, true }, { true, true, true, true, false }, { true, false, false, false, true }, { true, true, true, true, false } };
            font['C'] = new bool[,] { { false, true, true, true, true }, { true, false, false, false, false }, { true, false, false, false, false }, { true, false, false, false, false }, { false, true, true, true, true } };
            font['D'] = new bool[,] { { true, true, true, true, false }, { true, false, false, false, true }, { true, false, false, false, true }, { true, false, false, false, true }, { true, true, true, true, false } };
            font['E'] = new bool[,] { { true, true, true, true, true }, { true, false, false, false, false }, { true, true, true, true, false }, { true, false, false, false, false }, { true, true, true, true, true } };
            font['F'] = new bool[,] { { true, true, true, true, true }, { true, false, false, false, false }, { true, true, true, true, false }, { true, false, false, false, false }, { true, false, false, false, false } };
            font['G'] = new bool[,] { { false, true, true, true, true }, { true, false, false, false, false }, { true, false, true, true, true }, { true, false, false, false, true }, { false, true, true, true, false } };
            font['H'] = new bool[,] { { true, false, false, false, true }, { true, false, false, false, true }, { true, true, true, true, true }, { true, false, false, false, true }, { true, false, false, false, true } };
            font['I'] = new bool[,] { { true, true, true, true, true }, { false, false, true, false, false }, { false, false, true, false, false }, { false, false, true, false, false }, { true, true, true, true, true } };
            font['J'] = new bool[,] { { false, false, false, false, true }, { false, false, false, false, true }, { false, false, false, false, true }, { true, false, false, false, true }, { false, true, true, true, false } };
            font['K'] = new bool[,] { { true, false, false, false, true }, { true, false, false, true, false }, { true, true, true, false, false }, { true, false, false, true, false }, { true, false, false, false, true } };
            font['L'] = new bool[,] { { true, false, false, false, false }, { true, false, false, false, false }, { true, false, false, false, false }, { true, false, false, false, false }, { true, true, true, true, true } };
            font['M'] = new bool[,] { { true, false, false, false, true }, { true, true, false, true, true }, { true, false, true, false, true }, { true, false, false, false, true }, { true, false, false, false, true } };
            font['N'] = new bool[,] { { true, false, false, false, true }, { true, true, false, false, true }, { true, false, true, false, true }, { true, false, false, true, true }, { true, false, false, false, true } };
            font['O'] = new bool[,] { { false, true, true, true, false }, { true, false, false, false, true }, { true, false, false, false, true }, { true, false, false, false, true }, { false, true, true, true, false } };
            font['P'] = new bool[,] { { true, true, true, true, false }, { true, false, false, false, true }, { true, true, true, true, false }, { true, false, false, false, false }, { true, false, false, false, false } };
            font['Q'] = new bool[,] { { false, true, true, true, false }, { true, false, false, false, true }, { true, false, false, false, true }, { true, false, false, true, false }, { false, true, true, false, true } };
            font['R'] = new bool[,] { { true, true, true, true, false }, { true, false, false, false, true }, { true, true, true, true, false }, { true, false, false, true, false }, { true, false, false, false, true } };
            font['S'] = new bool[,] { { false, true, true, true, true }, { true, false, false, false, false }, { false, true, true, true, false }, { false, false, false, false, true }, { true, true, true, true, false } };
            font['T'] = new bool[,] { { true, true, true, true, true }, { false, false, true, false, false }, { false, false, true, false, false }, { false, false, true, false, false }, { false, false, true, false, false } };
            font['U'] = new bool[,] { { true, false, false, false, true }, { true, false, false, false, true }, { true, false, false, false, true }, { true, false, false, false, true }, { false, true, true, true, false } };
            font['V'] = new bool[,] { { true, false, false, false, true }, { true, false, false, false, true }, { true, false, false, false, true }, { false, true, false, true, false }, { false, false, true, false, false } };
            font['W'] = new bool[,] { { true, false, false, false, true }, { true, false, false, false, true }, { true, false, true, false, true }, { true, true, false, true, true }, { true, false, false, false, true } };
            font['X'] = new bool[,] { { true, false, false, false, true }, { false, true, false, true, false }, { false, false, true, false, false }, { false, true, false, true, false }, { true, false, false, false, true } };
            font['Y'] = new bool[,] { { true, false, false, false, true }, { false, true, false, true, false }, { false, false, true, false, false }, { false, false, true, false, false }, { false, false, true, false, false } };
            font['Z'] = new bool[,] { { true, true, true, true, true }, { false, false, false, true, false }, { false, false, true, false, false }, { false, true, false, false, false }, { true, true, true, true, true } };

            font['0'] = new bool[,] { { false, true, true, true, false }, { true, false, false, true, true }, { true, false, true, false, true }, { true, true, false, false, true }, { false, true, true, true, false } };
            font['1'] = new bool[,] { { false, false, true, false, false }, { false, true, true, false, false }, { false, false, true, false, false }, { false, false, true, false, false }, { false, true, true, true, false } };
            font['2'] = new bool[,] { { false, true, true, true, false }, { true, false, false, false, true }, { false, false, true, true, false }, { false, true, false, false, false }, { true, true, true, true, true } };
            font['3'] = new bool[,] { { true, true, true, true, false }, { false, false, false, false, true }, { false, true, true, true, false }, { false, false, false, false, true }, { true, true, true, true, false } };
            font['4'] = new bool[,] { { true, false, false, true, false }, { true, false, false, true, false }, { true, true, true, true, true }, { false, false, false, true, false }, { false, false, false, true, false } };
            font['5'] = new bool[,] { { true, true, true, true, true }, { true, false, false, false, false }, { true, true, true, true, false }, { false, false, false, false, true }, { true, true, true, true, false } };
            font['6'] = new bool[,] { { false, true, true, true, false }, { true, false, false, false, false }, { true, true, true, true, false }, { true, false, false, false, true }, { false, true, true, true, false } };
            font['7'] = new bool[,] { { true, true, true, true, true }, { false, false, false, false, true }, { false, false, false, true, false }, { false, false, true, false, false }, { false, false, true, false, false } };
            font['8'] = new bool[,] { { false, true, true, true, false }, { true, false, false, false, true }, { false, true, true, true, false }, { true, false, false, false, true }, { false, true, true, true, false } };
            font['9'] = new bool[,] { { false, true, true, true, false }, { true, false, false, false, true }, { false, true, true, true, true }, { false, false, false, false, true }, { false, true, true, true, false } };

            font[':'] = new bool[,] { { false, false, false, false, false }, { false, true, true, false, false }, { false, false, false, false, false }, { false, true, true, false, false }, { false, false, false, false, false } };

            return font;
        }
    }
}