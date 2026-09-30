using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.Controllers
{
    public class UsagerController : Controller
    {
        private readonly IUsagerService _usagerService;

        public UsagerController(IUsagerService usagerService)
        {
            _usagerService = usagerService;
        }

        public async Task<IActionResult> Index(string chaineRecherche)
        {
            if (!string.IsNullOrEmpty(chaineRecherche))
            {
                var result = await _usagerService.RechercherUsagersAsync(chaineRecherche);
                return View(result);
            }

            return View(await _usagerService.ObtenirTousLesUsagersAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var usager = await _usagerService.ObtenirUsagerParIdAsync(id);
            if (usager == null) return NotFound();

            return View(usager);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsagerCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _usagerService.AjouterUsagerCompletAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usager = await _usagerService.ObtenirUsagerParIdAsync(id);
            if (usager == null) return NotFound();

            var adressePrincipale = usager.UsagerAdresses?
                .FirstOrDefault(ua => ua.TypeAdresse == TypeAdresse.Principale)?.Adresse;

            var viewModel = new UsagerCreateViewModel
            {
                Usager = usager,
                Adresse = adressePrincipale ?? new Adresse(),
                TypeAdresse = TypeAdresse.Principale
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsagerCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Adresse ??= new Adresse();
                return View(model);
            }

            await _usagerService.MettreAJourUsagerCompletAsync(id, model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id, bool? suppressionImpossible = false)
        {
            var usager = await _usagerService.ObtenirUsagerParIdAsync(id);
            if (usager == null) return NotFound();

            if (suppressionImpossible.GetValueOrDefault())
            {
                ViewData["MessageErreur"] =
                    "La suppression a échoué. Réessayez, et si le problème persiste, "
                    + "prévenez la personne qui administre le site.";
            }

            return View(usager);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _usagerService.SupprimerUsagerAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { id, suppressionImpossible = true });
            }
        }
    }
}
