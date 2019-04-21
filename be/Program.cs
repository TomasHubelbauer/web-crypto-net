using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace be
{
    class Program
    {
        private static byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        static async Task Main(string[] args)
        {
            var server = new HttpListener();
            server.Prefixes.Add("http://localhost:8000/");
            server.Start();
            Console.WriteLine("Serving…");
            while (true) {
                var context = await server.GetContextAsync();
                Console.WriteLine($"Serving {context.Request.RawUrl}…");

                // CORS
                context.Request.Headers.Set("Access-Control-Allow-Origin", "*");
                if (context.Request.HttpMethod == "OPTIONS")
                {
                    context.Request.Headers.Set("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                    context.Request.Headers.Set("Access-Control-Allow-Methods", "GET, POST");
                    context.Request.Headers.Set("Access-Control-Max-Age", "1728000");
                    break;
                } else {
                    switch (context.Request.HttpMethod + ' ' + context.Request.Url.LocalPath) {
                        case "GET /": {
                            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("test"));
                            break;
                        }
                        case "GET /api": {
                            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Use POST"));
                            break;
                        }
                        case "POST /api": {
                            using (var aes = new AesManaged())
                            {
                                aes.Mode = CipherMode.CBC;
                                aes.Padding = PaddingMode.PKCS7;
                                using (var encryptor = aes.CreateEncryptor())
                                {
                                    using (var cryptoStream = new CryptoStream(context.Response.OutputStream, encryptor, CryptoStreamMode.Write))
                                    {
                                        await cryptoStream.WriteAsync(Encoding.UTF8.GetBytes("test"));
                                        cryptoStream.FlushFinalBlock();
                                    }
                                }
                            }

                            break;
                        }
                        default: {
                            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(context.Request.HttpMethod + ' ' + context.Request.Url.LocalPath));
                            break;
                        }
                    }
                }

                context.Response.OutputStream.Close();
            }
        }
    }
}
