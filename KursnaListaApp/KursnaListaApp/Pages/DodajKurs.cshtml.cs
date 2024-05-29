using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace KursnaListaApp.Pages
{
    public class DodajKursModel : PageModel
    {
        public List<string> valute;
        public List<SelectListItem> opcije;
        public string poruka;
        public double kurs;

        [BindProperty]
        public InputModel2 Input { get; set; }

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

        public async Task<IActionResult> OnPostAsync()
        {
            //prosti parametri u zahtevu client.PostAsync se prosledjuju kroz query string 
            //a kroz drugi argument se mogu proselditi oni koji us dekorisani  sa [FromBody] ili [FromForm] ali samo jedana takav a mi imamo
            //3 argumenta pa moramo ovako

            if (ModelState.IsValid)
            {
                var response = await client.PostAsync("https://localhost:7057/api/kurs?datum=" + Input.Datum.ToString("yyyy-MM-ddTHH:mm:ss") + "&valuta=" + Input.Valuta + "&vrednost=" + Input.vrednKursa, null);
                string result = response.Content.ReadAsStringAsync().Result; //rezultat je uvek string 
                                                                             //if (result == "true") { ViewData["Poruka"] = "Kurs za " + Input.Valuta + " na dan " + Input.Datum.ToString("yyyy-MM-ddTHH:mm:ss") + " je  " + Input.vrednKursa; }
                if (result == "false")
                {
                    ViewData["Greska"] = "Vec postoji ovakav kurs u bazi!";
                    await pupulateListOfCurrencies();
                    return Page();
                }
                else
                {
                    await pupulateListOfCurrencies();
                    return RedirectToPage("/Index");
                }
            }
            else
            {
                await pupulateListOfCurrencies();
                return Page();
            }
        }
    }

    public class InputModel2
    {

        [Required(ErrorMessage = "Polje za datum je obavezno!")]
        [DataType(DataType.Date, ErrorMessage = "Tip datuma mora biti datum!")]
        public DateTime Datum { get; set; }

        [Required(ErrorMessage = "Polje za valutu je obavezno!")]
        [MinLength(3, ErrorMessage = "Valuta mora imati tri cifre!")]
        [MaxLength(3, ErrorMessage = "Valuta mora imati tri cifre!")]
        public string Valuta { get; set; }

        [Required(ErrorMessage = "Polje za vrednost kursa je obavezno!")]
        public double vrednKursa { get; set; }


    }
}
