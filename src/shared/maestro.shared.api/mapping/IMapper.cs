namespace Maestro.Shared.Api.Mapping;

/// <summary>
/// Interface for object mapping
/// </summary>
public interface IMapper
{
    TDestination Map<TSource, TDestination>(TSource source);
    TDestination Map<TDestination>(object source);
    void Map<TSource, TDestination>(TSource source, TDestination destination);
}
