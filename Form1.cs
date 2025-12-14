using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace maze_algorithm
{
    public partial class MainForm : Form
    {
        private Maze maze;
        private const int CellSize = 20;     // pixels per cell
        private const int WallThickness = 2; // line thickness

        // store the path as a list of (x, y)
        private List<(int x, int y)> solutionPath;
        private bool[,] visited;

        public MainForm()
        {
            Text = "Maze Preview";
            DoubleBuffered = true; // reduce flicker

            int w = 40;
            int h = 40;
            maze = new Maze(w, h);

            // compute solution path from (0,0) to (w-1,h-1)
            solutionPath = NaiveAlgorithm.Solve((0, 0), (w - 1, h - 1), maze, out visited);

            int padding = 40;
            ClientSize = new Size(
                w * CellSize + padding,
                h * CellSize + padding
            );

            // Regenerate button
            var btn = new Button
            {
                Text = "Regenerate",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            btn.Click += (s, e) =>
            {
                maze = new Maze(w, h);
                solutionPath = NaiveAlgorithm.Solve((0, 0), (w - 1, h - 1), maze, out visited);
                Invalidate(); // redraw
            };
            Controls.Add(btn);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int offsetX = 20;
            int offsetY = 50;

            using (var wallPen = new Pen(Color.Black, WallThickness))
            using (var gridPen = new Pen(Color.LightGray, 1))
            using (var startBrush = new SolidBrush(Color.LightGreen))
            using (var endBrush = new SolidBrush(Color.LightCoral))
            using (var solutionPen = new Pen(Color.LightBlue, 10))
            using (var solutionBrush = new SolidBrush(Color.FromArgb(80, Color.GreenYellow)))
            {
                // background grid
                for (int x = 0; x < maze.Width; x++)
                {
                    for (int y = 0; y < maze.Height; y++)
                    {
                        int sx = offsetX + x * CellSize;
                        int sy = offsetY + y * CellSize;
                        e.Graphics.DrawRectangle(gridPen, sx, sy, CellSize, CellSize);
                    }
                }

                // start & end cells
                int startX = 0;
                int startY = 0;
                int endX = maze.Width - 1;
                int endY = maze.Height - 1;

                e.Graphics.FillRectangle(
                    startBrush,
                    offsetX + startX * CellSize + 2,
                    offsetY + startY * CellSize + 2,
                    CellSize - 4,
                    CellSize - 4
                );

                e.Graphics.FillRectangle(
                    endBrush,
                    offsetX + endX * CellSize + 2,
                    offsetY + endY * CellSize + 2,
                    CellSize - 4,
                    CellSize - 4
                );

                // draw walls
                for (int x = 0; x < maze.Width; x++)
                {
                    for (int y = 0; y < maze.Height; y++)
                    {
                        Cell c = maze.cells[x, y];

                        int sx = offsetX + x * CellSize;
                        int sy = offsetY + y * CellSize;
                        int ex = sx + CellSize;
                        int ey = sy + CellSize;

                        if (!c.up)
                            e.Graphics.DrawLine(wallPen, sx, sy, ex, sy);

                        if (!c.down)
                            e.Graphics.DrawLine(wallPen, sx, ey, ex, ey);

                        if (!c.left)
                            e.Graphics.DrawLine(wallPen, sx, sy, sx, ey);

                        if (!c.right)
                            e.Graphics.DrawLine(wallPen, ex, sy, ex, ey);
                    }
                }

                // PATH VISUALIZATION
                if (solutionPath != null && solutionPath.Count > 0)
                {
                    // fill each cell on the path
                    for (int i = 0; i < visited.GetLength(0); i++)
                    {
                        for (int j = 0; j < visited.GetLength(1); j++)
                        {
                            if (visited[i, j] == true)
                            {
                                int px = offsetX + i * CellSize;
                                int py = offsetY + j * CellSize;

                                e.Graphics.FillRectangle(
                                    solutionBrush,
                                    px + 2,
                                    py + 2,
                                    CellSize,
                                    CellSize
                                );
                            }
                        }
                    }

                    // draw blue line through centers
                    for (int i = 0; i < solutionPath.Count - 1; i++)
                    {
                        var a = solutionPath[i];
                        var b = solutionPath[i + 1];

                        int ax = offsetX + a.x * CellSize + CellSize / 2;
                        int ay = offsetY + a.y * CellSize + CellSize / 2;
                        int bx = offsetX + b.x * CellSize + CellSize / 2;
                        int by = offsetY + b.y * CellSize + CellSize / 2;

                        e.Graphics.DrawLine(solutionPen, ax, ay, bx, by);
                    }
                }
            }
        }
    }
}