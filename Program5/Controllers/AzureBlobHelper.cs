using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Program5.Controllers
{
    internal class AzureBlobHelper
    {
        const string myConnectionString = "DefaultEndpointsProtocol=https;AccountName=xuestorage1;AccountKey=Hhuc9fxOP8Z9EwOaXV7yQLahCs+gf5+ggmTyd9ACSSJSpGLF/TW1XSeevb/tvoWdG4gOYzBHU/O0Gmx/xSUzjQ==;EndpointSuffix=core.windows.net";
        const string myContainerName = "citycontainer";

        public static string GetBlob(string blobUrl)
        {
            try
            {
                // Download blob from source and read it as text.
                var response = new BlobClient(new Uri(blobUrl)).Download();
                StreamReader reader = new StreamReader(response.Value.Content);
                string text = reader.ReadToEnd();
                reader.Close();
                return text;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get blob. Error: " + ex.Message);
            }
        }

        public static string UploadBlob(string blobName, string text)
        {
            try
            {
                // Upload text as new blob in my blob store container.
                BlobServiceClient blobServiceClient = new BlobServiceClient(myConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(myContainerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                byte[] byteArray = Encoding.ASCII.GetBytes(text);
                MemoryStream stream = new MemoryStream(byteArray);
                blobClient.Upload(stream, overwrite: true);
                stream.Close();
                return blobClient.Uri.OriginalString;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload blob. Error: " + ex.Message);
            }
        }
    }
}