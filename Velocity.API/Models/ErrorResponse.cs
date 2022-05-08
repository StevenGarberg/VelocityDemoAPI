namespace Velocity.API.Models;

public class ErrorResponse
{
    public string Message { get; }

    public ErrorResponse(Exception e)
    {
        Message = e.Message;
    }
}