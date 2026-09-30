using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.Services
{
    public static class DbInitializer
    {
        public static async Task Initialize(BibliothequeLipajoliContext context, int joursDePret)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (await context.Livres.AnyAsync())
            {
                return;
            }

            List<Usager> usagers = DonneesDeDepart.Usagers();
            List<Emprunt> emprunts = DonneesDeDepart.Emprunts(joursDePret);
            DonneesDeDepart.PorterLesDefaillances(usagers, emprunts);

            await context.Adresses.AddRangeAsync(DonneesDeDepart.Adresses());
            await context.Livres.AddRangeAsync(DonneesDeDepart.Livres());
            await context.Redactions.AddRangeAsync(DonneesDeDepart.Redactions());
            await context.Usagers.AddRangeAsync(usagers);
            await context.UsagerAdresses.AddRangeAsync(DonneesDeDepart.UsagerAdresses());
            await context.Editions.AddRangeAsync(DonneesDeDepart.Editions());
            await context.Exemplaires.AddRangeAsync(DonneesDeDepart.Exemplaires());
            await context.Emprunts.AddRangeAsync(emprunts);

            await context.SaveChangesAsync();
        }
    }
}
