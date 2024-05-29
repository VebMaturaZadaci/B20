using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using KursnaListaAPI.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KursnaListaAPI.Controllers
{

    //[Produces("application/xml")]
    [Route("api/[controller]")]
    [ApiController]
    public class KursController : ControllerBase
    {

        private IWebHostEnvironment environment;
        readonly IConfiguration _configuration;
        private static List<Kurs> _kursevi;

        public KursController(IConfiguration configuration, IWebHostEnvironment webHost)
        {
            _configuration = configuration;
            environment = webHost;
            _kursevi = new List<Kurs>();
            //ucitavamo sve linije texta iz text fajla pa obradjujemo liniju po liniju i formiramo listu _kursevi
            string[] lines = System.IO.File.ReadAllLines(string.Concat(this.environment.WebRootPath, "/KursnaLista.txt"));
            foreach (string line in lines)
            {
                _kursevi.Add(new Kurs
                {
                    Datum = DateTime.Parse(line.Split('|')[0]),
                    OznakaValute = line.Split('|')[1],
                    VrednostKursa = Double.Parse(line.Split('|')[2])
                });
            }


        }

        // GET: api/<KursController>
        [HttpGet]
        public List<string> ProcitajSveValute()
        {
            List<string> valute = new List<string>(_kursevi.Select(o => o.OznakaValute).Distinct().ToList());
            return valute;
        }

        // GET api/<KursController>/datum/valuta
        [HttpGet("{datum}/{valuta}")]
        public double ProcitajKursNaDan(DateTime datum, string valuta)
        {
            // provera da li postoji kurs za izabrani datum i valutu
            if (_kursevi.Exists(x => x.Datum == datum && x.OznakaValute == valuta))
            {
                return _kursevi.Where(x => x.Datum == datum && x.OznakaValute == valuta).FirstOrDefault().VrednostKursa;
            }
            else { return 0.0; }
        }

        //void ObrisiPostojeci(Kurs k)
        //{

        //}

        //POST api/kurs?datum=...&valuta=...&vrednost=...
        [HttpPost]
        public bool UpisiKursNaDan(DateTime datum, string valuta, double vrednost)
        {

            //ispitati da li postoji vec unet kurs za datum i valutu slicno kao u citanju kursa
            foreach (Kurs k in _kursevi)
            {
                if (k.Datum.CompareTo(datum) == 0 && k.OznakaValute == valuta && k.VrednostKursa == vrednost)
                {
                    return false;
                }
            }

            //foreach (Kurs k in _kursevi)
            //{
            //    if (k.Datum.CompareTo(datum) == 0 && k.OznakaValute == valuta)
            //    {
            //        ObrisiPostojeci(k);
            //    }
            //}

            string novired = datum.ToString("\n" + "M/d/yyyy") + "|" + valuta + "|" + vrednost.ToString();
            //formiramo putanju do text fajla
            string docPath = string.Concat(this.environment.WebRootPath, "/KursnaLista.txt");
            //otvaramo strim i pisemo u njega
            using (StreamWriter outputFile = new StreamWriter(docPath, true))
            {
                outputFile.Write(novired);
            }
            return true;
        }



    }
}