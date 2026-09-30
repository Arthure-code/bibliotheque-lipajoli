using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.Regles;

namespace BibliothequeLIPAJOLI.Data
{
    // Ce que la bibliotheque contient au premier demarrage. Aucune base
    // derriere : le semeur se contente d'enregistrer ce qui sort d'ici.
    public static class DonneesDeDepart
    {
        private const string Quebec = "Québec";
        private const string Francais = "Français";

        public static List<Adresse> Adresses() => new List<Adresse>
        {
            new Adresse { AdresseID = 1, Rue = "123 Rue Principale", Ville = "Montréal", Province = Quebec, CodePostale = "H1A 1A1" },
            new Adresse { AdresseID = 2, Rue = "456 Avenue des Érables", Ville = Quebec, Province = Quebec, CodePostale = "G1K 2K2" },
            new Adresse { AdresseID = 3, Rue = "789 Boulevard Saint-Laurent", Ville = "Montréal", Province = Quebec, CodePostale = "H2X 3X3" },
            new Adresse { AdresseID = 4, Rue = "321 Rue Sherbrooke", Ville = "Montréal", Province = Quebec, CodePostale = "H3A 4A4" }
        };

        public static List<Livre> Livres() => new List<Livre>
        {
            new Livre { LivreID = 1, Code = "ROM001", Isbn10 = "1234567890", Isbn13 = "9781234567890", Titre = "Les Misérables", Prix = 14.99m, Annee = Date(1862, 1, 1), Resume = "Une fresque sociale et humaine...", ImageUrl = "/images/les-miserables.jpg", EnStock = true, CategorieID = 1, Langue = Francais, NombrePages = 1232 },
            new Livre { LivreID = 2, Code = "SCI001", Isbn10 = "0987654321", Isbn13 = "9780987654321", Titre = "Dune", Prix = 19.99m, Annee = Date(1965, 8, 1), Resume = "Un classique de la science-fiction...", ImageUrl = "/images/dune.jpg", EnStock = true, CategorieID = 2, Langue = Francais, NombrePages = 412 },
            new Livre { LivreID = 3, Code = "BIO001", Isbn10 = "1122334455", Isbn13 = "9781122334455", Titre = "Albert Camus, une vie", Prix = 22.50m, Annee = Date(1970, 5, 10), Resume = "Biographie de l’écrivain Albert Camus.", ImageUrl = "/images/camus-biographie.jpg", EnStock = true, CategorieID = 3, Langue = Francais, NombrePages = 350 },
            new Livre { LivreID = 4, Code = "POE001", Isbn10 = "5566778899", Isbn13 = "9785566778899", Titre = "Les Fleurs du Mal", Prix = 17.99m, Annee = Date(1857, 6, 1), Resume = "Recueil de poèmes majeurs.", ImageUrl = "/images/fleurs-du-mal.jpg", EnStock = true, CategorieID = 4, Langue = Francais, NombrePages = 280 }
        };

        public static List<Redaction> Redactions() => new List<Redaction>
        {
            new Redaction { LivreID = 1, AuteurID = 2, Role = "Auteur", Ordre = 1 },
            new Redaction { LivreID = 2, AuteurID = 5, Role = "Auteur", Ordre = 1 },
            new Redaction { LivreID = 3, AuteurID = 1, Role = "Sujet", Ordre = 1 },
            new Redaction { LivreID = 4, AuteurID = 4, Role = "Auteur", Ordre = 1 }
        };

        public static List<Usager> Usagers() => new List<Usager>
        {
            new Usager { ID = 1, Nom = "Tremblay", Prenom = "Marie", Courriel = "marie.tremblay@email.com", Statut = TypeUsager.Etudiant, NumeroAbonne = "LJ001" },
            new Usager { ID = 2, Nom = "Gagnon", Prenom = "Pierre", Courriel = "pierre.gagnon@email.com", Statut = TypeUsager.Enseignant, NumeroAbonne = "LJ002" },
            new Usager { ID = 3, Nom = "Roy", Prenom = "Sophie", Courriel = "sophie.roy@email.com", Statut = TypeUsager.Etudiant, NumeroAbonne = "LJ003" }
        };

        public static List<UsagerAdresse> UsagerAdresses() => new List<UsagerAdresse>
        {
            new UsagerAdresse { UsagerID = 1, AdresseID = 1, TypeAdresse = TypeAdresse.Principale },
            new UsagerAdresse { UsagerID = 2, AdresseID = 2, TypeAdresse = TypeAdresse.Principale },
            new UsagerAdresse { UsagerID = 3, AdresseID = 3, TypeAdresse = TypeAdresse.Principale }
        };

        public static List<Edition> Editions() => new List<Edition>
        {
            new Edition { EditionID = 1, LivreID = 1, NomEditeur = "Gallimard", AnneeEdition = Date(1865, 1, 1) },
            new Edition { EditionID = 2, LivreID = 2, NomEditeur = "Robert Laffont", AnneeEdition = Date(1972, 1, 1) },
            new Edition { EditionID = 3, LivreID = 3, NomEditeur = "Le Livre de Poche", AnneeEdition = Date(1995, 1, 1) },
            new Edition { EditionID = 4, LivreID = 4, NomEditeur = "Gallimard", AnneeEdition = Date(1972, 1, 1) }
        };

