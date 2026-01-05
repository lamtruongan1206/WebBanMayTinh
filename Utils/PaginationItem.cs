namespace WebBanMayTinh.Utils
{
    public class PaginationItem
    {
        public int? PageNumber { get; }
        public bool IsActive { get; }
        public bool IsEllipsis { get; }

        private PaginationItem(int? pageNumber, bool isActive, bool isEllipsis)
        {
            PageNumber = pageNumber;
            IsActive = isActive;
            IsEllipsis = isEllipsis;
        }

        public static PaginationItem Page(int pageNumber, bool isActive = false)
            => new PaginationItem(pageNumber, isActive, false);

        public static PaginationItem Ellipsis()
            => new PaginationItem(null, false, true);
    }
}
