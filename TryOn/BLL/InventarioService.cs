using ENTITIES;
using System;
using System.Collections.Generic;
using TryOn.DAL;

namespace TryOn.BLL
{
    public class InventarioService : IService<Inventario>
    {
        private readonly InventarioRepository _repository;

        public InventarioService()
        {
            _repository = new InventarioRepository();
        }

        public void Add(Inventario inventario)
        {
            // Validar datos
            if (inventario.PrendaId <= 0)
                throw new Exception("Debe seleccionar una prenda");

            if (string.IsNullOrEmpty(inventario.Talla))
                throw new Exception("La talla es obligatoria");

            if (string.IsNullOrEmpty(inventario.Color))
                throw new Exception("El color es obligatorio");

            if (inventario.Cantidad < 0)
                throw new Exception("La cantidad no puede ser negativa");

            _repository.Add(inventario);
        }

        public void Delete(int id)
        {
            var inventario = _repository.GetById(id);
            if (inventario == null)
                throw new Exception("Inventario no encontrado");

            _repository.Delete(id);
        }

        public IEnumerable<Inventario> Find(Func<Inventario, bool> predicate)
        {
            return _repository.Find(predicate);
        }

        public IEnumerable<Inventario> GetAll()
        {
            return _repository.GetAll();
        }

        public Inventario GetById(int id)
        {
            return _repository.GetById(id);
        }

        public IEnumerable<Inventario> GetByPrendaId(int prendaId)
        {
            return _repository.GetByPrendaId(prendaId);
        }

        public void Update(Inventario inventario)
        {
            var existingInventario = _repository.GetById(inventario.Id);
            if (existingInventario == null)
                throw new Exception("Inventario no encontrado");

            // Validar datos
            if (inventario.PrendaId <= 0)
                throw new Exception("Debe seleccionar una prenda");

            if (string.IsNullOrEmpty(inventario.Talla))
                throw new Exception("La talla es obligatoria");

            if (string.IsNullOrEmpty(inventario.Color))
                throw new Exception("El color es obligatorio");

            if (inventario.Cantidad < 0)
                throw new Exception("La cantidad no puede ser negativa");

            _repository.Update(inventario);
        }

        public void AjustarStock(int inventarioId, int cantidad)
        {
            var inventario = _repository.GetById(inventarioId);
            if (inventario == null)
                throw new Exception("Inventario no encontrado");

            inventario.Cantidad += cantidad;
            if (inventario.Cantidad < 0)
                throw new Exception("No hay suficiente stock disponible");

            _repository.Update(inventario);
        }
    }
}
