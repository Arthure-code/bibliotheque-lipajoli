using BibliothequeLIPAJOLI.Regles;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliothequeLIPAJOLI.Models
{
    public enum TypeUsager
    {
        Etudiant,
        Enseignant,
    }

    public class Usager : Personne
    {
        [Required(ErrorMessage = "Le numéro d’abonné est requis.")]
        [StringLength(20, ErrorMessage = "Le numéro d’abonné ne peut pas dépasser 20 caractères.")]
        [Display(Name = "Numéro d’abonné")]
        public string? NumeroAbonne { get; set; }

        [Required(ErrorMessage = "Le statut est requis.")]
        [Display(Name = "Statut de l’usager")]
        public TypeUsager? Statut { get; set; }

        [BindNever]
        [Display(Name = "Défaillances")]
        public int Defaillance { get; private set; }

        public void PorterUneDefaillance()
        {
            Defaillance++;
        }

        [NotMapped]
        [Display(Name = "Peut emprunter")]
        public bool PeutEmprunter => Defaillance < Pret.DefaillancesAvantBlocage;

        [Display(Name = "Emprunts en cours")]
        [NotMapped]
        public int EmpruntsEnCours => Emprunts.Count(e => !e.EstRetourne);

        public List<UsagerAdresse> UsagerAdresses { get; set; } = new();
        public List<Emprunt> Emprunts { get; set; } = new();
    }
}
