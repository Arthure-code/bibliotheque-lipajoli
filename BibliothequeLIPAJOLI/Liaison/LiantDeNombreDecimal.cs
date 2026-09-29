using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliothequeLIPAJOLI.Liaison
{
    public class LiantDeNombreDecimal : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext contexte)
        {
            ArgumentNullException.ThrowIfNull(contexte);

            ValueProviderResult valeurEnvoyee = contexte.ValueProvider.GetValue(contexte.ModelName);
            if (valeurEnvoyee == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            contexte.ModelState.SetModelValue(contexte.ModelName, valeurEnvoyee);
            string? texte = valeurEnvoyee.FirstValue;

            if (string.IsNullOrWhiteSpace(texte))
            {
                // Un champ vide reste vide : c'est au modèle de dire s'il
                // était obligatoire.
                contexte.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            if (!NombreDecimal.EssayerDeLire(texte, out decimal nombre))
            {
                contexte.ModelState.TryAddModelError(contexte.ModelName,
                    $"Écrivez ce nombre avec la virgule ou le point, par exemple 18,50 ou 18.50.");
                return Task.CompletedTask;
            }

            contexte.Result = ModelBindingResult.Success(nombre);
            return Task.CompletedTask;
        }
    }

    public class FournisseurDeLiantDecimal : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext contexte)
        {
            ArgumentNullException.ThrowIfNull(contexte);

            Type type = contexte.Metadata.UnderlyingOrModelType;
            return type == typeof(decimal) ? new LiantDeNombreDecimal() : null;
        }
    }
}
