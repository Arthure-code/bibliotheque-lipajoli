using BibliothequeLIPAJOLI.Regles;

namespace BibliothequeLIPAJOLI.Tests.Regles
{
    public class PretTests
    {
        private static readonly DateTime Emprunt = new DateTime(2026, 3, 2);

        [Fact]
        public void DateLimite_AjouteLesJoursAccordes()
        {
            //Etant donne un pret de quatorze jours
            //Lorsque
            DateTime limite = Pret.DateLimite(Emprunt, 14);

            //Alors
            Assert.Equal(new DateTime(2026, 3, 16), limite);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void DateLimite_RefuseUneDureeQuiNEnEstPasUne(int jours)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Pret.DateLimite(Emprunt, jours));
        }

        [Fact]
        public void EstEnRetard_LeJourMemeNEstPasUnRetard()
        {
            //Etant donne un retour le jour de la date limite
            DateTime limite = new DateTime(2026, 3, 16);

            //Alors
            Assert.False(Pret.EstEnRetard(limite, limite));
            Assert.False(Pret.EstEnRetard(limite, limite.AddHours(23)));
        }

        [Fact]
        public void EstEnRetard_LeLendemainEnEstUn()
        {
            //Etant donne un retour le lendemain de la date limite
            DateTime limite = new DateTime(2026, 3, 16);

            //Alors
            Assert.True(Pret.EstEnRetard(limite, limite.AddDays(1)));
        }

        [Theory]
        [InlineData(0, false, 0)]
        [InlineData(0, true, 1)]
        [InlineData(2, true, 3)]
        [InlineData(2, false, 2)]
        public void DefaillancesApresRetour_NAugmenteQueSurUnRetard(int avant, bool enRetard, int attendu)
        {
            //Lorsque
            int apres = Pret.DefaillancesApresRetour(avant, enRetard);

            //Alors
            Assert.Equal(attendu, apres);
        }

        [Fact]
        public void PeutEmprunter_UnDossierSansHistoireLePermet()
        {
            //Etant donne un usager sans defaillance ni emprunt
            //Alors
            Assert.True(Pret.PeutEmprunter(0, 0, false));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public void PeutEmprunter_TroisDefaillancesBloquentLUsager(int defaillances)
        {
            //Alors
            Assert.False(Pret.PeutEmprunter(defaillances, 0, false));
        }

        [Fact]
        public void PeutEmprunter_DeuxDefaillancesNeBloquentPas()
        {
            Assert.True(Pret.PeutEmprunter(2, 0, false));
        }

        [Fact]
        public void PeutEmprunter_TroisEmpruntsEnCoursSontLeMaximum()
        {
            //Etant donne un usager qui detient deja trois exemplaires
            //Alors il ne peut pas en prendre un quatrieme
            Assert.False(Pret.PeutEmprunter(0, 3, false));
            Assert.True(Pret.PeutEmprunter(0, 2, false));
        }

        [Fact]
        public void PeutEmprunter_UnSeulExemplaireDuMemeLivreALaFois()
        {
            //Etant donne un usager qui detient deja ce livre
            //Alors
            Assert.False(Pret.PeutEmprunter(0, 1, true));
        }
    }
}
