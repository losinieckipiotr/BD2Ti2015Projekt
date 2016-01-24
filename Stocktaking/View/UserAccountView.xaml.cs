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
    /// Interaction logic for UserAccountView.xaml
    /// </summary>
    /// 

    class UserRecord
	{
        public int id { get; set; }
        public string login { get; set; }
        public string typ_konta { get; set; }
        public konto konto { get; set; }

        public UserRecord(konto k)
        {
            this.id = k.id;
            this.login = k.login;
            this.typ_konta = k.konto_typ.nazwa;
            this.konto = k;
        }
	}

    public partial class UserAccountView : UserControl
    {
        private StocktakingDatabaseEntities db = null;
        private bool loadUI = false;

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        public UserAccountView()
        {
            InitializeComponent();
        }  

        private async void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    return;
                db = ViewLogic.dbContext;
                if (db == null || loadUI == false)
                    return;

                //asynchroniczne operacje na bazie danych/////////////////////////////////////////////////
                await db.konto.LoadAsync();
                await db.konto_typ.LoadAsync();
                List<pracownik> pr = await db.pracownik.Where(p => p.konto.Count == 0).ToListAsync();
                //////////////////////////////////////////////////////////////////////////////////////////

                System.Windows.Data.CollectionViewSource userRecordViewSource =
                    ((System.Windows.Data.CollectionViewSource)(this.FindResource("userRecordViewSource")));
                //db.konto.Load();
                List<UserRecord> rekordy = new List<UserRecord>();
                List<konto> konta = db.konto.Local.ToList();
                foreach (konto k in konta)
                {
                    rekordy.Add(new UserRecord(k));
                }
                userRecordViewSource.Source = rekordy.OrderBy(r => r.id);

                System.Windows.Data.CollectionViewSource konto_typViewSource =
                    ((System.Windows.Data.CollectionViewSource)(this.FindResource("konto_typViewSource")));
                //db.konto_typ.Load();
                konto_typViewSource.Source = db.konto_typ.Local.ToBindingList().OrderBy(t => t.id);

                typComboBox.ItemsSource = db.konto_typ.Local.ToList().OrderBy(t => t.id);

                System.Windows.Data.CollectionViewSource pracownikViewSource =
                    ((System.Windows.Data.CollectionViewSource)(this.FindResource("pracownikViewSource")));
                //List<pracownik> pr = db.pracownik.Where(p => p.konto.Count == 0).ToList();
                pracownikViewSource.Source = pr;

                loadUI = false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz zmienić login użytkownika?"))
                    return;

                konto k = ((UserRecord)userRecordDataGrid.SelectedItem).konto;

                k.login = zaznaczonyLoginTextBox.Text;

                db.SaveChanges();

                OdswiezKonta();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void usunButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz usunąć konto użytkownika?"))
                    return;

                konto k = ((UserRecord)userRecordDataGrid.SelectedItem).konto;

                db.konto.Remove(k);
                db.SaveChanges();

                OdswiezKonta();
                OdswiezPracownikow();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void typButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz zmienić typ użytkownika?"))
                    return;

                konto k = ((UserRecord)userRecordDataGrid.SelectedItem).konto;

                konto_typ wybranyTyp = (konto_typ)typKontaComboBox.SelectedItem;
                k.konto_typ = wybranyTyp;

                db.SaveChanges();

                OdswiezKonta();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void hasloButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                konto k = ((UserRecord)userRecordDataGrid.SelectedItem).konto;
                new UserAccountViewSubWindows.ChangePassword(k).ShowDialog();

                db.SaveChanges();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void dodajButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz dodać użytkownika?"))
                    return;

                string nowyLogin = nowyLoginTextBox.Text;
                if (nowyLogin == "")
                {
                    ViewLogic.Blad("Nie podano loginu!");
                    hasloPassBox.Clear();
                    powtorzPassBox.Clear();
                    return;
                }
                bool loginZajety = db.konto.Any(k => k.login == nowyLogin);
                string haslo = hasloPassBox.Password;
                string haslo2 = powtorzPassBox.Password;
                if (haslo != haslo2)
                {
                    ViewLogic.Blad("Wprowadzone hasło nie jest jednakowe!");
                    hasloPassBox.Clear();
                    powtorzPassBox.Clear();
                    return;
                }
                if (loginZajety)
                {
                    ViewLogic.Blad("Isnieje już użytkownik o podanym loginie!");
                    hasloPassBox.Clear();
                    powtorzPassBox.Clear();
                    return;
                }
                byte[] sha = ViewLogic.ObliczSHA(haslo);

                int noweId = 1;
                foreach (konto k in db.konto.Local.OrderBy(k => k.id))
                {
                    if (noweId != k.id)
                        break;
                    else
                        ++noweId;
                }

                konto_typ nowyTyp = (konto_typ)typComboBox.SelectedItem;
                pracownik p = (pracownik)pracownikDataGrid.SelectedItem;

                konto nowy = new konto
                {
                    id = noweId,
                    haslo = sha,
                    konto_typ = nowyTyp,
                    login = nowyLogin,
                    pracownik = p,
                    pracownik_id = p.id,
                    konto_typ_id = nowyTyp.id
                };
                db.konto.Add(nowy);
                db.SaveChanges();

                nowyLoginTextBox.Clear();
                hasloPassBox.Clear();
                powtorzPassBox.Clear();
                typComboBox.SelectedItem = null;
                OdswiezKonta();
                OdswiezPracownikow();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void userRecordDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UserRecord r = (UserRecord)userRecordDataGrid.SelectedItem;
                konto k = r.konto;

                if (k != null)
                {
                    typKontaComboBox.SelectedItem = (konto_typ)k.konto_typ;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void OdswiezKonta()
        {
            System.Windows.Data.CollectionViewSource userRecordViewSource =
                       ((System.Windows.Data.CollectionViewSource)(this.FindResource("userRecordViewSource")));
            db.konto.Load();
            List<konto> konta = db.konto.Local.ToList();
            List<UserRecord> rekordy = new List<UserRecord>();
            foreach (konto k in konta)
            {
                rekordy.Add(new UserRecord(k));
            }
            userRecordViewSource.Source = rekordy.OrderBy(r => r.id);
        }

        private void OdswiezPracownikow()
        {
            System.Windows.Data.CollectionViewSource pracownikViewSource =
                ((System.Windows.Data.CollectionViewSource)(this.FindResource("pracownikViewSource")));
            List<pracownik> pr = db.pracownik.Where(p => p.konto.Count == 0).ToList();
            pracownikViewSource.Source = pr;
        }
    }
}
