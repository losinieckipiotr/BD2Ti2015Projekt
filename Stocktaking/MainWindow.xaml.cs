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

namespace Stocktaking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    using ViewModel;
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginControl.focusLogin();
            StocktakingViewModel.CreateStocktaking(this);
            MessageBox.Show("Hello world.");
        }
        // metoda zmienia visibility każdej zakładki
        public void ChangeVisibility(int index=0,
            Visibility Login=Visibility.Collapsed,
            Visibility Tab=Visibility.Collapsed,
            Visibility UserAcc=Visibility.Collapsed,
            Visibility RoomsVis = Visibility.Collapsed,
            Visibility InstituteManagmentVis = Visibility.Collapsed,
            Visibility DictionaryVis = Visibility.Collapsed,
            Visibility InstituteDevicesList = Visibility.Collapsed,         
            Visibility InstituteWorkersList = Visibility.Collapsed,  
            Visibility RaportsVis = Visibility.Collapsed,
            Visibility LogoutVis= Visibility.Visible
            )
        {
            LoginControl.Visibility = Login;
            UserAccount.Visibility = UserAcc;
            Rooms.Visibility = RoomsVis;
            InstituteManagment.Visibility = InstituteManagmentVis;
            Dictionary.Visibility = DictionaryVis;
            InstituteDevicesManagment.Visibility = InstituteDevicesList;
            InstituteWorkersManagment.Visibility = InstituteWorkersList;
            Raports.Visibility = RaportsVis;
            Logout.Visibility = LogoutVis;
            TabControlMenu.SelectedIndex=index;
            TabControlMenu.Visibility = Tab;//dalem na koniec poniewaz po wykonaniu tego wywoluje sie event visible changed na moich tabach why??
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            ChangeVisibility(Login: Visibility.Visible, Tab: Visibility.Collapsed, LogoutVis: Visibility.Collapsed);
            StocktakingViewModel.Stocktaking.Logout();
        }
    }
}
