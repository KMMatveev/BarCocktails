using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BarCocktails
{
    public class Cocktails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public double Cost { get; set; }
        public bool Ready { get; set; }
        public Cocktails() { }
        public Cocktails(int id, string name,string decription, DateTime creationtime, double cost, bool ready)
        {
            Id = id;
            Name = name;
            Description = decription;
            CreationTime = creationtime;
            Cost = cost;
            Ready = ready;
        }
        public override string ToString()
        {
            return $@"
            <tr>
                <th>{Id}</th>
                <th>{Name}</th>
                <th>{Description}</th>
                <th>{CreationTime}</th>
                <th>{Cost}</th>
                <th>{Ready}</th>
            </tr>";
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {

            //Console.WriteLine(myORM.Add<products>(new products(2, "apple", 12.12, 1)));
            //Console.WriteLine(myORM.Add<Cocktails>(new Cocktails(2, "apple", "green",DateTime.Now, 12.12, true)));
            //Console.WriteLine(myORM.Update<products>(new products(4,"banana",12.12,1)));
            //Console.WriteLine(myORM.Update<products>(new products(3,"banana",12.12,1)));
            //Console.WriteLine(myORM.Update<products>(new products(3,"banana",12.12,1)));
            //Console.WriteLine(myORM.Delete<Cocktails>(4));
            //var list = myORM.Select<Cocktails>(new Cocktails(2, "apple", "green", DateTime.Now, 12.12, true));
            //foreach (var item in list)
            //{
            //    Console.WriteLine(item);
            //}

            HttpServer server = new HttpServer();
            server.StartServer();





            Console.ReadKey();

        }
    }
}
