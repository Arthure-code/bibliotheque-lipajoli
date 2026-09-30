using System.Diagnostics;
using BibliothequeLIPAJOLI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BibliothequeLIPAJOLI.Controllers
{
    public class ErreurController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            // L'identifiant de la requete permet de la retrouver dans les
            // journaux du serveur.
            return View(new ErreurViewModel
            {
                IdentifiantDeLaRequete = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
