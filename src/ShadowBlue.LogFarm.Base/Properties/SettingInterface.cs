namespace ShadowBlue.LogFarm.Base.Properties
{
    public interface ISettings
    {
        string ApplicationName { get; }
        string ElmahTableName { get; }
    }

    public sealed partial class Settings : ISettings
    {    
    }
}
