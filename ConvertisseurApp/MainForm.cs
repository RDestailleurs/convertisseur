
	using System;
	using System.Windows.Forms;
	using System.IO;
	using System.Text.RegularExpressions;
	using ConvertisseurApp;

namespace ConvertisseurApp
{
	public class MainForm : Form
	{
	private TextBox linkTextBox;
	private TextBox nameTextBox;
	private ComboBox extractorTypeComboBox;
	private Button formatButton;
	private Button downloadButton;
	private Button folderButton;
	private Button modeLinkButton;
	private Button modeNameButton;
	private Button searchNameButton;
	private Button downloadNameButton;
	private ListBox nameResultsListBox;
	private string downloadFolder;
	private string selectedFormat = "mp4";
	private ProgressBar progressBar;
	private bool isLinkMode = true;
	private List<ResultEntry> lastNameResults = new List<ResultEntry>();

		public MainForm()
		{
			this.Text = "Télécharger une vidéo ou chanson";
			this.Width = 500;
			this.Height = 200;

			downloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

			modeLinkButton = new Button
			{
				Text = "Par lien",
				Left = 20,
				Top = 10,
				Width = 100
			};
			modeLinkButton.Click += (s, e) => SetMode(true);

			modeNameButton = new Button
			{
				Text = "Par nom",
				Left = 130,
				Top = 10,
				Width = 100
			};
			modeNameButton.Click += (s, e) => SetMode(false);

			linkTextBox = new TextBox
			{
				Left = 20,
				Top = 50,
				Width = 440
			};

			nameTextBox = new TextBox
			{
				Left = 20,
				Top = 50,
				Width = 300
			};

			extractorTypeComboBox = new ComboBox
			{
				Left = 330,
				Top = 50,
				Width = 130,
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			extractorTypeComboBox.Items.AddRange(new string[] { "Toutes plateformes", "Vidéos", "Audios" });
			extractorTypeComboBox.SelectedIndex = 0;

			searchNameButton = new Button
			{
				Text = "Rechercher",
				Left = 20,
				Top = 90,
				Width = 120
			};
			searchNameButton.Click += SearchNameButton_Click;

			nameResultsListBox = new ListBox
			{
				Left = 20,
				Top = 130,
				Width = 440,
				Height = 60
			};

			formatButton = new Button
			{
				Text = "Format : mp4",
				Left = 160,
				Top = 90,
				Width = 120
			};
			formatButton.Click += FormatButton_Click;

			downloadButton = new Button
			{
				Text = "Télécharger",
				Left = 300,
				Top = 90,
				Width = 120
			};
			downloadButton.Click += DownloadButton_Click;

			folderButton = new Button
			{
				Text = "Choisir dossier",
				Left = 300,
				Top = 90,
				Width = 160
			};
			folderButton.Click += FolderButton_Click;

			downloadNameButton = new Button
			{
				Text = "Télécharger la sélection",
				Width = 220,
				Top = nameResultsListBox.Top + nameResultsListBox.Height + 10
				// Left sera centré dynamiquement dans SetMode
			};
			downloadNameButton.Click += DownloadNameButton_Click;
			downloadNameButton.Visible = false;

			progressBar = new ProgressBar
			{
				Left = 20,
				Top = 160,
				Width = 440,
				Height = 20,
				Minimum = 0,
				Maximum = 100,
				Value = 0,
				Style = ProgressBarStyle.Continuous
			};

			this.Controls.Add(modeLinkButton);
			this.Controls.Add(modeNameButton);
			this.Controls.Add(linkTextBox);
			this.Controls.Add(nameTextBox);
			this.Controls.Add(extractorTypeComboBox);
			this.Controls.Add(searchNameButton);
			this.Controls.Add(nameResultsListBox);
			this.Controls.Add(formatButton);
			this.Controls.Add(downloadButton);
			this.Controls.Add(folderButton);
			this.Controls.Add(progressBar);
			this.Controls.Add(downloadNameButton);

			SetMode(true);
	}

		private void SetMode(bool linkMode)
		{
			isLinkMode = linkMode;
			linkTextBox.Visible = linkMode;
			downloadButton.Visible = linkMode;
			progressBar.Visible = true;

			// Positionnement dynamique des boutons selon le mode
			if (linkMode)
			{
				// Emplacements originaux pour le mode lien
				formatButton.Left = 20;
				formatButton.Top = 90;
				downloadButton.Left = 160;
				downloadButton.Top = 90;
				folderButton.Left = 300;
				folderButton.Top = 90;
				progressBar.Left = 20;
				progressBar.Top = 160;
			}
			else
			{
				// Décale les boutons pour éviter la superposition avec "Rechercher"
				formatButton.Left = 160;
				formatButton.Top = 90;
				downloadButton.Left = 9999; // Masqué hors écran
				folderButton.Left = 300;
				folderButton.Top = 90;
				// Centre le bouton sous la listBox
				downloadNameButton.Left = nameResultsListBox.Left + (nameResultsListBox.Width - downloadNameButton.Width) / 2;
				downloadNameButton.Top = nameResultsListBox.Top + nameResultsListBox.Height + 10;
				// Place la barre de progression sous le bouton 'Télécharger la sélection'
				progressBar.Left = 20;
				progressBar.Top = downloadNameButton.Top + downloadNameButton.Height + 10;
			}
			formatButton.Visible = true;
			folderButton.Visible = true;

			nameTextBox.Visible = !linkMode;
			extractorTypeComboBox.Visible = !linkMode;
			searchNameButton.Visible = !linkMode;
			nameResultsListBox.Visible = !linkMode;
			downloadNameButton.Visible = !linkMode;
		}

		private async void DownloadNameButton_Click(object? sender, EventArgs e)
		{
			if (nameResultsListBox.SelectedItem == null || lastNameResults.Count == 0)
			{
				MessageBox.Show("Sélectionnez un résultat à télécharger.");
				return;
			}
			var selected = nameResultsListBox.SelectedItem as ResultEntry;
			if (selected == null || string.IsNullOrWhiteSpace(selected.Url) || selected.Title.StartsWith("[ERREUR yt-dlp]"))
			{
				MessageBox.Show("Sélection non valide ou pas de lien disponible.");
				return;
			}
			try
			{
				MessageBox.Show($"URL utilisée pour le téléchargement :\n{selected.Url}", "Diagnostic URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
				await LinkSearch.DownloadAsync(selected.Url, selectedFormat, downloadFolder, progressBar);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Erreur lors du téléchargement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
				progressBar.Value = 0;
			}
		}

		private async void SearchNameButton_Click(object? sender, EventArgs e)
		{
			string query = nameTextBox.Text.Trim();
			if (string.IsNullOrEmpty(query))
			{
				MessageBox.Show("Veuillez entrer un nom à rechercher.");
				return;
			}
			nameResultsListBox.Items.Clear();
			NameSearch.ExtractorType type = NameSearch.ExtractorType.All;
			if (extractorTypeComboBox.SelectedIndex == 1)
				type = NameSearch.ExtractorType.Video;
			else if (extractorTypeComboBox.SelectedIndex == 2)
				type = NameSearch.ExtractorType.Audio;
			try
			{
				var results = await NameSearch.SearchByNameAsync(query, type);
				lastNameResults = results;
				foreach (var r in results)
				{
					if (r.Title.StartsWith("[ERREUR yt-dlp]"))
					{
						MessageBox.Show(r.Title, "Erreur yt-dlp", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
				if (results.Count == 0)
				{
					nameResultsListBox.Items.Add("Aucun résultat trouvé.");
					MessageBox.Show("Aucun résultat ou une erreur s'est produite lors de la recherche.\nVérifie la sortie de yt-dlp ou la console.", "Aucun résultat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					nameResultsListBox.Items.AddRange(results.ToArray());
				}
			}
			catch (Exception ex)
			{
				nameResultsListBox.Items.Add($"Erreur: {ex.Message}");
				MessageBox.Show($"Erreur lors de la recherche: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
	}

									// Méthode utilitaire pour télécharger une vidéo/audio avec yt-dlp
									public static async Task DownloadVideoAsync(string url, string format, string downloadFolder, IProgress<int>? progress = null)
									{
										if (string.IsNullOrWhiteSpace(url))
											throw new ArgumentException("Le lien est vide.");

										string ytDlpExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
										string outputTemplate = Path.Combine(downloadFolder, "%(title)s.%(ext)s");
										string args = format == "mp3"
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

										if (progress != null)
											progress.Report(0);

										process.OutputDataReceived += (s, ev) =>
										{
											if (ev.Data != null)
											{
												var match = System.Text.RegularExpressions.Regex.Match(ev.Data, @"\[download\]\s+(\d{1,3}\.\d)%");
												if (match.Success)
												{
													if (double.TryParse(match.Groups[1].Value.Replace('.', ','), out double percent))
													{
														int value = (int)Math.Round(percent);
														progress?.Report(Math.Min(value, 100));
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
											progress?.Report(100);
											if (!string.IsNullOrWhiteSpace(error))
											{
												// Si l'erreur concerne le format, tente un fallback en 'best'
												if (error.Contains("requested format not available") || error.Contains("no suitable format found") || error.Contains("format") || error.Contains("ffmpeg"))
												{
													// Relance yt-dlp avec le format 'best'
													var fallbackArgs = $"-f best -o \"{outputTemplate}\" \"{url}\"";
													var fallbackProcess = new System.Diagnostics.Process();
													fallbackProcess.StartInfo.FileName = ytDlpExe;
													fallbackProcess.StartInfo.Arguments = fallbackArgs;
													fallbackProcess.StartInfo.UseShellExecute = false;
													fallbackProcess.StartInfo.RedirectStandardOutput = true;
													fallbackProcess.StartInfo.RedirectStandardError = true;
													fallbackProcess.StartInfo.CreateNoWindow = true;
													fallbackProcess.EnableRaisingEvents = true;
													if (progress != null)
														progress.Report(0);
													fallbackProcess.OutputDataReceived += (s, ev) =>
													{
														if (ev.Data != null)
														{
															var match = System.Text.RegularExpressions.Regex.Match(ev.Data, @"\[download\]\s+(\d{1,3}\.\d)%");
															if (match.Success)
															{
																if (double.TryParse(match.Groups[1].Value.Replace('.', ','), out double percent))
																{
																	int value = (int)Math.Round(percent);
																	progress?.Report(Math.Min(value, 100));
																}
															}
														}
													};
													fallbackProcess.Start();
													fallbackProcess.BeginOutputReadLine();
													string fallbackError = await fallbackProcess.StandardError.ReadToEndAsync();
													fallbackProcess.WaitForExit();
													progress?.Report(100);
													if (!string.IsNullOrWhiteSpace(fallbackError))
														throw new Exception("Erreur (fallback best) : " + fallbackError);
													return;
												}
												throw new Exception(error);
											}
										}
										catch (Exception ex)
										{
											progress?.Report(0);
											throw new Exception("Erreur lors du téléchargement : " + ex.Message, ex);
										}
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

			try
			{
				await LinkSearch.DownloadAsync(url, selectedFormat, downloadFolder, progressBar);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Erreur lors du téléchargement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
				progressBar.Value = 0;
			}
		}
	}
}
