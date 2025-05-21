using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities; // Required for WebEncoders
using System;
using System.Text; // Required for Encoding

namespace MosefakApi.Business.Services.Data_Protection // Or your actual namespace
{
    public class IdProtectorService : IIdProtectorService
    {
        private readonly IDataProtector _protector;

        public IdProtectorService(IDataProtectionProvider protectionProvider)
        {
            // Ensure the purpose string is consistent across your application
            // if you ever resolve IDataProtectionProvider in multiple places.
            _protector = protectionProvider.CreateProtector(purpose: "SecureId");
        }

        public string Protect(int id)
        {
            var protectedPayload = _protector.Protect(id.ToString());
            // Encode the protected string to be URL-safe
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(protectedPayload));
        }

        public int? UnProtect(string protectedIdUrlSafe)
        {
            if (string.IsNullOrEmpty(protectedIdUrlSafe))
            {
                return null;
            }

            try
            {
                // Decode the URL-safe string first
                byte[] decodedPayloadBytes = WebEncoders.Base64UrlDecode(protectedIdUrlSafe);
                string protectedPayload = Encoding.UTF8.GetString(decodedPayloadBytes);

                string decryptedId = _protector.Unprotect(protectedPayload);
                return int.Parse(decryptedId);
            }
            catch (FormatException) // Catch specific exception from Base64UrlDecode if input is invalid
            {
                // Log this error if possible: "Invalid Base64URL string format"
                return null;
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // Log this error if possible: "Data unprotection failed (likely tampered or wrong key/purpose)"
                return null; // Decryption failed
            }
            catch (Exception) // Catch any other unexpected errors during parsing or unprotection
            {
                // Log this error
                return null;
            }
        }
    }
}