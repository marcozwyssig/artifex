using System.Linq.Expressions;
using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.Shared.Domain.Specifications;

namespace Artifex.DeviceManagement.Domain.Specifications;

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