        public static List<Exemplaire> Exemplaires() => new List<Exemplaire>
        {
            new Exemplaire { ExemplaireID = 101, EditionID = 1, Reference = 1001, NbExemplaire = 5, DateAchat = Date(2023, 5, 10), Fournisseur = "Renaud-Bray" },
            new Exemplaire { ExemplaireID = 102, EditionID = 2, Reference = 1002, NbExemplaire = 3, DateAchat = Date(2022, 4, 18), Fournisseur = "Amazon" },
            new Exemplaire { ExemplaireID = 103, EditionID = 2, Reference = 1003, NbExemplaire = 2, DateAchat = Date(2021, 11, 20), Fournisseur = "Indigo" },
            new Exemplaire { ExemplaireID = 104, EditionID = 3, Reference = 1004, NbExemplaire = 1, DateAchat = Date(2023, 1, 5), Fournisseur = "Archambault" },
            new Exemplaire { ExemplaireID = 105, EditionID = 3, Reference = 1005, NbExemplaire = 1, DateAchat = Date(2023, 2, 10), Fournisseur = "Indigo" },
            new Exemplaire { ExemplaireID = 106, EditionID = 3, Reference = 1006, NbExemplaire = 1, DateAchat = Date(2023, 3, 15), Fournisseur = "Renaud-Bray" },
            new Exemplaire { ExemplaireID = 107, EditionID = 4, Reference = 1007, NbExemplaire = 2, DateAchat = Date(2024, 9, 3), Fournisseur = "Archambault" }
        };

        public static List<Emprunt> Emprunts(int joursDePret)
        {
            DateTime rendueATemps = Date(2025, 8, 1);
            DateTime renduEnRetard = Date(2025, 7, 10);

            return new List<Emprunt>
            {
                // Rendu la veille de la date limite : rien au dossier.
                new Emprunt
                {
                    EmpruntID = 1, UsagerID = 1, ExemplaireID = 101,
                    DateEmprunt = rendueATemps,
                    DateProbableRetour = Pret.DateLimite(rendueATemps, joursDePret),
                    DateRetour = Pret.DateLimite(rendueATemps, joursDePret).AddDays(-1)
                },
                // Rendu six jours après la date limite : une défaillance.
                new Emprunt
                {
                    EmpruntID = 2, UsagerID = 2, ExemplaireID = 102,
                    DateEmprunt = renduEnRetard,
                    DateProbableRetour = Pret.DateLimite(renduEnRetard, joursDePret),
                    DateRetour = Pret.DateLimite(renduEnRetard, joursDePret).AddDays(6)
                },
                new Emprunt
                {
                    EmpruntID = 3, UsagerID = 2, ExemplaireID = 103,
                    DateEmprunt = Date(2025, 7, 15),
                    DateProbableRetour = Pret.DateLimite(Date(2025, 7, 15), joursDePret)
                },
                // Sophie tient ses trois emprunts en cours, le maximum.
                new Emprunt
                {
                    EmpruntID = 4, UsagerID = 3, ExemplaireID = 104,
                    DateEmprunt = Date(2025, 6, 1),
                    DateProbableRetour = Pret.DateLimite(Date(2025, 6, 1), joursDePret)
                },
                new Emprunt
                {
                    EmpruntID = 5, UsagerID = 3, ExemplaireID = 105,
                    DateEmprunt = Date(2025, 6, 10),
                    DateProbableRetour = Pret.DateLimite(Date(2025, 6, 10), joursDePret)
                },
                new Emprunt
                {
                    EmpruntID = 6, UsagerID = 3, ExemplaireID = 106,
                    DateEmprunt = Date(2025, 7, 1),
                    DateProbableRetour = Pret.DateLimite(Date(2025, 7, 1), joursDePret)
                }
            };
        }

        // Un dossier ne porte pas de defaillance ecrite a la main : elle se
        // deduit des retours en retard.
        public static void PorterLesDefaillances(IEnumerable<Usager> usagers, IEnumerable<Emprunt> emprunts)
        {
            ArgumentNullException.ThrowIfNull(usagers);
            ArgumentNullException.ThrowIfNull(emprunts);

            foreach (Emprunt emprunt in emprunts)
            {
                bool enRetard = emprunt.DateRetour.HasValue
                    && Pret.EstEnRetard(emprunt.DateProbableRetour!.Value, emprunt.DateRetour.Value);

                if (enRetard)
                {
                    usagers.FirstOrDefault(u => u.ID == emprunt.UsagerID)?.PorterUneDefaillance();
                }
            }
        }

        // Une date civile, sans heure ni fuseau, comme la colonne qui la recoit.
        private static DateTime Date(int annee, int mois, int jour)
        {
            return new DateTime(annee, mois, jour, 0, 0, 0, DateTimeKind.Unspecified);
        }
    }
}
