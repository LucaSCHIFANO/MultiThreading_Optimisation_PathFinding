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




            if (character.Pv <= 0)
            {
                running = false;
                return;
            }

            Random random = new Random();

            if (character.weapon != null && character.weapon.Ammo > 0)
            {
                var targets = character.Map.GetCharactersInRange(character, character.weapon.Range);
                if (targets.Count > 0 && random.Next(100) < 25)
                {
                    character.Score += character.weapon.Shoot(targets[random.Next(targets.Count)]);
                    running = false;
                    return;
                }
            }

            //pathfinding

            if (nextTile == null || nextTile.Count == 0 || character.NeedRecheck)
            {
                character.NeedRecheck = false;
                GetNextTile(character);
            }
            else if (!character.Map.CheckBeforeMove((int)nextTile[nextTileId].GetPosition().X, (int)nextTile[nextTileId].GetPosition().Y)) GetNextTile(character);
            else
            {
                Tile tile = character.Map.GetTile(character.GetPosition());
                tile.Mutex.WaitOne();

                tile.IsOccupied = false;
                character.Map.MovePlayer(character, (int)nextTile[nextTileId].GetPosition().X, (int)nextTile[nextTileId].GetPosition().Y);
                nextTile[nextTileId].IsOccupied = true;
                tile.Mutex.ReleaseMutex();
                nextTile[nextTileId].Mutex.ReleaseMutex();
                nextTileId++;
                if (nextTileId >= nextTile.Count) nextTile.Clear();
            }

          
            running = false;

        }

        private void GetNextTile(Character character)
        {
            try
            {
                CastleNightGame.Instance.Rwls.EnterReadLock();

                int count = character.Map.Weapons.Count;
                List<WeaponBox> provWeapon = character.Map.Weapons;


                if (count > 0)
                {
                    int shortestId = -1;
                    int shortestValue = int.MaxValue;

                    List<Tile>[] list = new List<Tile>[count];

                    for (int i = 0; i < count; i++)
                    {
                        list[i] = Pathfinding.FindPath(new Vector2(character.PosX, character.PosY), new Vector2(provWeapon[i].PosX, provWeapon[i].PosY), character.Map, character);

                        if ((list[i] != null && list[i].Count != 0))
                        {
                            if (shortestId == -1)
                            {
                                shortestValue = list[i][list[i].Count - 1].Data[character.Id].GCost;
                                shortestId = i;
                            }
                            else if (shortestValue > list[i].Last().Data[character.Id].GCost)
                            {
                                shortestValue = list[i].Last().Data[character.Id].GCost;
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
                        x = random.Next(character.Map.GameConfig.width);
                        y = random.Next(character.Map.GameConfig.height);
                    } while (!character.Map.CanMoveToCellExcludingFutureDestroyed(x, y));
                        
                    List<Tile> list = new List<Tile>();
                    list = Pathfinding.FindPath(new Vector2(character.PosX, character.PosY), new Vector2(x, y), character.Map, character);

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