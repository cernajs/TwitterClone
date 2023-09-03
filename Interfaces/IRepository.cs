namespace TwitterClone.Data;

public interface IRepository<T> {
    Task<T> GetByIdAsync(string id);
    Task<IEnumerable<T>> ListAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}