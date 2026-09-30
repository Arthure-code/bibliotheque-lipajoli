using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BibliothequeLIPAJOLI.TestsFonctionnels
{
    public class FormulaireDeLUsagerTest : IDisposable
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

        private static Dictionary<string, string> UnDossierComplet(string jeton) =>
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = jeton,
                ["Usager.NumeroAbonne"] = "LJ001",
                ["Usager.Nom"] = "Tremblay-Roy",
                ["Usager.Prenom"] = "Marie",
                ["Usager.Courriel"] = "marie.roy@email.com",
                ["Usager.Statut"] = "Etudiant",
                ["Adresse.Rue"] = "12 rue des Lilas",
                ["Adresse.Ville"] = "Québec",
                ["Adresse.Province"] = "QC",
                ["Adresse.CodePostale"] = "G1R 2B3",
                ["TypeAdresse"] = "Principale"
            };

        [Theory]
        [InlineData("/Usager")]
        [InlineData("/Usager/Create")]
        [InlineData("/Usager/Details/2")]
        [InlineData("/Usager/Edit/1")]
        [InlineData("/Usager/Delete/1")]
        public async Task LesPagesDesUsagersRepondent(string adresse)
        {
            //Etant donne l'application complete
            HttpClient navigateur = Navigateur();

            //Alors
            Assert.Equal(HttpStatusCode.OK, (await navigateur.GetAsync(adresse)).StatusCode);
        }

        [Fact]
        public async Task Details_MontreLaDefaillanceEtLesEmpruntsEnCours()
        {
            //Etant donne l'usager qui a rendu un livre en retard
            HttpClient navigateur = Navigateur();

            //Lorsque
            string fiche = await navigateur.GetStringAsync("/Usager/Details/2");

            //Alors son dossier le dit
            Assert.Contains("Gagnon", fiche, StringComparison.Ordinal);
            Assert.Contains("LJ002", fiche, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Edit_CorrigeLeDossierEtSonAdresse()
        {
            //Etant donne un dossier a corriger
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Usager/Edit/1");

            //Lorsque le formulaire part
            HttpResponseMessage reponse = await navigateur.PostAsync("/Usager/Edit/1",
                new FormUrlEncodedContent(UnDossierComplet(jeton)));

            //Alors la correction est en base, nom comme adresse
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            string fiche = await navigateur.GetStringAsync("/Usager/Details/1");
            Assert.Contains("Tremblay-Roy", fiche, StringComparison.Ordinal);
            Assert.Contains("12 rue des Lilas", fiche, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Edit_UnCourrielManquantRevientAvecSonMessage()
        {
            //Etant donne un formulaire sans courriel
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Usager/Edit/1");
            Dictionary<string, string> champs = UnDossierComplet(jeton);
            champs["Usager.Courriel"] = string.Empty;

            //Lorsque
            HttpResponseMessage reponse = await navigateur.PostAsync("/Usager/Edit/1",
                new FormUrlEncodedContent(champs));

            //Alors la page revient, et le dossier n'a pas bouge
            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            Assert.Contains("Tremblay", await navigateur.GetStringAsync("/Usager/Details/1"),
                StringComparison.Ordinal);
        }

        [Fact]
        public async Task Create_InscritUnUsagerAvecSonAdresse()
        {
            //Etant donne une inscription complete
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Usager/Create");
            Dictionary<string, string> champs = UnDossierComplet(jeton);
            champs["Usager.NumeroAbonne"] = "LJ004";
            champs["Usager.Nom"] = "Beaulieu";
            champs["Usager.Prenom"] = "Luc";
            champs["Usager.Courriel"] = "luc.beaulieu@email.com";

            //Lorsque
            HttpResponseMessage reponse = await navigateur.PostAsync("/Usager/Create",
                new FormUrlEncodedContent(champs));

            //Alors il apparait dans la liste
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            Assert.Contains("Beaulieu", await navigateur.GetStringAsync("/Usager"),
                StringComparison.Ordinal);
        }

        public void Dispose()
        {
            _application.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
