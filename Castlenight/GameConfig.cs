namespace Castlenight
{
    public class GameConfig
    {
        //global game speed: increase to make map destruction & weapon drop faster
        public const int MAP_DESTRUCTION_SPEED = 3;
        //AI action speed (movement, shoot). Increase to make things go faster
        //Default value should have been 10, but it's unplayable with initial version
        public const int PLAYER_MOVE_SPEED = 1000;

        //size of the window
        public const int WINDOW_WIDTH = 1280;
        public const int WINDOW_HEIGHT = 720;

        //map size
        public int width = 30;
        public int height = 20;
        public const int tileSize = 30;

        //player count + how many times they'll try to move when there is no weapon + probability to shoot
        public int playerCount = 30;
        public const int numberOfTryPlayerMove = 5;
        public const int shootProba = 25;

        //how many weapons are dropped each time and if the pathfinding of player has to be recalculed
        public int crateDropCount = 5;
        public const int numberOfTryWeaponDrop = 15;
        public const bool needRecalcule = false;     
        
        //how many tiles are destryed each time
        public int destoyedTilesCount = 15;

        //timers for map destruction & weapon drop
        public int weaponDropTimer = 7;
        public int triggerTileDestructionTimer = 10;
        public int executeTileDestructionTimer = 5;

        //if the tiles will be destroyed in more than 0.5sec (no matter the destroy speed multiplier), its can be crossed
        public float okayToCrossTimer = 0.5f;


    }
}