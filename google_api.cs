
using EdfLookupAggregator.Controllers.Helper._Shared;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;


public class GoogleDriveDownloader
{
  
    private string[] _scopes = {DriveService.Scope.Drive }; // Change this if you're accessing Drive or Docs
    private string _applicationName = "Place application name here";
    protected DriveService _driveService;


    /*

    There are two ways to connect to Google API
    (1) Service account
    (2) OAuth Credentials (used to get access to user's data with their permission)
    
    They are shown below

    */


    /// <summary>
    /// Connects to Google using service account
    /// </summary>
    private void ConnectToGoogleService()
    {
        GoogleCredential credential;
        string path = "path to JSON credentials file";

        using (var stream = new FileStream(path,
            FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(_scopes);
        }

        // Create Google Sheets API service.
        _driveService = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName
        });
    }

    /// <summary>
    /// Connects to Google using user account
    /// </summary>
    private void ConnectToGoogleUser()
    {
        UserCredential credential;

        // This fill be generated if not present
        string token = "path to token file";

        
        string path = "path to OAuth JSON credentials file";

        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            string credPath = token;
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                _scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            Console.WriteLine("Credential file saved to: " + credPath);
        }


        _driveService = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName,
        });

    }


    public GoogleDriveDownloader()
    {
        Initialize();
    }

    public GoogleDriveDownloader(string basePath)
    {
        Initialize();
    }

    public void Initialize()
    {
        try
        {
            //ConnectToGoogleService();
            ConnectToGoogleUser();
        }
        catch (Exception ex)
        {
            // Unable to connect to Google API service. Verify the credentials and token file
        }
    }

    /// <summary>
    /// Extracts file Id from the google drive file url
    /// </summary>
    /// <param name="url"></param>
    /// <returns>file Id as string</returns>
    public string GetFileId(string url)
    {
        string fileId = String.Empty;

        try
        {
            if (url.Contains("/d/"))
            {
                List<string> tmp = new List<string>(url.Split(new string[] { "d/" }, StringSplitOptions.None));
                List<string> tmp2 = new List<string>(tmp[1].Split(new string[] { "/" }, StringSplitOptions.None));
                fileId = tmp2[0];
            }
            else
            {
                List<string> tmp = new List<string>(url.Split(new string[] { "id=" }, StringSplitOptions.None));
                fileId = tmp[1];
            }

        }catch (Exception ex)
        {
           // "Unable to extract file ID from the URL: "
        }

        return fileId.Trim().Trim('/');
        //return fileId;
    }

    /// <summary>
    /// Extracts folder Id from the google drive folder url
    /// </summary>
    /// <param name="url"></param>
    /// <returns>file Id as string</returns>
    public string GetFolderId(string url)
    {
        string folderId = String.Empty;

        try
        {
            if (url.Contains("folders/"))
            {
                List<string> tmp = new List<string>(url.Split(new string[] { "folders/" }, StringSplitOptions.None));
                if (tmp[1].Contains("?"))
                {
                    List<string> tmp2 = new List<string>(tmp[1].Split(new string[] { "?" }, StringSplitOptions.None));
                    folderId = tmp2[0];
                }
                else
                {
                    folderId = tmp[1];
                }
            }

        }
        catch (Exception ex)
        {
            // Unable to extract folder ID from the URL
        }

        return folderId.Trim().Trim('/');
    }


    /// <summary>
    /// Downloads the google drive file by its url and names it fileName
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="url"></param>
    /// <returns>FileInfo of the downloaded file</returns>
    public FileInfo DownloadFileByUrl(string fileName, string url)
    {
        try
        {
            var fileId = GetFileId(url);
            return DownloadFile(fileName, fileId);

        }
        catch (Exception ex)
        {
            // Unable to download

        }
        return null;

    }


    public FileInfo DownloadFile(string fileName, string fileId)
    {
        try
        {

            var stream = new MemoryStream();
            var fileRes = _driveService.Files.Get(fileId);
            fileRes.SupportsAllDrives = true;

            var file = fileRes.Execute();
            var fullFilePath = String.Empty;

            
            /*

            If the file is in google doc, google slide, or google sheet format,
            it needs to be converted to PDF or CSV to be downloaded.
            
            */
            if (file.MimeType.StartsWith(_googleDocMimeType))
            {
                _driveService.Files.Export(fileId, _pdfMimeType).Download(stream);
                fullFilePath += "\\" + fileName + "." + "pdf";

            }
            else if (file.MimeType.StartsWith(_googleSlideMimeType))
            {
                _driveService.Files.Export(fileId, _pdfMimeType).Download(stream);
                fullFilePath +=  "\\" + fileName + "." + "pdf";
            }
            else if (file.MimeType.StartsWith(_googleSheetMimeType))
            {
                _driveService.Files.Export(fileId, _csvMimeType).Download(stream);
                fullFilePath +=  "\\" + fileName + "." + "csv";
            }
            else
            {
                /*

                General case for all other files
                
                */
                var request = _driveService.Files.Get(fileId);
                string fileExt = file.Name.Split('.')[1];
                request.Download(stream);
                fullFilePath +=  "\\" + fileName + "." + GetFileExt(fileId);
            }

            
            
            // Save the stream to the disk at fullFilePath location
            if (stream != null)
            {

                using (var fs = new FileStream(fullFilePath, FileMode.Create))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fs);
                    stream.Close();
                }
            }

            return new FileInfo(fullFilePath);

        }
        catch (Exception ex)
        {
            _log.Error(String.Format("Unable to download [{0}] from the Google Drive of ID [{1}]: ", fileName, fileId), ex);

        }
        return null;
    }

    public string GetMimeType(string fileId)
    {
        try
        {

            var fileRes = _driveService.Files.Get(fileId);
            fileRes.SupportsAllDrives = true;

            var file = fileRes.Execute();
            var fullFilePath = String.Empty;

            var request = _driveService.Files.Get(fileId);

            return file.MimeType;

        }
        catch (Exception ex)
        {
            // Unable to get Mime Type of file ID

        }
        return null;
    }



    /// <summary>
    /// Copies a file to a particular google drive directory
    /// </summary>
    /// <param name="folderId">folder to copy the file to</param>
    /// <param name="fileName">name of the file being copied</param>
    /// <param name="fileId">file Id of the file being copied</param>
    protected void CopyFileInDrive(string folderId, string fileName, string fileId)
    {
        
        List<string> parentList = new List<string>
        {
            folderId
        };

        Google.Apis.Drive.v3.Data.File copiedFile = new Google.Apis.Drive.v3.Data.File
        {
            Name = fileName,
            Parents = parentList
        };

        var fileResCopy = _driveService.Files.Copy(copiedFile, fileId);
        fileResCopy.SupportsAllDrives = true;

        fileResCopy.Execute();
    }


    public void UploadFileToDrive(string driveFolderId)
    {


        string fullPath = "path to file to be uploaded";


        Google.Apis.Drive.v3.Data.File fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = fileName,
            Parents = new List<string>() { driveFolderId }
        };

        

        using (FileStream stream = new FileStream(fullPath, FileMode.Open))
        {
            _driveService.Files.Create(fileMetadata, stream, "fileType ex: text/plain").Upload();
        }

    }










    /// <summary>
    /// Returns the id of the changes folder in the parent directory given the parent folder id
    /// </summary>
    /// <param name="parentFolderId"></param>
    /// <returns></returns>
    public string GetDriveFolder(string parentFolderId, string folderName)
    {
        string changesFolderId = parentFolderId;

        var searchRes = _driveService.Files.List();
        searchRes.Q = String.Format("name = '{0}' and '{1}' in parents and mimeType = 'application/vnd.google-apps.folder'", folderName, parentFolderId);
        searchRes.Fields = "files(id, name, modifiedTime)";
        var filesRet = searchRes.Execute();
        if (filesRet.Files.Count != 0)
        {
            changesFolderId = filesRet.Files[0].Id;
        }
        else
        {
            Google.Apis.Drive.v3.Data.File folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                Parents = new List<string>() { parentFolderId },
                MimeType = "application/vnd.google-apps.folder"

            };
            var folderInfo = _driveService.Files.Create(folderMetadata);
            folderInfo.Fields = "id";
            Google.Apis.Drive.v3.Data.File folder = folderInfo.Execute();
            changesFolderId = folder.Id;
        }
        return changesFolderId;
    }


    public void DelDriveFolder(string folderId)
    {
        
        try
        {
            _driveService.Files.Delete(folderId).Execute();
        }
        catch (Exception ex)
        {
            _log.Error(String.Format("Could not delete folder with folder Id {0}", folderId), ex);
        }
    }






    /// <summary>
    /// Gets the file name of the file pointed to by the url
    /// </summary>
    /// <param name="url"></param>
    /// <returns>file name</returns>
    public string GetFileName(string url)
    {
        try
        {
            var fileId = GetFileId(url);
            var fileRes = _driveService.Files.Get(fileId);
            fileRes.SupportsAllDrives = true;
            fileRes.Fields = "name, fullFileExtension";
            var fileData = fileRes.Execute();

            return fileData.Name.Split(new string[] { "." + fileData.FullFileExtension }, StringSplitOptions.None)[0];
        }catch (Exception ex)
        {
            // Unable to get file name of the file
        }

        return "N/A";
    }

    /// <summary>
    /// Gets the full file name of the file pointed to by the url
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public string GetFullFileName(string url)
    {
        try
        {
            var fileId = GetFileId(url);
            var fileRes = _driveService.Files.Get(fileId);
            fileRes.SupportsAllDrives = true;
            fileRes.Fields = "name";
            var fileData = fileRes.Execute();

            return fileData.Name;
        }
        catch (Exception ex)
        {
            // Unable to get full file name of the file
        }

        return "N/A";
    }

    /// <summary>
    /// Gets the file extension of the file pointed to by the url
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public string GetFileExtByUrl(string url)
    {
        try
        {
            var fileId = GetFileId(url);
            return GetFileExt(fileId);
        }
        catch (Exception ex)
        {
            _log.Error(String.Format("Unable to get file extention of the file pointed to by [{0}]: ", url), ex);
        }

        return null;
    }


    public string GetFileExt(string fileId)
    {
        try
        {
            var fileRes = _driveService.Files.Get(fileId);
            fileRes.SupportsAllDrives = true;
            fileRes.Fields = "fullFileExtension";
            var fileData = fileRes.Execute();

            return fileData.FullFileExtension;
        }
        catch (Exception ex)
        {
            // "Unable to get file extention of the file
        }

        return null;
    }



    /// <summary>
    /// Gets the last modified date and time of the file pointed to by the url
    /// </summary>
    /// <param name="url"></param>
    /// <returns>file name</returns>
    public DateTime GetFileModifiedDate(string url)
    {
        try
        {
            var fileId = GetFileId(url);
            var fileRes = _driveService.Files.Get(fileId);
            fileRes.SupportsAllDrives = true;
            fileRes.Fields = "modifiedTime";
            var fileData = fileRes.Execute();

            DateTime time = (DateTime)fileData.ModifiedTime;

            return time;
        }
        catch (Exception ex)
        {
            // Unable to get last modified date the file
        }

        return DateTime.MinValue;
    }





}
