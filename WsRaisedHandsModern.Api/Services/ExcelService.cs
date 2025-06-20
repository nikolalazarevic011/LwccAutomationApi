using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spire.Xls;
using WsRaisedHandsModern.Api.Interfaces;

namespace WsRaisedHandsModern.Api.Services
{
    public class ExcelService : IExcelService
    {
        // private readonly IWebHostEnvironment _env;
        private readonly ILogger<ExcelService> _logger;

        public ExcelService(ILogger<ExcelService> logger)
        {
            _logger = logger;
        }

        public bool GenerateExcel<T>(IEnumerable<T> data, string filePath, string worksheetName = "Sheet1")
        {
            try
            {
                var workbook = new Workbook();
                var worksheet = workbook.Worksheets[0];
                worksheet.Name = worksheetName;

                // Add headers
                var properties = typeof(T).GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Range[1, i + 1].Text = properties[i].Name;
                }

                // Add data
                int row = 2;
                foreach (var item in data)
                {
                    for (int col = 0; col < properties.Length; col++)
                    {
                        if (properties[col].GetValue(item) != null)
                        { worksheet.Range[row, col + 1].Text = properties[col].GetValue(item)?.ToString() ?? string.Empty; }
                        else
                        { worksheet.Range[row, col + 1].Text = ""; }
                    }
                    row++;
                }

                //Save it as Excel file 
                workbook.SaveToFile(filePath, ExcelVersion.Version97to2003);
                workbook.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel file at {FilePath}", filePath);
                return false;
            }

            /*using var stream = new MemoryStream();
              workbook.SaveToStream(stream, FileFormat.Version2013);
              return stream.ToArray();*/
        }

    }
}