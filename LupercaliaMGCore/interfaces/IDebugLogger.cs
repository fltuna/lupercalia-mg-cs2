namespace LupercaliaMGCore.interfaces;

public interface IDebugLogger
{
    
    public void LogDebug(string information);
    
    public void LogTrace(string information);
    
    public void LogError(string information);
    
    public void LogWarning(string information);
    
    public void LogInformation(string information);
}