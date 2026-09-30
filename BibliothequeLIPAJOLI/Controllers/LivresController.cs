
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.Services;
using BibliothequeLIPAJOLI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BibliothequeLIPAJOLI.Interfaces;

namespace BibliothequeLIPAJOLI.Controllers
{
    public class LivresController : Controller
    {
        private readonly ILivreService _livres;
        private readonly IReferentielService _referentiel;

        public LivresController(ILivreService livres, IReferentielService referentiel)
        {
            _livres = livres;
            _referentiel = referentiel;
        }

        public async Task<IActionResult> Index(string? searchString, int? categorieId, string? sortOrder)
        {
            
            var livres = await _livres.RechercherLivresAsync(searchString, categorieId);

            livres = _livres.TrierLivres(livres, sortOrder ?? "");

            var categories = _referentiel.ObtenirCategories();

            var viewModel = new LivreFiltreViewModel
            {
                SearchString = searchString,
                CategorieId = categorieId,
                Livres = livres,
                Categories = new SelectList(categories, "CategorieID", "NomCategorie")
            };

            ViewData["CurrentSort"] = sortOrder; 
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var livre = await _livres.ObtenirLivreParIdAsync(id);
            if (livre == null) return NotFound();

            var exemplaires = livre.Editions?.SelectMany(e => e.Exemplaires ?? new List<Exemplaire>()).ToList() ?? new List<Exemplaire>();
            var emprunts = exemplaires.SelectMany(ex => ex.Emprunts ?? new List<Emprunt>()).ToList();

            var viewModel = new LivreDetailsViewModel
            {
                Livre = livre,
                Exemplaires = exemplaires,
                Emprunts = emprunts
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new LivreCreateViewModel
            {
                Categories = (_referentiel.ObtenirCategories())
                    .Select(c => new SelectListItem { Value = c.CategorieID.ToString(), Text = c.NomCategorie }),
                Auteurs = (_referentiel.ObtenirAuteurs())
                    .Select(a => new SelectListItem { Value = a.ID.ToString(), Text = a.NomComplet })
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LivreCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Categories = (_referentiel.ObtenirCategories())
                    .Select(c => new SelectListItem { Value = c.CategorieID.ToString(), Text = c.NomCategorie });
                viewModel.Auteurs = (_referentiel.ObtenirAuteurs())
                    .Select(a => new SelectListItem { Value = a.ID.ToString(), Text = a.NomComplet });
                return View(viewModel);
            }

            await _livres.CreerLivreCompletAsync(viewModel);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var livre = await _livres.ObtenirLivreParIdAsync(id.Value);
            if (livre == null) return NotFound();

            var categories = _referentiel.ObtenirCategories();
            var auteurs = _referentiel.ObtenirAuteurs();

            var viewModel = new LivreEditViewModel
            {
                Livre = livre,
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.CategorieID.ToString(),
                    Text = c.NomCategorie,
                    Selected = c.CategorieID == livre.CategorieID
                }),
                Auteurs = auteurs.Select(a => new SelectListItem
                {
                    Value = a.ID.ToString(),
                    Text = $"{a.Prenom} {a.Nom}",
                    Selected = livre.Redactions?.Any(r => r.AuteurID == a.ID) ?? false
                }),
                Redactions = livre.Redactions?.Select(r => r.AuteurID).ToList() ?? new List<int>()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LivreEditViewModel vm)
        {
            if (id != vm.Livre.LivreID) return NotFound();

            if (ModelState.IsValid)
            {
                var livre = await _livres.ModifierLivreCompletAsync(vm);
                if (livre == null) return NotFound();

                return RedirectToAction(nameof(Index));
            }

            vm.Categories = new SelectList(_referentiel.ObtenirCategories(), "CategorieID", "NomCategorie", vm.Livre.CategorieID);
            vm.Auteurs = new SelectList(_referentiel.ObtenirAuteurs(), "ID", "NomComplet");
            return View(vm);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var livre = await _livres.ObtenirLivreParIdAsync(id);
            if (livre == null) return NotFound();
            return View(livre);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
           
            var livre = await _livres.ObtenirLivreParIdAsync(id);

            if (livre == null)
                return NotFound();

            bool estEmprunte = livre.Editions?
                .SelectMany(e => e.Exemplaires ?? new List<Exemplaire>())
                .Any(ex => ex.Emprunts != null && ex.Emprunts.Any()) ?? false;

            if (estEmprunte)
            {
               
                ModelState.AddModelError(string.Empty,
                    "Impossible de supprimer ce livre car il est actuellement emprunté.");
                return View("Delete", livre);
            }

            var deleted = await _livres.SupprimerLivreAsync(id);
            if (!deleted)
            {
                ModelState.AddModelError(string.Empty,
                    "Impossible de supprimer ce livre car il est utilisé par d'autres données.");
                return View("Delete", livre);
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
