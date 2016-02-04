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
using System.Data.Entity;
using Stocktaking.ViewModel;

namespace Stocktaking.View.InstituteManagementViewSubWindows
{
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
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                roomsDataGrid.ItemsSource = await myDb.sala.Where(s => s.zaklad_id == myZaklad.id).ToListAsync();
                DeviceDataGrid.ItemsSource = await myDb.sprzet.ToListAsync();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Window_Loaded!");
            }
        }

        //sprawdzanie czy przycisk moze być aktywny
        private void DeviceDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                upDataUi();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w DeviceDataGrid_SelectionChanged!");
            }
        }

        // powybraniu elementów w datagrid, wprowadzenie zmian czyli przypisanie sprzętu do sali z zakładu
        private async void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sprzet device = (sprzet)DeviceDataGrid.SelectedItem;
                sala room = (sala)roomsDataGrid.SelectedItem;
                device.sala_id = room.id;
                await myDb.SaveChangesAsync();

                answer = true;
                this.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w SelectButton_Click!");
            }
        }

        //zamknięcie okna bez zmian
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                answer = false;
                this.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w CancelButton_Click!");
            }
        }

        //sprawdzenie czy przycisk może być aktywny
        private void roomsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                upDataUi();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w roomsDataGrid_SelectionChanged!");
            }
            
        }

        //sprawdzenie czy przycisk może być aktywny
        private void upDataUi()
        {
            if (roomsDataGrid.SelectedItem != null && DeviceDataGrid.SelectedItem != null)
                SelectButton.IsEnabled = true;
        }
    }
}
