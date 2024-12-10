using Train.Search.WebApplication.Infrastructure.Pagination;

namespace Train.Search.WebApplication.Infrastructure.ExternalHttpServices;

public interface IRailHttpClient
{
    public Task<HttpResponseMessage> GetAllConnectionsAsync();
    public Task<List<RailHttpClient.Station>> GetAllStationsAsync();
    public Task<List<RailHttpClient.Disturbance>> GetAllDisturbancesAsync();
    public Task<PagedResult<RailHttpClient.Disturbance>> GetAllDisturbancesWithPaginationAsync(Page page);

    public Task<PagedResult<RailHttpClient.Disturbance>> GetAllDisturbancesWithPaginationAndSortedAsync(
        Page page,
        string? sortingCriteria);


}