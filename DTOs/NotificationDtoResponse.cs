namespace taskmanager.DTOs
{
    public class NotificationDtoResponse
    {
        public Guid Id { get; init; }
        public string? Message { get; init; }
        public Guid TaskItemId { get; init; }
        public DateTime? ReadAt { get; init; }
        public DateTime? CreatedAt { get; init; }
    }
}
