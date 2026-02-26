using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace ozon
{
    internal class OzonApiClient
    {
        private static readonly HttpClient _http;

        static OzonApiClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };
            _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "ru-RU,ru;q=0.9,en;q=0.8");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua",
                "\"Chromium\";v=\"132\", \"Google Chrome\";v=\"132\", \"Not-A.Brand\";v=\"99\"");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
        }

        public async Task<List<Product>> SearchAsync(string query)
        {
            await _http.GetAsync("https://www.ozon.ru/");

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.ozon.ru/search/?{query}");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.ozon.ru/");
            request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
            request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ozon вернул {(int)response.StatusCode} {response.ReasonPhrase}");

            var html = await response.Content.ReadAsStringAsync();
            return ParseHtml(html);
        }

        private List<Product> ParseHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return ParseProductElements(doc);
        }


        private List<Product> ParseProductElements(HtmlDocument doc)
        {
            var products = new List<Product>();

            var tiles = doc.DocumentNode.SelectNodes("//*[contains(@class,'tile-root')]");
            if (tiles == null) return products;

            foreach (var tile in tiles)
            {
                var link = tile.SelectSingleNode(".//a[contains(@href, '/product/')]");

                var name = "";
                var nameSpan = tile.SelectSingleNode(".//span[contains(@class,'tsBody500Medium')]");
                name = WebUtility.HtmlDecode(nameSpan.InnerText).Trim();
                if (string.IsNullOrEmpty(name) || name.Length < 4) continue;

                var imgSrc = tile.SelectSingleNode(".//img").GetAttributeValue("src", "");

                var price = FindPriceInTile(tile);

                var fullUrl = "https://www.ozon.ru" + link.GetAttributeValue("href", "");
                products.Add(new Product { Name = name, Price = price, ImageUrl = imgSrc, Url = fullUrl });
            }

            return products;
        }

        private static string FindPriceInTile(HtmlNode tile)
        {
            var m = Regex.Match(tile.InnerText, @"[\d\s]{1,10}₽");
            return m.Success ? m.Value.Trim() : "";
        }
    }
}
