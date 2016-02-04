using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.Entity;

namespace Stocktaking.ViewModel
{
    //Klasa zaimplementowana jako Singleton zarzadzajaca GUI,
    //decyduje o widocznosci zakladek, wyswietla 
    //konkretne zakladki w zaleznosci od tego ktrou uzytkownik sie zalogowal.
    //Przechowuje konto zalogowanego uzytkownika oraz glowne okno aplikacji.
    class StocktakingViewModel
    {
        private Tab selectedTab = Tab.None;
        private StocktakingDatabaseEntities db = null;
        private MainWindow win = null;
        private konto userAcc = null;
        private Task initDbTask = null;
        private static StocktakingViewModel stocktaking = null;

        public static StocktakingViewModel Stocktaking { get { return stocktaking; } }//instancja
        public MainWindow Window { get { return win; } }//okno glowne
        public konto User { get { return userAcc; } }//zalogowany uzytkownik

        public Tab SelectedTab { get { return selectedTab; } set { selectedTab = value; } }

        //metoda tworzaca instancje klasy
        public static void CreateStocktaking(MainWindow win)
        {
            if (stocktaking == null)
                stocktaking = new StocktakingViewModel(win);
        }

        //kontruktor, inicjalizuje baze danych
        private StocktakingViewModel(MainWindow win)
        {
            this.win = win;
            InitDbAsync();
        }

        //Metoda odpowiada za logowanie, wyszukuje czy istnieje uzytkownik o danym loginie.
        //Oblicza SHA-512 dla wpisanego hasla i porownuje z danymi w bazie
        public bool NewLogin(string login, string password)
        {
            try
            {
                initDbTask.Wait();
                Task<konto> tempAcc = db.konto.SingleOrDefaultAsync(o => o.login == login);

                byte[] passwordAfter = ViewLogic.ObliczSHA(password);
                tempAcc.Wait();
                if (tempAcc.Result == null)
                {
                    ViewLogic.Blad("Błędny login lub hasło");
                    return false;
                }
                byte[] passwordFromDb = tempAcc.Result.haslo;
                for (int i = 0; i < 64; i++)
                {
                    if(passwordAfter[i] != passwordFromDb[i])
                    {
                        ViewLogic.Blad("Błędny login lub hasło");
                        userAcc = null;
                        return false;
                    }
                }

                userAcc = tempAcc.Result;
                UpdataWindow();
                return true;
            }
            catch (Exception)
            {
                userAcc = null;
                return false;
            }
        }

        //wylogowanie, usuniecie niepotrzebnych danych
        public void Logout()
        {
            userAcc = null;
            db = null;
            ViewLogic.DisposeDatabase();
            InitDbAsync();
        }

        public void OpenHelp()
        {
            if (selectedTab != Tab.None)
            {
                string myPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Help\", "StocktakingHelp.chm");
                System.Diagnostics.Process.Start(myPath);
            }
            
        }

        //inicjalizacja bazy danych w innym watku
        private void InitDbAsync()
        {
            initDbTask = new Task(() =>
            {
                ViewLogic.InitDataBase();
                db = ViewLogic.dbContext;
            });
            initDbTask.Start();
        }

        //metoda pomocniczna, pozwala na wymuszenie przeladowania zakladek
        public void RealoadTabs(
            bool dictionaryTab = false,
            bool instituteDevicesTab = false,
            bool instituteManagmentTab = false,
            bool instituteWorkersTab = false,
            bool raportsTab = false,
            bool roomsTab = false,
            bool userAccountTab = false)
        {
            win.DictionaryControl.LoadUI = dictionaryTab;
            win.InstituteDevicesControl.LoadUI = instituteDevicesTab;
            win.InstituteManagementControl.LoadUI = instituteManagmentTab;
            win.InstituteWorkersControl.LoadUI = instituteWorkersTab;
            win.RaportsControl.LoadUI = raportsTab;
            win.RoomsControl.LoadUI = roomsTab;
            win.UserAccountControl.LoadUI = userAccountTab;
        }

        //metoda decyduje o widocznosci konkretnych zakladek w zaleznosci od tego jaki uzytkownik jest zalogowany
        private void UpdataWindow()
        {
            switch (userAcc.konto_typ_id) 
                
            {
                case 1://Administrator
                    win.UserAccountControl.LoadUI = true;
                    win.DictionaryControl.LoadUI = true;
                    win.ChangeVisibility(
                        index: 0,
                        Tab: Visibility.Visible,
                        UserAcc: Visibility.Visible,
                        DictionaryVis: Visibility.Visible);
                    break;
                case 2://Dyrektor zakładu
                    win.InstituteManagementControl.LoadUI = true;
                    win.RaportsControl.LoadUI = true;
                    win.ChangeVisibility(
                        index:2,
                        Tab: Visibility.Visible,
                        InstituteManagmentVis: Visibility.Visible,
                        RaportsVis: Visibility.Visible);

                    break;
                case 3://kierownik instytutu
                    win.InstituteManagementControl.LoadUI = true;
                    win.RoomsControl.LoadUI = true;
                    win.InstituteWorkersControl.LoadUI = true;
                    win.RaportsControl.LoadUI = true;
                    win.ChangeVisibility(
                        index: 1,
                        Tab: Visibility.Visible,
                        InstituteManagmentVis: Visibility.Visible,
                        RoomsVis: Visibility.Visible,
                        InstituteDevicesList: Visibility.Visible,
                        InstituteWorkersList: Visibility.Visible, 
                        RaportsVis: Visibility.Visible);
                    break;
                case 4://zakładowy pracownik techniczny
                    win.InstituteManagementControl.LoadUI = true;
                    win.ChangeVisibility(index: 2,
                        Tab: Visibility.Visible,
                        InstituteManagmentVis: Visibility.Visible);
                    break;
                case 5:// instytutowy pracownik techniczny
                    win.RoomsControl.LoadUI = true;
                    win.ChangeVisibility(index: 1,
                        Tab: Visibility.Visible,
                        RoomsVis: Visibility.Visible,
                        InstituteDevicesList: Visibility.Visible);
                    break;
                default:
                    break;
            }
        }
    }

    enum Tab { None, UsrAcc, Rooms, InstMan, Dict, Devs, Workers, Raports }
}
