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
        private bool isOldYet = false;
        private bool isOldYetReversed;
        private Threat selectedThreat;
        private Threat selectedOldThreat;
        private ObservableCollection<Threat> threatsOnPage;
        private ObservableCollection<Threat> oldThreats;
        private int itemsOnPage;
        private int currentPage = 1;
        private int lastPage;
        private bool flag;
        private string paginationView = "";
        private ICommand _nextPageCommand;
        private ICommand _previousPageCommand;
        private ICommand _firsPageCommand;
        private ICommand _lastPageCommand;
        private ObservableCollection<Threat> difference;
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

            if (LoaderViewModel.isLoaded)
            {
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
                    if (value == null) { SelectedOldThreat = null; }
                    else { SelectedOldThreat = oldThreats.FirstOrDefault(x => x.Id == value.Id); }
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
                if (value != null)
                {
                    selectedOldThreat = value;
                }
                else
                {
                    selectedOldThreat = new Threat() { Id = "*НОВАЯ УГРОЗА*" };
                }
                OnPropertyChanged(nameof(SelectedOldThreat));
            }
        }

        public static ObservableCollection<Threat> Threats { get; set; }

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

        public bool IsOldYetReversed
        {
            get => isOldYetReversed;
            set
            {
                if (isOldYetReversed != value)
                {
                    isOldYetReversed = value;
                    OnPropertyChanged(nameof(isOldYetReversed));
                }
            }
        }

        public ObservableCollection<Threat> ThreatsOnPage
        {
            get => threatsOnPage;
            set
            {
                if (threatsOnPage != value)
                {
                    threatsOnPage = value;
                    OnPropertyChanged(nameof(ThreatsOnPage));
                }
            }
        }

        public string Title
        {
            get => title;
            set
            {
                if (title!= value)
                {
                    title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string PaginationView
        {
            get => paginationView;
            set
            {
                if (paginationView != value)
                {
                    paginationView = value;
                    OnPropertyChanged(nameof(PaginationView));
                }
            }
        }

        public List<int> PaggingCounts { get; } = new List<int>() { 15, 30, 50, 100 };

        public int ItemsOnPage
        {
            get => itemsOnPage;
            set
            {
                if (itemsOnPage != value)
                {
                    itemsOnPage = value;
                    OnPropertyChanged(nameof(ItemsOnPage));
                    currentPage = 1;
                    RefreshDataOnPage();
                }
            }
        }

        /// <summary>
        /// Commands
        /// </summary>

        public ICommand NextPageCommand
        {
            get => _nextPageCommand = _nextPageCommand ?? new DelegateCommand(param =>
            {
                currentPage += 1;
                RefreshDataOnPage();
            }, 
                CanExecuteNextPage);
        }

        public ICommand PreviousPageCommand
        {
            get => _previousPageCommand = _previousPageCommand ?? new DelegateCommand(param =>
            {
                currentPage -= 1;
                RefreshDataOnPage();
            },
                CanExecutePreviousPage);
        }

        public ICommand FirstPageCommand
        {
            get => _firsPageCommand = _firsPageCommand ?? new DelegateCommand(param =>
            {
                currentPage = 1;
                RefreshDataOnPage();
            },
                CanExecutePreviousPage);
        }

        public ICommand LastPageCommand
        {
            get => _lastPageCommand = _lastPageCommand ?? new DelegateCommand(param =>
            {
                currentPage = lastPage;
                RefreshDataOnPage();
            },
                CanExecuteNextPage);
        }

        public ICommand OnMainCommand { get; set; }
        /// <summary>
        /// Methods
        /// </summary>
        private void RefreshDataOnPage()
        {
            title = $"Просмотр угроз - [{fileSource}]";
            if (Threats.Count == 0) { return; }
            var src = new ObservableCollection<Threat>();
            if (IsOldYet)
            {
                src = difference;
            }
            else
            {
                src = Threats;
            }

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
