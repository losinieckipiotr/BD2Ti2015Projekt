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
    public partial class AddNewWorker : Window
    {
        private StocktakingDatabaseEntities myDb;
        private zaklad myZaklad;
        public bool answer = false;
        public AddNewWorker(StocktakingDatabaseEntities db, zaklad zak)
        {
            InitializeComponent();
            myDb = db;
            myZaklad = zak;
        }

        //załadowanie danych do dataGrid
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var workers = await myDb.pracownik.Where(o => o.sala_id == null).ToListAsync();
                var rooms = await myDb.sala.Where(s => s.zaklad_id == myZaklad.id).ToListAsync();

                WorkerDataGrid.ItemsSource = workers;
                RoomDataGrid.ItemsSource = rooms;
                InstituteTextBlock.Text = myZaklad.nazwa;
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Window_Loaded!");
            }
        }

        // sprawdzanie zaznaczenia
        private void WorkerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                upDataUi();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w WorkerDataGrid_SelectionChanged!");
            }
        }

        // sprawdzanie zaznaczenia
        private void RoomDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                upDataUi();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w RoomDataGrid_SelectionChanged!");
            }
        }

        // gdy wybrano elementy z obu dataGrid to przycisk jest aktywny
        private void upDataUi()
        {
            if (RoomDataGrid.SelectedItem != null && WorkerDataGrid.SelectedItem != null)
                SelectButton.IsEnabled = true;
        }

        //wprowadzenie zmian, przypisanie pracownika do zakładu i jednocześnie do sali, zamknięcie okna
        private async void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pracownik worker = (pracownik)WorkerDataGrid.SelectedItem;
                sala room = (sala)RoomDataGrid.SelectedItem;
                worker.sala_id = room.id;
                await myDb.SaveChangesAsync();

                answer = true;
                this.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w SelectButton_Click!");
            }
        }

        //zamknięcie okna beż wprowadzenia zmian
        private void CanselButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                answer = false;
                this.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w CanselButton_Click!");
            }
        }
    }
}
