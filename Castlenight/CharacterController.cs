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
            if (running)
                throw new Exception("ComputeAndExecuteAction called twice");
            running = true;


            character.RwlsPV.EnterReadLock(); 

            if (character.Pv <= 0)
            {
                running = false;
                character.RwlsPV.ExitReadLock();
                return;
            }
            character.RwlsPV.ExitReadLock();

            Random random = new Random();

            if (character.weapon != null && character.weapon.Ammo > 0)
            {
                var targets = character.Map.GetCharactersInRange(character, character.weapon.Range);
                if (targets.Count > 0 && random.Next(100) < character.ShootProba)
                {
                    character.Score += character.weapon.Shoot(targets[random.Next(targets.Count)]);
                    running = false;
                    return;
                }
            }

            //pathfinding
            character.RwlsPV.EnterReadLock(); // check to not be killed when moving

            if (nextTile == null || nextTile.Count == 0 || (character.NeedRecheck && GameConfig.needRecalcule)) // if there is no path save
            {
                character.NeedRecheck = false;
                GetNextTile(character);
            }
            else if (!character.Map.CheckBeforeMove((int)nextTile[nextTileId].GetPosition().X, (int)nextTile[nextTileId].GetPosition().Y)) // if the next tile is invalid
                GetNextTile(character);
            else // move to the next tile 
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
            character.RwlsPV.ExitReadLock();
        }

        private void GetNextTile(Character character)
        {
            try
            {
                CastleNightGame.Instance.Rwls.EnterReadLock();

                int count = character.Map.Weapons.Count;
                List<WeaponBox> provWeapon = character.Map.Weapons;


                if (count > 0) // check if there is at least one weapon
                {
                    int shortestId = -1;
                    int shortestValue = int.MaxValue;

                    List<Tile>[] list = new List<Tile>[count];

                    for (int i = 0; i < count; i++) // check the path the every weapon to see which one is the closest
                    {
                        list[i] = Pathfinding.FindPath(new Vector2(character.PosX, character.PosY), new Vector2(provWeapon[i].PosX, provWeapon[i].PosY), character.Map, character);

                        if ((list[i] != null && list[i].Count != 0)) // check if the path is valid
                        {
                            if (shortestId == -1) // there if a path is already saved
                            {
                                shortestValue = list[i][list[i].Count - 1].Data[character.Id].GCost;
                                shortestId = i;
                            }
                            else if (shortestValue > list[i].Last().Data[character.Id].GCost) // check if the new path is better than the old one
                            {
                                shortestValue = list[i].Last().Data[character.Id].GCost;
                                shortestId = i;
                            }
                        }
                    }

                    if (shortestId != -1 && list[shortestId] != null) // if the path is valid, the path is updated
                    {
                        nextTile.Clear();
                        nextTile = list[shortestId];
                        nextTileId = 0;
                    }
                }
                else // if there is no weapon, take a random tile and try to move to it
                {
                    List<Tile> list = new List<Tile>();
                    int id = 0;

                    int x, y;
                    do
                    {
                        Random random = new Random();
                        x = random.Next(character.Map.GameConfig.width);
                        y = random.Next(character.Map.GameConfig.height);
                        id++;
                    } while (!character.Map.CanMoveToCellExcludingFutureDestroyed(x, y) || id < GameConfig.numberOfTryPlayerMove); // if there is too much try without a valid path, abort

                    list.Clear();
                    list = Pathfinding.FindPath(new Vector2(character.PosX, character.PosY), new Vector2(x, y), character.Map, character);

                    if ((list != null && list.Count != 0)) // if a path was found, path updated
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