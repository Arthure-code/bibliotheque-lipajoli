using System.ComponentModel.DataAnnotations;

namespace BibliothequeLIPAJOLI.Models
{
    public enum TypeAdresse
    {
        Principale,
        Secondaire,
        Bureau,
        Autre
    }
    public class UsagerAdresse
    {
        public int UsagerID { get; set; }
        public Usager? Usager { get; set; }

        public int AdresseID { get; set; }
        public Adresse? Adresse { get; set; }

               public TypeAdresse? TypeAdresse { get; set; }

    }
}
