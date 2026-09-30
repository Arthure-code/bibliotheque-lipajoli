using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.Regles;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.Services
{
    public static class DbInitializer
    {
        public static async Task Initialize(BibliothequeLIPAJOLIContext context, int joursDePret)
        {
            if (await context.Livres.AnyAsync())
            {
                return; 
            }

            var adresses = new Adresse[]
            {
                new Adresse { AdresseID = 1, Rue = "123 Rue Principale", Ville = "Montréal", Province = "Québec", CodePostale = "H1A 1A1" },
                new Adresse { AdresseID = 2, Rue = "456 Avenue des Érables", Ville = "Québec", Province = "Québec", CodePostale = "G1K 2K2" },
                new Adresse { AdresseID = 3, Rue = "789 Boulevard Saint-Laurent", Ville = "Montréal", Province = "Québec", CodePostale = "H2X 3X3" },
                new Adresse { AdresseID = 4, Rue = "321 Rue Sherbrooke", Ville = "Montréal", Province = "Québec", CodePostale = "H3A 4A4" }
            };
            await context.Adresses.AddRangeAsync(adresses);

            var livres = new List<Livre>
            {
                new Livre { LivreID = 1, Code = "ROM001", Isbn10 = "1234567890", Isbn13 = "9781234567890", Titre = "Les Misérables", Prix = 14.99m, Annee = new DateTime(1862,1,1), Resume = "Une fresque sociale et humaine...", ImageUrl = "/images/les-miserables.jpg", EnStock = true, CategorieID = 1, Langue = "Français", NombrePages = 1232 },
                new Livre { LivreID = 2, Code = "SCI001", Isbn10 = "0987654321", Isbn13 = "9780987654321", Titre = "Dune",Prix = 19.99m, Annee = new DateTime(1965,8,1), Resume = "Un classique de la science-fiction...", ImageUrl = "/images/dune.jpg", EnStock = true, CategorieID = 2, Langue = "Français", NombrePages = 412 },
                new Livre { LivreID = 3, Code = "BIO001", Isbn10 = "1122334455", Isbn13 = "9781122334455", Titre = "Albert Camus, une vie", Prix = 22.50m, Annee = new DateTime(1970,5,10), Resume = "Biographie de l’écrivain Albert Camus.", ImageUrl = "/images/camus-biographie.jpg", EnStock = true, CategorieID = 3, Langue = "Français", NombrePages = 350 },
                new Livre { LivreID = 4, Code = "POE001", Isbn10 = "5566778899", Isbn13 = "9785566778899", Titre = "Les Fleurs du Mal",Prix = 17.99m, Annee = new DateTime(1857,6,1), Resume = "Recueil de poèmes majeurs.", ImageUrl = "/images/fleurs-du-mal.jpg", EnStock = true, CategorieID = 4, Langue = "Français", NombrePages = 280 }
            };
            await context.Livres.AddRangeAsync(livres);

            var redactions = new List<Redaction>
            {
                new Redaction { LivreID = 1, AuteurID = 2, Role = "Auteur", Ordre = 1 },
                new Redaction { LivreID = 2, AuteurID = 5, Role = "Auteur", Ordre = 1 },
                new Redaction { LivreID = 3, AuteurID = 1, Role = "Sujet", Ordre = 1 },
                new Redaction { LivreID = 4, AuteurID = 4, Role = "Auteur", Ordre = 1 }
            };
            await context.Redactions.AddRangeAsync(redactions);

            var usagers = new List<Usager>
{
    new Usager {
        ID = 1,
        Nom = "Tremblay",
        Prenom = "Marie",
        Courriel = "marie.tremblay@email.com",
        Statut = TypeUsager.Etudiant,
        NumeroAbonne = "LJ001",
    },
    new Usager {
        ID = 2,
        Nom = "Gagnon",
        Prenom = "Pierre",
        Courriel = "pierre.gagnon@email.com",
        Statut = TypeUsager.Enseignant,
        NumeroAbonne = "LJ002"
    },
    new Usager {
        ID = 3,
        Nom = "Roy",
        Prenom = "Sophie",
        Courriel = "sophie.roy@email.com",
        Statut = TypeUsager.Etudiant,
        NumeroAbonne = "LJ003"
    }
};
            await context.Usagers.AddRangeAsync(usagers);

            var usagerAdresses = new List<UsagerAdresse>
            {
                new UsagerAdresse { UsagerID = 1, AdresseID = 1, TypeAdresse = TypeAdresse.Principale},
                new UsagerAdresse {  UsagerID = 2, AdresseID = 2, TypeAdresse = TypeAdresse.Principale},
                new UsagerAdresse {  UsagerID = 3, AdresseID = 3, TypeAdresse = TypeAdresse.Principale}
            };
            await context.UsagerAdresses.AddRangeAsync(usagerAdresses);

            var editions = new List<Edition>
            {
                new Edition { EditionID = 1, LivreID = 1, NomEditeur = "Gallimard", AnneeEdition = new DateTime(1865,1,1) },
                new Edition { EditionID = 2, LivreID = 2, NomEditeur = "Robert Laffont", AnneeEdition = new DateTime(1972,1,1) },
                new Edition { EditionID = 3, LivreID = 3, NomEditeur = "Le Livre de Poche", AnneeEdition = new DateTime(1995,1,1) },
                new Edition { EditionID = 4, LivreID = 4, NomEditeur = "Gallimard", AnneeEdition = new DateTime(1972,1,1) }
            };
            await context.Editions.AddRangeAsync(editions);

            var exemplaires = new List<Exemplaire>
{
    new Exemplaire { ExemplaireID = 101, EditionID = 1, Reference = 1001, NbExemplaire = 5, DateAchat = new DateTime(2023,5,10), Fournisseur = "Renaud-Bray" },
    new Exemplaire { ExemplaireID = 102, EditionID = 2, Reference = 1002, NbExemplaire = 3, DateAchat = new DateTime(2022,4,18), Fournisseur = "Amazon" },
    new Exemplaire { ExemplaireID = 103, EditionID = 2, Reference = 1003, NbExemplaire = 2, DateAchat = new DateTime(2021,11,20), Fournisseur = "Indigo" },
    new Exemplaire { ExemplaireID = 104, EditionID = 3, Reference = 1004, NbExemplaire = 1, DateAchat = new DateTime(2023,1,5), Fournisseur = "Archambault" },
    new Exemplaire { ExemplaireID = 105, EditionID = 3, Reference = 1005, NbExemplaire = 1, DateAchat = new DateTime(2023,2,10), Fournisseur = "Indigo" },
    new Exemplaire { ExemplaireID = 106, EditionID = 3, Reference = 1006, NbExemplaire = 1, DateAchat = new DateTime(2023,3,15), Fournisseur = "Renaud-Bray" },
    new Exemplaire { ExemplaireID = 107, EditionID = 4, Reference = 1007, NbExemplaire = 2, DateAchat = new DateTime(2024,9,3), Fournisseur = "Archambault" }
};
            await context.Exemplaires.AddRangeAsync(exemplaires);

            var marie = usagers[0];
            var pierre = usagers[1];
            var sophie = usagers[2];

            var rendueATemps = new DateTime(2025, 8, 1);
            var renduEnRetard = new DateTime(2025, 7, 10);

            var emprunts = new List<Emprunt>
            {
                // Rendu la veille de la date limite : rien au dossier.
                new Emprunt
                {
                    EmpruntID = 1, UsagerID = marie.ID, ExemplaireID = 101,
                    DateEmprunt = rendueATemps,
                    DateProbableRetour = Pret.DateLimite(rendueATemps, joursDePret),
                    DateRetour = Pret.DateLimite(rendueATemps, joursDePret).AddDays(-1)
                },
                // Rendu six jours après la date limite : une défaillance.
                new Emprunt
                {
                    EmpruntID = 2, UsagerID = pierre.ID, ExemplaireID = 102,
                    DateEmprunt = renduEnRetard,
                    DateProbableRetour = Pret.DateLimite(renduEnRetard, joursDePret),
                    DateRetour = Pret.DateLimite(renduEnRetard, joursDePret).AddDays(6)
                },
                new Emprunt
                {
                    EmpruntID = 3, UsagerID = pierre.ID, ExemplaireID = 103,
                    DateEmprunt = new DateTime(2025, 7, 15),
                    DateProbableRetour = Pret.DateLimite(new DateTime(2025, 7, 15), joursDePret)
                },
                // Sophie tient ses trois emprunts en cours, le maximum.
                new Emprunt
                {
                    EmpruntID = 4, UsagerID = sophie.ID, ExemplaireID = 104,
                    DateEmprunt = new DateTime(2025, 6, 1),
                    DateProbableRetour = Pret.DateLimite(new DateTime(2025, 6, 1), joursDePret)
                },
                new Emprunt
                {
                    EmpruntID = 5, UsagerID = sophie.ID, ExemplaireID = 105,
                    DateEmprunt = new DateTime(2025, 6, 10),
                    DateProbableRetour = Pret.DateLimite(new DateTime(2025, 6, 10), joursDePret)
                },
                new Emprunt
                {
                    EmpruntID = 6, UsagerID = sophie.ID, ExemplaireID = 106,
                    DateEmprunt = new DateTime(2025, 7, 1),
                    DateProbableRetour = Pret.DateLimite(new DateTime(2025, 7, 1), joursDePret)
                }
            };
            await context.Emprunts.AddRangeAsync(emprunts);

            foreach (Emprunt emprunt in emprunts)
            {
                bool enRetard = emprunt.DateRetour.HasValue
                    && Pret.EstEnRetard(emprunt.DateProbableRetour!.Value, emprunt.DateRetour.Value);

                if (enRetard)
                {
                    usagers.Single(u => u.ID == emprunt.UsagerID).PorterUneDefaillance();
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
