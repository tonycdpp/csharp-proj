﻿using RestSharp;
using System;
using System.IO;
using System.Web.Http;
using System.Configuration;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace nhc_noaa.Controllers
{    
    public class DownloadController : BaseController
    {
        [HttpGet]
        public async Task<Images> EastAtlantic()
        {
            string year = DateTime.Now.Year.ToString();
            return await download(ConfigurationManager.AppSettings["DOMAIN"], 
                ConfigurationManager.AppSettings["EAST_ATL"], @">" + year + ".*rb.jpg");
        }

        static private async Task<Images> download(string domain, string path, string pattern)
        {
            var result = new Images();
            var client = new RestClient(domain);
            var req = new RestRequest(path, Method.GET);
            var response = await client.ExecuteTaskAsync(req);
            string html = response.Content;

            string fld = baseDir(path);          
            foreach (Match match in Regex.Matches(html, pattern))
            {
                foreach (Capture capture in match.Captures)
                {
                    string fileName = capture.Value.Replace(">", "");
                    result.Add(fileName, 0);
                    if (!File.Exists(fld + "\\" + fileName))
                    {
                        try
                        {
                            var img = new RestRequest(path + fileName, Method.GET);
                            response = await client.ExecuteTaskAsync(img);
                            result[fileName] = (int)response.StatusCode;
                            if (result[fileName] == 200)
                            {
                                var fs = File.Create(fld + "\\" + fileName);
                                await fs.WriteAsync(response.RawBytes, 0, response.RawBytes.Length);
                                fs.Close();
                            }
                        }
                        catch
                        {
                            result[fileName] = -3;
                        }
                    }
                }
            }
            return result;
        }


    }
}
