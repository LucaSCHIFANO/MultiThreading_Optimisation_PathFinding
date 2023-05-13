using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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

            List<Tile> openList = new List<Tile> { start };
            List<Tile> closeList = new List<Tile>();

            start.Data.GCost = 0;
            start.Data.HCost = CalculateDistance(start,end);
            start.Data.CalculateFCost();


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
                    int tentativeCost = currentTile.Data.GCost + CalculateDistance(currentTile, tile);
                    if (closeList.Contains(tile) && tentativeCost > tile.Data.GCost) continue;

                    else if(tentativeCost < tile.Data.GCost)
                    {
                        tile.Data.parent = currentTile;
                        tile.Data.GCost = tentativeCost;
                        tile.Data.HCost = CalculateDistance(tile, end);
                        tile.Data.CalculateFCost();


                        if (!openList.Contains(tile)) openList.Add(tile);

                    }

                }
            }
            return null;
        }

        static Tile GetLowerCost(ref List<Tile> list)
        {
            Tile lowerFCost = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i].Data.FCost < lowerFCost.Data.FCost) lowerFCost = list[i];
            }
            return lowerFCost;
        }

        static int CalculateDistance(Tile a, Tile b)
        {
            int xDistance = (int)Math.Abs(a.GetPosition().X - b.GetPosition().X);
            int yDistance = (int)Math.Abs(a.GetPosition().Y - b.GetPosition().Y);
            int remaining = Math.Abs(xDistance - yDistance);
            return 14 * Math.Min(xDistance, yDistance) + 10 * remaining * b.GetCost();
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

            if (tilePosition.X > 0 && map.CanMoveToCellExcludingFutureDestroyed((int)tilePosition.X - 1, (int)tilePosition.Y))
                list.Add(map.GetTile((int)tilePosition.X - 1, (int)tilePosition.Y));

            if (tilePosition.X < map.Width - 1 && map.CanMoveToCellExcludingFutureDestroyed((int)tilePosition.X + 1, (int)tilePosition.Y))
                list.Add(map.GetTile((int)tilePosition.X + 1, (int)tilePosition.Y));

            if (tilePosition.Y > 0 && map.CanMoveToCellExcludingFutureDestroyed((int)tilePosition.X, (int)tilePosition.Y - 1))
                list.Add(map.GetTile((int)tilePosition.X, (int)tilePosition.Y - 1));

            if (tilePosition.Y < map.Height - 1 && map.CanMoveToCellExcludingFutureDestroyed((int)tilePosition.X, (int)tilePosition.Y + 1))
                list.Add(map.GetTile((int)tilePosition.X, (int)tilePosition.Y + 1));

            return list;
        }
    }

}
