namespace BibliothequeLIPAJOLI.ViewModels
{
    public class ErreurViewModel
    {
        public string? IdentifiantDeLaRequete { get; set; }

        public bool MontrerLIdentifiant => !string.IsNullOrEmpty(IdentifiantDeLaRequete);
    }
}
