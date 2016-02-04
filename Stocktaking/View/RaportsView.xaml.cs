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
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;
using Stocktaking.ViewModel;
using Stocktaking.Data;
using Stocktaking.View.RaportsViewSubWindows;


namespace Stocktaking.View
{
    public partial class RaportsView : UserControl
    {
        private StocktakingDatabaseEntities db = null;
        private konto userAcc = null;
        private bool loadUI = true;

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        public RaportsView()
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
                StocktakingViewModel.Stocktaking.SelectedTab = Tab.Raports;
                if (db == null || loadUI == false)
                    return;

                userAcc = StocktakingViewModel.Stocktaking.User;

                if (!String.IsNullOrWhiteSpace(TypeTextBlock.Text))
                {
                    if (Room.IsChecked == true)
                    {
                        Room_Checked(this, null);
                    }
                    else if (Worker.IsChecked == true)
                    {
                        Worker_Checked(this, null);
                    }
                    else if (Device.IsChecked == true)
                    {
                        Device_Checked(this, null);
                    }
                    else if (RoomMan.IsChecked == true)
                    {
                        RoomMan_Checked(this, null);
                    }
                    else if (DeviceMan.IsChecked == true)
                    {
                        DeviceMan_Checked(this, null);
                    }
                }

                int Type = userAcc.konto_typ_id;
                switch (Type)
                {
                    case 2://Dyrektor zakładu
                        managerUI();
                        break;
                    case 3:// Kierownik instytutu
                        chiefUI();
                        break;
                    default:
                        break;
                }
                upData();
                loadUI = false;
            }
            catch (Exception)
            {
                
                throw;
            }
        }
       
        // zmienia ui pod dyrektora zakładu
        private void managerUI()
        {
            RoomMan.IsChecked = true;
            RadioButtonChief.Visibility = Visibility.Collapsed;
            RadioButtonManager.Visibility = Visibility.Visible;
            InstituteDatagrid.Visibility = Visibility.Visible;
            InstituteDatagrid.ItemsSource = getMyInstituteData();
        }

        //zmienia ui pod dyrektora instytutu
        private void chiefUI()
        {
            Worker.IsChecked = true;
            RadioButtonChief.Visibility = Visibility.Visible;
            RadioButtonManager.Visibility = Visibility.Collapsed;
            InstituteDatagrid.Visibility = Visibility.Collapsed;
        }

        // przeładowanie danych
        private void upData()
        {          
            TypeTextBlock.Text = userAcc.konto_typ.nazwa;// o to chodzilo ?       
            NameTextBlock.Text = userAcc.pracownik.imie + " " + userAcc.pracownik.nazwisko;
            GenerateRaportButton.IsEnabled = false;
            PathTextBox.Text = "";
            fileName = null;
        }

        private string fileName = "";
        //zapisuje do fileName ścieżke do pliku
        private void GetPathButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog mySaveFileDialog = new SaveFileDialog();
            mySaveFileDialog.InitialDirectory = @"c:\";
            mySaveFileDialog.Filter = "Pliki tekstowe (*.txt)|*.txt";
            var temp = mySaveFileDialog.ShowDialog();
            if (temp.Value == true)
            {
                fileName = mySaveFileDialog.FileName;
                PathTextBox.Text = fileName;
                GenerateRaportButton.IsEnabled = true;
            }

        }

        //generuje raporty w zależności kto sie zalogował
        private void GenerateRaportButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(PathTextBox.Text))
            {
                return;
            }
            int Type = userAcc.konto_typ_id;
            switch (Type)
            {
                case 2://Dyrektor zakładu
                    managerRaport();
                    break;
                case 3:// Kierownik instytutu
                    chiefRaport();
                    break;
                default:
                    MessageBox.Show("Coś poszło nie tak.");
                    break;
            }
            MessageBox.Show("Raport wygenerowano i zapisano.");
        }

        // delegat potrzebny do przesyłania funkcji które zwracają dane do tworzenia raportów
        private delegate dynamic getData();

        // tworzenie raportu dla dyrektora instytutu
        private void chiefRaport()
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                string myRaport = "Raport wygenerował " + TypeTextBlock.Text + ": " + NameTextBlock.Text + "\r\n";
                myRaport += "dnia: " + DateTime.Now;
                getData getMyData = new getData(GetInstitue);
                myRaport += createRaportFromInstitute(getMyData);
                getMyData = GetRoom;
                myRaport += createRaportFromRoom(getMyData);
                getMyData = GetDevice;
                myRaport += createRaportFromDevice(getMyData);
                sw.Write(myRaport);
                saveRaport(myRaport);
            }
        }

        // tworzenie raportu dla kierownika zakładu
        private void managerRaport()
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                string myRaport = "Raport wygenerował " + TypeTextBlock.Text + ": " + NameTextBlock.Text + "\r\n";
                myRaport += "dnia: " + DateTime.Now;
                getData getMyData = new getData(getMyInstituteData);
                myRaport += createRaportFromInstitute(getMyData);
                getMyData = GetRoomInstitute;
                myRaport += createRaportFromRoom(getMyData);
                getMyData = GetDeviceInstitute;
                myRaport += createRaportFromDevice(getMyData);
                sw.Write(myRaport);
                saveRaport(myRaport);
            }
        }

        // tworzenie raportu z zakładów
        private string createRaportFromInstitute(getData tem)
        {

            var institute = tem();
            string temp, t1, t2;
            string myRap = "\r\nId".PadRight(7) + "Nazwa".PadRight(50) + "Kierownik".PadRight(30) + "Pracownicy".PadRight(15)
                    + "Sale".PadRight(15);
            foreach (var item in institute)
            {
                t1 = item.Id.ToString();
                t2 = item.Pracownicy.ToString();
                temp = t1.PadRight(5) + item.Nazwa.PadRight(50) + item.Kierownik.PadRight(30) + t2.PadRight(15) + item.Sale;
                myRap += "\r\n";
                myRap += temp;
            }
            myRap += "\r\n";
            return myRap;
        }
        //tworzenie raportów z sal
        private string createRaportFromRoom(getData tem)
        {
            var room = tem();
            string temp, t1, t2, t3, t4, t5;
            string myRap = "\r\nId".PadRight(7) + "Rodzaj".PadRight(40) + "Liczba".PadRight(15) + "Pojemność".PadRight(15)
                            + "Pracownicy".PadRight(15) + "Sprzęt".PadRight(15);

            foreach (var item in room)
            {
                t1 = item.Id.ToString();
                t2 = item.Liczba.ToString();
                if (item.Pojemność != null)
                    t3 = item.Pojemność.ToString();
                else
                    t3 = "-";
                t4 = item.Pracownicy.ToString();
                t5 = item.Sprzęt.ToString();
                temp = t1.PadRight(5) + item.Rodzaj.PadRight(40) + t2.PadRight(15) + t3.PadRight(15) + t4.PadRight(15) + t5.PadRight(15);
                myRap += "\r\n";
                myRap += temp;
            }
            myRap += "\r\n";
            return myRap;
        }
        // tworzenie raportu z sprzętu
        private string createRaportFromDevice(getData tem)
        {
            var device = tem();
            string temp, t1, t2;
            string myRap = "\r\nId".PadRight(7) + "Typ".PadRight(40) + "Liczba".PadRight(15);
            foreach (var item in device)
            {
                t1 = item.Id.ToString();
                t2 = item.Liczba.ToString();
                temp = t1.PadRight(5) + item.Typ.PadRight(40) + t2.PadRight(15);
                myRap += "\r\n";
                myRap += temp;
            }
            return myRap;
        }

        // zapis myRaport do bazy
        private void saveRaport(string myRaport)
        {
            int newId = 1;
            if (db.raport.Any())
                newId = 1 + db.raport.Max(r => r.id);
            raport myR = new raport()
            {
                id = newId,
                data = DateTime.Now,
                imie_nazwisko = NameTextBlock.Text,
                konto_id = userAcc.id,
                raport1 = myRaport
            };
            db.raport.Add(myR);
            db.SaveChanges();
        }

        // gdy radio button sprzęt to przypisuje dane
        private void Device_Checked(object sender, RoutedEventArgs e)
        {
            RaportDatagrid.ItemsSource = GetDevice();
        }
        // gdy radio button sale to przypisuje dane
        private void Room_Checked(object sender, RoutedEventArgs e)
        {
            RaportDatagrid.ItemsSource = GetRoom();
        }
        // gdy radio button zakłady to przypisuje dane
        private void Worker_Checked(object sender, RoutedEventArgs e)
        {
            RaportDatagrid.ItemsSource = GetInstitue();
        }
        //gdy radio button sprzęt to przypisuje dane-dyrektor zakładu
        private void DeviceMan_Checked(object sender, RoutedEventArgs e)
        {
            RaportDatagrid.ItemsSource = GetDeviceInstitute();
        }
        //gdy radio button sale to przypisuje dane-dyrektor zakładu
        private void RoomMan_Checked(object sender, RoutedEventArgs e)
        {
            RaportDatagrid.ItemsSource = GetRoomInstitute();
        }

        //zwraca dane o zakładzie w którym jest kierownik zakładu
        private dynamic getMyInstituteData()
        {
            zaklad zak = DataFunctions.GetZaklad(userAcc.pracownik);
            var Institute = (from z in db.zaklad
                             where z.id == zak.id
                             select new
                             {
                                 Id = z.id,
                                 Nazwa = z.nazwa,
                                 Kierownik = z.pracownik.imie + " " + z.pracownik.nazwisko,
                                 Pracownicy = db.pracownik.Count(p => p.sala.zaklad_id == z.id),
                                 Sale = db.sala.Count(s => s.zaklad_id == z.id)
                             }).ToList();
            return Institute;
        }
        // zwraca dane o salach w zakładzie
        private dynamic GetRoomInstitute()
        {
            zaklad zak = DataFunctions.GetZaklad(userAcc.pracownik);
            var Rooms = (from s in db.sala_typ
                         select new
                         {
                             Id = s.id,
                             Rodzaj = s.typ_sali,
                             Liczba = db.sala.Count(sa => sa.sala_typ_id == s.id && sa.zaklad_id == zak.id),
                             Pojemność = (int?)db.sala.Where(sa => sa.sala_typ_id == s.id && sa.zaklad_id == zak.id).Sum(a => a.pojemnosc),
                             Pracownicy = db.pracownik.Count(p => p.sala.sala_typ_id == s.id && p.sala.zaklad_id == zak.id),
                             Sprzęt = db.sprzet.Count(sp => sp.sala.sala_typ_id == s.id && sp.sala.zaklad_id == zak.id)
                         }).ToList();
            Rooms.Add(new
            {
                Id = 0,
                Rodzaj = "Suma",
                Liczba = Rooms.Sum(a => a.Liczba),
                Pojemność = Rooms.Sum(s => s.Pojemność),
                Pracownicy = Rooms.Sum(p => p.Pracownicy),
                Sprzęt = Rooms.Sum(s => s.Sprzęt)
            });
            return Rooms;
        }
        // zwraca dane o sprzecie w zakladzie 
        private dynamic GetDeviceInstitute()
        {
            zaklad zak = DataFunctions.GetZaklad(userAcc.pracownik);

            var Devices = (from d in db.sprzet_typ
                           select new
                           {
                               Id = d.id,
                               Typ = d.typ_sprzetu,
                               Liczba = db.sprzet.Count(a => a.sprzet_typ_id == d.id && a.sala.zaklad_id == zak.id),
                           }).ToList();
            Devices.Add(new
            {
                Id = 0,
                Typ = "Suma",
                Liczba = Devices.Sum(a => a.Liczba),
            });
            return Devices;
        }
        //zwraca dane o zakładach dla instytutu
        private dynamic GetInstitue()
        {
            var Institute = (from z in db.zaklad
                             select new
                             {
                                 Id = z.id,
                                 Nazwa = z.nazwa,
                                 Kierownik = z.pracownik.imie + " " + z.pracownik.nazwisko,
                                 Pracownicy = db.pracownik.Count(p => p.sala.zaklad_id == z.id),
                                 Sale = db.sala.Count(s => s.zaklad_id == z.id)
                             }).ToList();
            Institute.Add(new
            {
                Id = 0,
                Nazwa = "Suma",
                Kierownik = " ",
                Pracownicy = Institute.Sum(a => a.Pracownicy),
                Sale = Institute.Sum(a => a.Sale)
            });
            return Institute;
        }
        // zwraca dane o salach dla instytutu
        private dynamic GetRoom()
        {
            var Rooms = (from s in db.sala_typ
                         select new
                         {
                             Id = s.id,
                             Rodzaj = s.typ_sali,
                             Liczba = db.sala.Count(sa => sa.sala_typ_id == s.id),
                             Pojemność = db.sala.Where(sa => sa.sala_typ_id == s.id).Sum(a => a.pojemnosc),
                             Pracownicy = db.pracownik.Count(p => p.sala.sala_typ_id == s.id),
                             Sprzęt = db.sprzet.Count(sp => sp.sala.sala_typ_id == s.id)
                         }).ToList();
            Rooms.Add(new
            {
                Id = 0,
                Rodzaj = "Suma",
                Liczba = Rooms.Sum(a => a.Liczba),
                Pojemność = Rooms.Sum(s => s.Pojemność),
                Pracownicy = Rooms.Sum(p => p.Pracownicy),
                Sprzęt = Rooms.Sum(s => s.Sprzęt)
            });
            return Rooms;
        }
        // zwraca dane o sprzecie 
        private dynamic GetDevice()
        {
            var Devices = (from d in db.sprzet_typ
                           select new
                           {
                               Id = d.id,
                               Typ = d.typ_sprzetu,
                               Liczba = db.sprzet.Count(a => a.sprzet_typ_id == d.id),
                           }).ToList();
            Devices.Add(new
            {
                Id = 0,
                Typ = "Suma",
                Liczba = Devices.Sum(a => a.Liczba),
            });
            return Devices;
        }
        
        // otwarcie nowego okna w celu wczytaniu do pliki starego raportu
        private void GenerateOldRaportButton_Click(object sender, RoutedEventArgs e)
        {
            var mySelectOldRaport = new SelectOldRaport(db);
            mySelectOldRaport.ShowDialog();
        }


    }
}
