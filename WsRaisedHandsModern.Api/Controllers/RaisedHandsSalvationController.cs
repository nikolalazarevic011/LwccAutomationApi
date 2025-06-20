using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WsRaisedHandsModern.Api.Data.CMSReporting.Entities;
using WsRaisedHandsModern.Api.Data.CMSReporting.Interfaces;
using Microsoft.Extensions.Logging;
using WsRaisedHandsModern.Api.Interfaces;
using WsRaisedHandsModern.Api.Helpers;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Drawing;
using Spire.Xls;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using WsRaisedHandsModern.Api.DTOs;
using System.Net.Mail;

namespace WsRaisedHandsModern.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RaisedHandsSalvationController : ControllerBase
    {
        private readonly IRaisedHandsCmsReportRepository _cmsReportRepo;
        private readonly ILogger<RaisedHandsSalvationController> _logger;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly IWebHostEnvironment _env;
        private readonly IExcelService _excelService;

        public RaisedHandsSalvationController(
                IRaisedHandsCmsReportRepository cmsReportRepo,
                ILogger<RaisedHandsSalvationController> logger,
                IEmailService emailService,
                IOptions<EmailSettings> emailSettings,
                IWebHostEnvironment env,
                IExcelService excelService)
        {
            _cmsReportRepo = cmsReportRepo;
            _logger = logger;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _env = env;
            _excelService = excelService;
        }

        //https://localhost:5001/RaisedHandsSalvation/by-date-range?startDate=2024-06-01&endDate=2024-06-30
        [HttpGet]
        public async Task<ActionResult<IEnumerable<tblFormSalvation>>> GetRaisedHandsByDateRangeAsync([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            //simply get the raised hands by date range and return the result in the response
            var result = await _cmsReportRepo.GetRaisedHandsByDateRangeAsync(startDate, endDate);
            if (result == null || !result.Any())
            {
                return NotFound("No data found for the specified date range.");
            }

            return Ok(result);
        }

        //https://localhost:5001/RaisedHandsSalvation/CreateExcel?dateFrom=2024-06-01&dateTo=2024-06-30
        /*Excel*/
        [HttpPost("CreateExcel")]
        public async Task<ActionResult<IEnumerable<tblFormSalvation>>> CreateExcelAsync([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
              //get the raised hands by date range
            var result = await _cmsReportRepo.GetRaisedHandsByDateRangeAsync(startDate, endDate);

              //Create Excel file
            if (result == null || !result.Any())
            {
                return NotFound("No data found for the specified date range.");
            }

              // Manual mapping from tblFormSalvation to RaisedHandsSalvationDTO
            List<RaisedHandsSalvationDTO> raisedHandResults = MapRaisedHandResults(result);

              // Create a list to hold the report file paths
            List<string> paths = [];

              // Create the Excel file for the raised hands results, returning the full path to the file
            string reportFullPath = CreateExcelSalvation(raisedHandResults, startDate, endDate);

              //Check to see if the file was created successfully
            if (string.IsNullOrEmpty(reportFullPath))
            {
                _logger.LogError("Failed to create Excel file for Raised Hands Salvation.");
                return StatusCode(500, "Internal server error while creating the Excel file.");
            }

              //Add the full path to the list of paths
            paths.Add(reportFullPath);
            
              //Convert the list paths to IEnumerable<Attachment> for email attachements
            var attachments = paths.Select(path => new Attachment(path)).ToList();

              //log the results
            _logger.LogInformation("Raised Hands Salvation report created successfully from {StartDate} to {EndDate}.", startDate, endDate);

              // Send Email    
            string subject = "Raised Hands/Submitted for Salvation for  " + DateTime.Today.ToShortDateString();
            string body = $"Raised Hands report from {startDate.ToShortDateString()} to {endDate.ToShortDateString()}";
            //string fullPath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/Files/SubmittedFormsForSalvation_" + DateTime.Today.ToShortDateString().Replace(@"/", "_") + ".xls");

            // Send the email with the report attached
            // Task SendEmailAsync(string toEmail, string subject, string message, IEnumerable<System.Net.Mail.Attachment>? attachments = null);
            try
            {
                _emailService.SendEmailAsync(_emailSettings.To,
                                         subject,
                                         body,
                                         attachments).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email with Raised Hands Salvation report.");
                return StatusCode(500, "Internal server error while sending the email.");       
            }
            
              //log the email sent
            _logger.LogInformation("Email Successfully sent with subject: {Subject} to {ToEmail}", subject, _emailSettings.To);

            return Ok(result);
        }

        public string CreateExcelSalvation(List<RaisedHandsSalvationDTO> report, DateTime startDate, DateTime endDate)
        {
            var filename = "SubmittedFormsForSalvation_" + DateTime.Today.ToShortDateString().Replace(@"/", "_") + ".xls";
            var foldername = "Files";

            // Create the path for the Excel file
            var filePath = Path.Combine(_env.ContentRootPath, foldername, filename);

            // var data = _cmsReportRepo.GetSalvationData(dateFrom, dateTo); // Your repo method
            var generateExcel = _excelService.GenerateExcel(report, filePath, "Salvation");

            if (generateExcel == false)
            {
                return string.Empty; // Return empty string if Excel generation fails
            }
            
            return filePath; // Return the full path to the created Excel file

              //Optional return a file to download
            /*return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");*/
        }

        /*string CreateExcelSalvation(List<RaisedHandsSalvationDTO> report, DateTime startDate, DateTime endDate)
        {
            try
            {
                var filename = "SubmittedFormsForSalvation_" + DateTime.Today.ToShortDateString().Replace(@"/", "_") + ".xls";
                var foldername = "Files";

                  // Create the path for the Excel file
                var fullPath = Path.Combine(_env.ContentRootPath, foldername, filename);

                  //Initialize worksheet    
                Workbook workbook = new Workbook();

                Worksheet sheet = workbook.Worksheets[0];
                DateTime dateFromFormat = Convert.ToDateTime(startDate);
                DateTime dateToFormat = Convert.ToDateTime(endDate);

                //create string
                //IQueryable<tblFormSalvation> list = dbSalvation.tblFormSalvation.Where(d => d.dateCreated >= dateFromFormat && d.dateCreated < dateToFormat);

                bool flgHeaders = false;
                int row = 1;

                foreach (var record in report)
                {
                    if (flgHeaders == false)
                    {
                        //Append headers
                        sheet.Range["A1"].Text = "First Name";
                        sheet.Range["B1"].Text = "Last Name";
                        sheet.Range["C1"].Text = "Email";
                        sheet.Range["D1"].Text = "DateCreated";
                        sheet.Range["A1:D1"].Style.Color = Color.LightBlue;
                        sheet.SetColumnWidth(1, 35);
                        sheet.SetColumnWidth(2, 25);
                        sheet.SetColumnWidth(3, 25);
                        sheet.SetColumnWidth(4, 25);
                        flgHeaders = true;
                    }
                    row++;
                    //Append Text
                    sheet.Range["A" + row.ToString()].Text = record.FirstName;
                    sheet.Range["B" + row.ToString()].Text = record.LastName;
                    sheet.Range["C" + row.ToString()].Text = record.Email;
                    sheet.Range["D" + row.ToString()].Text = record.DateCreated.ToString();
                }

                  //Save it as Excel file 
                workbook.SaveToFile(fullPath, ExcelVersion.Version97to2003);
                workbook.Dispose();
                return fullPath;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Excel file for Raised Hands Salvation.");
                   //return nothing Error
                return "";
            }
        }*/

        public List<RaisedHandsSalvationDTO> MapRaisedHandResults(IEnumerable<tblFormSalvation> result)
        {
            var raisedHandResults = result.Select(r => new RaisedHandsSalvationDTO
            {
                Id = r.AutoNum,
                SavedFlag = r.SavedFlag,
                FirstName = r.FirstName,
                LastName = r.LastName,
                Country = r.Country,
                Address1 = r.Address1,
                Address2 = r.Address2,
                City = r.City,
                StateRegionProvince = r.StateRegionProvince,
                PostalCode = r.PostalCode,
                PhoneNumber = r.PhoneNumber,
                Email = r.Email,
                ContactFlag = r.ContactFlag,
                DateCreated = r.DateCreated,
                Source = r.Source,
                EnewsletterFlag = r.EnewsletterFlag,
                TextblastsFlag = r.TextblastsFlag,
                NonMemberFlag = r.NonMemberFlag
                // Add other property mappings as needed
            }).ToList();

            return raisedHandResults;
        }

        /* 
 //insert sent email 
 if (insertEmailHistory(DateTime.Today) == true && (fullPath != "" && fullPathSalvation != ""))
 {
     //send email 
     List<string> listFiles = new List<string>();
     listFiles.Add(fullPathSalvation);

     Email.sendEmail(listFiles, subject);
     // subject = "RaisedHands Spanish for  " + DateTime.Today.ToShortDateString();
     // Email.sendEmail(fullPathSpanish ,subject);
 }
 return Ok();
}*/
     




        /*
        bool insertEmailHistory(DateTime dateSent)
        {
            try
            {
                RaisedHandsHistory raisedHandsHistory = new RaisedHandsHistory();
                raisedHandsHistory.DateEmailSent = dateSent;
                db.RaisedHandsHistory.Add(raisedHandsHistory);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        */






        //http://localhost:5049/RaisedHandsSalvation?startDate=2022-06-01&endDate=2022-06-30

        // GET: api/RaisedHandsSalvations
        // public IQueryable<RaisedHandsSalvation> GetRaisedHandsSalvation()
        // {
        //     return _context.RaisedHandsSalvation;
        // }

        // Task<IEnumerable<tblFormSalvation>> GetRaisedHandsByDateRangeAsync(DateTime startDate, DateTime endDate);

    }
}