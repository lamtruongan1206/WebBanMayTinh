using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace WebBanMayTinh.Services
{
    public class MomoService
    {
        // ====== CẤU HÌNH MOMO SANDBOX ======
        private const string ENDPOINT = "https://test-payment.momo.vn/v2/gateway/api/create";
        private const string PARTNER_CODE = "MOMO";
        private const string ACCESS_KEY = "F8BBA842ECF85";
        private const string SECRET_KEY = "K951B6PE1waDMi640xX08PD3vg6EkVlz";

        private const string RETURN_URL = "https://localhost:7112/Checkout/MomoReturn";
        private const string NOTIFY_URL = "https://localhost:7112/Checkout/MomoNotify";

        // ====== TẠO LINK THANH TOÁN ======
        public string CreatePayment(long amount, string orderId)
        {
            string requestId = Guid.NewGuid().ToString();

            // Chuỗi dùng để ký HMAC SHA256 (MoMo yêu cầu)
            string rawHash =
                $"accessKey={ACCESS_KEY}" +
                $"&amount={amount}" +
                $"&extraData=" +
                $"&ipnUrl={NOTIFY_URL}" +
                $"&orderId={orderId}" +
                $"&orderInfo=Thanh toán đơn hàng" +
                $"&partnerCode={PARTNER_CODE}" +
                $"&redirectUrl={RETURN_URL}" +
                $"&requestId={requestId}" +
                $"&requestType=captureWallet";

            string signature = HmacSHA256(rawHash, SECRET_KEY);

            var body = new
            {
                partnerCode = PARTNER_CODE,
                accessKey = ACCESS_KEY,
                requestId,
                amount = amount.ToString(),
                orderId,
                orderInfo = "Thanh toán đơn hàng",
                redirectUrl = RETURN_URL,
                ipnUrl = NOTIFY_URL,
                requestType = "captureWallet",
                extraData = "",
                signature,
                lang = "vi"
            };

            var client = new HttpClient();
            var response = client.PostAsync(
                ENDPOINT,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            ).Result;

            var json = response.Content.ReadAsStringAsync().Result;
            var doc = JsonDocument.Parse(json);

            // Nếu MoMo OK → trả về payUrl
            if (doc.RootElement.GetProperty("resultCode").GetInt32() == 0)
            {
                return doc.RootElement.GetProperty("payUrl").GetString();
            }

            return string.Empty;
        }

        // ====== HÀM KÝ SHA256 ======
        private static string HmacSHA256(string message, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)))
                .Replace("-", "")
                .ToLower();
        }
    }
}
