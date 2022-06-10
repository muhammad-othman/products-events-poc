namespace ProductsService.Data
{
    public interface IProductsEventsStore
    {
        Task<List<Product>> GetProducts();
        Task<Product> AddProducts(Product product);
        Task<Product> UpdateProducts(Product product);
        Task DeleteProducts(Guid id);
    }
}
