using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows;
namespace Stocktaking.ViewModel
{
    class StocktakingViewModel
    {
        private StocktakingDatabaseEntities db = null;
        private MainWindow win = null;
        private konto userAcc = null;
        private static StocktakingViewModel stocktaking = null;

        public static StocktakingViewModel Stocktaking { get { return stocktaking; } }
        public MainWindow Window { get { return win; } }
        public konto GetUser { get { return userAcc; } }
        public bool IsUserLogged { get { return userAcc == null; } }

        #region do instituteManagment:
        //public int GetUserType { get { return userAcc.konto_typ_id; } }
        //public int GetWorkerId { get { return userAcc.pracownik_id; } }
        //public int GetAccId { get { return userAcc.id; } }
        
        public zaklad GetZaklad
        {
            get
            {
                //zaklad ans = db.zaklad.SingleOrDefault
                //    (z => z.id == userAcc.pracownik.sala.zaklad_id);
                //zwraca kierownika a nie pracownika
                //return ans;

                //prosciej
                return userAcc.pracownik.sala.zaklad; 
            }
        }
        #endregion

        public static void CreateStocktaking( MainWindow win)
        {
            stocktaking = new StocktakingViewModel(win);
        }

        public StocktakingViewModel(MainWindow win)
        {
            this.win = win;
        }

        //logowanie
        public bool NewLogin(string login, string password)
        {
            try
            {
                ViewLogic.InitDataBase();
                db = ViewLogic.db;

                SHA512 sha = new SHA512Managed();
                byte[] passwordAfter = sha.ComputeHash(Encoding.Unicode.GetBytes(password));

                var tempAcc = db.konto.SingleOrDefault(o => (o.login == login));
                if (tempAcc == null)
                {
                    wrongPass();
                    return false;
                }
                byte[] passwordFromDb = tempAcc.haslo;

                for (int i = 0; i < 64; i++)
                {
                    if (passwordAfter[i] != passwordFromDb[i])
                    {
                        wrongPass();
                        db = null;
                        return false;
                    }
                }
                userAcc = tempAcc;
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
        }

        private void UpdataWindow()
        {
            switch (userAcc.konto_typ_id)//decyduje które zakładki mają zostać wyświetlone dla poszczególnych kont, 
                //index decyduje która zakładka ma zostac wybrana jako początkowa
            {
                case 1://Administrator
                    win.ChangeVisibility(
                        index: 0,
                        Tab: Visibility.Visible,
                        UserAcc: Visibility.Visible,
                        DictionaryVis: Visibility.Visible);
                    win.UserAccountControl.LoadUI = true;
                    win.DictionaryControl.LoadUI = true;
                    break;
                case 2://Dyrektor zakładu
                    win.ChangeVisibility(
                        index:2,
                        Tab: Visibility.Visible,
                        InstituteManagmentVis: Visibility.Visible,
                        RaportsVis: Visibility.Visible);
                    win.InstituteManagementControl.LoadUI = true;
                    win.RaportsControl.LoadUI = true;
                    break;
                case 3://kierownik instytutu               
                    win.ChangeVisibility(
                        index: 1,
                        Tab: Visibility.Visible,
                        InstituteManagmentVis: Visibility.Visible,
                        RoomsVis: Visibility.Visible,
                        InstituteDevicesList: Visibility.Visible,
                        InstituteWorkersList: Visibility.Visible, 
                        RaportsVis: Visibility.Visible);
                    win.InstituteManagementControl.LoadUI = true;
                    win.RoomsControl.LoadUI = true;
                    win.RaportsControl.LoadUI = true;
                    break;
                case 4://zakładowy pracownik techniczny
                    win.ChangeVisibility(index: 2,
                        Tab: Visibility.Visible,
                        InstituteManagmentVis: Visibility.Visible);
                    win.InstituteManagementControl.LoadUI = true;
                    break;
                case 5:// instytutowy pracownik techniczny
                    win.ChangeVisibility(index: 1,
                        Tab: Visibility.Visible,
                        RoomsVis: Visibility.Visible,
                        InstituteDevicesList: Visibility.Visible);
                    win.RoomsControl.LoadUI = true;
                    break;
                default:
                    break;
            }
        }

        private void wrongPass()
        {
            MessageBox.Show("Błędne hasło lub login.", "Coś poszło nie tak.");
        }

        public void DeleteItemFromKonto(int id)
        {
            var itemToDelete = db.konto.SingleOrDefault(x => x.id == id);
            if (itemToDelete != null)
            {
                db.konto.Remove(itemToDelete);
                db.SaveChanges();
            }
        }

    }
}
