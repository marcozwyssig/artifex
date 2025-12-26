using System.Linq.Expressions;
using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.Shared.Domain.Specifications;

namespace Artifex.DeviceManagement.Domain.Specifications;

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
