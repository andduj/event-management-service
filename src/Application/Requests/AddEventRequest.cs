namespace EventManagement.Application.Requests
{
    public record AddEventRequest(string Title, string Description, DateTime StartAt, DateTime EndAt);
}