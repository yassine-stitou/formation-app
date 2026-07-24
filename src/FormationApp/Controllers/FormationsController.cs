using FormationApp.Data;
using FormationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Controllers;

public class FormationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public FormationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Catalogue public : uniquement les formations publiées
    [AllowAnonymous]
    public async Task<IActionResult> Index(int? categorieId, string? recherche)
    {
        var estAdmin = User.IsInRole(Roles.Admin);

        var query = _context.Formations
            .Include(f => f.Categorie)
            .Include(f => f.Formateur)
            .Include(f => f.Inscriptions)
            .AsQueryable();

        if (!estAdmin)
            query = query.Where(f => f.Statut == StatutFormation.Publiee);

        if (categorieId.HasValue)
            query = query.Where(f => f.CategorieId == categorieId);

        if (!string.IsNullOrWhiteSpace(recherche))
            query = query.Where(f => f.Titre.Contains(recherche) || (f.Description != null && f.Description.Contains(recherche)));

        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Nom).ToListAsync();
        ViewBag.CategorieId = categorieId;
        ViewBag.Recherche = recherche;

        return View(await query.OrderByDescending(f => f.DateCreation).ToListAsync());
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var formation = await _context.Formations
            .Include(f => f.Categorie)
            .Include(f => f.Formateur)
            .Include(f => f.Modules.OrderBy(m => m.Ordre)).ThenInclude(m => m.Modalites.OrderBy(mo => mo.Ordre))
            .Include(f => f.Inscriptions)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (formation is null) return NotFound();
        if (formation.Statut != StatutFormation.Publiee && !User.IsInRole(Roles.Admin)) return NotFound();

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _context.Users.Where(u => u.UserName == User.Identity.Name).Select(u => u.Id).FirstOrDefault();
            ViewBag.EstInscrit = await _context.Inscriptions.AnyAsync(i => i.FormationId == id && i.ApplicationUserId == userId);
        }

        return View(formation);
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create()
    {
        await ChargerListesAsync();
        return View(new Formation { DateDebut = DateTime.Today, DateFin = DateTime.Today.AddMonths(1) });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Formation formation)
    {
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(formation.CategorieId, formation.FormateurId);
            return View(formation);
        }

        _context.Add(formation);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Formation créée avec succès. Ajoutez maintenant ses modules.";
        return RedirectToAction("Index", "Modules", new { formationId = formation.Id });
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var formation = await _context.Formations.FindAsync(id);
        if (formation is null) return NotFound();
        await ChargerListesAsync(formation.CategorieId, formation.FormateurId);
        return View(formation);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Formation formation)
    {
        if (id != formation.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(formation.CategorieId, formation.FormateurId);
            return View(formation);
        }

        try
        {
            _context.Update(formation);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Formation mise à jour.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Formations.AnyAsync(f => f.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var formation = await _context.Formations
            .Include(f => f.Categorie)
            .Include(f => f.Formateur)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (formation is null) return NotFound();
        return View(formation);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = Roles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var formation = await _context.Formations.FindAsync(id);
        if (formation is null) return NotFound();

        _context.Formations.Remove(formation);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Formation supprimée.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerStatut(int id, StatutFormation statut)
    {
        var formation = await _context.Formations.FindAsync(id);
        if (formation is null) return NotFound();

        formation.Statut = statut;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Statut de la formation mis à jour : {statut}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ChargerListesAsync(int? categorieId = null, int? formateurId = null)
    {
        ViewBag.CategorieId = new SelectList(await _context.Categories.OrderBy(c => c.Nom).ToListAsync(), "Id", "Nom", categorieId);
        ViewBag.FormateurId = new SelectList(await _context.Formateurs.OrderBy(f => f.Nom).ToListAsync(), "Id", "NomComplet", formateurId);
    }
}
