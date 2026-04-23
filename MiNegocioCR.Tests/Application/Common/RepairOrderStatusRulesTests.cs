using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Domain.Enums;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class RepairOrderStatusRulesTests
{
    [Theory]
    [InlineData(RepairOrderStatus.Pending, RepairOrderStatus.InProcess, true)]
    [InlineData(RepairOrderStatus.Pending, RepairOrderStatus.Cancelled, true)]
    [InlineData(RepairOrderStatus.InProcess, RepairOrderStatus.Processed, true)]
    [InlineData(RepairOrderStatus.InProcess, RepairOrderStatus.Cancelled, true)]
    [InlineData(RepairOrderStatus.Processed, RepairOrderStatus.Delivered, true)]
    [InlineData(RepairOrderStatus.Pending, RepairOrderStatus.Pending, true)]
    [InlineData(RepairOrderStatus.Pending, RepairOrderStatus.Processed, false)]
    [InlineData(RepairOrderStatus.Pending, RepairOrderStatus.Delivered, false)]
    [InlineData(RepairOrderStatus.InProcess, RepairOrderStatus.Delivered, false)]
    [InlineData(RepairOrderStatus.InProcess, RepairOrderStatus.InProcess, true)]
    [InlineData(RepairOrderStatus.Processed, RepairOrderStatus.Processed, true)]
    [InlineData(RepairOrderStatus.Processed, RepairOrderStatus.Cancelled, false)]
    [InlineData(RepairOrderStatus.Delivered, RepairOrderStatus.InProcess, false)]
    [InlineData(RepairOrderStatus.Delivered, RepairOrderStatus.Pending, false)]
    [InlineData(RepairOrderStatus.Cancelled, RepairOrderStatus.Pending, false)]
    public void IsValidTransition(RepairOrderStatus from, RepairOrderStatus to, bool expected)
    {
        RepairOrderStatusRules.IsValidTransition(from, to).Should().Be(expected);
    }
}
