using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliothequeLIPAJOLI.Models
{
  
    public class Categorie
    {
        public int CategorieID { get; set; }

        public string? NomCategorie { get; set; }

        public string? LibelleCategorie { get; set; }

        public ICollection<Livre> Livres { get; set; } = new List<Livre>();
    }
}
