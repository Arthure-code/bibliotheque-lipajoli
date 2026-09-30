using BibliothequeLIPAJOLI.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliothequeLIPAJOLI.ViewModels
{
    // Cataloguer un livre et le corriger demandent la meme chose : un seul
    // formulaire, donc un seul modele.
    public class LivreFormulaireViewModel
    {
        public Livre Livre { get; set; } = new Livre();
        public Edition Edition { get; set; } = new Edition();
        public Exemplaire Exemplaire { get; set; } = new Exemplaire();

        public List<int> Redactions { get; set; } = new List<int>();

        // Listes d'affichage : le navigateur ne les renvoie pas, et les
        // valider ferait echouer l'envoi sur des champs absents de l'ecran.
        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> Auteurs { get; set; } = new List<SelectListItem>();

        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
