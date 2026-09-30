using BibliothequeLIPAJOLI.Liaison;

namespace BibliothequeLIPAJOLI.Tests.Liaison
{
    public class NombreDecimalTests
    {
        [Theory]
        [InlineData("10.00", 10)]
        [InlineData("10,00", 10)]
        [InlineData("18,50", 18.50)]
        [InlineData("18.50", 18.50)]
        [InlineData("25", 25)]
        [InlineData("0", 0)]
        [InlineData("0,99", 0.99)]
        public void EssayerDeLire_LesDeuxEcrituresDonnentLeMemeNombre(string ecrit, double attendu)
        {
            //Lorsque
            bool lu = NombreDecimal.EssayerDeLire(ecrit, out decimal valeur);

            //Alors
            Assert.True(lu);
            Assert.Equal((decimal)attendu, valeur);
        }

        [Theory]
        [InlineData("1 234,56")]
        [InlineData("1234,56")]
        [InlineData("1,234.56")]
        [InlineData("1.234,56")]
        [InlineData("1234.56")]
        public void EssayerDeLire_MilleDeuxCentTrenteQuatreSEcritDeCinqFacons(string ecrit)
        {
            //Lorsque
            bool lu = NombreDecimal.EssayerDeLire(ecrit, out decimal valeur);

            //Alors la meme valeur sort a chaque fois
            Assert.True(lu);
            Assert.Equal(1234.56m, valeur);
        }

        [Theory]
        [InlineData("1.000.000", 1000000)]
        [InlineData("1 000 000", 1000000)]
        [InlineData("1,000,000", 1000000)]
        public void EssayerDeLire_UnSeparateurRepeteGroupeLesMilliers(string ecrit, double attendu)
        {
            Assert.True(NombreDecimal.EssayerDeLire(ecrit, out decimal valeur));
            Assert.Equal((decimal)attendu, valeur);
        }

        [Theory]
        [InlineData("10,000")]
        [InlineData("10.000")]
        [InlineData("1,500")]
        public void EssayerDeLire_RefuseCeQuiPeutVouloirDireDeuxChoses(string ecrit)
        {
            //Etant donne une ecriture que rien ne permet de trancher, dix
            //mille pour un anglophone et dix pour un francophone
            //Alors on refuse plutot que de deviner
            Assert.False(NombreDecimal.EssayerDeLire(ecrit, out _));
        }

        [Theory]
        [InlineData("18.")]
        [InlineData("18,")]
        [InlineData("abc")]
        [InlineData("18,50 $")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void EssayerDeLire_RefuseCeQuiNEstPasUnNombre(string? ecrit)
        {
            Assert.False(NombreDecimal.EssayerDeLire(ecrit, out _));
        }

        [Theory]
        [InlineData("-18,50", -18.50)]
        [InlineData("+18.50", 18.50)]
        public void EssayerDeLire_AccepteUnSigne(string ecrit, double attendu)
        {
            Assert.True(NombreDecimal.EssayerDeLire(ecrit, out decimal valeur));
            Assert.Equal((decimal)attendu, valeur);
        }

        [Fact]
        public void EssayerDeLire_GardeLesQuatreDecimalesQuOnLuiDonne()
        {
            //Etant donne quatre decimales, qu'aucun groupement n'explique
            Assert.True(NombreDecimal.EssayerDeLire("3,1416", out decimal valeur));
            Assert.Equal(3.1416m, valeur);
        }
    }
}
