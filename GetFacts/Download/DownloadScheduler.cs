using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Download
{
    internal class DownloadScheduler
    {
        private const int MIN_PRIO_FILTER = 1;
        private const int MAX_PRIO_FILTER = 4;
        private const int INDEX_NO_TASK = -1;
        private const int MAX_ACTIVE_TASKS = 1;

        private readonly Collection<DownloadTask> downloads;
        private int taskIndex = INDEX_NO_TASK;
        private int priorityFilter = MIN_PRIO_FILTER;
        //private int activeDownloads = 0;

        internal DownloadScheduler(Collection<DownloadTask> list)
        {
            downloads = list;
        }

        private int CountActiveTasks(int sameOrHigherPriority)
        {
            int count = 0;
            foreach(DownloadTask task in downloads)
            {
                if (task.Status == DownloadTask.DownloadStatus.Started)
                {
                    if (task.Priority <= sameOrHigherPriority)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        internal DownloadTask Next()
        {
            DownloadTask task = null;
            int maxTrials = downloads.Count * (1+MAX_PRIO_FILTER-MIN_PRIO_FILTER);
            int trialCount = 0;
            

            do
            {
                trialCount++;
                task = GetNextActivableTask();

                if( task!=null )
                {
                    if( task.Status== DownloadTask.DownloadStatus.ReadyToStart )
                    {
                        int activeTasks = CountActiveTasks(task.Priority);
                        if( activeTasks>=MAX_ACTIVE_TASKS )
                        {
                            task = null;
                        }
                    }
                }
               

            } while ((task == null) && (trialCount < maxTrials));

            return task;
        }

        private DownloadTask GetNextActivableTask()
        {
            DownloadTask task = GetNextTask();

            // ignorer la tache si elle est finie:
            if (task != null)
            {
                if (task.IsFinished) task = null;
            }

            // ignorer la tache si elle n'est pas à la bonne priorité
            if (task != null)
            {
                if ((priorityFilter % task.Priority) != 0) task = null;
            }

            return task;
        }

        private DownloadTask GetNextTask()
        {
            DownloadTask task = null;

            taskIndex++;

            if (downloads.Count == 0)
            {
                // Cas où il n'y a aucune tâche dans
                // la liste:
                taskIndex = INDEX_NO_TASK;
                priorityFilter = MIN_PRIO_FILTER;
                //activeDownloads = 0;
                return null;
            }


            // Retour au début de la liste des tâches
            // en cas de dépassement de la taille de 
            // cette liste:
            if (taskIndex >= downloads.Count)
            {
                taskIndex = 0;

                // Après un tour complet dans la liste des tâches:
                // - remettre à zéro le compteur de tâches actives
                // - changer le filtre de priorité
                //activeDownloads = 0;
                priorityFilter++;
                if (priorityFilter > MAX_PRIO_FILTER)
                {
                    priorityFilter = MIN_PRIO_FILTER;
                }
            }


            task = downloads[taskIndex];

            return task;
        }
    }
}
