namespace Library.API.Services
{
    public interface ITypeHelperService
    {
        bool TypeHasProperty<T>(string fields);
    }
}