using System.Linq.Expressions;
using Maestro.DeviceManagement.Domain.Aggregates;
using Maestro.DeviceManagement.Domain.Enums;
using Maestro.Shared.Domain.Specifications;

namespace Maestro.DeviceManagement.Domain.Specifications;

/// <summary>
/// Specification for filtering devices by network segment
/// </summary>
public class DeviceByNetworkSegmentSpecification : Specification<Device>
{
    private readonly NetworkSegment _segment;

    public DeviceByNetworkSegmentSpecification(NetworkSegment segment)
    {
        _segment = segment;
    }

    public override Expression<Func<Device, bool>> ToExpression()
    {
        return device => device.NetworkSegment == _segment;
    }
}
