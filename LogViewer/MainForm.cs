﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LogViewer
{
    public partial class MainForm : Form
    {
        LogFile logFile;

        public MainForm()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            OpenLogFile();
        }

        private void OpenLogFile()
        {
            try
            {
                if (logFile != null)
                {
                    logFile.Dispose();
                }
                searchResultsDataGridView.DataSource = null;
                searchPatternTextBox.Text = string.Empty;
                searchCountLabel.Text = string.Empty;
                logFile = new LogFile(filePathTextBox.Text);
                pageCountTextBox.Text = logFile.PageCount.ToString();
                pageNoNumericUpDown.Maximum = logFile.PageCount;
                LoadPage(logFile.ReadOnePage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void LoadPage(string text)
        {
            contentRichTextBox.Text = text;
            contentRichTextBox.RightMargin = TextRenderer.MeasureText(contentRichTextBox.Text, contentRichTextBox.Font).Width;
            pageNoNumericUpDown.Value = logFile.CurrentPage;
        }


        private void homeButton_Click(object sender, EventArgs e)
        {
            try
            {
                logFile.CurrentPage = 1;
                LoadPage(logFile.ReadOnePage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void endButton_Click(object sender, EventArgs e)
        {
            try
            {
                logFile.CurrentPage = logFile.PageCount;
                LoadPage(logFile.ReadOnePage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoadPage(logFile.ReadPreviousPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void downButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoadPage(logFile.ReadNextPage());
            }
            catch (EndOfStreamException)
            {
                // last page reached, do nothing
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (logFile != null)
                {
                    logFile.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void filePathLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RunSafely(() =>
            {
                using (var ofd = new OpenFileDialog())
                {
                    var result = ofd.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        filePathTextBox.Text = ofd.FileName;
                        OpenLogFile();
                    }
                }
            });
        }

        private void gotoPageButton_Click(object sender, EventArgs e)
        {
            int pageNo = (int)pageNoNumericUpDown.Value;
            if (pageNo > 0 && pageNo <= logFile.PageCount)
            {
                logFile.CurrentPage = pageNo;
                LoadPage(logFile.ReadOnePage());
            }
        }

        private void RunSafely(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            var results = logFile.SearchWithinFile(searchPatternTextBox.Text);
            searchResultsDataGridView.DataSource = results;
            searchCountLabel.Text = results.Count.ToString() + " results";
        }

        private void searchResultsDataGridView_DoubleClick(object sender, EventArgs e)
        {
            if (searchResultsDataGridView.SelectedRows.Count > 0)
            {
                var results = (List<SearchResult>)searchResultsDataGridView.DataSource;
                var index = searchResultsDataGridView.SelectedRows[0].Index;
                var selectedResult = results[index];
                var page = selectedResult.PageNo;
                logFile.CurrentPage = page;
                LoadPage(logFile.ReadOnePage());
                pageNoNumericUpDown.Value = logFile.CurrentPage;
                foreach (var result in FindAllResultsInThePage(results, index))
                {
                    contentRichTextBox.SelectionStart = result.Index;
                    contentRichTextBox.SelectionLength = result.Length;
                    contentRichTextBox.SelectionBackColor = Color.Yellow;
                    contentRichTextBox.SelectionLength = 0;
                }
            }
        }

        private List<SearchResult> FindAllResultsInThePage(List<SearchResult> results, int index)
        {
            List<SearchResult> samePageResults = new List<SearchResult>();
            samePageResults.Add(results[index]);
            var page = results[index].PageNo;
            for (int i = index + 1; i < results.Count && results[i].PageNo == page; i++)
            {
                samePageResults.Add(results[i]);
            }
            for (int i = index - 1; i >= 0 && results[i].PageNo == page; i--)
            {
                samePageResults.Add(results[i]);
            }
            return samePageResults;
        }

        private void searchPatternTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                searchButton.Focus();
                searchButton.PerformClick();
            }
        }

        private void filePathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                openButton.Focus();
                openButton.PerformClick();
            }
        }

        private void contentRichTextBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.PageDown)
            {
                downButton.PerformClick();
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                upButton.PerformClick();
            }
        }

        private void pageNoNumericUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                gotoPageButton.PerformClick();
            }
        }

    }
}