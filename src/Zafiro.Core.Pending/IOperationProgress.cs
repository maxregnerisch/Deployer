using System;

namespace Zafiro.Core
{
    public interface IOperationProgress
    {
        void Report(double percentage, string message = null);
        void SetIndeterminate(string message = null);
        void SetCompleted(string message = "Completed");
        void SetFailed(string message = "Failed");
        void Reset();
    }
}

