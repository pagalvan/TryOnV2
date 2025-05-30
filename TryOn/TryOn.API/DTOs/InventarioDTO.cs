namespace TryOn.API.DTOs
{
    public class InventarioDTO
    {
        public int Id { get; set; }
        public int PrendaId { get; set; }
        public PrendaDTO Prenda { get; set; }
        public string Talla { get; set; }
        public string Color { get; set; }
        public int Stock { get; set; }
    }
}