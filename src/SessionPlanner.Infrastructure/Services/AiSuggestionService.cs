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

    public async Task<AiReviewAnalysis> AnalyzeNeedForReviewAsync(int sessionId, int needId)
    {
        if (!IsConfigured)
            return new AiReviewAnalysis("AI non configurée.", [], null, null, []);

        try
        {
            var context = await BuildReviewContextAsync(sessionId, needId);
            var prompt = BuildReviewPrompt(context);
            var response = await CallOpenAiAsync(prompt);
            return ParseReviewResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI review analysis failed for need {NeedId}", needId);
            return new AiReviewAnalysis("Impossible d'analyser cette demande pour le moment.", [], null, null, []);
        }
    }

    private async Task<ReviewContext> BuildReviewContextAsync(int sessionId, int needId)
    {
        var need = await _db.TeachingNeeds
            .Include(n => n.Personnel)
            .Include(n => n.Course)
            .Include(n => n.Session)
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Items).ThenInclude(i => i.SoftwareVersion)
            .Include(n => n.Items).ThenInclude(i => i.OS)
            .FirstOrDefaultAsync(n => n.Id == needId && n.SessionId == sessionId);

        if (need is null)
            return new ReviewContext("", "", "", "", "", [], [], [], [], []);

        var courseHistory = await _db.TeachingNeeds
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Items).ThenInclude(i => i.SoftwareVersion)
            .Include(n => n.Session)
            .Where(n => n.CourseId == need.CourseId && n.Id != needId && n.Status == Core.Enums.NeedStatus.Approved)
            .OrderByDescending(n => n.ReviewedAt)
            .Take(5)
            .ToListAsync();

        var labStatuses = new List<string>();
        foreach (var item in need.Items.Where(i => i.SoftwareId.HasValue))
        {
            var entries = await _db.LaboratorySoftwares
                .Include(ls => ls.Laboratory)
                .Where(ls => ls.SoftwareId == item.SoftwareId!.Value)
                .ToListAsync();
            var installed = entries.Count(e => e.Status == "W");
            var total = entries.Count;
            labStatuses.Add($"{item.Software?.Name ?? "?"}: installé dans {installed}/{total} labos" +
                (entries.Any(e => e.Status == "M") ? $" (manquant dans: {string.Join(", ", entries.Where(e => e.Status == "M").Select(e => e.Laboratory.Name))})" : ""));
        }

        var otherSessionNeeds = await _db.TeachingNeeds
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Personnel)
            .Where(n => n.SessionId == sessionId && n.CourseId == need.CourseId && n.Id != needId
                        && n.Status != Core.Enums.NeedStatus.Draft)
            .ToListAsync();

        return new ReviewContext(
            SessionTitle: need.Session?.Title ?? "",
            CourseCode: need.Course?.Code ?? "",
            CourseName: need.Course?.Name ?? "",
            ProfessorName: need.Personnel != null ? $"{need.Personnel.FirstName} {need.Personnel.LastName}" : "",
            NeedStatus: need.Status.ToString(),
            Items: need.Items.Select(i => $"[{i.ItemType}] {i.Software?.Name ?? i.Description ?? "N/A"} {i.SoftwareVersion?.VersionNumber ?? ""} ({i.OS?.Name ?? ""}) {(string.IsNullOrWhiteSpace(i.Notes) ? "" : $"— {i.Notes}")}").ToList(),
            NeedMeta: new List<string>
            {
                need.ExpectedStudents.HasValue ? $"Étudiants attendus: {need.ExpectedStudents}" : "",
                !string.IsNullOrWhiteSpace(need.Notes) ? $"Notes: {need.Notes}" : "",
                !string.IsNullOrWhiteSpace(need.DesiredModifications) ? $"Modifications souhaitées: {need.DesiredModifications}" : "",
                !string.IsNullOrWhiteSpace(need.AdditionalComments) ? $"Commentaires: {need.AdditionalComments}" : "",
                need.IsFastTrack ? "FastTrack: Oui (identique à une demande précédente approuvée)" : ""
            }.Where(s => s != "").ToList(),
            LabStatuses: labStatuses,
            HistorySessions: courseHistory.Select(h => $"Session {h.Session?.Title ?? "?"}: {string.Join(", ", h.Items.Select(i => i.Software?.Name ?? i.Description ?? i.ItemType))}").ToList(),
            OtherNeedsThisSession: otherSessionNeeds.Select(n => $"{n.Personnel?.FirstName} {n.Personnel?.LastName}: {string.Join(", ", n.Items.Select(i => i.Software?.Name ?? i.Description ?? i.ItemType))} ({n.Status})").ToList()
        );
    }

    private static string BuildReviewPrompt(ReviewContext ctx)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Tu es un assistant pour un administrateur universitaire qui révise les demandes de besoins technologiques des professeurs.");
        sb.AppendLine("Analyse cette demande et donne un avis structuré pour aider l'admin à prendre une décision (approuver ou rejeter).");
        sb.AppendLine();
        sb.AppendLine($"Session: {ctx.SessionTitle}");
        sb.AppendLine($"Cours: {ctx.CourseCode} {ctx.CourseName}");
        sb.AppendLine($"Professeur: {ctx.ProfessorName}");
        sb.AppendLine($"Statut: {ctx.NeedStatus}");
        sb.AppendLine();

        if (ctx.NeedMeta.Count > 0)
        {
            sb.AppendLine("=== CONTEXTE DE LA DEMANDE ===");
            foreach (var m in ctx.NeedMeta) sb.AppendLine($"  {m}");
            sb.AppendLine();
        }

        sb.AppendLine("=== ITEMS DEMANDÉS ===");
        foreach (var item in ctx.Items) sb.AppendLine($"  {item}");
        sb.AppendLine();

        if (ctx.LabStatuses.Count > 0)
        {
            sb.AppendLine("=== STATUT DANS LES LABOS (matrice d'installation) ===");
            foreach (var ls in ctx.LabStatuses) sb.AppendLine($"  {ls}");
            sb.AppendLine();
        }

        if (ctx.HistorySessions.Count > 0)
        {
            sb.AppendLine("=== HISTORIQUE APPROUVÉ POUR CE COURS ===");
            foreach (var h in ctx.HistorySessions) sb.AppendLine($"  {h}");
            sb.AppendLine();
        }

        if (ctx.OtherNeedsThisSession.Count > 0)
        {
            sb.AppendLine("=== AUTRES DEMANDES POUR CE COURS CETTE SESSION ===");
            foreach (var o in ctx.OtherNeedsThisSession) sb.AppendLine($"  {o}");
            sb.AppendLine();
        }

        sb.AppendLine("Réponds en JSON strict (pas de markdown) avec ce format:");
        sb.AppendLine("""
{
  "summary": "Résumé de l'analyse en 2-3 phrases en français",
  "alerts": ["Alerte 1 si problème détecté", "Alerte 2..."],
  "suggestedAction": "approve" ou "review" ou "reject",
  "draftRejectReason": "Si reject, raison suggérée en français (null sinon)",
  "historyComparisons": [
    {"sessionTitle": "Session X", "similarity": "Identique / Similaire / Différent + détail court"}
  ]
}
""");
        sb.AppendLine("Sois concis. Les alertes sont pour les incohérences (version qui n'existe pas dans le catalogue, conflits avec d'autres demandes, logiciel déjà installé partout). suggestedAction='approve' si tout semble cohérent.");

        return sb.ToString();
    }

    private static AiReviewAnalysis ParseReviewResponse(string json)
    {
        var trimmed = json.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline > 0) trimmed = trimmed[(firstNewline + 1)..];
            if (trimmed.EndsWith("```")) trimmed = trimmed[..^3].TrimEnd();
        }

        using var doc = JsonDocument.Parse(trimmed);
        var root = doc.RootElement;

        var summary = root.TryGetProperty("summary", out var s) ? s.GetString() ?? "" : "";
        var alerts = new List<string>();
        if (root.TryGetProperty("alerts", out var alertsArr))
            foreach (var a in alertsArr.EnumerateArray())
                if (a.GetString() is { } val) alerts.Add(val);

        var suggestedAction = root.TryGetProperty("suggestedAction", out var sa) ? sa.GetString() : null;
        var draftRejectReason = root.TryGetProperty("draftRejectReason", out var drr) ? drr.GetString() : null;

        var comparisons = new List<AiHistoryComparison>();
        if (root.TryGetProperty("historyComparisons", out var hc))
            foreach (var c in hc.EnumerateArray())
                comparisons.Add(new AiHistoryComparison(
                    c.TryGetProperty("sessionTitle", out var st) ? st.GetString() ?? "" : "",
                    c.TryGetProperty("similarity", out var sim) ? sim.GetString() ?? "" : ""));

        return new AiReviewAnalysis(summary, alerts, suggestedAction, draftRejectReason, comparisons);
    }

    private record ReviewContext(
        string SessionTitle, string CourseCode, string CourseName,
        string ProfessorName, string NeedStatus,
        List<string> Items, List<string> NeedMeta,
        List<string> LabStatuses, List<string> HistorySessions,
        List<string> OtherNeedsThisSession);

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
