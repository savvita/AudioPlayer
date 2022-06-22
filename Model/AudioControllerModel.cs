using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Winamp.Model
{
    public class AudioControllerModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Output device
        /// </summary>
        private WaveOutEvent outputDevice;

        /// <summary>
        /// Current playing song
        /// </summary>
        private SongModel song;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AudioControllerModel(WaveOutEvent outputDevice)
        {
            this.outputDevice = outputDevice;
            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
        }

        /// <summary>
        /// Volume of the device
        /// </summary>
        public float Volume
        {
            get => outputDevice.Volume * 100f;

            set
            {
                outputDevice.Volume = value / 100f;
                OnPropertyChanged(nameof(Volume));
            }
        }

        /// <summary>
        /// Start playing the audio file
        /// </summary>
        /// <param name="audioFileReader">Audio file to play</param>
        public void Play(SongModel song)
        {
            if (outputDevice.PlaybackState == PlaybackState.Playing)
                song.Position = 0;

            this.song = song;

            if (outputDevice != null && this.song != null)
            {              
                outputDevice.Stop();
                outputDevice.Init(this.song.AudioFileReader);
                outputDevice.Play();
            }
        }

        /// <summary>
        /// Playback state of output device
        /// </summary>
        public PlaybackState PlaybackState
        {
            get => outputDevice.PlaybackState;
        }

        private void OutputDevice_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if(song.Position >= song.AudioFileReader.Length - 1)
                SongEnded?.Invoke();
        }

        public event Action SongEnded;

        /// <summary>
        /// Stop playing
        /// </summary>
        public void Stop()
        {
            if (outputDevice != null)
                outputDevice.Stop();
        }

        /// <summary>
        /// Release the resources
        /// </summary>
        public void Close()
        {
            if (outputDevice != null)
                outputDevice.Dispose();
        }

        /// <summary>
        /// Release the resources
        /// </summary>
        public void Dispose() => Close();

        private void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
