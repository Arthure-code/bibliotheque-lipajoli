using BibliothequeLIPAJOLI.Controllers;
using BibliothequeLIPAJOLI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BibliothequeLIPAJOLI.TestsFonctionnels
{
    // L'application entiere, son routage, sa liaison de modele, sa
    // validation et ses vues, montee pour un seul test. Seule la base est
    // remplacee : une SQLite en memoire qui meurt avec le test.
    // Le type passe a la fabrique ne sert qu'a designer l'assemblage de
    // l'application, et Program y est statique.
    public sealed class ApplicationDeTest : WebApplicationFactory<LivresController>
    {
        private readonly SqliteConnection _connexion = new SqliteConnection("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ConfigureServices(services =>
            {
                ServiceDescriptor? ancien = services.SingleOrDefault(
                    s => s.ServiceType == typeof(DbContextOptions<BibliothequeLipajoliContext>));

                if (ancien != null)
                {
                    services.Remove(ancien);
                }

                // La connexion reste ouverte : c'est elle qui tient la base.
                // Le demarrage de l'application joue ses migrations et seme
                // ses donnees dedans, comme il le ferait en vrai.
                _connexion.Open();
                services.AddDbContext<BibliothequeLipajoliContext>(options => options.UseSqlite(_connexion));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _connexion.Dispose();
            }
        }
    }
}
