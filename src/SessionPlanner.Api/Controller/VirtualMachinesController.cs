using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionPlanner.Core.Entities;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Api.Dtos.VirtualMachines;
using SessionPlanner.Api.Mappings;
using Asp.Versioning;

namespace SessionPlanner.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class VirtualMachinesController : ControllerBase
{
    private readonly AppDbContext _db;

    public VirtualMachinesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<VirtualMachineResponse>> Create(CreateVirtualMachineRequest request)
    {
        var os = await _db.OperatingSystems.FindAsync(request.OSId);
        if (os is null)
            return BadRequest("Operating system not found");

        if (request.HostServerId.HasValue)
        {
            var hostServer = await _db.PhysicalServers.FindAsync(request.HostServerId);
            if (hostServer is null)
                return BadRequest("Host server not found");
        }

        var vm = new VirtualMachine
        {
            Quantity = request.Quantity,
            CpuCores = request.CpuCores,
            RamGb = request.RamGb,
            StorageGb = request.StorageGb,
            AccessType = request.AccessType,
            Notes = request.Notes,
            OSId = request.OSId,
            HostServerId = request.HostServerId
        };

        _db.VirtualMachines.Add(vm);
        await _db.SaveChangesAsync();

        await _db.Entry(vm).Reference(v => v.OS).LoadAsync();
        if (vm.HostServerId.HasValue)
            await _db.Entry(vm).Reference(v => v.HostServer).LoadAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = vm.Id },
            vm.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VirtualMachineResponse>>> GetAll()
    {
        var vms = await _db.VirtualMachines
            .Include(v => v.OS)
            .Include(v => v.HostServer)
            .ToListAsync();
        return Ok(vms.Select(v => v.ToResponse()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VirtualMachineResponse>> GetById(int id)
    {
        var vm = await _db.VirtualMachines
            .Include(v => v.OS)
            .Include(v => v.HostServer)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (vm is null)
            return NotFound();
        return Ok(vm.ToResponse());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateVirtualMachineRequest request)
    {
        var vm = await _db.VirtualMachines.FindAsync(id);

        if (vm is null)
            return NotFound();

        var os = await _db.OperatingSystems.FindAsync(request.OSId);
        if (os is null)
            return BadRequest("Operating system not found");

        if (request.HostServerId.HasValue)
        {
            var hostServer = await _db.PhysicalServers.FindAsync(request.HostServerId);
            if (hostServer is null)
                return BadRequest("Host server not found");
        }

        vm.Quantity = request.Quantity;
        vm.CpuCores = request.CpuCores;
        vm.RamGb = request.RamGb;
        vm.StorageGb = request.StorageGb;
        vm.AccessType = request.AccessType;
        vm.Notes = request.Notes;
        vm.OSId = request.OSId;
        vm.HostServerId = request.HostServerId;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _db.VirtualMachines.FindAsync(id);

        if (vm is null)
            return NotFound();

        _db.VirtualMachines.Remove(vm);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
