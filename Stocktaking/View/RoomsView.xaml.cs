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
using Stocktaking.ViewModel;


namespace Stocktaking.View
{
    /// <summary>
    /// Interaction logic for Rooms.xaml
    /// </summary>
    

    // klasa reprezentująca wpis w DataGrid
    class RoomRecord
    {
        public int id { get; set; }
        public int numer { get; set; }
        public int pojemnosc { get; set; }
        public string typSali { get; set; }
        public string zaklad { get; set; }
        public sala sala { get; set; }

        public RoomRecord(sala s)
        {
            this.id = s.id;
            this.numer = s.numer;
            this.pojemnosc = s.pojemnosc;
            this.typSali = s.sala_typ.typ_sali;
            if (s.zaklad != null)
                this.zaklad = s.zaklad.nazwa;
            else
                this.zaklad = "Sala Międzyzakładowa";
            this.sala = s;
        }
    }

    public partial class Rooms : UserControl
    {
        private StocktakingDatabaseEntities db = null;
        private bool loadUI = true;

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        public Rooms()
        {
            InitializeComponent();
        }

        // funkcja odpowiedzialna za ładowanie danych do elementów GUI
        private async void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    return;
                db = ViewLogic.dbContext;
                StocktakingViewModel.Stocktaking.SelectedTab = Tab.Rooms;
                if (db == null || loadUI == false)
                    return;

                System.Windows.Data.CollectionViewSource roomRecordViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["roomRecordViewSource"];
                await db.sala.LoadAsync();
                List<sala> sale = db.sala.Local.ToList();
                List<RoomRecord> rekordy = new List<RoomRecord>();
                foreach (sala s in sale)
                {
                    rekordy.Add(new RoomRecord(s));
                }
                roomRecordViewSource.Source = rekordy.OrderBy(r => r.id);

                System.Windows.Data.CollectionViewSource sala_typViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["sala_typViewSource"];
                await db.sala_typ.LoadAsync();
                sala_typViewSource.Source = db.sala_typ.Local.ToBindingList();

                newRoomType.ItemsSource = db.sala_typ.Local.ToBindingList().OrderBy(t => t.id);

                System.Windows.Data.CollectionViewSource zakladViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["zakladViewSource"];
                await db.zaklad.LoadAsync();
                zakladViewSource.Source = db.zaklad.Local.ToBindingList();
                loadUI = false;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        // zapobieganie wpisywania do kontrolki TextBox znaków innych niż cyfry
        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        // aktualizowanie elementów GUI wraz ze zmianą zaznaczonego elementu w DataGrid
        private void salaDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                sala s = ((RoomRecord)salaDataGrid.SelectedItem).sala;
                if (s != null)
                    RoomType.SelectedItem = s.sala_typ;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        // funkcja aktualizowania danych sali, która już istnieje w bazie
        private async void RoomUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz zaktualizować dane sali?"))
                    return;

                RoomRecord r = (RoomRecord)salaDataGrid.SelectedItem;
                sala_typ st = (sala_typ)RoomType.SelectedItem;
                int nowyNumer = Convert.ToInt32(RoomNumber.Text);
                if (r.numer != nowyNumer)
                {
                    bool numerZajety = await db.sala.AnyAsync(s => s.numer == nowyNumer);
                    if (numerZajety)
                    {
                        ViewLogic.Blad("Isnieje już sala o podanym numerze!");
                        return;
                    }
                    r.sala.numer = nowyNumer;
                }
                r.sala.pojemnosc = Convert.ToInt32(RoomCapacity.Text);
                r.sala.sala_typ = st;

                await db.SaveChangesAsync();

                OdswiezSale();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //funkcja dodawania nowej sali do bazy
        private async void RoomAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz dodać salę?"))
                    return;

                string nowyNumerS = newRoomNumber.Text;
                string nowaPojemnoscS = newRoomCapacity.Text;
                if (nowyNumerS == "")
                {
                    ViewLogic.Blad("Nie podano numeru!");
                    return;
                }
                if (nowaPojemnoscS == "")
                {
                    ViewLogic.Blad("Nie podano pojemności!");
                    return;
                }
                int nowyNumer = Convert.ToInt32(nowyNumerS);
                int nowaPojemnosc = Convert.ToInt32(nowaPojemnoscS);

                bool numerZajety = await db.sala.AnyAsync(s => s.numer == nowyNumer);
                if (numerZajety)
                {
                    ViewLogic.Blad("Isnieje już sala o podanym numerze!");
                    return;
                }

                int noweId = 1;
                await db.sala.LoadAsync();
                foreach (sala s in db.sala.Local.OrderBy(s => s.id))
                {
                    if (noweId != s.id)
                        break;
                    else
                        ++noweId;
                }

                sala_typ nowyTyp = (sala_typ)newRoomType.SelectedItem;

                sala nowy = new sala
                {
                    id = noweId,
                    numer = nowyNumer,
                    pojemnosc = nowaPojemnosc,
                    sala_typ = nowyTyp,
                    sala_typ_id = nowyTyp.id,
                };
                db.sala.Add(nowy);
                await db.SaveChangesAsync();

                newRoomCapacity.Clear();
                newRoomNumber.Clear();
                newRoomType.SelectedItem = null;
                OdswiezSale();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // fnkcja usuwania sali z bazy
        private async void RoomDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz usunąć salę?"))
                    return;

                RoomRecord r = (RoomRecord)salaDataGrid.SelectedItem;

                db.sala.Remove(r.sala);
                await db.SaveChangesAsync();

                OdswiezSale();
            }
            catch (Exception)
            {

                throw;
            }
        }

        // odświeżenie CollectionViewSource
        private async void OdswiezSale()
        {
            System.Windows.Data.CollectionViewSource roomRecordViewSource =
                (System.Windows.Data.CollectionViewSource)this.Resources["roomRecordViewSource"];
            await db.sala.LoadAsync();
            List<sala> sale = db.sala.Local.ToList();
            List<RoomRecord> rekordy = new List<RoomRecord>();
            foreach (sala s in sale)
            {
                rekordy.Add(new RoomRecord(s));
            }
            roomRecordViewSource.Source = rekordy.OrderBy(r => r.id);

            StocktakingViewModel.Stocktaking.RealoadTabs(
                        raportsTab: true, instituteDevicesTab: true);
        }
    }
}
