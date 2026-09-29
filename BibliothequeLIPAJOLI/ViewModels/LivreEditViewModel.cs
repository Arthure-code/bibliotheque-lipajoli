using BibliothequeLIPAJOLI.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliothequeLIPAJOLI.ViewModels
{
    public class LivreEditViewModel
    {
        public Livre Livre { get; set; } = new Livre();
        public Edition Edition { get; set; } = new Edition();
        public Exemplaire Exemplaire { get; set; } = new Exemplaire();

        public List<int> Redactions { get; set; } = new List<int>();

        // Voir LivreCreateViewModel : listes d'affichage, hors liaison.
        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> Auteurs { get; set; } = new List<SelectListItem>();

        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
