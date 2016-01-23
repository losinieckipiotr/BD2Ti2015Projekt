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

namespace Stocktaking.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    using ViewModel;

    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        public void focusLogin()
        {
            this.login.Focus();
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Login();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void Login()
        {
            string log = login.Text;
            string pass = password.Password;
            if (StocktakingViewModel.Stocktaking.NewLogin(log, pass))
            {
                login.Clear();
                password.Clear();
            }
        }
    }
}
