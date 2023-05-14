using Microsoft.Xna.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Castlenight
{

    public class Map
    {
        private Tile[][] tiles;
        public Tile[][] Tiles { get => tiles; }

        private int width, height;
        double timeBeforeSelectTileTodestroy;
        double timeBeforeDestruction;
        List<Vector2> tilesToBeDestroyed = new List<Vector2>();
        public List<Vector2> TilesToBeDestroyed { get => tilesToBeDestroyed; }
        public int Width { get => width; }
        public int Height { get => height; }

        private List<Character> players = new List<Character>();
        public List<Character> Players { get => players; set => players = value; }

        private List<WeaponBox> weapons = new List<WeaponBox>();
        public List<WeaponBox> Weapons { get => weapons; }

        double timeBeforeWeaponDrop;

        GameConfig gameConfig;
        public GameConfig GameConfig { get => gameConfig; }

        ReaderWriterLockSlim rwls;

        public Map(GameConfig gameConfig)
        {
            //generate a random map
            this.gameConfig = gameConfig;
            width = gameConfig.width;
            height = gameConfig.height;
            tiles = new Tile[height][];
            rwls = CastleNightGame.Instance.Rwls;

            var _size = gameConfig.playerCount;


            Random random = new Random();
            for (int i = 0; i < height; ++i)
            {
                tiles[i] = new Tile[width];
                for (int j = 0; j < width; ++j)
                {
                    string kind = "";
                    int rand = random.Next(100);
                    if (rand < 50)
                        kind = "grass";
                    else if (rand < 80)
                        kind = "dirt";
                    else
                        kind = "highgrass";
                    tiles[i][j] = new Tile(kind, j, i, _size);
                }
            }

        }

        public void Update(GameTime gameTime)
        {
            //update map status (new tiles will be destroyed, destroy)

            if (timeBeforeSelectTileTodestroy > 0)
            {
                //time to flag some tile to be destroyed soon (so we can tell players)
                timeBeforeSelectTileTodestroy -= gameTime.ElapsedGameTime.TotalSeconds * GameConfig.MAP_DESTRUCTION_SPEED;
                if (timeBeforeSelectTileTodestroy <= 0)
                {
                    timeBeforeSelectTileTodestroy = 0;
                    timeBeforeDestruction = gameConfig.weaponDropTimer;
                    Random random = new Random();
                    for (int i = 0; i < gameConfig.destoyedTilesCount; ++i)
                    {
                        tilesToBeDestroyed.Add(new Vector2(random.Next(width), random.Next(height)));
                        GetTile(tilesToBeDestroyed[i]).SetSelected(gameConfig.executeTileDestructionTimer);
                    }
                    timeBeforeDestruction = gameConfig.executeTileDestructionTimer;

                }
            }
            else if (timeBeforeDestruction > 0)
            {
                var _size = gameConfig.playerCount;
                //destroy flagged tiles, killing players that are still on them
                timeBeforeDestruction -= gameTime.ElapsedGameTime.TotalSeconds * GameConfig.MAP_DESTRUCTION_SPEED;
                if (timeBeforeDestruction <= 0)
                {
                    timeBeforeDestruction = 0;
                    foreach (var element in tilesToBeDestroyed)
                    {
                        tiles[(int)element.Y][(int)element.X] = new Tile("destroyed", (int)element.X, (int)element.Y, _size);
                        for (int i = 0; i < players.Count; ++i)
                        {
                            if (players[i].PosY == (int)element.Y && players[i].PosX == (int)element.X)
                            {
                                players[i].Kill();
                                RemovePlayer(players[i]);

                                --i;
                            }
                        }
                        try
                        {
                            rwls.EnterWriteLock();

                            for (int i = 0; i < weapons.Count; ++i)
                            {
                                if (weapons[i].PosX == (int)element.X && weapons[i].PosY == (int)element.Y)
                                {
                                    weapons.RemoveAt(i);
                                    --i;
                                }
                            }
                        }
                        finally
                        {
                            rwls.ExitWriteLock();
                        }
                    }

                    tilesToBeDestroyed.Clear();
                }
            }
            else
            {
                timeBeforeSelectTileTodestroy = gameConfig.triggerTileDestructionTimer;
            }


            //weapon drop
            timeBeforeWeaponDrop -= gameTime.ElapsedGameTime.TotalSeconds * GameConfig.MAP_DESTRUCTION_SPEED;
            if (timeBeforeWeaponDrop <= 0)
            {
                try
                {
                    rwls.EnterWriteLock();

                    timeBeforeWeaponDrop = gameConfig.weaponDropTimer;
                    Random random = new Random();
                    for (int i = 0; i < gameConfig.crateDropCount; ++i)
                    {
                        int x, y;
                        int id = 0;
                        do
                        {
                            x = random.Next(gameConfig.width);
                            y = random.Next(gameConfig.height);
                            id++;
                        } while (!CanMoveToCellExcludingFutureDestroyed(x, y) || id <= GameConfig.numberOfTryWeaponDrop);

                        if (CanMoveToCellExcludingFutureDestroyed(x, y))
                        {
                            WeaponBox weaponBox = new WeaponBox(x, y);
                            weapons.Add(weaponBox);
                        }
                    }
                    ResetPlayerCheck();
                }
                finally
                {
                    rwls.ExitWriteLock();
                }
            }
        }

    



        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    tiles[j][i].Draw(graphics, gameTime, (float)timeBeforeDestruction);
                }
            }
            for (int i = 0; i < weapons.Count; ++i)
                weapons[i].Draw(graphics, gameTime);
            for (int i = 0; i < players.Count; ++i)
                players[i].Draw(graphics, gameTime);
        }

        #region Player Management
        public void AddPlayer(Character character)
        {
                if (players.Contains(character))
                    throw new Exception("Player already present");
            players.Add(character);
        }

        public void RemovePlayer(Character character)
        {
             players.Remove(character);

        }

        public void MovePlayer(Character character, int x, int y, bool immediate = false)
        {

                    if (character.Pv <= 0)
                        throw new Exception("Character is dead");
                    if (CanMoveToCell(x, y))
                    {
                        character.SetPosition(x, y);

                        try
                        {

                        rwls.EnterWriteLock();
                        for (int j = 0; j < weapons.Count; ++j)
                        {
                            if (weapons[j].PosX == x && weapons[j].PosY == y)
                            {
                            character.weapon = weapons[j].weapon;
                                weapons.RemoveAt(j);
                            }
                        }
                        }
                        finally
                        {
                            rwls.ExitWriteLock();
                        }
                        if (!immediate)
                            Thread.Sleep(tiles[y][x].GetCost() * 1000 / GameConfig.PLAYER_MOVE_SPEED);
                    }
                    else if (tiles[y][x].GetCost() == int.MaxValue)
                    {
                        character.Kill();
                    }
                    else
                    {
                        throw new Exception("Cell is invalid");
                    }

                    return;

        }

        public bool CanMoveToCell(int x, int y)
        {
            //check if player can move on given cell
            if (tiles[y][x].GetCost() == int.MaxValue) return false;
            if (tiles[y][x].IsOccupied) return false;
            return true;
        }

        public bool CanMoveToCellExcludingFutureDestroyed(int x, int y)
        {
            //check if player can move on given cell. Consider invalid cells that will be destroyed soon
            if (tiles[y][x].GetCost() == int.MaxValue)return false;
            if (GetTile(x, y).IsOccupied) return false;
            for (int i = 0; i < tilesToBeDestroyed.Count; ++i)
            {
                if ((int)tilesToBeDestroyed[i].X == x && (int)tilesToBeDestroyed[i].Y == y && timeBeforeDestruction < 0.5 * GameConfig.MAP_DESTRUCTION_SPEED)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckBeforeMove(int x, int y)
        {
            GetTile(x, y).Mutex.WaitOne();
            if (CanMoveToCellExcludingFutureDestroyed(x, y)) return true;
            else
            {
                GetTile(x, y).Mutex.ReleaseMutex();
                return false;
            }
        }


        public List<Character> GetCharactersInRange(Character origin, int radius)
        {
            //Find potential targets
            List<Character> list = new List<Character>();
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i] != origin)
                {
                    if (Math.Sqrt((players[i].PosX - origin.PosX) * (players[i].PosX - origin.PosX) + (players[i].PosY - origin.PosY) * (players[i].PosY - origin.PosY)) <= radius)
                    {
                        list.Add(players[i]);
                    }
                }
            }

            return list;
        }

        #endregion


        public Tile GetTile(int x, int y)
        {
            return Tiles[y][x];
        }

        public Tile GetTile(Vector2 xy)
        {
            return Tiles[(int)xy.Y][(int)xy.X];
        }

        public void ResetTiles(int id)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    tiles[i][j].Data[id].parent = null;
                    tiles[i][j].Data[id].GCost = int.MaxValue;
                    tiles[i][j].Data[id].CalculateFCost();
                }
            }
        }

        public void ResetPlayerCheck()
        {
            foreach (var item in players)
            {
                item.NeedRecheck = true;
            }
        }
    }
}