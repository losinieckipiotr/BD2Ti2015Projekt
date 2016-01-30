using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.Entity;

namespace Stocktaking.ViewModel
{
    class StocktakingViewModel
    {
        private StocktakingDatabaseEntities db = null;
        private MainWindow win = null;
        private konto userAcc = null;
        private Task initDbTask = null;
        private static StocktakingViewModel stocktaking = null;

        public static StocktakingViewModel Stocktaking { get { return stocktaking; } }
        public MainWindow Window { get { return win; } }
        public konto User { get { return userAcc; } }
        public bool IsUserLogged { get { return userAcc == null; } }

        public static void CreateStocktaking( MainWindow win)
        {
            stocktaking = new StocktakingViewModel(win);
        }

        public StocktakingViewModel(MainWindow win)
        {
            this.win = win;
            InitDbAsync();
        }

        //logowanie
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
                int[] resultTab = new int[64];
                int result = 0;
                for (int i = 0; i < 64; i++)
                {
                    resultTab[i] = passwordAfter[0] - passwordFromDb[0];
                    result += resultTab[i];
                }
                if (result != 0)
                {
                    ViewLogic.Blad("Błędny login lub hasło");
                    return false;
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

        //wylogowanie
        public void Logout()
        {
            userAcc = null;
            db = null;
            ViewLogic.DisposeDatabase();
            InitDbAsync();
        }

        private void InitDbAsync()
        {
            initDbTask = new Task(() =>
            {
                ViewLogic.InitDataBase();
                db = ViewLogic.dbContext;
            });
            initDbTask.Start();
        }

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

        private void UpdataWindow()
        {
            switch (userAcc.konto_typ_id)//decyduje które zakładki mają zostać wyświetlone dla poszczególnych kont, 
                //index decyduje która zakładka ma zostac wybrana jako początkowa
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
}
