using System.Diagnostics;
using BibliothequeLIPAJOLI.Controllers;
using BibliothequeLIPAJOLI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BibliothequeLIPAJOLI.Tests.Controleurs
{
    public class ErreurControllerTest : IDisposable
    {
        private readonly ErreurController _controleur = new ErreurController();
        private readonly Activity? _activiteDuDepart = Activity.Current;

        private void AvecLaDemande(string identifiant)
        {
            var demande = new DefaultHttpContext { TraceIdentifier = identifiant };
            _controleur.ControllerContext = new ControllerContext { HttpContext = demande };
        }

        private static ErreurViewModel ModeleDe(IActionResult resultat)
        {
            return Assert.IsType<ErreurViewModel>(Assert.IsType<ViewResult>(resultat).Model);
        }

        [Fact]
        public void Index_MontreLeNumeroDeLaDemandeQuandAucuneTraceNEstOuverte()
        {
            //Etant donne une demande que le serveur a numerotee
            Activity.Current = null;
            AvecLaDemande("0HN7GJ1KQPL5A");

            //Lorsque la page d'erreur s'affiche
            IActionResult resultat = _controleur.Index();

            //Alors le visiteur a de quoi nommer sa demande
            ErreurViewModel modele = ModeleDe(resultat);
            Assert.Equal("0HN7GJ1KQPL5A", modele.IdentifiantDeLaRequete);
            Assert.True(modele.MontrerLIdentifiant);
        }

        [Fact]
        public void Index_PrefereLIdentifiantDeLaTraceQuandIlYEnAUne()
        {
            //Etant donne une trace ouverte, celle que les journaux suivent
            AvecLaDemande("0HN7GJ1KQPL5A");
            using var trace = new Activity("demande").Start();

            //Lorsque
            ErreurViewModel modele = ModeleDe(_controleur.Index());

            //Alors
            Assert.Equal(trace.Id, modele.IdentifiantDeLaRequete);
        }

        [Fact]
        public void MontrerLIdentifiant_RienASignalerSansNumero()
        {
            //Etant donne une page d'erreur sans numero a montrer
            var modele = new ErreurViewModel();

            //Alors elle ne montre pas une ligne vide
            Assert.False(modele.MontrerLIdentifiant);
            Assert.False(new ErreurViewModel { IdentifiantDeLaRequete = "" }.MontrerLIdentifiant);
        }

        public void Dispose()
        {
            Activity.Current = _activiteDuDepart;
            GC.SuppressFinalize(this);
        }
    }
}
