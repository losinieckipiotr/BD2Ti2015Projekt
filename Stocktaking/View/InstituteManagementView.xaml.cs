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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Stocktaking.View
{
    /// <summary>
    /// Interaction logic for InstituteManagementView.xaml
    /// </summary>
    using ViewModel;
    using InstituteManagementViewSubWindows;
    public partial class InstituteManagementView : UserControl
    {
        private StocktakingDatabaseEntities db = null;
        private konto userAcc = null;
        private bool loadUI = false;

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        public InstituteManagementView()
        {
            InitializeComponent();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                db = ViewLogic.db;
                if (db == null || loadUI == false)
                    return;

                userAcc = StocktakingViewModel.Stocktaking.GetUser;

                int Type = userAcc.konto_typ_id;
                switch (Type)
                {
                    case 2://Dyrektor zakładu
                        managerUI();
                        upDataManager();
                        break;
                    case 3:// Kierownik instytutu
                        chiefUI();
                        upDataChief();
                        break;
                    case 4:// zakładowy pracownik techniczny
                        technicalWorkerUI();
                        upDataTechnician();
                        break;
                    default:
                        break;
                }
                loadUI = false;
            }
        }

        private zaklad getSelectionZaklad
        {
            get
            {
                if (userAcc.konto_typ_id == db.konto_typ.Single(a => a.nazwa == "Kierownik Instytutu").id)
                {
                    return (zaklad)DataGridInstitute.SelectedItem;
                }
                else
                {
                    if (StocktakingViewModel.Stocktaking.GetZaklad != null)
                        return db.zaklad.Single(a => a.id == StocktakingViewModel.Stocktaking.GetZaklad.id);
                    else
                        return null;
                }
            }
        }

        private void upDataChief()
        {
            ChiefNameTextBlock.Text = null;
            zaklad selectedZaklad = null;
            bool getSelect = false;
            if (DataGridInstitute.SelectedItem != null)
            {
                getSelect = true;
                selectedZaklad = getSelectionZaklad;
            }
            DataGridInstitute.ItemsSource = null;
            DataGridInstitute.ItemsSource = db.zaklad.ToList();
            if (getSelect)
            {
                //Błąd z tym aktualizowaniem po pierwszej zmianie w zakładach
                //gdy przypisuje który zakład był zaznaczony nie wywołuje się zdarzenie DataGridInstitute_SelectionChanged
                //Dlaczego??????????????
                DataGridInstitute.SelectedItem = selectedZaklad;
            }
        }

        private void upDataManager()
        {
            if (getSelectionZaklad != null)
            {
                TextBlockInstituteName.Text = getSelectionZaklad.nazwa;
                pracownik chief = db.pracownik.SingleOrDefault(p => p.id == getSelectionZaklad.kierownik);
                ChiefNameTextBlock.Text = chief.imie + " " + chief.nazwisko + " " + chief.sala.numer;
                var workers = from o in db.pracownik
                              where o.sala.zaklad_id == getSelectionZaklad.id && o.zaklad.Count == 0
                              select o;
                workersDatagrid.ItemsSource = workers.ToList();

                var rooms = db.sala.Where(s => s.zaklad_id == getSelectionZaklad.id);
                RoomsDataGrid.ItemsSource = rooms.ToList();

                var devices = from dev in db.sprzet
                              where dev.sala.zaklad_id == getSelectionZaklad.id
                              select dev;
                DevicesDataGrid.ItemsSource = devices.ToList();
                upDataEnable(true);
            }
        }

        private void upDataTechnician()
        {
            if (getSelectionZaklad != null)
            {
                TextBlockInstituteName.Text = getSelectionZaklad.nazwa;
                pracownik chief = db.pracownik.SingleOrDefault(p => p.id == getSelectionZaklad.kierownik);
                ChiefNameTextBlock.Text = chief.imie + " " + chief.nazwisko + " " + chief.sala.numer;

                var devices = from dev in db.sprzet
                              where dev.sala.zaklad_id == getSelectionZaklad.id
                              select dev;
                DevicesDataGrid.ItemsSource = devices.ToList();
                upDataEnable(true);
            }
        }

        private void upData()
        {
            int Type = userAcc.konto_typ_id;
            switch (Type)
            {
                case 2://Dyrektor zakładu
                    upDataManager();
                    break;
                case 3:// Kierownik instytutu
                    upDataChief();
                    break;
                case 4:// zakładowy pracownik techniczny
                    upDataTechnician();
                    break;
                default:
                    break;
            }
        }

        private void managerUI()
        {
            InstituteName.Visibility = Visibility.Visible;
            InstituteChief.Visibility = Visibility.Visible;
            ChangeChiefName.Visibility = Visibility.Collapsed;
            Institute.Visibility = Visibility.Collapsed;
            InstituteWorkers.Visibility = Visibility.Visible;
            InstituteRooms.Visibility = Visibility.Visible;
            TextBlockInstituteName.IsReadOnly = false;
            ChangeChiefRoom.Visibility = Visibility.Visible;
            ChangeInstituteName.Visibility = Visibility.Visible;
            MyGrid.ColumnDefinitions[0].Width = GridLength.Auto;
            MyGrid.ColumnDefinitions[1].Width = new GridLength(191, GridUnitType.Star);
            MyGrid.ColumnDefinitions[2].Width = new GridLength(175, GridUnitType.Star);
            MyGrid.ColumnDefinitions[3].Width = new GridLength(200, GridUnitType.Star);

        }

        private void chiefUI()
        {
            ChangeChiefRoom.Visibility = Visibility.Visible;
            ChangeChiefName.Visibility = Visibility.Visible;
            InstituteName.Visibility = Visibility.Visible;
            InstituteChief.Visibility = Visibility.Visible;
            Institute.Visibility = Visibility.Visible;
            InstituteWorkers.Visibility = Visibility.Visible;
            InstituteRooms.Visibility = Visibility.Visible;
            TextBlockInstituteName.IsReadOnly = false;
            ChangeInstituteName.Visibility = Visibility.Visible;
            MyGrid.ColumnDefinitions[0].Width = new GridLength(170, GridUnitType.Star);
            MyGrid.ColumnDefinitions[1].Width = new GridLength(170, GridUnitType.Star);
            MyGrid.ColumnDefinitions[2].Width = new GridLength(170, GridUnitType.Star);
            MyGrid.ColumnDefinitions[3].Width = new GridLength(170, GridUnitType.Star);
        }

        private void technicalWorkerUI()
        {
            ChangeChiefName.Visibility = Visibility.Collapsed;
            ChangeChiefRoom.Visibility = Visibility.Collapsed;
            InstituteChief.Visibility = Visibility.Visible;
            Institute.Visibility = Visibility.Collapsed;
            InstituteWorkers.Visibility = Visibility.Collapsed;
            InstituteRooms.Visibility = Visibility.Collapsed;
            TextBlockInstituteName.IsReadOnly = true;
            ChangeInstituteName.Visibility = Visibility.Collapsed;
            MyGrid.ColumnDefinitions[0].Width = GridLength.Auto;
            MyGrid.ColumnDefinitions[1].Width = GridLength.Auto;
            MyGrid.ColumnDefinitions[2].Width = GridLength.Auto;
            MyGrid.ColumnDefinitions[3].Width = new GridLength(200, GridUnitType.Star);

        }


        // wybranie zakładu
        private void DataGridInstitute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridInstitute.SelectedItem != null)
            {
                TextBlockInstituteName.Text = getSelectionZaklad.nazwa;
                pracownik temp = db.pracownik.SingleOrDefault(a => a.id == getSelectionZaklad.kierownik);
                ChiefNameTextBlock.Text = temp.imie + " " + temp.nazwisko + " " + temp.sala.numer;
                var workers = from o in db.pracownik
                              where o.sala.zaklad_id == getSelectionZaklad.id && o.zaklad.Count == 0
                              select o;
                workersDatagrid.ItemsSource = workers.ToList();

                var rooms = db.sala.Where(s => s.zaklad_id == getSelectionZaklad.id);
                RoomsDataGrid.ItemsSource = rooms.ToList();

                var devices = from dev in db.sprzet
                              where dev.sala.zaklad_id == getSelectionZaklad.id
                              select dev;
                DevicesDataGrid.ItemsSource = devices.ToList();

                upDataEnable(true);
            }
            else
            {
                upDataEnable(false);
            }
        }

        //aktualizacja które przeciski są aktywne
        private void upDataEnable(bool Enable)
        {
            ChangeInstituteName.IsEnabled = Enable;
            deleteInstituteButton.IsEnabled = Enable;
            AddWorkerButton.IsEnabled = Enable;
            AddRoomButton.IsEnabled = Enable;
            AddDeviceButton.IsEnabled = Enable;
        }
        private void TextBlockInstituteName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataGridInstitute.SelectedItem != null)
            {
                ChangeInstituteName.IsEnabled = true;
            }
        }

        //zmiana nazwy zakladu
        private void ChangeName_Click(object sender, RoutedEventArgs e)
        {
            var zaklad = db.zaklad.SingleOrDefault(z => z.id == getSelectionZaklad.id);
            zaklad.nazwa = TextBlockInstituteName.Text;
            db.SaveChanges();
            upData();
        }

        //Zmiana szefa zakladu
        private void ChangeChiefName_Click(object sender, RoutedEventArgs e)
        {
            pracownik newChief = (pracownik)workersDatagrid.SelectedItem;
            getSelectionZaklad.kierownik = newChief.id;
            db.SaveChanges();
            upData();
        }

        //zmiana pokoju od kierownika zakładu
        private void ChangeChiefRoom_Click(object sender, RoutedEventArgs e)
        {
            pracownik chief = db.pracownik.SingleOrDefault(prac => prac.id == getSelectionZaklad.kierownik);
            sala room = (sala)RoomsDataGrid.SelectedItem;
            chief.sala_id = room.id;
            db.SaveChanges();
            upData();
        }

        //usuwanie instytut NIE DZIALA!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            zaklad temp = db.zaklad.Single(a => a.id == getSelectionZaklad.id);
            var sale = db.sala.Where(s => s.zaklad_id == temp.id).ToList();
            foreach (var item in sale)
            {
                item.zaklad_id = null;
            }
            db.zaklad.Remove(temp);
            db.SaveChanges();
            upData();
        }

        //dodawanie nowego zakladu
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var MyAddNewInstitute = new AddNewInstitute(db);
            MyAddNewInstitute.ShowDialog();
            if (MyAddNewInstitute.Answer)
            {
                upData();
            }
        }

        private void workersDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (workersDatagrid.SelectedItem != null)
            {
                RemoveWorkerButton.IsEnabled = true;
                ChangeChiefName.IsEnabled = true;
            }
            else
            {
                RemoveWorkerButton.IsEnabled = false;
                ChangeChiefName.IsEnabled = false;
            }
        }

        private void RoomsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RoomsDataGrid.SelectedItem != null)
            {
                RemoveRoomButton.IsEnabled = true;
                ChangeChiefRoom.IsEnabled = true;
            }
            else
            {
                RemoveRoomButton.IsEnabled = false;
                ChangeChiefRoom.IsEnabled = false;
            }
        }

        private void DevicesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DevicesDataGrid.SelectedItem != null)
            {
                MoveDeviceButton.IsEnabled = true;
            }
            else
            {
                MoveDeviceButton.IsEnabled = false;
            }
        }

        //Dodawanie pracownika do zkładu
        private void AddWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            var MyAddNewWorker = new AddNewWorker(db, getSelectionZaklad);
            MyAddNewWorker.ShowDialog();
            if (MyAddNewWorker.answer)
            {
                upData();
            }
        }

        //usuwanie pracownika z zakładu
        private void RemoveWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            pracownik temp = (pracownik)workersDatagrid.SelectedItem;
            temp.sala_id = null;
            db.SaveChanges();
            upData();
        }

        //dodwanie pokoju do zakładu
        private void AddRoomButton_Click(object sender, RoutedEventArgs e)
        {
            var myAddNewRoom = new AddNewRoom(db, getSelectionZaklad);
            myAddNewRoom.ShowDialog();
            if (myAddNewRoom.answer)
            {
                upData();
            }
        }

        //Usuwanie pokoju z zakładu
        private void RemoveRoomButton_Click(object sender, RoutedEventArgs e)
        {
            sala myRoom = (sala)RoomsDataGrid.SelectedItem;
            pracownik myChief = (pracownik)db.pracownik.SingleOrDefault(o => o.sala_id == myRoom.id && o.zaklad.Count != 0);
            if (myChief != null)
            {
                MessageBox.Show("Do tej Sali jest przypisany kierownik zakładu:\n" +
                    myChief.imie + " " + myChief.nazwisko +
                    ".\nJeśli chcesz usunąć tę sale z zakladu najpierw zmień kierownika na takiego,który znajduje się w innej sali"
                    + "\nlub obecnego dodaj do innej sali.");
                return;
            }
            myRoom.zaklad_id = null;
            // po usunięciu sali z zakladu nalezy usunąć wszystkich pracowników z tej sali
            var workersFromRoom = db.pracownik.Where(o => o.sala_id == myRoom.id);
            foreach (var item in workersFromRoom)
            {
                item.sala_id = null;
            }
            db.SaveChanges();
            upData();
        }

        private void MoveDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            var myMoveDevice = new MoveDevice(db, (sprzet)DevicesDataGrid.SelectedItem);
            myMoveDevice.ShowDialog();
            if (myMoveDevice.answer)
            {
                upData();
            }
        }

        private void AddDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            var myAddDevice = new AddDevice(db, getSelectionZaklad);
            myAddDevice.ShowDialog();
            if (myAddDevice.answer)
            {
                upData();
            }
        }
    }
}
