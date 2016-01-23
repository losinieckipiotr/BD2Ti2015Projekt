using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Stocktaking.ViewModel;
using System.Diagnostics;
using System.IO;

namespace Stocktaking.View
{
    /// <summary>
    /// Interaction logic for MenuView.xaml
    /// </summary>
    /// 
    using ViewModel;
    public partial class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            StocktakingViewModel.Stocktaking.Logout();
            StocktakingViewModel.Stocktaking.Window.ChangeVisibility(
                                            Login: Visibility.Visible,
                                            Tab: Visibility.Collapsed,
                                            LogoutVis: Visibility.Collapsed);
            StocktakingViewModel.Stocktaking.Window.LoginControl.focusLogin();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            StocktakingViewModel.Stocktaking.Window.Close();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            //\Stocktaking\bin\Debug\Help\StocktakingHelp.chm
            string myPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Help\", "StocktakingHelp.chm");
            System.Diagnostics.Process.Start(myPath);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            string msg = "Aplikacja na projekt z Przedmiotu BD II\n";
            msg += "Autorzy:\n";
            msg += "Piotr Łosiniecki\n";
            msg += "Szymon Miech\n";
            msg += "Daniel Mikolas\n";
            msg += "Łukasz Okoń\n";
            msg += "Politechnika Śląska 2015";
            MessageBox.Show
                (msg, "O programie", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
