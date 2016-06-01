/*
 * File: MainWindow.xaml.cs
 * 
 * Purpose: 
 * 
 * Author: G. Gagnaux, Kohd & Art
 * 
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls;

namespace KohdAndArt.MediaTransfer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Some default values
        //
        const string DEFAULTSOURCEFOLDER = @"G:\";
        const string DEFAULTDESTINATIONFOLDER = @"G:\Img\";

        // Settings
        //
        private string _sourceFolder = Properties.Settings.Default.SourceFolder;
        private string _destinationFolder = Properties.Settings.Default.DestinationFolder;
        private bool _removeSourceFileAfterCopy = Properties.Settings.Default.RemoveSourceFilesAfterCopy;
        private bool _createEditsFolder = Properties.Settings.Default.CreateEditsFolder;
        private bool _createFinalFolder = Properties.Settings.Default.CreateFinalFolder;
        private string _finalDestinationFolderName = Properties.Settings.Default.FinalDestinationFolderName;

        public MainWindow()
        {
            InitializeComponent();
            progressBar.Visibility = System.Windows.Visibility.Hidden;
            progressBar.IsIndeterminate = false;
            textBlockProgress.Text = String.Empty;
            textBoxSourceFolder.Text = _sourceFolder;
            textBoxDestinationFolder.Text = _destinationFolder;
            checkBoxRemoveSourceFile.IsChecked = _removeSourceFileAfterCopy ? true : false;
            checkBoxCreateFolderEdit.IsChecked = _createEditsFolder ? true : false;
            checkBoxCreateFolderFinal.IsChecked = _createFinalFolder ? true : false;
            textBoxDestFolderName.Text = _finalDestinationFolderName;
        }

        private void buttonSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            _sourceFolder = GetFolder(_sourceFolder);
            textBoxSourceFolder.Text = _sourceFolder;
            Properties.Settings.Default.SourceFolder = _sourceFolder;
            Properties.Settings.Default.Save();
        }

        private void buttonDestinationFolder_Click(object sender, RoutedEventArgs e)
        {
            _destinationFolder = GetFolder(_destinationFolder);
            textBoxDestinationFolder.Text = _destinationFolder;
            Properties.Settings.Default.DestinationFolder = _destinationFolder;
            Properties.Settings.Default.Save();
        }

        private string GetFolder(string initialFolder)
        {
            var selectedFolder = String.Empty;

            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select Folder";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = initialFolder;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = initialFolder;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedFolder = dlg.FileName;
            }

            return selectedFolder;
        }

        private async void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.IsEnabled = false;
            int currentFileCount = 0;

            try
            {
                ShowProgressBar(true);

                IEnumerable<string> fileList = Directory.EnumerateFiles(textBoxSourceFolder.Text);
                int fileCount = fileList.Count<string>();
                progressBar.Maximum = fileCount;

                foreach (string filename in fileList)
                {
                    using (FileStream SourceStream = File.Open(filename, FileMode.Open))
                    {
                        // Check if file already exists in destination folder
                        string destinationPathRoot = textBoxDestinationFolder.Text;
                        FileInfo fi = new FileInfo(filename);

                        DestinationPaths paths = BuildDestinationPaths(fi, destinationPathRoot);

                        // This the folder where the files will be copied.
                        string destinationFolderAndFile = paths.OriginalsPath;
                        string finalPathOnly = destinationFolderAndFile.Substring(destinationFolderAndFile.LastIndexOf('\\'));

                        if (!File.Exists(destinationFolderAndFile))
                        {
                            // Create the target directory if it doesn't exist already
                            FileInfo fi2 = new FileInfo(destinationFolderAndFile);
                            DirectoryInfo di = Directory.CreateDirectory(fi2.DirectoryName);

                            // Create the other directories (No files will be copied into them)
                            if (_createEditsFolder) 
                                di = Directory.CreateDirectory(paths.EditsPath);
                            if (_createFinalFolder)
                                di = Directory.CreateDirectory(paths.FinalPath);

                            using (FileStream DestinationStream = File.Create(destinationFolderAndFile))
                            {
                                UpdateStatus(String.Format("Copying '{0}'", filename));
                                await SourceStream.CopyToAsync(DestinationStream);

                                progressBar.Value = ++currentFileCount;
                                string status = string.Format("'{0}' copied successfully.", filename);
                                UpdateStatus(status);
                            }
                        }
                        else
                        {
                            string text = string.Format("'{0}' already exists.  No work done.", destinationFolderAndFile);
                            UpdateStatus(text);
                        }
                    }

                    if (_removeSourceFileAfterCopy)
                    {
                        File.Delete(filename);
                        UpdateStatus(string.Format("'{0}' removed from source.", filename));
                    }
                }


                ShowProgressBar(false);
                UpdateStatus(String.Format("{0} files {1}.", currentFileCount, _removeSourceFileAfterCopy ? "moved" : "copied"));
                UpdateStatus("File Transfer Complete.");
            }
            catch (Exception ex)
            {
                UpdateStatus(ex.Message.ToString());
            }
            finally
            {
                buttonStart.IsEnabled = true;
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destinationPathRoot"></param>
        /// <returns></returns>
        private DestinationPaths BuildDestinationPaths(FileInfo sourceFile, string destinationPathRoot)
        {
            int year = sourceFile.CreationTime.Year;
            int month = sourceFile.CreationTime.Month;
            string monthName = GetMonthName(month);
            int day = sourceFile.CreationTime.Day;
            string filename = sourceFile.Name;

            string numericDate = String.Format("{0}{1}{2}", 
                                               year.ToString("D4"), 
                                               month.ToString("D2"), 
                                               day.ToString("D2"));

            var output = new DestinationPaths();


            // Example Path: G:\Images\2016\May\20160528\Originals\file.jpg
            string finalFolderPath = Properties.Settings.Default.FinalDestinationFolderName;
            output.OriginalsPath = String.Format(@"{0}\{1}\{2}\{3}\{4}\{5}",
                                            destinationPathRoot,
                                            year, monthName, numericDate, finalFolderPath, filename);

            output.EditsPath = String.Format(@"{0}\{1}\{2}\{3}\{4}",
                                            destinationPathRoot,
                                            year, monthName, numericDate, "Edits");

            output.FinalPath = String.Format(@"{0}\{1}\{2}\{3}\{4}",
                                            destinationPathRoot,
                                            year, monthName, numericDate, "Final");

            return output;
        }


        private void ShowProgressBar(bool setVisible)
        {
            progressBar.Visibility = setVisible ? System.Windows.Visibility.Visible : 
                                                  System.Windows.Visibility.Hidden;
        }
        private void UpdateStatus(string text)
        {
            string currentText = textBlockProgress.Text;
            StringBuilder sb = new StringBuilder();
            sb.Append(currentText);
            sb.AppendLine(text);
            textBlockProgress.Text = sb.ToString();
            scrollViewer.ScrollToEnd();
        }

        private void buttonClearStatusPanel_Click(object sender, RoutedEventArgs e)
        {
            textBlockProgress.Text = string.Empty;
        }

        private string GetMonthName(int month)
        {
            System.Globalization.DateTimeFormatInfo mfi = new
            System.Globalization.DateTimeFormatInfo();
            return mfi.GetMonthName(month).ToString();
        }

        private void checkBoxRemoveSourceFile_Checked(object sender, RoutedEventArgs e)
        {
            CheckboxRemoveSourceHandler(sender as CheckBox);
        }

        private void checkBoxRemoveSourceFile_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckboxRemoveSourceHandler(sender as CheckBox);
        }

        private void CheckboxRemoveSourceHandler(CheckBox checkBox)
        {
            _removeSourceFileAfterCopy = checkBoxRemoveSourceFile.IsChecked.Value;
            Properties.Settings.Default.RemoveSourceFilesAfterCopy = _removeSourceFileAfterCopy;
            Properties.Settings.Default.Save();
        }

        private void textBoxSourceFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            TextboxSourceHandler(sender as TextBox);
        }

        private void TextboxSourceHandler(TextBox textBox)
        {
            _sourceFolder = textBox.Text;
        }

        private void textBoxDestinationFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            TextboxDestinationHandler(sender as TextBox);
        }

        private void TextboxDestinationHandler(TextBox textBox)
        {
            _destinationFolder = textBox.Text;
        }

        private void buttonAbout_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate the dialog box
            AboutBoxWindow dlg = new AboutBoxWindow();

            // Configure the dialog box
            dlg.Owner = this;

            // Open the dialog box modally 
            dlg.ShowDialog();
        }

        private void checkBoxCreateFolderEdit_Checked(object sender, RoutedEventArgs e)
        {
            CheckboxCreateEditsFolderHandler(sender as CheckBox);
        }

        private void checkBoxCreateFolderEdit_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckboxCreateEditsFolderHandler(sender as CheckBox);
        }

        private void CheckboxCreateEditsFolderHandler(CheckBox checkBox)
        {
            _createEditsFolder = checkBoxCreateFolderEdit.IsChecked.Value;
            Properties.Settings.Default.CreateEditsFolder = _createEditsFolder;
            Properties.Settings.Default.Save();
        }


        private void checkBoxCreateFolderFinal_Checked(object sender, RoutedEventArgs e)
        {
            CheckboxCreateFinalFolderHandler(sender as CheckBox);
        }

        private void checkBoxCreateFolderFinal_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckboxCreateFinalFolderHandler(sender as CheckBox);
        }

        private void CheckboxCreateFinalFolderHandler(CheckBox checkBox)
        {
            _createFinalFolder = checkBoxCreateFolderFinal.IsChecked.Value;
            Properties.Settings.Default.CreateFinalFolder = _createFinalFolder;
            Properties.Settings.Default.Save();
        }

        private void textBoxDestFolderName_LostFocus(object sender, RoutedEventArgs e)
        {
            TextboxFinalDestFolderHandler(sender as TextBox);
        }

        private void TextboxFinalDestFolderHandler(TextBox textBox)
        {
            _finalDestinationFolderName = textBox.Text;
            Properties.Settings.Default.FinalDestinationFolderName = textBox.Text;
            Properties.Settings.Default.Save();
        }

        private void textBoxDestinationFolder_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

    public class DestinationPaths
    {
        public string OriginalsPath { get; set; }
        public string EditsPath { get; set; }
        public string FinalPath { get; set; }
    }
}
 