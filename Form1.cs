using System;
using System.Drawing;
using System.Windows.Forms;

namespace maze_algorithm;
    public partial class MainForm : Form
    {
        private Maze maze;
        private const int CellSize = 20;     // pixels per cell
        private const int WallThickness = 2; // line thickness

        public MainForm()
        {
            Text = "Maze Preview";
            DoubleBuffered = true; // reduce flicker

            // Create maze
            int w = 40;
            int h = 40;
            maze = new Maze(w, h);

            // Set form size based on maze dimensions
            int padding = 40;
            ClientSize = new Size(
                w * CellSize + padding,
                h * CellSize + padding
            );

            // Add a button to regenerate maze
            var btn = new Button
            {
                Text = "Regenerate",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            btn.Click += (s, e) =>
            {
                maze = new Maze(w, h);
                Invalidate(); // redraw
            };
            Controls.Add(btn);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // We reserve some top-left space for the button.
            int offsetX = 20;
            int offsetY = 50;

            using (var wallPen = new Pen(Color.Black, WallThickness))
            using (var pathPen = new Pen(Color.LightGray, 1))
            using (var startBrush = new SolidBrush(Color.LightGreen))
            using (var endBrush = new SolidBrush(Color.LightCoral))
            {
                // optional, draw background grid
                for (int x = 0; x < maze.Width; x++)
                {
                    for (int y = 0; y < maze.Height; y++)
                    {
                        int sx = offsetX + x * CellSize;
                        int sy = offsetY + y * CellSize;
                        e.Graphics.DrawRectangle(pathPen, sx, sy, CellSize, CellSize);
                    }
                }

                // highlight start and end cells
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

                // draw walls based on up/down/left/right flags
                for (int x = 0; x < maze.Width; x++)
                {
                    for (int y = 0; y < maze.Height; y++)
                    {
                        Cell c = maze.cells[x, y];

                        int sx = offsetX + x * CellSize;
                        int sy = offsetY + y * CellSize;
                        int ex = sx + CellSize;
                        int ey = sy + CellSize;

                        // If a direction is false, there is a wall
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
            }
        }
    }
