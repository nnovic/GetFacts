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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace GetFacts
{
    /// <summary>
    /// Logique d'interaction pour MainGui.xaml
    /// </summary>
    public partial class MainGui : Window
    {

        private readonly DispatcherTimer progressBarTimer;
        private const double progressBarResolution = 0.25;

        public MainGui()
        {
            InitializeComponent();
            navigator.Navigate(new WelcomePage());

            progressBarTimer = new DispatcherTimer();
            progressBarTimer.Tick += ProgressBarTimer_Tick;
            progressBarTimer.Interval = TimeSpan.FromSeconds(progressBarResolution);
        }

        private bool blinker;
        private void ProgressBarTimer_Tick(object sender, EventArgs e)
        {
            blinker = !blinker;
            double value = timerProgressValue.Value;
            value = Math.Min(value + progressBarResolution, timerProgressValue.Maximum);
            timerProgressValue.Value = value;

            object o = navigator.Content;

            if( (o is IHostsInformation hostsNews) && blinker)
            {
                TrayIcon.Icon = hostsNews.HasNewInformation ? GetFacts.Properties.Resources.NewFactsIcon : GetFacts.Properties.Resources.GetFactsIcon;
            }
            else
            {
                TrayIcon.Icon = GetFacts.Properties.Resources.GetFactsIcon;
            }
        }

        private void UpdateTooltips(object o)
        {
            StringBuilder title = new StringBuilder();

            title.Append("GetFacts");

            if (o is IHostsInformation host)
            {
                if (string.IsNullOrEmpty(host.InformationHeadline) == false)
                {
                    title.Append(" - ");
                    title.Append(host.InformationHeadline);
                }

                if (host.HasNewInformation)
                {
                    TrayIcon.ShowBalloonTip(host.InformationHeadline, host.InformationSummary, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                }
            }

            this.Title = title.ToString();
            TrayIcon.ToolTipText = title.ToString();
        }

        #region rotation of pages

        const int PauseGranularity_seconds = 1;
        const int DefaultPauseDuration = 10; // secondes
        private Thread rotationThread = null;
        private bool isRotating = false;
        private readonly object pauseLock = new object();
        private uint pauseForZoom_counter = 0;
        private uint pauseForRead_counter = 0;
        private bool skipOrder = false;
        private Stopwatch chrono = new Stopwatch();

        /// <summary>
        /// Bloque le thread si nécessaire, en fonction des durées min et max
        /// spécifiées en argument.
        /// </summary>
        /// <remarks>La durée effective de la pause est mesurée dans la variable 'totalPauseDuration_msec'. le thread appelant restera bloqué tant
        /// que 'totalPauseDuration_msec' est inférieur à l'équivalent en millisecondes de 'minDuration'.</remarks>
        /// <param name="minDuration">(en secondes)</param>
        /// <param name="maxDuration">(en secondes)</param>
        private void PauseIfRequired(int minDuration, int maxDuration)
        {
            lock (pauseLock)
            {
                long remainingTime = maxDuration * 1000;
                long totalPauseDuration_msec = 0; // mesure la durée effective totale de la pause.
                bool timedOut; 

                chrono.Restart();
                RemoveTrash();
                chrono.Stop();

                // décompter la durée du nettoyage du temps
                // d'affichage restant.
                remainingTime -= chrono.ElapsedMilliseconds;

                // ajouter la durée du nettoyage au temps
                // effectif d'affichage.
                totalPauseDuration_msec += chrono.ElapsedMilliseconds;

                do
                {                    
                    chrono.Restart();
                    timedOut = !Monitor.Wait(pauseLock, remainingTime>(PauseGranularity_seconds *1000)? (int)remainingTime:(PauseGranularity_seconds*1000));
                    if(skipOrder==true)
                    {
                        skipOrder = false;
                        return;
                    }
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
                        totalPauseDuration_msec += chrono.ElapsedMilliseconds;

                } while( (timedOut==false) 
                || ( (pauseForRead_counter>0) 
                    || (pauseForZoom_counter > 0) 
                    || (remainingTime>0) 
                    || (totalPauseDuration_msec<1000*minDuration)) );
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

        private void Skip()
        {
            lock (pauseLock)
            {
                pauseForRead_counter = 0;
                pauseForZoom_counter = 0;
                skipOrder = true;
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
                    Dispatcher.Invoke(()=> 
                    {
                        timerProgressValue.Minimum = 0;
                        timerProgressValue.Maximum = maxPauseDuration;
                        timerProgressValue.Value = 0;
                        progressBarTimer.Start();
                    });
                    
                    PauseIfRequired(minPauseDuration, maxPauseDuration);

                    Dispatcher.Invoke(() =>
                    {
                        progressBarTimer.Stop();
                        timerProgressValue.Minimum = 0;
                        timerProgressValue.Maximum = maxPauseDuration;
                        timerProgressValue.Value = 0;                        
                    });

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
                            UpdateTooltips(nextPage);
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

        /// <summary>
        /// Lance le thread qui gère l'affichage des 
        /// différentes pages contenant les infos collectées.
        /// </summary>
        /// <see cref="RotationTask"/>
        /// <seealso cref="StopRotationThread"/>
        private void StartRotationThread()
        {
            if(rotationThread==null)
            {
                rotationThread = new Thread(RotationTask) {Name = "RotationTask" };
                isRotating = true;
                rotationThread.Start();
            }
        }

        #endregion

        #region removal of unused files
        
        /// <summary>
        /// TODO
        /// Problèmes possibles: 
        /// - supprime les fichiers
        /// associés à des pages qui sont désactviées
        /// dans la configuration utilisateur.
        /// </summary>
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
            GetFacts.Facts.Facts.GetInstance().PageRefreshEvent += MainGui_PageRefreshEvent;
            RefreshIndication.Visibility = Visibility.Collapsed;
            NotificationIndication.Visibility = Visibility.Collapsed;
            StartRotationThread();
        }

        private void MainGui_PageRefreshEvent(object sender, Facts.Facts.PageRefreshEventArgs e)
        {
            Dispatcher.Invoke(() => {
                RefreshIndication.Visibility = e.Begins ? Visibility.Visible : Visibility.Collapsed;
            });            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TrayIcon.Dispose();
            NotificationSystem.GetInstance().Notifications.CollectionChanged -= Notifications_CollectionChanged;
            StopRotationThread();
            DownloadManager.GetInstance().Stop();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            NotificationSystem.GetInstance().Notifications.CollectionChanged += Notifications_CollectionChanged;
            WindowPosition wp = ConfigFactory.GetInstance().WindowPosition;
            wp?.ApplyTo(this);
            UpdateTooltips(null);
        }

        /// <summary>
        /// Montrer/Masquer le bouton "Messages"
        /// suivant le nombre de notifications dans
        /// NotificationSystem.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <seealso cref="NotificationSystem"/>
        private void Notifications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    int count = NotificationSystem.GetInstance().Notifications.Count;
                    Visibility v = count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    NotificationIndication.Visibility = v;
                }
                catch
                {
                }
            }), null);
        }

        private void ToggleMenuButton_Checked(object sender, RoutedEventArgs e)
        {
            ButtonsPanel.Visibility = Visibility.Visible;
        }

        private void ToggleMenuButton_Unchecked(object sender, RoutedEventArgs e)
        {
            HideEverythingBut(ToggleMenuButton, SkipButton2);
        }

        private void HideEverythingBut(params UIElement[] doNotHide)
        {
            foreach(UIElement control in UserInputsGrid.Children)
            {
                if( doNotHide.Contains(control)==false )
                {
                    control.Visibility = Visibility.Hidden;
                }
            }
        }

        private void ShowMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            HideEverythingBut(ToggleMenuButton, SkipButton2, ButtonsPanel);
            NotificationsPanel.Visibility = Visibility.Visible;
        }

        private void ConfigMenuButton_Click(object sender, RoutedEventArgs e)
        {
            HideEverythingBut(ToggleMenuButton, SkipButton2, ButtonsPanel);
            ConfigurationPanel.Visibility = Visibility.Visible;
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            Skip();
        }

        private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            //this.Activate();
        }

        private void TrayIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            this.Activate();
        }
    }
}
