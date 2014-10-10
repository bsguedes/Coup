using System.Collections.Generic;
using System.Windows;
using CoupCore;

namespace Coup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();            
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            var list = new List<Player> {new EasyBot.EasyBot(0), new EasyBot.EasyBot(1), new EasyBot.EasyBot(2), new EasyBot.EasyBot(3)};
            var g = new Game( list );
        }


    }

  
}
