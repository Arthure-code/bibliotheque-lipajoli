using BibliothequeLIPAJOLI.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliothequeLIPAJOLI.ViewModels
{
    public class LivreFiltreViewModel
    {
        public string? SearchString { get; set; }
        public int? Annee { get; set; }
        public string? SortOrder { get; set; }
        public int? CategorieId { get; set; }

        public IEnumerable<Livre> Livres { get; set; } = new List<Livre>();
        public SelectList? Categories { get; set; }
    }
}
