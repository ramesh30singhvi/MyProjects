using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.IO;
//using Renci.SshNet; //SSH.NET
using Chilkat;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Net;

namespace CPReservationApi.WebApi.Services
{
    public class SftpService
    {
        static private AppSettings _appSettings;

        public SftpService(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }

        private readonly string _host = "sftp://partnerupload.google.com";
        private readonly int _port = 19321;
        private readonly string _username = "tz985n";
        private readonly string _password = "03068f";

        public static void SendFile()
        {
            Uri uri = new Uri("sftp://partnerupload.google.com");

            var request = WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential("tz985n", "03068f");

            //using (var sftp = new SftpClient("sftp://partnerupload.google.com", 19321, "tz985n", "03068f"))
            //{
            //    sftp.Connect();

            //    if (!sftp.IsConnected)
            //        throw new Exception("Unable to connect to the SFTP server");

            //    //using (var fileStream = new FileStream(localFilePath, FileMode.Open))
            //    //{
            //    //    var remoteFileName = Path.GetFileName(localFilePath);
            //    //    var remotePath = Path.Combine(remoteDirectory, remoteFileName).Replace("\\", "/");
            //    //    sftp.UploadFile(fileStream, remotePath);
            //    //}

            //    sftp.Disconnect();
            //}
        }
    }
}
        
