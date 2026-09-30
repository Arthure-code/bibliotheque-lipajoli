using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Liaison;
using BibliothequeLIPAJOLI.Services;
using BibliothequeLIPAJOLI.Interfaces;

namespace BibliothequeLIPAJOLI
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<BibliothequeLipajoliContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("BibliothequeLipajoliContext") ?? throw new InvalidOperationException("Connection string 'BibliothequeLipajoliContext' not found.")));

            builder.Services.AddScoped<ILivreDepot, LivreDepot>();
            builder.Services.AddScoped<IUsagerDepot, UsagerDepot>();

            builder.Services.AddScoped<ILivreService, GestionLivres>();
            builder.Services.AddScoped<IUsagerService, GestionUsager>();

            // Lues une fois au démarrage, elles ne changent plus.
            builder.Services.AddSingleton<IReferentielService, BibliothequeService>();

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddControllersWithViews(options =>
            {
                // 18,50 et 18.50 donnent la même valeur ; une écriture
                // ambiguë est refusée au lieu d'être devinée.
                options.ModelBinderProviders.Insert(0, new FournisseurDeLiantDecimal());

                // Sans cela, la liaison de modèle écrit ses propres
                // messages en anglais au milieu d'une page française.
                var messages = options.ModelBindingMessageProvider;
                messages.SetValueIsInvalidAccessor(
                    valeur => $"La valeur {valeur} n'est pas valide.");
                messages.SetAttemptedValueIsInvalidAccessor(
                    (valeur, champ) => $"La valeur {valeur} n'est pas valide pour le champ {champ}.");
                messages.SetNonPropertyAttemptedValueIsInvalidAccessor(
                    valeur => $"La valeur {valeur} n'est pas valide.");
                messages.SetValueMustBeANumberAccessor(
                    champ => $"Le champ {champ} doit contenir un nombre.");
                messages.SetNonPropertyValueMustBeANumberAccessor(
                    () => "La valeur doit être un nombre.");
                messages.SetValueMustNotBeNullAccessor(
                    champ => $"Le champ {champ} est requis.");
                messages.SetMissingBindRequiredValueAccessor(
                    champ => $"Le champ {champ} est absent de l'envoi.");
                messages.SetMissingKeyOrValueAccessor(
                    () => "Une clé ou une valeur est absente.");
                messages.SetMissingRequestBodyRequiredValueAccessor(
                    () => "Le corps de la requête est absent.");
                messages.SetUnknownValueIsInvalidAccessor(
                    champ => $"La valeur fournie pour le champ {champ} n'est pas valide.");
                messages.SetNonPropertyUnknownValueIsInvalidAccessor(
                    () => "La valeur fournie n'est pas valide.");
            });

            var app = builder.Build();

            // Une seule culture, à l'affichage comme à la lecture.
            var cultureQuebecoise = new CultureInfo("fr-CA");
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(cultureQuebecoise),
                SupportedCultures = new List<CultureInfo> { cultureQuebecoise },
                SupportedUICultures = new List<CultureInfo> { cultureQuebecoise }
            });

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<BibliothequeLipajoliContext>();
                var reglages = services.GetRequiredService<IReferentielService>();
                // EnsureCreated ne jouerait pas les migrations et
                // laisserait la table d'historique vide.
                await context.Database.MigrateAsync();
                await DbInitializer.Initialize(context, reglages.JoursDePret);
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Usager}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
  
}
