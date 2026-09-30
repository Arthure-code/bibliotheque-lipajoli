using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.Regles;

namespace BibliothequeLIPAJOLI.Tests.Data
{
    public class DonneesDeDepartTests
    {
        private const int JoursDePret = 14;

        [Fact]
        public void LesLivresOntChacunLeurCodeEtLeurCategorie()
        {
            //Etant donne le catalogue de depart
            List<Livre> livres = DonneesDeDepart.Livres();

            //Alors chaque code est unique et suit sa categorie
            Assert.Equal(4, livres.Count);
            Assert.Equal(livres.Count, livres.Select(l => l.Code).Distinct().Count());
            Assert.Equal(livres.Count, livres.Select(l => l.CategorieID).Distinct().Count());
            Assert.All(livres, l => Assert.Matches("^[A-Z]{3}[0-9]{3}$", l.Code!));
        }

        [Fact]
        public void LesLivresPortentDesIsbnBienFormes()
        {
            //Alors dix chiffres d'un cote, treize de l'autre
            Assert.All(DonneesDeDepart.Livres(), livre =>
            {
                Assert.Matches("^[0-9]{10}$", livre.Isbn10!);
                Assert.Matches("^[0-9]{13}$", livre.Isbn13!);
            });
        }

        [Fact]
        public void ToutesLesDatesSontCiviles()
        {
            //Etant donne les dates semees, que SQLite recoit sans fuseau
            var dates = new List<DateTime?>();
            dates.AddRange(DonneesDeDepart.Livres().Select(l => l.Annee));
            dates.AddRange(DonneesDeDepart.Editions().Select(e => e.AnneeEdition));
            dates.AddRange(DonneesDeDepart.Exemplaires().Select(e => e.DateAchat));
            dates.AddRange(DonneesDeDepart.Emprunts(JoursDePret).Select(e => e.DateEmprunt));

            //Alors aucune ne pretend connaitre un fuseau
            Assert.All(dates, date => Assert.Equal(DateTimeKind.Unspecified, date!.Value.Kind));
        }

        [Fact]
        public void ChaqueLienDesigneQuelqueChoseQuiExiste()
        {
            //Etant donne les cles semees
            List<int> livres = DonneesDeDepart.Livres().Select(l => l.LivreID).ToList();
            List<int> editions = DonneesDeDepart.Editions().Select(e => e.EditionID).ToList();
            List<int> exemplaires = DonneesDeDepart.Exemplaires().Select(e => e.ExemplaireID).ToList();
            List<int> usagers = DonneesDeDepart.Usagers().Select(u => u.ID).ToList();
            List<int> adresses = DonneesDeDepart.Adresses().Select(a => a.AdresseID).ToList();

            //Alors aucun lien ne pend dans le vide
            Assert.All(DonneesDeDepart.Editions(), e => Assert.Contains(e.LivreID, livres));
            Assert.All(DonneesDeDepart.Redactions(), r => Assert.Contains(r.LivreID, livres));
            Assert.All(DonneesDeDepart.Exemplaires(), e => Assert.Contains(e.EditionID, editions));
            Assert.All(DonneesDeDepart.Emprunts(JoursDePret), e =>
            {
                Assert.Contains(e.UsagerID, usagers);
                Assert.Contains(e.ExemplaireID, exemplaires);
            });
            Assert.All(DonneesDeDepart.UsagerAdresses(), ua =>
            {
                Assert.Contains(ua.UsagerID, usagers);
                Assert.Contains(ua.AdresseID, adresses);
            });
        }

        [Fact]
        public void ChaqueEmpruntPorteSaDateLimite()
        {
            //Etant donne une duree de pret de quatorze jours
            List<Emprunt> emprunts = DonneesDeDepart.Emprunts(JoursDePret);

            //Alors la date limite se calcule, elle ne s'ecrit pas a la main
            Assert.All(emprunts, emprunt => Assert.Equal(
                Pret.DateLimite(emprunt.DateEmprunt!.Value, JoursDePret),
                emprunt.DateProbableRetour));
        }

        [Fact]
        public void LaDureeDePretSuitLaConfiguration()
        {
            //Etant donne une bibliotheque qui prete pour vingt et un jours
            List<Emprunt> emprunts = DonneesDeDepart.Emprunts(21);

            //Alors les dates limites suivent, rien n'est fige dans le code
            Assert.All(emprunts, emprunt => Assert.Equal(
                emprunt.DateEmprunt!.Value.AddDays(21), emprunt.DateProbableRetour));
        }

        [Fact]
        public void SeulLeRetourEnRetardPorteUneDefaillance()
        {
            //Etant donne les usagers et les emprunts de depart
            List<Usager> usagers = DonneesDeDepart.Usagers();
            List<Emprunt> emprunts = DonneesDeDepart.Emprunts(JoursDePret);

            //Lorsque la bibliotheque fait les comptes
            DonneesDeDepart.PorterLesDefaillances(usagers, emprunts);

            //Alors Marie a rendu a temps, Pierre a rendu en retard une fois,
            //et Sophie n'a encore rien rendu
            Assert.Equal(0, usagers.Single(u => u.Prenom == "Marie").Defaillance);
            Assert.Equal(1, usagers.Single(u => u.Prenom == "Pierre").Defaillance);
            Assert.Equal(0, usagers.Single(u => u.Prenom == "Sophie").Defaillance);
            Assert.All(usagers, u => Assert.True(u.PeutEmprunter));
        }

        [Fact]
        public void SophieTientLeMaximumDEmpruntsEnCours()
        {
            //Etant donne les emprunts de depart
            List<Emprunt> emprunts = DonneesDeDepart.Emprunts(JoursDePret);

            //Alors Sophie en a trois sur les bras, le maximum autorise
            int enCours = emprunts.Count(e => e.UsagerID == 3 && !e.EstRetourne);
            Assert.Equal(Pret.EmpruntsSimultanesMaximum, enCours);
        }

        [Fact]
        public void PorterLesDefaillances_IgnoreUnEmpruntDontLUsagerEstInconnu()
        {
            //Etant donne un emprunt rendu en retard par quelqu'un d'absent
            List<Usager> usagers = DonneesDeDepart.Usagers();
            var emprunts = new List<Emprunt>
            {
                new Emprunt
                {
                    UsagerID = 99,
                    DateProbableRetour = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                    DateRetour = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Unspecified)
                }
            };

            //Lorsque
            DonneesDeDepart.PorterLesDefaillances(usagers, emprunts);

            //Alors personne ne paie pour lui
            Assert.All(usagers, u => Assert.Equal(0, u.Defaillance));
        }

        [Fact]
        public void PorterLesDefaillances_RefuseUneListeAbsente()
        {
            Assert.Throws<ArgumentNullException>(
                () => DonneesDeDepart.PorterLesDefaillances(null!, new List<Emprunt>()));
            Assert.Throws<ArgumentNullException>(
                () => DonneesDeDepart.PorterLesDefaillances(new List<Usager>(), null!));
        }
    }
}
