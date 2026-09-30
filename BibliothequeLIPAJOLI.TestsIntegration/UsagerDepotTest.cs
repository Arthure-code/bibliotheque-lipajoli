using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.TestsIntegration
{
    public class UsagerDepotTest : IDisposable
    {
        private readonly BaseNeuve _base = new BaseNeuve();
        private readonly UsagerDepot _depot;

        public UsagerDepotTest()
        {
            _depot = new UsagerDepot(_base.Context);
        }

        private Usager Inscrit(int identifiant, string nom, string prenom, string abonne,
            TypeAdresse? typeAdresse = null, string rue = "1 rue Principale")
        {
            var usager = new Usager
            {
                ID = identifiant,
                Nom = nom,
                Prenom = prenom,
                Courriel = $"{prenom.ToLowerInvariant()}@example.com",
                NumeroAbonne = abonne,
                Statut = TypeUsager.Etudiant
            };

            _base.Context.Usagers.Add(usager);

            if (typeAdresse.HasValue)
            {
                _base.Context.UsagerAdresses.Add(new UsagerAdresse
                {
                    Usager = usager,
                    Adresse = new Adresse { Rue = rue, Ville = "Québec", Province = "QC", CodePostale = "G1R 2B3" },
                    TypeAdresse = typeAdresse
                });
            }

            _base.Context.SaveChanges();
            _base.Oublier();
            return usager;
        }

        [Fact]
        public async Task ListerAsync_RendTousLesUsagers()
        {
            //Etant donne trois dossiers
            Inscrit(1, "Tremblay", "Marie", "LJ001");
            Inscrit(2, "Gagnon", "Pierre", "LJ002");
            Inscrit(3, "Roy", "Sophie", "LJ003");

            //Lorsque
            List<Usager> usagers = await _depot.ListerAsync();

            //Alors
            Assert.Equal(3, usagers.Count);
        }

        [Fact]
        public async Task ObtenirAvecSesLiensAsync_RamenneLesAdressesEtLesEmprunts()
        {
            //Etant donne un usager avec une adresse et un emprunt
            Inscrit(1, "Tremblay", "Marie", "LJ001", TypeAdresse.Principale);
            _base.Context.Livres.Add(new Livre
            {
                LivreID = 1,
                Titre = "Les Misérables",
                Code = "ROM001",
                CategorieID = 1,
                Isbn10 = "1234567890",
                Isbn13 = "9781234567890",
                Editions = new List<Edition>
                {
                    new Edition
                    {
                        NomEditeur = "Gallimard",
                        Exemplaires = new List<Exemplaire>
                        {
                            new Exemplaire { ExemplaireID = 101, Reference = 1001, NbExemplaire = 2 }
                        }
                    }
                }
            });
            _base.Context.SaveChanges();

            _base.Context.Emprunts.Add(new Emprunt
            {
                UsagerID = 1,
                ExemplaireID = 101,
                DateEmprunt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Unspecified)
            });
            _base.Context.SaveChanges();
            _base.Oublier();

            //Lorsque
            Usager? usager = await _depot.ObtenirAvecSesLiensAsync(1);

            //Alors la fiche arrive complete
            Assert.NotNull(usager);
            UsagerAdresse lien = Assert.Single(usager!.UsagerAdresses);
            Assert.Equal("1 rue Principale", lien.Adresse?.Rue);
            Assert.Single(usager.Emprunts);
            Assert.Equal(1, usager.EmpruntsEnCours);
        }

        [Fact]
        public async Task ObtenirAvecSesLiensAsync_RendNullQuandLeDossierNExistePas()
        {
            Assert.Null(await _depot.ObtenirAvecSesLiensAsync(404));
        }

        [Fact]
        public async Task ObtenirAsync_RendUnUsagerQueLeContexteSuit()
        {
            //Etant donne un dossier
            Inscrit(1, "Tremblay", "Marie", "LJ001");

            //Lorsqu'on le demande pour le modifier
            Usager? usager = await _depot.ObtenirAsync(1);

            //Alors le contexte le suit
            Assert.NotNull(usager);
            Assert.Equal(EntityState.Unchanged, _base.Context.Entry(usager!).State);
        }

        [Theory]
        [InlineData("Trem")]
        [InlineData("Marie")]
        public async Task ChercherAsync_TrouveParLeNomOuParLePrenom(string terme)
        {
            //Etant donne deux dossiers
            Inscrit(1, "Tremblay", "Marie", "LJ001");
            Inscrit(2, "Gagnon", "Pierre", "LJ002");

            //Lorsque
            List<Usager> trouves = await _depot.ChercherAsync(terme);

            //Alors
            Assert.Equal("Tremblay", Assert.Single(trouves).Nom);
        }

        [Fact]
        public async Task ChercherAsync_RendVideQuandPersonneNeCorrespond()
        {
            //Etant donne un dossier
            Inscrit(1, "Tremblay", "Marie", "LJ001");

            //Alors
            Assert.Empty(await _depot.ChercherAsync("Beaulieu"));
        }

        [Fact]
        public async Task ObtenirLAdresseAsync_NeRendQueLeTypeDemande()
        {
            //Etant donne un usager qui a deux adresses
            Inscrit(1, "Tremblay", "Marie", "LJ001", TypeAdresse.Principale, "2 rue Principale");
            _base.Context.UsagerAdresses.Add(new UsagerAdresse
            {
                UsagerID = 1,
                Adresse = new Adresse { Rue = "9 rue du Bureau", Ville = "Lévis" },
                TypeAdresse = TypeAdresse.Bureau
            });
            _base.Context.SaveChanges();
            _base.Oublier();

            //Lorsque
            UsagerAdresse? lien = await _depot.ObtenirLAdresseAsync(1, TypeAdresse.Bureau);

            //Alors
            Assert.Equal("9 rue du Bureau", lien?.Adresse?.Rue);
        }

        [Fact]
        public async Task ObtenirLAdresseAsync_RendNullQuandCeTypeManque()
        {
            //Etant donne un usager qui n'a qu'une adresse principale
            Inscrit(1, "Tremblay", "Marie", "LJ001", TypeAdresse.Principale);

            //Alors
            Assert.Null(await _depot.ObtenirLAdresseAsync(1, TypeAdresse.Bureau));
        }

        [Fact]
        public async Task AjouterUneAdresse_EcritLUsagerLAdresseEtLeLienEnUneFois()
        {
            //Etant donne une inscription complete qui n'est pas encore en base
            var usager = new Usager
            {
                Nom = "Tremblay",
                Prenom = "Marie",
                Courriel = "marie@example.com",
                NumeroAbonne = "LJ001",
                Statut = TypeUsager.Etudiant
            };

            //Lorsque
            _depot.AjouterUneAdresse(new UsagerAdresse
            {
                Usager = usager,
                Adresse = new Adresse { Rue = "12 rue des Lilas", Ville = "Québec" },
                TypeAdresse = TypeAdresse.Principale
            });
            await _depot.EnregistrerAsync();
            _base.Oublier();

            //Alors les trois sont arrives ensemble
            Usager? relu = await _depot.ObtenirAvecSesLiensAsync(usager.ID);
            Assert.NotEqual(0, usager.ID);
            Assert.Equal("Tremblay", relu?.Nom);
            Assert.Equal("12 rue des Lilas", Assert.Single(relu!.UsagerAdresses).Adresse?.Rue);
        }

        [Fact]
        public async Task Supprimer_PuisEnregistrer_RetireLeDossier()
        {
            //Etant donne un dossier que rien ne retient
            Inscrit(1, "Tremblay", "Marie", "LJ001");
            Usager usager = (await _depot.ObtenirAsync(1))!;

            //Lorsque
            _depot.Supprimer(usager);
            await _depot.EnregistrerAsync();
            _base.Oublier();

            //Alors
            Assert.Null(await _depot.ObtenirAsync(1));
        }

        public void Dispose()
        {
            _base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
