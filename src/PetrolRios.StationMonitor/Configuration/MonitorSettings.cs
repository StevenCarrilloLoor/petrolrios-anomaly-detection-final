namespace PetrolRios.StationMonitor.Configuration;

public sealed class MonitorSettings
{
    public const string SectionName = "Monitor";

    public string CodigoEstacion { get; set; } = "EST-001";
    public string ServerUrl { get; set; } = "http://localhost:5170";
    public string Email { get; set; } = "agent-est-001@petrolrios.com";
    public string Password { get; set; } = "";
    public int IntervaloSegundos { get; set; } = 15;
    public int DiasConsulta { get; set; } = 30;
    public int PanelPuerto { get; set; } = 5190;
    public bool Configurado { get; set; }

    public MonitorSettings Clonar() => (MonitorSettings)MemberwiseClone();
}
