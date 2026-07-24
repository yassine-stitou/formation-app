namespace FormationApp.Services;

/// <summary>
/// Configuration du formateur IA (avatar).
/// Renseignez ces valeurs dans appsettings.json, appsettings.Development.json,
/// des variables d'environnement (Avatar__Provider, Avatar__ApiKey, ...) ou via
/// `dotnet user-secrets` en développement. Ne commitez jamais une vraie clé API.
/// </summary>
public class AvatarOptions
{
    public const string SectionName = "Avatar";

    /// <summary>Fournisseur actif : "HeyGen", "Anam" ou "None".</summary>
    public string Provider { get; set; } = "None";

    /// <summary>Clé API secrète du fournisseur (jamais exposée au navigateur).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Identifiant de l'avatar / persona chez le fournisseur.</summary>
    public string AvatarId { get; set; } = string.Empty;

    /// <summary>Identifiant de la voix à utiliser (Anam.ai).</summary>
    public string VoiceId { get; set; } = string.Empty;

    /// <summary>
    /// (HeyGen LiveAvatar) Identifiant du "context" (anciennement "knowledge base") associé à l'avatar,
    /// créé sur app.liveavatar.com. Laisser vide si vous n'utilisez pas de contexte prédéfini.
    /// </summary>
    public string KnowledgeId { get; set; } = string.Empty;

    /// <summary>
    /// (HeyGen LiveAvatar) Mode sandbox : ne consomme aucun crédit, idéal pour le développement
    /// et les démonstrations. Passez à false pour une session en conditions réelles.
    /// ⚠️ Constaté en pratique : certains avatars/contexts échouent au démarrage de session
    /// en sandbox (erreur 400 sur /v1/sessions/start côté HeyGen) alors même que la création
    /// de l'embed réussit. Si "Démarrer le formateur IA" échoue avec Sandbox=true, essayez
    /// Sandbox=false (attention : consomme alors de vrais crédits).
    /// </summary>
    public bool Sandbox { get; set; } = true;

    /// <summary>(Anam) Identifiant du LLM à utiliser (laisser vide pour le LLM par défaut d'Anam).</summary>
    public string LlmId { get; set; } = string.Empty;

    /// <summary>Nom affiché du formateur IA.</summary>
    public string PersonaName { get; set; } = "Assistant Formateur";

    /// <summary>
    /// Gabarit du prompt système. {0} est remplacé par le contexte du module en cours
    /// (titre, description, contenu pédagogique) afin que l'avatar puisse répondre
    /// aux questions des apprenants sur ce module précis.
    /// </summary>
    public string SystemPromptTemplate { get; set; } =
        "Tu es un formateur pédagogue et bienveillant. Réponds oralement, de façon claire et concise, " +
        "aux questions des apprenants sur le module suivant :\n\n{0}\n\n" +
        "Si une question sort du cadre du module, recentre poliment l'apprenant sur le sujet de la formation.";
}
