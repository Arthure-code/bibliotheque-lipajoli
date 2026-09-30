using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliothequeLIPAJOLI.Models
{
  
    public class Categorie
    {
        [BindNever]
        public int CategorieID { get; set; }

        public string? NomCategorie { get; set; }

        public string? LibelleCategorie { get; set; }

        public ICollection<Livre> Livres { get; set; } = new List<Livre>();
    }
}
