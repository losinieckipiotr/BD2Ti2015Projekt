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
    /// Interaction logic for AddNewRoom.xaml
    /// </summary>
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RoomDataGrid.ItemsSource = myDb.sala.Where(s => s.zaklad_id == null).ToList();
        }

        private void ChoicPersonDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectButton.IsEnabled = true;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            sala myRoom = (sala)RoomDataGrid.SelectedItem;
            myRoom.zaklad_id = myZaklad.id;
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
