namespace MES.Core.Entities;

public class StepDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }

    // Navigation properties
    public ICollection<StepExecution> StepExecutions { get; set; } = [];
}