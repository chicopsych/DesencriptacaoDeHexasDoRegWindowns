namespace DesencriptacaoDeHexasDoRegWindowns.Models
{
    public enum AppMode
    {
        None,
        Hex,
        Registry,
        Interactive,
        Help
    }

    public sealed class AppOptions
    {
        public AppMode Mode { get; set; } = AppMode.None;
        public string? HexValue { get; set; }
        public string? RegistryPath { get; set; }
        public string? RegistryValueName { get; set; }
        public bool Verbose { get; set; }
    }
}
