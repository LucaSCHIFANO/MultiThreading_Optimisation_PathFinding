using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Castlenight
{
    public class Tile
    {
        string name;
        int posX = 0;
        int posY = 0;

        TilesData data;
        public TilesData Data { get => data; set => data = value; }

        public bool selected;
        Texture2D texture;

        public Tile(string _name, int posX, int posY) 
        { 
            this.name = _name;
            this.posX = posX;
            this.posY = posY;
            this.data = new TilesData();
            texture = CastleNightGame.Instance.Content.Load<Texture2D>(name);
        }


        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            var spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            spriteBatch.Begin();
            if(selected)spriteBatch.Draw(texture, new Rectangle(posX* GameConfig.tileSize, posY* GameConfig.tileSize, GameConfig.tileSize, GameConfig.tileSize), Color.Red);
            else spriteBatch.Draw(texture, new Rectangle(posX * GameConfig.tileSize, posY * GameConfig.tileSize, GameConfig.tileSize, GameConfig.tileSize), Color.White);
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

        public Vector2 GetPosition()
        {
            return new Vector2(posX, posY);
        }
    }


    public class TilesData
    {
        public Tile parent = null;
        public int GCost = int.MaxValue - 1;
        public int HCost = int.MaxValue - 1;
        public int FCost = int.MaxValue - 1;

        public void CalculateFCost()
        {
            FCost = GCost + HCost;
        }
    }
}