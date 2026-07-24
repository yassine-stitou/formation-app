using Microsoft.AspNetCore.Identity;

namespace FormationApp.Models;

public class ApplicationUser : IdentityUser
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public DateTime DateInscription { get; set; } = DateTime.UtcNow;

    public string NomComplet => $"{Prenom} {Nom}".Trim();

    public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
}
