using System.Globalization;

namespace BibliothequeLIPAJOLI.Regles
{
    public static class Codification
    {
        private const int LongueurDuPrefixe = 3;
        private const int LongueurDeLaSequence = 3;
        private const string PrefixeParDefaut = "GEN";

        public static string Prefixe(string? nomCategorie)
        {
            if (string.IsNullOrWhiteSpace(nomCategorie))
            {
                return PrefixeParDefaut;
            }

            string lettres = new string(Texte.SansAccent(nomCategorie)
                .Where(char.IsLetter)
                .ToArray());

            if (lettres.Length == 0)
            {
                return PrefixeParDefaut;
            }

            return lettres.Length >= LongueurDuPrefixe
                ? lettres[..LongueurDuPrefixe].ToUpperInvariant()
                : lettres.ToUpperInvariant();
        }

        public static string Suivant(string prefixe, IEnumerable<string?> codesDejaAttribues)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(prefixe);
            ArgumentNullException.ThrowIfNull(codesDejaAttribues);

            int dernier = 0;

            foreach (string? code in codesDejaAttribues)
            {
                if (string.IsNullOrWhiteSpace(code) || !code.StartsWith(prefixe, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string suite = code[prefixe.Length..];
                if (suite.Length > 0 && suite.All(char.IsAsciiDigit)
                    && int.TryParse(suite, NumberStyles.None, CultureInfo.InvariantCulture, out int numero)
                    && numero > dernier)
                {
                    dernier = numero;
                }
            }

            // Au-delà de 999, le numéro s'allonge plutôt que de recommencer
            // et d'entrer en collision avec un code déjà donné.
            return prefixe + (dernier + 1).ToString(CultureInfo.InvariantCulture)
                .PadLeft(LongueurDeLaSequence, '0');
        }
    }
}
