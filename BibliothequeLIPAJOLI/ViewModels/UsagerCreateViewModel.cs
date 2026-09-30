using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.ViewModels
{
    public class UsagerCreateViewModel
    {
        public Usager Usager { get; set; } = new Usager();
        public Adresse Adresse { get; set; } = new Adresse();
        public TypeAdresse TypeAdresse { get; set; } = TypeAdresse.Principale;
    }

}
