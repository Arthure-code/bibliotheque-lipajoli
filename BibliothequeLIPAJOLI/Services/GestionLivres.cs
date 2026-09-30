using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.Regles;
using BibliothequeLIPAJOLI.ViewModels;

namespace BibliothequeLIPAJOLI.Services
{
    public class GestionLivres : ILivreService
    {
        private readonly ILivreDepot _depot;
        private readonly IReferentielService _referentiel;

        public GestionLivres(ILivreDepot depot, IReferentielService referentiel)
        {
            _depot = depot;
            _referentiel = referentiel;
        }

        public async Task<Livre?> ObtenirLivreParIdAsync(int identifiantLivre)
        {
            Livre? livre = await _depot.ObtenirAvecSesLiensAsync(identifiantLivre);

            if (livre != null)
            {
                Enrichir(livre);
            }

            return livre;
        }

        public async Task<List<Livre>> RechercherLivresAsync(string? texteCherche = null, int? categorieId = null)
        {
            string? terme = string.IsNullOrWhiteSpace(texteCherche) ? null : texteCherche.Trim();

            List<Livre> livres = await _depot.ChercherAsync(terme, AuteursQuiRepondentA(terme), categorieId);
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

        public async Task<Livre> CreerLivreCompletAsync(LivreFormulaireViewModel formulaire)
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

            _depot.Ajouter(formulaire.Livre);
            await _depot.EnregistrerAsync();
            Enrichir(formulaire.Livre);
            return formulaire.Livre;
        }

        public async Task<Livre?> ModifierLivreCompletAsync(int identifiantLivre, LivreFormulaireViewModel formulaire)
        {
            ArgumentNullException.ThrowIfNull(formulaire);

            Livre? livre = await _depot.ObtenirPourModificationAsync(identifiantLivre);

            if (livre == null)
            {
                return null;
            }

            // L'identite du livre ne vient pas du formulaire : ni sa cle,
            // ni son code, qui lui appartient pour toute sa vie.
            formulaire.Livre.LivreID = livre.LivreID;
            formulaire.Livre.Code = livre.Code;
            _depot.RemplacerLesValeurs(livre, formulaire.Livre);

            livre.Redactions = formulaire.Redactions.Select(auteurId => new Redaction
            {
                LivreID = livre.LivreID,
                AuteurID = auteurId,
                Role = "Auteur",
                Ordre = 1
            }).ToList();

            await _depot.EnregistrerAsync();
            Enrichir(livre);
            return livre;
        }

        public async Task<bool> SupprimerLivreAsync(int identifiantLivre)
        {
            Livre? livre = await _depot.ObtenirAsync(identifiantLivre);
            if (livre == null)
            {
                return false;
            }

            _depot.Supprimer(livre);
            await _depot.EnregistrerAsync();
            return true;
        }

        // Les auteurs viennent de la configuration, pas de la base : on resout
        // les noms ici, puis le depot garde les livres que ces auteurs ont signes.
        private List<int> AuteursQuiRepondentA(string? terme)
        {
            if (terme == null)
            {
                return new List<int>();
            }

            return _referentiel.ObtenirAuteurs()
                .Where(a => Texte.Contient($"{a.Prenom} {a.Nom}", terme)
                         || Texte.Contient($"{a.Nom} {a.Prenom}", terme))
                .Select(a => a.ID)
                .ToList();
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

            List<string?> codesDejaAttribues = await _depot.ListerLesCodesCommencantParAsync(prefixe);

            return Codification.Suivant(prefixe, codesDejaAttribues);
        }
    }
}
