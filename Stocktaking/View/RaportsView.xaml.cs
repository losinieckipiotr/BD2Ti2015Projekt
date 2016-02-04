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
using System.Data.Entity;
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
        private async void managerUI()
        {
            RoomMan.IsChecked = true;
            RadioButtonChief.Visibility = Visibility.Collapsed;
            RadioButtonManager.Visibility = Visibility.Visible;
            InstituteDatagrid.Visibility = Visibility.Visible;
            InstituteDatagrid.ItemsSource = await getMyInstituteData();
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
            try
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
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w GetPathButton_Click!");
            }
        }

        //generuje raporty w zależności kto sie zalogował
        private void GenerateRaportButton_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception)
            {
                
                ViewLogic.Blad("Wystapił bład w GetPathButton_Click!");
            }
        }

        // tworzenie raportu dla dyrektora instytutu
        private async void chiefRaport()
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                string myRaport = "Raport wygenerował " + TypeTextBlock.Text + ": " + NameTextBlock.Text + "\r\n";
                myRaport += "dnia: " + DateTime.Now;
                myRaport += createRaportFromInstitute(await GetInstitue());
                myRaport += createRaportFromRoom(await GetRoom());
                myRaport += createRaportFromDevice(await GetDevice());
                await sw.WriteAsync(myRaport);
                saveRaport(myRaport);
            }
        }

        // tworzenie raportu dla kierownika zakładu
        private async void managerRaport()
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                string myRaport = "Raport wygenerował " + TypeTextBlock.Text + ": " + NameTextBlock.Text + "\r\n";
                myRaport += "dnia: " + DateTime.Now;
                myRaport += createRaportFromInstitute(await getMyInstituteData());
                myRaport += createRaportFromRoom(await GetRoomInstitute());
                myRaport += createRaportFromDevice(await GetDeviceInstitute());
                await sw.WriteAsync(myRaport);
                saveRaport(myRaport);
            }
        }

        // tworzenie raportu z zakładów
        private string createRaportFromInstitute(dynamic institute)
        {
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
        private string createRaportFromRoom(dynamic room)
        {
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
        private string createRaportFromDevice(dynamic device)
        {
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
        private async void saveRaport(string myRaport)
        {
            int newId = 1;
            bool result = await db.raport.AnyAsync();
            if (result)
                newId = 1 + await db.raport.MaxAsync(r => r.id);
            raport myR = new raport()
            {
                id = newId,
                data = DateTime.Now,
                imie_nazwisko = NameTextBlock.Text,
                konto_id = userAcc.id,
                raport1 = myRaport
            };
            db.raport.Add(myR);
            await db.SaveChangesAsync();
        }

        // gdy radio button sprzęt to przypisuje dane
        private async void Device_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RaportDatagrid.ItemsSource = await GetDevice();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Device_Checked!");
            }
            
        }
        // gdy radio button sale to przypisuje dane
        private async void Room_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RaportDatagrid.ItemsSource = await GetRoom();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Room_Checked!");
            }
        }
        // gdy radio button zakłady to przypisuje dane
        private async void Worker_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RaportDatagrid.ItemsSource = await GetInstitue();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w Worker_Checked!");
            }
        }
        //gdy radio button sprzęt to przypisuje dane-dyrektor zakładu
        private async void DeviceMan_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RaportDatagrid.ItemsSource = await GetDeviceInstitute();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w DeviceMan_Checked!");
            }
        }
        //gdy radio button sale to przypisuje dane-dyrektor zakładu
        private async void RoomMan_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RaportDatagrid.ItemsSource = await GetRoomInstitute();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w RoomMan_Checked!");
            }
        }

        //zwraca dane o zakładzie w którym jest kierownik zakładu
        private Task<dynamic> getMyInstituteData()
        {
            Task<dynamic> t = new Task<dynamic>(() =>
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
            });
            t.Start();
            return t;
        }
        // zwraca dane o salach w zakładzie
        private Task<dynamic> GetRoomInstitute()
        {
            Task<dynamic> t = new Task<dynamic>(() =>
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
            });
            t.Start();
            return t;
        }
        // zwraca dane o sprzecie w zakladzie 
        private Task<dynamic> GetDeviceInstitute()
        {
            Task<dynamic> t = new Task<dynamic>(() =>
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
            });
            t.Start();
            return t;
        }
        //zwraca dane o zakładach dla instytutu
        private Task<dynamic> GetInstitue()
        {
            Task<dynamic> t = new Task<dynamic>(() =>
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
            });
            t.Start();
            return t;
        }
        // zwraca dane o salach dla instytutu
        private Task<dynamic> GetRoom()
        {
            Task<dynamic> t = new Task<dynamic>(() =>
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
            });
            t.Start();
            return t;
        }
        // zwraca dane o sprzecie 
        private dynamic GetDevice()
        {
            Task<dynamic> t = new Task<dynamic>(() =>
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
            });
            t.Start();
            return t;
        }
        
        // otwarcie nowego okna w celu wczytaniu do pliki starego raportu
        private void GenerateOldRaportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mySelectOldRaport = new SelectOldRaport(db);
                mySelectOldRaport.ShowDialog();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w GenerateOldRaportButton_Click!");
                throw;
            }
        }
    }
}
