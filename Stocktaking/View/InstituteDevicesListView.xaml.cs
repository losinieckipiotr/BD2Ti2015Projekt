using Stocktaking.ViewModel;
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

namespace Stocktaking.View
{
    // klasa reprezentująca wpis sprzętu w kontrolce DataGrid
    class DeviceRecord
    {
        public int id { get; set; }
        public string typ { get; set; }
        public string opis { get; set; }
        public string numer_sali { get; set; }
        public string zaklad { get; set; }
        public sprzet s { get; set; }
        public DeviceRecord(sprzet s)
        {
            this.id = s.id;
            this.typ = s.sprzet_typ.typ_sprzetu;
            this.opis = s.opis;
            if (s.sala != null)
                this.numer_sali = s.sala.numer.ToString();
            else
                this.numer_sali = "";
            if (s.sala.zaklad != null)
                this.zaklad = s.sala.zaklad.nazwa;
            else
                this.zaklad = "Sala międzyzakładowa";
            this.s = s;
        }
    }
    /// <summary>
    /// Interaction logic for InstituteDevicesListView.xaml
    /// </summary>
    public partial class InstituteDevicesListView : UserControl
    {
        private StocktakingDatabaseEntities db = null;
        private bool loadUI = true;

        public InstituteDevicesListView()
        {
            InitializeComponent();
        }

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        // funkcja odpowiedzialna za ładowanie danych do elementów GUI
        private async void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    return;
                db = ViewLogic.dbContext;
                StocktakingViewModel.Stocktaking.SelectedTab = Tab.Devs;
                if (db == null || loadUI == false)
                    return;

                System.Windows.Data.CollectionViewSource deviceRecordViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["deviceRecordViewSource"];
                await db.sprzet.LoadAsync();
                List<sprzet> sprzety = db.sprzet.Local.ToList();
                List<DeviceRecord> rekordy = new List<DeviceRecord>();
                foreach (sprzet s in sprzety)
                {
                    rekordy.Add(new DeviceRecord(s));
                }
                deviceRecordViewSource.Source = rekordy.OrderBy(r => r.id);

                System.Windows.Data.CollectionViewSource sprzet_typViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["sprzet_typViewSource"];
                await db.sprzet_typ.LoadAsync();
                sprzet_typViewSource.Source = db.sprzet_typ.Local.ToBindingList();

                System.Windows.Data.CollectionViewSource roomRecordViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["roomRecordViewSource"];
                await db.sala.LoadAsync();
                List<sala> sale = db.sala.Local.ToList();
                List<RoomRecord> rekordyS = new List<RoomRecord>();
                foreach (sala s in sale)
                {
                    rekordyS.Add(new RoomRecord(s));
                }
                roomRecordViewSource.Source = rekordyS.OrderBy(r => r.id);

                System.Windows.Data.CollectionViewSource zakladViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["zakladViewSource"];
                await db.zaklad.LoadAsync();
                zakladViewSource.Source = db.zaklad.Local.ToList();

                loadUI = false;
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w UserControl_IsVisibleChanged!");
            }
        }

        // funkcja aktualizowania danych sprzętu istniejącego już w bazie 
        private async void DeviceUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz zaktualizować dane sprzetu?"))
                    return;

                DeviceRecord r = (DeviceRecord)sprzetDataGrid.SelectedItem;
                sprzet_typ st = (sprzet_typ)DeviceType.SelectedItem;
                string nowyOpis = DeviceDescription.Text;
                if (r.opis != nowyOpis)
                {
                    bool opisIstnieje = await db.sprzet.AnyAsync(s => s.opis == nowyOpis);
                    if (opisIstnieje)
                    {
                        ViewLogic.Blad("Isnieje już sprzęt o podanym opisie!");
                        return;
                    }
                    r.s.opis = nowyOpis;
                }
                r.s.sprzet_typ = st;
                await db.SaveChangesAsync();
                OdswiezSprzety();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w DeviceUpdate_Click!");
            }
        }

        // funkcja dodawania nowego sprzętu do bazy
        private async void DeviceAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // potwierdzenie
                if (!ViewLogic.Potwierdz("Czy chcesz dodać sprzęt?"))
                    return;

                // pobranie i weryfikacja danych z GUI
                string nowyOpis = AddDeviceDescription.Text;
                if (nowyOpis == "")
                {
                    ViewLogic.Blad("Nie podano opisu!");
                    return;
                }
                bool opisIstnieje = db.sprzet.Any(s => s.opis == nowyOpis);
                if (opisIstnieje)
                {
                    ViewLogic.Blad("Isnieje już sprzęt o podanym opisie!");
                    return;
                }

                int noweId = 1;
                await db.sprzet.LoadAsync();
                foreach (sprzet s in db.sprzet.Local.OrderBy(s => s.id))
                {
                    if (noweId != s.id)
                        break;
                    else
                        ++noweId;
                }

                sprzet_typ nowyTyp = (sprzet_typ)AddDeviceType.SelectedItem;
                sala nowaSala = ((RoomRecord)dodajDataGrid.SelectedItem).sala;

                // utworzenie wpisu
                sprzet nowy = new sprzet
                {
                    id = noweId,
                    opis = nowyOpis,
                    sprzet_typ = nowyTyp,
                    sprzet_typ_id = nowyTyp.id,
                    sala = nowaSala,
                    sala_id = nowaSala.id
                    //sala_id = null;
                };
                db.sprzet.Add(nowy);
                await db.SaveChangesAsync();
                OdswiezSprzety();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w DeviceAdd_Click!");
            }
        }

        // usunięcie sprzętu z bazy
        private async void DeviceDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz usunąć sprzęt?"))
                    return;

                DeviceRecord r = (DeviceRecord)sprzetDataGrid.SelectedItem;

                db.sprzet.Remove(r.s);
                await db.SaveChangesAsync();
                OdswiezSprzety();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w DeviceDelete_Click!");
            }
        }

        //odświeżenie danych w kontrolkach (CollectionViewSource)
        private async void OdswiezSprzety()
        {
            System.Windows.Data.CollectionViewSource deviceRecordViewSource =
                (System.Windows.Data.CollectionViewSource)this.Resources["deviceRecordViewSource"];
            await db.sprzet.LoadAsync();
            List<sprzet> sprzety = db.sprzet.Local.ToList();
            List<DeviceRecord> rekordy = new List<DeviceRecord>();
            foreach (sprzet s in sprzety)
            {
                rekordy.Add(new DeviceRecord(s));
            }
            deviceRecordViewSource.Source = rekordy.OrderBy(r => r.id);

            StocktakingViewModel.Stocktaking.RealoadTabs(
                        raportsTab: true);
        }
    }
}
