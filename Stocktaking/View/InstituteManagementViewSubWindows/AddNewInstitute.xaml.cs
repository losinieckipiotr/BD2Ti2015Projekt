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
    /// Interaction logic for AddNewInstitute.xaml
    /// </summary>
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var temp = from o in myDb.pracownik
                       where o.sala_id == null || o.zaklad.Count == 0
                       select o;
            WorkersDataGrid.ItemsSource = temp.ToList();

            RoomsDataGrid.ItemsSource = myDb.sala.Where(s => s.zaklad_id == null).ToList();
            NewIdTextBox.Text = (1 + myDb.zaklad.Max(o => o.id)).ToString();
        }

        //stworzenie nowego zakładu z nazwą pobraną z textBoxa oraz wybranym pracownikiem w wybranej sali
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            pracownik temp = (pracownik)WorkersDataGrid.SelectedItem;
            sala room = (sala)RoomsDataGrid.SelectedItem;
            temp.sala_id = room.id;
            zaklad newZaklad = new zaklad() { id = int.Parse(NewIdTextBox.Text), nazwa = NewNameTextBox.Text, kierownik = temp.id };
            room.zaklad_id = newZaklad.id;
            myDb.zaklad.Add(newZaklad);
            myDb.SaveChanges();
            Answer = true;
            this.Close();
        }

        // zamknięcie okna beż wporwadzenia zmian
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Answer = false;
            this.Close();
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
            upData();
        }
        //sprawdzenie czy wybrano element
        private void RoomsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            upData();
        }
        //sprawdzenie czy wpisano tekst
        private void NewNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            upData();
        }

    }
}
