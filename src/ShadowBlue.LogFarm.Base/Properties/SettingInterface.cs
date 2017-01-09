namespace ShadowBlue.LogFarm.Base.Properties
{
    public interface ISettings
    {
        string ApplicationName { get; }
        string ElmahTableName { get; }
        string EnableElmahDynamoDb { get; }
    }

    public sealed partial class Settings : ISettings
    {    
    }
}
