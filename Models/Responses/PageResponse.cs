namespace ProvaPub.Models.Responses
{
    public class PagedResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNext => CurrentPage < TotalPages;
        public List<T> Data { get; set; } = new();

        public static PagedResponse<T> Ok(
            IEnumerable<T> items, int currentPage, int pageSize, int totalCount,
            string message = "Operação realizada com sucesso")
            => new()
            {
                Success = true,
                Message = message,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount,
                Data = items.ToList()
            };

        public static PagedResponse<T> Fail(string message)
            => new()
            {
                Success = false,
                Message = message,
                CurrentPage = 1,
                PageSize = 0,
                TotalCount = 0,
                Data = new()
            };
    }
}