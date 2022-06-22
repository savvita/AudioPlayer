using GalaSoft.MvvmLight.Command;
using NAudio.Wave;
using System.IO;
using System.Windows.Forms;
using Winamp.Model;

namespace Winamp.ModelView
{
    public class PlayerViewModel
    {
        public PlayerModel Player { get; set; }

        public PlayerViewModel()
        {
            Player = new PlayerModel();
        }

        #region Commands
        private RelayCommand? playCommand = null;
        private RelayCommand? pauseCommand = null;
        private RelayCommand? stopCommand = null;
        private RelayCommand? rewindCommand = null;
        private RelayCommand? addCommand = null;
        private RelayCommand? removeCommand = null;
        private RelayCommand? nextCommand = null;
        private RelayCommand? closeCommand = null;

        public RelayCommand PlayCommand
        {
            get => playCommand ?? new RelayCommand(() => Player.PlayCurrent());
        }

        public RelayCommand PauseCommand
        {
            get => pauseCommand ?? new RelayCommand(() => Player.Stop());
        }

        public RelayCommand StopCommand
        {
            get => stopCommand ?? new RelayCommand(() =>
            {
                Player.Stop();
                Player.Playlist.CurrentSong.Position = 0;
            });
        }

        public RelayCommand RewindCommand
        {
            get => rewindCommand ?? new RelayCommand(() => Player.Playlist.CurrentSong.Position = 0);
        }

        public RelayCommand AddCommand
        {
            get => addCommand ?? new RelayCommand(() => AddFile());
        }

        public RelayCommand RemoveCommand
        {
            get => removeCommand ?? new RelayCommand(() => RemoveFile());
        }

        public RelayCommand NextCommand
        {
            get => nextCommand ?? new RelayCommand(() =>
            {
                Player.Playlist.Next();

                if (Player.AudioController.PlaybackState == PlaybackState.Playing)
                {
                    Player.AudioController.Play(Player.Playlist.CurrentSong);
                }
            });
        }

        public RelayCommand CloseCommand
        {
            get => removeCommand ?? new RelayCommand(() => Player.Close());
        }
        #endregion

        private void AddFile()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "(mp3)|*.mp3|(wav)|*.wav";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                Player.Playlist.AddSong(openFile.FileName);
            }
        }

        private void RemoveFile()
        {
            if (Player.Playlist.CurrentSong != null)
            {
                Player.Playlist.RemoveSong(Path.GetFileName(Player.Playlist.SelectedSong.FilePath));
            }
        }
    }
}
