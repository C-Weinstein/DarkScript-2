﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using FastColoredTextBoxNS;

namespace DarkScript_2
{
    public partial class GUI : Form
    {
        public EMEVD currentEMEVD;
        public GUI thisForm;
        public bool scrolling = false;

        public GUI()
        {
            SetStyles();
            InitializeComponent();
            editorSplit.SplitterDistance = 400;
            thisForm = this;

            editorNumeric.AllowSeveralTextStyleDrawing = true;
            editorVerbose.AllowSeveralTextStyleDrawing = true;
        }

        public class EMEVD
        {
            public string FilePath { get; set; }
            public string NumericText { get; private set; }
            public string VerboseText { get; private set; }

            public static EMEVD Read(string path)
            {
                var emevd = new EMEVD(path);
                emevd.Refresh();
                return emevd;
            }

            private EMEVD(string path) => FilePath = path;

            public void Refresh()
            {
                NumericText = Numeric();
                VerboseText = Verbose();
            }

            public void RefreshVerbose()
            {
                VerboseText = Verbose();
            }

            public void Save(string input)
            {
                try
                {
                    File.WriteAllText("tmp.txt", input);
                    var p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardError = true;;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.FileName = "EventScriptTool.exe";
                    p.StartInfo.Arguments = "tmp.txt -p -o \"" + FilePath + "";
                    p.Start();
                    string err = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    p.Dispose();
                    File.Delete("tmp.txt");
                    if (string.IsNullOrWhiteSpace(err)) Refresh();
                    else MessageBox.Show(err);
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            public void SaveAs(string path, string input)
            {
                FilePath = path;
                Save(input);
            }

            private string ReadFile(string arguments)
            {
                try
                {
                    ActiveForm.Cursor = Cursors.WaitCursor;
                    var p = new Process();
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.FileName = "EventScriptTool.exe";
                    p.StartInfo.Arguments = arguments + " -o tmp.txt";
                    Console.WriteLine(p.StartInfo.FileName + " " + p.StartInfo.Arguments);
                    p.Start();
                    string err = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    if (err.Length > 0)
                    {
                        ActiveForm.Cursor = Cursors.Default;
                        MessageBox.Show(err);
                        return err;
                    }
                    string output = File.ReadAllText("tmp.txt");
                    File.Delete("tmp.txt");
                    ActiveForm.Cursor = Cursors.Default;
                    return output;
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    ActiveForm.Cursor = Cursors.Default;
                    return "ERROR";
                }

            }

            private string Numeric() => ReadFile("-n -s \"" + FilePath + "\"");

            private string Verbose() => ReadFile("-v -s \"" + FilePath + "\"");
        }


        private void GUI_Load(object sender, EventArgs e)
        {
            editorSplit.SplitterWidth = 20;
            editorSplit.SplitterDistance = 400;
        }

        private void openEMEVDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    currentEMEVD = EMEVD.Read(ofd.FileName);
                    editorNumeric.Text = AdjustNumeric(currentEMEVD.NumericText);
                    editorVerbose.Text = AdjustVerbose(currentEMEVD.VerboseText, currentEMEVD.NumericText);
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private string AdjustNumeric(string numeric) => numeric.Replace(Environment.NewLine + "    ^", " ^");

        private string AdjustVerbose(string verbose, string numeric) => verbose.Replace(Environment.NewLine + "Parameters:", " | Parameters:");

        private void saveEMEVDAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentEMEVD == null) return;
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                int cursorPos = editorNumeric.SelectionStart;
                int scrollPos = editorNumeric.VerticalScroll.Value;
                try
                {
                    currentEMEVD.SaveAs(sfd.FileName, editorNumeric.Text.Replace(" ^", Environment.NewLine + "    ^"));
                    editorNumeric.Text = AdjustNumeric(currentEMEVD.NumericText);
                    editorVerbose.Text = AdjustVerbose(currentEMEVD.VerboseText, currentEMEVD.NumericText);
                    editorNumeric.SelectionStart = cursorPos;
                    editorNumeric.VerticalScroll.Value = scrollPos;
                    editorVerbose.VerticalScroll.Value = scrollPos;
                    editorNumeric.VerticalScroll.Value = scrollPos;
                    editorVerbose.VerticalScroll.Value = scrollPos;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    this.Cursor = Cursors.Default;
                }
                Cursor = Cursors.Default;
            }
        }

