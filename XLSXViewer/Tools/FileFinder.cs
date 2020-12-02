using System;
using System.ComponentModel;
using System.IO;

namespace XLSXViewer.Tools
{
    public static class FileFinder
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static string pathDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private static string fileName = "Task.xlsx";

        public static string FileSource { get; set; }

        public static void Find(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (var findedFile in Directory.EnumerateFiles(pathDir, fileName, SearchOption.AllDirectories))
                {
                    FileInfo fileInfo;

                    fileInfo = new FileInfo(findedFile);
                    FileSource = fileInfo.FullName;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error(ex, "Попытка прочитать защищённый файл");
            }
            catch (PathTooLongException ex)
            {
                logger.Error(ex, "Путь к файлу оказался слишком длинным");
            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex, "Цель поиска файла оказалась null");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Получена неожиданная ошибка в {nameof(FileFinder)}");
            }
        }
    }
}
