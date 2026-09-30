using System.Diagnostics;
using BibliothequeLIPAJOLI.Controllers;
using BibliothequeLIPAJOLI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BibliothequeLIPAJOLI.Tests.Controleurs
{
    public class ErreurControllerTest
    {
        [Fact]
        public void Index_MontreLeNumeroDeLaDemandeQuandAucuneTraceNEstOuverte()
        {
            //Etant donne une demande que le serveur a numerotee
            Activity.Current = null;
            var controleur = new ErreurController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { TraceIdentifier = "0HN7GJ1KQPL5A" }
                }
            };

            //Lorsque la page d'erreur s'affiche
            IActionResult resultat = controleur.Index();

            //Alors le visiteur a de quoi nommer sa demande
            var modele = Assert.IsType<ErreurViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal("0HN7GJ1KQPL5A", modele.IdentifiantDeLaRequete);
            Assert.True(modele.MontrerLIdentifiant);
        }

        [Fact]
        public void Index_PrefereLIdentifiantDeLaTraceQuandIlYEnAUne()
        {
            //Etant donne une trace ouverte, celle que les journaux suivent
            var controleur = new ErreurController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { TraceIdentifier = "0HN7GJ1KQPL5A" }
                }
            };
            using var trace = new Activity("demande").Start();

            //Lorsque
            IActionResult resultat = controleur.Index();

            //Alors le numero de la trace l'emporte sur celui de la demande
            var modele = Assert.IsType<ErreurViewModel>(Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal(trace.Id, modele.IdentifiantDeLaRequete);
        }
    }
}
