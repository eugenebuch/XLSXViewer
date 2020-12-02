using System.Collections.Generic;
using System.Linq;
using XLSXViewer.Models;
using ClosedXML.Excel;
using System.IO;

namespace XLSXViewer.Tools
{
    public static class ExcelReader
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static IEnumerable<Threat> Read(string fileSource)
        {
            lock (fileSource)
            {
                if (File.Exists(fileSource))
                {
                    using (var wb = new XLWorkbook(fileSource))
                    {
                        var ws = wb.Worksheets.Worksheet(1);
                        var rows = ws.RangeUsed().RowsUsed();
                        foreach (var row in rows.Skip(2))
                        {
                            var metric = new Threat()
                            {
                                Id = "УБИ." + row.Cell(1).CachedValue.ToString().PadLeft(3, '0'),
                                Name = row.Cell(2).CachedValue.ToString(),
                                Description = row.Cell(3).CachedValue.ToString(),
                                Source = row.Cell(4).CachedValue.ToString(),
                                Object = row.Cell(5).CachedValue.ToString(),
                                Privacy = row.Cell(6).CachedValue.ToString() == "1" ? "да" : "нет",
                                Integrity = row.Cell(7).CachedValue.ToString() == "1" ? "да" : "нет",
                                Availabilty = row.Cell(8).CachedValue.ToString() == "1" ? "да" : "нет"
                            };

                            yield return metric;
                        }
                        ws.Clear();
                    }
                }
                else 
                {
                    logger.Warn($"Попытка чтения Excel из пустого файла. Файл ожидался в: [{fileSource}]");
                }
            }
        }
    }
}
