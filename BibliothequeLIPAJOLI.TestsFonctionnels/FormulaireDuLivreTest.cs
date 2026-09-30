using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BibliothequeLIPAJOLI.TestsFonctionnels
{
    public class FormulaireDuLivreTest : IDisposable
    {
        private readonly ApplicationDeTest _application = new ApplicationDeTest();

        // Sans cela, le client suit la redirection et on ne voit plus que
        // la page d'arrivee, jamais la reponse du formulaire.
        private HttpClient Navigateur() => _application.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        private static async Task<string> Jeton(HttpClient navigateur, string adresse)
        {
            string page = await navigateur.GetStringAsync(adresse);
            Match jeton = Regex.Match(page,
                "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
                RegexOptions.None, TimeSpan.FromSeconds(5));

            return jeton.Groups[1].Value;
        }

        private static Dictionary<string, string> UnLivreComplet(string jeton, string prix) =>
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = jeton,
                ["Livre.Titre"] = "Notre-Dame de Paris",
                ["Livre.Isbn10"] = "2222222222",
                ["Livre.Isbn13"] = "9782222222222",
                ["Livre.Prix"] = prix,
                ["Livre.CategorieID"] = "1",
                ["Livre.NombrePages"] = "940",
                ["Redactions"] = "2",
                ["Edition.NomEditeur"] = "Gallimard",
                ["Exemplaire.NbExemplaire"] = "3",
                ["Exemplaire.Reference"] = "2001"
            };

        [Theory]
        [InlineData("18,50")]
        [InlineData("18.50")]
        [InlineData("1 234,56")]
        public async Task Create_LePrixSEcritDesDeuxFaconsEtArriveSousUneSeule(string prix)
        {
            //Etant donne un formulaire rempli, le prix ecrit a la francaise
            //ou a l'anglaise
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Livres/Create");

            //Lorsque le visiteur l'envoie
            HttpResponseMessage reponse = await navigateur.PostAsync("/Livres/Create",
                new FormUrlEncodedContent(UnLivreComplet(jeton, prix)));

            //Alors le livre entre au catalogue
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            Assert.Equal("/Livres", reponse.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task Create_LaBibliothequeAttribueLeCodeSuivantDeLaCategorie()
        {
            //Etant donne quatre livres de depart, dont un seul roman
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Livres/Create");

            //Lorsqu'un deuxieme roman entre
            await navigateur.PostAsync("/Livres/Create",
                new FormUrlEncodedContent(UnLivreComplet(jeton, "18,50")));

            //Alors il prend la suite, sans que personne ne l'ait saisi
            string catalogue = await navigateur.GetStringAsync("/Livres");
            Assert.Contains("ROM002", catalogue, StringComparison.Ordinal);
            Assert.Contains("Notre-Dame de Paris", catalogue, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Create_UnTitreManquantRevientAvecSonMessage()
        {
            //Etant donne un formulaire sans titre
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Livres/Create");
            Dictionary<string, string> champs = UnLivreComplet(jeton, "18,50");
            champs["Livre.Titre"] = string.Empty;

            //Lorsque
            HttpResponseMessage reponse = await navigateur.PostAsync("/Livres/Create",
                new FormUrlEncodedContent(champs));

            //Alors la page revient en disant ce qui manque, et rien n'est ecrit
            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            string page = await reponse.Content.ReadAsStringAsync();
            Assert.Contains("Le titre est requis.", page, StringComparison.Ordinal);
            Assert.DoesNotContain("Notre-Dame de Paris</td>", page, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Create_UnPrixAmbiguEstRefusePlutotQueDevine()
        {
            //Etant donne un prix qui vaut dix mille pour un anglophone et dix
            //pour un francophone
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Livres/Create");

            //Lorsque
            HttpResponseMessage reponse = await navigateur.PostAsync("/Livres/Create",
                new FormUrlEncodedContent(UnLivreComplet(jeton, "10,000")));

            //Alors la bibliotheque redemande au lieu de choisir
            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            // Razor ecrit les accents en entites, donc on cherche la partie
            // du message qui n'en porte pas.
            Assert.Contains("par exemple 18,50 ou 18.50.", await reponse.Content.ReadAsStringAsync(),
                StringComparison.Ordinal);
        }

        [Fact]
        public async Task Edit_LeCodeDuLivreNeChangePasQuoiQuEnviePleFormulaire()
        {
            //Etant donne un formulaire qui pretend donner un autre code
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Livres/Edit/1");
            Dictionary<string, string> champs = UnLivreComplet(jeton, "14,99");
            champs["Livre.Titre"] = "Les Misérables, édition revue";
            champs["Livre.Isbn10"] = "1234567890";
            champs["Livre.Isbn13"] = "9781234567890";
            champs["Livre.Code"] = "PIRATE";
            champs["Livre.LivreID"] = "99";

            //Lorsque la requete arrive sur le livre 1
            HttpResponseMessage reponse = await navigateur.PostAsync("/Livres/Edit/1",
                new FormUrlEncodedContent(champs));

            //Alors le titre suit, mais le code reste celui de la bibliotheque
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            string fiche = await navigateur.GetStringAsync("/Livres/Details/1");
            Assert.Contains("ROM001", fiche, StringComparison.Ordinal);
            Assert.DoesNotContain("PIRATE", fiche, StringComparison.Ordinal);
        }

        public void Dispose()
        {
            _application.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
