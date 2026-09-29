using System.ComponentModel.DataAnnotations;

namespace BibliothequeLIPAJOLI.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AuPlusDeuxDecimalesAttribute : ValidationAttribute
    {
        public override bool IsValid(object? valeur)
        {
            if (valeur is null)
            {
                return true;
            }

            if (valeur is not decimal nombre)
            {
                return false;
            }

            return nombre == decimal.Round(nombre, 2);
        }
    }
}
