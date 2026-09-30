using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.Services;
using BibliothequeLIPAJOLI.ViewModels;
using Moq;

namespace BibliothequeLIPAJOLI.Tests.Services
{
    public class GestionUsagerTests
    {
        private readonly Mock<IUsagerDepot> _depot = new Mock<IUsagerDepot>();
        private readonly GestionUsager _service;

        public GestionUsagerTests()
        {
            _service = new GestionUsager(_depot.Object);
        }

        private static Usager Marie() => new Usager
        {
            ID = 1,
            Nom = "Tremblay",
            Prenom = "Marie",
            Courriel = "marie.tremblay@example.com",
            NumeroAbonne = "AB-001",
            Statut = TypeUsager.Etudiant
        };

        private static UsagerCreateViewModel Formulaire() => new UsagerCreateViewModel
        {
            Usager = new Usager
            {
                Nom = "Tremblay-Roy",
                Prenom = "Marie",
                Courriel = "marie.roy@example.com",
                NumeroAbonne = "AB-002",
                Statut = TypeUsager.Enseignant
            },
            Adresse = new Adresse
            {
                Rue = "12 rue des Lilas",
                Ville = "Québec",
                Province = "QC",
                CodePostale = "G1R 2B3"
            },
            TypeAdresse = TypeAdresse.Principale
        };

        [Fact]
        public async Task ObtenirTousLesUsagersAsync_RendCeQueLeDepotDonne()
        {
            //Etant donne un usager au dossier
            _depot.Setup(d => d.ListerAsync()).ReturnsAsync(new List<Usager> { Marie() });

            //Lorsque
            IEnumerable<Usager> usagers = await _service.ObtenirTousLesUsagersAsync();

            //Alors
            Assert.Equal("Tremblay", Assert.Single(usagers).Nom);
        }

        [Fact]
        public async Task RechercherUsagersAsync_PasseLeTermeAuDepot()
        {
            //Etant donne une recherche sur un nom
            _depot.Setup(d => d.ChercherAsync("Trem")).ReturnsAsync(new List<Usager> { Marie() });

            //Lorsque
            IEnumerable<Usager> trouves = await _service.RechercherUsagersAsync("Trem");

            //Alors
            Assert.Single(trouves);
            _depot.Verify(d => d.ChercherAsync("Trem"), Times.Once);
        }

        [Fact]
        public async Task SupprimerUsagerAsync_NeToucheARienQuandLUsagerEstAbsent()
        {
            //Etant donne un identifiant qui ne designe personne
            _depot.Setup(d => d.ObtenirAsync(404)).ReturnsAsync((Usager?)null);

            //Lorsque
            await _service.SupprimerUsagerAsync(404);

            //Alors
            _depot.Verify(d => d.Supprimer(It.IsAny<Usager>()), Times.Never);
            _depot.Verify(d => d.EnregistrerAsync(), Times.Never);
        }

        [Fact]
        public async Task SupprimerUsagerAsync_SupprimeEtEnregistre()
        {
            //Etant donne un usager au dossier
            Usager usager = Marie();
            _depot.Setup(d => d.ObtenirAsync(1)).ReturnsAsync(usager);

            //Lorsque
            await _service.SupprimerUsagerAsync(1);

            //Alors
            _depot.Verify(d => d.Supprimer(usager), Times.Once);
            _depot.Verify(d => d.EnregistrerAsync(), Times.Once);
        }

        [Fact]
        public async Task AjouterUsagerCompletAsync_EcritLUsagerSonAdresseEtLeLienEnUneFois()
        {
            //Etant donne une inscription complete
            UsagerCreateViewModel formulaire = Formulaire();
            UsagerAdresse? ecrit = null;
            _depot.Setup(d => d.AjouterUneAdresse(It.IsAny<UsagerAdresse>()))
                .Callback<UsagerAdresse>(lien => ecrit = lien);

            //Lorsque
            await _service.AjouterUsagerCompletAsync(formulaire);

            //Alors un seul enregistrement porte les trois
            Assert.NotNull(ecrit);
            Assert.Same(formulaire.Usager, ecrit!.Usager);
            Assert.Same(formulaire.Adresse, ecrit.Adresse);
            Assert.Equal(TypeAdresse.Principale, ecrit.TypeAdresse);
            _depot.Verify(d => d.EnregistrerAsync(), Times.Once);
        }

