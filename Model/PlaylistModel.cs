using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Winamp.Model
{
    public class PlaylistModel : INotifyPropertyChanged
    {
        private object locker = new object();
        private SynchronizationContext context;

        private Random random = new Random();

        /// <summary>
        /// Path to the directory with audio files
        /// </summary>
        public readonly string DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Playlist");

        /// <summary>
        /// List of the audiofiles in the playlist
        /// </summary>
        public ObservableCollection<SongModel> AudioFiles { get; set; }

        /// <summary>
        /// Represent the order of changing songs. True if random order otherwise false
        /// </summary>
        public bool IsShuffle { get; set; }

        /// <summary>
        /// Active audio file
        /// </summary>
        private SongModel currentSong;

        public SongModel CurrentSong
        {
            get => currentSong;

            set
            {
                currentSong = value;
                SelectedSong = CurrentSong;
                OnPropertyChanged(nameof(CurrentSong));
            }
        }

        /// <summary>
        /// Selected audio file
        /// </summary>
        private SongModel selectedSong;

        public SongModel SelectedSong
        {
            get => selectedSong;

            set
            {
                selectedSong = value;
                OnPropertyChanged(nameof(SelectedSong));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Create and initialize playlistmodel
        /// </summary>
        public PlaylistModel()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            AudioFiles = new ObservableCollection<SongModel>();

            foreach (string file in Directory.GetFiles(DirectoryPath))
            {
                AudioFiles.Add(new SongModel(file));
            }

            if (AudioFiles.Count > 0)
            {
                CurrentSong = AudioFiles[0];
            }
        }

        /// <summary>
        /// Get the next song to play
        /// </summary>
        /// <returns>Next song at the playlist according to the value IsShuffle</returns>
        public SongModel? Next()
        {
            if (CurrentSong != null)
            {
                CurrentSong.Position = 0;
            }

            SongModel? next = IsShuffle ? GetRandom() : GetNext();

            if (next != null)
                CurrentSong = next;

            return next;
        }

        /// <summary>
        /// Get the random song at the playlist
        /// </summary>
        /// <returns>Random song</returns>
        private SongModel? GetRandom()
        {
            if (AudioFiles == null || AudioFiles.Count == 0)
            {
                return null;
            }

            return AudioFiles[random.Next(AudioFiles.Count)];
        }

        /// <summary>
        /// Get the next song at the list
        /// </summary>
        /// <returns>If current song is not the last than next song at the playlist, otherwise null</returns>
        private SongModel? GetNext()
        {
            if (CurrentSong != AudioFiles.Last())
            {
                return AudioFiles[AudioFiles.IndexOf(CurrentSong) + 1];
            }

            return null;
        }

        /// <summary>
        /// Add new song to the playlist
        /// </summary>
        /// <param name="filepath">Path to the file of new song</param>
        public void AddSong(string filepath)
        {
            context = SynchronizationContext.Current;

            Task task = Task.Factory.StartNew(() =>
            {
                lock (locker)
                {
                    try
                    {
                        File.Copy(filepath, Path.Combine(DirectoryPath, Path.GetFileName(filepath)));

                        context.Send((_) => AudioFiles.Add(new SongModel(Path.Combine(DirectoryPath, Path.GetFileName(filepath)))), null);
                    }
                    catch { }
                }
            });          
        }

        /// <summary>
        /// Remove song from the list
        /// </summary>
        /// <param name="filename">Name of the song</param>
        public void RemoveSong(string filename)
        {
            context = SynchronizationContext.Current;

            Task task = Task.Factory.StartNew(() =>
            {
                lock (locker)
                {
                    try
                    {
                        string path = Path.Combine(DirectoryPath, Path.GetFileName(filename));
                        File.Delete(path);

                        context.Send((_) => AudioFiles.Remove(AudioFiles.Where((item) => item.FilePath.Equals(path)).First()), null);
                    }
                    catch { }
                }
            });
        }

        /// <summary>
        /// Release the resources
        /// </summary>
        public void Close()
        {
            foreach(SongModel song in AudioFiles)
            {
                if(song.AudioFileReader != null)
                {
                    song.AudioFileReader.Dispose();
                }
            }
        }

        public void Dispose() => Close();
    }
}
