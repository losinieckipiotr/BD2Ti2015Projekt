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

    class RoomRecord
    {
        public int id { get; set; }
        public int numer { get; set; }
        public int pojemnosc { get; set; }
        public string typSali { get; set; }
        public string zaklad { get; set; }
        public sala s { get; set; }

        public RoomRecord(sala s)
        {
            this.id = s.id;
            this.numer = s.numer;
            this.pojemnosc = s.pojemnosc;
            this.typSali = s.sala_typ.typ_sali;
            if (s.zaklad != null)
                this.zaklad = s.zaklad.nazwa;
            else
                this.zaklad = null;
            this.s = s;
        }
    }

    public partial class Rooms : UserControl
    {
        private StocktakingDatabaseEntities db = null;
        private bool loadUI = false;

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        public Rooms()
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

                System.Windows.Data.CollectionViewSource roomRecordViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["roomRecordViewSource"];
                db.sala.Load();
                List<sala> sale = db.sala.Local.ToList();
                List<RoomRecord> rekordy = new List<RoomRecord>();
                foreach (sala s in sale)
                {
                    rekordy.Add(new RoomRecord(s));
                }
                roomRecordViewSource.Source = rekordy.OrderBy(r => r.id);
                //System.Windows.Data.CollectionViewSource salaViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["salaViewSource"];
                //db.sala.Load();
                //salaViewSource.Source = db.sala.Local.ToBindingList();

                System.Windows.Data.CollectionViewSource sala_typViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["sala_typViewSource"];
                db.sala_typ.Load();
                sala_typViewSource.Source = db.sala_typ.Local.ToBindingList();

                System.Windows.Data.CollectionViewSource zakladViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["zakladViewSource"];
                db.zaklad.Load();
                zakladViewSource.Source = db.zaklad.Local.ToBindingList();

                loadUI = false;
            }
        }

        private void salaDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                RoomRecord r = null;
                if (salaDataGrid.SelectedItem != null)
                    r = (RoomRecord)salaDataGrid.SelectedItem;
                if (r != null)
                {
                    RoomType.SelectedItem = (sala_typ)r.s.sala_typ;
                    if (r.zaklad != null)
                        RoomInstitute.Text = r.zaklad;
                    else
                        RoomInstitute.Text = "";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;

            }
        }

        private void RoomUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Potwierdz("Czy chcesz zaktualizować dane sali?"))
                    return;

                RoomRecord r = (RoomRecord)salaDataGrid.SelectedItem;
                sala_typ st = (sala_typ)RoomType.SelectedItem;
                //zaklad z = (zaklad)RoomInstitute.SelectedItem;
                r.s.numer = Convert.ToInt32(RoomNumber.Text);
                r.s.pojemnosc = Convert.ToInt32(RoomCapacity.Text);
                r.s.sala_typ = st;
                //s.zaklad = z;
                db.SaveChanges();

                OdswiezSale();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OdswiezSale()
        {
            System.Windows.Data.CollectionViewSource roomRecordViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["roomRecordViewSource"];
            //db.sala.Load();
            roomRecordViewSource.Source = null;
            List<sala> sale = db.sala.Local.ToList();
            List<RoomRecord> rekordy = new List<RoomRecord>();
            foreach (sala s in sale)
            {
                rekordy.Add(new RoomRecord(s));
            }
            roomRecordViewSource.Source = rekordy.OrderBy(r => r.id);
        }

        private bool Potwierdz(string pytanie)
        {
            MessageBoxResult result = MessageBox.Show
                   (pytanie,
                   "Pytanie",
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Question,
                   MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
                return true;
            else
                return false;
        }

        private void Blad(string wiadomosc)
        {
            MessageBox.Show(
                       wiadomosc,
                       "Błąd",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error);
        }

        private void RoomAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Potwierdz("Czy chcesz dodać salę?"))
                    return;

                string nowyNumerS = RoomNumber.Text;
                string nowaPojemnoscS = RoomCapacity.Text;
                if (nowyNumerS == "")
                {
                    Blad("Nie podano numeru!");
                    return;
                }
                if (nowaPojemnoscS == "")
                {
                    Blad("Nie podano pojemności!");
                    return;
                }
                int nowyNumer = Convert.ToInt32(nowyNumerS);
                int nowaPojemnosc = Convert.ToInt32(nowaPojemnoscS);

                bool numerZajety = db.sala.Any(s => s.numer == nowyNumer);
                if (numerZajety)
                {
                    Blad("Isnieje już sala o podanym numerze!");
                    return;
                }

                int noweId = 1;
                foreach (sala s in db.sala.Local.OrderBy(s => s.id))
                {
                    if (noweId != s.id)
                        break;
                    else
                        ++noweId;
                }

                sala_typ nowyTyp = (sala_typ)RoomType.SelectedItem;
                //zaklad nowyZaklad = (zaklad)RoomInstitute.SelectedItem;

                sala nowy = new sala
                {
                    id = noweId,
                    numer = nowyNumer,
                    pojemnosc = nowaPojemnosc,
                    sala_typ = nowyTyp,
                    sala_typ_id = nowyTyp.id,
                    //zaklad = nowyZaklad,
                    //zaklad_id = nowyZaklad.id
                };
                db.sala.Add(nowy);
                db.SaveChanges();
                OdswiezSale();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void RoomDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Potwierdz("Czy chcesz usunąć salę?"))
                    return;

                RoomRecord r = (RoomRecord)salaDataGrid.SelectedItem;
                db.sala.Remove(r.s);
                db.SaveChanges();
                OdswiezSale();

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
