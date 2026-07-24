using FormationApp.Data;
using FormationApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormationApp.Controllers;

[Authorize]
[ApiController]
[Route("api/avatar")]
public class AvatarController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAvatarService _avatarService;

    public AvatarController(ApplicationDbContext context, IAvatarService avatarService)
    {
        _context = context;
        _avatarService = avatarService;
    }

    /// <summary>
    /// Crée une session pour le formateur IA (avatar), à partir du contexte pédagogique
    /// du module consulté. Le jeton renvoyé est de courte durée et n'expose jamais la clé API.
    /// </summary>
    [HttpPost("session/{moduleId:int}")]
    public async Task<IActionResult> CreerSession(int moduleId)
    {
        var module = await _context.Modules
            .Include(m => m.Formation)
            .Include(m => m.Modalites)
            .FirstOrDefaultAsync(m => m.Id == moduleId);

        if (module is null) return NotFound();

        var contenuModalites = string.Join("\n", module.Modalites
            .OrderBy(m => m.Ordre)
            .Select(m => $"- ({m.Type}) {m.Titre} : {m.Contenu}"));

        var contexte = $"Formation : {module.Formation?.Titre}\n" +
                        $"Module : {module.Titre}\n" +
                        $"Description : {module.Description}\n" +
                        $"{module.ContexteIA}\n\n" +
                        $"Contenu du module :\n{contenuModalites}";

        var session = await _avatarService.CreerSessionAsync(contexte);
        return Ok(session);
    }
}
