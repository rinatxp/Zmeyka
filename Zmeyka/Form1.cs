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

namespace Zmeyka
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label1.Text = "Яблок: " + "0";
            label2.Text = "00:00:00";
        }

        static byte poleX = 35, poleY = 25; //Размеры поля
        float prirost = 0.2F; //Коэффициент плавности движения
        int rost = 10;
        int every10coord;
        int pixelX, pixelY; //Размеры "пикселя"
        float headX, headY; //Координаты головы
        byte moveWay; //Направление движения
        byte appleNumber; //Кол-во яблок
        byte isGamePlay = 0; //Идет ли игра
        byte zaderzhkaKlavish = 0; //Создает задержку обработчика клавиш
        byte[,] Maze = new byte[poleX, poleY]; //Поле

        TimeSpan time;  //Значение времени
        Random rand1 = new Random((int)DateTime.Now.Ticks);

        List<Coords> snakeCoord = new List<Coords>(); //Координты змеи
        List<Coords> snake10CoordList = new List<Coords>();
        Coords appleCoord; //Координаты яблока

        private void MenuNewGame(object sender, EventArgs e)
        {
            snakeCoord.Clear(); //Очищаем List змеи
            snake10CoordList.Clear();
            appleNumber = 0; //Обнуляем кол-во яблок
            time = new TimeSpan(0, 0, 0); //Обнуляем время
            label1.Text = "Яблок: " + appleNumber.ToString();
            label2.Text = "00:00:00";
            every10coord = 0;

            Graphics G = pictureBox1.CreateGraphics();
            pixelX = pictureBox1.Width / poleX; //Ширина пикселей
            pixelY = pictureBox1.Height / poleY; //Высота пикселей

            imageList1.ImageSize = new Size(pixelX, pixelY);  //Подключаем текстуры
            imageList1.Images.Add(Image.FromFile("pictures\\area.jpg"));
            imageList1.Images.Add(Image.FromFile("pictures\\snake.jpg"));
            imageList1.Images.Add(Image.FromFile("pictures\\apple.jpg"));

            for (int i = 0; i < poleX; i++)
            {
                for (int j = 0; j < poleY; j++)
                    G.DrawImage(imageList1.Images[0], i * pixelX, j * pixelY);
            } //Заполняем поле

            for (float a = 10; a < 14; a += prirost)
            {
                if (a > 11.2)
                    G.DrawImage(imageList1.Images[1], 10 * pixelX, a * pixelY);
                snakeCoord.Add(new Coords(10 * pixelX, (poleY - a) * pixelY));
            } //Создаем змею

            snake10CoordList.Add(new Coords(10 * pixelX, (13) * pixelY));
            snake10CoordList.Add(new Coords(10 * pixelX, (12) * pixelY));
            snake10CoordList.Add(new Coords(10 * pixelX, (11) * pixelY));
            snake10CoordList.Add(new Coords(10 * pixelX, (10) * pixelY));

            appleCoord = CreateApple(); //Создаем яблоко
            G.DrawImage(imageList1.Images[2], appleCoord.X * pixelX, appleCoord.Y * pixelY);

            headX = 10.0F;
            headY = 13.0F; //Начальные координаты головы
            moveWay = 2; //Начальное направление
            timer1.Interval = 6;
            timer2.Interval = 1000;
            timer1.Start(); // Запускаем таймер
            timer2.Start();

            isGamePlay = 1; //Игра запущена
        }

        private void MenuPause(object sender, EventArgs e)
        {
            int t = 0;
            if (isGamePlay == 1) //Если игра была запущена
                t = 1;
            timer1.Stop(); timer2.Stop();
            string msg = "Змейка v1.0\nАвтор: Хайретдинов Р.И.";
            MessageBox.Show(msg, "О программе",  //Выводим сообщение
            MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (t == 1)
            {
                timer1.Start();
                timer2.Start();
            }
        }

        private void Timer1(object sender, EventArgs e)
        {
            Graphics G = pictureBox1.CreateGraphics();

            switch (moveWay)
            {
                case 0: //Вверх
                    headY = (float)(headY - prirost);
                    if (headY < 0)
                        headY = poleY;
                    break;
                case 1: //Вправо
                    headX = (float)(headX + prirost);
                    if (headX >= poleX)
                        headX = 0;
                    break;
                case 2: //Вниз
                    headY = (float)(headY + prirost);
                    if (headY >= poleY)
                        headY = 0;
                    break;
                case 3: //Влево
                    headX = (float)(headX - prirost);
                    if (headX < 0)
                        headX = poleX;
                    break;
            } //Вычисляем следующую координату змеи

            Coords c = new Coords(headX * pixelX, headY * pixelY); //Новые координаты
            G.DrawImage(imageList1.Images[0], snakeCoord[snakeCoord.Count - 1].X, snakeCoord[snakeCoord.Count - 1].Y);
            snakeCoord.Insert(0, c); // Увеличиваем змею на один сегмент  

            Stolknovenie(); //Функция обработки столкновений

            if (every10coord == 10)
            {
                snake10CoordList.Insert(0, c);
                every10coord = 0;
                if (rost == 0)
                    snake10CoordList.RemoveAt(snake10CoordList.Count - 1);
            } //Запоминаем каждую десятую координату змеи
            every10coord++;

            if (isGamePlay == 1) //Если игра запущена
            {
                G.DrawImage(imageList1.Images[1], snakeCoord[0].X, snakeCoord[0].Y); //Рисуем новый сегмент

                if (rost > 0)
                {
                    G.DrawImage(imageList1.Images[0], snakeCoord[snakeCoord.Count - 1].X, snakeCoord[snakeCoord.Count - 1].Y);
                    rost--;
                }
            }
            zaderzhkaKlavish = 0; //Можно считать следующую нажатую клавишу
        }

        private void Timer2(object sender, EventArgs e)
        {
            Graphics G = pictureBox1.CreateGraphics();
            time += new TimeSpan(0, 0, 1);  //Прибавляем секунду
            label2.Text = time.ToString();  //Выводим
        }

        private void ObrabotkaKlavish(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)  //Нажатие стрелки
            {
                case Keys.Up:  //Вверх
                    {
                        if (moveWay != 2 && zaderzhkaKlavish == 0)
                            moveWay = 0;
                        zaderzhkaKlavish = 1; //Больше нельзя считывать нажатие до того как не пройдет тик таймера змеи
                        break;
                    }
                case Keys.Right:  //Вправо
                    {
                        if (moveWay != 3 && zaderzhkaKlavish == 0)
                            moveWay = 1;
                        zaderzhkaKlavish = 1;
                        break;
                    }
                case Keys.Down:  //Вниз
                    {
                        if (moveWay != 0 && zaderzhkaKlavish == 0)
                            moveWay = 2;
                        zaderzhkaKlavish = 1;
                        break;
                    }
                case Keys.Left:  //Влево
                    {
                        if (moveWay != 1 && zaderzhkaKlavish == 0)
                            moveWay = 3;
                        zaderzhkaKlavish = 1;
                        break;
                    }
            }
        }

        Coords CreateApple()
        {
        g:
            int N1 = (rand1.Next(poleX - 4) + 3);
            int M1 = (rand1.Next(poleY - 4) + 3);
            Coords newAppleCoor = new Coords(N1, M1);
            for (int b = 0; b < snake10CoordList.Count; b++)
                if (Math.Abs(newAppleCoor.X * pixelX - snake10CoordList[b].X) <= 2 * pixelX && Math.Abs(newAppleCoor.Y * pixelY - snake10CoordList[b].Y) <= 2 * pixelY)
                    goto g;
            return newAppleCoor;
        } //Создание координат яблок

        void Stolknovenie()
        {
            int b = 0;
            float c = 0;
            float vis1 = 0;
            float vis2 = 0;

            switch (moveWay)
            {
                case 0: //Вверх
                    c = snakeCoord[0].X;
                    b = pixelX;
                    vis1 = 0;
                    vis2 = pixelY;
                    break;
                case 1: //Вправо
                    b = pixelY;
                    c = snakeCoord[0].Y;
                    vis1 = pixelX;
                    vis2 = 0;
                    break;
                case 2: //Вниз
                    c = snakeCoord[0].X;
                    b = pixelX;
                    vis1 = pixelY;
                    vis2 = 0;
                    break;
                case 3: //Влево
                    b = pixelY;
                    c = snakeCoord[0].Y;
                    vis1 = 0;
                    vis2 = pixelX;
                    break;
            }

            for (float a = c; a <= c + b; a += b)
            {
                if (moveWay == 0 || moveWay == 2)
                {
                    if (Math.Round(snakeCoord[0].Y + vis1) == Math.Round(appleCoord.Y * pixelY + vis2))
                        if (a >= appleCoord.X * pixelX && a <= appleCoord.X * pixelX + pixelX)
                        {
                            rost += 100 / (int)(prirost * 100);
                            Yabl();
                        }
                } //Если вверх или вниз
                else
                {
                    if (Math.Round(snakeCoord[0].X + vis1) == Math.Round(appleCoord.X * pixelX + vis2))
                        if (a >= appleCoord.Y * pixelY && a <= appleCoord.Y * pixelY + pixelY)
                        {
                            rost += 10 / (int)(prirost * 10);
                            Yabl();
                        }
                } //Если вправо или влево
            } //Съедание яблока

            for (int k = 0; k < snake10CoordList.Count - 1; k++)
                for (float a = c; a <= c + b; a += b)
                {
                    if (moveWay == 0 || moveWay == 2)
                    {
                        if (Math.Round(snakeCoord[0].Y + vis1) == Math.Round(snake10CoordList[k].Y + vis2))
                            if (a >= snake10CoordList[k].X && a <= snake10CoordList[k].X + pixelX)
                                GameOver();
                    } //Если вверх или вниз
                    else
                    {
                        if (Math.Round(snakeCoord[0].X + vis1) == Math.Round(snake10CoordList[k].X + vis2))
                            if (a >= snake10CoordList[k].Y && a <= snake10CoordList[k].Y + pixelY)
                                GameOver();
                    } //Если вправо или влево
                } //Столкновение с телом

            if (rost == 0)
            {
                snakeCoord.RemoveAt(snakeCoord.Count - 1);
            } //Если яблоко не было съедено
        } //Взаимодействие при столкновении с различными объектами

        void GameOver()
        {
            timer1.Stop();  //Останавливаем игру
            timer2.Stop();  //Останавливаем время
            isGamePlay = 0;  //Игра остановлена
            string msg = "Собрано яблок: ";
            msg += appleNumber;
            msg += "\nЗа время: ";
            msg += label2.Text;
            MessageBox.Show(msg, "Game over",  //Выводим сообщение (Game over)
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }  //Game over

        void Yabl()
        {
            Graphics G = pictureBox1.CreateGraphics();
            G.DrawImage(imageList1.Images[0], appleCoord.X * pixelX, appleCoord.Y * pixelY); //Рисуем
            appleCoord = CreateApple(); //Создаем новое
            G.DrawImage(imageList1.Images[2], appleCoord.X * pixelX, appleCoord.Y * pixelY); //Рисуем яблоко
            appleNumber++;
            label1.Text = "Яблок: " + appleNumber.ToString();
        } //Рисуем яблоко

        public struct Coords
        {
            public float X;
            public float Y;
            public Coords(float x, float y)
            {
                X = x;
                Y = y;
            }
        }
    }
}

