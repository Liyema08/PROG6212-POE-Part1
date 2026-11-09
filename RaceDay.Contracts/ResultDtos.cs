namespace RaceDay.Contracts;

public class ResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
    public int OverallPosition { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public EventDto? Event { get; set; }
    public CategoryDto? Category { get; set; }
}

public class CreateResultRequest
{
    public int EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
    public int OverallPosition { get; set; }
    public string Status { get; set; } = "Finished";
}