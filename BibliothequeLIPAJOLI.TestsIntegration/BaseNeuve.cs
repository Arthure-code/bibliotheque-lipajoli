using BibliothequeLIPAJOLI.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.TestsIntegration
{
    // Une base SQLite en memoire, montee pour un seul test et fermee avec
    // lui. xUnit construit une instance de la classe par test, donc rien ne
    // circule d'un test a l'autre.
    public sealed class BaseNeuve : IDisposable
    {
        private readonly SqliteConnection _connexion;

        public BibliothequeLipajoliContext Context { get; }

        public BaseNeuve()
        {
            _connexion = new SqliteConnection("DataSource=:memory:");
            _connexion.Open();

            DbContextOptions<BibliothequeLipajoliContext> options =
                new DbContextOptionsBuilder<BibliothequeLipajoliContext>()
                    .UseSqlite(_connexion)
                    .Options;

            Context = new BibliothequeLipajoliContext(options);
            Context.Database.EnsureCreated();
        }

        // Ce qu'une deuxieme requete verrait : sans cela, le suivi d'entites
        // rendrait l'objet garde en memoire au lieu de ce qui est ecrit.
        public void Oublier()
        {
            Context.ChangeTracker.Clear();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connexion.Dispose();
        }
    }
}
