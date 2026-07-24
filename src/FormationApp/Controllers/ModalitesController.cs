using FormationApp.Data;
using FormationApp.Models;
using FormationApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Controllers;

[Authorize(Roles = Roles.Admin)]
public class ModalitesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ModalitesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Create(int moduleId)
    {
        var module = await _context.Modules.Include(m => m.Formation).FirstOrDefaultAsync(m => m.Id == moduleId);
        if (module is null) return NotFound();

        var prochainOrdre = await _context.Modalites.Where(m => m.ModuleId == moduleId).CountAsync() + 1;

        ViewBag.Module = module;
        return View(new Modalite { ModuleId = moduleId, Ordre = prochainOrdre });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Modalite modalite)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Module = await _context.Modules.Include(m => m.Formation).FirstOrDefaultAsync(m => m.Id == modalite.ModuleId);
            return View(modalite);
        }

        _context.Add(modalite);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Modalité ajoutée avec succès.";

        if (modalite.Type == TypeModalite.Examen)
            return RedirectToAction(nameof(Questions), new { modaliteId = modalite.Id });

        var module = await _context.Modules.FindAsync(modalite.ModuleId);
        return RedirectToAction("Index", "Modules", new { formationId = module!.FormationId });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var modalite = await _context.Modalites.Include(m => m.Module).ThenInclude(m => m!.Formation).FirstOrDefaultAsync(m => m.Id == id);
        if (modalite is null) return NotFound();
        ViewBag.Module = modalite.Module;
        return View(modalite);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Modalite modalite)
    {
        if (id != modalite.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Module = await _context.Modules.Include(m => m.Formation).FirstOrDefaultAsync(m => m.Id == modalite.ModuleId);
            return View(modalite);
        }

        try
        {
            _context.Update(modalite);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Modalité mise à jour.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Modalites.AnyAsync(m => m.Id == id)) return NotFound();
            throw;
        }

        var module = await _context.Modules.FindAsync(modalite.ModuleId);
        return RedirectToAction("Index", "Modules", new { formationId = module!.FormationId });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var modalite = await _context.Modalites.Include(m => m.Module).ThenInclude(m => m!.Formation).FirstOrDefaultAsync(m => m.Id == id);
        if (modalite is null) return NotFound();
        return View(modalite);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var modalite = await _context.Modalites.Include(m => m.Module).FirstOrDefaultAsync(m => m.Id == id);
        if (modalite is null) return NotFound();
        var formationId = modalite.Module!.FormationId;

        _context.Modalites.Remove(modalite);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Modalité supprimée.";
        return RedirectToAction("Index", "Modules", new { formationId });
    }

    // --- Gestion des questions pour une modalité de type Examen -------------------

    public async Task<IActionResult> Questions(int modaliteId)
    {
        var modalite = await _context.Modalites
            .Include(m => m.Module).ThenInclude(m => m!.Formation)
            .Include(m => m.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(m => m.Id == modaliteId);

        if (modalite is null) return NotFound();
        if (modalite.Type != TypeModalite.Examen) return BadRequest("Cette modalité n'est pas un examen/quiz.");

        ViewBag.NouvelleQuestion = new QuestionFormViewModel { ModaliteId = modaliteId };
        return View(modalite);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AjouterQuestion(QuestionFormViewModel form)
    {
        var modalite = await _context.Modalites.FindAsync(form.ModaliteId);
        if (modalite is null) return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Veuillez compléter correctement le formulaire de question.";
            return RedirectToAction(nameof(Questions), new { modaliteId = form.ModaliteId });
        }

        var ordre = await _context.Questions.Where(q => q.ModaliteId == form.ModaliteId).CountAsync() + 1;
        var question = new Question { ModaliteId = form.ModaliteId, Enonce = form.Enonce, Ordre = ordre };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        var options = new List<(string? texte, int index)>
        {
            (form.Option1, 1), (form.Option2, 2), (form.Option3, 3), (form.Option4, 4)
        };

        foreach (var (texte, index) in options)
        {
            if (string.IsNullOrWhiteSpace(texte)) continue;
            _context.OptionsReponse.Add(new OptionReponse
            {
                QuestionId = question.Id,
                Texte = texte,
                EstCorrecte = index == form.OptionCorrecte
            });
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Question ajoutée.";
        return RedirectToAction(nameof(Questions), new { modaliteId = form.ModaliteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SupprimerQuestion(int id, int modaliteId)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question is not null)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Question supprimée.";
        }
        return RedirectToAction(nameof(Questions), new { modaliteId });
    }
}
