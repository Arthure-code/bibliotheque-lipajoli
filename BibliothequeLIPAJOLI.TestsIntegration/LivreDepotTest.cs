using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.TestsIntegration
{
    public class LivreDepotTest : IDisposable
    {
        private readonly BaseNeuve _base = new BaseNeuve();
        private readonly LivreDepot _depot;

        public LivreDepotTest()
        {
            _depot = new LivreDepot(_base.Context);
        }

        private Livre Catalogue(int identifiant, string titre, string code, int categorie = 1,
            int auteur = 7, int exemplaires = 2, int sortis = 0)
        {
            var livre = new Livre
            {
                LivreID = identifiant,
                Titre = titre,
                Code = code,
                CategorieID = categorie,
                Isbn10 = "1234567890",
                Isbn13 = "9781234567890",
                Redactions = new List<Redaction> { new Redaction { AuteurID = auteur, Role = "Auteur", Ordre = 1 } },
                Editions = new List<Edition>
                {
                    new Edition
                    {
                        NomEditeur = "Gallimard",
                        Exemplaires = new List<Exemplaire>
                        {
                            new Exemplaire
                            {
                                Reference = 1000 + identifiant,
                                NbExemplaire = exemplaires,
                                Emprunts = Enumerable.Range(0, sortis)
                                    .Select(_ => new Emprunt { UsagerID = 1, DateRetour = null })
                                    .ToList()
                            }
                        }
                    }
                }
            };

            if (sortis > 0 && !_base.Context.Usagers.Any())
            {
                // Un emprunt designe un usager : la base refuse le contraire.
                _base.Context.Usagers.Add(new Usager
                {
                    ID = 1,
                    Nom = "Tremblay",
                    Prenom = "Marie",
                    Courriel = "marie@example.com",
                    NumeroAbonne = "LJ001",
                    Statut = TypeUsager.Etudiant
                });
            }

            _base.Context.Livres.Add(livre);
            _base.Context.SaveChanges();
            _base.Oublier();
            return livre;
        }

        [Fact]
        public async Task ObtenirAvecSesLiensAsync_RamenneLesEditionsLesExemplairesEtLesEmprunts()
        {
            //Etant donne un livre en rayon dont un exemplaire est sorti
            Catalogue(1, "Les Misérables", "ROM001", exemplaires: 3, sortis: 1);

            //Lorsque le catalogue le charge
            Livre? livre = await _depot.ObtenirAvecSesLiensAsync(1);

            //Alors tout ce que la fiche montre est venu avec lui
            Assert.NotNull(livre);
            Assert.Single(livre!.Redactions);
            Edition edition = Assert.Single(livre.Editions);
            Exemplaire exemplaire = Assert.Single(edition.Exemplaires);
            Assert.Single(exemplaire.Emprunts);
            Assert.Equal(3, livre.Quantite);
            Assert.Equal(2, livre.QuantiteDisponible);
        }

        [Fact]
        public async Task ObtenirAvecSesLiensAsync_RendNullQuandLeLivreNExistePas()
        {
            Assert.Null(await _depot.ObtenirAvecSesLiensAsync(404));
        }

        [Fact]
        public async Task ObtenirPourModificationAsync_RendUnLivreQueLeContexteSuit()
        {
            //Etant donne un livre au catalogue
            Catalogue(1, "Dune", "SCI001");

            //Lorsqu'on le demande pour le modifier
            Livre? livre = await _depot.ObtenirPourModificationAsync(1);

            //Alors le contexte le suit, sinon aucune correction ne partirait
            Assert.NotNull(livre);
            Assert.Equal(EntityState.Unchanged, _base.Context.Entry(livre!).State);
        }

        [Fact]
        public async Task ObtenirAsync_RendLeLivreSeul()
        {
            //Etant donne un livre au catalogue
            Catalogue(1, "Dune", "SCI001");

            //Lorsque
            Livre? livre = await _depot.ObtenirAsync(1);

            //Alors
            Assert.Equal("Dune", livre?.Titre);
        }

        [Fact]
        public async Task ChercherAsync_SansCritereRendToutLeCatalogue()
        {
            //Etant donne deux livres
            Catalogue(1, "Les Misérables", "ROM001");
            Catalogue(2, "Dune", "SCI001", categorie: 2, auteur: 5);

            //Lorsque personne ne cherche rien
            List<Livre> livres = await _depot.ChercherAsync(null, Array.Empty<int>(), null);

            //Alors
            Assert.Equal(2, livres.Count);
        }

        [Theory]
        [InlineData("dune")]
        [InlineData("DUNE")]
        [InlineData("un")]
        public async Task ChercherAsync_LeTitreSeChercheSansSoucierDeLaCasse(string terme)
        {
            //Etant donne deux livres
            Catalogue(1, "Les Misérables", "ROM001");
            Catalogue(2, "Dune", "SCI001", categorie: 2, auteur: 5);

            //Lorsque
            List<Livre> livres = await _depot.ChercherAsync(terme, Array.Empty<int>(), null);

            //Alors
            Assert.Equal("Dune", Assert.Single(livres).Titre);
        }

        [Fact]
        public async Task ChercherAsync_UnAuteurRamenneLesLivresQuIlASignes()
        {
            //Etant donne deux livres de deux auteurs differents
            Catalogue(1, "Les Misérables", "ROM001", auteur: 7);
            Catalogue(2, "Dune", "SCI001", categorie: 2, auteur: 5);

            //Lorsque le terme ne ressemble a aucun titre, mais designe un auteur
            List<Livre> livres = await _depot.ChercherAsync("herbert", new[] { 5 }, null);

            //Alors
            Assert.Equal("Dune", Assert.Single(livres).Titre);
        }

        [Fact]
        public async Task ChercherAsync_LaCategorieRestreintCeQuiEstTrouve()
        {
            //Etant donne deux livres de deux categories
            Catalogue(1, "Les Misérables", "ROM001", categorie: 1);
            Catalogue(2, "Dune", "SCI001", categorie: 2, auteur: 5);

            //Lorsque le filtre porte sur la categorie seule
            List<Livre> livres = await _depot.ChercherAsync(null, Array.Empty<int>(), 2);

            //Alors
            Assert.Equal("SCI001", Assert.Single(livres).Code);
        }

        [Fact]
        public async Task ChercherAsync_RefuseUneListeDAuteursAbsente()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _depot.ChercherAsync("dune", null!, null));
        }

        [Fact]
        public async Task ListerLesCodesCommencantParAsync_NeRendQueLaFamilleDemandee()
        {
            //Etant donne trois livres de deux categories
            Catalogue(1, "Les Misérables", "ROM001");
            Catalogue(2, "Notre-Dame de Paris", "ROM002");
            Catalogue(3, "Dune", "SCI001", categorie: 2, auteur: 5);

            //Lorsque la bibliotheque cherche le prochain code d'un roman
            List<string?> codes = await _depot.ListerLesCodesCommencantParAsync("ROM");

            //Alors
            Assert.Equal(new[] { "ROM001", "ROM002" }, codes.OrderBy(c => c));
        }

        [Fact]
        public async Task Ajouter_PuisEnregistrer_EcritLeLivreEtTousSesLiens()
        {
            //Etant donne un livre complet qui n'est pas encore en base
            var livre = new Livre
            {
                Titre = "Quatrevingt-treize",
                Code = "ROM001",
                CategorieID = 1,
                Isbn10 = "1111111111",
                Isbn13 = "9781111111111",
                Redactions = new List<Redaction> { new Redaction { AuteurID = 7, Role = "Auteur", Ordre = 1 } },
                Editions = new List<Edition>
                {
                    new Edition
                    {
                        NomEditeur = "Hetzel",
                        Exemplaires = new List<Exemplaire> { new Exemplaire { Reference = 2001, NbExemplaire = 4 } }
                    }
                }
            };

            //Lorsque
            _depot.Ajouter(livre);
            await _depot.EnregistrerAsync();
            _base.Oublier();

            //Alors tout le graphe est arrive en base, avec une cle attribuee
            Assert.NotEqual(0, livre.LivreID);
            Livre? relu = await _depot.ObtenirAvecSesLiensAsync(livre.LivreID);
            Assert.Equal("Quatrevingt-treize", relu?.Titre);
            Assert.Equal(4, relu?.Quantite);
        }

        [Fact]
        public async Task Enregistrer_RefuseDeuxFoisLeMemeCode()
        {
            //Etant donne un code deja attribue
            Catalogue(1, "Les Misérables", "ROM001");

            //Lorsqu'un deuxieme livre pretend le porter
            _depot.Ajouter(new Livre
            {
                Titre = "Notre-Dame de Paris",
                Code = "ROM001",
                CategorieID = 1,
                Isbn10 = "2222222222",
                Isbn13 = "9782222222222"
            });

            //Alors l'index unique de la base refuse
            await Assert.ThrowsAsync<DbUpdateException>(() => _depot.EnregistrerAsync());
        }

        [Fact]
        public async Task RemplacerLesValeurs_CorrigeLesChampsSansToucherALaCle()
        {
            //Etant donne un livre deja catalogue
            Catalogue(1, "Les Miserables", "ROM001");
            Livre livre = (await _depot.ObtenirPourModificationAsync(1))!;

            //Lorsqu'on lui passe des valeurs corrigees
            _depot.RemplacerLesValeurs(livre, new Livre
            {
                LivreID = 1,
                Code = "ROM001",
                Titre = "Les Misérables",
                CategorieID = 1,
                Prix = 14.99m,
                Isbn10 = "1234567890",
                Isbn13 = "9781234567890"
            });
            await _depot.EnregistrerAsync();
            _base.Oublier();

            //Alors le titre suit, la cle et le code restent
            Livre? relu = await _depot.ObtenirAsync(1);
            Assert.Equal("Les Misérables", relu?.Titre);
            Assert.Equal("ROM001", relu?.Code);
            Assert.Equal(14.99m, relu?.Prix);
        }

        [Fact]
        public async Task Enregistrer_GardeLePrixADeuxDecimales()
        {
            //Etant donne un prix ecrit avec plus de precision que la colonne
            Catalogue(1, "Les Misérables", "ROM001");
            Livre livre = (await _depot.ObtenirPourModificationAsync(1))!;
            livre.Prix = 18.5m;

            //Lorsque
            await _depot.EnregistrerAsync();
            _base.Oublier();

            //Alors la base rend le meme nombre, sous une seule forme
            Assert.Equal(18.50m, (await _depot.ObtenirAsync(1))?.Prix);
        }

        [Fact]
        public async Task Supprimer_PuisEnregistrer_RetireLeLivre()
        {
            //Etant donne un livre au catalogue
            Catalogue(1, "Dune", "SCI001");
            Livre livre = (await _depot.ObtenirAsync(1))!;

            //Lorsque
            _depot.Supprimer(livre);
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
