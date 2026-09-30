using BibliothequeLIPAJOLI.Regles;

namespace BibliothequeLIPAJOLI.Tests.Regles
{
    public class CodificationTests
    {
        private static readonly string[] AucunCode = Array.Empty<string>();
        private static readonly string[] TroisRomans = { "ROM001", "ROM002", "ROM003" };
        private static readonly string[] DeuxCategories = { "ROM001", "RES001", "ROM002" };

        [Theory]
        [InlineData("Programmation", "PRO")]
        [InlineData("Réseau", "RES")]
        [InlineData("Poésie", "POE")]
        [InlineData("Émile", "EMI")]
        [InlineData("roman", "ROM")]
        public void Prefixe_PrendLesTroisPremieresLettresSansAccent(string categorie, string attendu)
        {
            //Lorsque
            string prefixe = Codification.Prefixe(categorie);

            //Alors
            Assert.Equal(attendu, prefixe);
        }

        [Theory]
        [InlineData("BD", "BD")]
        [InlineData("Art", "ART")]
        public void Prefixe_AccepteUneCategoriePlusCourteQueTroisLettres(string categorie, string attendu)
        {
            Assert.Equal(attendu, Codification.Prefixe(categorie));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("123")]
        public void Prefixe_RetombeSurGenericQuandLaCategorieNeDitRien(string? categorie)
        {
            //Alors un livre garde un code, meme sans categorie utilisable
            Assert.Equal("GEN", Codification.Prefixe(categorie));
        }

        [Fact]
        public void Suivant_CommenceAUnQuandLaCategorieEstVide()
        {
            //Etant donne une categorie sans aucun livre
            //Lorsque
            string code = Codification.Suivant("PRO", AucunCode);

            //Alors
            Assert.Equal("PRO001", code);
        }

        [Fact]
        public void Suivant_ReprendApresLePlusGrandNumeroDejaDonne()
        {
            //Etant donne trois romans deja codifies
            //Lorsque
            string code = Codification.Suivant("ROM", TroisRomans);

            //Alors
            Assert.Equal("ROM004", code);
        }

        [Fact]
        public void Suivant_IgnoreLesCodesDesAutresCategories()
        {
            //Etant donne des codes de deux categories melanges
            //Lorsque on demande le prochain code de reseau
            string code = Codification.Suivant("RES", DeuxCategories);

            //Alors la suite des romans ne compte pas
            Assert.Equal("RES002", code);
        }

        [Fact]
        public void Suivant_PasseAQuatreChiffresPlutotQueDeRecommencer()
        {
            //Etant donne une categorie arrivee au bout de ses trois chiffres
            var codes = new[] { "PRO999" };

            //Lorsque
            string code = Codification.Suivant("PRO", codes);

            //Alors le numero s'allonge au lieu d'entrer en collision
            Assert.Equal("PRO1000", code);
        }

        [Fact]
        public void Suivant_NeSeLaissePasTromperParUnCodeMalForme()
        {
            //Etant donne des codes que la bibliotheque n'a pas produits
            var codes = new[] { "PRO", "PROABC", "PRO0X2", null, "   ", "PRO007" };

            //Lorsque
            string code = Codification.Suivant("PRO", codes);

            //Alors seul le code lisible compte
            Assert.Equal("PRO008", code);
        }

        [Fact]
        public void Suivant_ExigeUnPrefixe()
        {
            Assert.Throws<ArgumentException>(() => Codification.Suivant(" ", AucunCode));
        }
    }
}
