using FormationApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Data;

/// <summary>
/// Initialise la base : rôles, compte admin par défaut et quelques données de démonstration.
/// Appelé une fois au démarrage depuis Program.cs.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // 1. Rôles
        foreach (var role in new[] { Roles.Admin, Roles.Formateur, Roles.Apprenant })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Compte administrateur par défaut (configurable via appsettings / variables d'environnement)
        var adminEmail = config["SeedAdmin:Email"] ?? "admin@formationapp.local";
        var adminPassword = config["SeedAdmin:Password"] ?? "Admin@12345";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nom = "Administrateur",
                Prenom = "Système",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }

        // 3. Données de démonstration (uniquement si la base est vide)
        if (!context.Categories.Any())
        {
            var categories = new[]
            {
                new Categorie { Nom = "Développement Web", Description = "HTML, CSS, JavaScript, ASP.NET, React..." },
                new Categorie { Nom = "Data & Intelligence Artificielle", Description = "Python, Machine Learning, IA générative" },
                new Categorie { Nom = "Gestion de projet", Description = "Agilité, Scrum, méthodes projet" }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            var formateur = new Formateur
            {
                Nom = "Bernard",
                Prenom = "Claire",
                Email = "claire.bernard@formationapp.local",
                Specialite = "Développement Full-Stack .NET",
                Bio = "10 ans d'expérience en développement web et formation professionnelle."
            };
            context.Formateurs.Add(formateur);
            await context.SaveChangesAsync();

            var formation = new Formation
            {
                Titre = "ASP.NET Core pour développeurs débutants",
                Description = "Apprenez à construire des applications web modernes avec ASP.NET Core, Entity Framework et une interface d'administration complète.",
                CategorieId = categories[0].Id,
                FormateurId = formateur.Id,
                Statut = StatutFormation.Publiee,
                PlacesDisponibles = 25,
                DateDebut = DateTime.UtcNow.AddDays(7),
                DateFin = DateTime.UtcNow.AddDays(37)
            };
            context.Formations.Add(formation);
            await context.SaveChangesAsync();

            var module1 = new Module
            {
                FormationId = formation.Id,
                Titre = "Introduction à ASP.NET Core",
                Description = "Les fondamentaux du framework et de l'architecture MVC.",
                Ordre = 1,
                ContexteIA = "Ce module couvre l'architecture MVC, le cycle de vie des requêtes ASP.NET Core, et l'injection de dépendances."
            };
            context.Modules.Add(module1);
            await context.SaveChangesAsync();

            var cours = new Modalite
            {
                ModuleId = module1.Id,
                Titre = "Présentation du framework",
                Type = TypeModalite.Cours,
                Ordre = 1,
                Contenu = "ASP.NET Core est un framework open-source et multiplateforme pour créer des applications web modernes...",
                DureeMinutes = 20
            };
            var exercice = new Modalite
            {
                ModuleId = module1.Id,
                Titre = "Créer votre premier contrôleur",
                Type = TypeModalite.Exercice,
                Ordre = 2,
                Contenu = "Créez un contrôleur MVC nommé HomeController avec une action Index qui retourne une vue.",
                DureeMinutes = 30
            };
            var examen = new Modalite
            {
                ModuleId = module1.Id,
                Titre = "Quiz : les fondamentaux",
                Type = TypeModalite.Examen,
                Ordre = 3,
                ScoreMinReussite = 60,
                DureeMinutes = 10
            };
            context.Modalites.AddRange(cours, exercice, examen);
            await context.SaveChangesAsync();

            var question = new Question
            {
                ModaliteId = examen.Id,
                Enonce = "Quel motif de conception ASP.NET Core utilise-t-il nativement pour fournir des services ?",
                Ordre = 1
            };
            context.Questions.Add(question);
            await context.SaveChangesAsync();

            context.OptionsReponse.AddRange(
                new OptionReponse { QuestionId = question.Id, Texte = "L'injection de dépendances", EstCorrecte = true },
                new OptionReponse { QuestionId = question.Id, Texte = "Le pattern Singleton statique global", EstCorrecte = false },
                new OptionReponse { QuestionId = question.Id, Texte = "Les variables globales", EstCorrecte = false },
                new OptionReponse { QuestionId = question.Id, Texte = "Aucun, ASP.NET Core ne fournit pas ce mécanisme", EstCorrecte = false }
            );
            await context.SaveChangesAsync();
        }
    }
}
