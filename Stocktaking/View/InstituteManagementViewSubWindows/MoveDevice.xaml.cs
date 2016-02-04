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
    public partial class MoveDevice : Window
    {
        private StocktakingDatabaseEntities myDb;
        public bool answer = false;
        private sprzet myDevice;
        public MoveDevice(StocktakingDatabaseEntities db, sprzet device)
        {
            InitializeComponent();
            myDb = db;
            myDevice = device;
        }

        // przyciski aktywne w zależności czy wybrano element
        private void RoomsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (RoomsDataGrid.SelectedItem != null)
                {
                    SelectButton.IsEnabled = true;
                }
                else
                {
                    SelectButton.IsEnabled = false;
                }
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w RoomsDataGrid_SelectionChanged!");
            }
        }

        // przeładowanie danych w zależności co jest zaznaczone
        private async void DataGridInstitute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (DataGridInstitute.SelectedItem != null)
                {
                    zaklad myZaklad = (zaklad)DataGridInstitute.SelectedItem;
                    RoomsDataGrid.ItemsSource = await myDb.sala.Where(s => s.zaklad_id == myZaklad.id).ToListAsync();
                }
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w DataGridInstitute_SelectionChanged!");
            }
        }

        // załądowanie danych
        private async void DataGridInstitute_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridInstitute.ItemsSource = await myDb.zaklad.ToListAsync();
                RoomsDataGrid.ItemsSource = await myDb.sala.ToListAsync();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w DataGridInstitute_Loaded!");
            }
        }

        //zapisanie zmian i wyjście z okna
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sala myRoom = (sala)RoomsDataGrid.SelectedItem;
                myDevice.sala_id = myRoom.id;
                myDb.SaveChanges();
                answer = true;
                this.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w SelectButton_Click!");
            }
        }

        // wyjście z okna bez zmian
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
    }
}
