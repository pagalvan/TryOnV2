using DAL;
using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryOn.BLL;

namespace TryOn.BLL
{
    public class PromocionService : IService<Promocion>
    {
        private readonly PromocionRepository _repository;

        public PromocionService()
        {
            _repository = new PromocionRepository();
        }

        public void Add(Promocion promocion)
        {
            // Validar datos
            if (string.IsNullOrEmpty(promocion.Titulo))
                throw new Exception("El título es obligatorio");

            if (promocion.PorcentajeDescuento <= 0 || promocion.PorcentajeDescuento > 100)
                throw new Exception("El porcentaje de descuento debe estar entre 1 y 100");

            if (promocion.FechaInicio > promocion.FechaFin)
                throw new Exception("La fecha de inicio debe ser anterior a la fecha de fin");

            if (string.IsNullOrEmpty(promocion.CodigoPromocion))
                throw new Exception("El código de promoción es obligatorio");

            _repository.Add(promocion);
        }

        public void Delete(int id)
        {
            var promocion = _repository.GetById(id);
            if (promocion == null)
                throw new Exception("Promoción no encontrada");

            _repository.Delete(id);
        }

        public IEnumerable<Promocion> Find(Func<Promocion, bool> predicate)
        {
            return _repository.Find(predicate);
        }

        public IEnumerable<Promocion> GetAll()
        {
            return _repository.GetAll();
        }

        public Promocion GetById(int id)
        {
            return _repository.GetById(id);
        }

        public IEnumerable<Promocion> GetPromocionesActivas()
        {
            return _repository.GetPromocionesActivas();
        }

        public void Update(Promocion promocion)
        {
            var existingPromocion = _repository.GetById(promocion.Id);
            if (existingPromocion == null)
                throw new Exception("Promoción no encontrada");

            // Validar datos
            if (string.IsNullOrEmpty(promocion.Titulo))
                throw new Exception("El título es obligatorio");

            if (promocion.PorcentajeDescuento <= 0 || promocion.PorcentajeDescuento > 100)
                throw new Exception("El porcentaje de descuento debe estar entre 1 y 100");

            if (promocion.FechaInicio > promocion.FechaFin)
                throw new Exception("La fecha de inicio debe ser anterior a la fecha de fin");

            if (string.IsNullOrEmpty(promocion.CodigoPromocion))
                throw new Exception("El código de promoción es obligatorio");

            _repository.Update(promocion);
        }
    }
}
