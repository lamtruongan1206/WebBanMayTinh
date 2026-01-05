using System.Globalization;

namespace WebBanMayTinh.Utils
{
    public static class CurrencyExtensions
    {
        public static string ToVnd(this decimal amount)
        {
            return string.Format(
                CultureInfo.GetCultureInfo("vi-VN"),
                "{0:c0}",
                amount
            );
        }

        public static string ToVnd(this decimal? amount)
        {
            if (!amount.HasValue) return "0 ₫";
            return amount.Value.ToVnd();
        }
    }
}
