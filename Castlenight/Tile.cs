using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Castlenight
{
    public class Tile
    {
        string name;
        int posX = 0;
        int posY = 0;

        List<TilesData> data; // a "tilesdata" for each character to make the A* work
        public List<TilesData> Data { get => data; set => data = value; }

        
        Texture2D texture;

        bool isOccupied;
        public bool IsOccupied { get => isOccupied; set => isOccupied = value; }

        Mutex mutex;
        public Mutex Mutex { get => mutex; }

        bool selected;
        float maxValue;
        float value;


        public Tile(string _name, int posX, int posY, int size) 
        { 
            this.name = _name;
            this.posX = posX;
            this.posY = posY;
            this.data = new List<TilesData>();
            texture = CastleNightGame.Instance.Content.Load<Texture2D>(name);
            mutex = new Mutex();

            for (int i = 0; i < size; i++)
            {
                data.Add(new TilesData());
            }
        }


        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, float destructionTime)
        {
            var spriteBatch = new SpriteBatch(graphics.GraphicsDevice);


            spriteBatch.Begin();
            if (selected)
            {
                value = 1 - (destructionTime / maxValue);
                spriteBatch.Draw(texture, new Rectangle(posX * GameConfig.tileSize, posY * GameConfig.tileSize, GameConfig.tileSize, GameConfig.tileSize), new Color(1, 1 - value, 1 - value));
                if(value == 1)selected= false;
            }
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

        public void SetSelected(float _maxValue)
        {
            selected = true;
            maxValue = _maxValue;
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