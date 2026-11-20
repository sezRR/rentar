using Rentar.Shared.Kernel.Exceptions;
using System;
using Xunit;

namespace Rentar.Domain.UnitTests;

/// <summary>Asserts that the domain refuses something, and says why with the message it is meant to.</summary>
internal static class Refused
{
    public static void With(string message, Func<object> build)
    {
        var refusal = Assert.Throws<BusinessRuleException>(build);
        Assert.Equal(message, refusal.Message);
    }

    public static void With(string message, Action check)
    {
        var refusal = Assert.Throws<BusinessRuleException>(check);
        Assert.Equal(message, refusal.Message);
    }
}
