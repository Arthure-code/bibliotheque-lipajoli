using System.ComponentModel.DataAnnotations;
using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.Tests.Modeles
{
    public class LivreValidationTests
    {
        private static Livre LivreComplet() => new Livre
        {
            Titre = "Les Misérables",
            Isbn10 = "1234567890",
            Isbn13 = "9781234567890",
            Prix = 14.99m,
            CategorieID = 1,
            Annee = new DateTime(1862, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
            Resume = "Une fresque sociale et humaine.",
            ImageUrl = "/images/les-miserables.jpg",
            Langue = "Français",
            NombrePages = 1232
        };

        private static List<ValidationResult> Valider(Livre livre)
        {
            var fautes = new List<ValidationResult>();
            Validator.TryValidateObject(livre, new ValidationContext(livre), fautes, validateAllProperties: true);
            return fautes;
        }

        [Fact]
        public void UnLivreCompletEstAccepte()
        {
            //Alors
            Assert.Empty(Valider(LivreComplet()));
        }

        [Fact]
        public void LeCodeNEstPasExigeDuFormulaire()
        {
            //Etant donne un livre sans code, puisque la bibliotheque
            //l'attribue elle-meme a l'enregistrement
            Livre livre = LivreComplet();

            //Alors la validation ne le reclame pas
            Assert.Null(livre.Code);
            Assert.Empty(Valider(livre));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void LeTitreEstObligatoire(string? titre)
        {
            //Etant donne un livre sans titre
            Livre livre = LivreComplet();
            livre.Titre = titre;

            //Alors
            Assert.Contains(Valider(livre), f => f.ErrorMessage == "Le titre est requis.");
        }

        [Theory]
        [InlineData("123456789")]
        [InlineData("12345678901")]
        [InlineData("12345abcde")]
        public void LIsbnDixExigeDixChiffres(string isbn)
        {
            //Etant donne un ISBN mal forme
            Livre livre = LivreComplet();
            livre.Isbn10 = isbn;

            //Alors
            Assert.NotEmpty(Valider(livre));
        }

        [Theory]
        [InlineData("978123456789")]
        [InlineData("97812345678901")]
        [InlineData("978123456789X")]
        public void LIsbnTreizeExigeTreizeChiffres(string isbn)
        {
            //Etant donne un ISBN mal forme
            Livre livre = LivreComplet();
            livre.Isbn13 = isbn;

            //Alors
            Assert.NotEmpty(Valider(livre));
        }

        [Fact]
        public void LePrixNePeutPasEtreNegatif()
        {
            //Etant donne un prix negatif
            Livre livre = LivreComplet();
            livre.Prix = -1m;

            //Alors
            Assert.Contains(Valider(livre), f => f.ErrorMessage == "Le prix ne peut pas être négatif.");
        }

        [Fact]
        public void LePrixSArreteADeuxDecimales()
        {
            //Etant donne un prix a trois decimales
            Livre livre = LivreComplet();
            livre.Prix = 14.995m;

            //Alors
            Assert.Contains(Valider(livre), f => f.ErrorMessage == "Le prix s'écrit avec au plus deux décimales.");
        }

        [Fact]
        public void LePrixPeutManquer()
        {
            //Etant donne un livre sans prix, qui ne sert qu'a facturer une perte
            Livre livre = LivreComplet();
            livre.Prix = null;

            //Alors
            Assert.Empty(Valider(livre));
        }

        [Fact]
        public void UneCategorieDoitEtreChoisie()
        {
            //Etant donne aucune categorie retenue dans la liste
            Livre livre = LivreComplet();
            livre.CategorieID = 0;

            //Alors
            Assert.Contains(Valider(livre), f => f.ErrorMessage == "La catégorie est requise.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10001)]
        public void LeNombreDePagesResteDansSesBornes(int pages)
        {
            //Etant donne un nombre de pages hors des bornes
            Livre livre = LivreComplet();
            livre.NombrePages = pages;

            //Alors
            Assert.NotEmpty(Valider(livre));
        }

        [Fact]
        public void LaLangueNeDepassePasQuinzeCaracteres()
        {
            //Etant donne une langue trop longue
            Livre livre = LivreComplet();
            livre.Langue = new string('a', 16);

            //Alors
            Assert.NotEmpty(Valider(livre));
        }

        [Fact]
        public void UnLivreNeufNaNiEditionNiRedaction()
        {
            //Etant donne un livre qu'on vient de creer
            var livre = new Livre();

            //Alors ses collections existent et sont vides
            Assert.Empty(livre.Editions);
            Assert.Empty(livre.Redactions);
            Assert.Equal(0, livre.Quantite);
            Assert.Equal(0, livre.QuantiteDisponible);
        }

        [Fact]
        public void LeLivreGardeCeQuOnLuiDonne()
        {
            //Etant donne un livre complet
            Livre livre = LivreComplet();
            livre.Code = "ROM001";
            livre.EnStock = true;
            livre.Categorie = new Categorie { CategorieID = 1, NomCategorie = "Roman" };

            //Alors chaque valeur se relit telle quelle
            Assert.Equal("ROM001", livre.Code);
            Assert.Equal("Les Misérables", livre.Titre);
            Assert.Equal("1234567890", livre.Isbn10);
            Assert.Equal("9781234567890", livre.Isbn13);
            Assert.Equal(14.99m, livre.Prix);
            Assert.Equal(1862, livre.Annee!.Value.Year);
            Assert.Equal("Une fresque sociale et humaine.", livre.Resume);
            Assert.Equal("/images/les-miserables.jpg", livre.ImageUrl);
            Assert.Equal("Français", livre.Langue);
            Assert.Equal(1232, livre.NombrePages);
            Assert.True(livre.EnStock);
            Assert.Equal("Roman", livre.Categorie!.NomCategorie);
        }
    }
}
