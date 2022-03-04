using Microsoft.VisualStudio.PlatformUI;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Input;

namespace XLSXViewer.ViewModels
{
    public class LoaderViewModel : ViewModelBase
    {
        private readonly BackgroundWorker worker;
        private int currentProgress;
        private string progressVisibility = "Нажмите 'Загрузить' для начала загрузки файла";
        private bool isNotLoaded = true;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly  WebClient client = new WebClient();

        public static bool IsLoaded;
        public LoaderViewModel()
        {
            IsLoaded = false;

            worker = new BackgroundWorker {WorkerReportsProgress = true};
            worker.DoWork += DoWork;
            InstigateWorkCommand =
                new DelegateCommand(o => worker.RunWorkerAsync(),
                    o => !worker.IsBusy);
        }

        public ICommand InstigateWorkCommand { get; }

        public bool IsNotLoaded { 
            get => isNotLoaded; 
            set
            {
                isNotLoaded = value;
                OnPropertyChanged(nameof(IsNotLoaded));
            } 
        }

        internal static string Url { get; } = @"https://bdu.fstec.ru/files/documents/thrlist.xlsx";
        internal static string Source { get; set; } = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Task.xlsx";

        public int CurrentProgress
        {
            get => currentProgress;
            set
            {
                if (currentProgress != value)
                {
                    currentProgress = value;
                    OnPropertyChanged(nameof(CurrentProgress));
                }
            }
        }

        public string ProgressVisibility
        {
            get => progressVisibility;
            set
            {
                if (progressVisibility == value) return;
                progressVisibility = value;
                OnPropertyChanged(nameof(ProgressVisibility));
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                client.DownloadProgressChanged += ProgressChanged;
                client.DownloadFileCompleted += DownloadCompleted;
                client.DownloadFileAsync(new Uri(Url), Source);
            }
            catch (WebException ex)
            {
                client.CancelAsync();
                ProgressVisibility =  "Ошибка сети, проверьте подключение";
                Logger.Fatal(ex, "Отсутствовало подключение к сети");
            }
            catch (ArgumentNullException ex)
            {
                ProgressVisibility = "Ошибка: Путь к файлу оказался Null";
                Logger.Error(ex, "Путь для сохранения файла оказался Null");
            } 
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var s = (DownloadProgressChangedEventArgs)e;
            CurrentProgress = e.ProgressPercentage;
            ProgressVisibility = new string(' ', 2) + s.BytesReceived + @"/" + s.TotalBytesToReceive + " байт";
        }
        
        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (currentProgress > 0 && !client.IsBusy)
            {
                IsLoaded = true;
                IsNotLoaded = !IsLoaded;
                ProgressVisibility += $"\nЗагрузка завершена. Файл загружен в: {Source}";
                Tools.FileFinder.FileSource = Source;
            }
            else
            {
                client.CancelAsync();

                IsLoaded = false;
                ProgressVisibility = "Ошибка сети, проверьте подключение";
                Logger.Fatal("Отсутствовало подключение к сети");
                var file = new FileInfo(Source);
                if (file.Exists) file.Delete();
            }
        }
    }
}
