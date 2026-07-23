namespace FrontendAdministrativo.DTOs
{
    public class FaseReporteDTO
    {
        public string Fase { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Finalizados { get; set; }
        public int Porcentaje { get; set; }
    }
}
