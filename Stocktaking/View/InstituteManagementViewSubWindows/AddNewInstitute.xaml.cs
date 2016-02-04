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
    public partial class AddNewInstitute : Window
    {
        private StocktakingDatabaseEntities myDb;
        public bool Answer;
        public AddNewInstitute(StocktakingDatabaseEntities db)
        {
            InitializeComponent();
            myDb = db;
        }

        //załądowanie danych, czyli pracowników i sal
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var temp = from o in myDb.pracownik
                           where o.sala_id == null || o.zaklad.Count == 0
                           select o;
                WorkersDataGrid.ItemsSource = await temp.ToListAsync();

                RoomsDataGrid.ItemsSource = await myDb.sala.Where(s => s.zaklad_id == null).ToListAsync();
                NewIdTextBox.Text = (1 + myDb.zaklad.Max(o => o.id)).ToString();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Window_Loaded!");
            }
        }

        //stworzenie nowego zakładu z nazwą pobraną z textBoxa oraz wybranym pracownikiem w wybranej sali
        private async void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pracownik temp = (pracownik)WorkersDataGrid.SelectedItem;
                sala room = (sala)RoomsDataGrid.SelectedItem;
                temp.sala_id = room.id;
                zaklad newZaklad = new zaklad() { id = int.Parse(NewIdTextBox.Text), nazwa = NewNameTextBox.Text, kierownik = temp.id };
                room.zaklad_id = newZaklad.id;
                myDb.zaklad.Add(newZaklad);
                await myDb.SaveChangesAsync();
                Answer = true;
                this.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w SelectButton_Click!");
            }
        }

        // zamknięcie okna beż wporwadzenia zmian
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Answer = false;
                this.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Button_Click!");
            }
        }


        // aktywowanie przycisku gdy wybrano elementy
        private void upData()
        {
            if (RoomsDataGrid.SelectedItem != null && WorkersDataGrid.SelectedItem != null && !String.IsNullOrWhiteSpace(NewNameTextBox.Text))
                SelectButton.IsEnabled = true;
        }

        //sprawdzenie czy wybrano element
        private void WorkersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                upData();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w WorkersDataGrid_SelectionChanged!");
            }
        }
        //sprawdzenie czy wybrano element
        private void RoomsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                upData();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w RoomsDataGrid_SelectionChanged!");
            }
        }
        //sprawdzenie czy wpisano tekst
        private void NewNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                upData();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w NewNameTextBox_TextChanged!");
            }
        }

    }
}
