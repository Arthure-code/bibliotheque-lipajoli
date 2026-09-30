using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using BibliothequeLIPAJOLI.Validation;

namespace BibliothequeLIPAJOLI.Models
{
    public class Livre
    {
        public int LivreID { get; set; }

        [BindNever]
        [ValidateNever]
        [StringLength(20)]
        [Display(Name = "Code du livre")]
        public string? Code { get; set; }

        [Required(ErrorMessage = "L'ISBN-10 est requis.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "L'ISBN-10 doit contenir exactement 10 caractères.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "L'ISBN-10 doit contenir uniquement des chiffres.")]
        [Display(Name = "ISBN-10")]
        public string? Isbn10 { get; set; }

        [Required(ErrorMessage = "L'ISBN-13 est requis.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "L'ISBN-13 doit contenir exactement 13 caractères.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "L'ISBN-13 doit contenir uniquement des chiffres.")]
        [Display(Name = "ISBN-13")]
        public string? Isbn13 { get; set; }

        [Required(ErrorMessage = "Le titre est requis.")]
        [StringLength(200, ErrorMessage = "Le titre ne peut pas dépasser 200 caractères.")]
        [Display(Name = "Titre")]
        public string? Titre { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le prix ne peut pas être négatif.")]
        [AuPlusDeuxDecimales(ErrorMessage = "Le prix s'écrit avec au plus deux décimales.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Prix")]
        public decimal? Prix { get; set; }

        [NotMapped]
        [Display(Name = "Quantité totale")]
        public int? Quantite
        {
            get
            {
                return Editions
                    .SelectMany(e => e.Exemplaires)
                    .Sum(ex => ex.NbExemplaire) ?? 0;
            }
        }

        [NotMapped]
        [Display(Name = "Quantité disponible")]
        public int? QuantiteDisponible
        {
            get
            {
                return Editions
                    .SelectMany(e => e.Exemplaires)
                    .Sum(ex =>
                        ex.NbExemplaire -
                        ex.Emprunts.Count(emp => emp.DateRetour == null)
                    ) ?? 0;
            }
        }

        [Display(Name = "Année de publication")]
        [DataType(DataType.Date)]
        public DateTime? Annee { get; set; }

        [Display(Name = "Résumé")]
        public string? Resume { get; set; }

        [Display(Name = "Image")]
        [DataType(DataType.ImageUrl)]
        public string? ImageUrl { get; set; }

        public bool? EnStock { get; set; } = true;

        public int CategorieID { get; set; }

        [Display(Name = "Langue")]
        [StringLength(15, ErrorMessage = "La langue ne peut pas dépasser 15 caractères.")]
        public string? Langue { get; set; }

        [Display(Name = "Nombre de pages")]
        [Range(1, 10000, ErrorMessage = "Le nombre de pages doit être entre 1 et 10 000.")]
        public int? NombrePages { get; set; }

        [NotMapped]
        public Categorie? Categorie { get; set; }

        public ICollection<Edition> Editions { get; set; } = new List<Edition>();
        public ICollection<Redaction> Redactions { get; set; } = new List<Redaction>();
    }
}
