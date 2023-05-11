using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace Castlenight
{
    //A weapon box contains a random weapon. It's dropped on the map so players can grab new equipment to figts
    public class WeaponBox
    {
        int posX = 0;
        public int PosX { get => posX; }
        int posY = 0;
        public int PosY { get => posY; }

        public Weapon weapon { get; private set; }

        public WeaponBox(int x, int y)
        {
            posX = x;
            posY = y;
            Random random = new Random();
            weapon = new Weapon(5 + random.Next(10), 1 + random.Next(5), 5 + random.Next(15));
        }

        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            Texture2D texture;
            var spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            texture = CastleNightGame.Instance.Content.Load<Texture2D>("crate");
            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Rectangle(posX * 20, posY * 20, 20, 20), Color.White);
            spriteBatch.End();
        }
    }
}