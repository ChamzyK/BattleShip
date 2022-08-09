using System;

namespace BattleShip
{
    [Serializable]
    public enum Square
    {
        Empty, //Пустая клетка
        Ship, //Клетка с кораблем
        BurningShip //Клетка с атакованным кораблем
    }
}
