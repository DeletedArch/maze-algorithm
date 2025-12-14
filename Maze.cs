using System;
using System.Collections.Generic;

public class Maze
{
    public Cell[,] cells;
    private int width;
    private int height;
    private Random rng = new Random();

    public int Width => width;
    public int Height => height;

    public Maze(int width, int height)
    {
        this.width = width;
        this.height = height;

        cells = new Cell[width, height];

        // Create cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new Cell();
            }
        }

        // Wire neighbors
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell c = cells[x, y];

                c.upCell = (y > 0) ? cells[x, y - 1] : null;
                c.downCell = (y < height - 1) ? cells[x, y + 1] : null;
                c.leftCell = (x > 0) ? cells[x - 1, y] : null;
                c.rightCell = (x < width - 1) ? cells[x + 1, y] : null;
            }
        }

        GenerateMaze();
    }

    private void GenerateMaze()
    {
        var stack = new Stack<(int x, int y)>();

        int startX = 0;
        int startY = 0;

        cells[startX, startY].Visited = true;
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            var (cx, cy) = stack.Peek();
            var neighbors = GetUnvisitedNeighbors(cx, cy);

            if (neighbors.Count == 0)
            {
                stack.Pop();
                continue;
            }

            var (nx, ny) = neighbors[rng.Next(neighbors.Count)];

            CarvePassage(cx, cy, nx, ny);

            cells[nx, ny].Visited = true;
            stack.Push((nx, ny));
        }

        // Optionally add loops to the maze
        AddLoops(width * height / 20);

        // Entrance at (0,0) open upward
        cells[0, 0].up = true;

        // Exit at (width-1,height-1) open downward
        cells[width - 1, height - 1].down = true;
    }

    private List<(int x, int y)> GetUnvisitedNeighbors(int x, int y)
    {
        var list = new List<(int x, int y)>();

        if (y > 0 && !cells[x, y - 1].Visited)
            list.Add((x, y - 1));
        if (y < height - 1 && !cells[x, y + 1].Visited)
            list.Add((x, y + 1));
        if (x > 0 && !cells[x - 1, y].Visited)
            list.Add((x - 1, y));
        if (x < width - 1 && !cells[x + 1, y].Visited)
            list.Add((x + 1, y));

        return list;
    }

    public static List<(int x, int y)> GetUnvisitedNeighbors(int x, int y, Maze maze, bool[,] visited)
    {
        var list = new List<(int x, int y)>();

        if (y > 0 && !visited[x, y - 1] && maze.cells[x, y].up == true && maze.cells[x, y - 1].down == true)
            list.Add((x, y - 1));
        if (y < maze.height - 1 && !visited[x, y + 1] && maze.cells[x, y].down == true && maze.cells[x, y + 1].up == true)
            list.Add((x, y + 1));
        if (x > 0 && !visited[x - 1, y] && maze.cells[x, y].left == true && maze.cells[x - 1, y].right == true)
            list.Add((x - 1, y));
        if (x < maze.width - 1 && !visited[x + 1, y] && maze.cells[x, y].right == true && maze.cells[x + 1, y].left == true)
            list.Add((x + 1, y));

        return list;
    }

    public static List<(int x, int y)> GetPossibleMoves(int x, int y, Maze maze)
    {
        var list = new List<(int x, int y)>();
        var c = maze.cells[x, y];

        // up
        if (c.up && y > 0 && maze.cells[x, y - 1].down)
            list.Add((x, y - 1));

        // down
        if (c.down && y < maze.Height - 1 && maze.cells[x, y + 1].up)
            list.Add((x, y + 1));

        // left
        if (c.left && x > 0 && maze.cells[x - 1, y].right)
            list.Add((x - 1, y));

        // right
        if (c.right && x < maze.Width - 1 && maze.cells[x + 1, y].left)
            list.Add((x + 1, y));

        return list;
    }

    private void CarvePassage(int x1, int y1, int x2, int y2)
    {
        if (x2 == x1 && y2 == y1 - 1)
        {
            // neighbor is above
            cells[x1, y1].up = true;
            cells[x2, y2].down = true;
        }
        else if (x2 == x1 && y2 == y1 + 1)
        {
            // neighbor is below
            cells[x1, y1].down = true;
            cells[x2, y2].up = true;
        }
        else if (x2 == x1 - 1 && y2 == y1)
        {
            // neighbor is left
            cells[x1, y1].left = true;
            cells[x2, y2].right = true;
        }
        else if (x2 == x1 + 1 && y2 == y1)
        {
            // neighbor is right
            cells[x1, y1].right = true;
            cells[x2, y2].left = true;
        }
        else
        {
            throw new InvalidOperationException("Cells are not adjacent.");
        }
    }

    private void AddLoops(int count)
{
    Random rng = new Random();
    int added = 0;

    while (added < count)
    {
        int x = rng.Next(width);
        int y = rng.Next(height);

        // pick a random direction to open
        int dir = rng.Next(4);

        switch (dir)
        {
            case 0: // up
                if (y > 0 && !cells[x, y].up)
                {
                    cells[x, y].up = true;
                    cells[x, y - 1].down = true;
                    added++;
                }
                break;
            case 1: // down
                if (y < height - 1 && !cells[x, y].down)
                {
                    cells[x, y].down = true;
                    cells[x, y + 1].up = true;
                    added++;
                }
                break;
            case 2: // left
                if (x > 0 && !cells[x, y].left)
                {
                    cells[x, y].left = true;
                    cells[x - 1, y].right = true;
                    added++;
                }
                break;
            case 3: // right
                if (x < width - 1 && !cells[x, y].right)
                {
                    cells[x, y].right = true;
                    cells[x + 1, y].left = true;
                    added++;
                }
                break;
        }
    }
}
}