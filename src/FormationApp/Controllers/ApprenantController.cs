using FormationApp.Data;
using FormationApp.Models;
using FormationApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Controllers;

[Authorize]
public class ApprenantController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApprenantController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> MesFormations()
    {
        var userId = _userManager.GetUserId(User)!;
        var inscriptions = await _context.Inscriptions
            .Include(i => i.Formation).ThenInclude(f => f!.Categorie)
            .Include(i => i.Formation).ThenInclude(f => f!.Formateur)
            .Where(i => i.ApplicationUserId == userId)
            .OrderByDescending(i => i.DateInscription)
            .ToListAsync();

        return View(inscriptions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscrire(int formationId)
    {
        var userId = _userManager.GetUserId(User)!;

        var dejaInscrit = await _context.Inscriptions.AnyAsync(i => i.FormationId == formationId && i.ApplicationUserId == userId);
        if (dejaInscrit)
        {
            TempData["Error"] = "Vous êtes déjà inscrit à cette formation.";
            return RedirectToAction("Details", "Formations", new { id = formationId });
        }

        var formation = await _context.Formations.Include(f => f.Inscriptions).FirstOrDefaultAsync(f => f.Id == formationId);
        if (formation is null) return NotFound();

        if (formation.NombreInscrits >= formation.PlacesDisponibles)
        {
            TempData["Error"] = "Cette formation est complète.";
            return RedirectToAction("Details", "Formations", new { id = formationId });
        }

        _context.Inscriptions.Add(new Inscription
        {
            ApplicationUserId = userId,
            FormationId = formationId,
            Statut = StatutInscription.Active
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Inscription réussie ! Vous pouvez commencer la formation.";
        return RedirectToAction(nameof(Player), new { formationId });
    }

    // Page plein écran dédiée au formateur IA, dans l'esprit d'un appel vidéo (Zoom-like)
    public async Task<IActionResult> AvatarSession(int formationId, int moduleId)
    {
        var userId = _userManager.GetUserId(User)!;

        var estInscrit = await _context.Inscriptions.AnyAsync(i => i.FormationId == formationId && i.ApplicationUserId == userId);
        var estAdmin = User.IsInRole(Roles.Admin);
        if (!estInscrit && !estAdmin) return Forbid();

        var module = await _context.Modules
            .Include(m => m.Formation)
            .Include(m => m.Modalites)
            .FirstOrDefaultAsync(m => m.Id == moduleId && m.FormationId == formationId);

        if (module is null) return NotFound();

        return View(module);
    }

    // Page de lecture d'un module : contenu pédagogique du module
    public async Task<IActionResult> Player(int formationId, int? moduleId = null)
    {
        var userId = _userManager.GetUserId(User)!;

        var estInscrit = await _context.Inscriptions.AnyAsync(i => i.FormationId == formationId && i.ApplicationUserId == userId);
        var estAdmin = User.IsInRole(Roles.Admin);
        if (!estInscrit && !estAdmin) return Forbid();

        var formation = await _context.Formations
            .Include(f => f.Modules.OrderBy(m => m.Ordre)).ThenInclude(m => m.Modalites.OrderBy(mo => mo.Ordre))
            .FirstOrDefaultAsync(f => f.Id == formationId);
        if (formation is null) return NotFound();

        var module = moduleId.HasValue
            ? formation.Modules.FirstOrDefault(m => m.Id == moduleId)
            : formation.Modules.OrderBy(m => m.Ordre).FirstOrDefault();

        if (module is null) return NotFound("Cette formation ne contient pas encore de module.");

        var vm = new ModulePlayerViewModel
        {
            Formation = formation,
            Module = module,
            TousLesModules = formation.Modules.OrderBy(m => m.Ordre).ToList()
        };

        return View(vm);
    }

    public async Task<IActionResult> PasserExamen(int modaliteId)
    {
        var modalite = await _context.Modalites
            .Include(m => m.Module).ThenInclude(m => m!.Formation)
            .Include(m => m.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(m => m.Id == modaliteId);

        if (modalite is null || modalite.Type != TypeModalite.Examen) return NotFound();

        return View(modalite);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoumettreExamen(int modaliteId, Dictionary<int, int> reponses)
    {
        var userId = _userManager.GetUserId(User)!;

        var modalite = await _context.Modalites
            .Include(m => m.Module).ThenInclude(m => m!.Formation)
            .Include(m => m.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(m => m.Id == modaliteId);

        if (modalite is null || modalite.Type != TypeModalite.Examen) return NotFound();

        var totalQuestions = modalite.Questions.Count;
        var bonnesReponses = 0;

        foreach (var question in modalite.Questions)
        {
            if (reponses.TryGetValue(question.Id, out var optionChoisieId))
            {
                var optionCorrecte = question.Options.FirstOrDefault(o => o.EstCorrecte);
                if (optionCorrecte is not null && optionCorrecte.Id == optionChoisieId)
                    bonnesReponses++;
            }
        }

        var score = totalQuestions == 0 ? 0 : Math.Round(bonnesReponses * 100.0 / totalQuestions, 1);
        var reussi = score >= modalite.ScoreMinReussite;

        _context.ResultatsExamen.Add(new ResultatExamen
        {
            ApplicationUserId = userId,
            ModaliteId = modaliteId,
            Score = score,
            Reussi = reussi
        });

        // Mise à jour simple de la progression de l'apprenant sur la formation
        var formationId = modalite.Module!.FormationId;
        var inscription = await _context.Inscriptions.FirstOrDefaultAsync(i => i.FormationId == formationId && i.ApplicationUserId == userId);
        if (inscription is not null && reussi)
        {
            var nbModalites = await _context.Modalites.CountAsync(m => m.Module!.FormationId == formationId);
            var increment = nbModalites == 0 ? 0 : 100 / nbModalites;
            inscription.Progression = Math.Min(100, inscription.Progression + increment);
            if (inscription.Progression >= 100)
            {
                inscription.Statut = StatutInscription.Terminee;
                inscription.DateCompletion = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        var resultat = new QuizResultViewModel
        {
            Modalite = modalite,
            Score = score,
            Reussi = reussi,
            NombreQuestions = totalQuestions,
            BonnesReponses = bonnesReponses
        };

        return View("ResultatExamen", resultat);
    }
}
