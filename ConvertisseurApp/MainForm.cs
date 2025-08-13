using System;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace ConvertisseurApp
{
	public class MainForm : Form
	{
		private TextBox linkTextBox;
		private Button formatButton;
		private Button downloadButton;
		private Button folderButton;
	private string downloadFolder;
	private string selectedFormat = "mp4";
	private ProgressBar progressBar;

		public MainForm()
		{
			this.Text = "Télécharger une vidéo ou chanson";
			this.Width = 500;
			this.Height = 200;

			downloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

			linkTextBox = new TextBox
			{
				Left = 20,
				Top = 20,
				Width = 440
			};

			formatButton = new Button
			{
				Text = "Format : mp4",
				Left = 20,
				Top = 60,
				Width = 120
			};
			formatButton.Click += FormatButton_Click;

			downloadButton = new Button
			{
				Text = "Télécharger",
				Left = 160,
				Top = 60,
				Width = 120
			};
			downloadButton.Click += DownloadButton_Click;

			folderButton = new Button
			{
				Text = "Choisir dossier",
				Left = 300,
				Top = 60,
				Width = 160
			};
			folderButton.Click += FolderButton_Click;

			progressBar = new ProgressBar
			{
				Left = 20,
				Top = 110,
				Width = 440,
				Height = 20,
				Minimum = 0,
				Maximum = 100,
				Value = 0,
				Style = ProgressBarStyle.Continuous
			};

			this.Controls.Add(linkTextBox);
			this.Controls.Add(formatButton);
			this.Controls.Add(downloadButton);
			this.Controls.Add(folderButton);
			this.Controls.Add(progressBar);
		}


		private void FormatButton_Click(object? sender, EventArgs e)
		{
			selectedFormat = selectedFormat == "mp4" ? "mp3" : "mp4";
			formatButton.Text = $"Format : {selectedFormat}";
		}

		private void FolderButton_Click(object? sender, EventArgs e)
		{
			using (var dialog = new FolderBrowserDialog())
			{
				dialog.Description = "Choisis le dossier de téléchargement";
				dialog.SelectedPath = downloadFolder;
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					downloadFolder = dialog.SelectedPath;
				}
			}
		}

	private async void DownloadButton_Click(object? sender, EventArgs e)
		{
			string url = linkTextBox.Text.Trim();
			if (string.IsNullOrEmpty(url))
			{
				MessageBox.Show("Veuillez entrer un lien.");
				return;
			}

			string ytDlpExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
			string outputTemplate = Path.Combine(downloadFolder, "%(title)s.%(ext)s");
			string args = selectedFormat == "mp3"
				? $"--extract-audio --audio-format mp3 -o \"{outputTemplate}\" \"{url}\""
				: $"-f bestvideo+bestaudio --merge-output-format mp4 -o \"{outputTemplate}\" \"{url}\"";

			var process = new System.Diagnostics.Process();
			process.StartInfo.FileName = ytDlpExe;
			process.StartInfo.Arguments = args;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;

			progressBar.Value = 0;

			process.OutputDataReceived += (s, ev) =>
			{
				if (ev.Data != null)
				{
					// yt-dlp affiche la progression sous la forme "[download]   42.3% ..."
					var match = Regex.Match(ev.Data, @"\[download\]\s+(\d{1,3}\.\d)%");
					if (match.Success)
					{
						if (double.TryParse(match.Groups[1].Value.Replace('.', ','), out double percent))
						{
							int value = (int)Math.Round(percent);
							if (progressBar.InvokeRequired)
								progressBar.Invoke(new Action(() => progressBar.Value = Math.Min(value, 100)));
							else
								progressBar.Value = Math.Min(value, 100);
						}
					}
				}
			};

			try
			{
				process.Start();
				process.BeginOutputReadLine();
				string error = await process.StandardError.ReadToEndAsync();
				process.WaitForExit();
				progressBar.Value = 100;
				if (!string.IsNullOrWhiteSpace(error))
				{
					MessageBox.Show("Erreur lors du téléchargement : " + error, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
					progressBar.Value = 0;
				}
				else
				{
					MessageBox.Show("Téléchargement terminé.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Erreur lors du téléchargement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
				progressBar.Value = 0;
			}
		}
	}
}
