namespace RaceDay.Contracts;

public class EventDto
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Venue { get; set; }
    public decimal Distance { get; set; }
    public string DistanceUnit { get; set; } = "km";
    public string EventType { get; set; } = string.Empty;
    public string? BannerImageUrl { get; set; }
    public decimal EntryFee { get; set; }
    public DateTime RegistrationDeadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<CategoryDto> Categories { get; set; } = new();
}

public class CreateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Venue { get; set; }
    public decimal Distance { get; set; }
    public string DistanceUnit { get; set; } = "km";
    public string EventType { get; set; } = string.Empty;
    public int MaxParticipants { get; set; } = 100;
    public decimal EntryFee { get; set; } = 0;
    public DateTime RegistrationDeadline { get; set; }
    public string Status { get; set; } = "Upcoming";
}