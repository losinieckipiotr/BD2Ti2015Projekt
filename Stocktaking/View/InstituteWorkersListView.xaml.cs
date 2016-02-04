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
    /// <summary>
    /// Interaction logic for InstituteWorkersListView.xaml
    /// </summary>
    /// 

    class WorkerRecord
    {
        public int id { get; set; }
        public string imie { get; set; }
        public string nazwisko { get; set; }
        public string sala { get; set; }
        public string zaklad { get; set; }
        public pracownik pracownik { get; set; }

        public WorkerRecord(pracownik p)
        {
            this.id = p.id;
            this.imie = p.imie;
            this.nazwisko = p.nazwisko;
            if (p.sala != null)
            {
                this.sala = p.sala.numer.ToString();
                if (p.sala.zaklad != null)
                    this.zaklad = p.sala.zaklad.nazwa;
                else
                    this.zaklad = "Brak zakładu";
            }
            else
            {
                this.sala = "Brak sali";
                this.zaklad = "Brak zakładu";
            }
            this.pracownik = p;
        }
    }

    public partial class InstituteWorkersListView : UserControl
    {
        private StocktakingDatabaseEntities db = null;
        private bool loadUI = true;

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        public InstituteWorkersListView()
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
                StocktakingViewModel.Stocktaking.SelectedTab = Tab.Workers;
                if (db == null || loadUI == false)
                    return;

                System.Windows.Data.CollectionViewSource workerRecordViewSource =
                    ((System.Windows.Data.CollectionViewSource)(this.FindResource("workerRecordViewSource")));
                await db.pracownik.LoadAsync();
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
                ViewLogic.Blad("Wystapił bład w UserControl_IsVisibleChanged!");
            }
        }

        private async void pracownikUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz zaktualizować dane pracownika?"))
                return;

                pracownik p = ((WorkerRecord)workerRecordDataGrid.SelectedItem).pracownik;
                string newName = imieTextBox.Text;
                string newSurname = nazwiskoTextBox.Text;
                p.imie = newName;
                p.nazwisko = newSurname;

                await db.SaveChangesAsync();

                OdswiezPracownikow();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w pracownikUpdate_Click!");
            }
        }

        private async void pracowikAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                 if (!ViewLogic.Potwierdz("Czy chcesz dodać pracownika?"))
                    return;
                
                string noweImie = noweImieTextBox.Text;
                string noweNazwisko = noweNazwiskoTextBox.Text;
                if (noweImie == "" || noweNazwisko == "")
                {
                    ViewLogic.Blad("Nie podano imienia lub nazwiska!");
                    return;
                }

                int noweId = 1;
                await db.pracownik.LoadAsync();
                foreach (pracownik p in db.pracownik.Local.OrderBy(p => p.id))
                {
                    if (noweId != p.id)
                        break;
                    else
                        ++noweId;
                }

                pracownik nowy = new pracownik
                {
                    id = noweId,
                    imie = noweImie,
                    nazwisko = noweNazwisko
                };
                db.pracownik.Add(nowy);
                await db.SaveChangesAsync();

                noweImieTextBox.Clear();
                noweNazwiskoTextBox.Clear();
                OdswiezPracownikow();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w pracowikAdd_Click!");
            }
        }

        private async void pracownikDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz usunąć pracownika?"))
                    return;

                pracownik p = ((WorkerRecord)workerRecordDataGrid.SelectedItem).pracownik;

                db.pracownik.Remove(p);
                await db.SaveChangesAsync();

                OdswiezPracownikow();
            }
            catch (Exception)
            {
                ViewLogic.Blad("Wystapił bład w pracownikDelete_Click!");
            }
        }

        private async void  OdswiezPracownikow()
        {
            System.Windows.Data.CollectionViewSource workerRecordViewSource =
                   ((System.Windows.Data.CollectionViewSource)(this.FindResource("workerRecordViewSource")));
            await db.pracownik.LoadAsync();
            List<pracownik> pracownicy = db.pracownik.Local.ToList();
            List<WorkerRecord> rekordy = new List<WorkerRecord>();
            foreach (pracownik p in pracownicy)
            {
                rekordy.Add(new WorkerRecord(p));
            }
            workerRecordViewSource.Source = rekordy.OrderBy(r => r.id);

            StocktakingViewModel.Stocktaking.RealoadTabs(
                        raportsTab: true);
        }
    }
}
