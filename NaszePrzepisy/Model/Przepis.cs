using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaszePrzepisy.Model
{
    public class Przepis
    {
        public string Posilek { get; set; }
        public string Nazwa { get; set; }
        public List<string> Skladniki { get; set; }
        public string Przygotowanie { get; set; }
        public int Id { get; set; }
        public Przepis()
        {

        }

        public Przepis(string posilek, string nazwa, List<string> skladniki, string przygotowanie, int id)
        {
            Posilek = posilek;
            Nazwa = nazwa;
            Skladniki = skladniki;
            Przygotowanie = przygotowanie;
            Id = id;
        }
    }
}
