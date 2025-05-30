
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITIES
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public bool EsAdmin { get; set; }
        public DateTime FechaRegistro { get; set; }

        public Usuario()
        {
            FechaRegistro = DateTime.Now;
        }

        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
