using System.Net;

namespace BibliothequeLIPAJOLI.TestsFonctionnels
{
    public class CatalogueTest : IDisposable
    {
        private readonly ApplicationDeTest _application = new ApplicationDeTest();

        private static async Task<string> Lire(HttpResponseMessage reponse)
        {
            return await reponse.Content.ReadAsStringAsync();
        }

        [Theory]
        [InlineData("/Livres")]
        [InlineData("/Livres/Create")]
        [InlineData("/Livres/Details/1")]
        [InlineData("/Livres/Edit/1")]
        public async Task LesPagesDuCatalogueRepondent(string adresse)
        {
            //Etant donne l'application complete, avec ses donnees de depart
            HttpClient navigateur = _application.CreateClient();

            //Lorsque le visiteur ouvre la page
            HttpResponseMessage reponse = await navigateur.GetAsync(adresse);

            //Alors
            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        }

        [Fact]
        public async Task Index_MontreLesQuatreLivresAvecLeurCode()
        {
            //Etant donne le catalogue de depart
            HttpClient navigateur = _application.CreateClient();

            //Lorsque
            string page = await Lire(await navigateur.GetAsync("/Livres"));

            //Alors chaque code attribue par la bibliotheque est la
            Assert.Contains("ROM001", page, StringComparison.Ordinal);
            Assert.Contains("SCI001", page, StringComparison.Ordinal);
            Assert.Contains("BIO001", page, StringComparison.Ordinal);
            Assert.Contains("POE001", page, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Index_LaRechercheParAuteurPasseParLaConfiguration()
        {
            //Etant donne un auteur qui n'existe que dans la configuration
            HttpClient navigateur = _application.CreateClient();

            //Lorsque le visiteur cherche son nom
            string page = await Lire(await navigateur.GetAsync("/Livres?searchString=hugo"));

            //Alors seul le livre qu'il a signe reste
            Assert.Contains("ROM001", page, StringComparison.Ordinal);
            Assert.DoesNotContain("SCI001", page, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Details_RepondIntrouvableQuandLeLivreNExistePas()
        {
            //Etant donne un identifiant qui ne designe rien
            HttpClient navigateur = _application.CreateClient();

            //Alors
            Assert.Equal(HttpStatusCode.NotFound, (await navigateur.GetAsync("/Livres/Details/404")).StatusCode);
        }

        [Fact]
        public async Task Create_LeFormulaireNeDemandePasLeCode()
        {
            //Etant donne la page de catalogage
            HttpClient navigateur = _application.CreateClient();

            //Lorsque
            string page = await Lire(await navigateur.GetAsync("/Livres/Create"));

            //Alors aucun champ ne reclame le code, que la bibliotheque attribue
            Assert.DoesNotContain("name=\"Livre.Code\"", page, StringComparison.Ordinal);
            Assert.Contains("name=\"Livre.Titre\"", page, StringComparison.Ordinal);
            Assert.Contains("name=\"Redactions\"", page, StringComparison.Ordinal);
        }

        public void Dispose()
        {
            _application.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
