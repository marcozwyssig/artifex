using System.Linq.Expressions;
using Maestro.DeviceManagement.Domain.Aggregates;
using Maestro.DeviceManagement.Domain.Enums;
using Maestro.Shared.Domain.Specifications;

namespace Maestro.DeviceManagement.Domain.Specifications;

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
