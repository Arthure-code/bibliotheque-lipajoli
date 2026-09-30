using System.Globalization;
using System.Text;

namespace BibliothequeLIPAJOLI.Regles
{
    public static class Texte
    {
        public static string SansAccent(string? texte)
        {
            if (string.IsNullOrEmpty(texte))
            {
                return string.Empty;
            }

            string decompose = texte.Normalize(NormalizationForm.FormD);
            var sortie = new StringBuilder(decompose.Length);

            foreach (char lettre in decompose)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(lettre) != UnicodeCategory.NonSpacingMark)
                {
                    sortie.Append(lettre);
                }
            }

            return sortie.ToString().Normalize(NormalizationForm.FormC);
        }

        public static bool Contient(string? source, string? recherche)
        {
            if (string.IsNullOrWhiteSpace(recherche))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            return SansAccent(source).Contains(SansAccent(recherche.Trim()),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
