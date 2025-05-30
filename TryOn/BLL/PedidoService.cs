using ENTITIES;
using System;
using System.Collections.Generic;
using TryOn.DAL;

namespace TryOn.BLL
{
    public class PedidoService : IService<Pedido>
    {
        private readonly PedidoRepository _repository;
        private readonly InventarioService _inventarioService;

        public PedidoService()
        {
            _repository = new PedidoRepository();
            _inventarioService = new InventarioService();
        }

        public void Add(Pedido pedido)
        {
            // Validar datos
            if (pedido.UsuarioId <= 0)
                throw new Exception("Debe seleccionar un usuario");

            if (pedido.Detalles == null || pedido.Detalles.Count == 0)
                throw new Exception("El pedido debe tener al menos un producto");

            // Verificar stock disponible y calcular total
            decimal total = 0;
            foreach (var detalle in pedido.Detalles)
            {
                var inventario = _inventarioService.GetById(detalle.InventarioId);
                if (inventario == null)
                    throw new Exception($"Inventario con ID {detalle.InventarioId} no encontrado");

                if (inventario.Cantidad < detalle.Cantidad)
                    throw new Exception($"No hay suficiente stock de {inventario.Prenda.Nombre} en talla {inventario.Talla} y color {inventario.Color}");

                detalle.PrecioUnitario = inventario.Prenda.PrecioVenta;
                detalle.Subtotal = detalle.PrecioUnitario * detalle.Cantidad;
                total += detalle.Subtotal;
            }

            pedido.Total = total;
            _repository.Add(pedido);
        }

        public void Delete(int id)
        {
            var pedido = _repository.GetById(id);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");

            _repository.Delete(id);
        }

        public IEnumerable<Pedido> Find(Func<Pedido, bool> predicate)
        {
            return _repository.Find(predicate);
        }

        public IEnumerable<Pedido> GetAll()
        {
            return _repository.GetAll();
        }

        public Pedido GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Update(Pedido pedido)
        {
            var existingPedido = _repository.GetById(pedido.Id);
            if (existingPedido == null)
                throw new Exception("Pedido no encontrado");

            _repository.Update(pedido);
        }

        public void CambiarEstado(int pedidoId, string nuevoEstado)
        {
            var pedido = _repository.GetById(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");

            pedido.Estado = nuevoEstado;
            _repository.Update(pedido);
        }
    }
}