using GetFacts.Download;
using GetFacts.Render;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

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

        const int TimeoutInterval = 1;
        const int DefaultPauseDuration = 10; // secondes
        private Thread rotationThread = null;
        private bool isRotating = false;
        private readonly object pauseLock = new object();
        private uint pauseForZoom_counter = 0;
        private uint pauseForRead_counter = 0;
        private Stopwatch chrono = new Stopwatch();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minDuration">(en secondes)</param>
        /// <param name="maxDuration">(en secondes)</param>
        private void PauseIfRequired(int minDuration, int maxDuration)
        {
            lock (pauseLock)
            {
                long remainingTime = maxDuration * 1000;
                long elapsedTime = 0;
                bool timedOut; 
                chrono.Restart();
                RemoveTrash();
                chrono.Stop();

                // décompter la durée du nettoyage du temps
                // d'affichage restant.
                remainingTime -= chrono.ElapsedMilliseconds;

                // ajouter la durée du nettoyage au temps
                // effectif d'affichage.
                elapsedTime += chrono.ElapsedMilliseconds;

                do
                {                    
                    chrono.Restart();                    
                    timedOut = !Monitor.Wait(pauseLock, remainingTime>(TimeoutInterval *1000)? (int)remainingTime:(TimeoutInterval*1000));
                    chrono.Stop();

                    // décompter la durée du blocage du
                    // temps d'affichage restant, quelle que
                    // soit la raison du blocage.
                    remainingTime -= chrono.ElapsedMilliseconds;

                    // ajouter la durée du blocage au temps
                    // effectif d'afficage, mais uniquement
                    // si la pause n'est pas provoquée 
                    // par un zoom d'un média
                    if(pauseForZoom_counter==0)
                        elapsedTime += chrono.ElapsedMilliseconds;

                    //Console.WriteLine("Pause: RT={0}/{1}, ET={2}/{3}",
                    //    remainingTime, MaxPageDisplayDuration*1000,
                    //    elapsedTime,MinPageDisplayDuration*1000);

                } while( (timedOut==false) 
                || ( (pauseForRead_counter>0) 
                    || (pauseForZoom_counter > 0) 
                    || (remainingTime>0) 
                    || (elapsedTime<1000*minDuration)) );
            }
        }

        private void Pause(CauseOfFreezing cause)
        {
            lock(pauseLock)
            {
                switch (cause)
                {
                    case CauseOfFreezing.CursorOnArticle: pauseForRead_counter++; break;
                    case CauseOfFreezing.ZoomOnMedia: pauseForZoom_counter++; break;
                }
                Monitor.Pulse(pauseLock);
            }
        }

        private void Resume(CauseOfFreezing cause)
        {
            lock (pauseLock)
            {
                switch (cause)
                {
                    case CauseOfFreezing.CursorOnArticle: if(pauseForRead_counter>0) pauseForRead_counter--; break;
                    case CauseOfFreezing.ZoomOnMedia: if(pauseForZoom_counter>0) pauseForZoom_counter--; break;
                }
                Monitor.Pulse(pauseLock);
            }
        }

        private void RotationTask()
        {
            int minPauseDuration = DefaultPauseDuration;
            int maxPauseDuration = DefaultPauseDuration;
            Page nextPage = null;
            Queue<Page> poolOfPages = new Queue<Page>();
            Facts.Facts.GetInstance().Initialize();

            while (isRotating)
            {
                try
                {
                    PauseIfRequired(minPauseDuration, maxPauseDuration);

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

                        if((nextPage!=null) && (nextPage is ICustomPause))
                        {
                            ICustomPause icp = (ICustomPause)nextPage;
                            minPauseDuration = icp.MinPageDisplayDuration;
                            maxPauseDuration = icp.MaxPageDisplayDuration;
                        }
                        else
                        {
                            minPauseDuration = DefaultPauseDuration;
                            maxPauseDuration = DefaultPauseDuration;
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
            Resume(e.Cause);
        }

        private void Freezable_Frozen(object sender, FreezeEventArgs e)
        {            
            Pause(e.Cause);
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
            string downloadDir = ConfigFactory.GetInstance().CacheDirectory;
            ISet<string> activeFiles = DownloadManager.GetInstance().GetAllFilesInUse();
            IEnumerable<string> tempFiles = Directory.EnumerateFiles(downloadDir);
            IEnumerable<string> deleteThoseFiles = tempFiles.Except(activeFiles);
            DownloadManager.GetInstance().DeleteFiles(deleteThoseFiles);
        }

        #endregion


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refreshIndication.Visibility = Visibility.Hidden;
            StartpRotationThread();
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopRotationThread();
            DownloadManager.GetInstance().Stop();
        }
    }
}
