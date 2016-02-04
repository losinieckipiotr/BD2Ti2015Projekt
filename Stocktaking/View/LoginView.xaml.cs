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

namespace Stocktaking.View
{
    public partial class LoginView : UserControl
    {
        //kontrolka logowania
        public LoginView()
        {
            InitializeComponent();
        }
        //publiczna metoda do ustawiania focusu na polu login
        public void focusLogin()
        {
            this.login.Focus();
        }

        //logowanie przy nacisnieciu enter
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                    Login();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w UserControl_KeyDown!");
            }
        }

        //lub nacisnieciu na przycisk
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Login();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Button_Click!");
            }
        }

        //metoda logowania
        private void Login()
        {
            string log = login.Text;
            string pass = password.Password;
            if (StocktakingViewModel.Stocktaking.NewLogin(log, pass))
            {
                login.Clear();
                password.Clear();
            }
            else
            {
                password.Clear();
            }
        }
    }
}
