using ENTITIES;
using System;
using System.Collections.Generic;
using TryOn.DAL;

namespace TryOn.BLL
{
    public class PrendaService : IService<Prenda>
    {
        private readonly PrendaRepository _repository;

        public PrendaService()
        {
            _repository = new PrendaRepository();
        }

        public void Add(Prenda prenda)
        {
            // Validar datos
            if (string.IsNullOrEmpty(prenda.Codigo))
                throw new Exception("El código es obligatorio");

            if (string.IsNullOrEmpty(prenda.Nombre))
                throw new Exception("El nombre es obligatorio");

            if (prenda.PrecioVenta <= 0)
                throw new Exception("El precio de venta debe ser mayor a cero");

            if (prenda.Costo <= 0)
                throw new Exception("El costo debe ser mayor a cero");

            if (prenda.CategoriaId <= 0)
                throw new Exception("Debe seleccionar una categoría");

            _repository.Add(prenda);
        }

        public void Delete(int id)
        {
            var prenda = _repository.GetById(id);
            if (prenda == null)
                throw new Exception("Prenda no encontrada");

            _repository.Delete(id);
        }

        public IEnumerable<Prenda> Find(Func<Prenda, bool> predicate)
        {
            return _repository.Find(predicate);
        }

        public IEnumerable<Prenda> GetAll()
        {
            return _repository.GetAll();
        }

        public Prenda GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Update(Prenda prenda)
        {
            var existingPrenda = _repository.GetById(prenda.Id);
            if (existingPrenda == null)
                throw new Exception("Prenda no encontrada");

            // Validar datos
            if (string.IsNullOrEmpty(prenda.Codigo))
                throw new Exception("El código es obligatorio");

            if (string.IsNullOrEmpty(prenda.Nombre))
                throw new Exception("El nombre es obligatorio");

            if (prenda.PrecioVenta <= 0)
                throw new Exception("El precio de venta debe ser mayor a cero");

            if (prenda.Costo <= 0)
                throw new Exception("El costo debe ser mayor a cero");

            if (prenda.CategoriaId <= 0)
                throw new Exception("Debe seleccionar una categoría");

            _repository.Update(prenda);
        }
    }
}
