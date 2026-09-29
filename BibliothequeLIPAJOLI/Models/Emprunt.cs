using System.ComponentModel.DataAnnotations;

namespace BibliothequeLIPAJOLI.Models
{
    public class Emprunt
    {
        public int EmpruntID { get; set; }
        public DateTime? DateEmprunt { get; set; }
        public DateTime? DateProbableRetour { get; set; }
        public DateTime? DateRetour { get; set; }
        public bool EstRetourne => DateRetour.HasValue;
        public bool EstEnretard => !EstRetourne && DateTime.Now > DateProbableRetour;

        public int ExemplaireID { get; set; }
        public Exemplaire? Exemplaire { get; set; }
        public int UsagerID { get; set; }
        public Usager? Usager { get; set; }
    }
}
