using System.Linq.Expressions;
using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.Shared.Domain.Specifications;

namespace Artifex.DeviceManagement.Domain.Specifications;

/// <summary>
/// Specification for filtering devices by status
/// </summary>
public class DeviceByStatusSpecification : Specification<Device>
{
    private readonly DeviceStatus _status;

    public DeviceByStatusSpecification(DeviceStatus status)
    {
        _status = status;
    }

    public override Expression<Func<Device, bool>> ToExpression()
    {
        return device => device.Status == _status;
    }
}
