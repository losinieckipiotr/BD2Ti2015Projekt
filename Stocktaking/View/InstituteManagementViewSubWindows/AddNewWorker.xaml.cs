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
    /// Interaction logic for AddNewWorker.xaml
    /// </summary>
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var workers = myDb.pracownik.Where(o => o.sala_id == null).ToList();
            var rooms = myDb.sala.Where(s => s.zaklad_id == myZaklad.id).ToList();

            WorkerDataGrid.ItemsSource = workers;
            RoomDataGrid.ItemsSource = rooms;
            InstituteTextBlock.Text = myZaklad.nazwa;
        }

        private void WorkerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            upDataUi();
        }

        private void RoomDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            upDataUi();
        }

        private void upDataUi()
        {
            if (RoomDataGrid.SelectedItem != null && WorkerDataGrid.SelectedItem != null)
                SelectButton.IsEnabled = true;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            pracownik worker = (pracownik)WorkerDataGrid.SelectedItem;
            sala room = (sala)RoomDataGrid.SelectedItem;
            worker.sala_id = room.id;
            myDb.SaveChanges();

            answer = true;
            this.Close();
        }

        private void CanselButton_Click(object sender, RoutedEventArgs e)
        {
            answer = false;
            this.Close();
        }
    }
}
