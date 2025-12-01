using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

public class Confetti
{
    public Vector2 Position;
    public float Speed;
    public Color Color;
    public float Size;

    private static Random rand = new Random();

    public Confetti(int screenWidth)
    {
        Reset(screenWidth);
    }

    public void Reset(int screenWidth)
    {
        Position = new Vector2(rand.Next(0, screenWidth), rand.Next(-600, 0));
        Speed = rand.Next(40, 120);
        Size = rand.Next(4, 10);
        Color = new Color(
            rand.Next(50, 255),
            rand.Next(50, 255),
            rand.Next(50, 255)
        );
    }

    public void Update(GameTime gameTime, int screenHeight, int screenWidth)
    {
        Position.Y += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Position.Y > screenHeight)
            Reset(screenWidth);
    }

    public void Draw(SpriteBatch sb, Texture2D pixel)
    {
        sb.Draw(pixel, new Rectangle((int)Position.X, (int)Position.Y, (int)Size, (int)Size), Color);
    }
}