        private void saveEMEVDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentEMEVD == null) return;
            int cursorPos = editorNumeric.SelectionStart;
            int scrollPos = editorNumeric.VerticalScroll.Value;
            try
            {
                Cursor = Cursors.WaitCursor;
                currentEMEVD.Save(editorNumeric.Text.Replace(" ^", Environment.NewLine + "    ^"));
                editorVerbose.Text = AdjustVerbose(currentEMEVD.VerboseText, currentEMEVD.NumericText);
                editorNumeric.Text = AdjustNumeric(currentEMEVD.NumericText);
                editorNumeric.VerticalScroll.Value = scrollPos;
                editorVerbose.VerticalScroll.Value = scrollPos;
                editorNumeric.VerticalScroll.Value = scrollPos;
                editorVerbose.VerticalScroll.Value = scrollPos;
                editorNumeric.SelectionStart = cursorPos;
                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Cursor = Cursors.Default;
            }
        }

        private void ScrollPosChanged(FastColoredTextBox target, FastColoredTextBox source)
        {
            if (scrolling) return;
            scrolling = true;
            if (source.VerticalScroll.Value <= target.VerticalScroll.Maximum)
            {
                int a = target.VerticalScroll.Maximum;
                int b = source.VerticalScroll.Maximum;
                int c = source.VerticalScroll.Value;
                int val = Math.Min(Math.Min(a, b), c);
                int checkCount = 0;
                while (target.VerticalScroll.Value != val)
                {
                    if (checkCount > 2) break;
                    target.VerticalScroll.Value = val;
                    checkCount++;
                }
            }
            Console.WriteLine("SRC --> " + source.VerticalScroll.Value + " : TGT --> " + target.VerticalScroll.Value);
            scrolling = false;
        }

        private void SyncNumToVerb() => ScrollPosChanged(editorNumeric, editorVerbose);

        private void SyncVerbToNum() => ScrollPosChanged(editorVerbose, editorNumeric);

        private void editorNumeric_Scroll(object sender, ScrollEventArgs e) => SyncVerbToNum();

        private void editorNumeric_SelectionChangedDelayed(object sender, EventArgs e) => SyncVerbToNum();

        private void editorNumeric_SelectionChanged(object sender, EventArgs e) => SyncVerbToNum();

        private void editorVerbose_Scroll(object sender, ScrollEventArgs e) => SyncNumToVerb();

        private void editorVerbose_SelectionChanged(object sender, EventArgs e) => SyncNumToVerb();

        private void editorVerbose_SelectionChangedDelayed(object sender, EventArgs e) => SyncNumToVerb();

        private void Box_KeyDown(object sender, KeyEventArgs e)
        {
            FastColoredTextBox box = sender as FastColoredTextBox;
            if (e.Control && e.KeyCode == Keys.F)
            {
                box.ShowFindDialog();
                e.SuppressKeyPress = true;
                e.Handled = true;
            } else if (e.Control && e.KeyCode == Keys.R)
            {
                box.ShowReplaceDialog();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        static Dictionary<string, Style> Styles = new Dictionary<string, Style>();


        private void SetStyles()
        {

            TextStyle style(Brush b) => new TextStyle(b, Brushes.Transparent, FontStyle.Regular);
            Styles["GREEN"] = style(Brushes.DarkGreen);
            Styles["BLUE"] = style(Brushes.Blue);
            Styles["DARKBLUE"] = style(Brushes.DarkBlue);
            Styles["SLATEBLUE"] = style(Brushes.DarkSlateBlue);
            Styles["PURPLE"] = style(Brushes.DarkMagenta);
            Styles["BLACK"] = style(Brushes.Black);
            Styles["RED"] = style(Brushes.Red);
        }



        private void editorNumeric_TextChanged(object sender, TextChangedEventArgs e)
        {

            void style(string brush, string pattern) => e.ChangedRange.SetStyle(Styles[brush], pattern);

            style("SLATEBLUE", "\\d+");
            style("GREEN", "[A-Za-z]+");
            style("BLUE", "(?<range>\\d+)\\[\\d+\\]");
            style("BLUE", "\\d+\\[(?<range>\\d+)\\]");
            style("DARKBLUE", "\\n\\d+,\\s*\\d+\\s*\\n");
            style("BLACK", "EVD[^\\n]+");
        }


        private void editorVerbose_TextChanged(object sender, TextChangedEventArgs e)
        {
            void style(string brush, string pattern) => e.ChangedRange.SetStyle(Styles[brush], pattern);

            style("BLACK", "[(){},]+");
            style("BLACK", "\\)\\s*\n");
            style("RED", "\\b(SKIP|END|IF)\\b");
            style("GREEN", "\\b(AND|OR|MAIN)\\n");
            style("BLUE", "\\b(END|Disable|Enable|RESTART|Set|Initialize|Batch)\\b");
            style("SLATEBLUE", "\\d+");
            style("SLATEBLUE", "[A-Za-z0-9\\s]+:(?<range>[^,)]+)");
            style("PURPLE", "(?<range>[A-Za-z0-9-\\s]+):([^,)]+)");
        }
    }
}
