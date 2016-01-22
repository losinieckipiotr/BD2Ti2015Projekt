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
    /// Interaction logic for MoveDevice.xaml
    /// </summary>
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

        private void RoomsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void DataGridInstitute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridInstitute.SelectedItem != null)
            {
                zaklad myZaklad = (zaklad)DataGridInstitute.SelectedItem;
                RoomsDataGrid.ItemsSource = myDb.sala.Where(s => s.zaklad_id == myZaklad.id).ToList();
            }
            else
            {

            }
        }

        private void DataGridInstitute_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridInstitute.ItemsSource = myDb.zaklad.ToList();
            RoomsDataGrid.ItemsSource = myDb.sala.ToList();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            sala myRoom = (sala)RoomsDataGrid.SelectedItem;
            myDevice.sala_id = myRoom.id;
            myDb.SaveChanges();
            answer = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            answer = false;
            this.Close();
        }
    }
}
