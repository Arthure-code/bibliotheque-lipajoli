using System.ComponentModel.DataAnnotations;
using BibliothequeLIPAJOLI.Models;
using BibliothequeLIPAJOLI.Validation;

namespace BibliothequeLIPAJOLI.Tests.Modeles
{
    public class UsagerTests
    {
        [Fact]
        public void Defaillance_PartDeZeroALInscription()
        {
            //Etant donne un usager qu'on vient d'inscrire
            var usager = new Usager();

            //Alors
            Assert.Equal(0, usager.Defaillance);
            Assert.True(usager.PeutEmprunter);
        }

        [Fact]
        public void PorterUneDefaillance_AugmenteLeCompteurDUn()
        {
            //Etant donne un dossier vierge
            var usager = new Usager();

            //Lorsque deux retours en retard sont portes
            usager.PorterUneDefaillance();
            usager.PorterUneDefaillance();

            //Alors
            Assert.Equal(2, usager.Defaillance);
            Assert.True(usager.PeutEmprunter);
        }

        [Fact]
        public void PeutEmprunter_DevientFauxALaTroisiemeDefaillance()
        {
            //Etant donne un usager qui a rendu trois fois en retard
            var usager = new Usager();
            usager.PorterUneDefaillance();
            usager.PorterUneDefaillance();
            usager.PorterUneDefaillance();

            //Alors
            Assert.Equal(3, usager.Defaillance);
            Assert.False(usager.PeutEmprunter);
        }

        [Fact]
        public void EmpruntsEnCours_NeCompteQueCeQuiNEstPasRendu()
        {
            //Etant donne deux emprunts rendus et un en cours
            var usager = new Usager
            {
                Emprunts = new List<Emprunt>
                {
                    new Emprunt { DateRetour = new DateTime(2026, 1, 2) },
                    new Emprunt { DateRetour = new DateTime(2026, 2, 3) },
                    new Emprunt { DateRetour = null }
                }
            };

            //Alors
            Assert.Equal(1, usager.EmpruntsEnCours);
        }

        [Fact]
        public void NomComplet_AssembleLePrenomEtLeNom()
        {
            //Etant donne une personne
            var usager = new Usager { Nom = "Tremblay", Prenom = "Marie" };

            //Alors
            Assert.Equal("Marie Tremblay", usager.NomComplet);
        }
    }

    public class LivreTests
    {
        private static Livre AvecExemplaires(params Exemplaire[] exemplaires) => new Livre
        {
            Editions = new List<Edition>
            {
                new Edition { Exemplaires = exemplaires.ToList() }
            }
        };

        [Fact]
        public void Quantite_AdditionneLesExemplairesDeToutesLesEditions()
        {
            //Etant donne deux editions de trois et deux exemplaires
            var livre = new Livre
            {
                Editions = new List<Edition>
                {
                    new Edition { Exemplaires = new List<Exemplaire> { new Exemplaire { NbExemplaire = 3 } } },
                    new Edition { Exemplaires = new List<Exemplaire> { new Exemplaire { NbExemplaire = 2 } } }
                }
            };

            //Alors
            Assert.Equal(5, livre.Quantite);
        }

        [Fact]
        public void Quantite_VautZeroQuandLeLivreNAAucunExemplaire()
        {
            Assert.Equal(0, new Livre().Quantite);
        }

        [Fact]
        public void QuantiteDisponible_RetrancheLesEmpruntsNonRendus()
        {
            //Etant donne cinq exemplaires dont deux sont sortis
            Livre livre = AvecExemplaires(new Exemplaire
            {
                NbExemplaire = 5,
                Emprunts = new List<Emprunt>
                {
                    new Emprunt { DateRetour = null },
                    new Emprunt { DateRetour = null },
                    new Emprunt { DateRetour = new DateTime(2026, 1, 1) }
                }
            });

            //Alors
            Assert.Equal(5, livre.Quantite);
            Assert.Equal(3, livre.QuantiteDisponible);
        }
    }

    public class EmpruntTests
    {
        [Fact]
        public void EstRetourne_DependDeLaDateDeRetour()
        {
            Assert.False(new Emprunt().EstRetourne);
            Assert.True(new Emprunt { DateRetour = new DateTime(2026, 1, 1) }.EstRetourne);
        }

        [Fact]
        public void EstEnRetard_UnEmpruntRenduNEstJamaisEnRetard()
        {
            //Etant donne un emprunt rendu apres la date prevue
            var emprunt = new Emprunt
            {
                DateProbableRetour = new DateTime(2020, 1, 1),
                DateRetour = new DateTime(2020, 5, 1)
            };

            //Alors le retard ne concerne que ce qui n'est pas revenu
            Assert.False(emprunt.EstEnretard);
        }

        [Fact]
        public void EstEnRetard_UnEmpruntNonRenduDontLaDateEstPasseeLEst()
        {
            var emprunt = new Emprunt { DateProbableRetour = new DateTime(2020, 1, 1) };

            Assert.True(emprunt.EstEnretard);
        }
    }

    public class AuPlusDeuxDecimalesTests
    {
        private readonly AuPlusDeuxDecimalesAttribute _regle = new AuPlusDeuxDecimalesAttribute();

        [Theory]
        [InlineData(18)]
        [InlineData(18.5)]
        [InlineData(0)]
        [InlineData(0.05)]
        public void IsValid_AccepteAuPlusDeuxDecimales(double valeur)
        {
            Assert.True(_regle.IsValid((decimal)valeur));
        }

        [Fact]
        public void IsValid_RefuseUneTroisiemeDecimale()
        {
            Assert.False(_regle.IsValid(18.505m));
        }

        [Fact]
        public void IsValid_UnPrixAbsentNEstPasUneFaute()
        {
            //Alors c'est a l'attribut Required de le dire, pas a celui-ci
            Assert.True(_regle.IsValid(null));
        }

        [Fact]
        public void IsValid_RefuseCeQuiNEstPasUnNombre()
        {
            Assert.False(_regle.IsValid("18,50"));
        }
    }
}
