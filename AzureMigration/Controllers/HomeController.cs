using AzureMigration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureMigration.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FileUpload(List<IFormFile> files)
        {

            long size = files.Sum(f => f.Length);

            var filePaths = new List<string>();
            ViewBag.count = files.Count;
            ViewBag.size = size;

            foreach (var formFile in files)
            {
              
                if(formFile.Length>0)
                {
                  
                    string FileName = formFile.FileName;
                    string blobstorageconnection = _configuration.GetValue<string>("BlobConnectionString");

                    CloudStorageAccount _csAccount = CloudStorageAccount.Parse(blobstorageconnection);

                    CloudBlobClient _csBlobClient = _csAccount.CreateCloudBlobClient();

                    CloudBlobContainer _cBlobContainer = _csBlobClient.GetContainerReference(_configuration.GetValue<string>("BlobContainerName"));

                    CloudBlockBlob _blockBlob = _cBlobContainer.GetBlockBlobReference(FileName);

                    await using (var data = formFile.OpenReadStream())
                    {
                        await _blockBlob.UploadFromStreamAsync(data);
                    }
                    var filePath = Path.GetTempFileName();
                    filePaths.Add(filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }
            

            return View(new { count = files.Count, size });
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public void Any()
        {
            //string targetFolder = "https://dev.azure.com/AzureCloudMigration/LocalTest/_git/backend/"
            //foreach (IFormFile file in files)
            //{
            //    if (file.Length <= 0) continue;

            //    //fileName is the the fileName including the relative path
            //    string path = Path.Combine(targetFolder, file.FileName);

            //    //check if folder exists, create if not
            //    var fi = new FileInfo(path);
            //    fi.Directory?.Create();

            //    //copy to target
            //    using var fileStream = new FileStream(path, FileMode.Create);
            //    await file.CopyToAsync(fileStream);
            //}
            //return View("Index");
        }
    }
}
