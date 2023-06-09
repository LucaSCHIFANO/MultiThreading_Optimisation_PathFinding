﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Castlenight
{
    class Params // Character sends to the thread
    {
        public Character character;

        public Params(Character _char)
        {
            this.character = _char;

        }
    }

    public class CastleNightGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private Map map;
        public Map Map { get { return map; } }
        public static CastleNightGame Instance { get; private set; }

        List<Thread> threadList = new List<Thread>(); // list of every character thread 

        ReaderWriterLockSlim rwls = new ReaderWriterLockSlim();
        public ReaderWriterLockSlim Rwls { get => rwls; }

        public CastleNightGame()
        {
            Instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = GameConfig.WINDOW_WIDTH;
            _graphics.PreferredBackBufferHeight = GameConfig.WINDOW_HEIGHT;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            //create game
            GameConfig gameConfig = new GameConfig();
            map = new Map(gameConfig);
            if (gameConfig.playerCount >= gameConfig.width * gameConfig.height) throw new Exception("Too many characters");

            for (int i = 0; i < gameConfig.playerCount; i++)
            {
                int x, y;
                do
                {
                    Random random = new Random();
                    x = random.Next(gameConfig.width);
                    y = random.Next(gameConfig.height);
                } while (!map.CanMoveToCell(x, y));


                //create character and set his id
                Character character = new Character("character1", x, y);
                character.SetId(i);

                // set the tile as "occupied" and add the player to the playerList 
                map.GetTile(x,y).IsOccupied = true;
                map.AddPlayer(character);
                
                threadList.Add(character.Thread);
                character.StartThread();
            }

            ParameterizedThreadStart parameterizedThreadStart = new ParameterizedThreadStart(CheckEndGame);
            Thread thread = new Thread(CheckEndGame);
            thread.IsBackground = true;

            Thread.Sleep(100);
            thread.Start();

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

        void CheckEndGame(Object obj) // check if there is one survivor ( or if everyone is dead :\ )
        {
            Debug.WriteLine("\n Start ! \n");

            Thread.Sleep(1000);
            bool isRunning = true;
            do
            {
                isRunning = true;
                int numberOfThread = 0;

                foreach (var item in threadList)
                {
                    if (!item.Join(100)) numberOfThread++;
                }
                if (numberOfThread <= 1) isRunning = false;

            } while (isRunning);

            Debug.WriteLine("\n Game !\n ");

        }
    }
}
