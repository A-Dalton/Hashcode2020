using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace Hashcode2k20
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string x = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            InputTextBox.Text = $"{x}\\input";
            OutputTextBox.Text = $"{x}\\output";
            ReadInputFiles();
        }

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(OutputTextBox.Text))
            {
                MessageBox.Show("Output Folder does not exist");
                return;
            }

            foreach (ListBoxItem item in FilesToProcessListBox.SelectedItems)
            {
                var logic = new ActualLogic((string)item.Tag, OutputTextBox.Text);
                ProgressListBox.Items.Add($"Initalized File '{item.Content}'...");
                string output = logic.Process();
                ProgressListBox.Items.Add($"Wrote output to file '{output}'");
            }
        }

        private void RefreshInputFilesButton_Click(object sender, RoutedEventArgs e) => ReadInputFiles();

        private void ReadInputFiles()
        {
            FilesToProcessListBox.Items.Clear();
            if (Directory.Exists(InputTextBox.Text))
            {
                foreach (string file in Directory.GetFiles(InputTextBox.Text))
                    FilesToProcessListBox.Items.Add(new ListBoxItem() { Tag = file, Content = Path.GetFileName(file) });
                FilesToProcessListBox.SelectAll();
            }
        }
    }
}
