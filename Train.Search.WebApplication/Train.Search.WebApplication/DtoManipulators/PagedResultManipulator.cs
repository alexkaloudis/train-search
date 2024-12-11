using Train.Search.WebApplication.ExternalHttpServices;
using Train.Search.WebApplication.Pagination;
using Train.Search.WebApplication.Search;

namespace Train.Search.WebApplication.DtoManipulators;

public class PagedResultManipulator
{
    public PagedResult<RailHttpClient.Disturbance> TokenizedDisturbanceSearchResultToPagedResult(
        List<TokenizedDisturbanceSearchResult> dataBefore,
        Page page)
    {
        var disturbancesList = dataBefore
            .Skip(page.PageIndex * page.PageSize)
            .Take(page.PageSize)
            .Select(t => t.Disturbance)
            .ToList();
        var dataAfter = new PagedResult<RailHttpClient.Disturbance>
        {
            Items = disturbancesList,
            TotalCount = dataBefore.Count
        };

        return dataAfter;
    }
    
    public PagedResult<RailHttpClient.Disturbance> TokenizedDisturbanceSearchResultToPagedResultSortedByTokenRelevance(
        List<TokenizedDisturbanceSearchResult> dataBefore,
        Page page)
    {
        var disturbancesList = dataBefore
            .Skip(page.PageIndex * page.PageSize)
            .Take(page.PageSize)
            .OrderByDescending(d => d.Score)
            .Select(t => t.Disturbance)
            .ToList();
        var dataAfter = new PagedResult<RailHttpClient.Disturbance>
        {
            Items = disturbancesList,
            TotalCount = dataBefore.Count
        };

        return dataAfter;
    }
    
    public PagedResult<RailHttpClient.Disturbance> TokenizedDisturbanceSearchResultToPagedResultSortedByTitle(
        List<TokenizedDisturbanceSearchResult> dataBefore,
        Page page)
    {
        var disturbancesList = dataBefore
            .Skip(page.PageIndex * page.PageSize)
            .Take(page.PageSize)
            .Select(t => t.Disturbance)
            .OrderByDescending(d => d.Title)
            .ToList();
        var dataAfter = new PagedResult<RailHttpClient.Disturbance>
        {
            Items = disturbancesList,
            TotalCount = dataBefore.Count
        };

        return dataAfter;
    }
    
    public PagedResult<RailHttpClient.Disturbance> TokenizedDisturbanceSearchResultToPagedResultSortedByTimestamp(
        List<TokenizedDisturbanceSearchResult> dataBefore,
        Page page)
    {
        var disturbancesList = dataBefore
            .Skip(page.PageIndex * page.PageSize)
            .Take(page.PageSize)
            .Select(t => t.Disturbance)
            .OrderByDescending(d => d.Timestamp)
            .ToList();
        var dataAfter = new PagedResult<RailHttpClient.Disturbance>
        {
            Items = disturbancesList,
            TotalCount = dataBefore.Count
        };

        return dataAfter;
    }
}