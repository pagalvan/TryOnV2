using DAL;
using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using TryOn.DAL;

namespace TryOn.BLL
{
    public class CategoryService : IService<Categoria>
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryService()
        {
            _categoryRepository = new CategoryRepository();
        }

        public IEnumerable<Categoria> GetAll()
        {
            try
            {
                return _categoryRepository.GetAll();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener las categorías: {ex.Message}", ex);
            }
        }

        public Categoria GetById(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID debe ser mayor a 0");

                return _categoryRepository.GetById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la categoría: {ex.Message}", ex);
            }
        }
        public IEnumerable<Categoria> Find(Func<Categoria, bool> predicate)
        {
            return _categoryRepository.Find(predicate);
        }
        public Categoria GetByName(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    throw new ArgumentException("El nombre de la categoría es requerido");

                return _categoryRepository.GetByName(nombre.Trim());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar la categoría: {ex.Message}", ex);
            }
        }

        public Categoria CreateOrGetCategory(string nombre, string descripcion = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                    throw new ArgumentException("El nombre de la categoría es requerido");

                // Limpiar el nombre de espacios en blanco
                nombre = nombre.Trim();

                // Primero verificar si ya existe (búsqueda más robusta)
                var existingCategory = _categoryRepository.GetByName(nombre);
                if (existingCategory != null)
                {
                    return existingCategory;
                }

                // Verificar nuevamente con ExistsByName para estar seguros
                if (_categoryRepository.ExistsByName(nombre))
                {
                    // Si existe pero GetByName no la encontró, intentar obtenerla de nuevo
                    var allCategories = _categoryRepository.GetAll();
                    var foundCategory = allCategories.FirstOrDefault(c =>
                        string.Equals(c.Nombre.Trim(), nombre, StringComparison.OrdinalIgnoreCase));

                    if (foundCategory != null)
                    {
                        return foundCategory;
                    }
                }

                // Si definitivamente no existe, crear nueva categoría
                var newCategory = new Categoria
                {
                    Nombre = nombre,
                    Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim()
                };

                _categoryRepository.Add(newCategory);
                return newCategory;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear o obtener la categoría: {ex.Message}", ex);
            }
        }

        public void Add(Categoria categoria)
        {
            try
            {
                ValidateCategory(categoria);

                if (_categoryRepository.ExistsByName(categoria.Nombre))
                    throw new InvalidOperationException($"Ya existe una categoría con el nombre '{categoria.Nombre}'");

                _categoryRepository.Add(categoria);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al insertar la categoría: {ex.Message}", ex);
            }
        }

        public void Update(Categoria categoria)
        {
            try
            {
                ValidateCategory(categoria);

                if (categoria.Id <= 0)
                    throw new ArgumentException("El ID debe ser mayor a 0");

                if (_categoryRepository.ExistsByName(categoria.Nombre, categoria.Id))
                    throw new InvalidOperationException($"Ya existe una categoría con el nombre '{categoria.Nombre}'");

                _categoryRepository.Update(categoria);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar la categoría: {ex.Message}", ex);
            }
        }

        public void Delete(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID debe ser mayor a 0");

                _categoryRepository.Delete(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar la categoría: {ex.Message}", ex);
            }
        }

        private void ValidateCategory(Categoria categoria)
        {
            if (categoria == null)
                throw new ArgumentNullException(nameof(categoria));

            if (string.IsNullOrWhiteSpace(categoria.Nombre))
                throw new ArgumentException("El nombre de la categoría es requerido");

            // Limpiar espacios en blanco
            categoria.Nombre = categoria.Nombre.Trim();

            if (categoria.Nombre.Length > 100) // Asumiendo un límite de 100 caracteres
                throw new ArgumentException("El nombre de la categoría no puede exceder 100 caracteres");

            if (!string.IsNullOrEmpty(categoria.Descripcion))
            {
                categoria.Descripcion = categoria.Descripcion.Trim();
                if (categoria.Descripcion.Length > 500)
                    throw new ArgumentException("La descripción no puede exceder 500 caracteres");
            }
        }
    }
}
