namespace Train.Search.WebApplication.Infrastructure.Pagination
{
    public class Page
    {
        public Page(int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 10;

        //public static bool TryParse(string s, out Page result)
        //{
        //    result = null;
        //    var parts = s.Split('-');
        //    if (parts.Length != 2)
        //    {
        //        return false;
        //    }

        //    if (int.TryParse(parts[0], out var pageIndex) && int.TryParse(parts[1], out var pageSize))
        //    {
        //        result = new Page { PageIndex = pageIndex, PageSize = pageSize };
        //        return true;
        //    }

        //    return false;
        //}

        //public static ValueTask<Page?> BindAsync(HttpContext context, ParameterInfo parameter)
        //{
        //    int.TryParse(context.Request.Query["PageIndex"], out var pageIndex);
        //    int.TryParse(context.Request.Query["PageSize"], out var pageSize);
        //    var result = new Page
        //    {
        //        PageIndex = pageIndex,
        //        PageSize = pageSize
        //    };
        //    return ValueTask.FromResult<Page?>(result);
        //}
    }
}
