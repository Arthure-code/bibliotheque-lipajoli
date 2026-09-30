using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.Services;
using BibliothequeLIPAJOLI.ViewModels;
using Moq;

namespace BibliothequeLIPAJOLI.Tests.Services
{
    public class GestionLivresTests
    {
        private readonly Mock<ILivreDepot> _depot = new Mock<ILivreDepot>();
        private readonly Mock<IReferentielService> _referentiel = new Mock<IReferentielService>();
        private readonly GestionLivres _service;

        private static readonly Categorie Roman = new Categorie { CategorieID = 1, NomCategorie = "Roman" };
        private static readonly Auteur Hugo = new Auteur { ID = 7, Prenom = "Victor", Nom = "Hugo" };

        public GestionLivresTests()
        {
            _referentiel.Setup(r => r.ObtenirCategorieParId(1)).Returns(Roman);
            _referentiel.Setup(r => r.ObtenirAuteurParId(7)).Returns(Hugo);
            _referentiel.Setup(r => r.ObtenirAuteurs()).Returns(new List<Auteur> { Hugo });

            _service = new GestionLivres(_depot.Object, _referentiel.Object);
        }

        private static Livre LivreEnRayon(int exemplaires = 2, int sortis = 0)
        {
            var emprunts = Enumerable.Range(0, sortis)
                .Select(_ => new Emprunt { DateRetour = null })
                .ToList();

            return new Livre
            {
                LivreID = 1,
                Titre = "Les Misérables",
                Code = "ROM001",
                CategorieID = 1,
                Redactions = new List<Redaction> { new Redaction { AuteurID = 7 } },
                Editions = new List<Edition>
                {
                    new Edition
                    {
                        Exemplaires = new List<Exemplaire>
                        {
                            new Exemplaire { NbExemplaire = exemplaires, Emprunts = emprunts }
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task ObtenirLivreParIdAsync_RendNullQuandLeLivreNExistePas()
        {
            //Etant donne un identifiant qui ne designe aucun livre
            _depot.Setup(d => d.ObtenirAvecSesLiensAsync(404)).ReturnsAsync((Livre?)null);

            //Lorsque
            Livre? livre = await _service.ObtenirLivreParIdAsync(404);

            //Alors
            Assert.Null(livre);
        }

        [Fact]
        public async Task ObtenirLivreParIdAsync_PoseLaCategorieEtLesAuteursVenusDeLaConfiguration()
        {
            //Etant donne un livre en base, dont la categorie et l'auteur n'y sont pas
            _depot.Setup(d => d.ObtenirAvecSesLiensAsync(1)).ReturnsAsync(LivreEnRayon());

            //Lorsque
            Livre? livre = await _service.ObtenirLivreParIdAsync(1);

            //Alors le livre arrive complet a la vue
            Assert.NotNull(livre);
            Assert.Equal("Roman", livre!.Categorie?.NomCategorie);
            Assert.Equal("Hugo", livre.Redactions.First().Auteur?.Nom);
        }

        [Theory]
        [InlineData(2, 0, true)]
        [InlineData(2, 2, false)]
        public async Task ObtenirLivreParIdAsync_LeStockSuitLesExemplairesSortis(int exemplaires, int sortis,
            bool attendu)
        {
            //Etant donne un livre dont une partie des exemplaires est sortie
            _depot.Setup(d => d.ObtenirAvecSesLiensAsync(1)).ReturnsAsync(LivreEnRayon(exemplaires, sortis));

            //Lorsque
            Livre? livre = await _service.ObtenirLivreParIdAsync(1);

            //Alors le stock se recalcule, il ne se lit pas en base
            Assert.Equal(attendu, livre!.EnStock);
        }

        [Fact]
        public async Task RechercherLivresAsync_ResoutLesAuteursSansAccentNiCasse()
        {
            //Etant donne une recherche ecrite sans majuscule et sans accent
            _depot.Setup(d => d.ChercherAsync("victor hugo", It.IsAny<IReadOnlyCollection<int>>(), null))
                .ReturnsAsync(new List<Livre>());

            //Lorsque
            await _service.RechercherLivresAsync("  victor hugo  ");

            //Alors le terme part sans ses espaces, avec l'auteur reconnu
            _depot.Verify(d => d.ChercherAsync("victor hugo",
                It.Is<IReadOnlyCollection<int>>(a => a.Contains(7)), null), Times.Once);
        }

        [Fact]
        public async Task RechercherLivresAsync_SansTermeNeCherchePersonne()
        {
            //Etant donne une page ouverte sans recherche
            _depot.Setup(d => d.ChercherAsync(null, It.IsAny<IReadOnlyCollection<int>>(), 3))
                .ReturnsAsync(new List<Livre> { LivreEnRayon() });

            //Lorsque seule la categorie filtre
            List<Livre> livres = await _service.RechercherLivresAsync("   ", 3);

            //Alors aucun auteur n'est resolu, et le catalogue arrive enrichi
            _depot.Verify(d => d.ChercherAsync(null, It.Is<IReadOnlyCollection<int>>(a => a.Count == 0), 3),
                Times.Once);
            Assert.Equal("Roman", livres.Single().Categorie?.NomCategorie);
        }

        [Theory]
        [InlineData("titre", "Au revoir")]
        [InlineData("code", "Les Misérables")]
        [InlineData("annee", "Une vie")]
        [InlineData("", "Les Misérables")]
        public void TrierLivres_ChaqueOrdreALeSien(string ordre, string premierAttendu)
        {
            //Etant donne trois livres
            var livres = new List<Livre>
            {
                new Livre { Titre = "Les Misérables", Code = "AAA001", Annee = new DateTime(1862, 1, 1) },
                new Livre { Titre = "Une vie", Code = "ZZZ001", Annee = new DateTime(1990, 1, 1) },
                new Livre { Titre = "Au revoir", Code = "MMM001", Annee = new DateTime(1930, 1, 1) }
            };

            //Lorsque
            List<Livre> tries = _service.TrierLivres(livres, ordre);

            //Alors
            Assert.Equal(premierAttendu, tries.First().Titre);
        }

        [Fact]
        public void TrierLivres_RefuseUneListeAbsente()
        {
            Assert.Throws<ArgumentNullException>(() => _service.TrierLivres(null!, "titre"));
        }

        [Fact]
        public async Task CreerLivreCompletAsync_AttribueLeCodeSuivantDeLaCategorie()
        {
            //Etant donne deux romans deja catalogues
            _depot.Setup(d => d.ListerLesCodesCommencantParAsync("ROM"))
                .ReturnsAsync(new List<string?> { "ROM001", "ROM002" });

            var formulaire = new LivreCreateViewModel
            {
                Livre = new Livre { Titre = "Quatrevingt-treize", CategorieID = 1 },
                Redactions = new List<int> { 7 }
            };

            //Lorsque le troisieme entre
            Livre livre = await _service.CreerLivreCompletAsync(formulaire);

            //Alors il prend la suite, sans que personne ne l'ait saisi
            Assert.Equal("ROM003", livre.Code);
            _depot.Verify(d => d.Ajouter(livre), Times.Once);
            _depot.Verify(d => d.EnregistrerAsync(), Times.Once);
        }

        [Fact]
        public async Task CreerLivreCompletAsync_RattacheLEditionLExemplaireEtLesAuteurs()
        {
            //Etant donne un formulaire complet
            _depot.Setup(d => d.ListerLesCodesCommencantParAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<string?>());

            var formulaire = new LivreCreateViewModel
            {
                Livre = new Livre { Titre = "Les Contemplations", CategorieID = 1 },
                Edition = new Edition { NomEditeur = "Hetzel" },
                Exemplaire = new Exemplaire { NbExemplaire = 4 },
                Redactions = new List<int> { 7 }
            };

            //Lorsque
            Livre livre = await _service.CreerLivreCompletAsync(formulaire);

            //Alors tout le graphe part en une seule fois
            Edition edition = Assert.Single(livre.Editions);
            Assert.Same(livre, edition.Livre);
            Assert.Equal(4, Assert.Single(edition.Exemplaires).NbExemplaire);
            Redaction redaction = Assert.Single(livre.Redactions);
            Assert.Equal(7, redaction.AuteurID);
            Assert.Equal("Auteur", redaction.Role);
        }

        [Fact]
        public async Task ModifierLivreCompletAsync_RendNullQuandLeLivreNExistePas()
        {
            //Etant donne un identifiant qui ne designe aucun livre
            _depot.Setup(d => d.ObtenirPourModificationAsync(404)).ReturnsAsync((Livre?)null);

            //Lorsque
            Livre? livre = await _service.ModifierLivreCompletAsync(404, new LivreEditViewModel());

            //Alors rien n'est enregistre
            Assert.Null(livre);
            _depot.Verify(d => d.EnregistrerAsync(), Times.Never);
        }

        [Fact]
        public async Task ModifierLivreCompletAsync_LaCleEtLeCodeNeViennentJamaisDuFormulaire()
        {
            //Etant donne un formulaire qui pretend changer la cle et le code
            Livre enBase = LivreEnRayon();
            _depot.Setup(d => d.ObtenirPourModificationAsync(1)).ReturnsAsync(enBase);

            var formulaire = new LivreEditViewModel
            {
                Livre = new Livre { LivreID = 99, Code = "PIRATE", Titre = "Titre corrigé", CategorieID = 1 },
                Redactions = new List<int> { 7 }
            };

            //Lorsque la requete arrive sur le livre 1
            Livre? livre = await _service.ModifierLivreCompletAsync(1, formulaire);

            //Alors le livre garde son identite, et seules les valeurs suivent
            Assert.Equal(1, formulaire.Livre.LivreID);
            Assert.Equal("ROM001", formulaire.Livre.Code);
            _depot.Verify(d => d.RemplacerLesValeurs(enBase, formulaire.Livre), Times.Once);
            _depot.Verify(d => d.EnregistrerAsync(), Times.Once);
            Assert.Equal(7, Assert.Single(livre!.Redactions).AuteurID);
        }

        [Fact]
        public async Task SupprimerLivreAsync_NeSupprimeRienQuandLeLivreEstAbsent()
        {
            //Etant donne un identifiant qui ne designe aucun livre
            _depot.Setup(d => d.ObtenirAsync(404)).ReturnsAsync((Livre?)null);

            //Lorsque
            bool supprime = await _service.SupprimerLivreAsync(404);

            //Alors
            Assert.False(supprime);
            _depot.Verify(d => d.Supprimer(It.IsAny<Livre>()), Times.Never);
            _depot.Verify(d => d.EnregistrerAsync(), Times.Never);
        }

        [Fact]
        public async Task SupprimerLivreAsync_SupprimeEtEnregistre()
        {
            //Etant donne un livre au catalogue
            Livre livre = LivreEnRayon();
            _depot.Setup(d => d.ObtenirAsync(1)).ReturnsAsync(livre);

            //Lorsque
            bool supprime = await _service.SupprimerLivreAsync(1);

            //Alors
            Assert.True(supprime);
            _depot.Verify(d => d.Supprimer(livre), Times.Once);
            _depot.Verify(d => d.EnregistrerAsync(), Times.Once);
        }
    }
}
