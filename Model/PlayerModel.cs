using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Winamp.Model
{
    public class PlayerModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Update values of properties
        /// </summary>
        private Task updateTask;

        /// <summary>
        /// Current playlist
        /// </summary>
        public PlaylistModel Playlist { get; set; }

        /// <summary>
        /// Represents the audio controller of the player
        /// </summary>
        public AudioControllerModel AudioController { get; set; }

        /// <summary>
        /// Time left to the end of the song
        /// </summary>
        public TimeSpan TimeLeft
        {
            get => Playlist.CurrentSong.TimeLeft;
        }

        /// <summary>
        /// Cureent position of the current song
        /// </summary>
        public long Position
        {
            get => Playlist.CurrentSong != null ? Playlist.CurrentSong.Position : 0;
            set
            {
                Playlist.CurrentSong.Position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public PlayerModel()
        {
            Playlist = new PlaylistModel();
            AudioController = new AudioControllerModel(new WaveOutEvent());

            if (Playlist.AudioFiles.Count > 0)
            {
                Playlist.CurrentSong = Playlist.AudioFiles[0];
            }

            AudioController.SongEnded += AudioController_SongEnded;

            StartUpdateTask();
        }

        /// <summary>
        /// Create and start update task
        /// </summary>
        private void StartUpdateTask()
        {
            updateTask = new Task(() =>
            {
                while (true)
                {
                    if (Playlist.CurrentSong != null)
                    {
                        OnPropertyChanged(nameof(TimeLeft));
                        OnPropertyChanged(nameof(Position));
                    }
                    Thread.Sleep(1000);
                }
            });

            updateTask.Start();
        }

        /// <summary>
        /// Actions if song ended
        /// </summary>
        private void AudioController_SongEnded()
        {
            SongModel? next = Playlist.Next();

            if (next != null)
                AudioController.Play(next);
        }

        /// <summary>
        /// Start playing of the current song
        /// </summary>
        public void PlayCurrent()
        {
            Playlist.CurrentSong = Playlist.SelectedSong;
            Task.Factory.StartNew(() => AudioController.Play(Playlist.SelectedSong));
        }

        /// <summary>
        /// Stop playing the current song
        /// </summary>
        public void Stop()
        {
            AudioController.Stop();
        }

        /// <summary>
        /// Release all the resources
        /// </summary>
        public void Close()
        {
            AudioController.Close();
            Playlist.Close();
        }
    }
}
