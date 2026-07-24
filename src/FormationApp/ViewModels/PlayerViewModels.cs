using FormationApp.Models;

namespace FormationApp.ViewModels;

public class ModulePlayerViewModel
{
    public Formation Formation { get; set; } = null!;
    public Module Module { get; set; } = null!;
    public List<Module> TousLesModules { get; set; } = new();
    public string AvatarProvider { get; set; } = "none";
}

public class QuizAnswerSubmission
{
    public int QuestionId { get; set; }
    public int OptionId { get; set; }
}

public class QuizResultViewModel
{
    public Modalite Modalite { get; set; } = null!;
    public double Score { get; set; }
    public bool Reussi { get; set; }
    public int NombreQuestions { get; set; }
    public int BonnesReponses { get; set; }
}

/// <summary>Formulaire simplifié d'ajout de question (utilisé côté admin) : un énoncé et 4 options,
/// dont une seule est marquée comme correcte via un bouton radio.</summary>
public class QuestionFormViewModel
{
    public int ModaliteId { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "L'énoncé est obligatoire.")]
    [System.ComponentModel.DataAnnotations.Display(Name = "Énoncé de la question")]
    public string Enonce { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "L'option 1 est obligatoire.")]
    [System.ComponentModel.DataAnnotations.Display(Name = "Option 1")]
    public string Option1 { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "L'option 2 est obligatoire.")]
    [System.ComponentModel.DataAnnotations.Display(Name = "Option 2")]
    public string Option2 { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Display(Name = "Option 3 (facultatif)")]
    public string? Option3 { get; set; }

    [System.ComponentModel.DataAnnotations.Display(Name = "Option 4 (facultatif)")]
    public string? Option4 { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Indiquez la bonne réponse.")]
    [System.ComponentModel.DataAnnotations.Display(Name = "Option correcte")]
    public int OptionCorrecte { get; set; } = 1;
}
