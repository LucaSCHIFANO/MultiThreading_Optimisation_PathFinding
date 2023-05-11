using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Castlenight
{
    public class Tile
    {
        string name;
        int posX = 0;
        int posY = 0;

        public Tile(string _name, int posX, int posY) 
        { 
            this.name = _name;
            this.posX = posX;
            this.posY = posY;
        }

        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            Texture2D texture;
            var spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            texture = CastleNightGame.Instance.Content.Load<Texture2D>(name);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Rectangle(posX*20, posY*20, 20, 20), Color.White);
            spriteBatch.End();
        }

        public int GetCost()
        {
            if (name == "destroyed")
                return int.MaxValue;
            if (name == "grass")
                return 1;
            if (name == "highgrass")
                return 3;
            if (name == "dirt")
                return 2;
            return 0;
        }
    }
}