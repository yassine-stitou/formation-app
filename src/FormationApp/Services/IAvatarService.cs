namespace FormationApp.Services;

public record AvatarSessionResult(
    string Provider,
    bool Configured,
    string? Token,
    string? EmbedUrl,
    string? AvatarId,
    string? VoiceId,
    string? KnowledgeId,
    string? PersonaName,
    string? ErrorMessage
);

public interface IAvatarService
{
    /// <summary>
    /// Génère un jeton de session côté serveur pour le fournisseur d'avatar IA configuré,
    /// à partir du contexte pédagogique du module (utilisé comme "connaissance" de l'avatar).
    /// La clé API du fournisseur n'est jamais transmise au navigateur.
    /// </summary>
    Task<AvatarSessionResult> CreerSessionAsync(string contextePedagogique, CancellationToken ct = default);
}
