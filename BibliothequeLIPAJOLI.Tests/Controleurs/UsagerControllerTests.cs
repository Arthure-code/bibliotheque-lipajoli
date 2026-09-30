using BibliothequeLIPAJOLI.Controllers;
using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.Tests.Doubles;
using BibliothequeLIPAJOLI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BibliothequeLIPAJOLI.Tests.Controleurs
{
    public class UsagerControllerTests
    {
        private readonly Mock<IUsagerService> _usagers = new Mock<IUsagerService>();
        private readonly UsagerController _controleur;

        public UsagerControllerTests()
        {
            _controleur = Controleur.Preparer(new UsagerController(_usagers.Object));
        }

        private static Usager Usager(int id = 1) => new Usager
        {
            ID = id,
            Nom = "Tremblay",
            Prenom = "Marie",
            Courriel = "marie.tremblay@example.com",
            NumeroAbonne = "LJ001",
            Statut = TypeUsager.Etudiant
        };

        [Fact]
        public async Task Index_SansRechercheDemandeToutLeMonde()
        {
            //Etant donne trois usagers enregistres
            _usagers.Setup(s => s.ObtenirTousLesUsagersAsync())
                    .ReturnsAsync(new List<Usager> { Usager(), Usager(2), Usager(3) });

            //Lorsque
            IActionResult resultat = await _controleur.Index(string.Empty);

            //Alors
            _usagers.Verify(s => s.ObtenirTousLesUsagersAsync(), Times.Once);
            _usagers.Verify(s => s.RechercherUsagersAsync(It.IsAny<string>()), Times.Never);
            Assert.Equal(3, Assert.IsAssignableFrom<IEnumerable<Usager>>(
                Assert.IsType<ViewResult>(resultat).Model).Count());
        }

        [Fact]
        public async Task Index_AvecRechercheNePasseQueParLaRecherche()
        {
            //Etant donne une recherche sur un nom
            _usagers.Setup(s => s.RechercherUsagersAsync("trem"))
                    .ReturnsAsync(new List<Usager> { Usager() });

            //Lorsque
            IActionResult resultat = await _controleur.Index("trem");

            //Alors
            _usagers.Verify(s => s.RechercherUsagersAsync("trem"), Times.Once);
            _usagers.Verify(s => s.ObtenirTousLesUsagersAsync(), Times.Never);
            Assert.Single(Assert.IsAssignableFrom<IEnumerable<Usager>>(
                Assert.IsType<ViewResult>(resultat).Model));
        }

        [Fact]
        public async Task Details_RetourneLUsagerDemande()
        {
            //Etant donne un usager enregistre
            _usagers.Setup(s => s.ObtenirUsagerParIdAsync(1)).ReturnsAsync(Usager());

            //Lorsque
            IActionResult resultat = await _controleur.Details(1);

            //Alors
            Assert.Equal("Tremblay", Assert.IsType<Usager>(Assert.IsType<ViewResult>(resultat).Model).Nom);
        }

        [Fact]
        public async Task Details_RetourneIntrouvableQuandLeDossierNExistePas()
        {
            _usagers.Setup(s => s.ObtenirUsagerParIdAsync(404)).ReturnsAsync((Usager?)null);

            Assert.IsType<NotFoundResult>(await _controleur.Details(404));
        }

        [Fact]
        public async Task Create_EnregistreLUsagerEtRevientALaListe()
        {
            //Etant donne un formulaire valide
            var formulaire = new UsagerCreateViewModel { Usager = Usager(0) };

            //Lorsque
            IActionResult resultat = await _controleur.Create(formulaire);

            //Alors
            _usagers.Verify(s => s.AjouterUsagerCompletAsync(formulaire), Times.Once);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task Create_NEnregistreRienQuandLeModeleEstInvalide()
        {
            //Etant donne un courriel manquant
            var formulaire = new UsagerCreateViewModel();
            _controleur.ModelState.AddModelError("Usager.Courriel", "Le courriel est requis.");

            //Lorsque
            IActionResult resultat = await _controleur.Create(formulaire);

            //Alors
            _usagers.Verify(s => s.AjouterUsagerCompletAsync(It.IsAny<UsagerCreateViewModel>()), Times.Never);
            Assert.IsType<ViewResult>(resultat);
        }

        [Fact]
        public async Task Edit_PresenteLUsagerAvecSonAdressePrincipale()
        {
            //Etant donne un usager qui a une adresse principale et une autre
            Usager usager = Usager();
            usager.UsagerAdresses = new List<UsagerAdresse>
            {
                new UsagerAdresse { TypeAdresse = TypeAdresse.Bureau, Adresse = new Adresse { Rue = "1 rue du Bureau" } },
                new UsagerAdresse { TypeAdresse = TypeAdresse.Principale, Adresse = new Adresse { Rue = "2 rue Principale" } }
            };
            _usagers.Setup(s => s.ObtenirUsagerParIdAsync(1)).ReturnsAsync(usager);

            //Lorsque
            IActionResult resultat = await _controleur.Edit(1);

            //Alors c'est bien la principale qui est proposee
            var modele = Assert.IsType<UsagerCreateViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal("2 rue Principale", modele.Adresse.Rue);
            Assert.Equal(TypeAdresse.Principale, modele.TypeAdresse);
        }

        [Fact]
        public async Task Edit_RetourneIntrouvableQuandLeDossierNExistePas()
        {
            _usagers.Setup(s => s.ObtenirUsagerParIdAsync(404)).ReturnsAsync((Usager?)null);

            Assert.IsType<NotFoundResult>(await _controleur.Edit(404));
        }

        [Fact]
        public async Task Edit_MetAJourLeDossierDesigneParLAdresseEtNonParLeFormulaire()
        {
            //Etant donne un formulaire qui pretend modifier un autre dossier
            var formulaire = new UsagerCreateViewModel { Usager = Usager(99) };

            //Lorsque la requete arrive sur /Usager/Edit/1
            IActionResult resultat = await _controleur.Edit(1, formulaire);

            //Alors c'est le dossier 1 qui est mis a jour, pas le 99
            _usagers.Verify(s => s.MettreAJourUsagerCompletAsync(1, formulaire), Times.Once);
            _usagers.Verify(s => s.MettreAJourUsagerCompletAsync(99, It.IsAny<UsagerCreateViewModel>()), Times.Never);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task Edit_NEnregistreRienQuandLeModeleEstInvalide()
        {
            //Etant donne un nom manquant
            var formulaire = new UsagerCreateViewModel();
            _controleur.ModelState.AddModelError("Usager.Nom", "Le nom est requis.");

            //Lorsque
            IActionResult resultat = await _controleur.Edit(1, formulaire);

            //Alors
            _usagers.Verify(s => s.MettreAJourUsagerCompletAsync(It.IsAny<int>(), It.IsAny<UsagerCreateViewModel>()),
                Times.Never);
            Assert.IsType<ViewResult>(resultat);
        }

        [Fact]
        public async Task Delete_PresenteLeDossierAvantDeLeSupprimer()
        {
            //Etant donne un dossier au fichier
            _usagers.Setup(s => s.ObtenirUsagerParIdAsync(1)).ReturnsAsync(Usager());

            //Lorsque la page de confirmation s'ouvre
            IActionResult resultat = await _controleur.Delete(1);

            //Alors elle montre le dossier, sans message d'echec
            var vue = Assert.IsType<ViewResult>(resultat);
            Assert.Equal(1, Assert.IsType<Usager>(vue.Model).ID);
            Assert.Null(_controleur.ViewData["MessageErreur"]);
        }

        [Fact]
        public async Task Delete_RetourneIntrouvableQuandLeDossierNExistePas()
        {
            //Etant donne un identifiant qui ne designe personne
            _usagers.Setup(s => s.ObtenirUsagerParIdAsync(404)).ReturnsAsync((Usager?)null);

            //Alors
            Assert.IsType<NotFoundResult>(await _controleur.Delete(404));
        }

        [Fact]
        public async Task Delete_ExpliqueLEchecQuandLaSuppressionARate()
        {
            //Etant donne un retour de la suppression qui a echoue
            _usagers.Setup(s => s.ObtenirUsagerParIdAsync(1)).ReturnsAsync(Usager());

            //Lorsque la page se rouvre
            IActionResult resultat = await _controleur.Delete(1, suppressionImpossible: true);

            //Alors le visiteur lit pourquoi, au lieu d'une page blanche
            Assert.IsType<ViewResult>(resultat);
            Assert.Contains("La suppression a échoué",
                _controleur.ViewData["MessageErreur"]?.ToString());
        }

        [Fact]
        public async Task DeleteConfirmed_SupprimeLeDossierEtRevientALaListe()
        {
            //Lorsque
            IActionResult resultat = await _controleur.DeleteConfirmed(1);

            //Alors
            _usagers.Verify(s => s.SupprimerUsagerAsync(1), Times.Once);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_RenvoieSurLaPageQuandLaBaseRefuse()
        {
            //Etant donne un usager encore lie a un emprunt
            _usagers.Setup(s => s.SupprimerUsagerAsync(1))
                .ThrowsAsync(new DbUpdateException("La cle etrangere retient le dossier."));

            //Lorsque
            IActionResult resultat = await _controleur.DeleteConfirmed(1);

            //Alors la page revient en annoncant l'echec, sans s'effondrer
            var redirection = Assert.IsType<RedirectToActionResult>(resultat);
            Assert.Equal("Delete", redirection.ActionName);
            Assert.Equal(1, redirection.RouteValues!["id"]);
            Assert.True(Assert.IsType<bool>(redirection.RouteValues["suppressionImpossible"]));
        }
    }
}
