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
    /// Interaction logic for InstituteWorkersListView.xaml
    /// </summary>
    /// 

    class WorkerRecord
    {
        public int id { get; set; }
        public string imie { get; set; }
        public string nazwisko { get; set; }
        public int sala { get; set; }
        public string zaklad { get; set; }
        public pracownik pracownik { get; set; }

        public WorkerRecord(pracownik p)
        {
            this.id = p.id;
            this.imie = p.imie;
            this.nazwisko = p.nazwisko;
            this.sala = p.sala.numer;
            this.zaklad = p.sala.zaklad.nazwa;
            this.pracownik = p;
        }
    }

    public partial class InstituteWorkersListView : UserControl
    {
        private StocktakingDatabaseEntities db = null;
        private bool loadUI = false;

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        public InstituteWorkersListView()
        {
            InitializeComponent();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    return;
                db = ViewLogic.dbContext;
                if (db == null || loadUI == false)
                    return;

                System.Windows.Data.CollectionViewSource workerRecordViewSource =
                    ((System.Windows.Data.CollectionViewSource)(this.FindResource("workerRecordViewSource")));
                db.pracownik.Load();
                List<pracownik> pracownicy = db.pracownik.Local.ToList();
                List<WorkerRecord> rekordy = new List<WorkerRecord>();
                foreach (pracownik p in pracownicy)
                {
                    rekordy.Add(new WorkerRecord(p));
                }
                workerRecordViewSource.Source = rekordy.OrderBy(r => r.id);
                
                loadUI = false;
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
