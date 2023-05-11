using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Castlenight
{

    public class CastleNightGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private Map map;
        public Map Map { get { return map; } }
        public static CastleNightGame Instance { get; private set; }

        public CastleNightGame()
        {
            Instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            //create game
            GameConfig gameConfig = new GameConfig();
            map = new Map(gameConfig);
            for(int i = 0; i< gameConfig.playerCount; i++)
            {
                int x, y;
                do
                {
                    Random random = new Random();
                    x = random.Next(gameConfig.width);
                    y = random.Next(gameConfig.height);
                } while (!map.CanMoveToCell(x, y));

                Character character = new Character("character1", x, y);
                map.AddPlayer(character);
            }
            base.Initialize();
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            map.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            map.Draw(_graphics, gameTime);

            base.Draw(gameTime);
        }
    }
}
