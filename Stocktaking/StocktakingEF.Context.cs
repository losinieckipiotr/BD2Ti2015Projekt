﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Stocktaking
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class StocktakingDatabaseEntities : DbContext
    {
        public StocktakingDatabaseEntities()
            : base("name=StocktakingDatabaseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<konto> konto { get; set; }
        public virtual DbSet<konto_typ> konto_typ { get; set; }
        public virtual DbSet<pracownik> pracownik { get; set; }
        public virtual DbSet<raport> raport { get; set; }
        public virtual DbSet<sala> sala { get; set; }
        public virtual DbSet<sala_typ> sala_typ { get; set; }
        public virtual DbSet<sprzet> sprzet { get; set; }
        public virtual DbSet<sprzet_typ> sprzet_typ { get; set; }
        public virtual DbSet<zaklad> zaklad { get; set; }
    }
}
