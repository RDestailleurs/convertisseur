using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvertisseurApp
{
    public static class LinkSearch
    {
        public static async Task DownloadAsync(string url, string format, string downloadFolder, ProgressBar progressBar)
        {
            progressBar.Value = 0;
            var progress = new Progress<int>(v => {
                if (progressBar.InvokeRequired)
                    progressBar.Invoke(new Action(() => progressBar.Value = v));
                else
                    progressBar.Value = v;
            });
            try
            {
                await MainForm.DownloadVideoAsync(url, format, downloadFolder, progress);
                progressBar.Value = 100;
                MessageBox.Show("Téléchargement terminé.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du téléchargement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressBar.Value = 0;
            }
        }
    }
}
