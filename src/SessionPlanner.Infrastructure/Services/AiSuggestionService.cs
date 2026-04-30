using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Enums;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class AiSuggestionService(
    AppDbContext db,
    IConfiguration config,
    ILogger<AiSuggestionService> logger
    ) : IAiSuggestionService
{
    private const string DefaultOpenAiModel = "gpt-4o-mini";

    private readonly AppDbContext _db = db;
    private readonly string? _apiKey = config["OpenAI:ApiKey"];
    private readonly string? _model = config["OpenAI:Model"];
    private ChatClient? ChatClient
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return null;

            return new ChatClient(_model ?? DefaultOpenAiModel, apiKey: _apiKey);
        }
    }
    private readonly ILogger<AiSuggestionService> _logger = logger;
    private static readonly JsonSerializerOptions _writeIntendedJsonOptions = new()
    {
        WriteIndented = true,
    };

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<AiSuggestionsResult> SuggestItemsAsync(int sessionId, int courseId, NeedItemType? itemType = null)
    {
        if (!IsConfigured)
            return new AiSuggestionsResult([], "AI suggestions are not configured.");

        var context = await BuildContextAsync(sessionId, courseId);
        var prompt = BuildPrompt(context, itemType);

        try
        {
            var response = await GetCompletionAsync(prompt);
            return ParseResponse(response);
        }
        catch (OpenAiQuotaException ex)
        {
            return new AiSuggestionsResult([], ex.Message);
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
            var response = await GetCompletionAsync(prompt);
            return ParseReviewResponse(response);
        }
        catch (OpenAiQuotaException ex)
        {
            return new AiReviewAnalysis(ex.Message, [], null, null, []);
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
            .Where(n => n.CourseId == need.CourseId && n.Id != needId && n.Status == NeedStatus.Approved)
            .OrderByDescending(n => n.ReviewedAt)
            .Take(5)
            .ToListAsync();

        var labStatuses = need.Items.Where(i => i.SoftwareId.HasValue).Aggregate(new List<string>(), (list, item) =>
        {
            var entries = _db.LaboratorySoftwares
                .Include(ls => ls.Laboratory)
                .Where(ls => ls.SoftwareId == item.SoftwareId!.Value)
                .ToList();
            var installed = entries.Count(e => e.Status == "W");
            var total = entries.Count;
            var softwareName = item.Software?.Name ?? "?";
            var missingLabNames = entries.Where(e => e.Status == "M").Select(e => e.Laboratory.Name);
            var missingLabsSuffix = missingLabNames.Any() ? $"(manquant dans: {string.Join(", ", missingLabNames)})" : "";

            list.Add($"{softwareName}: installé dans {installed}/{total} labos {missingLabsSuffix}");
            return list;
        });

        var otherSessionNeeds = await _db.TeachingNeeds
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Personnel)
            .Where(n => n.SessionId == sessionId && n.CourseId == need.CourseId && n.Id != needId
                        && n.Status != NeedStatus.Draft)
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
            HistorySessions: courseHistory.Select(h => $"Session {h.Session?.Title ?? "?"}: {string.Join(", ", h.Items.Select(i => i.Software?.Name ?? i.Description ?? i.ItemType.ToSnakeCase()))}").ToList(),
            OtherNeedsThisSession: otherSessionNeeds.Select(n => $"{n.Personnel?.FirstName} {n.Personnel?.LastName}: {string.Join(", ", n.Items.Select(i => i.Software?.Name ?? i.Description ?? i.ItemType.ToSnakeCase()))} ({n.Status})").ToList()
        );
    }

    /// <summary>
    /// Exemple de format de réponse attendu de l'IA pour l'analyse de révision, utilisé dans le prompt pour guider la structure de la réponse.
    /// </summary>
    private static readonly string ReviewResponseFormatExample = JsonSerializer.Serialize(new
    {
        summary = "Résumé de l'analyse en 2-3 phrases en français",
        alerts = new[] { "Alerte 1 si problème détecté", "Alerte 2..." },
        suggestedAction = "approve ou review ou reject",
        draftRejectReason = "Si reject, raison suggérée en français (null sinon)",
        historyComparisons = new[] { new {
            sessionTitle = "Session X",
            similarity = "Identique / Similaire / Différent + détail court"
        } }
    }, options: _writeIntendedJsonOptions);

    private static string BuildReviewPrompt(ReviewContext ctx)
    {
        var promptBuilder = new StringBuilder();
        void AppendAsNewLines(List<string> lines, uint indent = 0)
        {
            var indentation = new string(' ', (int)indent);
            foreach (var line in lines)
                promptBuilder.AppendLine($"{indentation}{line}");
        }
        void AppendAsNewLinesIndent2(List<string> lines) => AppendAsNewLines(lines, indent: 2);
        promptBuilder.AppendLine("Tu es un assistant pour un administrateur universitaire qui révise les demandes de besoins technologiques des professeurs.");
        promptBuilder.AppendLine("Analyse cette demande et donne un avis structuré pour aider l'admin à prendre une décision (approuver ou rejeter).");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine($"Session: {ctx.SessionTitle}");
        promptBuilder.AppendLine($"Cours: {ctx.CourseCode} {ctx.CourseName}");
        promptBuilder.AppendLine($"Professeur: {ctx.ProfessorName}");
        promptBuilder.AppendLine($"Statut: {ctx.NeedStatus}");
        promptBuilder.AppendLine();

        if (ctx.NeedMeta.Count > 0)
        {
            promptBuilder.AppendLine("=== CONTEXTE DE LA DEMANDE ===");
            AppendAsNewLinesIndent2(ctx.NeedMeta);
            promptBuilder.AppendLine();
        }

        promptBuilder.AppendLine("=== ITEMS DEMANDÉS ===");
        AppendAsNewLinesIndent2(ctx.Items);
        promptBuilder.AppendLine();

        if (ctx.LabStatuses.Count > 0)
        {
            promptBuilder.AppendLine("=== STATUT DANS LES LABOS (matrice d'installation) ===");
            AppendAsNewLinesIndent2(ctx.LabStatuses);
            promptBuilder.AppendLine();
        }

        if (ctx.HistorySessions.Count > 0)
        {
            promptBuilder.AppendLine("=== HISTORIQUE APPROUVÉ POUR CE COURS ===");
            AppendAsNewLinesIndent2(ctx.HistorySessions);
            promptBuilder.AppendLine();
        }

        if (ctx.OtherNeedsThisSession.Count > 0)
        {
            promptBuilder.AppendLine("=== AUTRES DEMANDES POUR CE COURS CETTE SESSION ===");
            AppendAsNewLinesIndent2(ctx.OtherNeedsThisSession);
            promptBuilder.AppendLine();
        }

        promptBuilder.AppendLine("Réponds en JSON strict (pas de markdown) avec ce format:");
        promptBuilder.AppendLine(ReviewResponseFormatExample);
        promptBuilder.AppendLine("Sois concis. Les alertes sont pour les incohérences (version qui n'existe pas dans le catalogue, conflits avec d'autres demandes, logiciel déjà installé partout). suggestedAction='approve' si tout semble cohérent.");

        return promptBuilder.ToString();
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
        string SessionTitle,
        string CourseCode,
        string CourseName,
        string ProfessorName,
        string NeedStatus,
        List<string> Items,
        List<string> NeedMeta,
        List<string> LabStatuses,
        List<string> HistorySessions,
        List<string> OtherNeedsThisSession
    );

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
            .Where(n => n.CourseId == courseId && n.Status == NeedStatus.Approved)
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
                        && n.Status != NeedStatus.Draft)
            .ToListAsync();

        return new CourseContext(
            SessionTitle: session?.Title ?? "",
            CourseCode: course?.Code ?? "",
            CourseName: course?.Name ?? "",
            History: [.. history.Select(n => new HistoryEntry(
                Session: n.Session?.Title ?? "",
                Items: [.. n.Items.Select(i => new HistoryItem(
                    Type: i.ItemType.ToSnakeCase(),
                    Software: i.Software?.Name,
                    Version: i.SoftwareVersion?.VersionNumber,
                    Os: i.OS?.Name,
                    Notes: i.Notes
                ))]
            ))],
            CatalogSoftware: [.. catalog.Select(s => new CatalogEntry(
                Name: s.Name,
                InstallCommand: s.InstallCommand,
                Versions: [.. s.SoftwareVersions.Select(v => new CatalogVersion(
                    Version: v.VersionNumber,
                    Os: v.OS?.Name ?? ""
                ))]
            ))],
            OperatingSystems: [.. osList.Select(o => o.Name)],
            AlreadyRequested: [.. existingNeeds.SelectMany(n => n.Items).Select(i =>
                i.Software?.Name ?? i.Description ?? i.ItemType.ToSnakeCase()
            ).Distinct()]
        );
    }

    private static readonly string BuildPromptJsonResponseFormat = JsonSerializer.Serialize(new
    {
        summary = "Résumé court de ta recommandation en français (1-2 phrases)",
        suggestions = new[] {
            new {
                itemType = "software ou vm ou saas etc.",
                label = "Nom affiché de la suggestion",
                softwareName = "Nom exact du logiciel (du catalogue si possible)",
                version = "Version suggérée",
                os = "Nom de l'OS",
                installCommand = "Commande d'installation si connue",
                notes = "Notes optionnelles",
                reason = "Pourquoi cette suggestion (en français, 1 phrase)"
            }
        }
    }, options: _writeIntendedJsonOptions);

    private static string BuildPrompt(CourseContext ctx, NeedItemType? itemType)
    {
        var promptBuilder = new StringBuilder();
        // Le prompt explique clairement le contexte du cours et de la session, puis fournit des informations détaillées sur l'historique des besoins approuvés pour ce cours, le catalogue de logiciels disponibles et les demandes déjà soumises pour cette session. L'IA est guidée pour suggérer des éléments pertinents en se basant sur ces données, tout en évitant les doublons avec les demandes existantes.
        promptBuilder.AppendLine($"""
        Tu es un assistant pour un système de planification de besoins technologiques universitaires.
        Un professeur prépare une demande de besoins pour son cours. Ton rôle est de suggérer les logiciels et ressources qu'il pourrait avoir besoin, basé sur l'historique du cours et le catalogue disponible.

        Session: {ctx.SessionTitle}
        Cours: {ctx.CourseCode} {ctx.CourseName}

        """);

        // Affiche l'historique des demandes approuvées pour ce cours, en mettant en évidence les logiciels utilisés. Cela aidera l'IA à suggérer des éléments cohérents avec les pratiques passées du cours.
        if (ctx.History.Count > 0)
        {
            promptBuilder.AppendLine("=== HISTORIQUE DES DEMANDES APPROUVÉES POUR CE COURS ===");
            foreach (var h in ctx.History.Take(5))
            {
                promptBuilder.AppendLine($"Session {h.Session}:");
                foreach (var item in h.Items)
                {
                    var softwarePart = item.Software ?? item.Notes ?? "N/A";
                    var versionPart = item.Version ?? "";
                    var osPart = item.Os ?? "";

                    promptBuilder.AppendLine($"  - [{item.Type}] {softwarePart} {versionPart} ({osPart})");
                }
            }
            promptBuilder.AppendLine();
        }

        if (ctx.AlreadyRequested.Count > 0)
        {
            promptBuilder.AppendLine("=== DÉJÀ DEMANDÉ POUR CETTE SESSION (ne pas suggérer à nouveau) ===");
            foreach (var r in ctx.AlreadyRequested)
                promptBuilder.AppendLine($"  - {r}");
            promptBuilder.AppendLine();
        }

        promptBuilder.AppendLine("=== CATALOGUE LOGICIEL DISPONIBLE (extraits) ===");
        foreach (var s in ctx.CatalogSoftware.Take(30))
        {
            var versions = string.Join(", ", s.Versions.Select(v => $"{v.Version} ({v.Os})").Take(3));
            var installCmd = s.InstallCommand != null ? $" | cmd: {s.InstallCommand}" : "";

            promptBuilder.AppendLine($"  - {s.Name}: {versions}{installCmd}");
        }
        promptBuilder.AppendLine();

        promptBuilder.AppendLine($"Systèmes d'exploitation disponibles: {string.Join(", ", ctx.OperatingSystems)}");
        promptBuilder.AppendLine();

        if (itemType != null)
        {
            var jsonItemType = JsonNamingPolicy.SnakeCaseLower.ConvertName(itemType.Value.ToString());

            promptBuilder.AppendLine($"Le professeur travaille sur un item de type: {jsonItemType}");
        }

        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Réponds en JSON strict avec ce format (pas de markdown, juste le JSON):");
        promptBuilder.AppendLine(BuildPromptJsonResponseFormat);
        promptBuilder.AppendLine("Donne entre 2 et 6 suggestions pertinentes. Priorise les logiciels de l'historique du cours. Utilise les noms exacts du catalogue quand c'est possible. Ne suggère PAS ce qui est déjà demandé pour cette session.");

        return promptBuilder.ToString();
    }

    private async Task<string> GetCompletionAsync(string prompt)
    {
        if (ChatClient is null)
            throw new InvalidOperationException("ChatClient is not initialized. Verify that IsConfigured is true before calling this method.");

        ChatMessage[] messages =
        [
            new SystemChatMessage("Tu es un assistant technique universitaire. Réponds uniquement en JSON valide."),
            new UserChatMessage(prompt)
        ];
        var options = new ChatCompletionOptions
        {
            Temperature = 0.3f,
            MaxOutputTokenCount = 1500
        };

        try
        {
            var completion = await ChatClient.CompleteChatAsync(messages, options);
            return completion.Value.Content[0].Text;
        }
        catch (System.ClientModel.ClientResultException ex) when (ex.Status == 429)
        {
            string? errorCode = null;
            var rawBody = ex.GetRawResponse()?.Content?.ToString();
            if (rawBody is not null)
            {
                try
                {
                    using var errDoc = JsonDocument.Parse(rawBody);
                    errorCode = errDoc.RootElement.TryGetProperty("error", out var err)
                        && err.TryGetProperty("code", out var c) ? c.GetString() : null;
                }
                catch (JsonException) { }
            }

            throw errorCode == "insufficient_quota"
                ? new OpenAiQuotaException("Le quota de l'API OpenAI est épuisé. Veuillez vérifier le plan de facturation sur platform.openai.com.")
                : new OpenAiQuotaException("Trop de requêtes vers l'API OpenAI. Veuillez réessayer dans quelques secondes.");
        }
        catch (System.ClientModel.ClientResultException ex)
        {
            _logger.LogError(ex, "OpenAI returned {StatusCode}", ex.Status);
            throw new HttpRequestException($"OpenAI API error ({ex.Status})", ex);
        }
    }

    public class OpenAiQuotaException(string message) : Exception(message)
    { }

    private static AiSuggestionsResult ParseResponse(string json)
    {
        // Strip markdown fences if the model wraps its reply
        var trimmed = json.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');

            if (firstNewline > 0)
                trimmed = trimmed[(firstNewline + 1)..];
            if (trimmed.EndsWith("```"))
                trimmed = trimmed[..^3].TrimEnd();
        }

        using var doc = JsonDocument.Parse(trimmed);
        var root = doc.RootElement;

        var summary = root.TryGetProperty("summary", out var s)
            ? s.GetString()
            : null;

        var suggestions = new List<AiSuggestedItem>();
        if (root.TryGetProperty("suggestions", out var arr))
        {
            suggestions = arr.EnumerateArray().Aggregate(new List<AiSuggestedItem>(), (list, item) =>
            {
                list.Add(new AiSuggestedItem(
                    ItemType: ParseNeedItemType(item.TryGetProperty("itemType", out var it) ? it.GetString() : null),
                    Label: item.TryGetProperty("label", out var l) ? l.GetString() ?? "" : "",
                    SoftwareName: item.TryGetProperty("softwareName", out var sn) ? sn.GetString() : null,
                    Version: item.TryGetProperty("version", out var v) ? v.GetString() : null,
                    Os: item.TryGetProperty("os", out var o) ? o.GetString() : null,
                    InstallCommand: item.TryGetProperty("installCommand", out var ic) ? ic.GetString() : null,
                    Notes: item.TryGetProperty("notes", out var n) ? n.GetString() : null,
                    Reason: item.TryGetProperty("reason", out var r) ? r.GetString() ?? "" : ""
                ));
                return list;
            });
        }

        return new AiSuggestionsResult(suggestions, summary);
    }

    /// <summary>
    /// Parses a snake_case itemType string from AI JSON output into a <see cref="NeedItemType"/> enum value.
    /// Defaults to <see cref="NeedItemType.Software"/> for unknown or null values.
    /// </summary>
    private static NeedItemType ParseNeedItemType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return NeedItemType.Software;

        // Strip underscores and do a case-insensitive parse to handle snake_case ("virtual_machine" → VirtualMachine)
        var normalized = value.Replace("_", "");
        return Enum.TryParse<NeedItemType>(normalized, ignoreCase: true, out var result)
            ? result
            : NeedItemType.Software;
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

    // ───── Rejection assistance ─────

    public async Task<RejectionAssistResult> GetRejectionAssistanceAsync(int sessionId, int needId)
    {
        if (!IsConfigured)
            return new RejectionAssistResult("AI suggestions are not configured.", [], null);

        var need = await _db.TeachingNeeds
            .Include(n => n.Personnel)
            .Include(n => n.Course)
            .Include(n => n.Session)
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Items).ThenInclude(i => i.SoftwareVersion)
            .Include(n => n.Items).ThenInclude(i => i.OS)
            .FirstOrDefaultAsync(n => n.Id == needId && n.SessionId == sessionId);

        if (need is null || string.IsNullOrWhiteSpace(need.RejectionReason))
            return new RejectionAssistResult(
                "Aucune information de rejet disponible.", [], null);

        var prompt = BuildRejectionAssistPrompt(need);

        try
        {
            var json = await GetCompletionAsync(prompt);
            return ParseRejectionAssistResponse(json);
        }
        catch (OpenAiQuotaException ex)
        {
            return new RejectionAssistResult(ex.Message, [], null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rejection assist failed for need {NeedId}", needId);
            return new RejectionAssistResult(
                "Impossible de générer l'assistance pour le moment.", [], null);
        }
    }

    private static readonly string JsonBuildRejectionAssistPromptFormat = JsonSerializer.Serialize(new
    {
        explanation = "Explication claire en français du problème identifié par l'admin, en 1-2 phrases simples",
        steps = new[]
        {
            new
            {
                action = "modifier | ajouter | retirer | vérifier",
                target = "Nom de l'élément concerné (ex: 'IntelliJ', 'Notes de la demande', 'Nombre d'étudiants')",
                detail = "Instruction précise de ce qu'il faut faire (ex: 'Changer la version de 2023.1 à 2024.1')"
            }
        },
        revisedNotes = "Si les notes de la demande devraient être modifiées, proposer le nouveau texte ici (null sinon)"
    }, options: _writeIntendedJsonOptions);

    private static string BuildRejectionAssistPrompt(TeachingNeed need)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine($"""
        Tu es un assistant qui aide un professeur à corriger une demande de besoins technologiques qui a été rejetée par l'administration.
        Analyse le motif de rejet et la demande actuelle, puis propose des étapes concrètes de correction.

        Cours: {need.Course?.Code} {need.Course?.Name}
        Session: {need.Session?.Title}

        === MOTIF DE REJET ===
        {need.RejectionReason}

        === CONTENU ACTUEL DE LA DEMANDE ===
        """);

        if (!string.IsNullOrWhiteSpace(need.Notes))
            promptBuilder.AppendLine($"Notes: {need.Notes}");
        if (need.ExpectedStudents.HasValue)
            promptBuilder.AppendLine($"Étudiants attendus: {need.ExpectedStudents}");
        if (!string.IsNullOrWhiteSpace(need.DesiredModifications))
            promptBuilder.AppendLine($"Modifications souhaitées: {need.DesiredModifications}");
        if (!string.IsNullOrWhiteSpace(need.AdditionalComments))
            promptBuilder.AppendLine($"Commentaires: {need.AdditionalComments}");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("=== ITEMS DE LA DEMANDE ===");
        foreach (var item in need.Items)
        {
            var parts = new List<string> { $"[{item.ItemType}]" };

            if (item.Software != null)
                parts.Add(item.Software.Name);
            if (item.SoftwareVersion != null)
                parts.Add($"v{item.SoftwareVersion.VersionNumber}");
            if (item.OS != null)
                parts.Add($"({item.OS.Name})");
            if (item.Quantity.HasValue)
                parts.Add($"Qté: {item.Quantity}");
            if (!string.IsNullOrWhiteSpace(item.Description))
                parts.Add(item.Description);
            if (!string.IsNullOrWhiteSpace(item.Notes))
                parts.Add($"— {item.Notes}");

            promptBuilder.AppendLine($"  {string.Join(" ", parts)}");
        }

        if (need.Items.Count == 0)
            promptBuilder.AppendLine("  (aucun item)");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("Réponds en JSON strict (pas de markdown) avec ce format:");
        promptBuilder.AppendLine(JsonBuildRejectionAssistPromptFormat);
        promptBuilder.AppendLine("Donne entre 1 et 5 étapes de correction concrètes et actionnables. Sois concis et pratique.");

        return promptBuilder.ToString();
    }

    private static RejectionAssistResult ParseRejectionAssistResponse(string json)
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

        var explanation = root.TryGetProperty("explanation", out var e) ? e.GetString() ?? "" : "";

        var steps = new List<RejectionCorrectionStep>();
        if (root.TryGetProperty("steps", out var stepsArr))
        {
            foreach (var s in stepsArr.EnumerateArray())
            {
                var action = s.TryGetProperty("action", out var a) ? a.GetString() ?? "" : "";
                var target = s.TryGetProperty("target", out var t) ? t.GetString() ?? "" : "";
                var detail = s.TryGetProperty("detail", out var d) ? d.GetString() ?? "" : "";
                steps.Add(new RejectionCorrectionStep(action, target, detail));
            }
        }

        var revisedNotes = root.TryGetProperty("revisedNotes", out var rn) ? rn.GetString() : null;

        return new RejectionAssistResult(explanation, steps, revisedNotes);
    }

    // ───── Auto-fill ─────

    public async Task<AutoFillResult> AutoFillFieldsAsync(AutoFillRequest request)
    {
        var result = request.ItemType switch
        {
            NeedItemType.Software => await AutoFillSoftwareAsync(request),
            NeedItemType.Saas => await AutoFillSaasAsync(request),
            NeedItemType.VirtualMachine => await AutoFillVmAsync(request),
            NeedItemType.PhysicalServer => await AutoFillServerAsync(request),
            NeedItemType.Configuration => await AutoFillConfigurationAsync(request),
            NeedItemType.EquipmentLoan => await AutoFillEquipmentAsync(request),
            _ => new AutoFillResult(new Dictionary<string, AutoFillSuggestion>(), "none")
        };

        return result;
    }

    private async Task<AutoFillResult> AutoFillSoftwareAsync(AutoFillRequest req)
    {
        var suggestions = new Dictionary<string, AutoFillSuggestion>();
        var softwareName = req.CurrentValues.GetValueOrDefault("softwareName", "").Trim();
        var source = "none";

        if (string.IsNullOrEmpty(softwareName))
            return new AutoFillResult(suggestions, source);

        // 1. Check history for this course
        var historyItems = await _db.TeachingNeeds
            .Where(n => n.CourseId == req.CourseId && n.Status == Core.Enums.NeedStatus.Approved)
            .OrderByDescending(n => n.ReviewedAt)
            .SelectMany(n => n.Items)
            .Include(i => i.Software)
            .Include(i => i.SoftwareVersion)
            .Include(i => i.OS)
            .Where(i => i.ItemType == NeedItemType.Software && i.Software != null
                        && EF.Functions.Like(i.Software.Name, softwareName))
            .Take(5)
            .ToListAsync();

        if (historyItems.Count > 0)
        {
            source = "history";
            var best = historyItems[0];

            if (string.IsNullOrEmpty(req.CurrentValues.GetValueOrDefault("versionNumber")) && best.SoftwareVersion != null)
                suggestions["versionNumber"] = new AutoFillSuggestion(
                    best.SoftwareVersion.VersionNumber,
                    "Dernière version utilisée pour ce cours",
                    0.9f);

            if (string.IsNullOrEmpty(req.CurrentValues.GetValueOrDefault("osId")) && best.OSId.HasValue)
                suggestions["osId"] = new AutoFillSuggestion(
                    best.OSId.Value.ToString(),
                    "OS utilisé précédemment pour ce cours",
                    0.9f);

            if (string.IsNullOrEmpty(req.CurrentValues.GetValueOrDefault("installationDetails")) && !string.IsNullOrEmpty(best.Software?.InstallCommand))
                suggestions["installationDetails"] = new AutoFillSuggestion(
                    best.Software.InstallCommand,
                    "Commande du catalogue",
                    0.85f);

            var historyNotes = historyItems
                .Where(i => !string.IsNullOrWhiteSpace(i.Notes))
                .Select(i => i.Notes!)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(req.CurrentValues.GetValueOrDefault("notes")) && historyNotes != null)
                suggestions["notes"] = new AutoFillSuggestion(
                    historyNotes,
                    "Notes de la demande précédente",
                    0.7f);

            return new AutoFillResult(suggestions, source);
        }

        // 2. Fallback: catalog lookup
        var catalogSw = await _db.Softwares
            .Include(s => s.SoftwareVersions).ThenInclude(v => v.OS)
            .FirstOrDefaultAsync(s => EF.Functions.Like(s.Name, softwareName));

        if (catalogSw != null)
        {
            source = "catalog";

            var latestVersion = catalogSw.SoftwareVersions
                .OrderByDescending(v => v.Id)
                .FirstOrDefault();

            if (latestVersion != null && string.IsNullOrEmpty(req.CurrentValues.GetValueOrDefault("versionNumber")))
                suggestions["versionNumber"] = new AutoFillSuggestion(
                    latestVersion.VersionNumber,
                    "Dernière version du catalogue",
                    0.8f);

            if (latestVersion != null && string.IsNullOrEmpty(req.CurrentValues.GetValueOrDefault("osId")))
                suggestions["osId"] = new AutoFillSuggestion(
                    latestVersion.OsId.ToString(),
                    "OS de la dernière version catalogue",
                    0.75f);

            if (!string.IsNullOrEmpty(catalogSw.InstallCommand) && string.IsNullOrEmpty(req.CurrentValues.GetValueOrDefault("installationDetails")))
                suggestions["installationDetails"] = new AutoFillSuggestion(
                    catalogSw.InstallCommand,
                    "Commande d'installation du catalogue",
                    0.85f);

            return new AutoFillResult(suggestions, source);
        }

        return new AutoFillResult(suggestions, source);
    }

    private async Task<AutoFillResult> AutoFillSaasAsync(AutoFillRequest req)
    {
        var suggestions = new Dictionary<string, AutoFillSuggestion>();
        var name = req.CurrentValues.GetValueOrDefault("name", "").Trim();
        if (string.IsNullOrEmpty(name)) return new AutoFillResult(suggestions, "none");

        var historyItem = await _db.TeachingNeeds
            .Where(n => n.CourseId == req.CourseId && n.Status == Core.Enums.NeedStatus.Approved)
            .OrderByDescending(n => n.ReviewedAt)
            .SelectMany(n => n.Items)
            .Where(i => i.ItemType == NeedItemType.Saas && i.DetailsJson != null && i.DetailsJson.Contains(name))
            .FirstOrDefaultAsync();

        if (historyItem?.DetailsJson != null)
        {
            try
            {
                using var doc = JsonDocument.Parse(historyItem.DetailsJson);
                if (doc.RootElement.TryGetProperty("numberOfAccounts", out var acc))
                {
                    var val = acc.ToString();
                    if (!string.IsNullOrEmpty(val) && string.IsNullOrEmpty(req.CurrentValues.GetValueOrDefault("numberOfAccounts")))
                        suggestions["numberOfAccounts"] = new AutoFillSuggestion(val, "Nombre de comptes de la demande précédente", 0.85f);
                }
            }
            catch { /* ignore parse errors */ }
        }

        return new AutoFillResult(suggestions, suggestions.Count > 0 ? "history" : "none");
    }

    private async Task<AutoFillResult> AutoFillVmAsync(AutoFillRequest req)
    {
        var suggestions = new Dictionary<string, AutoFillSuggestion>();

        var historyItems = await _db.TeachingNeeds
            .Where(n => n.CourseId == req.CourseId && n.Status == Core.Enums.NeedStatus.Approved)
            .OrderByDescending(n => n.ReviewedAt)
            .SelectMany(n => n.Items)
            .Where(i => i.ItemType == NeedItemType.VirtualMachine && i.DetailsJson != null)
            .Take(3)
            .ToListAsync();

        if (historyItems.Count > 0 && historyItems[0].DetailsJson != null)
        {
            try
            {
                using var doc = JsonDocument.Parse(historyItems[0].DetailsJson!);
                var root = doc.RootElement;
                TrySuggestFromJson(root, "cpuCores", req.CurrentValues, suggestions, "CPU des VMs précédentes");
                TrySuggestFromJson(root, "ramGb", req.CurrentValues, suggestions, "RAM des VMs précédentes");
                TrySuggestFromJson(root, "storageGb", req.CurrentValues, suggestions, "Stockage des VMs précédentes");
                TrySuggestFromJson(root, "accessType", req.CurrentValues, suggestions, "Type d'accès précédent");
                TrySuggestFromJson(root, "quantity", req.CurrentValues, suggestions, "Quantité précédente");

                if (root.TryGetProperty("osId", out var osVal) && string.IsNullOrEmpty(req.CurrentValues.GetValueOrDefault("osId")))
                    suggestions["osId"] = new AutoFillSuggestion(osVal.ToString(), "OS des VMs précédentes", 0.85f);
            }
            catch { /* ignore */ }
        }

        return new AutoFillResult(suggestions, suggestions.Count > 0 ? "history" : "none");
    }

    private async Task<AutoFillResult> AutoFillServerAsync(AutoFillRequest req)
    {
        var suggestions = new Dictionary<string, AutoFillSuggestion>();

        var historyItems = await _db.TeachingNeeds
            .Where(n => n.CourseId == req.CourseId && n.Status == Core.Enums.NeedStatus.Approved)
            .OrderByDescending(n => n.ReviewedAt)
            .SelectMany(n => n.Items)
            .Where(i => i.ItemType == NeedItemType.PhysicalServer && i.DetailsJson != null)
            .Take(3)
            .ToListAsync();

        if (historyItems.Count > 0 && historyItems[0].DetailsJson != null)
        {
            try
            {
                using var doc = JsonDocument.Parse(historyItems[0].DetailsJson!);
                var root = doc.RootElement;
                TrySuggestFromJson(root, "cpuCores", req.CurrentValues, suggestions, "CPU précédent");
                TrySuggestFromJson(root, "ramGb", req.CurrentValues, suggestions, "RAM précédente");
                TrySuggestFromJson(root, "storageGb", req.CurrentValues, suggestions, "Stockage précédent");
                TrySuggestFromJson(root, "accessType", req.CurrentValues, suggestions, "Type d'accès précédent");
                if (root.TryGetProperty("osId", out var osVal) && string.IsNullOrEmpty(req.CurrentValues.GetValueOrDefault("osId")))
                    suggestions["osId"] = new AutoFillSuggestion(osVal.ToString(), "OS précédent", 0.85f);
            }
            catch { /* ignore */ }
        }

        return new AutoFillResult(suggestions, suggestions.Count > 0 ? "history" : "none");
    }

    private async Task<AutoFillResult> AutoFillConfigurationAsync(AutoFillRequest req)
    {
        var suggestions = new Dictionary<string, AutoFillSuggestion>();

        var historyItems = await _db.TeachingNeeds
            .Where(n => n.CourseId == req.CourseId && n.Status == NeedStatus.Approved)
            .OrderByDescending(n => n.ReviewedAt)
            .SelectMany(n => n.Items)
            .Where(i => i.ItemType == NeedItemType.Configuration && i.DetailsJson != null)
            .Take(3)
            .ToListAsync();

        if (historyItems.Count > 0 && historyItems[0].DetailsJson != null)
        {
            try
            {
                using var doc = JsonDocument.Parse(historyItems[0].DetailsJson!);
                var root = doc.RootElement;
                TrySuggestFromJson(root, "osIds", req.CurrentValues, suggestions, "OS des configurations précédentes");
                TrySuggestFromJson(root, "laboratoryIds", req.CurrentValues, suggestions, "Labos des configurations précédentes");
                TrySuggestFromJson(root, "notes", req.CurrentValues, suggestions, "Notes de la configuration précédente");
            }
            catch { /* ignore */ }
        }

        return new AutoFillResult(suggestions, suggestions.Count > 0 ? "history" : "none");
    }

    private async Task<AutoFillResult> AutoFillEquipmentAsync(AutoFillRequest req)
    {
        var suggestions = new Dictionary<string, AutoFillSuggestion>();
        var name = req.CurrentValues.GetValueOrDefault("name", "").Trim();
        if (string.IsNullOrEmpty(name)) return new AutoFillResult(suggestions, "none");

        var historyItem = await _db.TeachingNeeds
            .Where(n => n.CourseId == req.CourseId && n.Status == NeedStatus.Approved)
            .OrderByDescending(n => n.ReviewedAt)
            .SelectMany(n => n.Items)
            .Where(i => i.ItemType == NeedItemType.EquipmentLoan && i.DetailsJson != null && i.DetailsJson.Contains(name))
            .FirstOrDefaultAsync();

        if (historyItem?.DetailsJson != null)
        {
            try
            {
                using var doc = JsonDocument.Parse(historyItem.DetailsJson);
                var root = doc.RootElement;
                TrySuggestFromJson(root, "quantity", req.CurrentValues, suggestions, "Quantité précédente");
                TrySuggestFromJson(root, "defaultAccessories", req.CurrentValues, suggestions, "Accessoires précédents");
            }
            catch { /* ignore */ }
        }

        return new AutoFillResult(suggestions, suggestions.Count > 0 ? "history" : "none");
    }

    private static void TrySuggestFromJson(
        JsonElement root, string fieldName,
        Dictionary<string, string> currentValues,
        Dictionary<string, AutoFillSuggestion> suggestions,
        string reason)
    {
        if (root.TryGetProperty(fieldName, out var val))
        {
            var strVal = val.ToString();
            if (!string.IsNullOrEmpty(strVal) && string.IsNullOrEmpty(currentValues.GetValueOrDefault(fieldName)))
                suggestions[fieldName] = new AutoFillSuggestion(strVal, reason, 0.8f);
        }
    }
}
