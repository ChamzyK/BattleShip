using System;
using System.Collections.Generic;

namespace BattleShip
{
    [Serializable]
    public class Field
    {
        public Square[,] Squares { get; private set; } //Все клетки
        public List<Tuple<int, int>> BannedSquares { get; private set; } //Заблокированные клетки (нельзя разместить корабли)
        public int OneCount { get; set; }//Количество оставшихся кораблей размером 1
        public int TwoCount { get; set; } //Количество оставшихся кораблей размером 2
        public int ThreeCount { get; set; } //Количество оставшихся кораблей размером 3
        public int FourCount { get; set; } //Количество оставшихся кораблей размером 4

        public readonly int SIZE = 10; //Размер поля

        public Field()
        {
            Squares = new Square[SIZE, SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    Squares[i, j] = Square.Empty;
                }
            }
            BannedSquares = new List<Tuple<int, int>>();

            OneCount = 4;
            TwoCount = 3;
            ThreeCount = 2;
            FourCount = 1;
        }

        //Полная очистка поля
        public void Clear()
        {
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    Squares[i, j] = Square.Empty;
                }
            }

            BannedSquares.Clear();

            OneCount = 4;
            TwoCount = 3;
            ThreeCount = 2;
            FourCount = 1;
        }

        //Попытка добавить корабль на поле
        public bool TryAddShip(int count, int x, int y, int signX, int signY)
        {
            if (x < SIZE &&
                x >= 0 &&
                y < SIZE &&
                y >= 0 &&
                (signX == 0 ^ signY == 0) &&
                ((count - 1) * signX + x) >= 0 &&
                ((count - 1) * signX + x) < SIZE &&
                ((count - 1) * signY + y) >= 0 &&
                ((count - 1) * signY + y) < SIZE &&
                ShipNotExist(count))
            {
                Tuple<int, int>[] newShipCoordinates = new Tuple<int, int>[count];
                for (int i = 0; i < count; i++)
                {
                    Tuple<int, int> tuple = new Tuple<int, int>((i) * signX + x, (i) * signY + y);
                    if (BannedSquares.Contains(tuple)) return false;
                    newShipCoordinates[i] = tuple;
                }
                if (ArroundShipEmpty(newShipCoordinates))
                {
                    for (int i = 0; i < newShipCoordinates.Length; i++)
                    {
                        Squares[newShipCoordinates[i].Item1, newShipCoordinates[i].Item2] = Square.Ship;
                        AddBannedSquares(newShipCoordinates);
                    }
                    CrementShip(count, -1);
                    return true;
                }
            }
            return false;
        }

        //Добавление забанненой клетки
        private void AddNewBannedTuple(int x, int y)
        {
            Tuple<int, int> tuple1 = new Tuple<int, int>(x, y);
            if (!BannedSquares.Contains(tuple1))
            {
                BannedSquares.Add(tuple1);
            }
        }

        //Добавление забаненных клеток (для размещения)
        private void AddBannedSquares(Tuple<int, int>[] newShipCoordinates)
        {
            for (int i = 0; i < newShipCoordinates.Length; i++)
            {
                AddNewBannedTuple(newShipCoordinates[i].Item1, newShipCoordinates[i].Item2);
                AddNewBannedTuple(newShipCoordinates[i].Item1 + 1, newShipCoordinates[i].Item2 + 1);
                AddNewBannedTuple(newShipCoordinates[i].Item1 + 1, newShipCoordinates[i].Item2 - 1);
                AddNewBannedTuple(newShipCoordinates[i].Item1 + 1, newShipCoordinates[i].Item2);
                AddNewBannedTuple(newShipCoordinates[i].Item1 - 1, newShipCoordinates[i].Item2 + 1);
                AddNewBannedTuple(newShipCoordinates[i].Item1 - 1, newShipCoordinates[i].Item2 - 1);
                AddNewBannedTuple(newShipCoordinates[i].Item1 - 1, newShipCoordinates[i].Item2);
                AddNewBannedTuple(newShipCoordinates[i].Item1, newShipCoordinates[i].Item2 + 1);
                AddNewBannedTuple(newShipCoordinates[i].Item1, newShipCoordinates[i].Item2 - 1);

            }
        }

        //Уменьшение или увеличение количества оставшихся кораблей (для размещения)
        private void CrementShip(int count, int sign)
        {
            switch (count)
            {
                case 1:
                    OneCount += sign;
                    break;
                case 2:
                    TwoCount += sign;
                    break;
                case 3:
                    ThreeCount += sign;
                    break;
                case 4:
                    FourCount += sign;
                    break;
                default:
                    throw new Exception("Неверная длина корабля");
            }
        }

        //Проверка на наличие кораблей вокруг координат arr (для размещения)
        private bool ArroundShipEmpty(Tuple<int, int>[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Item1 + 1 < SIZE &&
                    arr[i].Item1 + 1 >= 0 &&
                    !((Squares[arr[i].Item1 + 1, arr[i].Item2]) == Square.Empty)) return false;
                if (arr[i].Item1 - 1 < SIZE &&
                    arr[i].Item1 - 1 >= 0 &&
                    !((Squares[arr[i].Item1 - 1, arr[i].Item2]) == Square.Empty)) return false;
                if (arr[i].Item2 + 1 < SIZE &&
                    arr[i].Item2 + 1 >= 0 &&
                    !((Squares[arr[i].Item1, arr[i].Item2 + 1]) == Square.Empty)) return false;
                if (arr[i].Item2 - 1 < SIZE &&
                    arr[i].Item2 - 1 >= 0 &&
                    !((Squares[arr[i].Item1, arr[i].Item2 - 1]) == Square.Empty)) return false;

                if (arr[i].Item1 + 1 < SIZE &&
                    arr[i].Item1 + 1 >= 0 &&
                    arr[i].Item2 + 1 < SIZE &&
                    arr[i].Item2 + 1 >= 0 &&
                    !((Squares[arr[i].Item1 + 1, arr[i].Item2 + 1]) == Square.Empty)) return false;
                if (arr[i].Item1 - 1 < SIZE &&
                    arr[i].Item1 - 1 >= 0 &&
                    arr[i].Item2 + 1 < SIZE &&
                    arr[i].Item2 + 1 >= 0 &&
                    !((Squares[arr[i].Item1 - 1, arr[i].Item2 + 1]) == Square.Empty)) return false;
                if (arr[i].Item2 - 1 < SIZE &&
                    arr[i].Item2 - 1 >= 0 &&
                    arr[i].Item1 + 1 < SIZE &&
                    arr[i].Item1 + 1 >= 0 &&
                    !((Squares[arr[i].Item1 + 1, arr[i].Item2 - 1]) == Square.Empty)) return false;
                if (arr[i].Item2 - 1 < SIZE &&
                    arr[i].Item2 - 1 >= 0 &&
                    arr[i].Item1 - 1 < SIZE &&
                    arr[i].Item1 - 1 >= 0 &&
                    !((Squares[arr[i].Item1 - 1, arr[i].Item2 - 1]) == Square.Empty)) return false;
            }
            return true;
        }

        //Проверка оставшихся кораблей (для размещения)
        private bool ShipNotExist(int count)
        {
            switch (count)
            {
                case 1:
                    return OneCount > 0;
                case 2:
                    return TwoCount > 0;
                case 3:
                    return ThreeCount > 0;
                case 4:
                    return FourCount > 0;
                default:
                    return false;
            }
        }

        //Выстрел по координатам
        public ShotResult Shot(int x, int y)
        {
            if (x >= 0 && x < SIZE && y >= 0 && y < SIZE)
            {
                if (Squares[x, y] == Square.Empty)
                {
                    return ShotResult.Miss;
                }
                if (Squares[x, y] == Square.Ship)
                {
                    Squares[x, y] = Square.BurningShip;
                    if (IsBoom(x, y))
                    {
                        return ShotResult.Boom;
                    }
                    else
                    {
                        return ShotResult.Hit;
                    }
                }
            }
            return ShotResult.Miss;
        }

        //Проверка на потопление корабля
        public bool IsBoom(int x, int y)
        {
            int signX = GetSignX(x, y);
            int signY = GetSignY(x, y);
            int minX = GetMinX(x, y);
            int minY = GetMinY(x, y);

            if (signX == 1 && signY == 1)
            {
                if (minX + signX < 10 && Squares[minX + signX, minY] == Square.Empty) signX = 0;
                else signY = 0;
            }

            while(minX >= 0 && minX < 10 && minY >= 0 && minY < 10 && Squares[minX,minY] != Square.Empty)
            {
                if(Squares[minX,minY] != Square.BurningShip)
                {
                    return false;
                }
                minX += signX;
                minY += signY;
            }

            return true;
        }

        //Получение направление корабля относительно X
        public int GetSignX(int x, int y)
        {
            int minY = GetMinY(x, y);
            if (minY == y)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //Получение направление корабля относительно Y
        public int GetSignY(int x, int y)
        {
            int minX = GetMinX(x, y);
            if (minX == x)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //Получение начальной координаты расположения корабля по X
        public int GetMinX(int x, int y)
        {
            if (x - 1 >= 0 && (Squares[x - 1, y] == Square.Ship || Squares[x - 1, y] == Square.BurningShip))
            {
                return GetMinX(x - 1, y);
            }
            else
            {
                return x;
            }
        }

        //Получение начальной координаты расположения корабля по Y
        public int GetMinY(int x, int y)
        {
            if (y - 1 >= 0 && (Squares[x, y - 1] == Square.Ship || Squares[x, y - 1] == Square.BurningShip))
            {
                return GetMinY(x, y - 1);
            }
            else
            {
                return y;
            }
        }

        //Результат выстрела
        public enum ShotResult
        {
            Miss,
            Hit,
            Boom
        }
    }
}
