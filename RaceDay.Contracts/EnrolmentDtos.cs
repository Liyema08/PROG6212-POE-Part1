namespace RaceDay.Contracts;

public class CreateEnrolmentRequest
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public string? Notes { get; set; }
}

public class EnrolmentDto
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? BibNumber { get; set; }
    
    public EventDto? Event { get; set; }
    public CategoryDto? Category { get; set; }
    public ParticipantDto? Participant { get; set; }
}

public class ParticipantDto
{
    public int ParticipantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}