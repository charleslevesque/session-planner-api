using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Enums;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Data;

namespace SessionPlanner.Infrastructure.Services;

public class TeachingNeedService : ITeachingNeedService
{
    private static readonly JsonSerializerOptions DetailJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly AppDbContext _db;
    private readonly ICourseService _courseService;
    private readonly ISoftwareService _softwareService;
    private readonly ISoftwareVersionService _softwareVersionService;
    private readonly ISaaSProductService _saasProductService;
    private readonly IVirtualMachineService _virtualMachineService;
    private readonly IPhysicalServerService _physicalServerService;
    private readonly IEquipmentModelService _equipmentModelService;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<TeachingNeedService> _logger;

    public TeachingNeedService(
        AppDbContext db,
        ICourseService courseService,
        ISoftwareService softwareService,
        ISoftwareVersionService softwareVersionService,
        ISaaSProductService saasProductService,
        IVirtualMachineService virtualMachineService,
        IPhysicalServerService physicalServerService,
        IEquipmentModelService equipmentModelService,
        IConfigurationService configurationService,
        ILogger<TeachingNeedService> logger)
    {
        _db = db;
        _courseService = courseService;
        _softwareService = softwareService;
        _softwareVersionService = softwareVersionService;
        _saasProductService = saasProductService;
        _virtualMachineService = virtualMachineService;
        _physicalServerService = physicalServerService;
        _equipmentModelService = equipmentModelService;
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<int?> GetPersonnelIdForUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        return user?.PersonnelId;
    }

    public async Task<int?> GetOrCreatePersonnelIdForUserAsync(int userId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user is null)
            return null;

        if (user.PersonnelId is not null)
            return user.PersonnelId;

        var rawUsername = (user.Username ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(rawUsername))
            return null;

        // Reuse existing Personnel by email when present to avoid duplicates.
        var existingPersonnel = await _db.Personnel
            .FirstOrDefaultAsync(p => p.Email == rawUsername);

        if (existingPersonnel is not null)
        {
            user.PersonnelId = existingPersonnel.Id;
            await _db.SaveChangesAsync();
            return existingPersonnel.Id;
        }

        var localPart = rawUsername.Split('@')[0];
        var nameParts = localPart
            .Split(new[] { '.', '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var firstName = nameParts.Length > 0 ? ToTitle(nameParts[0]) : "Teacher";
        var lastName = nameParts.Length > 1 ? ToTitle(nameParts[1]) : "User";

        var personnel = new Personnel
        {
            FirstName = firstName,
            LastName = lastName,
            Email = rawUsername,
            Function = PersonnelFunction.Professor,
        };

        _db.Personnel.Add(personnel);
        await _db.SaveChangesAsync();

        user.PersonnelId = personnel.Id;
        await _db.SaveChangesAsync();

        return personnel.Id;
    }

    private static string ToTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "User";
        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }

    public async Task<List<TeachingNeed>> GetAllBySessionAsync(int sessionId, int? filterByPersonnelId = null)
    {
        var query = _db.TeachingNeeds
            .Include(n => n.Personnel)
            .Include(n => n.Course)
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Items).ThenInclude(i => i.SoftwareVersion)
            .Include(n => n.Items).ThenInclude(i => i.OS)
            .Where(n => n.SessionId == sessionId);

        if (filterByPersonnelId.HasValue)
            query = query.Where(n => n.PersonnelId == filterByPersonnelId.Value);

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<List<TeachingNeed>> GetMyNeedsAsync(int personnelId, int? sessionId = null, int? courseId = null, IEnumerable<NeedStatus>? statuses = null)
    {
        var query = _db.TeachingNeeds
            .Include(n => n.Session)
            .Include(n => n.Personnel)
            .Include(n => n.Course)
            .Where(n => n.PersonnelId == personnelId);

        if (sessionId.HasValue)
            query = query.Where(n => n.SessionId == sessionId.Value);

        if (courseId.HasValue)
            query = query.Where(n => n.CourseId == courseId.Value);

        if (statuses is not null)
        {
            var statusList = statuses.ToList();
            if (statusList.Count > 0)
                query = query.Where(n => statusList.Contains(n.Status));
        }

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<TeachingNeed?> GetByIdAsync(int sessionId, int id)
    {
        return await _db.TeachingNeeds
            .Include(n => n.Personnel)
            .Include(n => n.Course)
            .Include(n => n.Items).ThenInclude(i => i.Software)
            .Include(n => n.Items).ThenInclude(i => i.SoftwareVersion)
            .Include(n => n.Items).ThenInclude(i => i.OS)
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);
    }

