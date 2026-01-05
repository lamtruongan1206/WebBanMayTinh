namespace WebBanMayTinh.Utils
{
    public class PaginationResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int CurrentPage { get; }
        public int PageSize { get; }
        public int TotalItems { get; }
        public int TotalPages { get; }

        public int MaxPageDisplay { get; }

        public PaginationResult(
            IReadOnlyList<T> items,
            int totalItems,
            int currentPage,
            int pageSize,
            int maxPageDisplay = 5)
        {
            Items = items;
            TotalItems = totalItems;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            CurrentPage = currentPage < 1 ? 1 :
                          currentPage > TotalPages ? TotalPages :
                          currentPage;

            MaxPageDisplay = maxPageDisplay;
        }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public IEnumerable<PaginationItem> GetPages()
        {
            var pages = new List<PaginationItem>();

            if (TotalPages <= MaxPageDisplay + 2)
            {
                for (int i = 1; i <= TotalPages; i++)
                    pages.Add(PaginationItem.Page(i, i == CurrentPage));

                return pages;
            }

            pages.Add(PaginationItem.Page(1, CurrentPage == 1));

            int start = Math.Max(2, CurrentPage - MaxPageDisplay / 2);
            int end = Math.Min(TotalPages - 1, CurrentPage + MaxPageDisplay / 2);

            if (start > 2)
                pages.Add(PaginationItem.Ellipsis());

            for (int i = start; i <= end; i++)
                pages.Add(PaginationItem.Page(i, i == CurrentPage));

            if (end < TotalPages - 1)
                pages.Add(PaginationItem.Ellipsis());

            pages.Add(PaginationItem.Page(TotalPages, CurrentPage == TotalPages));

            return pages;
        }
    }
}
