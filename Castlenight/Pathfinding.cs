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
        public static List<Tile> FindPath(Vector2 _start, Vector2 _end, Map map, Character character) // A*
        {
            var id = character.Id;
            map.ResetTiles(id);
            List<Vector2> travel = new List<Vector2>();

            Tile start = map.GetTile(_start);
            Tile end = map.GetTile(_end);

            List<Tile> openList = new List<Tile> { start };
            List<Tile> closeList = new List<Tile>();

            start.Data[id].GCost = 0;
            start.Data[id].HCost = CalculateDistance(start,end);
            start.Data[id].CalculateFCost();


            while (openList.Count > 0)
            {
                Tile currentTile = GetLowerCost(ref openList, id);
                if (currentTile == end)
                {
                    return CalculatePath(currentTile, id);
                }

                openList.Remove(currentTile);
                closeList.Add(currentTile);

                foreach (Tile tile in GetNeighbors(currentTile, map))
                {
                    int tentativeCost = currentTile.Data[character.Id].GCost + CalculateDistance(currentTile, tile);
                    if (closeList.Contains(tile) && tentativeCost > tile.Data[id].GCost) continue;

                    else if(tentativeCost < tile.Data[id].GCost)
                    {
                        tile.Data[id].parent = currentTile;
                        tile.Data[id].GCost = tentativeCost;
                        tile.Data[id].HCost = CalculateDistance(tile, end);
                        tile.Data[id].CalculateFCost();


                        if (!openList.Contains(tile)) openList.Add(tile);

                    }

                }
            }
            return null;
        }

        static Tile GetLowerCost(ref List<Tile> list, int id) 
        {
            Tile lowerFCost = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i].Data[id].FCost < lowerFCost.Data[id].FCost) lowerFCost = list[i];
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

        static List<Tile> CalculatePath(Tile endSwitch, int id) // reverse the path
        {
            List<Tile> list = new List<Tile>();

            Tile currentTile = endSwitch;

            while (currentTile.Data[id].parent != null)
            {
                list.Add(currentTile);
                currentTile = currentTile.Data[id].parent;
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
