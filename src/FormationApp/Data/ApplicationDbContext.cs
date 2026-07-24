using FormationApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Categorie> Categories => Set<Categorie>();
    public DbSet<Formateur> Formateurs => Set<Formateur>();
    public DbSet<Formation> Formations => Set<Formation>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Modalite> Modalites => Set<Modalite>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<OptionReponse> OptionsReponse => Set<OptionReponse>();
    public DbSet<Inscription> Inscriptions => Set<Inscription>();
    public DbSet<ResultatExamen> ResultatsExamen => Set<ResultatExamen>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Formation -> Categorie (Restrict pour ne pas supprimer une catégorie utilisée)
        builder.Entity<Formation>()
            .HasOne(f => f.Categorie)
            .WithMany(c => c.Formations)
            .HasForeignKey(f => f.CategorieId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Formation>()
            .HasOne(f => f.Formateur)
            .WithMany(fo => fo.Formations)
            .HasForeignKey(f => f.FormateurId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Module>()
            .HasOne(m => m.Formation)
            .WithMany(f => f.Modules)
            .HasForeignKey(m => m.FormationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Modalite>()
            .HasOne(mo => mo.Module)
            .WithMany(m => m.Modalites)
            .HasForeignKey(mo => mo.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Question>()
            .HasOne(q => q.Modalite)
            .WithMany(mo => mo.Questions)
            .HasForeignKey(q => q.ModaliteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OptionReponse>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Inscription>()
            .HasOne(i => i.Formation)
            .WithMany(f => f.Inscriptions)
            .HasForeignKey(i => i.FormationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Inscription>()
            .HasOne(i => i.ApplicationUser)
            .WithMany(u => u.Inscriptions)
            .HasForeignKey(i => i.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Un apprenant ne peut s'inscrire qu'une seule fois à la même formation
        builder.Entity<Inscription>()
            .HasIndex(i => new { i.ApplicationUserId, i.FormationId })
            .IsUnique();

        builder.Entity<ResultatExamen>()
            .HasOne(r => r.Modalite)
            .WithMany(mo => mo.Resultats)
            .HasForeignKey(r => r.ModaliteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Formateur>()
            .HasIndex(f => f.Email)
            .IsUnique();

        builder.Entity<Modalite>()
            .Property(m => m.Type)
            .HasConversion<string>();

        builder.Entity<Formation>()
            .Property(f => f.Statut)
            .HasConversion<string>();

        builder.Entity<Inscription>()
            .Property(i => i.Statut)
            .HasConversion<string>();
    }
}
