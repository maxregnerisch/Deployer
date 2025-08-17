using System;

namespace Deployer.Exceptions
{
    public class DeploymentException : Exception
    {
        public DeploymentException(string message) : base(message)
        {
        }

        public DeploymentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

