using GetFacts;
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

        #region TemplateExplorer to SourceExplorer

        private string templateFile = null;

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
                    templateFile = value;
                    pageTemplate = TemplateFactory.GetInstance().GetTemplate(templateFile);
                    OnWorkflowUpdated();
                }
            }
        }

        private PageTemplate pageTemplate = null;

        public PageTemplate PageTemplate
        {
            get
            {
                return pageTemplate;
            }
        }

        public bool IsReadyForSourceExplorer
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
    }
}
