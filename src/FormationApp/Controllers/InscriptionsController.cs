using FormationApp.Data;
using FormationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Controllers;

[Authorize(Roles = Roles.Admin)]
public class InscriptionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public InscriptionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? formationId, StatutInscription? statut)
    {
        var query = _context.Inscriptions
            .Include(i => i.ApplicationUser)
            .Include(i => i.Formation)
            .AsQueryable();

        if (formationId.HasValue) query = query.Where(i => i.FormationId == formationId);
        if (statut.HasValue) query = query.Where(i => i.Statut == statut);

        ViewBag.Formations = await _context.Formations.OrderBy(f => f.Titre).ToListAsync();
        ViewBag.FormationId = formationId;
        ViewBag.Statut = statut;

        return View(await query.OrderByDescending(i => i.DateInscription).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerStatut(int id, StatutInscription statut)
    {
        var inscription = await _context.Inscriptions.FindAsync(id);
        if (inscription is null) return NotFound();

        inscription.Statut = statut;
        if (statut == StatutInscription.Terminee) inscription.DateCompletion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Statut de l'inscription mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var inscription = await _context.Inscriptions.FindAsync(id);
        if (inscription is null) return NotFound();

        _context.Inscriptions.Remove(inscription);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Inscription supprimée.";
        return RedirectToAction(nameof(Index));
    }
}
