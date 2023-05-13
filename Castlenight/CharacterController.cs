using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Castlenight
{
    public interface ICharacterController
    {
        public void ComputeAndExecuteAction(Character character);
        void TileAboutToBeDestroyed(List<Vector2> tilesToBeDestroyed, double timeBeforeDestruction);
    }

    public class RandomCharacterController : ICharacterController
    {
        bool running = false;
        
        //Target weapon
        List<Tile> nextTile = new List<Tile>();
        int nextTileId = 0;

        public void ComputeAndExecuteAction(Character character)
        {
            //Random action controller: will do something random (but valid) on each tick
            if (running)
                throw new Exception("ComputeAndExecuteAction called twice");
            running = true;


            Character.Mutex.WaitOne();


            if (character.Pv <= 0)
            {
                running = false;
                Character.Mutex.ReleaseMutex();
                return;
            }

            Random random = new Random();

            if (character.weapon != null && character.weapon.Ammo > 0)
            {
                var targets = CastleNightGame.Instance.Map.GetCharactersInRange(character, character.weapon.Range);
                if (targets.Count > 0 && random.Next(100) < 25)
                {
                    character.Score += character.weapon.Shoot(targets[random.Next(targets.Count)]);
                    running = false;
                    Character.Mutex.ReleaseMutex();
                    return;
                }
            }

            //pathfinding

            if (nextTile == null || nextTile.Count == 0 || character.NeedRecheck)
            {
                character.NeedRecheck = false;
                GetNextTile(character);
            }
            else if (!CastleNightGame.Instance.Map.CanMoveToCellExcludingFutureDestroyed((int)nextTile[nextTileId].GetPosition().X, (int)nextTile[nextTileId].GetPosition().Y)) GetNextTile(character);
            else
            {
                CastleNightGame.Instance.Map.MovePlayer(character, (int)nextTile[nextTileId].GetPosition().X, (int)nextTile[nextTileId].GetPosition().Y);
                nextTileId++;
                if (nextTileId >= nextTile.Count) nextTile.Clear();
            }

          


            Character.Mutex.ReleaseMutex();
            running = false;

        }

        public void TileAboutToBeDestroyed(List<Vector2> tilesToBeDestroyed, double timeBeforeDestruction)
        {
        }

        private void GetNextTile(Character character)
        {
            try
            {
                CastleNightGame.Instance.Rwls.EnterReadLock();

                int count = CastleNightGame.Instance.Map.Weapons.Count;
                List<WeaponBox> provWeapon = CastleNightGame.Instance.Map.Weapons;


                if (count > 0)
                {
                    int shortestId = -1;
                    int shortestValue = int.MaxValue;

                    List<Tile>[] list = new List<Tile>[count];

                    for (int i = 0; i < count; i++)
                    {
                        list[i] = Pathfinding.FindPath(new Vector2(character.PosX, character.PosY), new Vector2(provWeapon[i].PosX, provWeapon[i].PosY), CastleNightGame.Instance.Map);

                        if ((list[i] != null && list[i].Count != 0))
                        {
                            if (shortestId == -1)
                            {
                                shortestValue = list[i][list[i].Count - 1].Data.GCost;
                                shortestId = i;
                            }
                            else if (shortestValue > list[i].Last().Data.GCost)
                            {
                                shortestValue = list[i].Last().Data.GCost;
                                shortestId = i;
                            }
                        }
                    }

                    if (shortestId != -1 && list[shortestId] != null)
                    {
                        nextTile.Clear();
                        nextTile = list[shortestId];
                        nextTileId = 0;
                    }
                } else
                {
                    int x, y;
                    do
                    {
                        Random random = new Random();
                        x = random.Next(CastleNightGame.Instance.Map.GameConfig.width);
                        y = random.Next(CastleNightGame.Instance.Map.GameConfig.height);
                    } while (!CastleNightGame.Instance.Map.CanMoveToCellExcludingFutureDestroyed(x, y));

                    List<Tile> list = new List<Tile>();
                    list = Pathfinding.FindPath(new Vector2(character.PosX, character.PosY), new Vector2(x, y), CastleNightGame.Instance.Map);

                    if ((list != null && list.Count != 0))
                    {
                        nextTile.Clear();
                        nextTile = list;
                        nextTileId = 0;
                    }
                }
            }
            finally
            {
                CastleNightGame.Instance.Rwls.ExitReadLock();
            }
        }
    }
}