using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BetterloidPython.Config;
using Microsoft.Scripting.Hosting;
using Microsoft.Win32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BetterloidPython
{
    /// <summary>
    /// Interaction logic for PythonScriptSelector.xaml
    /// </summary>
    public partial class PythonScriptSelector : Window
    {
        private Config.Config config;
        private string configPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BetterloidPython", "AppSettings.json");
        private ScriptEngine engine;

        public PythonScriptSelector()
        {
            InitializeComponent();
            Title = "Betterloid Python Script Selector";
            LoadConfig();
            UpdateScriptsListbox();
            engine = IronPython.Hosting.Python.CreateEngine();
        }
        private void LoadConfig()
        {
            if (Path.Exists(configPath))
            {
                try
                {
#nullable enable
                    Config.Config? configTemp = JsonSerializer.Deserialize<Config.Config>(File.ReadAllText(configPath));
                    if (configTemp != null)
                    {
                        config = configTemp;
                    }
#nullable disable
                }
                catch (Exception e)
                {
                    MessageBoxResult userChoice = System.Windows.MessageBox.Show(this, "An exception occurred while loading the parameters: " + e.ToString() + "\n Would you like to recreate the config?", "Error in BetterloadPython settings", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Error);
                    if (userChoice == MessageBoxResult.Yes)
                    {
                        File.Delete(configPath);
                        LoadConfig();
                    }
                }
            }
            else
            {
                config = new Config.Config();
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            try
            {
                if (!File.Exists(Path.GetDirectoryName(configPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                }
                File.WriteAllText(configPath, JsonSerializer.Serialize(config));
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(this, "An exception occurred while saving the parameters: " + e.ToString(), "Error in BetterloadPython settings", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void UpdateScriptsListbox()
        {
            LstbScripts.ItemsSource = config.Scripts.Keys.ToList();
            LstbScripts.Items.Refresh();
        }


        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (LstbScripts.SelectedItem != null)
            {
                config.Scripts.Remove(LstbScripts.SelectedItem as string);
                SaveConfig();
                UpdateScriptsListbox();
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Python files (*.py)|*.py|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);
                if (config.Scripts.ContainsKey(fileName) == false)
                {
                    config.Scripts.Add(fileName, filePath);
                    SaveConfig();
                    UpdateScriptsListbox();
                }
                else
                {
                    System.Windows.MessageBox.Show(this, $"A script with the filename: {fileName} already exists. Please rename it", "Error while registering a new script", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            if (LstbScripts.SelectedItem != null)
            {
                if (LstbScripts.SelectedItem is string fileName)
                {
                    string filePath = config.Scripts[fileName];
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            ScriptSource scope = engine.CreateScriptSourceFromFile(filePath);
                            scope.ExecuteProgram();
                        }
                        catch (Exception error)
                        {
                            System.Windows.MessageBox.Show(this, error.ToString(), "Error while running Python script", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(this, "Script does not exists.", "Error while loading Python script", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
        }

        private void LstbScripts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstbScripts.SelectedItem != null)
            {
                LblScriptInfos.Content = "Script Name: " + LstbScripts.SelectedItem as string;
                LblScriptInfos.Content = "Script Path: " + config.Scripts[LstbScripts.SelectedItem as string];
            }
        }
    }
}
