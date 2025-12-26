using System.Linq.Expressions;
using Maestro.DeviceManagement.Domain.Aggregates;
using Maestro.DeviceManagement.Domain.Enums;
using Maestro.Shared.Domain.Specifications;

namespace Maestro.DeviceManagement.Domain.Specifications;

/// <summary>
/// Specification for filtering devices by vendor
/// </summary>
public class DeviceByVendorSpecification : Specification<Device>
{
    private readonly Vendor _vendor;

    public DeviceByVendorSpecification(Vendor vendor)
    {
        _vendor = vendor;
    }

    public override Expression<Func<Device, bool>> ToExpression()
    {
        return device => device.Vendor == _vendor;
    }
}
