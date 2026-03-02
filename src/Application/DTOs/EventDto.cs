namespace EventManagement.Application.DTOs
{
    public record EventDto(Guid Id, string Title, string Description, DateTime StartAt, DateTime EndAt);
}
