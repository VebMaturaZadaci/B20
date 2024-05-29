using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace KursnaListaApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<string> valute;
        public List<SelectListItem> opcije;
        public string poruka;
        public double kurs;

        [BindProperty]
        public InputModel Input { get; set; }
        //pravimo klijenta koji ce upucivati HTTP zahteve veb servisu (pogledajte Startup klasu)
        static readonly HttpClient client = new HttpClient();

        public async Task OnGetAsync()
        {

            await pupulateListOfCurrencies();


        }

        [Consumes("application/xml")]
        async Task pupulateListOfCurrencies()
        {
            string responseBody = await client.GetStringAsync("https://localhost:7057/api/kurs/");
            //Regex izbaci sve sto nije slovo ili zarez, zarez nam treba kao separator :)
            responseBody = Regex.Replace(responseBody, @"[^a-zA-Z\,]", "");
            //od preciscenog odgovora se formira lista valute 
            valute = responseBody.Split(',').ToList();
            opcije = valute.Select(a =>
                              new SelectListItem
                              {
                                  Value = a,
                                  Text = a
                              }).ToList();
        }

        //ako zelite da naglasite da metoda ocekuje XML format podatka stavite donji dekorator
        [Consumes("application/xml")]
        public async Task<IActionResult> OnPostAsync()
        {

            if (!ModelState.IsValid)
            {
                return Page();
            }

            //parametri get  zahteva se salju kroz url koji se formira u zagradi
            string responseBody = await client.GetStringAsync("https://localhost:7057/api/kurs/" + Input.Datum.ToString("yyyy-MM-ddTHH:mm:ss") + "/" + Input.Valuta);
            kurs = Convert.ToDouble(responseBody); //string se konvertuje u double, odgvoor je uvek string
            if (kurs == 0)
            {
                ViewData["Poruka"] = "Kurs za izabrani dan ne postoji u bazi!";
            }
            else
            {
                ViewData["Poruka"] = "Kurs na dan: " + kurs;
            }
            await pupulateListOfCurrencies();
            return Page();
        }
    }

    public class InputModel
    {

        [Required(ErrorMessage = "Polje za datum je obavezno!")]
        [DataType(DataType.Date, ErrorMessage = "Tip datuma mora biti datum!")]
        public DateTime Datum { get; set; }

        [Required(ErrorMessage = "Polje za valutu je obavezno!")]
        [MinLength(3, ErrorMessage = "Valuta mora imati tri cifre!")]
        [MaxLength(3, ErrorMessage = "Valuta mora imati tri cifre!")]
        public string Valuta { get; set; }


    }

}