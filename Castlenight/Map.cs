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

        private List<WeaponBox> weapons = new List<WeaponBox>();
        public List<WeaponBox> Weapons { get => weapons; }

        double timeBeforeWeaponDrop;

        GameConfig gameConfig;

        public Map(GameConfig gameConfig)
        {
            //generate a random map
            this.gameConfig = gameConfig;
            width = gameConfig.width;
            height = gameConfig.height;
            tiles = new Tile[height][];
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
                    tiles[i][j] = new Tile(kind, j, i);
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
                    Random random = new Random();
                    for (int i = 0; i < gameConfig.destoyedTilesCount; ++i)
                        tilesToBeDestroyed.Add(new Vector2(random.Next(width), random.Next(height)));
                    timeBeforeDestruction = gameConfig.executeTileDestructionTimer;
                    for (int i = 0; i < players.Count; ++i)
                        players[i].TileAboutToBeDestroyed(tilesToBeDestroyed, timeBeforeDestruction);
                }
            }
            else if (timeBeforeDestruction > 0)
            {
                //destroy flagged tiles, killing players that are still on them
                timeBeforeDestruction -= gameTime.ElapsedGameTime.TotalSeconds * GameConfig.MAP_DESTRUCTION_SPEED;
                if (timeBeforeDestruction <= 0)
                {
                    timeBeforeDestruction = 0;
                    foreach (var element in tilesToBeDestroyed)
                    {
                        tiles[(int)element.Y][(int)element.X] = new Tile("destroyed", (int)element.X, (int)element.Y);
                        for (int i = 0; i < players.Count; ++i)
                        {
                            if (players[i].PosY == (int)element.Y && players[i].PosX == (int)element.X)
                            {
                                players[i].Kill();
                                RemovePlayer(players[i]);

                                --i;
                            }
                        }
                        for (int i = 0; i < weapons.Count; ++i)
                        {
                            if (weapons[i].PosX == (int)element.X && weapons[i].PosY == (int)element.Y)
                            {
                                weapons.RemoveAt(i);
                                --i;
                            }
                        }
                    }
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
                    CastleNightGame.Instance.Rwls.EnterWriteLock();
                    timeBeforeWeaponDrop = 10;
                    Random random = new Random();
                    for (int i = 0; i < gameConfig.crateDropCount; ++i)
                    {
                        int x, y;
                        do
                        {
                            x = random.Next(gameConfig.width);
                            y = random.Next(gameConfig.height);
                        } while (!CastleNightGame.Instance.Map.CanMoveToCell(x, y));

                        WeaponBox weaponBox = new WeaponBox(x, y);
                            weapons.Add(weaponBox);
                    }
                    timeBeforeDestruction = gameConfig.weaponDropTimer;
                }
                finally
                {
                    CastleNightGame.Instance.Rwls.ExitWriteLock();
                }
            }
        }

    



        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    tiles[j][i].Draw(graphics, gameTime);
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
            for (int i = 0; i < players.Count; ++i)
                if (players[i] == character)
                    throw new Exception("Player already present");
            players.Add(character);
        }

        public void RemovePlayer(Character character)
        {
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i] == character)
                {
                    players.RemoveAt(i);
                    return;
                }
            }
            throw new Exception("Player not present");
        }

        public void MovePlayer(Character character, int x, int y, bool immediate = false)
        {
            //Move character on given tile is it's empty (no player) & valid. grab weapon that is on it
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i] == character)
                {
                    if (character.Pv <= 0)
                        throw new Exception("Character is dead");
                    if (CanMoveToCell(x, y))
                    {
                        players[i].SetPosition(x, y);
                        CastleNightGame.Instance.Rwls.EnterWriteLock();
                        for (int j = 0; j < weapons.Count; ++j)
                        {
                            if (weapons[j].PosX == x && weapons[j].PosY == y)
                            {
                                players[i].weapon = weapons[j].weapon;
                                weapons.RemoveAt(j);
                            }
                        }
                        CastleNightGame.Instance.Rwls.ExitWriteLock();
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
            }
            throw new Exception("Player not present");
        }

        public bool CanMoveToCell(int x, int y)
        {
            //check if player can move on given cell
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].PosY == y && players[i].PosX == x)
                {
                    return false;
                }
            }
            if (tiles[y][x].GetCost() == int.MaxValue)
                return false;
            return true;
        }

        public bool CanMoveToCellExcludingFutureDestroyed(int x, int y)
        {
            //check if player can move on given cell. Consider invalid cells that will be destroyed soon
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].PosY == y && players[i].PosX == x)
                {
                    return false;
                }
            }
            for (int i = 0; i < tilesToBeDestroyed.Count; ++i)
            {
                if ((int)tilesToBeDestroyed[i].X == x && (int)tilesToBeDestroyed[i].Y == y)
                {
                    return false;
                }
            }
            if (tiles[y][x].GetCost() == int.MaxValue)
                return false;
            return true;
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
            return Tiles[x][y];
        }

        public Tile GetTile(Vector2 xy)
        {
            return Tiles[(int)xy.X][(int)xy.Y];
        }

        public void ResetTiles()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    tiles[i][j].Data.parent = null;
                    tiles[i][j].Data.currentCost = int.MaxValue;
                }
            }
        }
    }
}