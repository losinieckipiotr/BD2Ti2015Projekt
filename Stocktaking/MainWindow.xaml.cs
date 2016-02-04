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

        //ustawienie focusa na kontrolke loginu,
        //stworzenie kontekstu bazy i logiki zakladek aplikacji
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginControl.focusLogin();
            StocktakingViewModel.CreateStocktaking(this);
        }

        // metoda zmienia visibility każdej zakładki,
        //index decyduje która zakładka ma zostac wybrana jako początkowa
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
            TabControlMenu.Visibility = Tab;
        }
    }
}
