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


        Thread thread = null;
        public Thread Thread { get => thread; }


        bool needRecheck;
        public bool NeedRecheck { get => needRecheck; set => needRecheck = value; }

        int shootProba;
        public int ShootProba { get => shootProba; }


        Map map;
        public Map Map { get => map; }

        Texture2D texture;

        ReaderWriterLockSlim rwlsPV;
        public ReaderWriterLockSlim RwlsPV { get => rwlsPV; }

        public Character(string _name, int posX, int posY)
        {
            this.name = _name;
            this.posX = posX;
            this.posY = posY;
            this.pv = 100;

            controller = new RandomCharacterController();
            weapon = new Weapon(5, 1, 2);

            // get value once and save them

            rwlsPV= new ReaderWriterLockSlim();

            shootProba = GameConfig.shootProba;
            
            ParameterizedThreadStart parameterizedThreadStart = new ParameterizedThreadStart(UpdateCharacter);
            thread = new Thread(UpdateCharacter);
            thread.IsBackground = true;

            //get the texture here and not in each draw
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

        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, SpriteBatch spriteBatch)
        {
            
            spriteBatch.Draw(texture, new Rectangle(posX * GameConfig.tileSize, posY * GameConfig.tileSize, GameConfig.tileSize, GameConfig.tileSize), Color.White);
            
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
            rwlsPV.EnterWriteLock(); // check to not kill a character if he is moving
            int _score = damage;
            if (pv < 0) _score = -10;
            else
            {
                pv -= damage;
                if (pv < 0)
                {
                    Debug.WriteLine("Player killed");
                    _score += 50;
                    map.RemovePlayer(this);
                }
            }
            rwlsPV.ExitWriteLock();
            return _score;
        }

        //Kill the unit immediately, used when unit is on a destroyed tile
        public void Kill()
        {
            rwlsPV.EnterWriteLock(); //check to not kill a character if he is moving
            pv = 0;
            score -= 1000;
            Debug.WriteLine("Player dead by falling in a hole");
            rwlsPV.ExitWriteLock();
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

        public Vector2 GetPosition()
        {
            return new Vector2(posX, posY);
        }
    }
}