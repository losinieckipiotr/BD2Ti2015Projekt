using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Stocktaking.Data
{
    static class DataFunctions
    {
        public static zaklad GetZaklad(pracownik p)
        {
            return p.sala.zaklad;
        }
    }
}
