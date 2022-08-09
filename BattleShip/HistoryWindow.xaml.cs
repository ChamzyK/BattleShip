using System.Windows;

namespace BattleShip
{
    public partial class HistoryWindow : Window
    {
        public HistoryWindow(Game game)
        {
            InitializeComponent();
            PlayerListBox.ItemsSource = game.PlayerShotPlaces;
            CompListBox.ItemsSource = game.CompShotPlaces;
        }
    }
}
