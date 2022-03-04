using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using XLSXViewer.Models;
using XLSXViewer.Tools;

namespace XLSXViewer.ViewModels
{
    public class AppViewModel : ViewModelBase
    {
        /// <summary>
        /// VM instantiation
        /// </summary>
        private bool isOldYet;
        private bool isOldYetReversed;
        private Threat selectedThreat;
        private Threat selectedOldThreat;
        private ObservableCollection<Threat> threatsOnPage;
        private readonly ObservableCollection<Threat> oldThreats;
        private int itemsOnPage;
        private int currentPage = 1;
        private int lastPage;
        private readonly bool flag;
        private string paginationView = "";
        private ICommand nextPageCommand;
        private ICommand previousPageCommand;
        private ICommand firsPageCommand;
        private ICommand lastPageCommand;
        private readonly ObservableCollection<Threat> difference;
        private string title = "Просмотр угроз";
        private string fileSource;

        public AppViewModel(bool flag = false)
        {
            isOldYetReversed = !isOldYet;

            var worker = new BackgroundWorker();
            worker.DoWork += FileFinder.Find;
            worker.RunWorkerAsync();

            oldThreats = Threats ?? new ObservableCollection<Threat>();
            itemsOnPage = PaggingCounts[0];
            Threats = new ObservableCollection<Threat>();
            difference = new ObservableCollection<Threat>();
            ThreatsOnPage = new ObservableCollection<Threat>();

            if (!LoaderViewModel.IsLoaded) return;
            this.flag = flag;
            worker = new BackgroundWorker();
            worker.DoWork += WorkWithData;
            worker.RunWorkerAsync();


            OnMainCommand = new DelegateCommand(param =>
            {
                IsOldYet = false;
                RefreshDataOnPage();
            });
        }

        /// <summary>
        /// Properties
        /// </summary>
        /// 

        public Threat SelectedThreat
        {
            get => selectedThreat;
            set
            {
                if (isOldYet)
                {
                    SelectedOldThreat = value == null ? null : oldThreats.FirstOrDefault(x => x.Id == value.Id);
                }

                selectedThreat = value;
                OnPropertyChanged(nameof(SelectedThreat));
            }
        }

        public Threat SelectedOldThreat
        {
            get => selectedOldThreat;
            set
            {
                selectedOldThreat = value ?? new Threat() { Id = "*НОВАЯ УГРОЗА*" };
                OnPropertyChanged(nameof(SelectedOldThreat));
            }
        }

        private static ObservableCollection<Threat> Threats { get; set; }

        public bool IsOldYet
        {
            get => isOldYet;
            set
            {
                if (isOldYet != value)
                {
                    IsOldYetReversed = !value;
                    isOldYet = value;
                    OnPropertyChanged(nameof(IsOldYet));
                }
            }
        }

        private bool IsOldYetReversed
        {
            get => isOldYetReversed;
            set
            {
                if (isOldYetReversed == value) return;
                isOldYetReversed = value;
                OnPropertyChanged(nameof(isOldYetReversed));
            }
        }

        public ObservableCollection<Threat> ThreatsOnPage
        {
            get => threatsOnPage;
            set
            {
                if (threatsOnPage == value) return;
                threatsOnPage = value;
                OnPropertyChanged(nameof(ThreatsOnPage));
            }
        }

        public string Title
        {
            get => title;
            set
            {
                if (title == value) return;
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string PaginationView
        {
            get => paginationView;
            set
            {
                if (paginationView == value) return;
                paginationView = value;
                OnPropertyChanged(nameof(PaginationView));
            }
        }

        private List<int> PaggingCounts { get; } = new List<int>() { 15, 30, 50, 100 };

        public int ItemsOnPage
        {
            get => itemsOnPage;
            set
            {
                if (itemsOnPage == value) return;
                itemsOnPage = value;
                OnPropertyChanged(nameof(ItemsOnPage));
                currentPage = 1;
                RefreshDataOnPage();
            }
        }

        /// <summary>
        /// Commands
        /// </summary>

        public ICommand NextPageCommand =>
            nextPageCommand ??= new DelegateCommand(param =>
                {
                    currentPage += 1;
                    RefreshDataOnPage();
                }, 
                CanExecuteNextPage);

        public ICommand PreviousPageCommand =>
            previousPageCommand ??= new DelegateCommand(param =>
                {
                    currentPage -= 1;
                    RefreshDataOnPage();
                },
                CanExecutePreviousPage);

        public ICommand FirstPageCommand =>
            firsPageCommand ??= new DelegateCommand(param =>
                {
                    currentPage = 1;
                    RefreshDataOnPage();
                },
                CanExecutePreviousPage);

        public ICommand LastPageCommand =>
            lastPageCommand ??= new DelegateCommand(param =>
                {
                    currentPage = lastPage;
                    RefreshDataOnPage();
                },
                CanExecuteNextPage);

        public ICommand OnMainCommand { get; set; }
        /// <summary>
        /// Methods
        /// </summary>
        private void RefreshDataOnPage()
        {
            title = $"Просмотр угроз - [{fileSource}]";
            if (Threats.Count == 0) { return; }

            var src = IsOldYet ? difference : Threats;

            lastPage = src.Count % itemsOnPage > 0 ? src.Count / itemsOnPage + 1 : src.Count / itemsOnPage;
            PaginationView = $"{currentPage}/{lastPage}";
            var renew = new ObservableCollection<Threat>();
            var currentStart = itemsOnPage * (currentPage - 1);
            var currentEnd = currentStart + ((src.Count - currentStart) < itemsOnPage ? (src.Count - currentStart) : itemsOnPage);
            for (int i = currentStart; i < currentEnd; i++)
            {
                renew.Add(src[i]);
            }
            ThreatsOnPage = renew;
        }

        private void WorkWithData(object sender, DoWorkEventArgs e)
        {
            fileSource = FileFinder.FileSource ?? LoaderViewModel.Source;
            if (flag)
            {
                foreach (var elem in ExcelReader.Read(fileSource))
                {
                    if (!oldThreats.Contains(elem) && oldThreats.Count != 0)
                    {
                        IsOldYet = true;
                        difference.Add(elem);
                    }
                    Threats.Add(elem);
                }
                if (oldThreats.Count != 0)
                {   // MVVM unfriendly
                    MessageBox.Show(difference.Count == 0 ? "Обновлений нет" : $"Обновлено строк {difference.Count}",
                        "Обновление", MessageBoxButton.OK);
                }
                RefreshDataOnPage();
            }
        }

        public bool CanExecuteNextPage(object parameter) => currentPage < lastPage;

        private bool CanExecutePreviousPage(object obj) => currentPage > 1; 
    }
}
