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
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;
using System.Data.Entity;
using Stocktaking.ViewModel;
using Stocktaking.Data;

namespace Stocktaking.View.RaportsViewSubWindows
{
    public partial class SelectOldRaport : Window
    {
        private StocktakingDatabaseEntities myDb;
        public SelectOldRaport(StocktakingDatabaseEntities db, int institute = 0)
        {
            InitializeComponent();
            myDb = db;
        }

        private void RaportDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                buttonUpdata();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w RaportDatagrid_SelectionChanged!");
            }
        }

        //w zależności kto się zalogował generuje inną listę dostępnych raportów
        private void RaportDatagrid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int Type = StocktakingViewModel.Stocktaking.User.konto_typ_id;
                switch (Type)
                {
                    case 2://Dyrektor zakładu
                        upDataMan();
                        break;
                    case 3:// Kierownik instytutu
                        upDataChief();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w RaportDatagrid_Loaded!");
            }
        }
    
        //wczytanie danych dla dyrektora instytutu
        private void upDataChief()
        {
            var raports = myDb.raport;
            RaportDatagrid.ItemsSource = raports.ToList();
        }

        //wczytanie danych dla dyrektora zakładu
        private async void upDataMan()
        {
            zaklad zak = DataFunctions.GetZaklad(StocktakingViewModel.Stocktaking.User.pracownik);
            var raports = myDb.raport.Where(r => r.konto.pracownik.id == zak.pracownik.id);
            RaportDatagrid.ItemsSource = await raports.ToListAsync();
        }

        private string fileName = "";
        //pobranie ścieżki zapisu do pliku
        private void GetPathButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog mySaveFileDialog = new SaveFileDialog();
                mySaveFileDialog.InitialDirectory = @"c:\";
                mySaveFileDialog.Filter = "Pliki tekstowe (*.txt)|*.txt";
                var temp = mySaveFileDialog.ShowDialog();
                if (temp.Value == true)
                {
                    fileName = mySaveFileDialog.FileName;
                    PathTextBox.Text = fileName;
                    buttonUpdata();
                }
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w GetPathButton_Click!");
            }
        }

        // zmiana aktywności przycisków
        private void buttonUpdata()
        {
            if (RaportDatagrid.SelectedItem != null && !String.IsNullOrWhiteSpace(PathTextBox.Text))
                SaveButton.IsEnabled = true;
        }

        //wyjście z okienka
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w CancelButton_Click!");
            }
        }

        //zapisywanie raportu do pliku i zamknięcie okna
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    raport rap = (raport)RaportDatagrid.SelectedItem;
                    string myRaport = "Raport wczytano z bazy dnia:" + DateTime.Now;
                    myRaport += "\r\n\r\n";
                    myRaport += rap.raport1;
                    await sw.WriteAsync(myRaport);
                }
                this.Close();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w SaveButton_Click!");
            }
        }
    }
}
