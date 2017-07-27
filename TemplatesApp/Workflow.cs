using GetFacts;
using GetFacts.Download;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesApp
{
    public class Workflow
    {
        public event EventHandler WorkflowUpdated;

        internal void OnWorkflowUpdated()
        {
            WorkflowUpdated?.Invoke(this, EventArgs.Empty);
        }

        #region collect TemplateExplorer data for SourceExplorer step

        private string templateFile = null;

        /// <summary>
        /// Chaine de caractère qui définit le nom du fichier 'template'
        /// sur lequel on travaille.
        /// Cette information est nécessaire pour permettre de passer à l'étape
        /// 'SourceExplorer'.
        /// </summary>
        public string TemplateFile
        {
            get
            {
                return templateFile;
            }
            internal set
            {
                if( string.Compare(templateFile, value)!=0 )
                {
                    // Provoquer le passage vers l'état 'invalide'
                    if (templateFile != null)
                    {
                        templateFile = null;
                        pageTemplate = null;
                        OnWorkflowUpdated();
                    }

                    // Provoquer le passage vers l'état 'valide'
                    if (value != null)
                    {
                        templateFile = value;
                        pageTemplate = TemplateFactory.GetInstance().GetExistingTemplate(templateFile);
                        OnWorkflowUpdated();
                    }
                }
            }
        }

        private PageTemplate pageTemplate = null;

        /// <summary>
        /// Retourne l'instance de PageTemplate qui a été créée lorsque
        /// le membre TemplateFile a été initialisé.
        /// </summary>
        public PageTemplate PageTemplate
        {
            get
            {
                return pageTemplate;
            }
        }

        /// <summary>
        /// Retourne true si toutes les informations requises pour 
        /// l'étape 'SourceExplorer' sont réunies.
        /// </summary>
        public bool IsTemplateDataAvailable
        {
            get
            {
                if(  string.IsNullOrEmpty(templateFile) 
                    || (pageTemplate==null) )
                {
                    return false;
                }
                return true;
            }
        }

        #endregion

        #region collect SourceExplorer data for TemplateEditor step

        private DownloadTask downloadTask = null;

        public DownloadTask DownloadTask
        {
            get
            {
                return downloadTask;
            }
            set
            {
                if( downloadTask!=value )
                {
                    if( downloadTask!=null)
                    {
                        downloadTask.TaskFinished -= DownloadTask_TaskFinished;
                    }
                    downloadTask = value;
                    if( downloadTask!=null )
                    {
                        downloadTask.TaskFinished += DownloadTask_TaskFinished;
                    }
                    OnWorkflowUpdated();
                }
            }
        }

        private void DownloadTask_TaskFinished(object sender, EventArgs e)
        {
            OnWorkflowUpdated();
        }

        /// <summary>
        /// Retourne true si toutes les informations requises pour 
        /// l'étape 'TemplateEditor' sont réunies.
        /// </summary>
        public bool IsPageDataAvailable
        {
            get
            {
                if ((DownloadTask!=null) 
                    && (DownloadTask.Status==DownloadTask.DownloadStatus.Completed) )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion



        public bool IsReadyToSaveTemplate
        {
            get
            {
                return IsTemplateDataAvailable;
            }
        }
    }
}
