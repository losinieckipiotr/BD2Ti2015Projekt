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

namespace Stocktaking.View.InstituteManagementViewSubWindows
{
    /// <summary>
    /// Interaction logic for AddDevice.xaml
    /// </summary>
    public partial class AddDevice : Window
    {
        private StocktakingDatabaseEntities myDb;
        public bool answer = false;
        private zaklad myZaklad;
        public AddDevice(StocktakingDatabaseEntities db, zaklad zak)
        {
            InitializeComponent();
            myDb = db;
            myZaklad = zak;
        }

        //wczytanie danych
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            roomsDataGrid.ItemsSource = myDb.sala.Where(s => s.zaklad_id == myZaklad.id).ToList();
            DeviceDataGrid.ItemsSource = myDb.sprzet.ToList();
        }

        //sprawdzanie czy przycisk moze być aktywny
        private void DeviceDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            upDataUi();
        }

        // powybraniu elementów w datagrid, wprowadzenie zmian czyli przypisanie sprzętu do sali z zakładu
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            sprzet device = (sprzet)DeviceDataGrid.SelectedItem;
            sala room = (sala)roomsDataGrid.SelectedItem;
            device.sala_id = room.id;
            myDb.SaveChanges();

            answer = true;
            this.Close();
        }

        //zamknięcie okna bez zmian
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            answer = false;
            this.Close();
        }

        //sprawdzenie czy przycisk może być aktywny
        private void roomsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            upDataUi();
        }

        //sprawdzenie czy przycisk może być aktywny
        private void upDataUi()
        {
            if (roomsDataGrid.SelectedItem != null && DeviceDataGrid.SelectedItem != null)
                SelectButton.IsEnabled = true;
        }
    }
}
