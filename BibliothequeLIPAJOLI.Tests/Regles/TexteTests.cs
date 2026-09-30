using BibliothequeLIPAJOLI.Regles;

namespace BibliothequeLIPAJOLI.Tests.Regles
{
    public class TexteTests
    {
        [Theory]
        [InlineData("Émile", "Emile")]
        [InlineData("Poésie", "Poesie")]
        [InlineData("Les Misérables", "Les Miserables")]
        [InlineData("Hugo", "Hugo")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void SansAccent_RetireLesAccentsEtGardeLeReste(string? texte, string attendu)
        {
            Assert.Equal(attendu, Texte.SansAccent(texte));
        }

        [Theory]
        [InlineData("Victor Hugo", "hugo")]
        [InlineData("Victor Hugo", "HUGO")]
        [InlineData("Victor Hugo", "victor hugo")]
        [InlineData("Émile Zola", "emile")]
        [InlineData("Emile Zola", "émile")]
        [InlineData("Victor Hugo", "  hugo  ")]
        public void Contient_IgnoreLaCasseLesAccentsEtLesEspacesAutour(string source, string recherche)
        {
            Assert.True(Texte.Contient(source, recherche));
        }

        [Theory]
        [InlineData("Victor Hugo", "zola")]
        [InlineData("", "hugo")]
        [InlineData(null, "hugo")]
        public void Contient_DitNonQuandLeTexteNEstPasLa(string? source, string recherche)
        {
            Assert.False(Texte.Contient(source, recherche));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Contient_UneRechercheVideNeRetrancheRien(string? recherche)
        {
            //Alors tout le monde correspond, la liste n'est pas filtree
            Assert.True(Texte.Contient("Victor Hugo", recherche));
        }
    }
}
