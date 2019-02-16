using Common.StreamHelpers;
using FortniteReplayParser;
using OfficeOpenXml;
using System;
using System.IO;
using System.Threading.Tasks;
using UnrealReplayParser;
using UnrealReplayParser.Tests;
namespace ChartsNite.ReplayOrganizer
{
    class Program
    {
        static async Task Main()
        {
            using( var excelPackage = new ExcelPackage() )
            {
                ExcelWorksheet replayWorksheet = excelPackage.Workbook.Worksheets.Add( "replayName" );
                DataDumper dataDumper = new DataDumper( replayWorksheet );
                foreach( var path in new ReplayFetcher().GetAllReplaysPath() )
                {
                    dataDumper.DumpValue( Path.GetFileNameWithoutExtension( path ) );
                    using( var replayStream = File.OpenRead( path ) )
                    using( var fortniteDataGrabber = new FortniteDataGrabber( replayStream ) )
                    {
                        await fortniteDataGrabber.Visit();
                        if( fortniteDataGrabber.ReplayInfo == null )
                        {
                            dataDumper.DumpValue( "ReplayInfo is NULL." );
                            continue;
                        }
                        ReplayInfo info = fortniteDataGrabber.ReplayInfo;
                        dataDumper.DumpValue( info.BCompressed );
                        dataDumper.DumpValue( info.BIsLive );
                        dataDumper.DumpValue( info.Changelist );
                        dataDumper.DumpValue( info.FileVersion );
                        dataDumper.DumpValue( info.FriendlyName );
                        dataDumper.DumpValue( info.LengthInMs );
                        dataDumper.DumpValue( info.NetworkVersion );
                        dataDumper.DumpValue( info.Timestamp );
                        dataDumper.DumpValue( info.TotalDataSizeInBytes );
                        Console.WriteLine( "Processed: " + Path.GetFileNameWithoutExtension( path ) );
                        dataDumper.ReturnToNewRow();
                    }
                }
                excelPackage.SaveAs( new FileInfo( "ExcelExport.xlsx" ) );
            }
        }
    }

    class DataDumper
    {
        readonly ExcelWorksheet _excelWorksheet;
        int _rowNumber;
        int _colNumber;
        public DataDumper( ExcelWorksheet excelWorksheet )
        {
            _rowNumber = 1;
            _colNumber = 1;
            _excelWorksheet = excelWorksheet;
        }

        public void DumpValue( object dataToDump )
        {
            _excelWorksheet.SetValue( _rowNumber, _colNumber, dataToDump );
            _colNumber++;
        }

        public int ReturnToNewRow()
        {
            _colNumber = 1;
            _rowNumber++;
            return _rowNumber;
        }

    }
}
