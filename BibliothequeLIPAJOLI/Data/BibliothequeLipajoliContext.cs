using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.Data
{
    public class BibliothequeLipajoliContext : DbContext
    {
        public BibliothequeLipajoliContext(DbContextOptions<BibliothequeLipajoliContext> options)
            : base(options)
        {
        }
        public DbSet<Livre> Livres { get; set; }
        public DbSet<Redaction> Redactions { get; set; }
        public DbSet<Emprunt> Emprunts { get; set; }
        public DbSet<Adresse> Adresses { get; set; }
        public DbSet<UsagerAdresse> UsagerAdresses { get; set; }
        public DbSet<Exemplaire> Exemplaires { get; set; }
        public DbSet<Usager> Usagers { get; set; }
        public DbSet<Edition> Editions { get; set; }
        public DbSet<Personne> Personnes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           
            modelBuilder.Ignore<Auteur>();
            modelBuilder.Ignore<Categorie>();
            modelBuilder.Entity<Usager>().ToTable("Usager");
            modelBuilder.Entity<Livre>().ToTable("Livre");
            modelBuilder.Entity<Edition>().ToTable("Edition");
            modelBuilder.Entity<Emprunt>().ToTable("Emprunt");
            modelBuilder.Entity<Exemplaire>().ToTable("Exemplaire");
            modelBuilder.Entity<Personne>().ToTable("Personne");
            modelBuilder.Entity<Redaction>().ToTable("Redaction");
            modelBuilder.Entity<Adresse>().ToTable("Adresse");
            modelBuilder.Entity<UsagerAdresse>().ToTable("UsagerAdresse");

            modelBuilder.Entity<Livre>()
    .HasKey(l => l.LivreID);

            modelBuilder.Entity<Livre>()
                .Property(l => l.LivreID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Livre>(entity =>
            {
                entity.Property(l => l.CategorieID);
                entity.Ignore(l => l.Categorie);

                // Une somme d'argent se déclare, sans quoi la précision
                // change d'une base à l'autre.
                entity.Property(l => l.Prix).HasPrecision(18, 2);

                entity.Property(l => l.Code).IsRequired().HasMaxLength(20);
                entity.HasIndex(l => l.Code).IsUnique();
            });

            modelBuilder.Entity<UsagerAdresse>()
    .HasOne(ua => ua.Adresse)
    .WithMany(a => a.UsagerAdresses)
    .HasForeignKey(ua => ua.AdresseID);

            modelBuilder.Entity<UsagerAdresse>()
    .HasKey(ua => new { ua.UsagerID, ua.AdresseID });

            modelBuilder.Entity<UsagerAdresse>()
                .HasOne(ua => ua.Usager)
                .WithMany(u => u.UsagerAdresses)
                .HasForeignKey(ua => ua.UsagerID);

            modelBuilder.Entity<Redaction>()
    .HasIndex(r => new { r.LivreID, r.AuteurID})
    .IsUnique();

            modelBuilder.Entity<Redaction>(entity =>
            {
                entity.HasKey(r => new { r.LivreID, r.AuteurID});
                entity.HasOne(r => r.Livre)
                      .WithMany(l => l.Redactions)
                      .HasForeignKey(r => r.LivreID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(r => r.AuteurID).IsRequired();
                entity.Ignore(r => r.Auteur);
            });

            modelBuilder.Entity<Exemplaire>()
                .HasOne(e => e.Edition)
                .WithMany(ed => ed.Exemplaires)
                .HasForeignKey(e => e.EditionID);

           modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.Usager)
                .WithMany(u => u.Emprunts)
                .HasForeignKey(e => e.UsagerID);

            modelBuilder.Entity<Emprunt>()
     .HasOne(e => e.Exemplaire)
     .WithMany(ex => ex.Emprunts)
     .HasForeignKey(e => e.ExemplaireID)
     .OnDelete(DeleteBehavior.Restrict);

           modelBuilder.Entity<Edition>()
                .HasOne(ed => ed.Livre)
                .WithMany(l=>l.Editions)
                .HasForeignKey(ed => ed.LivreID);
        }
    }
}