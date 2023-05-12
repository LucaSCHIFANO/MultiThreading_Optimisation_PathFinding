using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castlenight
{
    internal class Pathfinding
    {
        public static List<Tile> FindPath(Vector2 _start, Vector2 _end, Map map)
        {
            map.ResetTiles();
            List<Vector2> travel = new List<Vector2>();

            Tile start = CastleNightGame.Instance.Map.GetTile(_start);
            Tile end = CastleNightGame.Instance.Map.GetTile(_end);
            start.Data.currentCost = 0;


            List<Tile> openList = new List<Tile> { start };
            List<Tile> closeList = new List<Tile>();


            while (openList.Count > 0)
            {
                Tile currentTile = GetLowerCost(ref openList);
                if (currentTile == end)
                {
                    return CalculatePath(currentTile);
                }

                openList.Remove(currentTile);
                closeList.Add(currentTile);

                foreach (Tile tile in GetNeighbors(currentTile, map))
                {
                    if (closeList.Contains(tile)) continue;

                    int tentativeCost = currentTile.Data.currentCost + tile.GetCost();
                    if(tentativeCost < tile.Data.currentCost)
                    {
                        tile.Data.parent = currentTile;
                        tile.Data.currentCost = tentativeCost;

                        if (!openList.Contains(tile)) openList.Add(tile);

                    }

                }
            }
            return null;
        }

        static Tile GetLowerCost(ref List<Tile> list)
        {
            Tile lowerDistanceTile = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i].Data.currentCost < lowerDistanceTile.Data.currentCost) lowerDistanceTile = list[i];
            }
            return lowerDistanceTile;
        }

        static List<Tile> CalculatePath(Tile endSwitch)
        {
            List<Tile> list = new List<Tile>();

            Tile currentTile = endSwitch;

            while (currentTile.Data.parent != null)
            {
                list.Add(currentTile);
                currentTile = currentTile.Data.parent;
            }

            list.Reverse();
            return list;
        }

        static List<Tile> GetNeighbors(Tile tile, Map map)
        {
            List<Tile> list = new List<Tile>();
            Vector2 tilePosition = tile.GetPosition();

            if (tile.GetPosition().X > 0 && map.CanMoveToCell((int)tilePosition.X - 1, (int)tilePosition.Y))
                list.Add(map.GetTile((int)tilePosition.X - 1, (int)tilePosition.Y));

            if (tile.GetPosition().X < map.Width - 1 && map.CanMoveToCell((int)tilePosition.X + 1, (int)tilePosition.Y))
                list.Add(map.GetTile((int)tilePosition.X + 1, (int)tilePosition.Y));

            if (tile.GetPosition().Y > 0 && map.CanMoveToCell((int)tilePosition.X, (int)tilePosition.Y - 1))
                list.Add(map.GetTile((int)tilePosition.X, (int)tilePosition.Y - 1));

            if (tile.GetPosition().Y < map.Height - 1 && map.CanMoveToCell((int)tilePosition.X, (int)tilePosition.Y + 1))
                list.Add(map.GetTile((int)tilePosition.X, (int)tilePosition.Y + 1));

            return list;
        }
    }

}
