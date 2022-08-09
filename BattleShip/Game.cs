using System;
using System.Collections.Generic;
using static BattleShip.Field;

namespace BattleShip
{
    [Serializable]
    public class Game
    {
        public Field PlayerField { get; set; } //Поле пользователя
        public Field CompField { get; set; } //Поле компьютера
        public bool CanPlayerShot { get; set; } //Для определения хода (чей сейчас ход)
        public bool IsWin { get; set; } //Для определения конца игры
        public bool PlayerWin { get; set; } //Для определения победителя
        public List<Tuple<int, int>> CompShotPlaces { get; set; } //Клетки по которым стрелял компьютер
        public List<Tuple<int, int>> PlayerShotPlaces { get; set; } //Клетки по которым стрелял игрок

        public Game()
        {
            InitCompField();
            PlayerField = new Field();
            CanPlayerShot = true;
            IsWin = false;
            CompShotPlaces = new List<Tuple<int, int>>();
            PlayerShotPlaces = new List<Tuple<int, int>>();
        }

        //Инициализация поля компьютера
        private void InitCompField()
        {
            CompField = new Field();
            SetRandomShips(CompField);
        }

        //Размещение кораблей случайным образом
        public void SetRandomShips(Field field)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            field.Clear();

            while (field.OneCount != 0)
            {
                field.TryAddShip(1, rnd.Next(0, field.SIZE), rnd.Next(0, field.SIZE), 1, 0);
            }

            while (field.TwoCount != 0)
            {
                if (rnd.Next(0, 2) == 1)
                {
                    field.TryAddShip(2, rnd.Next(0, field.SIZE), rnd.Next(0, field.SIZE), 1, 0);
                }
                else
                {
                    field.TryAddShip(2, rnd.Next(0, field.SIZE), rnd.Next(0, field.SIZE), 0, 1);
                }
            }

            while (field.ThreeCount != 0)
            {
                if (rnd.Next(0, 2) == 1)
                {
                    field.TryAddShip(3, rnd.Next(0, field.SIZE), rnd.Next(0, field.SIZE), 1, 0);
                }
                else
                {
                    field.TryAddShip(3, rnd.Next(0, field.SIZE), rnd.Next(0, field.SIZE), 0, 1);
                }
            }

            while (field.FourCount != 0)
            {
                if (rnd.Next(0, 2) == 1)
                {
                    field.TryAddShip(4, rnd.Next(0, field.SIZE), rnd.Next(0, field.SIZE), 1, 0);
                }
                else
                {
                    field.TryAddShip(4, rnd.Next(0, field.SIZE), rnd.Next(0, field.SIZE), 0, 1);
                }
            }
        }

        //Выстрел по координатам
        public ShotResult? Shot(int x, int y)
        {
            if (!IsWin && !PlayerShotPlaces.Contains(new Tuple<int, int>(x, y)))
            {
                ShotResult temp = CompField.Shot(x, y);
                CanPlayerShot = !(temp == ShotResult.Miss);
                PlayerShotPlaces.Add(new Tuple<int, int>(x,y));
                if(CheckWin(CompField))
                {
                    IsWin = true;
                    PlayerWin = true;
                }
                return temp;
            }
            return null;
        }

        //Проверка на конец игры
        private bool CheckWin(Field field)
        {
            foreach (var item in field.Squares)
            {
                if (item == Square.Ship)
                    return false;
            }
            return true;
        }

        //Автоматический выстрел (для компьютера)
        public ShotResult? AutoShot()
        {
            if (!IsWin)
            {
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                Tuple<int, int> tuple = new Tuple<int, int>(rnd.Next(0, 10), rnd.Next(0, 10));
                while (CompShotPlaces.Contains(tuple))
                {
                    tuple = new Tuple<int, int>(rnd.Next(0, 10), rnd.Next(0, 10));
                }
                ShotResult temp = PlayerField.Shot(tuple.Item1, tuple.Item2);
                CanPlayerShot = temp == ShotResult.Miss;
                CompShotPlaces.Add(tuple);
                if (CheckWin(PlayerField))
                {
                    IsWin = true;
                    PlayerWin = false;
                }
                return temp;
            }
            return null;
        }
    }
}
