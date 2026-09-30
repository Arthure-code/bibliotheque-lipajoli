using BibliothequeLIPAJOLI.Controllers;
using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BibliothequeLIPAJOLI.Tests.Controleurs
{
    public class LivresControllerTest
    {
        private static readonly int[] UnAuteur = { 2 };

        private readonly Mock<ILivreService> _livres = new Mock<ILivreService>();
        private readonly Mock<IReferentielService> _referentiel = new Mock<IReferentielService>();
        private readonly LivresController _controleur;

        public LivresControllerTest()
        {
            _referentiel.Setup(r => r.ObtenirCategories()).Returns(new List<Categorie>
            {
                new Categorie { CategorieID = 1, NomCategorie = "Roman" },
                new Categorie { CategorieID = 2, NomCategorie = "Science-fiction" }
            });
            _referentiel.Setup(r => r.ObtenirAuteurs()).Returns(new List<Auteur>
            {
                new Auteur { ID = 2, Nom = "Hugo", Prenom = "Victor" }
            });

            _controleur = new LivresController(_livres.Object, _referentiel.Object);
        }

        private static Livre Livre(int id = 1) => new Livre
        {
            LivreID = id,
            Code = "ROM001",
            Titre = "Les Misérables",
            CategorieID = 1
        };

        [Fact]
        public async Task Index_PasseLesCriteresAuServiceEtTrieLeResultat()
        {
            //Etant donne un catalogue qui repond a la recherche
            var trouves = new List<Livre> { Livre() };
            _livres.Setup(s => s.RechercherLivresAsync("hugo", 1)).ReturnsAsync(trouves);
            _livres.Setup(s => s.TrierLivres(trouves, "titre")).Returns(trouves);

            //Lorsque
            IActionResult resultat = await _controleur.Index("hugo", 1, "titre");

            //Alors le service a recu les criteres, et la vue recoit la liste
            _livres.Verify(s => s.RechercherLivresAsync("hugo", 1), Times.Once);
            _livres.Verify(s => s.TrierLivres(trouves, "titre"), Times.Once);

            var vue = Assert.IsType<ViewResult>(resultat);
            var modele = Assert.IsType<LivreFiltreViewModel>(vue.Model);
            Assert.Equal("hugo", modele.SearchString);
            Assert.Single(modele.Livres);
            Assert.NotNull(modele.Categories);
        }

        [Fact]
        public async Task Index_SansCritereDemandeToutLeCatalogue()
        {
            //Etant donne aucune recherche
            _livres.Setup(s => s.RechercherLivresAsync(null, null)).ReturnsAsync(new List<Livre>());
            _livres.Setup(s => s.TrierLivres(It.IsAny<List<Livre>>(), string.Empty))
                   .Returns(new List<Livre>());

            //Lorsque
            await _controleur.Index(null, null, null);

            //Alors
            _livres.Verify(s => s.RechercherLivresAsync(null, null), Times.Once);
        }

        [Fact]
        public async Task Details_RetourneLeLivreAvecSesExemplairesEtSesEmprunts()
        {
            //Etant donne un livre dont un exemplaire a ete emprunte
            Livre livre = Livre();
            var emprunt = new Emprunt { EmpruntID = 7, DateEmprunt = new DateTime(2026, 1, 5) };
            var exemplaire = new Exemplaire { ExemplaireID = 101, Emprunts = new List<Emprunt> { emprunt } };
            livre.Editions = new List<Edition>
            {
                new Edition { EditionID = 1, Exemplaires = new List<Exemplaire> { exemplaire } }
            };
            _livres.Setup(s => s.ObtenirLivreParIdAsync(1)).ReturnsAsync(livre);

            //Lorsque
            IActionResult resultat = await _controleur.Details(1);

            //Alors
            var modele = Assert.IsType<LivreDetailsViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Single(modele.Exemplaires);
            Assert.Single(modele.Emprunts);
        }

        [Fact]
        public async Task Details_RetourneIntrouvableQuandLeLivreNExistePas()
        {
            //Etant donne un identifiant qui ne correspond a rien
            _livres.Setup(s => s.ObtenirLivreParIdAsync(404)).ReturnsAsync((Livre?)null);

            //Alors
            Assert.IsType<NotFoundResult>(await _controleur.Details(404));
        }

        [Fact]
        public async Task Create_ProposeLesCategoriesEtLesAuteurs()
        {
            //Lorsque
            IActionResult resultat = await _controleur.Create();

            //Alors les deux listes viennent du referentiel
            var modele = Assert.IsType<LivreCreateViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal(2, modele.Categories.Count());
            Assert.Single(modele.Auteurs);
            Assert.Equal("2", modele.Auteurs.First().Value);
        }

        [Fact]
        public async Task Create_EnregistreLeLivreEtRevientALaListe()
        {
            //Etant donne un formulaire valide
            var formulaire = new LivreCreateViewModel { Livre = Livre(0), Redactions = UnAuteur.ToList() };
            _livres.Setup(s => s.CreerLivreCompletAsync(formulaire)).ReturnsAsync(formulaire.Livre);

            //Lorsque
            IActionResult resultat = await _controleur.Create(formulaire);

            //Alors
            _livres.Verify(s => s.CreerLivreCompletAsync(formulaire), Times.Once);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task Create_NEnregistreRienQuandLeModeleEstInvalide()
        {
            //Etant donne un titre manquant
            var formulaire = new LivreCreateViewModel();
            _controleur.ModelState.AddModelError("Livre.Titre", "Le titre est requis.");

            //Lorsque
            IActionResult resultat = await _controleur.Create(formulaire);

            //Alors rien n'est ecrit, et les listes sont de retour
            _livres.Verify(s => s.CreerLivreCompletAsync(It.IsAny<LivreCreateViewModel>()), Times.Never);
            var modele = Assert.IsType<LivreCreateViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal(2, modele.Categories.Count());
            Assert.Single(modele.Auteurs);
        }

        [Fact]
        public async Task Edit_PresenteLeLivreAvecSaCategorieEtSesAuteursCoches()
        {
            //Etant donne un livre signe par Hugo
            Livre livre = Livre();
            livre.Redactions = new List<Redaction> { new Redaction { LivreID = 1, AuteurID = 2 } };
            _livres.Setup(s => s.ObtenirLivreParIdAsync(1)).ReturnsAsync(livre);

            //Lorsque
            IActionResult resultat = await _controleur.Edit(1);

            //Alors
            var modele = Assert.IsType<LivreEditViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal(UnAuteur, modele.Redactions);
            Assert.True(modele.Categories.Single(c => c.Value == "1").Selected);
            Assert.True(modele.Auteurs.Single(a => a.Value == "2").Selected);
        }

        [Fact]
        public async Task Edit_RetourneIntrouvableSansIdentifiantOuSansLivre()
        {
            _livres.Setup(s => s.ObtenirLivreParIdAsync(404)).ReturnsAsync((Livre?)null);

            Assert.IsType<NotFoundResult>(await _controleur.Edit(null));
            Assert.IsType<NotFoundResult>(await _controleur.Edit(404));
        }

        [Fact]
        public async Task Edit_ModifieLeLivreDesigneParLAdresseEtNonParLeFormulaire()
        {
            //Etant donne un formulaire qui pretend modifier un autre livre
            var formulaire = new LivreEditViewModel { Livre = Livre(99) };
            _livres.Setup(s => s.ModifierLivreCompletAsync(1, formulaire)).ReturnsAsync(formulaire.Livre);

            //Lorsque la requete arrive sur /Livres/Edit/1
            IActionResult resultat = await _controleur.Edit(1, formulaire);

            //Alors c'est le livre 1 qui est modifie, pas le 99
            _livres.Verify(s => s.ModifierLivreCompletAsync(1, formulaire), Times.Once);
            _livres.Verify(s => s.ModifierLivreCompletAsync(99, It.IsAny<LivreEditViewModel>()), Times.Never);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task Edit_EnregistreLesChangementsEtRevientALaListe()
        {
            //Etant donne un formulaire valide
            var formulaire = new LivreEditViewModel { Livre = Livre() };
            _livres.Setup(s => s.ModifierLivreCompletAsync(1, formulaire)).ReturnsAsync(formulaire.Livre);

            //Lorsque
            IActionResult resultat = await _controleur.Edit(1, formulaire);

            //Alors
            _livres.Verify(s => s.ModifierLivreCompletAsync(1, formulaire), Times.Once);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task Edit_RetourneIntrouvableQuandLeLivreADisparuEntreTemps()
        {
            //Etant donne un livre supprime par quelqu'un d'autre
            var formulaire = new LivreEditViewModel { Livre = Livre() };
            _livres.Setup(s => s.ModifierLivreCompletAsync(1, formulaire)).ReturnsAsync((Livre?)null);

            //Alors
            Assert.IsType<NotFoundResult>(await _controleur.Edit(1, formulaire));
        }

        [Fact]
        public async Task DeleteConfirmed_RefuseDeSupprimerUnLivreEmprunte()
        {
            //Etant donne un livre dont un exemplaire est sorti
            Livre livre = Livre();
            livre.Editions = new List<Edition>
            {
                new Edition
                {
                    Exemplaires = new List<Exemplaire>
                    {
                        new Exemplaire { Emprunts = new List<Emprunt> { new Emprunt() } }
                    }
                }
            };
            _livres.Setup(s => s.ObtenirLivreParIdAsync(1)).ReturnsAsync(livre);

            //Lorsque
            IActionResult resultat = await _controleur.DeleteConfirmed(1);

            //Alors la suppression n'a pas lieu et la page l'explique
            _livres.Verify(s => s.SupprimerLivreAsync(It.IsAny<int>()), Times.Never);
            var vue = Assert.IsType<ViewResult>(resultat);
            Assert.Equal("Delete", vue.ViewName);
            Assert.Contains("emprunté", _controleur.ModelState[string.Empty]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task DeleteConfirmed_SupprimeUnLivreQuePersonneNeDetient()
        {
            //Etant donne un livre sans emprunt
            _livres.Setup(s => s.ObtenirLivreParIdAsync(1)).ReturnsAsync(Livre());
            _livres.Setup(s => s.SupprimerLivreAsync(1)).ReturnsAsync(true);

            //Lorsque
            IActionResult resultat = await _controleur.DeleteConfirmed(1);

            //Alors
            _livres.Verify(s => s.SupprimerLivreAsync(1), Times.Once);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_AnnonceLEchecQuandLaBaseRefuse()
        {
            //Etant donne une suppression refusee par la base
            _livres.Setup(s => s.ObtenirLivreParIdAsync(1)).ReturnsAsync(Livre());
            _livres.Setup(s => s.SupprimerLivreAsync(1)).ReturnsAsync(false);

            //Lorsque
            IActionResult resultat = await _controleur.DeleteConfirmed(1);

            //Alors
            var vue = Assert.IsType<ViewResult>(resultat);
            Assert.Equal("Delete", vue.ViewName);
            Assert.NotEmpty(_controleur.ModelState[string.Empty]!.Errors);
        }
    }
}
