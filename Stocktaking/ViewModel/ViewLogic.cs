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
using System.Security.Cryptography;

namespace Stocktaking.ViewModel
{
    static class ViewLogic
    {
        static private StocktakingDatabaseEntities db = null;

        static public StocktakingDatabaseEntities dbContext
        {
            get
            {
                return db;
            }
        }

        static public void InitDataBase()
        {
            if(db == null)
                db = new StocktakingDatabaseEntities();
        }

        static public void DisposeDatabase()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }

        static public byte[] ObliczSHA(string haslo)
        {
            SHA512 shaM = new SHA512Managed();
            byte[] sha = shaM.ComputeHash(Encoding.Unicode.GetBytes(haslo));
            return sha;
        }

        static public bool Potwierdz(string pytanie)
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

        static public void Blad(string wiadomosc)
        {
            MessageBox.Show(
                       wiadomosc,
                       "Błąd",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error);
        }
    }
}
