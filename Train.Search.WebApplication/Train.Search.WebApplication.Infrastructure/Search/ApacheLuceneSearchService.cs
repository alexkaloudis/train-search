using System.Globalization;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lucene.Net.QueryParsers.Classic;
using Train.Search.WebApplication.Infrastructure.DtoManipulators;
using Train.Search.WebApplication.Infrastructure.ExternalHttpServices;
using Train.Search.WebApplication.Infrastructure.Pagination;

namespace Train.Search.WebApplication.Infrastructure.Search;

public class ApacheLuceneSearchService
{
    private const LuceneVersion Version = LuceneVersion.LUCENE_48;
    private readonly Analyzer _analyzer;
    private readonly RAMDirectory _directory;
    private readonly IndexWriter _writer;
    private readonly IRailHttpClient _client;
    private readonly PagedResultManipulator _pagedResultManipulator;

    public ApacheLuceneSearchService(
        IRailHttpClient client,
        PagedResultManipulator pagedResultManipulator)
    {
        _analyzer = new StandardAnalyzer(Version);
        _directory = new RAMDirectory();
        var config = new IndexWriterConfig(Version, _analyzer);
        _writer = new IndexWriter(_directory, config);
        _client = client;
        _pagedResultManipulator = pagedResultManipulator;
    }

    public async Task AddStationsToIndex()
    {
        var stations = await _client.GetAllStationsAsync();
        foreach (var document in stations.Select(station => new Document
                 {
                     new StringField("Id", station.Id, Field.Store.YES),
                     new TextField("Name", station.Name, Field.Store.YES),
                     new StringField("LocationX", station.LocationX, Field.Store.YES),
                     new StringField("LocationY", station.LocationY, Field.Store.YES),
                     new StringField("Standardname", station.Standardname, Field.Store.YES)
                 }))
        {
            _writer.AddDocument(document);
        }
        _writer.Commit();
    }
    
    public async Task AddDisturbancesToIndex()
    {
        var disturbances = await _client.GetAllDisturbancesAsync();
        foreach (var document in disturbances.Select(disturbance => new Document
                 {
                     new StringField("Id", disturbance.Id, Field.Store.YES),
                     new StringField("Title", disturbance.Title, Field.Store.YES),
                     new TextField("Description", disturbance.Description, Field.Store.YES),
                     new StringField("Link", disturbance.Link, Field.Store.YES),
                     new StringField("Timestamp", disturbance.Timestamp.ToString(CultureInfo.InvariantCulture), Field.Store.YES)
                 }))
        {
            _writer.AddDocument(document);
        }
        _writer.Commit();
    }

    
    //TODO: IMPLEMENT PAGINATION HERE
    public IEnumerable<RailHttpClient.Station> SearchStations(string searchTerm, Page page)
    {
        var directoryReader = DirectoryReader.Open(_directory);
        var indexSearcher = new IndexSearcher(directoryReader);

        string[] fields = ["Name", "LocationX", "LocationY", "Standardname"];
        var queryParser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, fields, _analyzer);
        var query = queryParser.Parse(searchTerm);

        var hits = indexSearcher.Search(query, 10).ScoreDocs;

        var stations = new List<RailHttpClient.Station>();
        foreach (var hit in hits)
        {
            var document = indexSearcher.Doc(hit.Doc);
            stations
                .Add(new RailHttpClient
                    .Station(
                    document.Get("Id"),
                    document.Get("Name"),
                    document.Get("LocationX"),
                    document.Get("LocationY"),
                    document.Get("Standardname")
                    )
            );
        }

