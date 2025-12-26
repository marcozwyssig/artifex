using System.Linq.Expressions;
using Maestro.DeviceManagement.Domain.Aggregates;
using Maestro.DeviceManagement.Domain.Enums;
using Maestro.Shared.Domain.Specifications;

namespace Maestro.DeviceManagement.Domain.Specifications;

/// <summary>
/// Specification for finding all online devices
/// </summary>
public class OnlineDevicesSpecification : Specification<Device>
{
    public override Expression<Func<Device, bool>> ToExpression()
    {
        return device => device.Status == DeviceStatus.Online;
    }
}
