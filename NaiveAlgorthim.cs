using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace maze_algorithm
{
    public static class NaiveAlgorithm
    {
        public static void Solve(
            (int x, int y) start,
            (int x, int y) end,
            Maze maze,
            ref bool[,] visited,
            ref List<(int x, int y)> path,
            Action invalidate,
            Func<bool> cancelRequested,
            out long elapsedMilliseconds)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            visited = new bool[maze.Width, maze.Height];
            path = new List<(int x, int y)>();

            var currentPath = new List<(int x, int y)>();
            List<(int x, int y)> bestPath = null;

            // start recursive exploration
            Explore(
                start.x, start.y,
                end,
                maze,
                visited,
                currentPath,
                ref bestPath,
                invalidate,
                cancelRequested);

            // copy best path to output
            if (bestPath != null)
            {
                path = bestPath;
            }
           
            for (int i = 0; i < visited.GetLength(0); i++)
            {
                for (int j = 0; j < visited.GetLength(1); j++)
                {
                    visited[i, j] = true;
                }
            }

            timer.Stop();
            elapsedMilliseconds = timer.ElapsedMilliseconds;

            return;
        }

        private static void Explore(
            int x, int y,
            (int x, int y) end,
            Maze maze,
            bool[,] visited,
            List<(int x, int y)> currentPath,
            ref List<(int x, int y)> bestPath,
            Action invalidate,
            Func<bool> cancelRequested)
        {
            // check for cancel
            if (cancelRequested())
                return;

            // out of bounds
            if (x < 0 || x >= maze.Width || y < 0 || y >= maze.Height)
                return;

            // already visited in this path
            if (visited[x, y])
                return;

            // pruning: if current path is already longer than best, stop
            if (bestPath != null && currentPath.Count >= bestPath.Count)
                return;

            // mark visited and add to path
            visited[x, y] = true;
            currentPath.Add((x, y));

            // visualize (optional, can slow things down a lot)
            // if (GlobalSettings.UseAnimation)
            // {
            //     invalidate();
            //     Thread.Sleep(1);
            // }

            // check if we reached the goal
            if (x == end.x && y == end.y)
            {
                // found a path! save if it's the best
                if (bestPath == null || currentPath.Count < bestPath.Count)
                {
                    bestPath = new List<(int x, int y)>(currentPath);
                }
            }
            else
            {
                // try all four directions
                var moves = Maze.GetPossibleMoves(x, y, maze);

                foreach (var move in moves)
                {
                    Explore(
                        move.x, move.y,
                        end,
                        maze,
                        visited,
                        currentPath,
                        ref bestPath,
                        invalidate,
                        cancelRequested);
                }
            }

            // backtrack: unmark visited and remove from path
            visited[x, y] = false;
            currentPath.RemoveAt(currentPath.Count - 1);
        }
    }
}