        return stations
            .Skip(page.PageIndex * page.PageSize)
            .Take(page.PageSize)
            .ToList();
    }
    
    //TODO: IMPLEMENT PAGINATION HERE
    public IEnumerable<RailHttpClient.Disturbance> SearchDisturbances(string searchTerm,  Page page)
    {
        var directoryReader = DirectoryReader.Open(_directory);
        var indexSearcher = new IndexSearcher(directoryReader);

        string[] fields = ["Id", "Title","Description", "Link", "Timestamp"];
        var queryParser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, fields, _analyzer);
        var query = queryParser.Parse(searchTerm);

        var hits = indexSearcher.Search(query, 10).ScoreDocs;

        var disturbances = new List<RailHttpClient.Disturbance>();
        foreach (var hit in hits)
        {
            var document = indexSearcher.Doc(hit.Doc);
            disturbances
                .Add(new RailHttpClient
                    .Disturbance(
                        document.Get("Id"),
                        document.Get("Title"),
                        document.Get("Description"),
                        document.Get("Link"),
                        DateTime.ParseExact(
                            document.Get("Timestamp"),
                            "MM/dd/yyyy HH:mm:ss",
                            CultureInfo.InvariantCulture)
                    )
                );
        }
        return disturbances
            .Skip(page.PageIndex * page.PageSize)
            .Take(page.PageSize)
            .ToList();
    }
    
    public PagedResult<RailHttpClient.Disturbance> SearchDisturbancesWithTokenization(
    string searchTerm, 
    Page page, 
    string? sortingCriteria = null)
{
    using var directoryReader = DirectoryReader.Open(_directory);
    var indexSearcher = new IndexSearcher(directoryReader);

    string[] fields = { "Id", "Title", "Description", "Link", "Timestamp" };
    
    // Create a query parser with support for boolean operators
    var queryParser = new QueryParser(LuceneVersion.LUCENE_48, "Description", _analyzer)
    {
        // Enable boolean operators
        DefaultOperator = QueryParserBase.OR_OPERATOR,
        AllowLeadingWildcard = true
    };

    // Parse the search term with support for boolean logic
    Query query;
    try 
    {
        // Replace "AND" with "+" and "OR" with "||" for more standard boolean syntax
        searchTerm = searchTerm
            .Replace(" AND ", " +")
            .Replace(" OR ", " ||")
            .Replace(" NOT ", " -");

        query = queryParser.Parse(searchTerm);
    }
    catch (ParseException)
    {
        // Fallback to a simple term query if parsing fails
        query = queryParser.Parse(QueryParserBase.Escape(searchTerm));
    }

    var hits = indexSearcher.Search(query, 10).ScoreDocs;
    
    var results = new List<TokenizedDisturbanceSearchResult>();
    
    foreach (var hit in hits)
    {
        var document = indexSearcher.Doc(hit.Doc);
        var disturbance = new RailHttpClient.Disturbance(
            document.Get("Id"),
            document.Get("Title"),
            document.Get("Description"),
            document.Get("Link"),
            DateTime.ParseExact(
                document.Get("Timestamp"),
                "MM/dd/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture)
        );

        var tokenizedFields = new Dictionary<string, List<string>>();
        foreach (var field in fields)
        {
            var fieldContent = document.Get(field);
            if (!string.IsNullOrEmpty(fieldContent))
            {
                tokenizedFields[field] = TokenizeText(fieldContent);
            }
        }

        results.Add(new TokenizedDisturbanceSearchResult(
            disturbance,
            tokenizedFields,
            hit.Score
        ));
    }
    
    return RequestedToBeSorted(sortingCriteria ?? string.Empty, results, page);
}
    private PagedResult<RailHttpClient.Disturbance> RequestedToBeSorted(
        string sortingCriteria,
        List<TokenizedDisturbanceSearchResult> results,
        Page page)
    {
        return sortingCriteria switch
        {
            "relevance" => _pagedResultManipulator.TokenizedDisturbanceSearchResultToPagedResultSortedByTokenRelevance(
                results, page),
            "title" => _pagedResultManipulator
                .TokenizedDisturbanceSearchResultToPagedResultSortedByTitle(results, page),
            "date" => _pagedResultManipulator.TokenizedDisturbanceSearchResultToPagedResultSortedByTimestamp(
                results, page),
            _ => _pagedResultManipulator.TokenizedDisturbanceSearchResultToPagedResult(results, page)
        };
    }

    private List<string> TokenizeText(string text)
    {
        var tokens = new List<string>();

        using (var tokenStream = _analyzer.GetTokenStream("content", new StringReader(text)))
        {
            var termAttribute = tokenStream.AddAttribute<ICharTermAttribute>();
            tokenStream.Reset();

            while (tokenStream.IncrementToken())
            {
                tokens.Add(termAttribute.ToString());
            }

            tokenStream.End();
        }

        return tokens;
    }
}

public class TokenizedDisturbanceSearchResult
{
    public TokenizedDisturbanceSearchResult(RailHttpClient.Disturbance disturbance, Dictionary<string, List<string>> tokenizedFields, float score)
    {
        Disturbance = disturbance;
        TokenizedFields = tokenizedFields;
        Score = score;
    }

    public RailHttpClient.Disturbance Disturbance { get; set; }
    public Dictionary<string, List<string>> TokenizedFields { get; set; }
    public float Score { get; set; }
}