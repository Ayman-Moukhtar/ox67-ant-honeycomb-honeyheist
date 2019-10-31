using System;
using System.Collections.Generic;
using System.Linq;

namespace Honeycomb
{
    public class MainClass
    {
        #region Data Structures
        public class Coordinates
        {
            public Coordinates(int X, int Y, int Z)
            {
                this.X = X;
                this.Y = Y;
                this.Z = Z;
            }
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public override string ToString()
            {
                return X + "" + Y + "" + Z;
            }
        }

        public class Cell
        {
            public Coordinates Coordinates { get; set; }
            public Cell LeadingCell { get; set; }
            public bool IsBlocked { get; set; }
            public bool IsExplored { get; set; }
            public int Id { get; set; }
        }

        public class Honeycomb
        {
            private readonly Func<Coordinates, Coordinates>[] _directions =
            {
                // Right
                (Coordinates coordinates) => new Coordinates(coordinates.X, coordinates.Y + 1, coordinates.Z - 1),

                // Left
                (Coordinates coordinates) => new Coordinates(coordinates.X, coordinates.Y - 1, coordinates.Z + 1),

                // Down Left
                (Coordinates coordinates) => new Coordinates(coordinates.X + 1, coordinates.Y - 1, coordinates.Z),

                // Down Right
                (Coordinates coordinates) => new Coordinates(coordinates.X + 1, coordinates.Y, coordinates.Z - 1),

                // Up Left
                (Coordinates coordinates) => new Coordinates(coordinates.X - 1, coordinates.Y, coordinates.Z + 1),

                // Up Right
                (Coordinates coordinates) => new Coordinates(coordinates.X - 1, coordinates.Y + 1, coordinates.Z)
            };

            public Honeycomb()
            {
                Cells = new Dictionary<string, Cell>();
            }
            // Key: Stringified Cube Coordinates, Value: Cell
            public Dictionary<string, Cell> Cells { get; set; }
            public Cell Start { get; set; }
            public Cell End { get; set; }

            public List<Cell> GetUnexploredUnblockedNeighbors(Cell current)
            {
                var cells = new List<Cell>();

                foreach (var direction in _directions)
                {
                    var coordinates = direction(current.Coordinates).ToString();

                    if (!Cells.ContainsKey(coordinates)) { continue; }

                    var cell = Cells[coordinates];

                    if (cell.IsExplored || cell.IsBlocked) { continue; }

                    cells.Add(Cells[coordinates]);
                }

                return cells;
            }
        }
        #endregion

        public static void Main(string[] args)
        {
            WalkTheAnt(6, 6, 1, 45, new[] { 15, 16, 17, 19, 26, 27, 52, 53, 58, 65, 74 });
            Console.WriteLine();
            Console.ReadLine();
        }

        public static void WalkTheAnt(
            int numberOfCellsOnEdge,
            int maxNoOfCellsAntCanWalk,
            int startingCellId,
            int targetCellId,
            int[] blockedCellsIds
            )
        {
            var queue = new Queue<Cell>();
            var blockedCellsIdsHash = new HashSet<int>(blockedCellsIds);
            var honeycomb = BuildHoneycomb(numberOfCellsOnEdge, startingCellId, targetCellId, blockedCellsIdsHash);
        
            Cell current = null;

            honeycomb.Start.IsExplored = true;
            queue.Enqueue(honeycomb.Start);

            while (queue.Any())
            {
                current = queue.Dequeue();

                // If reached destination
                if (current == honeycomb.End)
                {
                    break;
                }

                honeycomb.GetUnexploredUnblockedNeighbors(current)
                    .ForEach(cell =>
                    {
                        if (!queue.Contains(cell))
                        {
                            queue.Enqueue(cell);
                            cell.IsExplored = true;
                        }

                        cell.LeadingCell = current;
                    });
            }

            var shortestSteps = GetShortestStepsFromBreadCrumbs(current);

            if (shortestSteps > maxNoOfCellsAntCanWalk)
            {
                Console.WriteLine("No");
                return;
            }
            Console.WriteLine(shortestSteps);
        }

        private static int GetShortestStepsFromBreadCrumbs(Cell end)
        {
            var tracker = end;
            var steps = 0;

            while (tracker.LeadingCell != null)
            {
                steps += 1;
                tracker = tracker.LeadingCell;
            }

            return steps;
        }

        public static Honeycomb BuildHoneycomb(int numberOfCellsOnEdge, int startingCellId, int targetCellId, HashSet<int> blockedCellsIdsHash)
        {
            var stepsFromCenter = numberOfCellsOnEdge - 1;
            var currentCellId = 1;
            var cells = new Dictionary<string, Cell>();
            Cell start = null;
            Cell end = null;

            for (var x = -stepsFromCenter; x <= stepsFromCenter; x += 1)
            {
                for (var y = Math.Max(-stepsFromCenter, -x - stepsFromCenter); y <= Math.Min(stepsFromCenter, -x + stepsFromCenter); y += 1)
                {
                    var z = -x - y;
                    var coordinates = new Coordinates(x, y, z);
                    var cell = new Cell
                    {
                        Coordinates = coordinates,
                        Id = currentCellId,
                        IsBlocked = blockedCellsIdsHash.Contains(currentCellId)
                    };
                    if (currentCellId == startingCellId)
                    {
                        start = cell;
                    }
                    else if (currentCellId == targetCellId)
                    {
                        end = cell;
                    }

                    cells.Add(coordinates.ToString(), cell);
                    currentCellId +=1;
                }
            }

            return new Honeycomb
            {
                Cells = cells,
                Start = start,
                End = end
            };
        }
    }
}
