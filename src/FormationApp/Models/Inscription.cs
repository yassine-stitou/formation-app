using System.ComponentModel.DataAnnotations;

namespace FormationApp.Models;

public class Inscription
{
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser? ApplicationUser { get; set; }

    [Required]
    public int FormationId { get; set; }
    public Formation? Formation { get; set; }

    public DateTime DateInscription { get; set; } = DateTime.UtcNow;

    public StatutInscription Statut { get; set; } = StatutInscription.EnAttente;

    [Range(0, 100)]
    public int Progression { get; set; } = 0;

    public DateTime? DateCompletion { get; set; }
}

public class ResultatExamen
{
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser? ApplicationUser { get; set; }

    [Required]
    public int ModaliteId { get; set; }
    public Modalite? Modalite { get; set; }

    [Range(0, 100)]
    public double Score { get; set; }

    public bool Reussi { get; set; }

    public DateTime DatePassage { get; set; } = DateTime.UtcNow;
}
