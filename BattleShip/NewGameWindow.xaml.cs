using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BattleShip
{
    public partial class NewGameWindow : Window
    {
        internal Game game { get; set; }

        public NewGameWindow()
        {
            InitializeComponent();
            choosenSquares = new List<Tuple<int, int>>();
            game = new Game();
            FieldGridInit(FieldGrid, 10, Brushes.CadetBlue);
            InfoTextBlock.Text = string.Format("Щелкните по полю чтобы добавить корабль. Осталось: 1 \n" +
                    "Серый цвет: корабль установлен. Зеленый: в процессе установки.");
        }

        //Инициализация поля
        private void FieldGridInit(Grid grid, int size, Brush brush)
        {
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
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

        //Нажатие на кнопку ok
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Отмена
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            game = null;
            Close();
        }

        private List<Tuple<int, int>> choosenSquares; //Выбранные координаты

        //Установка цвета для 1 клетки
        private void SetSquareColor(int x, int y, Brush brush)
        {
            foreach (var item in FieldGrid.Children)
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

        //Клик по полю
        private void FieldGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int x = Grid.GetColumn((UIElement)e.OriginalSource);
            int y = Grid.GetRow((UIElement)e.OriginalSource);
            if (choosenSquares.Count != 0 && choosenSquares[0] == new Tuple<int, int>(0, 0)) choosenSquares.Clear();
            choosenSquares.Sort();
            if (choosenSquares.Count == 0)
            {
                if (game.PlayerField.OneCount != 0 && game.PlayerField.TryAddShip(1, x - 1, y - 1, 1, 0))
                {
                    SetSquareColor(x, y, Brushes.Silver);
                }
                else if (game.PlayerField.OneCount == 0 && !game.PlayerField.BannedSquares.Contains(new Tuple<int, int>(x - 1, y - 1)))
                {
                    SetSquareColor(x, y, Brushes.Green);
                    choosenSquares.Add(new Tuple<int, int>(x, y));
                }
            }
            else if (choosenSquares.Contains(new Tuple<int, int>(x, y)) && (IsBeginSquare(x, y) || IsEndSquare(x, y)))
            {
                SetSquareColor(x, y, Brushes.CadetBlue);
                choosenSquares.Remove(new Tuple<int, int>(x, y));
            }
            else if (CheckRightSquare(x, y) && !game.PlayerField.BannedSquares.Contains(new Tuple<int, int>(x - 1, y - 1)))
            {
                choosenSquares.Add(new Tuple<int, int>(x, y));
                choosenSquares.Sort();
                SetSquareColor(x, y, Brushes.Green);
                int count = 0;
                if (game.PlayerField.TwoCount != 0)
                {
                    count = 2;
                }
                else if (game.PlayerField.ThreeCount != 0)
                {
                    count = 3;
                }
                else if (game.PlayerField.FourCount != 0)
                {
                    count = 4;
                }
                if (count == choosenSquares.Count)
                {
                    if (game.PlayerField.TryAddShip(count, choosenSquares[0].Item1 - 1, choosenSquares[0].Item2 - 1, GetSignX(), GetSignY()))
                    {
                        foreach (var item in choosenSquares)
                        {
                            SetSquareColor(item.Item1, item.Item2, Brushes.Silver);
                        }
                        choosenSquares.Clear();
                    }
                }
            }
            RefreshTextBlocks();
        }

        //Проверка на корректность координаты
        private bool CheckRightSquare(int x, int y)
        {
            if (!choosenSquares.Contains(new Tuple<int, int>(x, y)))
            {
                if (choosenSquares.Count == 1)
                {
                    if (Math.Abs(choosenSquares[0].Item1 - x) == 0 && Math.Abs(choosenSquares[0].Item2 - y) == 1)
                    {
                        return true;
                    }
                    if (Math.Abs(choosenSquares[0].Item1 - x) == 1 && Math.Abs(choosenSquares[0].Item2 - y) == 0)
                    {
                        return true;
                    }
                }

                else
                {
                    bool result = false;
                    if (choosenSquares[0].Item1 == choosenSquares[choosenSquares.Count - 1].Item1)
                    {
                        result = (Math.Abs(choosenSquares[0].Item1 - x) == 0 && Math.Abs(choosenSquares[0].Item2 - y) == 1) ||
                            (Math.Abs(choosenSquares[choosenSquares.Count - 1].Item1 - x) == 0 && Math.Abs(choosenSquares[choosenSquares.Count - 1].Item2 - y) == 1);
                    }
                    if (choosenSquares[0].Item2 == choosenSquares[choosenSquares.Count - 1].Item2)
                    {
                        result = (Math.Abs(choosenSquares[0].Item1 - x) == 1 && Math.Abs(choosenSquares[0].Item2 - y) == 0) ||
                            (Math.Abs(choosenSquares[choosenSquares.Count - 1].Item1 - x) == 1 && Math.Abs(choosenSquares[choosenSquares.Count - 1].Item2 - y) == 0);
                    }
                    return result;
                }
            }
            return false;
        }

        //Явлется ли клетка последней частью корабля
        private bool IsEndSquare(int x, int y)
        {
            return choosenSquares[choosenSquares.Count - 1].Item1 == x && choosenSquares[choosenSquares.Count - 1].Item2 == y;
        }

        //Явлется ли клетка передней частью корабля
        private bool IsBeginSquare(int x, int y)
        {
            return choosenSquares[0].Item1 == x && choosenSquares[0].Item2 == y;
        }

        //Обновление информации
        private void RefreshTextBlocks()
        {
            txtBlock1.Text = game.PlayerField.OneCount.ToString();
            txtBlock2.Text = game.PlayerField.TwoCount.ToString();
            txtBlock3.Text = game.PlayerField.ThreeCount.ToString();
            txtBlock4.Text = game.PlayerField.FourCount.ToString();

            if (game.PlayerField.OneCount != 0)
            {
                InfoTextBlock.Text = string.Format("Щелкните по полю чтобы добавить корабль. Осталось: 1 клетка\n" +
                    "Серый цвет: корабль установлен. Зеленый: в процессе установки.");
            }
            else if (game.PlayerField.TwoCount != 0)
            {
                InfoTextBlock.Text = string.Format("Щелкните по полю чтобы добавить корабль. Осталось: {0}\n" +
                    "Серый цвет: корабль установлен. Зеленый: в процессе установки.", 2 - choosenSquares.Count);
            }
            else if (game.PlayerField.ThreeCount != 0)
            {
                InfoTextBlock.Text = string.Format("Щелкните по полю чтобы добавить корабль. Осталось: {0}\n" +
                    "Серый цвет: корабль установлен. Зеленый: в процессе установки.", 3 - choosenSquares.Count);
            }
            else if (game.PlayerField.FourCount != 0)
            {
                InfoTextBlock.Text = string.Format("Щелкните по полю чтобы добавить корабль. Осталось: {0}\n" +
                    "Серый цвет: корабль установлен. Зеленый: в процессе установки.", 4 - choosenSquares.Count);
            }
            else
            {
                InfoTextBlock.Text = "Все корабли расставленны. Нажмите ok.";
                OkButton.IsEnabled = true;
            }
        }

        //Получение направлении по X
        private int GetSignX()
        {
            if (choosenSquares[0].Item1 == choosenSquares[choosenSquares.Count - 1].Item1)
                return 0;
            else
            {
                if(choosenSquares[0].Item1 - choosenSquares[choosenSquares.Count - 1].Item1 < 0)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }

        //Получение информации по Y
        private int GetSignY()
        {
            if (choosenSquares[0].Item2 == choosenSquares[choosenSquares.Count - 1].Item2)
                return 0;
            else
            {
                if (choosenSquares[0].Item2 - choosenSquares[choosenSquares.Count - 1].Item2 < 0)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }

        //Очистка поля
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            game = new Game();
            OkButton.IsEnabled = false;
            choosenSquares.Clear();
            FieldGridInit(FieldGrid, 10, Brushes.CadetBlue);
            RefreshTextBlocks();
        }

        //Заполнение поля случайным образом
        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            game.SetRandomShips(game.PlayerField);
            Close();
        }
    }
}
