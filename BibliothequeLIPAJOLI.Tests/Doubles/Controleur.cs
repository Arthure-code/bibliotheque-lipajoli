using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BibliothequeLIPAJOLI.Tests.Doubles
{
    internal static class Controleur
    {
        public static TControleur Preparer<TControleur>(TControleur controleur) where TControleur : Controller
        {
            controleur.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Fournisseur vide du cadriciel : aucune dépendance à monter.
            controleur.MetadataProvider = new EmptyModelMetadataProvider();
            controleur.Url = Mock.Of<IUrlHelper>();
            controleur.TempData = Mock.Of<ITempDataDictionary>();

            return controleur;
        }
    }
}
