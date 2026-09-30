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
        [Fact]
        public async Task Index_PasseLesCriteresAuServiceEtTrieLeResultat()
        {
            //Etant donne un catalogue qui repond a la recherche
            var trouves = new List<Livre> { new Livre { LivreID = 1, Code = "ROM001", Titre = "Les Misérables", CategorieID = 1 } };
            var livres = new Mock<ILivreService>();
            var referentiel = new Mock<IReferentielService>();
            livres.Setup(s => s.RechercherLivresAsync("hugo", 1)).ReturnsAsync(trouves);
            livres.Setup(s => s.TrierLivres(trouves, "titre")).Returns(trouves);
            referentiel.Setup(r => r.ObtenirCategories()).Returns(new List<Categorie>
            {
                new Categorie { CategorieID = 1, NomCategorie = "Roman" },
                new Categorie { CategorieID = 2, NomCategorie = "Science-fiction" }
            });
            var controleur = new LivresController(livres.Object, referentiel.Object);

            //Lorsque
            IActionResult resultat = await controleur.Index("hugo", 1, "titre");

            //Alors le service a recu les criteres, et la vue recoit la liste
            livres.Verify(s => s.RechercherLivresAsync("hugo", 1), Times.Once);
            livres.Verify(s => s.TrierLivres(trouves, "titre"), Times.Once);

            var modele = Assert.IsType<LivreFiltreViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal("hugo", modele.SearchString);
            Assert.Single(modele.Livres);
            Assert.NotNull(modele.Categories);
        }

        [Fact]
        public async Task Index_SansCritereDemandeToutLeCatalogue()
        {
            //Etant donne aucune recherche
            var livres = new Mock<ILivreService>();
            var referentiel = new Mock<IReferentielService>();
            livres.Setup(s => s.RechercherLivresAsync(null, null)).ReturnsAsync(new List<Livre>());
            livres.Setup(s => s.TrierLivres(It.IsAny<List<Livre>>(), string.Empty)).Returns(new List<Livre>());
            referentiel.Setup(r => r.ObtenirCategories()).Returns(new List<Categorie>());
            var controleur = new LivresController(livres.Object, referentiel.Object);

            //Lorsque
            await controleur.Index(null, null, null);

            //Alors
            livres.Verify(s => s.RechercherLivresAsync(null, null), Times.Once);
        }

        [Fact]
        public async Task Details_RetourneLeLivreAvecSesExemplairesEtSesEmprunts()
        {
            //Etant donne un livre dont un exemplaire a ete emprunte
            var exemplaire = new Exemplaire
            {
                ExemplaireID = 101,
                Emprunts = new List<Emprunt> { new Emprunt { EmpruntID = 7, DateEmprunt = new DateTime(2026, 1, 5) } }
            };
            var livre = new Livre
            {
                LivreID = 1,
                Code = "ROM001",
                Titre = "Les Misérables",
                CategorieID = 1,
                Editions = new List<Edition>
                {
                    new Edition { EditionID = 1, Exemplaires = new List<Exemplaire> { exemplaire } }
                }
            };
            var livres = new Mock<ILivreService>();
            livres.Setup(s => s.ObtenirLivreParIdAsync(1)).ReturnsAsync(livre);
            var controleur = new LivresController(livres.Object, new Mock<IReferentielService>().Object);

            //Lorsque
            IActionResult resultat = await controleur.Details(1);

            //Alors
            var modele = Assert.IsType<LivreDetailsViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Single(modele.Exemplaires);
            Assert.Single(modele.Emprunts);
        }

        [Fact]
        public async Task Details_RetourneIntrouvableQuandLeLivreNExistePas()
        {
            //Etant donne un identifiant qui ne correspond a rien
            var livres = new Mock<ILivreService>();
            livres.Setup(s => s.ObtenirLivreParIdAsync(404)).ReturnsAsync((Livre?)null);
            var controleur = new LivresController(livres.Object, new Mock<IReferentielService>().Object);

            //Alors
            Assert.IsType<NotFoundResult>(await controleur.Details(404));
        }

        [Fact]
        public async Task Create_ProposeLesCategoriesEtLesAuteurs()
        {
            //Etant donne un referentiel qui porte deux categories et un auteur
            var referentiel = new Mock<IReferentielService>();
            referentiel.Setup(r => r.ObtenirCategories()).Returns(new List<Categorie>
            {
                new Categorie { CategorieID = 1, NomCategorie = "Roman" },
                new Categorie { CategorieID = 2, NomCategorie = "Science-fiction" }
            });
            referentiel.Setup(r => r.ObtenirAuteurs()).Returns(new List<Auteur>
            {
                new Auteur { ID = 2, Nom = "Hugo", Prenom = "Victor" }
            });
            var controleur = new LivresController(new Mock<ILivreService>().Object, referentiel.Object);

            //Lorsque
            IActionResult resultat = await controleur.Create();

            //Alors les deux listes viennent du referentiel
            var modele = Assert.IsType<LivreFormulaireViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal(2, modele.Categories.Count());
            Assert.Single(modele.Auteurs);
            Assert.Equal("2", modele.Auteurs.First().Value);
        }

        [Fact]
        public async Task Create_EnregistreLeLivreEtRevientALaListe()
        {
            //Etant donne un formulaire valide
            var formulaire = new LivreFormulaireViewModel
            {
                Livre = new Livre { Code = "ROM001", Titre = "Les Misérables", CategorieID = 1 },
                Redactions = new List<int> { 2 }
            };
            var livres = new Mock<ILivreService>();
            livres.Setup(s => s.CreerLivreCompletAsync(formulaire)).ReturnsAsync(formulaire.Livre);
            var controleur = new LivresController(livres.Object, new Mock<IReferentielService>().Object);

            //Lorsque
            IActionResult resultat = await controleur.Create(formulaire);

            //Alors
            livres.Verify(s => s.CreerLivreCompletAsync(formulaire), Times.Once);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task Create_NEnregistreRienQuandLeModeleEstInvalide()
        {
            //Etant donne un titre manquant
            var livres = new Mock<ILivreService>();
            var referentiel = new Mock<IReferentielService>();
            referentiel.Setup(r => r.ObtenirCategories()).Returns(new List<Categorie>
            {
                new Categorie { CategorieID = 1, NomCategorie = "Roman" },
                new Categorie { CategorieID = 2, NomCategorie = "Science-fiction" }
            });
            referentiel.Setup(r => r.ObtenirAuteurs()).Returns(new List<Auteur>
            {
                new Auteur { ID = 2, Nom = "Hugo", Prenom = "Victor" }
            });
            var controleur = new LivresController(livres.Object, referentiel.Object);
            controleur.ModelState.AddModelError("Livre.Titre", "Le titre est requis.");

            //Lorsque
            IActionResult resultat = await controleur.Create(new LivreFormulaireViewModel());

            //Alors rien n'est ecrit, et les listes sont de retour
            livres.Verify(s => s.CreerLivreCompletAsync(It.IsAny<LivreFormulaireViewModel>()), Times.Never);
            var modele = Assert.IsType<LivreFormulaireViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal(2, modele.Categories.Count());
            Assert.Single(modele.Auteurs);
        }

        [Fact]
        public async Task Edit_PresenteLeLivreAvecSaCategorieEtSesAuteursCoches()
        {
            //Etant donne un livre signe par Hugo
            var livre = new Livre
            {
                LivreID = 1,
                Code = "ROM001",
                Titre = "Les Misérables",
                CategorieID = 1,
                Redactions = new List<Redaction> { new Redaction { LivreID = 1, AuteurID = 2 } }
            };
            var livres = new Mock<ILivreService>();
            var referentiel = new Mock<IReferentielService>();
            livres.Setup(s => s.ObtenirLivreParIdAsync(1)).ReturnsAsync(livre);
            referentiel.Setup(r => r.ObtenirCategories()).Returns(new List<Categorie>
            {
                new Categorie { CategorieID = 1, NomCategorie = "Roman" },
                new Categorie { CategorieID = 2, NomCategorie = "Science-fiction" }
            });
            referentiel.Setup(r => r.ObtenirAuteurs()).Returns(new List<Auteur>
            {
                new Auteur { ID = 2, Nom = "Hugo", Prenom = "Victor" }
            });
            var controleur = new LivresController(livres.Object, referentiel.Object);

            //Lorsque
            IActionResult resultat = await controleur.Edit(1);

            //Alors la categorie du livre et son auteur sont deja coches
            var modele = Assert.IsType<LivreFormulaireViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal(new[] { 2 }, modele.Redactions);
            Assert.True(modele.Categories.Single(c => c.Value == "1").Selected);
            Assert.True(modele.Auteurs.Single(a => a.Value == "2").Selected);
        }

        [Fact]
        public async Task Edit_RetourneIntrouvableSansIdentifiantOuSansLivre()
        {
            //Etant donne un identifiant qui ne designe aucun livre
            var livres = new Mock<ILivreService>();
            livres.Setup(s => s.ObtenirLivreParIdAsync(404)).ReturnsAsync((Livre?)null);
            var controleur = new LivresController(livres.Object, new Mock<IReferentielService>().Object);

            //Alors l'absence d'adresse et l'absence de livre se valent
            Assert.IsType<NotFoundResult>(await controleur.Edit(null));
            Assert.IsType<NotFoundResult>(await controleur.Edit(404));
        }

        [Fact]
        public async Task Edit_ModifieLeLivreDesigneParLAdresseEtNonParLeFormulaire()
        {
            //Etant donne un formulaire qui pretend modifier un autre livre
            var formulaire = new LivreFormulaireViewModel
            {
                Livre = new Livre { LivreID = 99, Code = "ROM001", Titre = "Les Misérables", CategorieID = 1 }
            };
            var livres = new Mock<ILivreService>();
            livres.Setup(s => s.ModifierLivreCompletAsync(1, formulaire)).ReturnsAsync(formulaire.Livre);
            var controleur = new LivresController(livres.Object, new Mock<IReferentielService>().Object);

            //Lorsque la requete arrive sur /Livres/Edit/1
            IActionResult resultat = await controleur.Edit(1, formulaire);

            //Alors c'est le livre 1 qui est modifie, pas le 99
            livres.Verify(s => s.ModifierLivreCompletAsync(1, formulaire), Times.Once);
            livres.Verify(s => s.ModifierLivreCompletAsync(99, It.IsAny<LivreFormulaireViewModel>()), Times.Never);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task Edit_EnregistreLesChangementsEtRevientALaListe()
        {
            //Etant donne un formulaire valide
            var formulaire = new LivreFormulaireViewModel
            {
                Livre = new Livre { LivreID = 1, Code = "ROM001", Titre = "Les Misérables", CategorieID = 1 }
            };
            var livres = new Mock<ILivreService>();
            livres.Setup(s => s.ModifierLivreCompletAsync(1, formulaire)).ReturnsAsync(formulaire.Livre);
            var controleur = new LivresController(livres.Object, new Mock<IReferentielService>().Object);

            //Lorsque
            IActionResult resultat = await controleur.Edit(1, formulaire);

            //Alors
            livres.Verify(s => s.ModifierLivreCompletAsync(1, formulaire), Times.Once);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task Edit_RetourneIntrouvableQuandLeLivreADisparuEntreTemps()
        {
            //Etant donne un livre supprime par quelqu'un d'autre
            var formulaire = new LivreFormulaireViewModel
            {
                Livre = new Livre { LivreID = 1, Code = "ROM001", Titre = "Les Misérables", CategorieID = 1 }
            };
            var livres = new Mock<ILivreService>();
            livres.Setup(s => s.ModifierLivreCompletAsync(1, formulaire)).ReturnsAsync((Livre?)null);
            var controleur = new LivresController(livres.Object, new Mock<IReferentielService>().Object);

            //Alors
            Assert.IsType<NotFoundResult>(await controleur.Edit(1, formulaire));
        }

        [Fact]
        public async Task DeleteConfirmed_RefuseDeSupprimerUnLivreEmprunte()
        {
            //Etant donne un livre dont un exemplaire est sorti
            var livre = new Livre
            {
                LivreID = 1,
                Code = "ROM001",
                Titre = "Les Misérables",
                CategorieID = 1,
                Editions = new List<Edition>
                {
                    new Edition
                    {
                        Exemplaires = new List<Exemplaire>
                        {
                            new Exemplaire { Emprunts = new List<Emprunt> { new Emprunt() } }
                        }
                    }
                }
            };
            var livres = new Mock<ILivreService>();
            livres.Setup(s => s.ObtenirLivreParIdAsync(1)).ReturnsAsync(livre);
            var controleur = new LivresController(livres.Object, new Mock<IReferentielService>().Object);

            //Lorsque
            IActionResult resultat = await controleur.DeleteConfirmed(1);

            //Alors la suppression n'a pas lieu et la page l'explique
            livres.Verify(s => s.SupprimerLivreAsync(It.IsAny<int>()), Times.Never);
            var vue = Assert.IsType<ViewResult>(resultat);
            Assert.Equal("Delete", vue.ViewName);
            Assert.Contains("emprunté", controleur.ModelState[string.Empty]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task DeleteConfirmed_SupprimeUnLivreQuePersonneNeDetient()
        {
            //Etant donne un livre sans emprunt
            var livres = new Mock<ILivreService>();
            livres.Setup(s => s.ObtenirLivreParIdAsync(1))
                .ReturnsAsync(new Livre { LivreID = 1, Code = "ROM001", Titre = "Les Misérables", CategorieID = 1 });
            livres.Setup(s => s.SupprimerLivreAsync(1)).ReturnsAsync(true);
            var controleur = new LivresController(livres.Object, new Mock<IReferentielService>().Object);

            //Lorsque
            IActionResult resultat = await controleur.DeleteConfirmed(1);

            //Alors
            livres.Verify(s => s.SupprimerLivreAsync(1), Times.Once);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_AnnonceLEchecQuandLaBaseRefuse()
        {
            //Etant donne une suppression refusee par la base
            var livres = new Mock<ILivreService>();
            livres.Setup(s => s.ObtenirLivreParIdAsync(1))
                .ReturnsAsync(new Livre { LivreID = 1, Code = "ROM001", Titre = "Les Misérables", CategorieID = 1 });
            livres.Setup(s => s.SupprimerLivreAsync(1)).ReturnsAsync(false);
            var controleur = new LivresController(livres.Object, new Mock<IReferentielService>().Object);

            //Lorsque
            IActionResult resultat = await controleur.DeleteConfirmed(1);

            //Alors
            var vue = Assert.IsType<ViewResult>(resultat);
            Assert.Equal("Delete", vue.ViewName);
            Assert.NotEmpty(controleur.ModelState[string.Empty]!.Errors);
        }
    }
}
