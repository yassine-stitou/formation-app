using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace FormationApp.Services;

public class AvatarService : IAvatarService
{
    private readonly HttpClient _httpClient;
    private readonly AvatarOptions _options;
    private readonly ILogger<AvatarService> _logger;

    public AvatarService(HttpClient httpClient, IOptions<AvatarOptions> options, ILogger<AvatarService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AvatarSessionResult> CreerSessionAsync(string contextePedagogique, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) ||
            string.Equals(_options.Provider, "None", StringComparison.OrdinalIgnoreCase))
        {
            return NonConfigure("Aucun fournisseur d'avatar IA n'est configuré. Renseignez la section \"Avatar\" dans appsettings.json.");
        }

        try
        {
            return _options.Provider.ToLowerInvariant() switch
            {
                "heygen" => await CreerSessionHeyGenAsync(ct),
                "anam" => await CreerSessionAnamAsync(contextePedagogique, ct),
                _ => NonConfigure($"Fournisseur inconnu : {_options.Provider}. Valeurs supportées : HeyGen, Anam.")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la session avatar IA ({Provider})", _options.Provider);
            return NonConfigure("Impossible de contacter le service d'avatar IA pour le moment.");
        }
    }

    private AvatarSessionResult NonConfigure(string message) => new(
        Provider: _options.Provider, Configured: false, Token: null, EmbedUrl: null,
        AvatarId: null, VoiceId: null, KnowledgeId: null, PersonaName: _options.PersonaName, ErrorMessage: message);

    // --- HeyGen (LiveAvatar) --------------------------------------------------
    // HeyGen a remplacé son ancienne "Streaming Avatar API" (/v1/streaming.*, sunset en 2026)
    // par LiveAvatar. Doc : POST https://api.liveavatar.com/v2/embeddings (header X-API-KEY).
    // La réponse contient directement une URL d'iframe prête à l'emploi — plus besoin de SDK
    // côté client ni de jeton de streaming séparé.
    // Référence : https://docs.liveavatar.com/
    //
    // NOTE : contrairement à Anam.ai, cette méthode utilise un "context" (KnowledgeId) statique
    // et pré-créé dans le tableau de bord LiveAvatar plutôt que le contexte pédagogique du
    // module en cours. LiveAvatar expose une API pour créer/mettre à jour des contexts
    // dynamiquement (POST/PATCH https://api.liveavatar.com/v1/contexts), ce qui permettrait
    // d'injecter le contenu de chaque module à la volée — non implémenté ici car le schéma
    // exact de cette API évolue ; consultez docs.liveavatar.com avant de l'automatiser.
    private async Task<AvatarSessionResult> CreerSessionHeyGenAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.AvatarId))
        {
            return NonConfigure("Avatar__AvatarId n'est pas renseigné (identifiant de l'avatar LiveAvatar).");
        }
        if (string.IsNullOrWhiteSpace(_options.KnowledgeId))
        {
            return NonConfigure("Avatar__KnowledgeId n'est pas renseigné. LiveAvatar exige désormais un context_id : " +
                "créez-en un sur https://app.liveavatar.com/contexts puis collez son identifiant dans la configuration.");
        }

        var payload = new Dictionary<string, object?>
        {
            ["avatar_id"] = _options.AvatarId,
            ["context_id"] = _options.KnowledgeId,
            ["is_sandbox"] = _options.Sandbox
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.liveavatar.com/v2/embeddings");
        request.Headers.Add("X-API-KEY", _options.ApiKey);
        request.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("HeyGen (LiveAvatar) a répondu {Status} : {Body}", response.StatusCode, body);
            return NonConfigure("Le service HeyGen a refusé la demande de session (vérifiez la clé API, l'avatar_id et le quota).");
        }

        using var doc = JsonDocument.Parse(body);
        var embedUrl = doc.RootElement.GetProperty("data").GetProperty("url").GetString();

        return new AvatarSessionResult(
            Provider: "HeyGen",
            Configured: true,
            Token: null,
            EmbedUrl: embedUrl,
            AvatarId: _options.AvatarId,
            VoiceId: _options.VoiceId,
            KnowledgeId: _options.KnowledgeId,
            PersonaName: _options.PersonaName,
            ErrorMessage: null);
    }

    // --- Anam.ai ------------------------------------------------------------
    // Doc : POST https://api.anam.ai/v1/auth/session-token (Authorization: Bearer <clé>)
    // Corps : { personaConfig: { name, avatarId, voiceId, llmId, systemPrompt } }
    // Le sessionToken renvoyé est utilisé côté client par @anam-ai/js-sdk (createClient).
    private async Task<AvatarSessionResult> CreerSessionAnamAsync(string contextePedagogique, CancellationToken ct)
    {
        var systemPrompt = string.Format(_options.SystemPromptTemplate, contextePedagogique);

        var personaConfig = new Dictionary<string, object?>
        {
            ["name"] = _options.PersonaName,
            ["avatarId"] = _options.AvatarId,
            ["voiceId"] = _options.VoiceId,
            ["systemPrompt"] = systemPrompt
        };
        if (!string.IsNullOrWhiteSpace(_options.LlmId))
        {
            personaConfig["llmId"] = _options.LlmId;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anam.ai/v1/auth/session-token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = JsonContent.Create(new { personaConfig });

        using var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Anam.ai a répondu {Status} : {Body}", response.StatusCode, body);
            return NonConfigure("Le service Anam.ai a refusé la demande de session (vérifiez la clé API et le quota).");
        }

        using var doc = JsonDocument.Parse(body);
        var sessionToken = doc.RootElement.GetProperty("sessionToken").GetString();

        return new AvatarSessionResult(
            Provider: "Anam",
            Configured: true,
            Token: sessionToken,
            EmbedUrl: null,
            AvatarId: _options.AvatarId,
            VoiceId: _options.VoiceId,
            KnowledgeId: null,
            PersonaName: _options.PersonaName,
            ErrorMessage: null);
    }
}
