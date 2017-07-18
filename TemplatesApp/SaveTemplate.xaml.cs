using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TemplatesApp
{
    /// <summary>
    /// Logique d'interaction pour SaveTemplate.xaml
    /// </summary>
    public partial class SaveTemplate : UserControl
    {
        private Workflow workflow = null;
        private bool isReady = false;


        public SaveTemplate()
        {
            InitializeComponent();
        }

        internal Workflow Workflow
        {
            get
            {
                return workflow;
            }
            set
            {
                workflow = value;
                workflow.WorkflowUpdated += Workflow_WorkflowUpdated;
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        private void Workflow_WorkflowUpdated(object sender, EventArgs e)
        {
            /*if (workflow.IsReadyToSaveTemplate != isReady)
            {
                isReady = workflow.IsReadyToSaveTemplate;
                if (isReady)
                {
                    InitSaveTemplate();
                }
                else
                {
                    ClearSaveTemplate();
                }
            }*/
        }

        private void InitSaveTemplate()
        {
            //TODO
        }

        private void ClearSaveTemplate()
        {
            // TODO
        }
    }
}
