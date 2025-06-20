using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WsRaisedHandsModern.Api.Interfaces
{
    public interface IExcelService
    {
        bool GenerateExcel<T>(IEnumerable<T> data, string filePath, string worksheetName = "Sheet1");
    }
}