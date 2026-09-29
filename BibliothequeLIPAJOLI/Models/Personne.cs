using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace BibliothequeLIPAJOLI.Models
{
    public abstract class Personne
    {
        [BindNever]
        public int ID { get; set; }
        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères.")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis.")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères.")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le courriel est requis.")]
        [EmailAddress(ErrorMessage = "Le courriel n'est pas valide.")]
        [Display(Name = "Adresse courriel")]
        public string Courriel { get; set; } = string.Empty;

        [Display(Name = "Prénom et Nom")]
        public string NomComplet => $"{Prenom} {Nom}";
    }
}