    public async Task<TeachingNeed> CreateAsync(int sessionId, int personnelId, int courseId, string? notes,
        int? expectedStudents = null, bool? hasTechNeeds = null, bool? foundAllCourses = null,
        string? desiredModifications = null, bool? allowsUpdates = null, string? additionalComments = null)
    {
        var session = await _db.Sessions.FindAsync(sessionId)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.Status != SessionStatus.Open)
            throw new InvalidOperationException("Cannot create a need: the session is not open.");

        var need = new TeachingNeed
        {
            SessionId = sessionId,
            PersonnelId = personnelId,
            CourseId = courseId,
            Notes = notes,
            ExpectedStudents = expectedStudents,
            HasTechNeeds = hasTechNeeds,
            FoundAllCourses = foundAllCourses,
            DesiredModifications = desiredModifications,
            AllowsUpdates = allowsUpdates,
            AdditionalComments = additionalComments
        };

        _db.TeachingNeeds.Add(need);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(sessionId, need.Id)
            ?? throw new InvalidOperationException("Failed to reload created teaching need.");
    }

    public async Task<TeachingNeed?> UpdateAsync(int sessionId, int id, int courseId, string? notes,
        int? expectedStudents = null, bool? hasTechNeeds = null, bool? foundAllCourses = null,
        string? desiredModifications = null, bool? allowsUpdates = null, string? additionalComments = null)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        if (need.Status != NeedStatus.Draft && need.Status != NeedStatus.Submitted && need.Status != NeedStatus.Rejected)
            throw new InvalidOperationException("Need can only be modified when in Draft, Submitted, or Rejected status.");

        need.CourseId = courseId;
        need.Notes = notes;
        need.ExpectedStudents = expectedStudents;
        need.HasTechNeeds = hasTechNeeds;
        need.FoundAllCourses = foundAllCourses;
        need.DesiredModifications = desiredModifications;
        need.AllowsUpdates = allowsUpdates;
        need.AdditionalComments = additionalComments;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(sessionId, id);
    }

    public async Task<bool> DeleteAsync(int sessionId, int id)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return false;

        if (need.Status == NeedStatus.Approved)
            throw new InvalidOperationException("Approved needs cannot be cancelled.");

        _db.TeachingNeeds.Remove(need);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<TeachingNeedItem?> AddItemAsync(int sessionId, int needId, string itemType, int? softwareId, int? softwareVersionId, int? osId, int? quantity, string? description, string? notes, string? detailsJson)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == needId);

        if (need is null) return null;

        if (need.Status != NeedStatus.Draft && need.Status != NeedStatus.Submitted && need.Status != NeedStatus.Rejected)
            throw new InvalidOperationException("Items can only be added to Draft, Submitted, or Rejected needs.");

        var item = new TeachingNeedItem
        {
            TeachingNeedId = needId,
            ItemType = itemType,
            SoftwareId = softwareId,
            SoftwareVersionId = softwareVersionId,
            OSId = osId,
            Quantity = quantity,
            Description = description,
            Notes = notes,
            DetailsJson = detailsJson
        };

        _db.TeachingNeedItems.Add(item);
        await _db.SaveChangesAsync();

        return await _db.TeachingNeedItems
            .Include(i => i.Software)
            .Include(i => i.SoftwareVersion)
            .Include(i => i.OS)
            .FirstOrDefaultAsync(i => i.Id == item.Id);
    }

    public async Task<bool> RemoveItemAsync(int sessionId, int needId, int itemId)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == needId);

        if (need is null) return false;

        if (need.Status != NeedStatus.Draft && need.Status != NeedStatus.Submitted && need.Status != NeedStatus.Rejected)
            throw new InvalidOperationException("Items can only be removed from Draft, Submitted, or Rejected needs.");

        var item = await _db.TeachingNeedItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TeachingNeedId == needId);

        if (item is null) return false;

        _db.TeachingNeedItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<TeachingNeed?> SubmitAsync(int sessionId, int id)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        EnsureStatus(need, NeedStatus.Draft, "Need can only be submitted from Draft status.");

        need.Status = NeedStatus.Submitted;
        need.SubmittedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(sessionId, id);
    }

    public async Task<TeachingNeed?> ReviewAsync(int sessionId, int id)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        EnsureStatus(need, NeedStatus.Submitted, "Need can only be moved to UnderReview from Submitted status.");

        need.Status = NeedStatus.UnderReview;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(sessionId, id);
    }

    public async Task<TeachingNeed?> ApproveAsync(int sessionId, int id, int? reviewedByUserId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var need = await _db.TeachingNeeds
                .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

            if (need is null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            EnsureStatus(need, NeedStatus.UnderReview, "Need can only be approved from UnderReview status.");

            need.Status = NeedStatus.Approved;
            need.ReviewedAt = DateTime.UtcNow;
            need.ReviewedByUserId = reviewedByUserId;

            await _db.SaveChangesAsync();

            // Re-read items fresh from DB after status change so that any item that was
            // deleted + re-created by the teacher before resubmission is reflected here,
            // never a stale in-memory snapshot from a previous query.
            var latestItems = await _db.TeachingNeedItems
                .Where(i => i.TeachingNeedId == id)
                .ToListAsync();

            await PropagateApprovedItemsToCourseAsync(need.CourseId, latestItems);

            await transaction.CommitAsync();
            return await GetByIdAsync(sessionId, id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task PropagateApprovedItemsToCourseAsync(int courseId, ICollection<TeachingNeedItem> items)
    {
        foreach (var item in items)
        {
            var type = (item.ItemType ?? string.Empty).Trim().ToLowerInvariant();
            switch (type)
            {
                case "software":
                    await PropagateSoftwareItemAsync(courseId, item);
                    break;
                case "saas":
                    await PropagateSaasItemAsync(courseId, item);
                    break;
                case "virtual_machine":
                    await PropagateVirtualMachineItemAsync(courseId, item);
                    break;
                case "physical_server":
                    await PropagatePhysicalServerItemAsync(courseId, item);
                    break;
                case "equipment_loan":
                    await PropagateEquipmentItemAsync(courseId, item);
                    break;
                case "configuration":
                    await PropagateConfigurationItemAsync(courseId, item);
                    break;
                case "other":
                    _logger.LogInformation(
                        "Skipping teaching need item {ItemId} (type other) — no course resource to create.",
                        item.Id);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Item {item.Id}: unsupported itemType '{item.ItemType}'.");
            }
        }
    }

    private async Task PropagateSoftwareItemAsync(int courseId, TeachingNeedItem item)
    {
        if (item.SoftwareVersionId is int svId)
        {
            var r = await _courseService.AssociateSoftwareVersionAsync(courseId, svId);
            if (r is null)
                throw new InvalidOperationException(
                    $"Item {item.Id}: SoftwareVersion {svId} not found in the catalogue.");
            return;
        }

        if (item.SoftwareId is int sid)
        {
            var r = await _courseService.AssociateSoftwareAsync(courseId, sid);
            if (r is null)
                throw new InvalidOperationException(
                    $"Item {item.Id}: Software {sid} not found in the catalogue.");
            return;
        }

        var details = ParseDetailsDict(item.DetailsJson);
        var softwareName = DetailString(details, "softwareName", "name");
        var versionNumber = DetailString(details, "versionNumber", "version");
        var osId = DetailInt(details, "osId", "OSId");

        if (string.IsNullOrWhiteSpace(softwareName) || string.IsNullOrWhiteSpace(versionNumber) || osId is null)
            throw new InvalidOperationException(
                $"Item {item.Id} (software): detailsJson is incomplete — softwareName, versionNumber and osId are required.");

        var trimmedName = softwareName.Trim();
        var trimmedVersion = versionNumber.Trim();

        var software = await _db.Softwares
            .FirstOrDefaultAsync(s => s.Name.ToLower() == trimmedName.ToLowerInvariant());
        software ??= await _softwareService.CreateAsync(trimmedName);

        var version = await _db.SoftwareVersions
            .FirstOrDefaultAsync(v =>
                v.SoftwareId == software.Id &&
                v.OsId == osId.Value &&
                v.VersionNumber == trimmedVersion);

        if (version is null)
        {
            var installationDetails = DetailString(details, "installationDetails");
            var notes = DetailString(details, "notes");
            version = await _softwareVersionService
                .CreateAsync(software.Id, osId.Value, trimmedVersion, installationDetails, notes)
                ?? throw new InvalidOperationException(
                    $"Item {item.Id}: could not create SoftwareVersion for OS {osId}.");
        }

        var assoc = await _courseService.AssociateSoftwareVersionAsync(courseId, version.Id);
        if (assoc is null)
            throw new InvalidOperationException(
                $"Item {item.Id}: SoftwareVersion {version.Id} not found after creation.");
    }

    private async Task PropagateSaasItemAsync(int courseId, TeachingNeedItem item)
    {
        var details = ParseDetailsDict(item.DetailsJson);
        var name = DetailString(details, "name");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException(
                $"Item {item.Id} (saas): detailsJson missing required field 'name'.");

        var trimmed = name.Trim();
        var product = await _db.SaaSProducts
            .FirstOrDefaultAsync(p => p.Name.ToLower() == trimmed.ToLowerInvariant());

        if (product is null)
        {
            var accounts = DetailInt(details, "numberOfAccounts");
            var notes = DetailString(details, "notes");
            product = await _saasProductService.CreateAsync(trimmed, accounts, notes);
        }

        var r = await _courseService.AssociateSaaSProductAsync(courseId, product.Id);
        if (r is null)
            throw new InvalidOperationException(
                $"Item {item.Id}: SaaS product {product.Id} not found after find-or-create.");
    }

    private async Task PropagateVirtualMachineItemAsync(int courseId, TeachingNeedItem item)
    {
        var details = ParseDetailsDict(item.DetailsJson);
        var quantity = DetailInt(details, "quantity") ?? 1;
        var cpu = DetailInt(details, "cpuCores");
        var ram = DetailInt(details, "ramGb");
        var storage = DetailInt(details, "storageGb");
        var accessType = DetailString(details, "accessType");
        var osId = DetailInt(details, "osId", "OSId");
        var hostServerId = DetailInt(details, "hostServerId", "HostServerId");
        var notes = DetailString(details, "notes");

        if (cpu is null || ram is null || storage is null || string.IsNullOrWhiteSpace(accessType) || osId is null)
            throw new InvalidOperationException(
                $"Item {item.Id} (virtual_machine): detailsJson is incomplete — cpuCores, ramGb, storageGb, accessType and osId are required.");

        var trimmedAccess = accessType.Trim();

        // Find-or-create: match by full spec (quantity+cpu+ram+storage+accessType+osId+hostServerId).
        var existing = await _db.VirtualMachines
            .FirstOrDefaultAsync(v =>
                v.Quantity == quantity &&
                v.CpuCores == cpu.Value &&
                v.RamGb == ram.Value &&
                v.StorageGb == storage.Value &&
                v.AccessType == trimmedAccess &&
                v.OSId == osId.Value &&
                v.HostServerId == hostServerId);

        VirtualMachine vm;
        if (existing is not null)
        {
            vm = existing;
        }
        else
        {
            var result = await _virtualMachineService.CreateAsync(
                quantity, cpu.Value, ram.Value, storage.Value, trimmedAccess, notes, osId.Value, hostServerId);

            if (result.Status != VirtualMachineOperationStatus.Success || result.VirtualMachine is null)
                throw new InvalidOperationException(
                    $"Item {item.Id}: VM creation failed with status {result.Status}.");

            vm = result.VirtualMachine;
        }

        var assoc = await _courseService.AssociateVirtualMachineAsync(courseId, vm.Id);
        if (assoc is null)
            throw new InvalidOperationException(
                $"Item {item.Id}: VM {vm.Id} not found after find-or-create.");
    }

    private async Task PropagatePhysicalServerItemAsync(int courseId, TeachingNeedItem item)
    {
        var details = ParseDetailsDict(item.DetailsJson);
        var hostname = DetailString(details, "hostname");
        var cpu = DetailInt(details, "cpuCores");
        var ram = DetailInt(details, "ramGb");
        var storage = DetailInt(details, "storageGb");
        var accessType = DetailString(details, "accessType");
        var osId = DetailInt(details, "osId", "OSId");
        var notes = DetailString(details, "notes");

        if (string.IsNullOrWhiteSpace(hostname) || cpu is null || ram is null || storage is null ||
            string.IsNullOrWhiteSpace(accessType) || osId is null)
            throw new InvalidOperationException(
                $"Item {item.Id} (physical_server): detailsJson is incomplete — hostname, cpuCores, ramGb, storageGb, accessType and osId are required.");

        var h = hostname.Trim();
        var result = await _physicalServerService.CreateAsync(
            h, cpu.Value, ram.Value, storage.Value, accessType.Trim(), notes, osId.Value);

        PhysicalServer? server = result.Server;

        if (result.Status == PhysicalServerOperationStatus.DuplicateHostname)
            server = await _db.PhysicalServers.FirstOrDefaultAsync(s => s.Hostname == h);
        else if (result.Status != PhysicalServerOperationStatus.Success)
            throw new InvalidOperationException(
                $"Item {item.Id}: physical server creation failed with status {result.Status}.");

        if (server is null)
            throw new InvalidOperationException(
                $"Item {item.Id}: could not resolve physical server '{h}' after find-or-create.");

        var assoc = await _courseService.AssociatePhysicalServerAsync(courseId, server.Id);
        if (assoc is null)
            throw new InvalidOperationException(
                $"Item {item.Id}: physical server {server.Id} not found after find-or-create.");
    }

    private async Task PropagateEquipmentItemAsync(int courseId, TeachingNeedItem item)
    {
        var details = ParseDetailsDict(item.DetailsJson);
        var name = DetailString(details, "name");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException(
                $"Item {item.Id} (equipment_loan): detailsJson missing required field 'name'.");

        var trimmed = name.Trim();
        var qty = DetailInt(details, "quantity") ?? 1;
        var accessories = DetailString(details, "defaultAccessories");
        var notes = DetailString(details, "notes");

        // Find-or-create by name (case-insensitive).
        var model = await _db.EquipmentModels
            .FirstOrDefaultAsync(e => e.Name.ToLower() == trimmed.ToLowerInvariant());

        model ??= await _equipmentModelService.CreateAsync(trimmed, qty, accessories, notes);

        var assoc = await _courseService.AssociateEquipmentModelAsync(courseId, model.Id);
        if (assoc is null)
            throw new InvalidOperationException(
                $"Item {item.Id}: equipment model {model.Id} not found after find-or-create.");
    }

    private async Task PropagateConfigurationItemAsync(int courseId, TeachingNeedItem item)
    {
        var details = ParseDetailsDict(item.DetailsJson);
        var title = DetailString(details, "title");
        var osRaw = DetailString(details, "osIds");
        var labRaw = DetailString(details, "laboratoryIds");

        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException(
                $"Item {item.Id} (configuration): detailsJson missing required field 'title'.");

        var osIds = ParseIdList(osRaw);
        var labIds = ParseIdList(labRaw);

        if (osIds.Count == 0 || labIds.Count == 0)
            throw new InvalidOperationException(
                $"Item {item.Id} (configuration): detailsJson missing required fields 'osIds' and/or 'laboratoryIds'.");

        var trimmedTitle = title.Trim();
        var notes = DetailString(details, "notes");

        var configuration = await FindMatchingConfigurationAsync(trimmedTitle, osIds, labIds);

        configuration ??= await _configurationService.CreateAsync(trimmedTitle, osIds, labIds, notes);

        var assoc = await _courseService.AssociateConfigurationAsync(courseId, configuration.Id);
        if (assoc is null)
            throw new InvalidOperationException(
                $"Item {item.Id}: configuration {configuration.Id} not found after find-or-create.");
    }

    private static List<int> ParseIdList(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
    }

    private async Task<Configuration?> FindMatchingConfigurationAsync(string title, IReadOnlyCollection<int> osIds, IReadOnlyCollection<int> labIds)
    {
        var normalized = title.ToLowerInvariant();
        var requestedOs = osIds.ToHashSet();
        var requestedLabs = labIds.ToHashSet();

        var candidates = await _db.Configurations
            .Include(c => c.ConfigurationOSes)
            .Include(c => c.LaboratoryConfigurations)
            .Where(c => c.Title.ToLower() == normalized)
            .ToListAsync();

        foreach (var candidate in candidates)
        {
            var candidateOs = candidate.ConfigurationOSes.Select(x => x.OSId).ToHashSet();
            var candidateLabs = candidate.LaboratoryConfigurations.Select(x => x.LaboratoryId).ToHashSet();
            if (candidateOs.SetEquals(requestedOs) && candidateLabs.SetEquals(requestedLabs))
                return candidate;
        }

        return null;
    }

    private static IReadOnlyDictionary<string, JsonElement>? ParseDetailsDict(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, DetailJsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string? DetailString(IReadOnlyDictionary<string, JsonElement>? d, params string[] keys)
    {
        if (d is null) return null;
        foreach (var key in keys)
        {
            if (!d.TryGetValue(key, out var el))
                continue;
            if (el.ValueKind == JsonValueKind.Null || el.ValueKind == JsonValueKind.Undefined)
                return null;
            if (el.ValueKind == JsonValueKind.String)
                return el.GetString();
            return el.ToString();
        }

        return null;
    }

    private static int? DetailInt(IReadOnlyDictionary<string, JsonElement>? d, params string[] keys)
    {
        if (d is null) return null;
        foreach (var key in keys)
        {
            if (!d.TryGetValue(key, out var el))
                continue;
            if (el.ValueKind == JsonValueKind.Null || el.ValueKind == JsonValueKind.Undefined)
                return null;
            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var i))
                return i;
            if (el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out var j))
                return j;
        }

        return null;
    }

    public async Task<TeachingNeed?> RejectAsync(int sessionId, int id, string reason, int? reviewedByUserId)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        EnsureStatus(need, NeedStatus.UnderReview, "Need can only be rejected from UnderReview status.");

        need.Status = NeedStatus.Rejected;
        need.RejectionReason = reason;
        need.ReviewedAt = DateTime.UtcNow;
        need.ReviewedByUserId = reviewedByUserId;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(sessionId, id);
    }

    public async Task<TeachingNeed?> ReviseAsync(int sessionId, int id)
    {
        var need = await _db.TeachingNeeds
            .FirstOrDefaultAsync(n => n.SessionId == sessionId && n.Id == id);

        if (need is null) return null;

        EnsureStatus(need, NeedStatus.Rejected, "Need can only be revised from Rejected status.");

        need.Status = NeedStatus.Draft;
        need.RejectionReason = null;
        need.ReviewedAt = null;
        need.ReviewedByUserId = null;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(sessionId, id);
    }

    private static void EnsureStatus(TeachingNeed need, NeedStatus expectedStatus, string message)
    {
        if (need.Status != expectedStatus)
            throw new InvalidOperationException(message);
    }
}
