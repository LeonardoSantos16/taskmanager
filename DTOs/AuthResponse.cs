namespace taskmanager.DTOs
{
    public record AuthResponse
    {
        public required string Token { get; init; }
        public required DateTime ExpiresAtUtc { get; init; }
        public required UserDtoResponse User { get; init; }
    }
}
