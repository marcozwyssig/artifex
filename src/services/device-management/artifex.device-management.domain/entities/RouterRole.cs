using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.Entities;

/// <summary>
/// Router role entity - handles routing between networks
/// </summary>
public class RouterRole : DeviceRole
{
    public bool IsEdgeRouter { get; private set; }
    public bool IsCoreRouter { get; private set; }
    public bool SupportsBGP { get; private set; }
    public bool SupportsOSPF { get; private set; }
    public bool SupportsEIGRP { get; private set; }
    public int NumberOfRoutingTables { get; private set; }

    // EF Core requires a parameterless constructor
    private RouterRole() : base()
    {
    }

    private RouterRole(
        Guid id,
        Guid deviceId,
        bool isEdgeRouter,
        bool isCoreRouter,
        bool supportsBGP,
        bool supportsOSPF,
        bool supportsEIGRP,
        int numberOfRoutingTables,
        string? description = null)
        : base(id, deviceId, "Router", description)
    {
        IsEdgeRouter = isEdgeRouter;
        IsCoreRouter = isCoreRouter;
        SupportsBGP = supportsBGP;
        SupportsOSPF = supportsOSPF;
        SupportsEIGRP = supportsEIGRP;
        NumberOfRoutingTables = numberOfRoutingTables;
    }

    public static Result<RouterRole> Create(
        Guid deviceId,
        bool isEdgeRouter = false,
        bool isCoreRouter = false,
        bool supportsBGP = false,
        bool supportsOSPF = false,
        bool supportsEIGRP = false,
        int numberOfRoutingTables = 1,
        string? description = null)
    {
        if (numberOfRoutingTables < 1)
        {
            return Result.Failure<RouterRole>("Router must have at least one routing table");
        }

        var role = new RouterRole(
            Guid.NewGuid(),
            deviceId,
            isEdgeRouter,
            isCoreRouter,
            supportsBGP,
            supportsOSPF,
            supportsEIGRP,
            numberOfRoutingTables,
            description);

        var validationResult = role.Validate();
        if (validationResult.IsFailure)
        {
            return Result.Failure<RouterRole>(validationResult.Error);
        }

        return Result.Success(role);
    }

    public override Result Validate()
    {
        if (IsEdgeRouter && IsCoreRouter)
        {
            return Result.Failure("Router cannot be both edge and core router");
        }

        return Result.Success();
    }

    public override IReadOnlyCollection<int> GetRequiredPorts()
    {
        var ports = new List<int> { 22, 161 };

        if (SupportsBGP)
        {
            ports.Add(179);
        }

        return ports.AsReadOnly();
    }

    public Result UpdateConfiguration(
        bool? isEdgeRouter = null,
        bool? isCoreRouter = null,
        bool? supportsBGP = null,
        bool? supportsOSPF = null,
        bool? supportsEIGRP = null)
    {
        if (isEdgeRouter.HasValue) IsEdgeRouter = isEdgeRouter.Value;
        if (isCoreRouter.HasValue) IsCoreRouter = isCoreRouter.Value;
        if (supportsBGP.HasValue) SupportsBGP = supportsBGP.Value;
        if (supportsOSPF.HasValue) SupportsOSPF = supportsOSPF.Value;
        if (supportsEIGRP.HasValue) SupportsEIGRP = supportsEIGRP.Value;

        var validationResult = Validate();
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        UpdateModifiedTimestamp();
        return Result.Success();
    }
}
