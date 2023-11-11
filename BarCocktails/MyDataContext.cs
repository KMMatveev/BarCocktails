using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarCocktails
{
    public interface MyDataContext
    {
        bool Add<T>(T elem);
        bool Update<T>(T elem);
        bool Delete<T>(int id);
        List<T> Select<T>(T elem);
        T SelectById<T>(int id);
    }
}
