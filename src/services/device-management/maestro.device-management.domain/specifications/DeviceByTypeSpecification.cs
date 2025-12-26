using System.Linq.Expressions;
using Maestro.DeviceManagement.Domain.Aggregates;
using Maestro.DeviceManagement.Domain.Enums;
using Maestro.Shared.Domain.Specifications;

namespace Maestro.DeviceManagement.Domain.Specifications;

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
