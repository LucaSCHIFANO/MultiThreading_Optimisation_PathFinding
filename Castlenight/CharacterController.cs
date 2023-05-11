﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        public void ComputeAndExecuteAction(Character character)
        {
            //Random action controller: will do something random (but valid) on each tick
            if (running)
                throw new Exception("ComputeAndExecuteAction called twice");
            running = true;


            Character.Mutex.WaitOne();


            if (character.Pv == 0)
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


            int dir = random.Next(4);
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
            }
            //Debug.WriteLine("New character position: " + character.PosX + ", " + character.PosY);
            Character.Mutex.ReleaseMutex();
            running = false;

        }

        public void TileAboutToBeDestroyed(List<Vector2> tilesToBeDestroyed, double timeBeforeDestruction)
        {
        }
    }
}