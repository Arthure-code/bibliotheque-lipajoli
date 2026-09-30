using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.Regles;
using BibliothequeLIPAJOLI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.Services
{
    public class GestionLivres : ILivreService
    {
        private readonly BibliothequeLIPAJOLIContext _context;
        private readonly IReferentielService _referentiel;

        public GestionLivres(BibliothequeLIPAJOLIContext context, IReferentielService referentiel)
        {
            _context = context;
            _referentiel = referentiel;
        }

        public async Task<Livre?> ObtenirLivreParIdAsync(int identifiantLivre)
        {
            Livre? livre = await ChargerLeCatalogue()
                .FirstOrDefaultAsync(l => l.LivreID == identifiantLivre);

            if (livre != null)
            {
                Enrichir(livre);
            }

            return livre;
        }

        public async Task<List<Livre>> RechercherLivresAsync(string? texteCherche = null, int? categorieId = null)
        {
            IQueryable<Livre> requete = _context.Livres;

            if (!string.IsNullOrWhiteSpace(texteCherche))
            {
                string terme = texteCherche.Trim();

                // Les auteurs viennent de la configuration, pas de la base :
                // on résout les noms, puis on garde les livres signés.
                List<int> auteursTrouves = _referentiel.ObtenirAuteurs()
                    .Where(a => Texte.Contient($"{a.Prenom} {a.Nom}", terme)
                             || Texte.Contient($"{a.Nom} {a.Prenom}", terme))
                    .Select(a => a.ID)
                    .ToList();

                // Like ignore la casse, Contains non.
                requete = requete.Where(l =>
                    (l.Titre != null && EF.Functions.Like(l.Titre, $"%{terme}%"))
                    || l.Redactions.Any(r => auteursTrouves.Contains(r.AuteurID)));
            }

            if (categorieId.HasValue)
            {
                requete = requete.Where(l => l.CategorieID == categorieId.Value);
            }

            List<Livre> livres = await ChargerLeCatalogue(requete).ToListAsync();
            livres.ForEach(Enrichir);
            return livres;
        }

        public List<Livre> TrierLivres(List<Livre> livres, string ordre)
        {
            ArgumentNullException.ThrowIfNull(livres);

            return (ordre ?? string.Empty).ToLowerInvariant() switch
            {
                "titre" => livres.OrderBy(l => l.Titre).ToList(),
                "code" => livres.OrderBy(l => l.Code).ToList(),
                "annee" => livres.OrderByDescending(l => l.Annee).ToList(),
                _ => livres
            };
        }

        public async Task<Livre> CreerLivreCompletAsync(LivreCreateViewModel formulaire)
        {
            ArgumentNullException.ThrowIfNull(formulaire);

            formulaire.Edition.Livre = formulaire.Livre;
            formulaire.Livre.Editions = new List<Edition> { formulaire.Edition };

            formulaire.Livre.Redactions = formulaire.Redactions
                .Select(auteurId => new Redaction
                {
                    AuteurID = auteurId,
                    Livre = formulaire.Livre,
                    Role = "Auteur",
                    Ordre = 1
                }).ToList();

            formulaire.Exemplaire.Edition = formulaire.Edition;
            formulaire.Edition.Exemplaires = new List<Exemplaire> { formulaire.Exemplaire };

            formulaire.Livre.Code = await AttribuerLeCodeAsync(formulaire.Livre.CategorieID);

            _context.Livres.Add(formulaire.Livre);
            await _context.SaveChangesAsync();
            Enrichir(formulaire.Livre);
            return formulaire.Livre;
        }

        public async Task<Livre?> ModifierLivreCompletAsync(LivreEditViewModel formulaire)
        {
            ArgumentNullException.ThrowIfNull(formulaire);

            Livre? livre = await _context.Livres
                .Include(l => l.Redactions)
                .Include(l => l.Editions).ThenInclude(e => e.Exemplaires)
                .FirstOrDefaultAsync(l => l.LivreID == formulaire.Livre.LivreID);

            if (livre == null)
            {
                return null;
            }

            // Le code appartient au livre pour toute sa vie.
            string codeAttribue = livre.Code!;
            _context.Entry(livre).CurrentValues.SetValues(formulaire.Livre);
            livre.Code = codeAttribue;

            livre.Redactions = formulaire.Redactions.Select(auteurId => new Redaction
            {
                LivreID = livre.LivreID,
                AuteurID = auteurId,
                Role = "Auteur",
                Ordre = 1
            }).ToList();

            await _context.SaveChangesAsync();
            Enrichir(livre);
            return livre;
        }

        public async Task<bool> SupprimerLivreAsync(int identifiantLivre)
        {
            Livre? livre = await _context.Livres.FindAsync(identifiantLivre);
            if (livre == null)
            {
                return false;
            }

            _context.Livres.Remove(livre);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<Livre> ChargerLeCatalogue(IQueryable<Livre>? depuis = null)
        {
            return (depuis ?? _context.Livres)
                .Include(l => l.Redactions)
                .Include(l => l.Editions).ThenInclude(e => e.Exemplaires)
                    .ThenInclude(ex => ex.Emprunts);
        }

        private void Enrichir(Livre livre)
        {
            livre.EnStock = livre.QuantiteDisponible > 0;
            livre.Categorie = _referentiel.ObtenirCategorieParId(livre.CategorieID);

            foreach (Redaction redaction in livre.Redactions)
            {
                redaction.Auteur = _referentiel.ObtenirAuteurParId(redaction.AuteurID);
            }
        }

        private async Task<string> AttribuerLeCodeAsync(int categorieID)
        {
            Categorie? categorie = _referentiel.ObtenirCategorieParId(categorieID);
            string prefixe = Codification.Prefixe(categorie?.NomCategorie);

            List<string?> codesDejaAttribues = await _context.Livres
                .Where(l => l.Code != null && l.Code.StartsWith(prefixe))
                .Select(l => l.Code)
                .ToListAsync();

            return Codification.Suivant(prefixe, codesDejaAttribues);
        }
    }
}
