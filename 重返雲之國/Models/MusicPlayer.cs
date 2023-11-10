using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace 重返雲之國_外傳.Models
{
    public class MusicPlayer
    {
        private string path; //檔案位子
        private readonly string FolderName = "BGM";

        public MusicPlayer()
        {
            this.MusicVolume = Properties.Settings.Default.MusicVolume;
            this.EffectVolume = Properties.Settings.Default.EffectVolume;
            path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName);
            mediaPlayers.Add(null);
        }

        #region 短音效
        private List<MediaPlayer> mediaPlayers = new List<MediaPlayer>();

        public void PlayShort(string effect)
        {
            Uri file = new Uri(System.IO.Path.Combine(path, effect + ".wav"), UriKind.Relative);
            int i = 0;
            foreach(MediaPlayer player in mediaPlayers)
            {
                if(player == null) { break; }
                else { i++; }
            }
            if(i >= mediaPlayers.Count) {  mediaPlayers.Add(null); }
            MediaPlayer media = mediaPlayers[i] = new MediaPlayer();
            media.Open(file);
            media.Volume = EffectVolume;
            media.MediaEnded += (s, e) => { mediaPlayers[i] = null; };
            media.Play();
        }
        #endregion

        #region 長音效
        private MediaPlayer _effect { get; set; }
        private double EffectVolume;

        public void PlayLong(string effect)
        {
            if (_effect != null)
            {
                _effect.Stop();
                _effect = null;
            }

            Uri file = new Uri(System.IO.Path.Combine(path, effect + ".wav"), UriKind.Relative);
            _effect = new MediaPlayer();
            _effect.Open(file);
            _effect.Volume = EffectVolume;
            _effect.Play();
        }

        public void SetEffectVolume(double volume)
        {
            EffectVolume = volume;
            if (_effect != null)
            {
                _effect.Volume = volume;
            }
        }

        public void StopLong()
        {
            _effect?.Stop();
            _effect = null;
        }
        #endregion

        #region 長音樂
        private MediaPlayer _music { get; set; }
        private double MusicVolume;

        public void PlayMusic(string music)
        {
            if(_music != null)
            {
                _music.Stop();
                _music = null;
            }

            Uri file = new Uri(System.IO.Path.Combine(path, music + ".wav"), UriKind.Relative);
            _music = new MediaPlayer();
            _music.Open(file);
            _music.Volume = MusicVolume;
            _music.MediaEnded += (s, e) =>
            {
                _music.Position = TimeSpan.Zero;
                _music.Play();
            };
            _music.Play();
        }

        public void SetMusicVolume(double volume)
        {
            MusicVolume = volume;
            if(_music != null)
            {
                _music.Volume = volume;
            }
        }

        public void StopMusic()
        {
            _music?.Stop();
            _music = null;
        }
        #endregion
    }
}
