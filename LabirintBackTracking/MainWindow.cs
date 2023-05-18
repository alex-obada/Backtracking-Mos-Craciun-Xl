using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Mail;
using System.Threading;

namespace LabirintBackTracking
{
    public partial class MainWindow : Form
    {
        private readonly int[] di = { -1, -1, 1,  1 };
        private readonly int[] dj = { -1,  1, 1, -1 };

        private int n;
        private bool[,] obstacles;
        private int[,] labirint;
        private List<KeyValuePair<int, int>> path = new List<KeyValuePair<int, int>>();
        
        private TableLayoutPanel tablePanel;
        private Label[,] table;
        private const string blockedTile = "██";
        private const string openTile = "";

        private readonly Color openTileColor = Color.White;
        private readonly Color visitedTileColor = Color.Aqua;
        private readonly Color pathTileColor = Color.Green;
        private readonly Color blockedTileColor = Color.Red;

        Thread backtrackingThread = null;


        public MainWindow()
        {
            DoubleBuffered = true;
            InitializeComponent();
            ReadData();
            InitializeGUI();
        }

        private void InitializeGUI()
        {
            table = new Label[n, n];
            tablePanel = new TableLayoutPanel()
            {
                RowCount = n,
                ColumnCount = n,
                Dock = DockStyle.Fill,
            };

            int width = panel.Width;
            int height = panel.Height;
            int lenght = Math.Max(width, height) / n;

            for (int i = 0; i < n; i++)
                tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, lenght));

            for (int i = 0; i < n; i++)
            {
                tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, lenght));

                for (int j = 0; j < n; j++)
                {
                    table[i, j] = new Label()
                    {
                        BackColor = obstacles[i, j] ? blockedTileColor : openTileColor,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill,
                        Text = obstacles[i, j] ? blockedTile : openTile,
                        Font = new Font("Arial", 24),
                        BorderStyle = BorderStyle.FixedSingle,
                        Margin = new Padding(2, 2, 2, 2),
                    };
                    table[i, j].Click += HandleTileClicked;
                    tablePanel.Controls.Add(table[i, j], j, i);
                }
            }
            panel.Controls.Add(tablePanel);
        }

        private void HandleTileClicked(object sender, EventArgs e)
        {
            if (backtrackingThread != null && backtrackingThread.IsAlive)
                return;

            Label label = (Label)sender;

            FindCoordinates(label, out int i, out int j);

            if (obstacles[i, j])
            {
                label.BackColor = openTileColor;
                label.Text = openTile;
            }
            else
            {
                label.BackColor = blockedTileColor;
                label.Text = blockedTile;
            }

            obstacles[i, j] = !obstacles[i, j];
        }

        private void FindCoordinates(Label label, out int i, out int j)
        {
            j = 0;
            for (i = 0; i < n; ++i)
                for (j = 0; j < n; ++j)
                    if (label == table[i, j])
                        return;
        }

        private void ReadData() 
        {
            StreamReader streamReader = null;
            try
            {
                streamReader = new StreamReader("./input.txt");
                string line;
                char[] sep = { ' ' };

                line = streamReader.ReadLine();
                n = int.Parse(line);
                obstacles = new bool[n, n];
                labirint = new int[n, n];

                for (int i = 0; (line = streamReader.ReadLine()) != null; i++)
                {
                    string[] split = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < n; j++)
                        if (int.Parse(split[j]) != 0)
                            obstacles[i, j] = true;
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Nu s-a gasit fisierul input.txt",
                                "Eroare", MessageBoxButtons.OK, 
                                MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            catch (Exception)
            {
                MessageBox.Show("Format incorect al fisierului input.txt",
                                "Eroare", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                    streamReader.Dispose();
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (backtrackingThread != null && backtrackingThread.IsAlive)
                return;
            
            backtrackingThread = new Thread(() => BackTracking());
            backtrackingThread.Start();
        }

        private void BackTracking(int i = 0, int j = 0, int step = 1)
        {
            if (!IsValidTile(i, j)) 
                return;

            labirint[i, j] = step;
            path.Add(new KeyValuePair<int, int>(i, j));
            UpdateTile(i, j, step.ToString(), visitedTileColor, 300);

            //MessageBox.Show($"Celula ({i + 1}, {j + 1}), pasul {step}");

            // stop condition
            if (i == n - 1 && j == n - 1)
                DisplayPath();
            else 
                // main loop
                for (int d = 0; d < 4; ++d)
                    BackTracking(i + di[d], j + dj[d], step + 1);

            UpdateTile(i, j, openTile, openTileColor, 300);
            path.RemoveAt(path.Count - 1);
            labirint[i, j] = 0;
        }

        private void DisplayPath()
        {
            int i, j, step = 1;
            foreach (var pair in path)
            {
                i = pair.Key;
                j = pair.Value;
                UpdateTile(i, j, step.ToString(), pathTileColor);
                step++;
            }

            Thread.Sleep(1500);
            for (int p = path.Count - 1; p >= 0; --p)
            {
                i = path[p].Key;
                j = path[p].Value;

                UpdateTile(i, j, $"{p + 1}", visitedTileColor);
            }

        }

        private void UpdateTile(int i, int j, string text, Color color, int sleepInterval = 0)
        {
            if (table[i, j].InvokeRequired)
            {
                Thread.Sleep(sleepInterval);
                table[i, j]?.Invoke(
                    new Action<int, int, string, Color, int>(UpdateTile), 
                    i, j, text, color, sleepInterval);
                return;
            }

            table[i, j].BackColor = color;
            table[i, j].Text = text;
            table[i, j].Update();
        }

        private bool IsValidTile(int i, int j)
        {
            if (i < 0 || i >= n || j < 0 || j >= n)
                return false;

            if (labirint[i, j] != 0) 
                return false;

            if (obstacles[i, j]) 
                return false;

            return true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (backtrackingThread == null || !backtrackingThread.IsAlive)
                return;

            backtrackingThread.Abort();

            labirint = new int[n, n];
            foreach(var step in path)
                UpdateTile(step.Key, step.Value, openTile, openTileColor);

            path = new List<KeyValuePair<int, int>>();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (backtrackingThread == null || !backtrackingThread.IsAlive)
                return;

            backtrackingThread.Abort();
        }

    }
}
