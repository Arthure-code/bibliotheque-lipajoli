# bibliotheque-lipajoli

[![Build](https://github.com/Arthure-code/bibliotheque-lipajoli/actions/workflows/build.yml/badge.svg)](https://github.com/Arthure-code/bibliotheque-lipajoli/actions/workflows/build.yml)
[![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=Arthure-code_bibliotheque-lipajoli&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Arthure-code_bibliotheque-lipajoli)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Arthure-code_bibliotheque-lipajoli&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Arthure-code_bibliotheque-lipajoli)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Arthure-code_bibliotheque-lipajoli&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Arthure-code_bibliotheque-lipajoli)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Arthure-code_bibliotheque-lipajoli&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Arthure-code_bibliotheque-lipajoli)
[![Security rating](https://sonarcloud.io/api/project_badges/measure?project=Arthure-code_bibliotheque-lipajoli&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Arthure-code_bibliotheque-lipajoli)
[![Code smells](https://sonarcloud.io/api/project_badges/measure?project=Arthure-code_bibliotheque-lipajoli&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Arthure-code_bibliotheque-lipajoli)
[![Duplicated lines](https://sonarcloud.io/api/project_badges/measure?project=Arthure-code_bibliotheque-lipajoli&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Arthure-code_bibliotheque-lipajoli)

A library counter: books on the shelves, members with a file, and the loans
between them. A book is catalogued and given a code, a member is registered and
followed, and the file remembers who returned late.

ASP.NET Core 8 MVC, Entity Framework Core 8, SQLite. The site is in French.

## Screenshots

**The catalogue**

![The book list: a search box for a title or an author, a category selector, then a table with the code, the title, the authors, the category, the total quantity and the quantity still on the shelf, each row offering edit, details and delete](docs/catalogue.png)

**A book**

![The page of Les Misérables: its two ISBNs, its code, its price, its category, its summary and its year, then its authors, its editions, the copies the library owns and the loans made on them](docs/livre-details.png)

**Cataloguing a book**

![The creation form: title, both ISBNs, price, category, summary, year, a multiple selection of authors, then the edition and the copy. No field asks for the code: a line says the library assigns it from the category](docs/livre-nouveau.png)

**The members**

![The member list: a search box on the name or first name, then a table with the subscriber number, the name, the first name, the email, the status and the number of failures, one member showing one](docs/usagers.png)

**A member file**

![The file of Pierre Gagnon: his subscriber number, his name, his email, his status, his failures, his loans in progress and whether he may borrow, then his addresses and the history of his loans with the dates](docs/usager-details.png)

**Editing a member**

![The edit form: subscriber number, name, first name, email, status, then the street, city, province, postal code and the kind of address](docs/usager-modifier.png)

## How it works

**A book code is assigned, never typed.** Three letters from the category and a
three-digit sequence of its own: PRO001, PRO002, RES001. The form does not ask
for it, a posted code is refused at binding, and the column carries a unique
index. The rule itself is a pure function of the category name and the codes
already given.

**A price is read the same way on both sides of the Atlantic.** 18,50 and 18.50
give the same number, and so do 1 234,56, 1,234.56 and 1.234,56. What cannot be
decided is refused rather than guessed: 10,000 means ten thousand to one reader
and ten to another, so the form asks again instead of storing one of the two.
Whatever is typed, the column holds a single form.

**A failure is a late return, not a loan in progress.** The counter starts at
zero when the member registers, no form can write it, and only a return past
the limit date increases it. Three failures and the member stops borrowing,
three loans in hand and they stop too. The loan duration comes from the
configuration file, not from the code.

**Identifiers come from the address bar.** The keys refuse model binding, so a
posted identifier cannot send a save to another file, and the update reads the
key from the route.

**Authors and categories are not in the database.** They are read once from the
configuration at startup. Searching an author resolves the names in memory,
without accents or case, then keeps the books they signed.

**A member, their address and the link between them are written together.** One
save, or nothing: a failure halfway leaves no orphan address behind.

**The controllers never see the database.** They hold an interface, the service
behind it holds the context. That is what makes the controllers testable with
nothing but mocks.

## Running it

```bash
cd BibliothequeLIPAJOLI
dotnet run
```

The SQLite file is created by the migrations on the first run and filled with
four books, three members, their addresses, editions, copies and loans. Delete
`bibliotheque.db` to start over.

```bash
dotnet test
```

## Résumé

Comptoir de bibliothèque en ASP.NET Core 8 MVC avec Entity Framework Core et
SQLite. Un livre reçoit son code de la bibliothèque, trois lettres de sa
catégorie et une séquence propre à celle-ci, et ce code n'est ni saisi ni
modifiable. Un prix s'écrit avec une virgule ou un point et arrive en base sous
une seule forme ; une écriture ambiguë est refusée plutôt que devinée. La
défaillance d'un usager compte ses retours en retard, part de zéro, ne se
saisit pas, et trois défaillances l'empêchent d'emprunter, comme trois emprunts
en cours. Les identifiants viennent de l'adresse et jamais du formulaire. Les
auteurs, les catégories et la durée d'un prêt sont lus dans la configuration.
Les contrôleurs ne connaissent que des interfaces, les services parlent seuls à
la base, et cent soixante tests unitaires vérifient les règles, les
annotations, la lecture des nombres et les contrôleurs avec des mocks.

## Licence

MIT. See [LICENSE](LICENSE).
