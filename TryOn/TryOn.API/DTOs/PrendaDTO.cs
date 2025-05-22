namespace TryOn.API.DTOs
{
    public class PrendaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int CategoriaId { get; set; }
        public CategoriaDTO Categoria { get; set; }
        public string ImagenUrl { get; set; }
    }
}