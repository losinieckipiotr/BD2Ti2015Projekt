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

namespace Stocktaking.View.RaportsViewSubWindows
{
    /// <summary>
    /// Interaction logic for SelectOldRaport.xaml
    /// </summary>
    using ViewModel;
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
            buttonUpdata();
        }

        private void RaportDatagrid_Loaded(object sender, RoutedEventArgs e)
        {
            int Type = StocktakingViewModel.Stocktaking.GetUser.konto_typ_id;
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

        private void upDataChief()
        {
            var raports = myDb.raport;
            RaportDatagrid.ItemsSource = raports.ToList();
        }

        private void upDataMan()
        {
            zaklad zak = StocktakingViewModel.Stocktaking.GetZaklad;
            var raports = myDb.raport.Where(r => r.konto.pracownik.id == zak.pracownik.id);
            RaportDatagrid.ItemsSource = raports.ToList();
        }

        private string fileName = "";
        private void GetPathButton_Click(object sender, RoutedEventArgs e)
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

        private void buttonUpdata()
        {
            if (RaportDatagrid.SelectedItem != null && !String.IsNullOrWhiteSpace(PathTextBox.Text))
                SaveButton.IsEnabled = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                raport rap = (raport)RaportDatagrid.SelectedItem;
                string myRaport = "Raport wczytano z bazy dnia:" + DateTime.Now;
                myRaport += "\r\n\r\n";
                myRaport += rap.raport1;
                sw.Write(myRaport);
            }
            this.Close();
        }
    }
}
