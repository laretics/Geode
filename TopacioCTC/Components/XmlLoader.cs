using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace TopacioCTC.Components
{

    public class XmlLoader
    {
        private readonly HttpClient mvarHttpCliente;

        public XmlLoader(HttpClient httpclient)
        {
            mvarHttpCliente = httpclient;
        }
        public async Task<XmlDocument> LoadXmlAsync(string filePath)
        {
            HttpResponseMessage response = await mvarHttpCliente.GetAsync(filePath);
            string xmlString = await response.Content.ReadAsStringAsync();
            return new XmlDocument();
        }
        
    }
}


/*
 *    


public class XmlService
{
    private readonly HttpClient _httpClient;

    public XmlService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<XDocument> LoadXmlAsync(string filePath)
    {
        var response = await _httpClient.GetAsync(filePath);
        response.EnsureSuccessStatusCode();
        var xmlString = await response.Content.ReadAsStringAsync();
        return XDocument.Parse(xmlString);
    }
}

 * */