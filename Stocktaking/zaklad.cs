//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class zaklad
    {
        public zaklad()
        {
            this.sala = new HashSet<sala>();
        }
    
        public int id { get; set; }
        public string nazwa { get; set; }
        public int kierownik { get; set; }
    
        public virtual pracownik pracownik { get; set; }
        public virtual ICollection<sala> sala { get; set; }
    }
}
