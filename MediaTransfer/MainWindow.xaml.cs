/*
 * File: MainWindow.xaml.cs
 * 
 * Purpose: 
 * 
 * Author: G. Gagnaux, Kohd & Art
 * 
 */


using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace KohdAndArt.MediaTransfer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants
        const string DEFAULTSOURCEFOLDER = @"G:\";
        const string DEFAULTDESTINATIONFOLDER = @"G:\Img\";
        #endregion

        #region Settings
        private MediaTransferSettings _settings = new MediaTransferSettings();
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            InitializeLogging();

            // Read in existing settings and setup controls
            _settings.Load();
            InitializeControls();
        }
        #endregion

        #region Initialization Routines
        private void InitializeLogging()
        {
            var traceStream = File.Open("MediaTransfer.log",
                                           FileMode.Append, FileAccess.Write, FileShare.Read);
            var traceListener =
                        new TextWriterTraceListener(traceStream);
            Trace.Listeners.Add(traceListener);
            Trace.WriteLine("");
            Trace.WriteLine("------------START------------");
            Trace.WriteLine("Trace Started " + DateTime.Now.ToString());
        }

        private void InitializeControls()
        {
            progressBar.Visibility = System.Windows.Visibility.Hidden;
            progressBar.IsIndeterminate = false;
            textBlockProgress.Text = String.Empty;
            textBoxSourceFolder.Text = _settings.SourceFolder;
            textBoxDestinationFolder.Text = _settings.DestinationFolder;
            checkBoxRemoveSourceFile.IsChecked = _settings.RemoveSourceFileAfterCopy ? true : false;
            checkBoxCreateFolderEdit.IsChecked = _settings.CreateEditsFolder ? true : false;
            checkBoxCreateFolderFinal.IsChecked = _settings.CreateFinalFolder ? true : false;
            textBoxDestFolderName.Text = _settings.DestinationFolderName;
        }
        #endregion

        #region Event Handlers
        private void buttonSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            _settings.SourceFolder = GetFolder(_settings.SourceFolder);
            textBoxSourceFolder.Text = _settings.SourceFolder;
            _settings.Save();
        }

        private void buttonDestinationFolder_Click(object sender, RoutedEventArgs e)
        {
            _settings.DestinationFolder = GetFolder(_settings.DestinationFolder);
            textBoxDestinationFolder.Text = _settings.DestinationFolder;
            _settings.Save();
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private async void buttonStart_Click(object sender, RoutedEventArgs e)
        //{
        //    buttonStart.IsEnabled = false;
        //    var currentFileCount = 0;

        //    try
        //    {
        //        ShowProgressBar(true);

        //        IEnumerable<string> fileList = Directory.EnumerateFiles(textBoxSourceFolder.Text);
        //        int fileCount = fileList.Count<string>();
        //        progressBar.Maximum = fileCount;

        //        foreach (string filename in fileList)
        //        {
        //            var dateTaken = GetDateTakenFromImage(filename);

        //            using (FileStream SourceStream = File.Open(filename, FileMode.Open))
        //            {
        //                // Check if file already exists in destination folder
        //                var destinationPathRoot = textBoxDestinationFolder.Text;
        //                var fi = new FileInfo(filename);

        //                var paths = BuildDestinationPaths(fi, dateTaken, destinationPathRoot);

        //                // This the folder where the files will be copied.
        //                var destinationFolderAndFile = paths.OriginalsPath;
        //                var lastIndex = destinationFolderAndFile.LastIndexOf('\\');
        //                //string finalPathOnly = destinationFolderAndFile.Substring(lastIndex);

        //                if (!File.Exists(destinationFolderAndFile))
        //                {
        //                    // Create the target directory if it doesn't exist already
        //                    var fi2 = new FileInfo(destinationFolderAndFile);
        //                    var di = Directory.CreateDirectory(fi2.DirectoryName);

        //                    // Create the other directories (No files will be copied into them)
        //                    if (_settings.CreateEditsFolder)
        //                        di = Directory.CreateDirectory(paths.EditsPath);
        //                    if (_settings.CreateFinalFolder)
        //                        di = Directory.CreateDirectory(paths.FinalPath);

        //                    using (FileStream DestinationStream = File.Create(destinationFolderAndFile))
        //                    {
        //                        UpdateStatus(String.Format("Copying '{0}'", filename));
        //                        await SourceStream.CopyToAsync(DestinationStream);

        //                        progressBar.Value = ++currentFileCount;
        //                        var status = string.Format("'{0}' copied successfully.", filename);
        //                        UpdateStatus(status);
        //                    }
        //                }
        //                else
        //                {
        //                    progressBar.Value = ++currentFileCount;
        //                    var text = string.Format("'{0}' already exists.  No work done.", destinationFolderAndFile);
        //                    UpdateStatus(text);
        //                }
        //            }

        //            if (_settings.RemoveSourceFileAfterCopy)
        //            {
        //                File.Delete(filename);
        //                UpdateStatus(string.Format("'{0}' removed from source.", filename));
        //            }
        //        }


        //        ShowProgressBar(false);
        //        UpdateStatus(String.Format("{0} files {1}.", currentFileCount, _settings.RemoveSourceFileAfterCopy ? "moved" : "copied"));
        //        UpdateStatus("File Transfer Complete.");
        //    }
        //    catch (Exception ex)
        //    {
        //        UpdateStatus(ex.Message.ToString());
        //    }
        //    finally
        //    {
        //        buttonStart.IsEnabled = true;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.IsEnabled = false;
            var currentFileCount = 0;

            try
            {
                ShowProgressBar(true);

                IEnumerable<string> fileList = Directory.EnumerateFiles(textBoxSourceFolder.Text);
                int fileCount = fileList.Count<string>();
                progressBar.Maximum = fileCount;

                //
                // TODO
                // Find pairs of files (RAW + JPEG)
                // Get the datestamp for the JPEG and use that for the RAW's datestamp
                // In some cases, the GetDateTakenFromImage() function below will throw
                // an exception if it tries to load a non-JPEG file
                //
            
                foreach (string filename in fileList)
                {
                    var dateTaken = DateTime.Now;
                    try
                    {
                        dateTaken = (DateTime)GetDateTakenFromImage(filename);
                    }
                    catch
                    {
                    }

                    // Check if file already exists in destination folder
                    var destinationPathRoot = textBoxDestinationFolder.Text;
                    var fi = new FileInfo(filename);

                    var paths = BuildDestinationPaths(fi, dateTaken, destinationPathRoot);

                    // This the folder where the files will be copied.
                    var destinationFolderAndFile = paths.OriginalsPath;
                    var lastIndex = destinationFolderAndFile.LastIndexOf('\\');
                    //string finalPathOnly = destinationFolderAndFile.Substring(lastIndex);

                    if (!File.Exists(destinationFolderAndFile))
                    {
                        // Create the target directory if it doesn't exist already
                        var fi2 = new FileInfo(destinationFolderAndFile);
                        var di = Directory.CreateDirectory(fi2.DirectoryName);

                        // Create the other directories (No files will be copied into them)
                        if (_settings.CreateEditsFolder)
                            di = Directory.CreateDirectory(paths.EditsPath);
                        if (_settings.CreateFinalFolder)
                            di = Directory.CreateDirectory(paths.FinalPath);

                        using (FileStream SourceStream = File.Open(filename, FileMode.Open))
                        {
                            using (FileStream DestinationStream = File.Create(destinationFolderAndFile))
                            {
                                UpdateStatus(String.Format("Copying '{0}'", filename));
                                await SourceStream.CopyToAsync(DestinationStream);

                                progressBar.Value = ++currentFileCount;
                                var status = string.Format("'{0}' copied successfully.", filename);
                                UpdateStatus(status);
                            }
                        }
                    }
                    else
                    {
                        progressBar.Value = ++currentFileCount;
                        var text = string.Format("'{0}' already exists.  No work done.", destinationFolderAndFile);
                        UpdateStatus(text);
                    }

                    if (_settings.RemoveSourceFileAfterCopy)
                    {
                        File.Delete(filename);
                        UpdateStatus(string.Format("'{0}' removed from source.", filename));
                    }
                }

                ShowProgressBar(false);
                UpdateStatus(String.Format("{0} files {1}.", currentFileCount, _settings.RemoveSourceFileAfterCopy ? "moved" : "copied"));
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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void buttonClearStatusPanel_Click(object sender, RoutedEventArgs e)
        {
            textBlockProgress.Text = string.Empty;
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
            _settings.RemoveSourceFileAfterCopy = checkBoxRemoveSourceFile.IsChecked.Value;
            _settings.Save();
        }

        private void textBoxSourceFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            TextboxSourceHandler(sender as TextBox);
        }

        private void TextboxSourceHandler(TextBox textBox)
        {
            _settings.SourceFolder = textBox.Text;
        }

        private void textBoxDestinationFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            TextboxDestinationHandler(sender as TextBox);
        }

        private void TextboxDestinationHandler(TextBox textBox)
        {
            _settings.DestinationFolder = textBox.Text;
        }

        private void buttonAbout_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate the dialog box
            var dlg = new AboutBoxWindow();

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
            _settings.CreateEditsFolder = checkBoxCreateFolderEdit.IsChecked.Value;
            _settings.Save();
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
            _settings.CreateFinalFolder = checkBoxCreateFolderFinal.IsChecked.Value;
            _settings.Save();
        }

        private void textBoxDestFolderName_LostFocus(object sender, RoutedEventArgs e)
        {
            TextboxFinalDestFolderHandler(sender as TextBox);
        }

        private void TextboxFinalDestFolderHandler(TextBox textBox)
        {
            _settings.DestinationFolderName = textBox.Text;
            _settings.Save();

        }

        private void textBoxDestinationFolder_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        #endregion

        #region Private Methods
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destinationPathRoot"></param>
        /// <returns></returns>
        private DestinationPaths BuildDestinationPaths(FileInfo sourceFile, DateTime dateTaken, string destinationPathRoot)
        {
            //var year = sourceFile.LastWriteTime.Year;
            //var month = sourceFile.LastWriteTime.Month;
            //var monthName = GetMonthName(month);
            //var day = sourceFile.LastWriteTime.Day;
            //var filename = sourceFile.Name;

            // Get some values from the dateTaken object
            var year = dateTaken.Year;
            var month = dateTaken.Month;
            var monthName = GetMonthName(month);
            var day = dateTaken.Day;
            var filename = sourceFile.Name;

            var numericDate = $"{year.ToString("D4")}{month.ToString("D2")}{day.ToString("D2")}";

            var output = new DestinationPaths();


            // Example Path: G:\Images\2016\May\20160528\Originals\file.jpg
            var finalFolderPath = Properties.Settings.Default.FinalDestinationFolderName;
            output.OriginalsPath = String.Format(@"{0}\{1}\{2}\{3}\{4}\{5}",
                                            destinationPathRoot,
                                            year, monthName, numericDate, finalFolderPath, filename);

            output.EditsPath = String.Format(@"{0}\{1}\{2}\{3}\{4}",
                                            destinationPathRoot,
                                            year, monthName, numericDate, "Edits");

            output.FinalPath = String.Format(@"{0}\{1}\{2}\{3}\{4}",
                                            destinationPathRoot,
                                            year, monthName, numericDate, "Final");

            //var temp = GetDateTakenFromImage(sourceFile.FullName);

            return output;
        }


        // We init this once so that if the function is repeatedly called
        // it isn't stressing the garbage man
        private static Regex r = new Regex(":");

        // Retrieves the datetime WITHOUT loading the whole image
        private DateTime? GetDateTakenFromImage(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    using (System.Drawing.Image myImage = System.Drawing.Image.FromStream(fs, false, false))
                    {
                        System.Drawing.Imaging.PropertyItem propItem = myImage.GetPropertyItem(36867);
                        string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                        return DateTime.Parse(dateTaken);
                    }
                } 
                catch (Exception ex)
                {
                    var msg = ex.Message;

                }
                return null;
            }
        }

        private DateTime GetDateTaken(string path)
        {
            System.Drawing.Image myImage = System.Drawing.Image.FromFile(path);
            System.Drawing.Imaging.PropertyItem propItem = myImage.GetPropertyItem(306);
            DateTime dtaken;

            //Convert date taken metadata to a DateTime object
            string sdate = Encoding.UTF8.GetString(propItem.Value).Trim();
            string secondhalf = sdate.Substring(sdate.IndexOf(" "), (sdate.Length - sdate.IndexOf(" ")));
            string firsthalf = sdate.Substring(0, 10);
            firsthalf = firsthalf.Replace(":", "-");
            sdate = firsthalf + secondhalf;
            dtaken = DateTime.Parse(sdate);

            return dtaken;
        }

        private void ShowProgressBar(bool setVisible)
        {
            progressBar.Visibility = setVisible ? System.Windows.Visibility.Visible :
                                                  System.Windows.Visibility.Hidden;
        }

        private void UpdateStatus(string text)
        {
            var currentText = textBlockProgress.Text;
            var sb = new StringBuilder();
            sb.Append(currentText);
            sb.AppendLine(text);

            //this.UIThread(() => this.textBlockProgress.Text = sb.ToString());
            textBlockProgress.Text = sb.ToString();
            scrollViewer.ScrollToEnd();
        }

        private string GetMonthName(int month)
        {
            System.Globalization.DateTimeFormatInfo mfi = new
            System.Globalization.DateTimeFormatInfo();
            return mfi.GetMonthName(month).ToString();
        }
        #endregion

        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Trace.WriteLine("Trace stopped " + DateTime.Now.ToString());
            Trace.WriteLine("------------FINISH------------");
            Trace.Flush();
        }
    }

    public class DestinationPaths
    {
        public string OriginalsPath { get; set; }
        public string EditsPath { get; set; }
        public string FinalPath { get; set; }
    }
}
