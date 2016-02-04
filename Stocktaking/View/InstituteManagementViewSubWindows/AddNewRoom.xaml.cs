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
    public partial class AddNewRoom : Window
    {
        private StocktakingDatabaseEntities myDb;
        private zaklad myZaklad;
        public bool answer = false;
        public AddNewRoom(StocktakingDatabaseEntities db, zaklad zaklad)
        {
            InitializeComponent();
            myDb = db;
            myZaklad = zaklad;
        }

        //wczytanie danych
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RoomDataGrid.ItemsSource = await myDb.sala.Where(s => s.zaklad_id == null).ToListAsync();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Window_Loaded!");
            }
        }

        // gdy wybrano elemnet przycisk jest aktywny
        private void ChoicPersonDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SelectButton.IsEnabled = true;
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w ChoicPersonDataGrid_SelectionChanged!");
            }
        }

        //wprowadzenie zmian, czyli dodanie sali do zakładu i zamknięcie okna
        private async void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sala myRoom = (sala)RoomDataGrid.SelectedItem;
                myRoom.zaklad_id = myZaklad.id;
                await myDb.SaveChangesAsync();

                answer = true;
                this.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w SelectButton_Click!");
            }

        }

        // zamknięcie okna bez zmian
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
