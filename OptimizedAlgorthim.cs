using maze_algorithm;
using System.Diagnostics;

public static class OptimizedAlgorithm
{
    public static void Solve(
        (int x, int y) start,
        (int x, int y) end,
        Maze maze, ref bool[,] visited, ref List<(int x, int y)> pathStack, Action invalidate, Func<bool> cancelRequested, out long elapsedMilliseconds)
    {
        // ===== TIMER: create and start =====
        Stopwatch timer = new Stopwatch();
        timer.Start();
        // ===================================
        Stack<(int x, int y)> stack = new Stack<(int x, int y)>();
        pathStack = new List<(int x, int y)>();
        visited = new bool[maze.Width, maze.Height];

        stack.Push(start);
        visited[start.x, start.y] = true;
        pathStack.Add(start);  // path and stack both have just 'start'

        while (stack.Count > 0)
        {
            if (cancelRequested())
            {
                // ===== TIMER: stop before returning =====
                timer.Stop();
                elapsedMilliseconds = timer.ElapsedMilliseconds;
                // =========================================
                return;
            }
            invalidate();
            Thread.Sleep(GlobalSettings.UseAnimation ? 1 : 0);
            var currentPos = stack.Peek();

            // reached the end
            if (currentPos.x == end.x && currentPos.y == end.y)
            {
                // ===== TIMER: stop before returning =====
                timer.Stop();
                elapsedMilliseconds = timer.ElapsedMilliseconds;
                // =========================================
                return;   // pathStack now contains the path from start to end
            }

            var neighbors = Maze.GetUnvisitedNeighbors(
                currentPos.x, currentPos.y,
                maze, visited);

            if (neighbors.Count == 0)
            {
                // dead end: backtrack one step on both stack and pathStack
                stack.Pop();
                pathStack.RemoveAt(pathStack.Count - 1);
                continue;
            }

            // take the first neighbor (you could randomize or pick any)
            var next = neighbors[0];
            visited[next.x, next.y] = true;
            stack.Push(next);
            pathStack.Add(next);
        }

        // no path found
        // ===== TIMER: stop before returning =====
        timer.Stop();
        elapsedMilliseconds = timer.ElapsedMilliseconds;
        // =========================================
        return;
    }
}