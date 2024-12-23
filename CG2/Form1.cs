using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace CG2
{
    public partial class Form1 : Form
    {
        private bool showGrid = true; // Флаг для отображения/скрытия сетки
        private float scaleFactor = 1.0f;
        private const float scaleStep = 0.1f; // Шаг изменения масштаба
        int n = 12; // Количество делений
        double hx, hy; // Цена деления
        double x0, y0; // Центр прямоугольной системы координат
        private int countPoints = 1;
        private List<MyPoint> points = new List<MyPoint>();
        private Point p; // Вводимая точка P
        private int xp, yp; // Координаты точки P
        bool Transfer = true; // Режим перенос
        bool Scaling = false; // Режим масштабирование
        bool Reflection = false; // Режим отражение
        bool Turn = false; // Режим поворот
        private List<MyPoint> pointsAfter = new List<MyPoint>();
        bool buttonBuild = false;
        int countClick = 0;

        public Form1()
        {
            InitializeComponent();
            this.pictureBoxNormal.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxNormal_Paint);
            this.pictureBoxChanged.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxChanged_Paint);
            dataGridView_p.CellBeginEdit += DataGridView_p_CellBeginEdit;
            dataGridView_p.CellEndEdit += DataGridView_p_CellEndEdit;
            InitializePictureBoxBackground(); 

        }
        private void DrawGrid(Graphics g, double x0, double y0)
        {
            // Сетка
            using (Pen streak = new Pen(Color.LightGray))
            {
                streak.DashPattern = new float[] { 10.0f, 5.0f }; // Длина штриха - 10, пробел - 5

                for (int i = -(n / 2) + 1; i < n / 2; i++)
                {
                    int countX = (int)(i * hx + x0); // Xэ = X*Hx + X0
                    int countY = (int)(y0 - i * hy); // Yэ = Y0 - Y*Hy

                    // Рисуем вертикальные линии
                    g.DrawLine(streak, countX, 0, countX, (float)(y0 * 2));

                    // Рисуем горизонтальные линии
                    g.DrawLine(streak, 0, countY, (float)(x0 * 2), countY);
                }
            }
        }
        private void DrawArrow(Graphics g, int x1, int y1, int x2, int y2)
        {
            // Рисуем линию оси
            g.DrawLine(Pens.Black, x1, y1, x2, y2);

            // Размер стрелки
            float arrowSize = 10;
            double angle = Math.Atan2(y2 - y1, x2 - x1);

            // Рисуем два отрезка стрелки
            g.DrawLine(Pens.Black, x2, y2,
                x2 - arrowSize * (float)Math.Cos(angle - Math.PI / 6),
                y2 - arrowSize * (float)Math.Sin(angle - Math.PI / 6));

            g.DrawLine(Pens.Black, x2, y2,
                x2 - arrowSize * (float)Math.Cos(angle + Math.PI / 6),
                y2 - arrowSize * (float)Math.Sin(angle + Math.PI / 6));
        }
        private void DrawAxes(Graphics g, double x0, double y0)
        {
            // Рисуем координатные оси с использованием стрелок
            int width = (int)(x0 * 2);
            int height = (int)(y0 * 2);

            // Вертикальная ось
            DrawArrow(g, (int)x0, height, (int)x0, 0);

            // Горизонтальная ось
            DrawArrow(g, 0, (int)y0, width, (int)y0);

            // Подписи осей
            Font font = new Font("Arial", 10);
            g.DrawString("Y", font, Brushes.Black, (float)x0 + 5, 5);
            g.DrawString("X", font, Brushes.Black, width - 20, (float)y0 - 20);

            // Отрисовка делений
            using (Pen seg = new Pen(Color.Black, 2))
            {
                for (int i = -(n / 2) + 1; i < n / 2; i++)
                {
                    int countX = (int)(i * hx + x0); // Xэ = X*Hx + X0
                    int countY = (int)(y0 - i * hy); // Yэ = Y0 - Y*Hy

                    // Делаем деления на осях
                    g.DrawLine(seg, countX, (float)y0 + 5, countX, (float)y0 - 5);
                    g.DrawLine(seg, (float)x0 + 5, countY, (float)x0 - 5, countY);

                    // Подписи делений
                    string label = i.ToString();
                    if (i != 0)
                    {
                        g.DrawString(label, font, Brushes.Black, countX - 10, (float)y0 + 5);
                        g.DrawString(label, font, Brushes.Black, (float)x0 + 5, countY - 10);
                    }
                }
            }

            // Отрисовка точки центра
            g.FillEllipse(Brushes.Black, (float)x0 - 3, (float)y0 - 3, 6, 6); // Точка центра
            g.DrawString("O", font, Brushes.Black, (float)x0 + 5, (float)y0 + 5); // Подпись центра
        }
        private void DrawSystems(Graphics g, Graphics h, double x0, double y0)
        {
            // Отрисовка сетки и осей на обоих PictureBox
            g.Clear(Color.White);
            DrawGrid(g, x0, y0);
            DrawAxes(g, x0, y0);

            h.Clear(Color.White);
            DrawGrid(h, x0, y0);
            DrawAxes(h, x0, y0);
        }
        private void pictureBoxNormal_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Центр координат
            x0 = pictureBoxNormal.Width / 2.0;
            y0 = pictureBoxNormal.Height / 2.0;

            // Цена деления
            hx = pictureBoxNormal.Width / (double)n;
            hy = hx;

            // Рисуем сетку и оси
            DrawGrid(g, x0, y0);
            DrawAxes(g, x0, y0);
        }
        private void pictureBoxChanged_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Центр координат
            x0 = pictureBoxChanged.Width / 2.0;
            y0 = pictureBoxChanged.Height / 2.0;

            // Цена деления
            hx = pictureBoxChanged.Width / (double)n;
            hy = hx;

            // Рисуем сетку и оси
            DrawGrid(g, x0, y0);
            DrawAxes(g, x0, y0);
        }
        private void TableStyle(DataGridView dataGridView)
        {
            dataGridView.ColumnHeadersVisible = false;
            dataGridView.RowHeadersVisible = false;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        private void IsFirstColumn(DataGridViewCellCancelEventArgs e)
        {
            // Проверяем, редактируется ли ячейка в первом столбце
            if (e.ColumnIndex == 0)
            {
                // Отменяем редактирование ячейки
                e.Cancel = true;
            }
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            // Скрываем элементы интерфейса
            label4.Visible = false;
            label5.Visible = false;
            label6.Visible = false;
            label7.Visible = false;
            dataGridView1.Visible = false;
            dataGridView2.Visible = false;

            // Таблица точки P
            label2.BackColor = Color.Transparent;
            label3.BackColor = Color.Transparent;

            dataGridView_p.ColumnCount = 2;
            dataGridView_p.Rows.Add("X");
            dataGridView_p.Rows.Add("Y");
            TableStyle(dataGridView_p);
            dataGridView_p.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_p.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView_p.SelectionMode = DataGridViewSelectionMode.CellSelect;

            // Таблица исходных точек
            dataGridView_points.ColumnCount = 1;
            dataGridView_points.Rows.Add();
            dataGridView_points.Rows.Add("X");
            dataGridView_points.Rows.Add("Y");
            TableStyle(dataGridView_points);
            dataGridView_points.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_points.SelectionMode = DataGridViewSelectionMode.CellSelect;

            // Таблица полученных точек
            dataGridView_after.ColumnCount = 1;
            dataGridView_after.Rows.Add();
            dataGridView_after.Rows.Add("X");
            dataGridView_after.Rows.Add("Y");
            TableStyle(dataGridView_after);
            dataGridView_after.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_after.SelectionMode = DataGridViewSelectionMode.CellSelect;

            // Матрицы
            dataGridView_matr.ColumnCount = 3;
            dataGridView1.ColumnCount = 3;
            dataGridView2.ColumnCount = 3;

            for (int i = 0; i < 3; i++)
            {
                dataGridView_matr.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView_matr.Rows.Add();
                dataGridView_matr.Rows[i].DefaultCellStyle.BackColor = this.BackColor;

                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].DefaultCellStyle.BackColor = this.BackColor;

                dataGridView2.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView2.Rows.Add();
                dataGridView2.Rows[i].DefaultCellStyle.BackColor = this.BackColor;
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    dataGridView_matr.Rows[i].Cells[j].Value = "-";
                    dataGridView1.Rows[i].Cells[j].Value = "-";
                    dataGridView2.Rows[i].Cells[j].Value = "-";
                }
            }

            dataGridView_matr.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.CellSelect;
        }
        private void DataGridView_p_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            IsFirstColumn(e);
        }

        private void DrawPoint(Graphics g, int x, int y, int number)
        {
            using (Pen pen = new Pen(Color.Red, 5))
            {
                g.FillEllipse(Brushes.Red, x - 3, y - 3, 6, 6);
            }

            using (Font font = new Font("Arial", 8))
            {
                g.DrawString(number.ToString(), font, Brushes.Black, x, y - 15);
            }
        }

        public class MyPoint
        {
            public int Number { get; set; } // Номер точки
            public double X { get; set; } // Координата X
            public double Y { get; set; } // Координата Y

            public MyPoint(int number, double x, double y)
            {
                Number = number;
                X = Math.Round(x, 1);
                Y = Math.Round(y, 1);
            }
        }
        private void pictureBoxNormal_MouseClick(object sender, MouseEventArgs e)
        {
            // Получаем координаты клика относительно PictureBox
            int x = e.X;
            int y = e.Y;
            float x0 = pictureBoxNormal.Width / 2.0f;
            float y0 = pictureBoxNormal.Height / 2.0f;

            // Рисуем точку
            using (Graphics g = pictureBoxNormal.CreateGraphics())
            {
                DrawPoint(g, x, y, countPoints);
            }

            // Добавляем данные в таблицу
            dataGridView_points.Columns.Add("", "");
            dataGridView_points.Rows[0].Cells[countPoints].Value = countPoints;

            // Вычисляем координаты Xд и Yд
            dataGridView_points.Rows[1].Cells[countPoints].Value = Math.Round((x - x0) / hx, 1);
            dataGridView_points.Rows[2].Cells[countPoints].Value = Math.Round((y0 - y) / hy, 1);
            dataGridView_points.Columns[countPoints].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView_points.SelectionMode = DataGridViewSelectionMode.CellSelect;

            // Создаём и добавляем точку
            MyPoint myPoint = new MyPoint(countPoints, (x - x0) / hx, (y0 - y) / hy);
            points.Add(myPoint);

            countPoints++;
        }
        private void DrawVector(Graphics g)
        {
            using (Pen pen = new Pen(Color.Red, 5))
            using (Font font = new Font("Arial", 8))
            {
                // Рисуем подпись "P" около точки
                g.DrawString("P", font, Brushes.Black, xp, yp - 15);

                // Рисуем стрелку от центра к точке
                using (Pen arc = new Pen(Color.Red, 2))
                {
                    arc.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 4, true);
                    g.DrawLine(arc, (float)x0, (float)y0, xp, yp);
                }
            }
        }
        private void DataGridView_p_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Получаем ячейку, которую редактировал пользователь
            DataGridViewCell cell = dataGridView_p.Rows[e.RowIndex].Cells[e.ColumnIndex];

            // Проверяем, не пустое ли значение ячейки
            if (cell.Value != null && !string.IsNullOrWhiteSpace(cell.Value.ToString()))
            {
                if (e.RowIndex == 0)
                {
                    // !!! Xэ = X*Hx + X0
                    p.X = (int)Convert.ToDouble(cell.EditedFormattedValue);
                    xp = (int)(p.X * hx + x0); // Преобразование к целому числу для рисования
                }
                else
                {
                    // !!! Yэ = Y0 - Y*Hy
                    p.Y = (int)Convert.ToDouble(cell.EditedFormattedValue);
                    yp = (int)(y0 - p.Y * hy);

                    if (Transfer)
                    {
                        using (Graphics g = pictureBoxNormal.CreateGraphics())
                        {
                            DrawVector(g);
                        }
                    }
                }
            }
            else
            {
                // Ячейка пустая
                MessageBox.Show("Ячейка пустая.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void ConnectDots(Graphics g, List<MyPoint> points)
        {
            int x1, y1, x2, y2;
            using (Pen pen = new Pen(Color.Blue, 2))
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if (i == points.Count - 1)
                    {
                        // Соединяем последнюю точку с первой
                        x1 = (int)(points[i].X * hx + x0);
                        y1 = (int)(y0 - points[i].Y * hy);
                        x2 = (int)(points[0].X * hx + x0);
                        y2 = (int)(y0 - points[0].Y * hy);
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                    else
                    {
                        // Соединяем текущую точку со следующей
                        x1 = (int)(points[i].X * hx + x0);
                        y1 = (int)(y0 - points[i].Y * hy);
                        x2 = (int)(points[i + 1].X * hx + x0);
                        y2 = (int)(y0 - points[i + 1].Y * hy);
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                }
            }
        }
        private void Transformation(Graphics g)
        {
            if (Transfer)
            {
                // Считываем координаты вектора OP
                int a, b;
                if (!string.IsNullOrEmpty(dataGridView_p.Rows[0].Cells[1].Value?.ToString()) &&
                    !string.IsNullOrEmpty(dataGridView_p.Rows[1].Cells[1].Value?.ToString()))
                {
                    a = Convert.ToInt32(dataGridView_p.Rows[0].Cells[1].Value);
                    b = Convert.ToInt32(dataGridView_p.Rows[1].Cells[1].Value);

                    foreach (var point in points)
                    {
                        double x_ = point.X + a;
                        double y_ = point.Y + b;

                        if (buttonBuild)
                        {
                            var column = new DataGridViewTextBoxColumn { AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells };
                            dataGridView_after.Columns.Add(column);
                        }

                        dataGridView_after.Rows[0].Cells[point.Number].Value = point.Number;
                        dataGridView_after.Rows[1].Cells[point.Number].Value = x_;
                        dataGridView_after.Rows[2].Cells[point.Number].Value = y_;

                        DrawPoint(g, (int)(x_ * hx + x0), (int)(y0 - y_ * hy), point.Number);
                        pointsAfter.Add(new MyPoint(point.Number, x_, y_));
                    }

                    // Отрисовка многоугольника
                    ConnectDots(g, pointsAfter);

                    // Заполнение базовой матрицы переноса
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (i == j)
                                dataGridView_matr.Rows[i].Cells[j].Value = 1;
                            else if (i == 2 && j == 0)
                                dataGridView_matr.Rows[i].Cells[j].Value = a;
                            else if (i == 2 && j == 1)
                                dataGridView_matr.Rows[i].Cells[j].Value = b;
                            else
                                dataGridView_matr.Rows[i].Cells[j].Value = 0;
                        }
                    }

                    // Матрицы dataGridView1 и dataGridView2 для переноса единичны
                    SetIdentityMatrix(dataGridView1);
                    SetIdentityMatrix(dataGridView2);
                }
            }

            if (Reflection)
            {
                foreach (var point in points)
                {
                    double x_ = point.X;
                    double y_ = -point.Y; // Отражение относительно оси OX

                    if (buttonBuild)
                    {
                        var column = new DataGridViewTextBoxColumn { AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells };
                        dataGridView_after.Columns.Add(column);
                    }

                    dataGridView_after.Rows[0].Cells[point.Number].Value = point.Number;
                    dataGridView_after.Rows[1].Cells[point.Number].Value = Math.Round(x_, 1);
                    dataGridView_after.Rows[2].Cells[point.Number].Value = Math.Round(y_, 1);

                    DrawPoint(g, (int)(x_ * hx + x0), (int)(y0 - y_ * hy), point.Number);
                    pointsAfter.Add(new MyPoint(point.Number, x_, y_));
                }

                ConnectDots(g, pointsAfter);

                // Обновление матрицы для отражения
                dataGridView_matr.Rows[0].Cells[0].Value = 1;
                dataGridView_matr.Rows[1].Cells[1].Value = -1;
                dataGridView_matr.Rows[2].Cells[2].Value = 1;

                SetIdentityMatrix(dataGridView1);
                SetIdentityMatrix(dataGridView2);
            }

            if (Turn)
            {
                if (!string.IsNullOrEmpty(dataGridView_p.Rows[0].Cells[1].Value?.ToString()) &&
                    !string.IsNullOrEmpty(dataGridView_p.Rows[1].Cells[1].Value?.ToString()) &&
                    !string.IsNullOrEmpty(textBoxF.Text))
                {
                    int xP = Convert.ToInt32(dataGridView_p.Rows[0].Cells[1].Value);
                    int yP = Convert.ToInt32(dataGridView_p.Rows[1].Cells[1].Value);
                    double angle = Math.PI * Convert.ToDouble(textBoxF.Text) / 180.0; // Угол в радианах

                    // Матрица базового поворота
                    double[,] matr2 = new double[3, 3];
                    matr2[0, 0] = Math.Cos(angle);
                    matr2[0, 1] = -Math.Sin(angle);
                    matr2[0, 2] = xP * (1 - Math.Cos(angle)) + yP * Math.Sin(angle);

                    matr2[1, 0] = Math.Sin(angle);
                    matr2[1, 1] = Math.Cos(angle);
                    matr2[1, 2] = yP * (1 - Math.Cos(angle)) - xP * Math.Sin(angle);

                    matr2[2, 0] = 0;
                    matr2[2, 1] = 0;
                    matr2[2, 2] = 1;

                    // dataGridView1: Переход в точку P
                    SetTranslationMatrix(dataGridView1, -xP, -yP);
                    // dataGridView2: Обратный переход
                    SetTranslationMatrix(dataGridView2, xP, yP);

                    foreach (var point in points)
                    {
                        double x_ = point.X * matr2[0, 0] + point.Y * matr2[0, 1] + matr2[0, 2];
                        double y_ = point.X * matr2[1, 0] + point.Y * matr2[1, 1] + matr2[1, 2];

                        if (buttonBuild)
                        {
                            var column = new DataGridViewTextBoxColumn { AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells };
                            dataGridView_after.Columns.Add(column);
                        }

                        dataGridView_after.Rows[0].Cells[point.Number].Value = point.Number;
                        dataGridView_after.Rows[1].Cells[point.Number].Value = Math.Round(x_, 1);
                        dataGridView_after.Rows[2].Cells[point.Number].Value = Math.Round(y_, 1);

                        DrawPoint(g, (int)(x_ * hx + x0), (int)(y0 - y_ * hy), point.Number);
                        pointsAfter.Add(new MyPoint(point.Number, x_, y_));
                    }

                    ConnectDots(g, pointsAfter);

                    UpdateMatrixTable(dataGridView_matr, matr2);
                }
            }

            if (Scaling)
            {
                if (!string.IsNullOrEmpty(dataGridView_p.Rows[0].Cells[1].Value?.ToString()) &&
                    !string.IsNullOrEmpty(dataGridView_p.Rows[1].Cells[1].Value?.ToString()) &&
                    !string.IsNullOrEmpty(textBoxK.Text))
                {
                    int xP = Convert.ToInt32(dataGridView_p.Rows[0].Cells[1].Value);
                    int yP = Convert.ToInt32(dataGridView_p.Rows[1].Cells[1].Value);
                    double k = Convert.ToDouble(textBoxK.Text);

                    // dataGridView_matr: Базовое масштабирование
                    dataGridView_matr.Rows[0].Cells[0].Value = k;
                    dataGridView_matr.Rows[0].Cells[1].Value = 0;
                    dataGridView_matr.Rows[0].Cells[2].Value = xP * (1 - k);

                    dataGridView_matr.Rows[1].Cells[0].Value = 0;
                    dataGridView_matr.Rows[1].Cells[1].Value = k;
                    dataGridView_matr.Rows[1].Cells[2].Value = yP * (1 - k);

                    dataGridView_matr.Rows[2].Cells[0].Value = 0;
                    dataGridView_matr.Rows[2].Cells[1].Value = 0;
                    dataGridView_matr.Rows[2].Cells[2].Value = 1;

                    // dataGridView1: Переход в P
                    SetTranslationMatrix(dataGridView1, -xP, -yP);

                    // dataGridView2: Обратный переход
                    SetTranslationMatrix(dataGridView2, xP, yP);

                    foreach (var point in points)
                    {
                        double x_ = xP + (point.X - xP) * k;
                        double y_ = yP + (point.Y - yP) * k;

                        if (buttonBuild)
                        {
                            var column = new DataGridViewTextBoxColumn { AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells };
                            dataGridView_after.Columns.Add(column);
                        }

                        dataGridView_after.Rows[0].Cells[point.Number].Value = point.Number;
                        dataGridView_after.Rows[1].Cells[point.Number].Value = Math.Round(x_, 1);
                        dataGridView_after.Rows[2].Cells[point.Number].Value = Math.Round(y_, 1);

                        DrawPoint(g, (int)(x_ * hx + x0), (int)(y0 - y_ * hy), point.Number);
                        pointsAfter.Add(new MyPoint(point.Number, x_, y_));
                    }

                    ConnectDots(g, pointsAfter);
                }
            }
        }

        private void SetTranslationMatrix(DataGridView dgv, int dx, int dy)
        {
            dgv.Rows[0].Cells[0].Value = 1;
            dgv.Rows[0].Cells[1].Value = 0;
            dgv.Rows[0].Cells[2].Value = dx;

            dgv.Rows[1].Cells[0].Value = 0;
            dgv.Rows[1].Cells[1].Value = 1;
            dgv.Rows[1].Cells[2].Value = dy;

            dgv.Rows[2].Cells[0].Value = 0;
            dgv.Rows[2].Cells[1].Value = 0;
            dgv.Rows[2].Cells[2].Value = 1;
        }

        private void SetIdentityMatrix(DataGridView dgv)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    dgv.Rows[i].Cells[j].Value = i == j ? 1 : 0;
                }
            }
        }

        private void UpdateMatrixTable(DataGridView dataGridView, double[,] matrix)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    dataGridView.Rows[i].Cells[j].Value = Math.Round(matrix[i, j], 2);
                }
            }
        }

        private void button_build_Click(object sender, EventArgs e)
        {
            buttonBuild = true;

            // Создаем объект Graphics для первого PictureBox
            using (Graphics h = pictureBoxNormal.CreateGraphics())
            {
                // Отрисовка многоугольника
                ConnectDots(h, points);
            }

            // Создаем объект Graphics для второго PictureBox
            using (Graphics g = pictureBoxChanged.CreateGraphics())
            {
                // Выполняем преобразования
                Transformation(g);
            }
        }
        private void button_grid_Click(object sender, EventArgs e)
        {
            buttonBuild = false;

            // Получение объектов Graphics для обоих PictureBox
            using (Graphics g = pictureBoxNormal.CreateGraphics())
            using (Graphics h = pictureBoxChanged.CreateGraphics())
            {
                // Центр координат для обоих PictureBox
                x0 = pictureBoxNormal.Width / 2.0;
                y0 = pictureBoxNormal.Height / 2.0;

                hx = pictureBoxNormal.Width / (double)n; // Цена деления
                hy = hx;

                if (countClick % 2 == 0)
                {
                    // Очистка обоих PictureBox
                    g.Clear(Color.White);
                    h.Clear(Color.White);

                    // Отрисовка осей без сетки
                    DrawAxes(g, x0, y0);
                    foreach (var point in points)
                    {
                        DrawPoint(g, (int)(point.X * hx + x0), (int)(y0 - point.Y * hy), point.Number);
                    }
                    ConnectDots(g, points);

                    // Для второго PictureBox
                    DrawAxes(h, x0, y0);
                    Transformation(h);
                }
                else
                {
                    // Отрисовка сетки и осей для первого PictureBox
                    DrawGrid(g, x0, y0);
                    DrawAxes(g, x0, y0);
                    foreach (var point in points)
                    {
                        DrawPoint(g, (int)(point.X * hx + x0), (int)(y0 - point.Y * hy), point.Number);
                    }
                    ConnectDots(g, points);

                    // Отрисовка сетки, осей и преобразованных точек для второго PictureBox
                    DrawGrid(h, x0, y0);
                    DrawAxes(h, x0, y0);
                    Transformation(h);
                }
            }

            // Увеличиваем счетчик кликов
            countClick++;
        }
        private void InitializePictureBoxBackground()
        {
            // Установить белый цвет фона для PictureBox
            pictureBoxNormal.BackColor = Color.White;
            pictureBoxChanged.BackColor = Color.White;
        }
        private void InitialState()
        {
            // Очищаем списки точек
            points.Clear();
            pointsAfter.Clear();
            countPoints = 1;

            // Очищаем матрицы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    dataGridView_matr.Rows[i].Cells[j].Value = "-";
                    dataGridView1.Rows[i].Cells[j].Value = "-";
                    dataGridView2.Rows[i].Cells[j].Value = "-";
                }
            }

            // Очищаем таблицы
            dataGridView_points.Rows.Clear();
            dataGridView_after.Rows.Clear();

            // Инициализируем таблицу исходных точек
            dataGridView_points.ColumnCount = 1;
            dataGridView_points.Rows.Add();
            dataGridView_points.Rows.Add("X");
            dataGridView_points.Rows.Add("Y");
            TableStyle(dataGridView_points);

            // Инициализируем таблицу полученных точек
            dataGridView_after.ColumnCount = 1;
            dataGridView_after.Rows.Add();
            dataGridView_after.Rows.Add("X");
            dataGridView_after.Rows.Add("Y");
            TableStyle(dataGridView_after);

            // Получаем объекты Graphics для обоих PictureBox
            using (Graphics g = pictureBoxNormal.CreateGraphics())
            using (Graphics h = pictureBoxChanged.CreateGraphics())
            {
                DrawSystems(g, h, x0, y0);
            }
        }
        private void отражениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Turn || Scaling)
            {
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
                label7.Visible = false;
                dataGridView1.Visible = false;
                dataGridView2.Visible = false;
            }

            labelF.Visible = false;
            labelK.Visible = false;
            labelP.Visible = true;
            textBoxF.Visible = false;
            textBoxK.Visible = false;
            dataGridView_p.Visible = false;
            labelP.Text = "Относительно оси ОХ";

            Transfer = false;  
            Scaling = false;   
            Reflection = true;
            Turn = false;

            InitialState();
        }

        private void переносToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Turn || Scaling)
            {
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
                label7.Visible = false;
                dataGridView1.Visible = false;
                dataGridView2.Visible = false;
            }

            labelF.Visible = false;
            labelK.Visible = false;
            labelP.Visible = true;
            textBoxF.Visible = false;
            textBoxK.Visible = false;
            dataGridView_p.Visible = true;
            labelP.Text = "Точка P:";

            Transfer = true;   
            Scaling = false;  
            Reflection = false;
            Turn = false;

            InitialState();
        }

        private void поворотToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label4.Visible = true;
            label5.Visible = true;
            label6.Visible = true;
            label7.Visible = true;
            dataGridView1.Visible = true;
            dataGridView2.Visible = true;

            labelF.Visible = true;
            labelK.Visible = false;
            labelP.Visible = true;
            textBoxF.Visible = true;
            textBoxK.Visible = false;
            dataGridView_p.Visible = true;
            labelP.Text = "Точка \nплоскости:";

            Transfer = false;  // Режим перенос
            Scaling = false;   // Режим масштабирование
            Reflection = false;// Режим отражение
            Turn = true;

            InitialState();
        }

        private void масштабированиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Transfer  || Reflection)
            {
                label4.Visible = true;
                label5.Visible = true;
                label6.Visible = true;
                label7.Visible = true;
                dataGridView1.Visible = true;
                dataGridView2.Visible = true;
            }

            labelF.Visible = false;
            labelK.Visible = true;
            labelP.Visible = true;
            labelP.Text = "Точка P:";
            textBoxF.Visible = false;
            textBoxK.Visible = true;
            dataGridView_p.Visible = true;

            Transfer = false;  // Режим перенос
            Scaling = true;    // Режим масштабирование
            Reflection = false;// Режим отражение
            Turn = false;

            InitialState();
        }

    }
}
