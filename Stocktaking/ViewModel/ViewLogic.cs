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
    //Klasa zaimplementowana jako Singleton, przechowuje baze danych,
    //posiada kilka metod pomocniczych, ktore przydaja sie przy implemetacji GUI
    static class ViewLogic
    {
        static private StocktakingDatabaseEntities db = null;

        //kontekst bazy danych
        static public StocktakingDatabaseEntities dbContext { get { return db; } }

        //inicjalizaca bazy
        static public void InitDataBase()
        {
            if(db == null)
                db = new StocktakingDatabaseEntities();
        }

        //usuniecie kontekstu bazy
        static public void DisposeDatabase()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }

        //obliczanie SHA512
        static public byte[] ObliczSHA(string haslo)
        {
            SHA512 shaM = new SHA512Managed();
            byte[] sha = shaM.ComputeHash(Encoding.Unicode.GetBytes(haslo));
            return sha;
        }

        //MessageBox  proszacy o potwierdzenie
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

        //MessaheBox infomujacy o bledzie
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
