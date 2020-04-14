namespace LarkspurEmberWebProvider.Models
{
    public class ApplicationSettings
    {
        public string Name { get; set; }
        public string LogFolder { get; set; }
        public string ReleaseDate { get; set; }
        public string Version { get; set; }
        public string Environment { get; set; }
        public string Server { get; set; }

        public ApplicationSettingsEmberTree EmberTree { get; set; } = new ApplicationSettingsEmberTree();
    }

    public class ApplicationSettingsEmberTree
    {
        public int Port { get; set; } = 9003;
        public string Identifier { get; set; } = "Larkspur";
        public string Description { get; set; } = "Larkspur";
        public string Product { get; set; } = "Larkspur EmBER+ Provider";
        public string Company { get; set; } = "IRIS Broadcast";
        public string Version { get; set; } = "0.0.1";
    }
}