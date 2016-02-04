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
    //menu aplikacji
    public partial class MenuView : UserControl
    {
        //konstruktor
        public MenuView()
        {
            InitializeComponent();
        }

        //metoda owierania pomocy
        public void Help_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StocktakingViewModel.Stocktaking.OpenHelp();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Help_Click");
            }

        }

        //metoda wylogowania
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StocktakingViewModel.Stocktaking.Logout();
                StocktakingViewModel.Stocktaking.Window.ChangeVisibility(
                                                Login: Visibility.Visible,
                                                Tab: Visibility.Collapsed,
                                                LogoutVis: Visibility.Collapsed);
                StocktakingViewModel.Stocktaking.Window.LoginControl.focusLogin();
                StocktakingViewModel.Stocktaking.SelectedTab = Tab.None;
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Logout_Click");
            }
        }

        //metoda o programie
        private void About_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w About_Click");
            }
        }

        //metoda zamkniecia aplikacji
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StocktakingViewModel.Stocktaking.Window.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Close_Click");
            }
        }
    }
}
