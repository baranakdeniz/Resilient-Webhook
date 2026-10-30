using System.Security.Cryptography;
using System.Text;

namespace ResilientWebhook.Api.Validations
{
    public sealed class WebhookSignatureValidator
    {
        private readonly byte[] _secretBytes;

        public WebhookSignatureValidator(IConfiguration config)
        {
            var secret = config["WebhookSecret"];
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("WebhookSecret is not configured.");
            }
            _secretBytes = Encoding.UTF8.GetBytes(secret);
        }
        public bool TryValidate(string timestamp , byte[] rawBytes , string providedHex)
        {
            byte[] timestampBytes = Encoding.UTF8.GetBytes(timestamp);

            byte[] dataToSign = new byte[timestampBytes.Length + 1 + rawBytes.Length];

            timestampBytes.CopyTo(dataToSign, 0);

            dataToSign[timestampBytes.Length] = (byte)'.';

            rawBytes.CopyTo(dataToSign, timestampBytes.Length + 1);

            byte[] expected = HMACSHA256.HashData(_secretBytes, dataToSign);

            byte[] provided;

            try { provided = Convert.FromHexString(providedHex); }
            catch 
            {
               return false;
            }

            return CryptographicOperations.FixedTimeEquals(expected, provided);
        }
    }
}
