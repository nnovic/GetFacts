using GetFacts.Download;
using GetFacts.Render;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GetFacts
{
    /// <summary>
    /// Logique d'interaction pour MainGui.xaml
    /// </summary>
    public partial class MainGui : Window
    {

        public MainGui()
        {
            InitializeComponent();
            navigator.Navigate(new WelcomePage());
        }

        #region rotation of pages

        const int PageDisplayDuration = 5000;
        private Thread rotationThread = null;
        private bool isRotating = false;
        private readonly object pauseLock = new object();
        private uint pauseCounter = 0;
        private Stopwatch chrono = new Stopwatch();

        private void PauseIfRequired()
        {
            lock(pauseLock)
            {
                do
                {
                    chrono.Restart();
                    RemoveTrash();
                    chrono.Stop();

                    long elapsed = chrono.ElapsedMilliseconds;
                    int sleepDuration = Math.Min(PageDisplayDuration, (int)(PageDisplayDuration - elapsed));
                    if (sleepDuration < 100) sleepDuration = 100;

                    Monitor.Wait(pauseLock, sleepDuration);

                } while (pauseCounter > 0);
            }
        }

        private void Pause()
        {
            lock(pauseLock)
            {
                pauseCounter++;
                Monitor.Pulse(pauseLock);
            }
        }

        private void Resume()
        {
            lock (pauseLock)
            {
                if(pauseCounter>0) pauseCounter--;
                Monitor.Pulse(pauseLock);
            }
        }

        private void RotationTask()
        {
            Page nextPage = null;
            Queue<Page> poolOfPages = new Queue<Page>();
            Facts.Facts.GetInstance().Initialize();

            while (isRotating)
            {
                try
                {
                    PauseIfRequired();

                    Dispatcher.Invoke(() => 
                    {
                        if (poolOfPages.Count == 0)
                        {
                            List<Page> tmp = Facts.Facts.GetInstance().CreateNextPages();
                            if (tmp != null) { foreach (Page p in tmp) { poolOfPages.Enqueue(p); } }
                        }

                        // If there was a "nextPage" set from
                        // previous iteration of the loop,
                        // then check if some tidying up is required
                        // before selecting another page.
                        if (nextPage != null)
                        {
                            if (nextPage is IFreezable freezable)
                            {
                                freezable.Frozen -= Freezable_Frozen;
                                freezable.Unfrozen -= Freezable_Unfrozen;
                            }
                        }

                        if (poolOfPages.Count > 0)
                        {
                            nextPage = poolOfPages.Dequeue();
                        }
                        else
                        {
                            nextPage = null;
                        }

                        if( nextPage!=null )
                        {
                            if (nextPage is IFreezable freezable)
                            {
                                freezable.Frozen += Freezable_Frozen;
                                freezable.Unfrozen += Freezable_Unfrozen;
                            }
                            navigator.Navigate(nextPage);
                            navigator.RemoveBackEntry();
                        }
                    });
                    
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private void Freezable_Unfrozen(object sender, FreezeEventArgs e)
        {
            Console.WriteLine("RESUME");
            Resume();
        }

        private void Freezable_Frozen(object sender, FreezeEventArgs e)
        {
            Console.WriteLine("PAUSE");
            Pause();
        }

        private void StopRotationThread()
        {
            if(rotationThread!=null)
            {
                isRotating = false;
                rotationThread.Interrupt();
            }
        }

        private void StartpRotationThread()
        {
            if(rotationThread==null)
            {
                rotationThread = new Thread(RotationTask) {Name = "RotationTask" };
                isRotating = true;
                rotationThread.Start();
            }
        }

        #endregion

        #region removal of unsued files
        
        private void RemoveTrash()
        {

            //remove active urls from downloads and
            //delete what remains:
            ISet<string> activeUrls = Facts.Facts.GetInstance().GetAllUrls();
            ISet<string> downloads = DownloadManager.GetInstance().GetAllUrls();
            IEnumerable<string> deleteThoseDownloads = downloads.Except(activeUrls);
            DownloadManager.GetInstance().RemoveTasks(deleteThoseDownloads);

            // remove active files from content of the download directory
            // to obtain orphaned files, and remove those.
            string downloadDir = ConfigurationManager.AppSettings.Get("DownloadDirectory");
            ISet<string> activeFiles = DownloadManager.GetInstance().GetAllFilesInUse();
            IEnumerable<string> tempFiles = Directory.EnumerateFiles(downloadDir);
            IEnumerable<string> deleteThoseFiles = tempFiles.Except(activeFiles);
            DownloadManager.GetInstance().DeleteFiles(deleteThoseFiles);
        }

        #endregion


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartpRotationThread();
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopRotationThread();
            DownloadManager.GetInstance().Stop();
        }
    }
}
