using Microsoft.Xna.Framework.Graphics;

namespace SoutezHad
{
    public interface IRenderable
    {
        void Draw(SpriteBatch spriteBatch, Texture2D pixel, int gridSize);
    }
}

