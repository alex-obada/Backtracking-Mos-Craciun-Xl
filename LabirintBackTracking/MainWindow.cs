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
        private readonly List<KeyValuePair<int, int>> path = new List<KeyValuePair<int, int>>();
        
        private TableLayoutPanel tablePanel;
        private Label[,] table;
        

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

            int width = tablePanel.Width;
            int height = tablePanel.Height;
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
                        BackColor = obstacles[i, j] ? Color.Red : Color.White,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill,
                        Text = obstacles[i, j] ? "██" : "",
                        Font = new Font("Arial", 24),
                        BorderStyle = BorderStyle.FixedSingle,
                        Margin = new Padding(2, 2, 2, 2),
                    };
                    tablePanel.Controls.Add(table[i, j], j, i);
                }
            }
            panel.Controls.Add(tablePanel);
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

        private bool lockBacktracking = false;
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (lockBacktracking)
                return;
            
            lockBacktracking = true;

            BackTracking(0, 0, 1);

            lockBacktracking = false;
        }

        private void BackTracking(int i, int j, int step)
        {
            if (!IsValid(i, j)) 
                return;

            labirint[i, j] = step;
            path.Add(new KeyValuePair<int, int>(i, j));
            
            //MessageBox.Show($"Celula ({i + 1}, {j + 1}), pasul {step}");

            // stop condition
            if (i == n - 1 && j == n - 1)
                DisplayPath();
            else 
                // main loop
                for (int d = 0; d < 4; ++d)
                    BackTracking(i + di[d], j + dj[d], step + 1);

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

                table[i, j].BackColor = Color.Green;
                table[i, j].Text = $"{step}";

                step++;
                Application.DoEvents();
                Thread.Sleep(100);
            }

            Thread.Sleep(1000);
            foreach (var pair in path)
            {
                i = pair.Key;
                j = pair.Value;

                table[i, j].BackColor = Color.White;
                table[i, j].Text = "";
            }

            Application.DoEvents();
        }

        private bool IsValid(int i, int j)
        {
            if (i < 0 || i >= n || j < 0 || j >= n)
                return false;

            if (labirint[i, j] != 0) 
                return false;

            if (obstacles[i, j]) 
                return false;

            return true;
        }

    }
}
