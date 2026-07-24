using System.ComponentModel.DataAnnotations;

namespace FormationApp.Models;

public class Formateur
{
    public int Id { get; set; }

    // Lien optionnel vers un compte de connexion (rôle Formateur)
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est obligatoire.")]
    [StringLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    [Display(Name = "Téléphone")]
    public string? Telephone { get; set; }

    [StringLength(150)]
    [Display(Name = "Spécialité")]
    public string? Specialite { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    [StringLength(300)]
    [Display(Name = "Photo (URL)")]
    public string? PhotoUrl { get; set; }

    public ICollection<Formation> Formations { get; set; } = new List<Formation>();

    public string NomComplet => $"{Prenom} {Nom}".Trim();
}
