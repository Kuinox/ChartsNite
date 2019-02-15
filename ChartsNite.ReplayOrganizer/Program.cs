using Common.StreamHelpers;
using FortniteReplayParser;
using NSubstitute;
using OfficeOpenXml;
using System;
using System.IO;
using System.Threading.Tasks;
using UnrealReplayParser.Tests;
namespace ChartsNite.ReplayOrganizer
{
    class Program
    {
        static async Task Main()
        {
            using(var excelPackage = new ExcelPackage())
            {
                ExcelWorksheet replayWorksheet = excelPackage.Workbook.Worksheets.Add("replayName");
                int rowNumber = 1;
                foreach (var path in new ReplayFetcher().GetAllReplaysPath())
                {
                    replayWorksheet.SetValue(rowNumber, 1, Path.GetFileNameWithoutExtension(path));
                    using (var replayStream = File.OpenRead(path))
                    using (var fortniteVisitor = Substitute.ForPartsOf<FortniteReplayVisitor>(replayStream))
                    {
                        await fortniteVisitor.When((x) => x.)
                        replayWorksheet.SetValue(rowNumber, 1, );
                        await fortniteVisitor.Visit();
                    }
                    rowNumber++;
                }
                excelPackage.SaveAs(new FileInfo("ExcelExport.xlsx"));
            }
        }
    }
}
