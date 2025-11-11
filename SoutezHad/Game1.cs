using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace SoutezHad
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Snake snake;
        private Point food;
        private Random random;

        private Texture2D pixel;
        private SpriteFont font;

        private const int GRID_SIZE = 20;
        private const int GRID_WIDTH = 30;
        private const int GRID_HEIGHT = 20;

        private float moveTimer;
        private const float MOVE_DELAY = 0.15f; // Rychlost hada (sekundy)

        private int score;
        private bool gameOver;

        private KeyboardState previousKeyState;

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
            SpawnFood();

            score = 0;
            gameOver = false;
            moveTimer = 0;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Vytvoření jednoduchého pixelu pro kreslení
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // Pokud máš font, načti ho - jinak zakomentuj řádek s DrawString
            // font = Content.Load<SpriteFont>("Font");
        }

        private void SpawnFood()
        {
            bool validPosition;
            do
            {
                food = new Point(random.Next(GRID_WIDTH), random.Next(GRID_HEIGHT));
                validPosition = true;

                // Zkontroluj, že jídlo není na hadovi
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
                // Restart hry mezerníkem
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

            // Pohyb hada s časovačem
            moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (moveTimer >= MOVE_DELAY)
            {
                moveTimer = 0;
                snake.Update();

                // Kontrola kolize se zdí
                if (snake.CheckWallCollision(GRID_WIDTH, GRID_HEIGHT))
                {
                    gameOver = true;
                }

                // Kontrola kolize se sebou
                if (snake.CheckSelfCollision())
                {
                    gameOver = true;
                }

                // Kontrola sežrání jídla
                if (snake.Head == food)
                {
                    snake.Grow();
                    score += 10;
                    SpawnFood();
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            if (!gameOver)
            {
                // Kreslení hada
                snake.Draw(spriteBatch, pixel, Color.LimeGreen);

                // Kreslení jídla
                Rectangle foodRect = new Rectangle(
                    food.X * GRID_SIZE,
                    food.Y * GRID_SIZE,
                    GRID_SIZE,
                    GRID_SIZE
                );
                spriteBatch.Draw(pixel, foodRect, Color.Red);

                // spriteBatch.DrawString(font, $"Skóre: {score}", new Vector2(10, 10), Color.White);
            }
            else
            {
                // string gameOverText = "GAME OVER!";
                // string scoreText = $"Skóre: {score}";
                // string restartText = "Stiskni MEZERNÍK pro restart";
                // spriteBatch.DrawString(font, gameOverText, new Vector2(250, 150), Color.Red);
                // spriteBatch.DrawString(font, scoreText, new Vector2(270, 190), Color.White);
                // spriteBatch.DrawString(font, restartText, new Vector2(190, 230), Color.Gray);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}