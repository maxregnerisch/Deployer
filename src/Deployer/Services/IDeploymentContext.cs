namespace Deployer.Services
{
    public interface IDeploymentContext
    {
        string WorkFolder { get; }
        string ImagePath { get; set; }
        Device Device { get; set; }
        bool DualBoot { get; set; }
    }
}

