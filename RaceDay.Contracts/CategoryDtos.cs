namespace RaceDay.Contracts;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryType { get; set; } = string.Empty;
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? EntryFee { get; set; }
}