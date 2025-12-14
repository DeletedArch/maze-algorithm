using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;

namespace maze_algorithm
{
    public class GlobalSettings
    {
        public static bool UseAnimation = true;
    }

    public partial class MainForm : Form
    {
        private Maze maze;
        private const int CellSize = 20;
        private const int WallThickness = 2;

        private List<(int x, int y)> solutionPath;
        private bool[,] visited;
        private Thread solverThread;
        private bool solverCancelRequested = false;

        // ===== TIMER DISPLAY: label to show elapsed time =====
        private Label timeLabel;
        // ======================================================

        public MainForm()
        {
            Text = "Maze Preview";
            DoubleBuffered = true;

            int w = 50;
            int h = 50;
            maze = new Maze(w, h);

            int padding = 40;
            ClientSize = new Size(
                w * CellSize + padding,
                h * CellSize + padding + 30
            );

            // ===== TIMER DISPLAY: create the label =====
            timeLabel = new Label
            {
                Text = "Time: --",
                AutoSize = true,
                Location = new Point(700, 10),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            Controls.Add(timeLabel);
            // ============================================

            // start initial solve
            solverThread = new Thread(() =>
            {
                long timeMs;
                OptimizedAlgorithm.Solve(
                    (0, 0),
                    (w - 1, h - 1),
                    maze,
                    ref visited,
                    ref solutionPath,
                    () =>
                    {
                        if (InvokeRequired)
                            BeginInvoke(new Action(Invalidate));
                        else
                            Invalidate();
                    },
                    () => solverCancelRequested,
                    out timeMs);

                // ===== TIMER DISPLAY: update label on UI thread =====
                UpdateTimeLabel(timeMs);
                // =====================================================
            })
            { IsBackground = true };
            solverThread.Start();

            // Regenerate Optimized button
            var btn = new Button
            {
                Text = "Regenerate Optimized",
                AutoSize = true,
                Location = new Point(10, 10)
            };

            // Regenerate Naive button
            var btn2 = new Button
            {
                Text = "Regenerate Naive",
                AutoSize = true,
                Location = new Point(200, 10)
            };

            btn.Click += (s, e) =>
            {
                solverCancelRequested = true;
                if (solverThread != null && solverThread.IsAlive)
                {
                    try { solverThread.Join(50); }
                    catch { }
                }

                maze = new Maze(w, h);

                // ===== TIMER DISPLAY: reset label =====
                UpdateTimeLabel("Running...");
                // ======================================

                solverThread = new Thread(() =>
                {
                    long timeMs;
                    OptimizedAlgorithm.Solve(
                        (0, 0),
                        (w - 1, h - 1),
                        maze,
                        ref visited,
                        ref solutionPath,
                        () =>
                        {
                            if (InvokeRequired)
                                BeginInvoke(new Action(Invalidate));
                            else
                                Invalidate();
                        },
                        () => solverCancelRequested,
                        out timeMs);

                    // ===== TIMER DISPLAY: update label =====
                    UpdateTimeLabel(timeMs);
                    // ========================================
                })
                { IsBackground = true };

                solverCancelRequested = false;
                solverThread.Start();
                Invalidate();
            };

            btn2.Click += (s, e) =>
            {
                solverCancelRequested = true;
                if (solverThread != null && solverThread.IsAlive)
                {
                    try { solverThread.Join(50); }
                    catch { }
                }

                maze = new Maze(w, h);

                // ===== TIMER DISPLAY: reset label =====
                UpdateTimeLabel("Running...");
                // ======================================

                solverThread = new Thread(() =>
                {
                    long timeMs;
                    NaiveAlgorithm.Solve(
                        (0, 0),
                        (w - 1, h - 1),
                        maze,
                        ref visited,
                        ref solutionPath,
                        () =>
                        {
                            if (InvokeRequired)
                                BeginInvoke(new Action(Invalidate));
                            else
                                Invalidate();
                        },
                        () => solverCancelRequested,
                        out timeMs);

                    // ===== TIMER DISPLAY: update label =====
                    UpdateTimeLabel(timeMs);
                    // ========================================

                    Invalidate();
                })
                { IsBackground = true };

                solverCancelRequested = false;
                solverThread.Start();
                Invalidate();
            };

            Button animButton = new Button
            {
                Text = "Toggle Animation",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            animButton.Click += (s, e) =>
            {
                GlobalSettings.UseAnimation = !GlobalSettings.UseAnimation;
            };

            Controls.Add(btn);
            Controls.Add(btn2);
            Controls.Add(animButton);
        }

        // ===== TIMER DISPLAY: helper methods to update label =====
        private void UpdateTimeLabel(long milliseconds)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => timeLabel.Text = $"Time: {milliseconds} ms"));
            }
            else
            {
                timeLabel.Text = $"Time: {milliseconds} ms";
            }
        }

        private void UpdateTimeLabel(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => timeLabel.Text = $"Time: {text}"));
            }
            else
            {
                timeLabel.Text = $"Time: {text}";
            }
        }
        // ==========================================================

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int offsetX = 20;
            int offsetY = 50;

            using (var wallPen = new Pen(Color.Black, WallThickness))
            using (var gridPen = new Pen(Color.LightGray, 1))
            using (var startBrush = new SolidBrush(Color.LightCoral))
            using (var endBrush = new SolidBrush(Color.LightGreen))
            using (var solutionPen = new Pen(Color.Black, 6))
            using (var solutionBrush = new SolidBrush(Color.FromArgb(100, 150, 150, 150)))
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
                if (solutionPath != null && solutionPath.Count > 0 && visited != null)
                {
                    // fill visited cells
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

                    // draw line through path centers
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