using Xunit;

// Los tests de integración comparten variables de entorno de proceso para inyectar
// el connection string del Testcontainer; se deshabilita la paralelización para
// evitar que dos fixtures (con contenedores distintos) se pisen entre sí.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
