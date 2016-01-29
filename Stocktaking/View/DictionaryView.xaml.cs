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

namespace Stocktaking.View
{
    /// <summary>
    /// Interaction logic for DictionaryView.xaml
    /// </summary>
    /// 
    using ViewModel;

    public partial class DictionaryView : UserControl
    {
        private StocktakingDatabaseEntities db = null;
        private bool loadUI = false;

        public bool LoadUI { get { return loadUI; } set { loadUI = value; } }

        public DictionaryView()
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
                
                System.Windows.Data.CollectionViewSource sprzet_typViewSource =
                   ((System.Windows.Data.CollectionViewSource)(this.FindResource("sprzet_typViewSource")));
                await db.sprzet_typ.LoadAsync();//operacja asynchroniczna
                sprzet_typViewSource.Source = db.sprzet_typ.Local.ToBindingList().OrderBy(t => t.id);
                
                System.Windows.Data.CollectionViewSource sala_typViewSource =
                    ((System.Windows.Data.CollectionViewSource)(this.FindResource("sala_typViewSource")));
                await db.sala_typ.LoadAsync();//operacja asynchroniczna
                sala_typViewSource.Source = db.sala_typ.Local.ToBindingList().OrderBy(s => s.id);

                loadUI = false;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private async void typButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz zmienić nazwę typu?"))
                    return;

                sprzet_typ typ = (sprzet_typ)sprzet_typDataGrid.SelectedItem;
                typ.typ_sprzetu = typTextBox.Text;

                await db.SaveChangesAsync();

                OdswiezSprzet();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void nowyTypButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz dodać nowy typ?"))
                    return;

                string nowyTyp = nowyTypTextBox.Text;
                if (nowyTyp == "")
                {
                    ViewLogic.Blad("Nie podano nazwy typu!");
                    return;
                }
                bool typZajety = await db.sprzet_typ.AnyAsync(t => t.typ_sprzetu == nowyTyp);
                if (typZajety)
                {
                    ViewLogic.Blad("Isnieje już typ o tej nazwie!");
                    return;
                }

                int noweId = 1;
                await db.sprzet_typ.LoadAsync();
                foreach (sprzet_typ t in db.sprzet_typ.Local.OrderBy(t => t.id))
                {
                    if (noweId != t.id)
                        break;
                    else
                        ++noweId;
                }

                sprzet_typ typ = new sprzet_typ
                {
                    id = noweId,
                    typ_sprzetu = nowyTyp
                };
                db.sprzet_typ.Add(typ);
                await db.SaveChangesAsync();

                nowyTypTextBox.Clear();
                OdswiezSprzet();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void usunTypButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz usunąć typ?"))
                    return;

                sprzet_typ typ = (sprzet_typ)sprzet_typDataGrid.SelectedItem;

                if (typ.sprzet.Count != 0)
                {
                    ViewLogic.Blad("Nie można usunąć ponieważ istnieją sprzęty tego typu!");
                    return;
                }

                db.sprzet_typ.Remove(typ);
                await db.SaveChangesAsync();

                OdswiezSprzet();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void salaTypButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz zmienić nazwę typu?"))
                    return;

                sala_typ typ = (sala_typ)sala_typDataGrid.SelectedItem;
                typ.typ_sali = salaTypTextBox.Text;

                await db.SaveChangesAsync();

                OdswiezSale();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void salaNowyTypButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz dodać nowy typ?"))
                    return;

                string nowyTyp = salaNowyTypTextBox.Text;
                if (nowyTyp == "")
                {
                    ViewLogic.Blad("Nie podano nazwy typu!");
                    return;
                }
                bool typZajety = await db.sala_typ.AnyAsync(t => t.typ_sali == nowyTyp);
                if (typZajety)
                {
                    ViewLogic.Blad("Isnieje już typ o tej nazwie!");
                    return;
                }

                int noweId = 1;
                await db.sala_typ.LoadAsync();
                foreach (sala_typ s in db.sala_typ.Local.OrderBy(s => s.id))
                {
                    if (noweId != s.id)
                        break;
                    else
                        ++noweId;
                }

                sala_typ st = new sala_typ
                {
                    id = noweId,
                    typ_sali = nowyTyp
                };
                db.sala_typ.Add(st);
                db.SaveChanges();

                salaNowyTypTextBox.Clear();
                OdswiezSale();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void salaUsunTypButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViewLogic.Potwierdz("Czy chcesz usunąć typ?"))
                    return;

                sala_typ sala = (sala_typ)sala_typDataGrid.SelectedItem;

                if (sala.sala.Count != 0)
                {
                    ViewLogic.Blad("Nie można usunąć ponieważ istnieją sale tego typu!");
                    return;
                }

                db.sala_typ.Remove(sala);
                await db.SaveChangesAsync();

                OdswiezSale();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void OdswiezSprzet()
        {
            System.Windows.Data.CollectionViewSource sprzet_typViewSource =
                ((System.Windows.Data.CollectionViewSource)(this.FindResource("sprzet_typViewSource")));
            await db.sprzet_typ.LoadAsync();
            sprzet_typViewSource.Source = db.sprzet_typ.Local.ToBindingList().OrderBy(t => t.id);
        }

        private async void OdswiezSale()
        {
            System.Windows.Data.CollectionViewSource sala_typViewSource =
                ((System.Windows.Data.CollectionViewSource)(this.FindResource("sala_typViewSource")));
            await db.sala_typ.LoadAsync();
            sala_typViewSource.Source = db.sala_typ.Local.ToBindingList().OrderBy(s => s.id);
        }
    }
}
