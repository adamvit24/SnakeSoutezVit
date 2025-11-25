using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoutezHad
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Snake snake;
        private List<Fruit> fruits;
        private PowerUp powerUp;
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

        // Power-up systém
        private bool doubleFruitActive;
        private float doubleFruitTimer;
        private const float DOUBLE_FRUIT_DURATION = 15f;

        // Spawn timery
        private float powerUpSpawnTimer;
        private const float POWER_UP_SPAWN_INTERVAL = 20f; // Každých 20 sekund šance na spawn
        private const float POWER_UP_SPAWN_CHANCE = 0.5f; // 50% šance

        private float specialFruitTimer;
        private const float SPECIAL_FRUIT_CHECK_INTERVAL = 5f; // Každých 5 sekund kontrola

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
            fruits = new List<Fruit>();
            powerUp = null;

            FruitType next = (FruitType)(random.Next(0, 3));
            SpawnFruit(next);

            score = 0;
            gameOver = false;
            moveTimer = 0;
            doubleFruitActive = false;
            doubleFruitTimer = 0;
            powerUpSpawnTimer = 0;
            specialFruitTimer = 0;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        private void SpawnFruit(FruitType type)
        {
            Point position;
            bool validPosition;

            do
            {
                position = new Point(random.Next(GRID_WIDTH), random.Next(GRID_HEIGHT));
                validPosition = true;

                // Kontrola kolize s hadem
                foreach (Point segment in snake.Segments)
                {
                    if (segment == position)
                    {
                        validPosition = false;
                        break;
                    }
                }

                // Kontrola kolize s jiným ovocem
                foreach (Fruit fruit in fruits)
                {
                    if (fruit.Position == position)
                    {
                        validPosition = false;
                        break;
                    }
                }

                // Kontrola kolize s power-upem
                if (powerUp != null && powerUp.IsActive && powerUp.Position == position)
                {
                    validPosition = false;
                }

            } while (!validPosition);

            fruits.Add(new Fruit(position, type));
        }

        private void SpawnPowerUp()
        {
            if (powerUp != null && powerUp.IsActive) return; // Už existuje power-up

            Point position;
            bool validPosition;

            do
            {
                position = new Point(random.Next(GRID_WIDTH), random.Next(GRID_HEIGHT));
                validPosition = true;

                foreach (Point segment in snake.Segments)
                {
                    if (segment == position)
                    {
                        validPosition = false;
                        break;
                    }
                }

                foreach (Fruit fruit in fruits)
                {
                    if (fruit.Position == position)
                    {
                        validPosition = false;
                        break;
                    }
                }

            } while (!validPosition);

            powerUp = new PowerUp(position);
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

            // Update ovoce a power-upu
            foreach (Fruit fruit in fruits)
            {
                fruit.Update(gameTime);
            }

            if (powerUp != null && powerUp.IsActive)
            {
                powerUp.Update(gameTime);
            }

            // Power-up timer
            if (doubleFruitActive)
            {
                doubleFruitTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (doubleFruitTimer <= 0)
                {
                    doubleFruitActive = false;
                }
            }

            // Spawn timery
            powerUpSpawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (powerUpSpawnTimer >= POWER_UP_SPAWN_INTERVAL)
            {
                powerUpSpawnTimer = 0;
                if (random.NextDouble() < POWER_UP_SPAWN_CHANCE)
                {
                    SpawnPowerUp();
                }
            }

            specialFruitTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (specialFruitTimer >= SPECIAL_FRUIT_CHECK_INTERVAL)
            {
                specialFruitTimer = 0;

                // Šance na spawn 
                double rand = random.NextDouble();
                if (rand < 0.60) // 15% šance
                {
                    SpawnFruit(FruitType.Kiwi);
                }
                else if (rand < 0.79) // 30% šance 
                {
                    SpawnFruit(FruitType.Melon);
                }
            }

            // Pohyb hada
            moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (moveTimer >= MOVE_DELAY)
            {
                moveTimer = 0;
                snake.Update();

                if (snake.CheckWallCollision(GRID_WIDTH, GRID_HEIGHT))
                {
                    gameOver = true;
                }

                if (snake.CheckSelfCollision())
                {
                    gameOver = true;
                }

                // Kontrola sežrání ovoce
                for (int i = fruits.Count - 1; i >= 0; i--)
                {
                    if (snake.Head == fruits[i].Position)
                    {
                        snake.Grow();
                        score += fruits[i].Score;
                        fruits.RemoveAt(i);

                        // Spawn nového ovoce podle aktivního power-upu
                        int fruitsToSpawn = doubleFruitActive ? 2 : 1;
                        for (int j = 0; j < fruitsToSpawn; j++)
                        {
                            SpawnFruit(FruitType.Apple);
                        }
                    }
                }

                // Kontrola sežrání power-upu
                if (powerUp != null && powerUp.IsActive && snake.Head == powerUp.Position)
                {
                    powerUp.IsActive = false;
                    doubleFruitActive = true;
                    doubleFruitTimer = DOUBLE_FRUIT_DURATION;

                    // Přidej druhé ovoce okamžitě
                    if (fruits.Count == 1)
                    {
                        SpawnFruit(FruitType.Apple);
                    }
                }
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
                // Kreslení ovoce
                foreach (Fruit fruit in fruits)
                {
                    fruit.Draw(spriteBatch, pixel, GRID_SIZE);
                }

                // Kreslení power-upu
                if (powerUp != null && powerUp.IsActive)
                {
                    powerUp.Draw(spriteBatch, pixel, GRID_SIZE);
                }

                // Kreslení hada
                snake.Draw(spriteBatch, pixel, Color.LimeGreen);

                // Skóre
                DrawText($"SKORE: {score}", 10, 10, Color.White, 2);
                DrawText($"DELKA: {snake.Segments.Count}", 10, 35, Color.LightGray, 2);

                // Indikátor power-upu
                if (doubleFruitActive)
                {
                    int timeLeft = (int)Math.Ceiling(doubleFruitTimer);
                    DrawText($"DOUBLE FRUIT: {timeLeft}s", 10, 60, Color.Gold, 2);
                }
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