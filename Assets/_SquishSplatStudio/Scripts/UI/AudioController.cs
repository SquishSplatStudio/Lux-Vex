/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public sealed class AudioController : MonoBehaviour
    {

        #region ---[base singleton stuff]---

        static AudioController() { }
        public static AudioController Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        private AudioSource _music;
        private AudioSource _ambient;
        private AudioSource _fx;
        private AudioSource _vo;

        [SerializeField] float _voVolume = .4f;
        [SerializeField] float _fxVolume = .2f;
        [SerializeField] float _amVolume = .05f;
        [SerializeField] float _musicVolume = .5f;

        [SerializeField] AudioClip[] JukeBox;
        [SerializeField] AudioClip ButtonClick;
        [SerializeField] AudioClip WayPointPlacement;
        [SerializeField] AudioClip StructurePlacement;
        [SerializeField] AudioClip ControlCrystalSpawn;
        [SerializeField] AudioClip LightTowerComplete;
        [SerializeField] AudioClip LightWellComplete;
        [SerializeField] AudioClip PurifierComplete;
        [SerializeField] AudioClip Ambience;

        [SerializeField] AudioClip[] EnemyVanquished;
        [SerializeField] AudioClip[] StructureDestroyed;
        [SerializeField] AudioClip[] WorkerDeath;
        [SerializeField] AudioClip[] WorkerAttacking;
        [SerializeField] AudioClip[] EnemyAttacking;
        [SerializeField] AudioClip[] EnemyAttackingStructure;
        [SerializeField] AudioClip[] NoResources;
        [SerializeField] AudioClip[] StructureCompleted;
        [SerializeField] AudioClip[] WorkerCompleted;
        [SerializeField] AudioClip[] StructureBuilding;
        [SerializeField] AudioClip[] BuildingWorker;
        [SerializeField] AudioClip[] Exploring;
        [SerializeField] AudioClip[] Guarding;
        [SerializeField] AudioClip[] Mining;
        [SerializeField] AudioClip[] Clicked;
        [SerializeField] AudioClip[] SolarActivity;
        [SerializeField] AudioClip[] LightCapacity;

        public void Start()
        {
            _fx = gameObject.AddComponent<AudioSource>();
            _vo = gameObject.AddComponent<AudioSource>();
            _ambient = gameObject.AddComponent<AudioSource>();
            
            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = false;

            var music = new List<Music>();
            music.Add(new Music() { AudioClip = JukeBox[0], ClipDuration = 192f, Volume = _musicVolume });
            music.Add(new Music() { AudioClip = JukeBox[1], ClipDuration = 240f, Volume = _musicVolume });
            music.Add(new Music() { AudioClip = JukeBox[2], ClipDuration = 293f, Volume = _musicVolume });

            StartCoroutine(PlayAndFadeMusicLoop(_music, music, 5f));
        }

        class Music
        {
            public AudioClip AudioClip { get; set; }
            public float ClipDuration { get; set; }

            public float Volume { get; set; }
        }

        IEnumerator Fade(AudioSource source, float finalVolume, float fadeDuration)
        {
            float fadeSpeed = Mathf.Abs(source.volume - finalVolume) / fadeDuration;

            while (!Mathf.Approximately(source.volume, finalVolume))
            {
                source.volume = Mathf.MoveTowards(source.volume, finalVolume, fadeSpeed * Time.deltaTime);
                yield return null;
            }
        }

        IEnumerator PlayAndFadeMusicLoop(AudioSource source, List<Music> music, float fadeDuration)
        {
            Music song = null;
            if (GameHandler.Instance != null)
            {
                float p = (GameHandler.Instance.sunTimer / (GameHandler.Instance.levelTime * 60f));
                if (p <= 1)
                {
                    song = music[0];
                }
                else if (p <= .66)
                {
                    song = music[1];
                }
                else if (p <= .33)
                {
                    song = music[1];
                }
            }
            else 
            {
                song = music[0];
            }

            source.Stop();
            source.volume = 0f;
            source.clip = song.AudioClip;
            source.Play();

            StartCoroutine(Fade(source, song.Volume, fadeDuration * .5f));
            yield return new WaitForSeconds(song.ClipDuration - fadeDuration);

            StartCoroutine(Fade(source, 0f, fadeDuration));
            yield return new WaitForSeconds(fadeDuration);

            yield return new WaitForSeconds(60f);

            StartCoroutine(PlayAndFadeMusicLoop(_music, music, 5f));
        }

        class LastPlayed
        {
            public float LastVoPlayed { get; set; }
            public PlayType PlayType { get; set; }
            public AudioClip AudioClip { get; set; }
        }

        public enum PlayType
        {
            EnemyVanquished,
            StructureDestroyed,
            WorkerDeath,
            WorkerAttacking,
            EnemyAttacking,
            NoResources,
            StructureCompleted,
            WorkerCompleted,
            StructureBuilding,
            BuildingWorker,
            Exploring,
            Guarding,
            Mining,
            Clicked,
            Other,
            SolarActivity,
            LightCapacity,
            EnemyAttackingStructure
        }

        List<LastPlayed> _lastPlayed = new List<LastPlayed>();
        Stack<LastPlayed> _musicQueue = new Stack<LastPlayed>();

        public void Play(AudioClip audioClip, AudioSourceType audioSourceType, float volume = .75f, PlayType playType = PlayType.Other)
        {
            bool play = false;
            LastPlayed lpRef = null;

            if (playType != PlayType.Other)
            {
                if (_lastPlayed.Count != 0)
                {
                    for (int i = 0; i < _lastPlayed.Count; i++)
                    {
                        var lp = _lastPlayed[i];
                        if (lp.PlayType == PlayType.EnemyAttacking || lp.PlayType == PlayType.EnemyAttackingStructure)
                        {
                            if(lp.LastVoPlayed + 10f < Time.time)
                            {
                                lp.LastVoPlayed = Time.time;
                                lp.AudioClip = audioClip;
                                play = true;
                            }
                            lpRef = lp;
                            //break;
                        }
                        else if (lp.LastVoPlayed + 5f < Time.time && lp.PlayType == playType)
                        {
                            lp.LastVoPlayed = Time.time;
                            lp.AudioClip = audioClip;
                            play = true;
                            lpRef = lp;
                            //break;
                        }
                        else if (lp.PlayType == playType)
                        {
                            play = false;
                            lpRef = lp;
                            //break;
                        }
                        if (lpRef != null)
                            break;
                    }
                    if (lpRef == null)
                        play = true;
                }
                else
                {
                    play = true;
                }

                if (lpRef == null)
                {
                    lpRef = new LastPlayed() {AudioClip = audioClip, PlayType = playType, LastVoPlayed = Time.time };
                    _lastPlayed.Add(lpRef);
                }

                if(play)
                    _musicQueue.Push(lpRef);

            }

            switch (audioSourceType)
            {
                case AudioSourceType.FX:
                    if (_fx == null)
                        _fx = gameObject.AddComponent<AudioSource>(); // testing purposes
                    _fx.PlayOneShot(audioClip, volume);
                    break;
                case AudioSourceType.VO:
                    PlayMusicQueue(volume);
                    break;
                case AudioSourceType.AMBIENT:
                    _ambient.PlayOneShot(audioClip, volume);
                    break;
                case AudioSourceType.MUSIC:
                    _music.PlayOneShot(audioClip, volume);
                    break;
            };
        }

        void PlayMusicQueue(float volume)
        {
            while (_musicQueue.Count > 0)
            {
                var pop = _musicQueue.Pop();
                StartCoroutine(PlayItems(pop.AudioClip, volume));
            }
        }

        IEnumerator PlayItems(AudioClip audioClip, float volume)
        {
            while (_vo.isPlaying)
                yield return new WaitForSeconds(1f);
            _vo.clip = audioClip;
            _vo.volume = volume;
            _vo.Play();
        }

        void PlayAmbientLoop(AudioClip audioClip, float volume = .75f)
        {
            _ambient = gameObject.AddComponent<AudioSource>(); // testing purposes
            _ambient.clip = audioClip;
            _ambient.volume = volume;
            _ambient.loop = true;
            _ambient.Play();
        }

        public enum AudioSourceType
        {
            FX,
            VO,
            AMBIENT,
            MUSIC
        }

        AudioClip GetRandomClip(AudioClip[] clips) 
        {
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }

        public void PlayButtonClick() => Play(ButtonClick, AudioSourceType.FX, _fxVolume);
        public void PlayWayPointPlacement() => Play(WayPointPlacement, AudioSourceType.FX, _fxVolume);
        public void PlayStructurePlacement() => Play(StructurePlacement, AudioSourceType.FX, _fxVolume);
        public void PlayLightTowerComplete() => Play(LightTowerComplete, AudioSourceType.FX, _fxVolume);
        public void PlayLightWellComplete() => Play(LightWellComplete, AudioSourceType.FX, _fxVolume);
        public void PlayPurifierComplete() =>  Play(PurifierComplete, AudioSourceType.FX, _fxVolume);
        public void PlayAmbience() => PlayAmbientLoop(Ambience, _amVolume);
        public void PlayControlCrystalSpawn() => Play(ControlCrystalSpawn, AudioSourceType.FX, _fxVolume);
        public void PlayEnemyVanquished() => Play(GetRandomClip(EnemyVanquished), AudioSourceType.VO, _voVolume, PlayType.EnemyVanquished);
        public void PlayStructureDestroyed() => Play(GetRandomClip(StructureDestroyed), AudioSourceType.VO, _voVolume, PlayType.StructureDestroyed);
        public void PlayWorkerDeath() => Play(GetRandomClip(WorkerDeath), AudioSourceType.VO, _voVolume, PlayType.WorkerDeath);
        public void PlayWorkerAttacking() => Play(GetRandomClip(WorkerAttacking), AudioSourceType.VO, _voVolume, PlayType.WorkerAttacking);
        public void PlayEnemyAttacking() => Play(GetRandomClip(EnemyAttacking), AudioSourceType.VO, _voVolume, PlayType.EnemyAttacking);
        public void PlayStructureCompleted() => Play(GetRandomClip(StructureCompleted), AudioSourceType.VO, _voVolume, PlayType.StructureCompleted);
        public void PlayWorkerCompleted() => Play(GetRandomClip(WorkerCompleted), AudioSourceType.VO, _voVolume, PlayType.WorkerCompleted);
        public void PlayStructureBuilding() => Play(GetRandomClip(StructureBuilding), AudioSourceType.VO, _voVolume, PlayType.StructureBuilding);
        public void PlayBuildingWorker() => Play(GetRandomClip(BuildingWorker), AudioSourceType.VO, _voVolume, PlayType.BuildingWorker);
        public void PlayExploring() => Play(GetRandomClip(Exploring), AudioSourceType.VO, _voVolume, PlayType.Exploring);
        public void PlayGuarding() => Play(GetRandomClip(Guarding), AudioSourceType.VO, _voVolume, PlayType.Guarding);
        public void PlayMining() => Play(GetRandomClip(Mining), AudioSourceType.VO, _voVolume, PlayType.Mining);
        public void PlayClicked() => Play(GetRandomClip(Clicked), AudioSourceType.VO, _voVolume, PlayType.Clicked); // TODO
        public void PlayNoResources() => Play(GetRandomClip(NoResources), AudioSourceType.VO, _voVolume, PlayType.NoResources); // TODO
        public void PlaySolarActivity(int index) => Play(SolarActivity[index], AudioSourceType.VO, _voVolume, PlayType.SolarActivity); // TODO
        public void PlayLightCapacity(int index) => Play(LightCapacity[index], AudioSourceType.VO, _voVolume, PlayType.LightCapacity); // TODO
        public void PlayEnemyAttackingStructure() => Play(GetRandomClip(EnemyAttackingStructure), AudioSourceType.VO, _voVolume, PlayType.EnemyAttackingStructure);
        
    }
}
