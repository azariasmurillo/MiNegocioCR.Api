using FluentAssertions;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Domain.Enums;
using Xunit;

namespace MiNegocioCR.Tests.Application.Common;

public class InternetOrderStatusRulesTests
{
    [Theory]
    [InlineData(InternetOrderStatus.Created, InternetOrderStatus.Purchased, true)]
    [InlineData(InternetOrderStatus.Purchased, InternetOrderStatus.Received, true)]
    [InlineData(InternetOrderStatus.Received, InternetOrderStatus.Delivered, true)]
    [InlineData(InternetOrderStatus.Created, InternetOrderStatus.Cancelled, true)]
    [InlineData(InternetOrderStatus.Created, InternetOrderStatus.Delivered, false)]
    [InlineData(InternetOrderStatus.Delivered, InternetOrderStatus.Purchased, false)]
    public void IsValidTransition(InternetOrderStatus from, InternetOrderStatus to, bool expected)
    {
        InternetOrderStatusRules.IsValidTransition(from, to).Should().Be(expected);
    }
}
