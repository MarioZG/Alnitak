namespace Alnitak.Services
{
    public interface IServiceFactory
    {
        T GetService<T>();
    }
}