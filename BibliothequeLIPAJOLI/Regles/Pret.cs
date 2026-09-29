namespace BibliothequeLIPAJOLI.Regles
{
    public static class Pret
    {
        public const int DefaillancesAvantBlocage = 3;

        public const int EmpruntsSimultanesMaximum = 3;

        public static DateTime DateLimite(DateTime dateEmprunt, int joursDePret)
        {
            if (joursDePret <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(joursDePret),
                    "La durée d'un prêt se compte en jours et vaut au moins un.");
            }

            return dateEmprunt.AddDays(joursDePret);
        }

        public static bool EstEnRetard(DateTime dateLimite, DateTime dateRetour)
        {
            return dateRetour.Date > dateLimite.Date;
        }

        public static int DefaillancesApresRetour(int defaillances, bool enRetard)
        {
            return enRetard ? defaillances + 1 : defaillances;
        }

        public static bool PeutEmprunter(int defaillances, int empruntsEnCours, bool detientDejaCeLivre)
        {
            return defaillances < DefaillancesAvantBlocage
                && empruntsEnCours < EmpruntsSimultanesMaximum
                && !detientDejaCeLivre;
        }
    }
}
