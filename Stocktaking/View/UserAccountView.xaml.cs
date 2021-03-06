﻿using System;
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
    //klasa pomocniczna, zluzy do wyswietlania danych o
    //uzytkownikach
    //poslugiwanie sie klasa pomocnicza ulatwia implementacje GUI
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

    //kontrolka znajdujaca sie w zakladce do zarzadzania uzytkownikami
    public partial class UserAccountView : UserControl
    {
        private StocktakingDatabaseEntities db = null;
        private bool loadUI = true;

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        //konstruktor
        public UserAccountView()
        {
            InitializeComponent();
        }  

        //metoda uruchamiajaca sie wtedy gdy zostanie zmieniona widocznosc zakladki oraz gdy
        //uzytkownik przejdzie na ta zakladke
        //ladowane tutaj sa rekordy do gridow oraz inne typy do comboboxa
        private async void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    return;
                db = ViewLogic.dbContext;
                StocktakingViewModel.Stocktaking.SelectedTab = Tab.UsrAcc; 
                if (db == null || loadUI == false)
                    return;

                System.Windows.Data.CollectionViewSource userRecordViewSource =
                    ((System.Windows.Data.CollectionViewSource)(this.FindResource("userRecordViewSource")));
                List<UserRecord> rekordy = new List<UserRecord>();
                await db.konto.LoadAsync();//operacja asynchroniczna
                List<konto> konta = db.konto.Local.ToList();
                foreach (konto k in konta)
                {
                    rekordy.Add(new UserRecord(k));
                }
                userRecordViewSource.Source = rekordy.OrderBy(r => r.id);

                System.Windows.Data.CollectionViewSource konto_typViewSource =
                    ((System.Windows.Data.CollectionViewSource)(this.FindResource("konto_typViewSource")));
                await db.konto_typ.LoadAsync();//operacja asynchroniczna
                konto_typViewSource.Source = db.konto_typ.Local.ToBindingList().OrderBy(t => t.id);

                typComboBox.ItemsSource = db.konto_typ.Local.ToList().OrderBy(t => t.id);

                System.Windows.Data.CollectionViewSource pracownikViewSource =
                    ((System.Windows.Data.CollectionViewSource)(this.FindResource("pracownikViewSource")));
                pracownikViewSource.Source = await db.pracownik.Where(p => p.konto.Count == 0).ToListAsync();//operacja asnychroniczna

                loadUI = false;
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w UserControl_IsVisibleChanged!");
            }
        }

        //metoda przycisku zmiany loginu
        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz zmienić login użytkownika?"))
                    return;

                konto k = ((UserRecord)userRecordDataGrid.SelectedItem).konto;

                k.login = zaznaczonyLoginTextBox.Text;

                await db.SaveChangesAsync();

                OdswiezKonta();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w loginButton_Click!");
            }
        }

        //metoda przycisku usuwania uzytkwonika
        private async void usunButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz usunąć konto użytkownika?"))
                    return;

                konto k = ((UserRecord)userRecordDataGrid.SelectedItem).konto;
                if(k == StocktakingViewModel.Stocktaking.User)
                {
                    ViewLogic.Blad("Nie możesz usunąć swojego konta!");
                    return;
                }

                db.konto.Remove(k);
                await db.SaveChangesAsync();

                System.Windows.Data.CollectionViewSource userRecordViewSource =
                    ((System.Windows.Data.CollectionViewSource)(this.FindResource("userRecordViewSource")));
                await db.konto.LoadAsync();
                List<konto> konta = db.konto.Local.ToList();
                List<UserRecord> rekordy = new List<UserRecord>();
                foreach (konto kk in konta)
                {
                    rekordy.Add(new UserRecord(kk));
                }
                userRecordViewSource.Source = rekordy.OrderBy(r => r.id);

                System.Windows.Data.CollectionViewSource pracownikViewSource =
                ((System.Windows.Data.CollectionViewSource)(this.FindResource("pracownikViewSource")));
                pracownikViewSource.Source = await db.pracownik.Where(p => p.konto.Count == 0).ToListAsync();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w usunButton_Click!");
            }
        }

        //metoda zmiany typu uztkownika
        private async void typButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz zmienić typ użytkownika?"))
                    return;

                konto k = ((UserRecord)userRecordDataGrid.SelectedItem).konto;

                konto_typ wybranyTyp = (konto_typ)typKontaComboBox.SelectedItem;
                k.konto_typ = wybranyTyp;

                await db.SaveChangesAsync();

                OdswiezKonta();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w typButton_Click!");
            }
        }

        //metoda zmiany hasla uzytkownika
        private async void hasloButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                konto k = ((UserRecord)userRecordDataGrid.SelectedItem).konto;
                new UserAccountViewSubWindows.ChangePassword(k).ShowDialog();

                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w hasloButton_Click!");
            }
        }

        //metoda dodawania nowego uzytkownika
        private async void dodajButton_Click(object sender, RoutedEventArgs e)
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
                bool loginZajety = await db.konto.AnyAsync(k => k.login == nowyLogin);
                if (loginZajety)
                {
                    ViewLogic.Blad("Isnieje już użytkownik o podanym loginie!");
                    hasloPassBox.Clear();
                    powtorzPassBox.Clear();
                    return;
                }

                string haslo = hasloPassBox.Password;
                string haslo2 = powtorzPassBox.Password;
                if (haslo != haslo2)
                {
                    ViewLogic.Blad("Wprowadzone hasło nie jest jednakowe!");
                    hasloPassBox.Clear();
                    powtorzPassBox.Clear();
                    return;
                }

                byte[] sha = ViewLogic.ObliczSHA(haslo);

                int noweId = 1;
                await db.konto.LoadAsync();
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
                await db.SaveChangesAsync();

                nowyLoginTextBox.Clear();
                hasloPassBox.Clear();
                powtorzPassBox.Clear();
                typComboBox.SelectedItem = null;

                System.Windows.Data.CollectionViewSource userRecordViewSource =
                       ((System.Windows.Data.CollectionViewSource)(this.FindResource("userRecordViewSource")));
                await db.konto.LoadAsync();
                List<konto> konta = db.konto.Local.ToList();
                List<UserRecord> rekordy = new List<UserRecord>();
                foreach (konto k in konta)
                {
                    rekordy.Add(new UserRecord(k));
                }
                userRecordViewSource.Source = rekordy.OrderBy(r => r.id);

                System.Windows.Data.CollectionViewSource pracownikViewSource =
                ((System.Windows.Data.CollectionViewSource)(this.FindResource("pracownikViewSource")));
                pracownikViewSource.Source = await db.pracownik.Where(pp => pp.konto.Count == 0).ToListAsync();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w dodajButton_Click!");
            }
        }

        //metoda wywoluje sie przy zmianie zaznaczenia w siatce z uzytkownikami
        //w comboBoxie wybierany jest nady typ uzytkownika
        private void userRecordDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                konto k = ((UserRecord)userRecordDataGrid.SelectedItem).konto;
                if (k != null)
                    typKontaComboBox.SelectedItem = k.konto_typ;
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w userRecordDataGrid_SelectionChanged!");
            }
        }

        //metoda pomocnicza odswiezajaca konta
        private async void OdswiezKonta()
        {
            System.Windows.Data.CollectionViewSource userRecordViewSource =
                       ((System.Windows.Data.CollectionViewSource)(this.FindResource("userRecordViewSource")));
            await db.konto.LoadAsync();
            List<konto> konta = db.konto.Local.ToList();
            List<UserRecord> rekordy = new List<UserRecord>();
            foreach (konto k in konta)
            {
                rekordy.Add(new UserRecord(k));
            }
            userRecordViewSource.Source = rekordy.OrderBy(r => r.id);
        }

        //metoda pomocniczna odswiezajaca pracownikow
        private async void OdswiezPracownikow()
        {
            System.Windows.Data.CollectionViewSource pracownikViewSource =
                ((System.Windows.Data.CollectionViewSource)(this.FindResource("pracownikViewSource")));
            pracownikViewSource.Source = await db.pracownik.Where(p => p.konto.Count == 0).ToListAsync();
        }
    }
}
