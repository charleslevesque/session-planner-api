using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class AiSuggestionService : IAiSuggestionService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _model;
    private readonly ILogger<AiSuggestionService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public AiSuggestionService(AppDbContext db, HttpClient httpClient, IConfiguration config, ILogger<AiSuggestionService> logger)
    {
        _db = db;
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = config["OpenAI:ApiKey"];
        _model = config["OpenAI:Model"] ?? "gpt-4o-mini";
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<AiSuggestionsResult> SuggestItemsAsync(int sessionId, int courseId, string? itemType = null)
    {
        if (!IsConfigured)
            return new AiSuggestionsResult([], "AI suggestions are not configured.");

        var context = await BuildContextAsync(sessionId, courseId);
        var prompt = BuildPrompt(context, itemType);

        try
        {
            var response = await CallOpenAiAsync(prompt);
            return ParseResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI call failed for session {SessionId}, course {CourseId}", sessionId, courseId);
            return new AiSuggestionsResult([], "Impossible de générer des suggestions pour le moment.");
        }
    }

    private async Task<CourseContext> BuildContextAsync(int sessionId, int courseId)
    {
        var session = await _db.Sessions.FindAsync(sessionId);
        var course = await _db.Courses.FindAsync(courseId);

        // Approved history for this course (all sessions)
        var history = await _db.TeachingNeeds
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Items).ThenInclude(i => i.SoftwareVersion)
            .Include(n => n.Items).ThenInclude(i => i.OS)
            .Include(n => n.Session)
            .Where(n => n.CourseId == courseId && n.Status == Core.Enums.NeedStatus.Approved)
            .OrderByDescending(n => n.ReviewedAt)
            .Take(10)
            .ToListAsync();

        // Software catalog (names + versions)
        var catalog = await _db.Softwares
            .Include(s => s.SoftwareVersions).ThenInclude(v => v.OS)
            .OrderBy(s => s.Name)
            .ToListAsync();

        // OS list
        var osList = await _db.OperatingSystems.OrderBy(o => o.Name).ToListAsync();

        // Already submitted needs for this session+course (to avoid duplicates)
        var existingNeeds = await _db.TeachingNeeds
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Items).ThenInclude(i => i.SoftwareVersion)
            .Where(n => n.SessionId == sessionId && n.CourseId == courseId
                        && n.Status != Core.Enums.NeedStatus.Draft)
            .ToListAsync();

        return new CourseContext(
            SessionTitle: session?.Title ?? "",
            CourseCode: course?.Code ?? "",
            CourseName: course?.Name ?? "",
            History: history.Select(n => new HistoryEntry(
                Session: n.Session?.Title ?? "",
                Items: n.Items.Select(i => new HistoryItem(
                    Type: i.ItemType,
                    Software: i.Software?.Name,
                    Version: i.SoftwareVersion?.VersionNumber,
                    Os: i.OS?.Name,
                    Notes: i.Notes
                )).ToList()
            )).ToList(),
            CatalogSoftware: catalog.Select(s => new CatalogEntry(
                Name: s.Name,
                InstallCommand: s.InstallCommand,
                Versions: s.SoftwareVersions.Select(v => new CatalogVersion(
                    Version: v.VersionNumber,
                    Os: v.OS?.Name ?? ""
                )).ToList()
            )).ToList(),
            OperatingSystems: osList.Select(o => o.Name).ToList(),
            AlreadyRequested: existingNeeds.SelectMany(n => n.Items).Select(i =>
                i.Software?.Name ?? i.Description ?? i.ItemType
            ).Distinct().ToList()
        );
    }

    private static string BuildPrompt(CourseContext ctx, string? itemType)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Tu es un assistant pour un système de planification de besoins technologiques universitaires.");
        sb.AppendLine("Un professeur prépare une demande de besoins pour son cours. Ton rôle est de suggérer les logiciels et ressources qu'il pourrait avoir besoin, basé sur l'historique du cours et le catalogue disponible.");
        sb.AppendLine();
        sb.AppendLine($"Session: {ctx.SessionTitle}");
        sb.AppendLine($"Cours: {ctx.CourseCode} {ctx.CourseName}");
        sb.AppendLine();

        if (ctx.History.Count > 0)
        {
            sb.AppendLine("=== HISTORIQUE DES DEMANDES APPROUVÉES POUR CE COURS ===");
            foreach (var h in ctx.History.Take(5))
            {
                sb.AppendLine($"Session {h.Session}:");
                foreach (var item in h.Items)
                    sb.AppendLine($"  - [{item.Type}] {item.Software ?? item.Notes ?? "N/A"} {item.Version ?? ""} ({item.Os ?? ""})");
            }
            sb.AppendLine();
        }

        if (ctx.AlreadyRequested.Count > 0)
        {
            sb.AppendLine("=== DÉJÀ DEMANDÉ POUR CETTE SESSION (ne pas re-suggérer) ===");
            foreach (var r in ctx.AlreadyRequested)
                sb.AppendLine($"  - {r}");
            sb.AppendLine();
        }

        sb.AppendLine("=== CATALOGUE LOGICIEL DISPONIBLE (extraits) ===");
        foreach (var s in ctx.CatalogSoftware.Take(30))
        {
            var versions = string.Join(", ", s.Versions.Select(v => $"{v.Version} ({v.Os})").Take(3));
            sb.AppendLine($"  - {s.Name}: {versions}{(s.InstallCommand != null ? $" | cmd: {s.InstallCommand}" : "")}");
        }
        sb.AppendLine();

        sb.AppendLine($"Systèmes d'exploitation disponibles: {string.Join(", ", ctx.OperatingSystems)}");
        sb.AppendLine();

        if (itemType != null)
            sb.AppendLine($"Le professeur travaille sur un item de type: {itemType}");

        sb.AppendLine();
        sb.AppendLine("Réponds en JSON strict avec ce format (pas de markdown, juste le JSON):");
        sb.AppendLine("""
{
  "summary": "Résumé court de ta recommandation en français (1-2 phrases)",
  "suggestions": [
    {
      "itemType": "software",
      "label": "Nom affiché de la suggestion",
      "softwareName": "Nom exact du logiciel (du catalogue si possible)",
      "version": "Version suggérée",
      "os": "Nom de l'OS",
      "installCommand": "Commande d'installation si connue",
      "notes": "Notes optionnelles",
      "reason": "Pourquoi cette suggestion (en français, 1 phrase)"
    }
  ]
}
""");
        sb.AppendLine("Donne entre 2 et 6 suggestions pertinentes. Priorise les logiciels de l'historique du cours. Utilise les noms exacts du catalogue quand possible. Ne suggère PAS ce qui est déjà demandé pour cette session.");

        return sb.ToString();
    }

    private async Task<string> CallOpenAiAsync(string prompt)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "Tu es un assistant technique universitaire. Réponds uniquement en JSON valide." },
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = 1500
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOpts), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";
    }

    private static AiSuggestionsResult ParseResponse(string json)
    {
        // Strip markdown fences if the model wraps its reply
        var trimmed = json.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline > 0) trimmed = trimmed[(firstNewline + 1)..];
            if (trimmed.EndsWith("```")) trimmed = trimmed[..^3].TrimEnd();
        }

        using var doc = JsonDocument.Parse(trimmed);
        var root = doc.RootElement;

        var summary = root.TryGetProperty("summary", out var s) ? s.GetString() : null;

        var suggestions = new List<AiSuggestedItem>();
        if (root.TryGetProperty("suggestions", out var arr))
        {
            foreach (var item in arr.EnumerateArray())
            {
                suggestions.Add(new AiSuggestedItem(
                    ItemType: item.TryGetProperty("itemType", out var it) ? it.GetString() ?? "software" : "software",
                    Label: item.TryGetProperty("label", out var l) ? l.GetString() ?? "" : "",
                    SoftwareName: item.TryGetProperty("softwareName", out var sn) ? sn.GetString() : null,
                    Version: item.TryGetProperty("version", out var v) ? v.GetString() : null,
                    Os: item.TryGetProperty("os", out var o) ? o.GetString() : null,
                    InstallCommand: item.TryGetProperty("installCommand", out var ic) ? ic.GetString() : null,
                    Notes: item.TryGetProperty("notes", out var n) ? n.GetString() : null,
                    Reason: item.TryGetProperty("reason", out var r) ? r.GetString() ?? "" : ""
                ));
            }
        }

        return new AiSuggestionsResult(suggestions, summary);
    }

    private record CourseContext(
        string SessionTitle, string CourseCode, string CourseName,
        List<HistoryEntry> History,
        List<CatalogEntry> CatalogSoftware,
        List<string> OperatingSystems,
        List<string> AlreadyRequested);

    private record HistoryEntry(string Session, List<HistoryItem> Items);
    private record HistoryItem(string Type, string? Software, string? Version, string? Os, string? Notes);
    private record CatalogEntry(string Name, string? InstallCommand, List<CatalogVersion> Versions);
    private record CatalogVersion(string Version, string Os);
}
