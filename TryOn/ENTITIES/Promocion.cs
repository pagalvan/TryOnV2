using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITIES
{
    public class Promocion
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int? PrendaId { get; set; }
        public int? CategoriaId { get; set; }
        public string CodigoPromocion { get; set; }
        public bool Activa { get; set; }

        // Propiedades de navegación
        public Prenda Prenda { get; set; }
        public Categoria Categoria { get; set; }

        public Promocion()
        {
            FechaInicio = DateTime.Now;
            FechaFin = DateTime.Now.AddDays(7);
            Activa = true;
            CodigoPromocion = GenerarCodigoAleatorio();
        }

        private string GenerarCodigoAleatorio()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
