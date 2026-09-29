using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliothequeLIPAJOLI.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adresse",
                columns: table => new
                {
                    AdresseID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Rue = table.Column<string>(type: "TEXT", nullable: true),
                    Ville = table.Column<string>(type: "TEXT", nullable: true),
                    Province = table.Column<string>(type: "TEXT", nullable: true),
                    CodePostale = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adresse", x => x.AdresseID);
                });

            migrationBuilder.CreateTable(
                name: "Livre",
                columns: table => new
                {
                    LivreID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Isbn10 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Isbn13 = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    Titre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Prix = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Annee = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Resume = table.Column<string>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    EnStock = table.Column<bool>(type: "INTEGER", nullable: true),
                    CategorieID = table.Column<int>(type: "INTEGER", nullable: false),
                    Langue = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    NombrePages = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livre", x => x.LivreID);
                });

            migrationBuilder.CreateTable(
                name: "Personne",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Courriel = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personne", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Edition",
                columns: table => new
                {
                    EditionID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomEditeur = table.Column<string>(type: "TEXT", nullable: true),
                    AnneeEdition = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LivreID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edition", x => x.EditionID);
                    table.ForeignKey(
                        name: "FK_Edition_Livre_LivreID",
                        column: x => x.LivreID,
                        principalTable: "Livre",
                        principalColumn: "LivreID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Redaction",
                columns: table => new
                {
                    LivreID = table.Column<int>(type: "INTEGER", nullable: false),
                    AuteurID = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    Ordre = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redaction", x => new { x.LivreID, x.AuteurID });
                    table.ForeignKey(
                        name: "FK_Redaction_Livre_LivreID",
                        column: x => x.LivreID,
                        principalTable: "Livre",
                        principalColumn: "LivreID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usager",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumeroAbonne = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Statut = table.Column<int>(type: "INTEGER", nullable: false),
                    Defaillance = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usager", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Usager_Personne_ID",
                        column: x => x.ID,
                        principalTable: "Personne",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exemplaire",
                columns: table => new
                {
                    ExemplaireID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Reference = table.Column<int>(type: "INTEGER", nullable: true),
                    NbExemplaire = table.Column<int>(type: "INTEGER", nullable: true),
                    DateAchat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Fournisseur = table.Column<string>(type: "TEXT", nullable: true),
                    EditionID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exemplaire", x => x.ExemplaireID);
                    table.ForeignKey(
                        name: "FK_Exemplaire_Edition_EditionID",
                        column: x => x.EditionID,
                        principalTable: "Edition",
                        principalColumn: "EditionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsagerAdresse",
                columns: table => new
                {
                    UsagerID = table.Column<int>(type: "INTEGER", nullable: false),
                    AdresseID = table.Column<int>(type: "INTEGER", nullable: false),
                    TypeAdresse = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsagerAdresse", x => new { x.UsagerID, x.AdresseID });
                    table.ForeignKey(
                        name: "FK_UsagerAdresse_Adresse_AdresseID",
                        column: x => x.AdresseID,
                        principalTable: "Adresse",
                        principalColumn: "AdresseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsagerAdresse_Usager_UsagerID",
                        column: x => x.UsagerID,
                        principalTable: "Usager",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Emprunt",
                columns: table => new
                {
                    EmpruntID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateEmprunt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateProbableRetour = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateRetour = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExemplaireID = table.Column<int>(type: "INTEGER", nullable: false),
                    UsagerID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emprunt", x => x.EmpruntID);
                    table.ForeignKey(
                        name: "FK_Emprunt_Exemplaire_ExemplaireID",
                        column: x => x.ExemplaireID,
                        principalTable: "Exemplaire",
                        principalColumn: "ExemplaireID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Emprunt_Usager_UsagerID",
                        column: x => x.UsagerID,
                        principalTable: "Usager",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Edition_LivreID",
                table: "Edition",
                column: "LivreID");

            migrationBuilder.CreateIndex(
                name: "IX_Emprunt_ExemplaireID",
                table: "Emprunt",
                column: "ExemplaireID");

            migrationBuilder.CreateIndex(
                name: "IX_Emprunt_UsagerID",
                table: "Emprunt",
                column: "UsagerID");

            migrationBuilder.CreateIndex(
                name: "IX_Exemplaire_EditionID",
                table: "Exemplaire",
                column: "EditionID");

            migrationBuilder.CreateIndex(
                name: "IX_Livre_Code",
                table: "Livre",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Redaction_LivreID_AuteurID",
                table: "Redaction",
                columns: new[] { "LivreID", "AuteurID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsagerAdresse_AdresseID",
                table: "UsagerAdresse",
                column: "AdresseID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Emprunt");

            migrationBuilder.DropTable(
                name: "Redaction");

            migrationBuilder.DropTable(
                name: "UsagerAdresse");

            migrationBuilder.DropTable(
                name: "Exemplaire");

            migrationBuilder.DropTable(
                name: "Adresse");

            migrationBuilder.DropTable(
                name: "Usager");

            migrationBuilder.DropTable(
                name: "Edition");

            migrationBuilder.DropTable(
                name: "Personne");

            migrationBuilder.DropTable(
                name: "Livre");
        }
    }
}
