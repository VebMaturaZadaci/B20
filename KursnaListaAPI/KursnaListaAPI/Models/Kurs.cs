using System.ComponentModel.DataAnnotations;

namespace KursnaListaAPI.Models
{
    public class Kurs
    {
        [DataType(DataType.Date)]
        public DateTime Datum { get; set; }
        [Required]
        public string OznakaValute { get; set; }
        [Required]
        public double VrednostKursa { get; set; }
    }
}
