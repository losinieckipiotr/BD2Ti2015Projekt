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
using System.Data.Entity;
using Stocktaking.Data;

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
        private bool loadUI = true;

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        public InstituteManagementView()
        {
            InitializeComponent();
        }

        // w zależności kto się zalogował tak interfejs zostanie zmodyfikowany
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    return;
                db = ViewLogic.dbContext;
                StocktakingViewModel.Stocktaking.SelectedTab = Tab.InstMan;
                if (db == null || loadUI == false)
                    return;

                userAcc = StocktakingViewModel.Stocktaking.User;

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
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w UserControl_IsVisibleChanged!");
            }
        }

        // zwraca zakład który jest zaznaczony lub ten do którego należy zalogowany użytkownik
        private zaklad getSelectionZaklad()
        {
            if (userAcc.konto_typ_id == 3)//kierownik instytutu
            {
                return (zaklad)DataGridInstitute.SelectedItem;
            }
            else
            {
                return DataFunctions.GetZaklad(userAcc.pracownik);
            }
        }

        // przeładowanie danych dla dyrektora instytutu
        private async void upDataChief()
        {
            ChiefNameTextBlock.Text = null;
            zaklad selectedZaklad = (zaklad)DataGridInstitute.SelectedItem;
            DataGridInstitute.ItemsSource = null;
            await db.zaklad.LoadAsync();
            DataGridInstitute.ItemsSource = db.zaklad.Local.ToBindingList();
            DataGridInstitute.SelectedItem = selectedZaklad;
            StocktakingViewModel.Stocktaking.RealoadTabs(
                instituteDevicesTab: true,
                instituteWorkersTab: true,
                raportsTab: true,
                roomsTab: true);
        }

        // przeładowanie danych dla dyrektora zakładu
        private async void upDataManager()
        {
            zaklad selectedZaklad = getSelectionZaklad();
            if (selectedZaklad != null)
            {
                TextBlockInstituteName.Text = selectedZaklad.nazwa;
                pracownik chief = selectedZaklad.pracownik;
                ChiefNameTextBlock.Text = chief.imie + " " + chief.nazwisko + " " + chief.sala.numer;
                var workers = from o in db.pracownik
                              where o.sala.zaklad_id == selectedZaklad.id && o.zaklad.Count == 0
                              select o;
                workersDatagrid.ItemsSource = await workers.ToListAsync();

                var rooms = db.sala.Where(s => s.zaklad_id == selectedZaklad.id);
                RoomsDataGrid.ItemsSource = await rooms.ToListAsync();

                var devices = from dev in db.sprzet
                              where dev.sala.zaklad_id == selectedZaklad.id
                              select dev;
                DevicesDataGrid.ItemsSource = await devices.ToListAsync();
                upDataEnable(true);
                StocktakingViewModel.Stocktaking.RealoadTabs(
                    raportsTab: true);
            }
        }

        // przeładowanie danych dla pracownika technicznego
        private async void upDataTechnician()
        {
            zaklad selectedZaklad = getSelectionZaklad();
            if (selectedZaklad != null)
            {
                TextBlockInstituteName.Text = selectedZaklad.nazwa;
                pracownik chief = selectedZaklad.pracownik;
                ChiefNameTextBlock.Text = chief.imie + " " + chief.nazwisko + " " + chief.sala.numer;

                var devices = from dev in db.sprzet
                              where dev.sala.zaklad_id == selectedZaklad.id
                              select dev;
                DevicesDataGrid.ItemsSource = await devices.ToListAsync();
                upDataEnable(true);
                StocktakingViewModel.Stocktaking.RealoadTabs(
                    instituteDevicesTab: true,
                    roomsTab: true);
            }
        }

        //w zależności kto jest zalogoany takie przeładowanie zostanie wykonane
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

        //interfejs dyrektora zakładu
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

        // interfejs dyrektora instytutu
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

        // interfejs pracownika technicznego
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
        private async void DataGridInstitute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (DataGridInstitute.SelectedItem != null)
                {
                    zaklad selectedZaklad = getSelectionZaklad();
                    TextBlockInstituteName.Text = selectedZaklad.nazwa;
                    pracownik temp = selectedZaklad.pracownik;
                    ChiefNameTextBlock.Text = temp.imie + " " + temp.nazwisko + " " + temp.sala.numer;
                    var workers = from o in db.pracownik
                                  where o.sala.zaklad_id == selectedZaklad.id && o.zaklad.Count == 0
                                  select o;
                    workersDatagrid.ItemsSource = await workers.ToListAsync();

                    var rooms = db.sala.Where(s => s.zaklad_id == selectedZaklad.id);
                    RoomsDataGrid.ItemsSource = await rooms.ToListAsync();

                    var devices = from dev in db.sprzet
                                  where dev.sala.zaklad_id == selectedZaklad.id
                                  select dev;
                    DevicesDataGrid.ItemsSource = await devices.ToListAsync();

                    upDataEnable(true);
                }
                else
                {
                    upDataEnable(false);
                }
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w DataGridInstitute_SelectionChanged!");
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

        //przycisk aktywny po zmianie nazwy zakładu
        private void TextBlockInstituteName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ChangeInstituteName.IsEnabled = true;
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w TextBlockInstituteName_TextChanged!");
            }       
        }

        //zmiana nazwy zakladu
        private async void ChangeName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getSelectionZaklad().nazwa = TextBlockInstituteName.Text;
                await db.SaveChangesAsync();
                upData();
                ChangeInstituteName.IsEnabled = false;
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w ChangeName_Click!");
            }
        }

        //Zmiana szefa zakladu
        private async void ChangeChiefName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pracownik newChief = (pracownik)workersDatagrid.SelectedItem;
                getSelectionZaklad().kierownik = newChief.id;
                await db.SaveChangesAsync();
                upData();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w ChangeChiefName_Click!");
            }
        }

        //zmiana pokoju od kierownika zakładu
        private async void ChangeChiefRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pracownik chief = getSelectionZaklad().pracownik;
                sala room = (sala)RoomsDataGrid.SelectedItem;
                chief.sala_id = room.id;
                await db.SaveChangesAsync();
                upData();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w ChangeChiefRoom_Click!");
            }
        }

        //usuwanie instytut 
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                zaklad temp = getSelectionZaklad();
                var sale = db.sala.Where(s => s.zaklad_id == temp.id).ToList();
                foreach (var item in sale)
                {
                    item.zaklad_id = null;
                }
                db.zaklad.Remove(temp);
                await db.SaveChangesAsync();
                upData();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Button_Click!");
            }
        }

        //dodawanie nowego zakladu
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                AddNewInstitute MyAddNewInstitute = new AddNewInstitute(db);
                MyAddNewInstitute.ShowDialog();
                if (MyAddNewInstitute.Answer)
                {
                    upData();
                }
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Button_Click_1!");
            }

        }

        //jeśli pracownik zaznaczony to przyciski aktywne
        private void workersDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
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
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w workersDatagrid_SelectionChanged!");
            }
            
        }

        //jeśli sala zaznaczona to przyciski aktywne
        private void RoomsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
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
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w RoomsDataGrid_SelectionChanged!");
            }
        }

        // jeśli sprzęt zaznaczony to przycisk aktywny
        private void DevicesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
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
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w DevicesDataGrid_SelectionChanged!");
            }
        }

        //Dodawanie pracownika do zkładu
        private void AddWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var MyAddNewWorker = new AddNewWorker(db, getSelectionZaklad());
                MyAddNewWorker.ShowDialog();
                if (MyAddNewWorker.answer)
                {
                    upData();
                }
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w AddWorkerButton_Click!");
            }
        }

        //usuwanie pracownika z zakładu
        private async void RemoveWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pracownik temp = (pracownik)workersDatagrid.SelectedItem;
                temp.sala_id = null;
                await db.SaveChangesAsync();
                upData();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w RemoveWorkerButton_Click!");
            }
        }

        //dodwanie pokoju do zakładu
        private void AddRoomButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var myAddNewRoom = new AddNewRoom(db, getSelectionZaklad());
                myAddNewRoom.ShowDialog();
                if (myAddNewRoom.answer)
                {
                    upData();
                    StocktakingViewModel.Stocktaking.RealoadTabs(instituteDevicesTab: true);
                }
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w AddRoomButton_Click!");
            }
        }

        //Usuwanie pokoju z zakładu
        private async void RemoveRoomButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sala myRoom = (sala)RoomsDataGrid.SelectedItem;
                pracownik myChief = await db.pracownik.SingleOrDefaultAsync(o => o.sala_id == myRoom.id && o.zaklad.Count != 0);
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
                await db.SaveChangesAsync();
                upData();
                StocktakingViewModel.Stocktaking.RealoadTabs(instituteDevicesTab: true);
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w RemoveRoomButton_Click!");
            }
        }

        //przenoszenie sprzętu do innej sali
        private void MoveDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var myMoveDevice = new MoveDevice(db, (sprzet)DevicesDataGrid.SelectedItem);
                myMoveDevice.ShowDialog();
                if (myMoveDevice.answer)
                {
                    upData();
                }
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w MoveDeviceButton_Click!");
            }
        }

        // dodawanie sprzętu
        private void AddDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var myAddDevice = new AddDevice(db, getSelectionZaklad());
                myAddDevice.ShowDialog();
                if (myAddDevice.answer)
                {
                    upData();
                }
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w AddDeviceButton_Click!");
            }
        }
    }
}
