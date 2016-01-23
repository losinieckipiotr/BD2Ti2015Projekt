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
using System.Windows.Shapes;
using Stocktaking.ViewModel;

namespace Stocktaking.View.UserAccountViewSubWindows
{
    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        private konto user = null;

        public ChangePassword(konto k)
        {
            InitializeComponent();
            user = k;
        }

        private void zmienButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string haslo = hasloPassBox.Password;
                string haslo2 = powtorzPassBox.Password;
                if (haslo != haslo2)
                {
                    ViewLogic.Blad("Wprowadzone hasło nie jest jednakowe!");
                    hasloPassBox.Clear();
                    powtorzPassBox.Clear();
                    return;
                }
                user.haslo = ViewLogic.ObliczSHA(haslo);
                this.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void anuljButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
