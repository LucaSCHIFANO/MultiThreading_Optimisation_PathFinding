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

            if (nextTile.Count == 0) GetNextTile(character);
            else if (!CastleNightGame.Instance.Map.CanMoveToCell((int)nextTile[nextTileId].GetPosition().X, (int)nextTile[nextTileId].GetPosition().Y)) GetNextTile(character);
            else
            {
                CastleNightGame.Instance.Map.MovePlayer(character, (int)nextTile[nextTileId].GetPosition().X, (int)nextTile[nextTileId].GetPosition().Y);
                nextTileId++;
                if (nextTileId >= nextTile.Count) nextTile.Clear();
            }

            /*int dir = random.Next(4);
            if (dir == 0 && character.PosX > 0)
            {
                if (CastleNightGame.Instance.Map.CanMoveToCell(character.PosX - 1, character.PosY))
                {
                    CastleNightGame.Instance.Map.MovePlayer(character, character.PosX - 1, character.PosY);
                }
            }
            if (dir == 1 && character.PosX < CastleNightGame.Instance.Map.Width - 1)
            {
                if (CastleNightGame.Instance.Map.CanMoveToCell(character.PosX + 1, character.PosY))
                {
                    CastleNightGame.Instance.Map.MovePlayer(character, character.PosX + 1, character.PosY);
                }
            }
            if (dir == 2 && character.PosY > 0)
            {
                if (CastleNightGame.Instance.Map.CanMoveToCell(character.PosX, character.PosY - 1))
                {
                    CastleNightGame.Instance.Map.MovePlayer(character, character.PosX, character.PosY - 1);
                }
            }
            if (dir == 3 && character.PosY < CastleNightGame.Instance.Map.Height - 1)
            {
                if (CastleNightGame.Instance.Map.CanMoveToCell(character.PosX, character.PosY + 1))
                {
                    CastleNightGame.Instance.Map.MovePlayer(character, character.PosX, character.PosY + 1);
                }
            }*/


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

                int count = CastleNightGame.Instance.Map.Weapons.Count - 1;
                List<WeaponBox> provWeapon = CastleNightGame.Instance.Map.Weapons;

                if (count > 0)
                {
                    int shortestId = 0;
                    int shortestValue = int.MaxValue;

                    List<Tile>[] list = new List<Tile>[count];

                    for (int i = 0; i < count; i++)
                    {
                        list[i] = Pathfinding.FindPath(new Vector2(character.PosX, character.PosY), new Vector2(provWeapon[i].PosX, provWeapon[i].PosY), CastleNightGame.Instance.Map);
                        if (list[shortestId][list[shortestId].Count - 1].Data.currentCost > list[i][list[i].Count - 1].Data.currentCost)
                        {
                            shortestValue = list[i][list[i].Count - 1].Data.currentCost;
                            shortestId = i;
                        }
                    }

                    nextTile.Clear();
                    nextTile = list[shortestId];
                    nextTileId = 0;
                }
            }
            finally
            {
                CastleNightGame.Instance.Rwls.ExitReadLock();
            }
        }
    }
}