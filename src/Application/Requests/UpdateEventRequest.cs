namespace EventManagement.Application.Requests
{
    public record UpdateEventRequest(string Title, string Description, DateTime StartAt, DateTime EndAt);
}