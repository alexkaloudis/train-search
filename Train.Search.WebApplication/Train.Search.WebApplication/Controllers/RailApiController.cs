using Microsoft.AspNetCore.Mvc;
using Train.Search.WebApplication.ExternalHttpServices;
using Train.Search.WebApplication.Pagination;
using Train.Search.WebApplication.Search;

namespace Train.Search.WebApplication.Controllers;

[ApiController]
[Route("[controller]")]
public class RailApiController : ControllerBase
{
    private readonly ILogger<RailApiController> _logger;
    private readonly ApacheLuceneSearchService _apacheLucene;
    private readonly IRailHttpClient _httpClient;
    
    public RailApiController(
        ILogger<RailApiController> logger,
        ApacheLuceneSearchService apacheLucene,
        IRailHttpClient httpClient)
    {
        _apacheLucene = apacheLucene;
        _logger = logger;
        _httpClient = httpClient;
    }
    
    [HttpGet("{pageIndex}/{pageSize}/disturbance")]
    public async Task<PagedResult<RailHttpClient.Disturbance>> GetDisturbances(
        int pageIndex, 
        int pageSize)
    {
        Page page = new Page(pageIndex, pageSize);
        var result = await _httpClient.GetAllDisturbancesWithPaginationAsync(page);
        return result;
    }
    
    [HttpGet("{pageIndex}/{pageSize}/{sortingCriteria}/disturbance")]
    public async Task<PagedResult<RailHttpClient.Disturbance>> GetDisturbancesSorted(
        int pageIndex, 
        int pageSize,
        string sortingCriteria)
    {
        var page = new Page(pageIndex, pageSize);
        sortingCriteria = sortingCriteria.ToLower();
        var result = await _httpClient.GetAllDisturbancesWithPaginationAndSortedAsync(page, sortingCriteria);
        return result;
    }
    
    // [HttpGet("{searchParameter}/{pageIndex}/{pageSize}/station")]
    // public async Task<IEnumerable<RailHttpClient.Station>> SearchStations(string searchParameter, int pageIndex, int pageSize)
    // {
    //     Page page = new Page(pageIndex, pageSize);
    //     await _apacheLucene.AddStationsToIndex();
    //     var result = _apacheLucene.SearchStations(searchParameter, page);
    //     return result;
    // }
    

    
    [HttpGet("{searchParameter}/{pageIndex}/{pageSize}/disturbance/search")]
    public async Task<IEnumerable<RailHttpClient.Disturbance>> SearchDisturbances(string searchParameter, int pageIndex, int pageSize)
    {
        Page page = new Page(pageIndex, pageSize);
        await _apacheLucene.AddDisturbancesToIndex();
        var result = _apacheLucene.SearchDisturbances(searchParameter, page);
        return result;
    }
    
    [HttpGet("{searchParameter}/{pageIndex}/{pageSize}/disturbance/tokenization")]
    public async Task<PagedResult<RailHttpClient.Disturbance>> SearchDisturbancesWithTokenization(string searchParameter, int pageIndex, int pageSize)
    {
        Page page = new Page(pageIndex, pageSize);
        await _apacheLucene.AddDisturbancesToIndex();
        var result = _apacheLucene.SearchDisturbancesWithTokenization(searchParameter, page);
        return result;
    }
    
    [HttpGet("{searchParameter}/{pageIndex}/{pageSize}/{sortingCriteria}/disturbance/tokenization")]
    public async Task<PagedResult<RailHttpClient.Disturbance>> SearchDisturbancesWithTokenization(
        string searchParameter, 
        int pageIndex, 
        int pageSize,
        string sortingCriteria)
    {
        Page page = new Page(pageIndex, pageSize);
        await _apacheLucene.AddDisturbancesToIndex();
        var result = _apacheLucene.SearchDisturbancesWithTokenization(searchParameter, page, sortingCriteria);
        return result;
    }
}