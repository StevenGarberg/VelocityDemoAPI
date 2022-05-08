namespace Velocity.API.Exceptions;

public class PercentageOverflowException : Exception
{
    public PercentageOverflowException() : base("Sum of percentages higher than 100%.") { }
}