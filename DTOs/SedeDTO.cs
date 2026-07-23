namespace FrontendAdministrativo.DTOs
{
    public class SedeDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public int CapacidadAprox { get; set; }
    }
}
