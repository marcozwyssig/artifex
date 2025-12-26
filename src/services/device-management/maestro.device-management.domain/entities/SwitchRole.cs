using Maestro.Shared.Domain;

namespace Maestro.DeviceManagement.Domain.Entities;

/// <summary>
/// Switch role entity - handles layer 2/3 switching
/// </summary>
public class SwitchRole : DeviceRole
{
    public bool IsLayer3Switch { get; private set; }
    public bool IsAccessSwitch { get; private set; }
    public bool IsDistributionSwitch { get; private set; }
    public bool IsCoreSwitch { get; private set; }
    public int NumberOfPorts { get; private set; }
    public int NumberOfVlans { get; private set; }
    public bool SupportsSpanningTree { get; private set; }
    public bool SupportsLACP { get; private set; }
    public bool SupportsStackable { get; private set; }

    // EF Core requires a parameterless constructor
    private SwitchRole() : base()
    {
    }

    private SwitchRole(
        Guid id,
        Guid deviceId,
        bool isLayer3Switch,
        bool isAccessSwitch,
        bool isDistributionSwitch,
        bool isCoreSwitch,
        int numberOfPorts,
        int numberOfVlans,
        bool supportsSpanningTree,
        bool supportsLACP,
        bool supportsStackable,
        string? description = null)
        : base(id, deviceId, "Switch", description)
    {
        IsLayer3Switch = isLayer3Switch;
        IsAccessSwitch = isAccessSwitch;
        IsDistributionSwitch = isDistributionSwitch;
        IsCoreSwitch = isCoreSwitch;
        NumberOfPorts = numberOfPorts;
        NumberOfVlans = numberOfVlans;
        SupportsSpanningTree = supportsSpanningTree;
        SupportsLACP = supportsLACP;
        SupportsStackable = supportsStackable;
    }

    public static Result<SwitchRole> Create(
        Guid deviceId,
        bool isLayer3Switch = false,
        bool isAccessSwitch = false,
        bool isDistributionSwitch = false,
        bool isCoreSwitch = false,
        int numberOfPorts = 24,
        int numberOfVlans = 1,
        bool supportsSpanningTree = true,
        bool supportsLACP = false,
        bool supportsStackable = false,
        string? description = null)
    {
        if (numberOfPorts < 1)
        {
            return Result.Failure<SwitchRole>("Switch must have at least one port");
        }

        if (numberOfVlans < 1)
        {
            return Result.Failure<SwitchRole>("Switch must support at least one VLAN");
        }

        var role = new SwitchRole(
            Guid.NewGuid(),
            deviceId,
            isLayer3Switch,
            isAccessSwitch,
            isDistributionSwitch,
            isCoreSwitch,
            numberOfPorts,
            numberOfVlans,
            supportsSpanningTree,
            supportsLACP,
            supportsStackable,
            description);

        var validationResult = role.Validate();
        if (validationResult.IsFailure)
        {
            return Result.Failure<SwitchRole>(validationResult.Error);
        }

        return Result.Success(role);
    }

    public override Result Validate()
    {
        var roleCount = new[] { IsAccessSwitch, IsDistributionSwitch, IsCoreSwitch }.Count(r => r);
        if (roleCount > 1)
        {
            return Result.Failure("Switch can only have one hierarchical role (access, distribution, or core)");
        }

        return Result.Success();
    }

    public override IReadOnlyCollection<int> GetRequiredPorts()
    {
        return new List<int> { 22, 80, 161, 443 }.AsReadOnly();
    }
}
