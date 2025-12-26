using System.Linq.Expressions;
using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.Shared.Domain.Specifications;

namespace Artifex.DeviceManagement.Domain.Specifications;

/// <summary>
/// Specification for filtering devices by type
/// </summary>
public class DeviceByTypeSpecification : Specification<Device>
{
    private readonly DeviceType _type;

    public DeviceByTypeSpecification(DeviceType type)
    {
        _type = type;
    }

    public override Expression<Func<Device, bool>> ToExpression()
    {
        return device => device.Type == _type;
    }
}
