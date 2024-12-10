using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using Train.Search.WebApplication.Infrastructure.ExternalHttpServices;

namespace Train.Search.WebApplication.Infrastructure.Search;

public class OpenSearchService
{
    private ILogger<OpenSearchService> _logger;
    private readonly IOpenSearchClient _openSearchClient;
    private const string DefaultIndex = "students";
    
    public OpenSearchService(
        ILogger<OpenSearchService> logger,
        IOpenSearchClient openSearchClient)
    {
        _logger = logger;
        _openSearchClient = openSearchClient;
    }
    

    public async Task<bool> IndexListAsync(List<Dictionary<string, object>> documents)
    {
        try
        {
            // Create or update index mapping if needed
            var indexExistsResponse = await _openSearchClient.Indices.ExistsAsync(DefaultIndex);
            if (!indexExistsResponse.Exists)
            {
                var createIndexResponse = await _openSearchClient.Indices.CreateAsync(DefaultIndex, c => c
                    .Settings(s => s
                        .NumberOfShards(1)
                        .NumberOfReplicas(1)
                    )
                );

                if (!createIndexResponse.IsValid)
                {
                    throw new Exception($"Failed to create index: {createIndexResponse.DebugInformation}");
                }
            }

            // Index the documents
            var bulkRequest = new BulkDescriptor();

            foreach (var document in documents)
            {
                string docId = document.ContainsKey("id") ? document["id"].ToString() : Guid.NewGuid().ToString();
                
                bulkRequest.Index<Dictionary<string, object>>(i => i
                    .Index(DefaultIndex)
                    .Id(docId)
                    .Document(document));
            }

            var bulkResponse = await _openSearchClient.BulkAsync(bulkRequest);

            if (!bulkResponse.IsValid)
            {
                _logger.LogError("Failed to bulk index documents. Error: {Error}", bulkResponse.DebugInformation);
                return false;
            }

            _logger.LogInformation("Successfully indexed {Count} documents", documents.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while indexing documents");
            throw;
        }
    }

    public async Task<List<Dictionary<string, object>>> SearchDocumentsAsync(string searchTerm, string? field = null, int from = 0, int size = 50)
    {
        try
        {
            var searchResponse = await _openSearchClient.SearchAsync<Dictionary<string, object>>(s => s
                .Index(DefaultIndex)
                .From(from)
                .Size(size)
                .Query(q => 
                    string.IsNullOrWhiteSpace(field)
                        ? q.MultiMatch(m => m
                            .Fields("*")
                            .Query(searchTerm)
                            .Type(TextQueryType.BestFields)
                            .Fuzziness(Fuzziness.Auto))
                        : q.Match(m => m
                            .Field(field)
                            .Query(searchTerm)
                            .Fuzziness(Fuzziness.Auto))
                )
            );

            if (!searchResponse.IsValid)
            {
                _logger.LogError("Failed to search documents. Error: {Error}", searchResponse.DebugInformation);
                return new List<Dictionary<string, object>>();
            }

            return searchResponse.Documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching documents");
            throw;
        }
    }

    public async Task<Dictionary<string, object>?> GetDocumentByFieldAsync(string fieldName, string value)
    {
        try
        {
            var searchResponse = await _openSearchClient.SearchAsync<Dictionary<string, object>>(s => s
                .Index(DefaultIndex)
                .Query(q => q
                    .Match(m => m
                        .Field(fieldName)
                        .Query(value)
                    )
                )
                .Size(1)
            );

            if (!searchResponse.IsValid || !searchResponse.Documents.Any())
            {
                _logger.LogWarning("No document found with {FieldName}: {Value}", fieldName, value);
                return null;
            }

            return searchResponse.Documents.First();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting document by field");
            throw;
        }
    }

    public async Task<bool> UpdateDocumentAsync(string id, Dictionary<string, object> document)
    {
        try
        {
            var response = await _openSearchClient.UpdateAsync<Dictionary<string, object>, object>(
                id,
                u => u
                    .Index(DefaultIndex)
                    .Doc(document)
                    .RetryOnConflict(3)
            );

            if (!response.IsValid)
            {
                _logger.LogError("Failed to update document. Error: {Error}", response.DebugInformation);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating document");
            throw;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string id)
    {
        try
        {
            var response = await _openSearchClient.DeleteAsync<Dictionary<string, object>>(id, d => d.Index(DefaultIndex));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to delete document. Error: {Error}", response.DebugInformation);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting document");
            throw;
        }
    }
}