        [Fact]
        public async Task AjouterUsagerCompletAsync_RefuseUnFormulaireAbsent()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.AjouterUsagerCompletAsync(null!));
        }

        [Fact]
        public async Task MettreAJourUsagerCompletAsync_NeToucheARienQuandLUsagerEstAbsent()
        {
            //Etant donne un identifiant qui ne designe personne
            _depot.Setup(d => d.ObtenirAsync(404)).ReturnsAsync((Usager?)null);

            //Lorsque
            await _service.MettreAJourUsagerCompletAsync(404, Formulaire());

            //Alors
            _depot.Verify(d => d.EnregistrerAsync(), Times.Never);
        }

        [Fact]
        public async Task MettreAJourUsagerCompletAsync_ReprendLesChampsSansToucherALaDefaillance()
        {
            //Etant donne un usager qui porte deja une defaillance
            Usager usager = Marie();
            usager.PorterUneDefaillance();
            _depot.Setup(d => d.ObtenirAsync(1)).ReturnsAsync(usager);
            _depot.Setup(d => d.ObtenirLAdresseAsync(1, TypeAdresse.Principale))
                .ReturnsAsync((UsagerAdresse?)null);

            //Lorsque le formulaire change son nom et son statut
            await _service.MettreAJourUsagerCompletAsync(1, Formulaire());

            //Alors seuls les champs du formulaire bougent
            Assert.Equal("Tremblay-Roy", usager.Nom);
            Assert.Equal("marie.roy@example.com", usager.Courriel);
            Assert.Equal(TypeUsager.Enseignant, usager.Statut);
            Assert.Equal(1, usager.Defaillance);
        }

        [Fact]
        public async Task MettreAJourUsagerCompletAsync_CorrigeLAdresseQuandElleExisteDeja()
        {
            //Etant donne une adresse principale deja au dossier
            var adresse = new Adresse { Rue = "1 rue Ancienne", Ville = "Lévis" };
            _depot.Setup(d => d.ObtenirAsync(1)).ReturnsAsync(Marie());
            _depot.Setup(d => d.ObtenirLAdresseAsync(1, TypeAdresse.Principale))
                .ReturnsAsync(new UsagerAdresse { UsagerID = 1, Adresse = adresse });

            //Lorsque
            await _service.MettreAJourUsagerCompletAsync(1, Formulaire());

            //Alors l'adresse est corrigee, pas doublee
            Assert.Equal("12 rue des Lilas", adresse.Rue);
            Assert.Equal("Québec", adresse.Ville);
            _depot.Verify(d => d.AjouterUneAdresse(It.IsAny<UsagerAdresse>()), Times.Never);
            _depot.Verify(d => d.EnregistrerAsync(), Times.Once);
        }

        [Fact]
        public async Task MettreAJourUsagerCompletAsync_AjouteLAdresseQuandLeTypeManqueAuDossier()
        {
            //Etant donne un usager sans adresse de ce type
            _depot.Setup(d => d.ObtenirAsync(1)).ReturnsAsync(Marie());
            _depot.Setup(d => d.ObtenirLAdresseAsync(1, TypeAdresse.Principale))
                .ReturnsAsync((UsagerAdresse?)null);

            UsagerAdresse? ecrit = null;
            _depot.Setup(d => d.AjouterUneAdresse(It.IsAny<UsagerAdresse>()))
                .Callback<UsagerAdresse>(lien => ecrit = lien);

            //Lorsque
            await _service.MettreAJourUsagerCompletAsync(1, Formulaire());

            //Alors elle entre au dossier, rattachee au bon usager
            Assert.NotNull(ecrit);
            Assert.Equal(1, ecrit!.UsagerID);
            Assert.Equal("12 rue des Lilas", ecrit.Adresse?.Rue);
            _depot.Verify(d => d.EnregistrerAsync(), Times.Once);
        }

        [Fact]
        public async Task MettreAJourUsagerCompletAsync_RefuseUnFormulaireAbsent()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.MettreAJourUsagerCompletAsync(1, null!));
        }
    }
}
