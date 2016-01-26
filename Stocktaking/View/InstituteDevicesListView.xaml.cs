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
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    return;
                db = ViewLogic.dbContext;
                if (db == null || loadUI == false)
                    return;

                System.Windows.Data.CollectionViewSource deviceRecordViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["deviceRecordViewSource"];
                db.sprzet.Load();
                List<sprzet> sprzety = db.sprzet.Local.ToList();
                List<DeviceRecord> rekordy = new List<DeviceRecord>();
                foreach (sprzet s in sprzety)
                {
                    rekordy.Add(new DeviceRecord(s));
                }
                deviceRecordViewSource.Source = rekordy.OrderBy(r => r.id);

                System.Windows.Data.CollectionViewSource sprzet_typViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["sprzet_typViewSource"];
                db.sprzet_typ.Load();
                sprzet_typViewSource.Source = db.sprzet_typ.Local.ToBindingList();

                System.Windows.Data.CollectionViewSource salaViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["salaViewSource"];
                db.sala.Load();
                salaViewSource.Source = db.sala.Local.ToBindingList();

                System.Windows.Data.CollectionViewSource zakladViewSource =
                    (System.Windows.Data.CollectionViewSource)this.Resources["zakladViewSource"];
                db.zaklad.Load();
                zakladViewSource.Source = db.zaklad.Local.ToBindingList();

                loadUI = false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void DeviceUpdate_Click(object sender, RoutedEventArgs e)
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
                    bool opisIstnieje = db.sprzet.Any(s => s.opis == nowyOpis);
                    if (opisIstnieje)
                    {
                        ViewLogic.Blad("Isnieje już sprzęt o podanym opisie!");
                        return;
                    }
                    r.s.opis = nowyOpis;
                }
                r.s.sprzet_typ = st;
                db.SaveChanges();
                OdswiezSprzety();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeviceAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz dodać sprzęt?"))
                    return;

                string nowyOpis = DeviceDescription.Text;
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
                foreach (sprzet s in db.sprzet.Local.OrderBy(s => s.id))
                {
                    if (noweId != s.id)
                        break;
                    else
                        ++noweId;
                }

                sprzet_typ nowyTyp = (sprzet_typ)DeviceType.SelectedItem;

                sprzet nowy = new sprzet
                {
                    id = noweId,
                    opis = nowyOpis,
                    sprzet_typ = nowyTyp,
                    sprzet_typ_id = nowyTyp.id,
                    //sala_id = null;
                };
                db.sprzet.Add(nowy);
                db.SaveChanges();
                OdswiezSprzety();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeviceDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz usunąć sprzęt?"))
                    return;

                DeviceRecord r = (DeviceRecord)sprzetDataGrid.SelectedItem;

                db.sprzet.Remove(r.s);
                db.SaveChanges();
                OdswiezSprzety();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void OdswiezSprzety()
        {
            System.Windows.Data.CollectionViewSource deviceRecordViewSource =
                (System.Windows.Data.CollectionViewSource)this.Resources["deviceRecordViewSource"];
            db.sprzet.Load();
            List<sprzet> sprzety = db.sprzet.Local.ToList();
            List<DeviceRecord> rekordy = new List<DeviceRecord>();
            foreach (sprzet s in sprzety)
            {
                rekordy.Add(new DeviceRecord(s));
            }
            deviceRecordViewSource.Source = rekordy.OrderBy(r => r.id);
        }
    }
}
