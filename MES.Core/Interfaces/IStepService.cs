namespace MES.Core.Interfaces;

public interface IStepService
{
    Task StartStepAsync(int stepExecutionId, string operatorName);
    Task FinishStepAsync(int stepExecutionId, string operatorName);
    Task FailQcAsync(int stepExecutionId, string operatorName, string reason);
    Task PassQcAsync(int stepExecutionId, string operatorName);
}