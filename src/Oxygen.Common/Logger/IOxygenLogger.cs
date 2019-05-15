namespace Oxygen.Common.Logger
{
    public interface IOxygenLogger
    {
        void LogError(string message);
        void LogInfo(string message);
    }
}
