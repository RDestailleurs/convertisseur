# Convertisseur vidéo/audio universel

Ce projet est une application Windows Forms en C# permettant de télécharger des vidéos ou de la musique depuis de nombreux sites (YouTube, etc.) et de les convertir en MP4 ou MP3.

## Prérequis

- .NET 6 SDK ou supérieur : https://dotnet.microsoft.com/download 
- [yt-dlp](https://github.com/yt-dlp/yt-dlp/releases/latest) : placez `yt-dlp.exe` dans le dossier du projet ou à côté de l'exécutable généré
- [ffmpeg](https://ffmpeg.org/download.html) : doit être accessible dans le PATH système (pour la conversion en mp3/mp4)

## Compilation

1. Clonez le dépôt ou téléchargez les sources.
2. Ouvrez un terminal dans le dossier du projet (`ConvertisseurApp`).
3. Compilez le projet : 
   ```powershell
   dotnet build
   ```
4. (Optionnel) Pour générer un exécutable autonome : 
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
   ```
   L'exécutable sera dans `ConvertisseurApp/bin/Release/net6.0-windows/win-x64/publish/`

## Utilisation
 
- Lancez l'application (`dotnet run --project ConvertisseurApp` ou l'exécutable généré).
- Entrez le lien de la vidéo.
- Choisissez le format (mp3/mp4).
- Cliquez sur "Télécharger".
- Le fichier sera enregistré dans le dossier Téléchargements par défaut (modifiable).

## Notes

- yt-dlp et ffmpeg sont nécessaires pour le bon fonctionnement.
- Le dossier `bin/` et `obj/` ne doivent pas être versionnés (voir `.gitignore`).

----


Projet développé par RDestailleurs.
