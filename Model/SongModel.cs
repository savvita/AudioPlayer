using NAudio.Wave;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Winamp.Model
{
    public class SongModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Path to the audio file
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Name to display
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Duration of the song
        /// </summary>
        public TimeSpan TotalTime { get; }

        /// <summary>
        /// Duration of the song in the seconds
        /// </summary>
        public int TotalSeconds { get; }

        /// <summary>
        /// Length of the audiofile in bytes
        /// </summary>
        public long Length { get; }

        public TimeSpan TimeLeft
        {
            get => new TimeSpan(0, 0, (int)(TotalSeconds - Position * TotalSeconds / Length));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Current position at the file
        /// </summary>
        public long Position
        {
            get => AudioFileReader.Position;
            set
            {
                AudioFileReader.Position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        /// <summary>
        /// Reader for getting the audio of the song
        /// </summary>
        public AudioFileReader AudioFileReader { get; }

        /// <summary>
        /// Create and initialize the songmodel
        /// </summary>
        /// <param name="filePath">Path to the audio file</param>
        public SongModel(string filePath)
        {
            FilePath = filePath;
            DisplayName = Path.GetFileNameWithoutExtension(filePath);
            AudioFileReader = new AudioFileReader(filePath);
            TotalTime = AudioFileReader.TotalTime;
            TotalSeconds = (int)TotalTime.TotalSeconds;
            Length = AudioFileReader.Length;
            Position = 0;
        }
    }
}
