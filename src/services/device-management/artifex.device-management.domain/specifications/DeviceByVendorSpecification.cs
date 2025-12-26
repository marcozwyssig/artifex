using System.Linq.Expressions;
using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.Shared.Domain.Specifications;

namespace Artifex.DeviceManagement.Domain.Specifications;

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
