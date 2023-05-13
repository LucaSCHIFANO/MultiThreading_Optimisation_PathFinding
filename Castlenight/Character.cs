using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq.Expressions;

namespace Castlenight
{

    public class Character
    {
        string name;
        int pv = 0;
        public int Pv { get => pv; }
        int posX = 0;
        public int PosX { get => posX; }
        int posY = 0;
        public int PosY { get => posY; }
        int score = 0;
        public int Score { get => score; set => score = value; }

        int id;
        public int Id { get => id;}
        public Weapon weapon { get; set; }


        ICharacterController controller;
        public ICharacterController Controller { get => controller; }


        static Mutex mutex;
        public static Mutex Mutex { get => mutex; }


        Thread thread = null;
        public Thread Thread { get => thread; }

        bool needRecheck;
        public bool NeedRecheck { get => needRecheck; set => needRecheck = value; }

        Map map;
        public Map Map { get => map; }

        Texture2D texture;

        public Character(string _name, int posX, int posY)
        {
            this.name = _name;
            this.posX = posX;
            this.posY = posY;
            this.pv = 1000; // 100;

            controller = new RandomCharacterController();
            weapon = new Weapon(5, 1, 2);

            if (Character.Mutex == null) mutex = new Mutex();
            
            ParameterizedThreadStart parameterizedThreadStart = new ParameterizedThreadStart(UpdateCharacter);
            thread = new Thread(UpdateCharacter);
            thread.IsBackground = true;

            texture = CastleNightGame.Instance.Content.Load<Texture2D>(name);
            map = CastleNightGame.Instance.Map; 
        }

        public void StartThread()
        {
            thread.Start(new Params(this));
        }

        public void Update(GameTime gameTime)
        {
            if (controller != null)
                controller.ComputeAndExecuteAction(this);
        }

        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            var spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Rectangle(posX * GameConfig.tileSize, posY * GameConfig.tileSize, GameConfig.tileSize, GameConfig.tileSize), Color.White);
            spriteBatch.End();
        }

        public void SetPosition(int posX, int posY)
        {
            if (!map.CanMoveToCell(posX, posY))
            {
                throw new Exception("Moving a character on an invalid space");
            }
            this.posX = posX;
            this.posY = posY;
        }

        //returns a score based on damage & kill
        public int TakeDamage(int damage)
        {
            int score = damage;
            if (pv < 0)
                score = -10;
            else
            {
                pv -= damage;
                if (pv < 0)
                {
                    Debug.WriteLine("Player killed");
                    score += 50;
                    map.RemovePlayer(this);
                }
            }

            return score;
        }

        //Tells player a tile is about to be destroyed
        public void TileAboutToBeDestroyed(List<Vector2> tilesToBeDestroyed, double timeBeforeDestruction)
        {
            if (controller != null)
                controller.TileAboutToBeDestroyed(tilesToBeDestroyed, timeBeforeDestruction);
        }

        //Kill the unit immediately, used when unit is on a desptroyed tile
        public void Kill()
        {
            pv = 0;
            score -= 1000;
            Debug.WriteLine("Player dead by falling in a hole");
        }

        void UpdateCharacter(Object obj)
        {
            Thread.Sleep(1000);

            if (obj == null)
            {
                Console.WriteLine("Missing param!");
                return;
            }
            Params param = obj as Params;
            if (param == null)
            {
                Console.WriteLine("Bad param!");
                return;
            }

            while (param.character.Pv > 0)
            {
                if (Controller != null)
                    Controller.ComputeAndExecuteAction(this);

                Thread.Sleep(100);
            }



        }

        public void SetId(int _id)
        {
            if(_id >= 0) id = _id;
        }
    }
}