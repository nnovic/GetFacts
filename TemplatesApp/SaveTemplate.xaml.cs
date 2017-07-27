using GetFacts;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
            if (workflow.IsReadyToSaveTemplate != isReady)
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
            }
        }

        private void InitSaveTemplate()
        {
            SaveButton.IsEnabled = true;
        }

        private void ClearSaveTemplate()
        {
            SaveButton.IsEnabled = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            object o = Workflow.PageTemplate;
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            string output = JsonConvert.SerializeObject(o, Formatting.Indented, settings);
            string file = Workflow.TemplateFile;
            string path = Path.Combine(TemplateFactory.GetInstance().TemplatesDirectory, file);
            File.WriteAllText(path, output);
        }
    }
}
