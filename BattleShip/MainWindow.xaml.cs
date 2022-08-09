using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BattleShip
{
    public partial class MainWindow : Window
    {
        private Game game;

        public MainWindow()
        {
            InitializeComponent();
            FieldGridInit(LeftFieldGrid, 10, Brushes.White);
            FieldGridInit(RightFieldGrid, 10, Brushes.White);
        }

        //Инициализация поля
        private void FieldGridInit(Grid grid, int size, Brush brush)
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            //Добавление колонок и строк
            for (int i = 0; i < size + 1; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
            }

            //Добавление обозначений
            for (int i = 0; i < size; i++)
            {
                grid.Children.Add(CoordinateSymbol(((i + 1)).ToString(), i + 1, 0));
                grid.Children.Add(CoordinateSymbol(((char)('A' + i)).ToString(), 0, i + 1));
            }

            //Добавление ячеек
            for (int i = 1; i <= size; i++)
            {
                for (int j = 1; j <= size; j++)
                {
                    Rectangle rect = new Rectangle
                    {
                        Fill = brush
                    };

                    UIElement uIElement = SetInBorder(rect);

                    Grid.SetRow(rect, i);
                    Grid.SetColumn(rect, j);
                    Grid.SetRow(uIElement, i);
                    Grid.SetColumn(uIElement, j);

                    grid.Children.Add(uIElement);
                }
            }
        }

        //Текст на поле
        private UIElement CoordinateSymbol(string symbol, int row, int column)
        {
            //Новый TextBox для отображения текста
            TextBlock textBlock1 = new TextBlock
            {
                Text = symbol,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 18,
                FontFamily = new FontFamily("Segoe Print")
            };

            //Обворачивание
            UIElement border = SetInBorder(textBlock1);

            //Установление координат
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);

            return border;
        }

        //Обворачивание в Border
        private Border SetInBorder(UIElement uIElement)
        {
            return new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Child = uIElement
            }; ;
        }

        //Обновление поля
        private void RefreshFieldGrid(Grid grid, Field field)
        {
            for (int i = 0; i < field.SIZE; i++)
            {
                for (int j = 0; j < field.SIZE; j++)
                {
                    if (field.Squares[i, j] == Square.Ship)
                    {
                        foreach (var item in grid.Children)
                        {
                            if (item is Border && Grid.GetRow((UIElement)item) == j + 1 && Grid.GetColumn((UIElement)item) == i + 1)
                            {
                                if ((item as Border).Child is Rectangle)
                                    ((item as Border).Child as Rectangle).Fill = Brushes.Green;
                            }
                        }
                    }
                }
            }
        }

        //Выход
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Правила
        private void InfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("«Морской бой» — игра для двух участников, в которой игроки по очереди стреляют по координатам " +
                "на неизвестной им карте соперника. Если у соперника по этим координатам имеется корабль (координаты заняты)," +
                " то корабль или его часть «топится», а попавший получает право сделать ещё один ход. Цель игрока — первым " +
                "потопить все корабли противника.", "Описание игры");
        }

        //Новая игра
        private void NewGameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            NewGameWindow newGame = new NewGameWindow
            {
                Owner = this
            };
            newGame.ShowDialog();
            if (newGame.game != null)
            {
                game = newGame.game;
                FieldGridInit(LeftFieldGrid, 10, Brushes.White);
                RefreshFieldGrid(LeftFieldGrid, game.PlayerField);
                FieldGridInit(RightFieldGrid, 10, Brushes.White);
                InfoTextBlock.Text = "Новая игра!!!";
            }
        }

        //Сохранить
        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (game != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                SaveFileDialog sfd = new SaveFileDialog { Filter = "Файлы игры|*.sav|Все файлы|*.*" };

                if (sfd.ShowDialog() == true)
                {
                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.OpenOrCreate))
                    {
                        bf.Serialize(fs, game);
                    }
                }
            }
            else
            {
                MessageBox.Show("Сохранять нечего!!!","Ошибка");
            }
        }

        //Загрузить
        private void LoadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BinaryFormatter bf = new BinaryFormatter();
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Файлы игры|*.sav|Все файлы|*.*" };

            if (ofd.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(ofd.FileName, FileMode.OpenOrCreate))
                {
                    game = (Game)bf.Deserialize(fs);
                }
                FieldGridInit(LeftFieldGrid, 10, Brushes.White);
                RefreshFieldGrid(LeftFieldGrid, game.PlayerField);
                FieldGridInit(RightFieldGrid, 10, Brushes.White);

                SetLoadColor(game.CompShotPlaces,LeftFieldGrid, game.PlayerField);
                SetLoadColor(game.PlayerShotPlaces,RightFieldGrid, game.CompField);

                InfoTextBlock.Text = "Продолжение предыдущей";
            }
        }

        //Установка цветов ячеек после загрузки из файла
        private void SetLoadColor(List<Tuple<int, int>> shotPlaces, Grid fieldGrid, Field field)
        {
            for (int i = 0; i < shotPlaces.Count; i++)
            {
                if(field.Squares[shotPlaces[i].Item1, shotPlaces[i].Item2] == Square.Empty)
                {
                    SetSquareColor(shotPlaces[i].Item1 + 1, shotPlaces[i].Item2 + 1, Brushes.CadetBlue,fieldGrid);
                }
                else if (field.Squares[shotPlaces[i].Item1, shotPlaces[i].Item2] == Square.BurningShip)
                {
                    if (field.IsBoom(shotPlaces[i].Item1, shotPlaces[i].Item2))
                    {
                        SetX(shotPlaces[i].Item1 + 1, shotPlaces[i].Item2 + 1, Brushes.Red, fieldGrid);
                    }
                    else
                    {
                        SetSquareColor(shotPlaces[i].Item1 + 1, shotPlaces[i].Item2 + 1, Brushes.Orange, fieldGrid);
                    }
                }
            }
        }

        //О разработчике
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Приложение: Морской бой." +
                            "\nРазработал: Чамзы К.Э." +
                            "\nФакультет: Бизнес-Информатика" +
                            "\nГруппа: БИСТ-211" +
                            "\nНовосибирск 2019-2020", "Информация о разработчике");
        }

        //История
        private void HistoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (game != null)
            {
                HistoryWindow hs = new HistoryWindow(game);
                hs.Owner = this;
                hs.ShowDialog();
            }
        }

        //При входе курсора мыши в правое поле
        private void RightFieldGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/NO.ani")).Stream);

        }

        //При выходе курсора мыши в правое поле
        private void RightFieldGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void RightFieldGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (game != null)
            {
                int x = Grid.GetColumn((UIElement)e.OriginalSource);
                int y = Grid.GetRow((UIElement)e.OriginalSource);

                Field.ShotResult? shotResult = game.Shot(x - 1, y - 1);

                if (shotResult != null)
                {
                    switch (shotResult)
                    {
                        case Field.ShotResult.Boom:
                            SetBoomColor(x - 1, y - 1, RightFieldGrid, game.CompField);
                            break;
                        case Field.ShotResult.Hit:
                            SetSquareColor(x, y, Brushes.Orange, RightFieldGrid);
                            break;
                        case Field.ShotResult.Miss:
                            SetSquareColor(x, y, Brushes.CadetBlue, RightFieldGrid);
                            break;
                    }
                        while (!game.CanPlayerShot && !game.IsWin)
                        {
                            shotResult = game.AutoShot();

                            if (shotResult != null)
                            {
                                switch (shotResult)
                                {
                                    case Field.ShotResult.Boom:
                                        SetBoomColor(game.CompShotPlaces[game.CompShotPlaces.Count - 1].Item1, game.CompShotPlaces[game.CompShotPlaces.Count - 1].Item2, LeftFieldGrid, game.PlayerField);
                                        break;
                                    case Field.ShotResult.Hit:
                                        SetSquareColor(game.CompShotPlaces[game.CompShotPlaces.Count - 1].Item1 + 1, game.CompShotPlaces[game.CompShotPlaces.Count - 1].Item2 + 1, Brushes.Orange, LeftFieldGrid);
                                        break;
                                    case Field.ShotResult.Miss:
                                        SetSquareColor(game.CompShotPlaces[game.CompShotPlaces.Count - 1].Item1 + 1, game.CompShotPlaces[game.CompShotPlaces.Count - 1].Item2 + 1, Brushes.CadetBlue, LeftFieldGrid);
                                        break;
                                }
                            }
                        }
                    if(game.IsWin)
                    {
                        if (game.PlayerWin)
                        {
                            MessageBox.Show("Победил игрок", "Результат игры");
                        }
                        else
                        {
                            MessageBox.Show("Победил компьютер", "Результат игры");
                        }

                        InfoTextBlock.Text = "Начните новую игру или загрузите старую";
                    }
                }
            }
        }

        //Установка цвета для потопленных кораблей
        private void SetBoomColor(int x, int y, Grid fieldGrid, Field field)
        {

            int signX = field.GetSignX(x, y);
            int signY = field.GetSignY(x, y);
            int minX = field.GetMinX(x, y);
            int minY = field.GetMinY(x, y);

            if (signX == 1 && signY == 1)
            {
                if (minX + signX < 10 && field.Squares[minX + signX, minY] == Square.Empty) signX = 0;
                else signY = 0;
            }

            while (minX >= 0 && minX < 10 && minY >= 0 && minY < 10 && field.Squares[minX, minY] != Square.Empty)
            {
                SetX(minX + 1, minY + 1, Brushes.Red, fieldGrid);
                minX += signX;
                minY += signY;
            }
        }

        private void SetX(int x, int y, Brush brush, Grid fieldGrid)
        {
            foreach (var item in fieldGrid.Children)
            {
                if (item is Border && Grid.GetRow((UIElement)item) == y && Grid.GetColumn((UIElement)item) == x)
                {
                    if ((item as Border).Child is Rectangle)
                    {
                        TextBlock temp = new TextBlock
                        {
                            Text = "X",
                            TextAlignment = TextAlignment.Center,
                            Background = brush
                        };
                        (item as Border).Child = temp;
                        Grid.SetColumn(temp, x);
                        Grid.SetRow(temp, y);
                    }
                }
            }
        }

        //Установка цвета для 1 клетки
        private void SetSquareColor(int x, int y, Brush brush, Grid fieldGrid)
        {
            foreach (var item in fieldGrid.Children)
            {
                if (item is Border && Grid.GetRow((UIElement)item) == y && Grid.GetColumn((UIElement)item) == x)
                {
                    if ((item as Border).Child is Rectangle)
                    {
                        ((item as Border).Child as Rectangle).Fill = brush;
                    }
                }
            }
        }

        //Горячие клавиши управления
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    NewGameMenuItem_Click(this, new RoutedEventArgs());
                    break;
                case Key.B:
                    SaveMenuItem_Click(this, new RoutedEventArgs());
                    break;
                case Key.C:
                    LoadMenuItem_Click(this, new RoutedEventArgs());
                    break;
                case Key.D:
                    HistoryMenuItem_Click(this, new RoutedEventArgs());
                    break;
                case Key.E:
                    InfoMenuItem_Click(this,new RoutedEventArgs());
                    break;
                case Key.F:
                    AboutMenuItem_Click(this, new RoutedEventArgs());
                    break;
                case Key.G:
                    Close();
                    break;
            }
        }
    }
}
