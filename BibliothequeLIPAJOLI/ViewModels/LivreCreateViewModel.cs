using BibliothequeLIPAJOLI.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliothequeLIPAJOLI.ViewModels
{
    public class LivreCreateViewModel
    {
        public Livre Livre { get; set; } = new Livre();
        public Edition Edition { get; set; } = new Edition();
        public Exemplaire Exemplaire { get; set; } = new Exemplaire();

        public List<int> Redactions { get; set; } = new List<int>();

        // Listes d'affichage : le navigateur ne les renvoie pas, et les
        // valider ferait échouer l'envoi sur des champs absents de l'écran.
        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> Auteurs { get; set; } = new List<SelectListItem>();

        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
