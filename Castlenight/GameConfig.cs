﻿namespace Castlenight
{
    public class GameConfig
    {
        //global game speed: increase to make map destruction & weapon drop faster
        public const int MAP_DESTRUCTION_SPEED = 3;
        //AI action speed (movement, shoot). Increase to make things go faster
        //Default value should have been 10, but it's unplayable with initial version
        public const int PLAYER_MOVE_SPEED = 1000;

        //map size
        public int width = 40;
        public int height = 30;
        //player count
        public int playerCount = 50;
        //how many weapons are dropped each time
        public int crateDropCount = 5;
        //how many tiles are destryed each time
        public int destoyedTilesCount = 100;


        //timers for map destruction & weapon drop
        public int weaponDropTimer = 7;
        public int triggerTileDestructionTimer = 10;
        public int executeTileDestructionTimer = 5;
    }
}