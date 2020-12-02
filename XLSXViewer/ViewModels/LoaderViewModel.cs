using Microsoft.VisualStudio.PlatformUI;
using System;
using System.ComponentModel;
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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool isLoaded;
        public LoaderViewModel()
        {
            isLoaded = false;
            InstigateWorkCommand =
                    new DelegateCommand(o => worker.RunWorkerAsync(),
                                        o => !worker.IsBusy);

            worker = new BackgroundWorker();
            worker.DoWork += DoWork;
            worker.ProgressChanged += ProgressChanged;
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
                if (progressVisibility != value)
                {
                    progressVisibility = value;
                    OnPropertyChanged(nameof(ProgressVisibility));
                }
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var client = new WebClient();
                client.DownloadProgressChanged += ProgressChanged;
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
                client.DownloadFileAsync(new Uri(Url), Source);
            }
            catch (WebException ex)
            {
                ProgressVisibility =  "Ошибка сети, проверьте подключение";
                logger.Fatal(ex, "Отсутствовало подключение к сети");
            }
            catch (ArgumentNullException ex)
            {
                ProgressVisibility = "Ошибка: Путь к файлу оказался Null";
                logger.Error(ex, "Путь для сохранения файла оказался Null");
            } 
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var s = (DownloadProgressChangedEventArgs)e;
            CurrentProgress = e.ProgressPercentage;
            ProgressVisibility = new string(' ', 2) +s.BytesReceived + @"/" + s.TotalBytesToReceive + " байт";
        }
        
        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (currentProgress > 0)
            {
                isLoaded = true;
                IsNotLoaded = !isLoaded;
                ProgressVisibility += $"\nЗагрузка завершена. Файл загружен в: {Source}";
                Tools.FileFinder.FileSource = Source;
            }
            else
            {
                isLoaded = false;
                ProgressVisibility = "Ошибка сети, проверьте подключение";
                logger.Fatal("Отсутствовало подключение к сети");
            }
        }
    }
}
