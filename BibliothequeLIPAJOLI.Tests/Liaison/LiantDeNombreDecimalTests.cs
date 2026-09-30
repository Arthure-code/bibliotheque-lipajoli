using System.Globalization;
using BibliothequeLIPAJOLI.Liaison;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Moq;

namespace BibliothequeLIPAJOLI.Tests.Liaison
{
    public class LiantDeNombreDecimalTests
    {
        private const string Champ = "Livre.Prix";

        //Le formulaire tel que le navigateur l'envoie : des chaines, rien d'autre.
        private sealed class ChampsDuFormulaire : IValueProvider
        {
            private readonly Dictionary<string, string> _champs;

            public ChampsDuFormulaire(params (string Nom, string Valeur)[] champs)
            {
                _champs = champs.ToDictionary(c => c.Nom, c => c.Valeur);
            }

            public bool ContainsPrefix(string prefix) => _champs.ContainsKey(prefix);

            public ValueProviderResult GetValue(string key)
            {
                return _champs.TryGetValue(key, out string? valeur)
                    ? new ValueProviderResult(new StringValues(valeur), CultureInfo.InvariantCulture)
                    : ValueProviderResult.None;
            }
        }

        private static DefaultModelBindingContext Contexte(params (string, string)[] champs)
        {
            return new DefaultModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(decimal?)),
                ModelName = Champ,
                ModelState = new ModelStateDictionary(),
                ValueProvider = new ChampsDuFormulaire(champs)
            };
        }

        [Theory]
        [InlineData("18,50", 18.50)]
        [InlineData("18.50", 18.50)]
        [InlineData("1 234,56", 1234.56)]
        [InlineData("0", 0)]
        public async Task LeMemeNombreSEcritDesDeuxFacons(string saisie, double attendu)
        {
            //Etant donne un prix saisi a la francaise ou a l'anglaise
            DefaultModelBindingContext contexte = Contexte((Champ, saisie));

            //Lorsque le cadriciel lie le formulaire
            await new LiantDeNombreDecimal().BindModelAsync(contexte);

            //Alors une seule valeur en sort
            Assert.True(contexte.Result.IsModelSet);
            Assert.Equal((decimal)attendu, contexte.Result.Model);
            Assert.Equal(0, contexte.ModelState.ErrorCount);
        }

        [Fact]
        public async Task UnChampVideNEstPasUneFaute()
        {
            //Etant donne un prix laisse vide
            DefaultModelBindingContext contexte = Contexte((Champ, ""));

            //Lorsque
            await new LiantDeNombreDecimal().BindModelAsync(contexte);

            //Alors c'est au modele de dire s'il etait obligatoire
            Assert.True(contexte.Result.IsModelSet);
            Assert.Null(contexte.Result.Model);
            Assert.Equal(0, contexte.ModelState.ErrorCount);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("10,000")]
        [InlineData("18.")]
        public async Task CeQuiNEstPasUnNombreEstSignaleAuVisiteur(string saisie)
        {
            //Etant donne une saisie qu'on ne peut pas lire sans deviner
            DefaultModelBindingContext contexte = Contexte((Champ, saisie));

            //Lorsque
            await new LiantDeNombreDecimal().BindModelAsync(contexte);

            //Alors le champ garde ce que la personne a ecrit, avec un message
            Assert.False(contexte.Result.IsModelSet);
            Assert.Equal(1, contexte.ModelState.ErrorCount);
            Assert.Equal(saisie, contexte.ModelState[Champ]!.AttemptedValue);
        }

        [Fact]
        public async Task UnChampAbsentNEstPasTouche()
        {
            //Etant donne un formulaire qui ne porte pas ce champ
            DefaultModelBindingContext contexte = Contexte(("Livre.Titre", "Les Misérables"));

            //Lorsque
            await new LiantDeNombreDecimal().BindModelAsync(contexte);

            //Alors rien n'est lie et rien n'est reproche
            Assert.False(contexte.Result.IsModelSet);
            Assert.True(contexte.ModelState.IsValid);
            Assert.Empty(contexte.ModelState);
        }

        [Fact]
        public async Task LeLiantExigeUnContexte()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => new LiantDeNombreDecimal().BindModelAsync(null!));
        }
    }

    public class FournisseurDeLiantDecimalTests
    {
        private readonly FournisseurDeLiantDecimal _fournisseur = new FournisseurDeLiantDecimal();

        private static ModelBinderProviderContext Contexte(Type type)
        {
            var contexte = new Mock<ModelBinderProviderContext>();
            contexte.SetupGet(c => c.Metadata)
                .Returns(new EmptyModelMetadataProvider().GetMetadataForType(type));

            return contexte.Object;
        }

        [Theory]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(decimal?))]
        public void UnNombreDecimalRecoitNotreLiant(Type type)
        {
            Assert.IsType<LiantDeNombreDecimal>(_fournisseur.GetBinder(Contexte(type)));
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(DateTime))]
        public void LeResteGardeLeLiantDuCadriciel(Type type)
        {
            Assert.Null(_fournisseur.GetBinder(Contexte(type)));
        }

        [Fact]
        public void LeFournisseurExigeUnContexte()
        {
            Assert.Throws<ArgumentNullException>(() => _fournisseur.GetBinder(null!));
        }
    }
}
