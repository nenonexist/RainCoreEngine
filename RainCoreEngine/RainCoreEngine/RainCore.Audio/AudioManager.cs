using System.Linq;
using OpenTK.Audio.OpenAL;

namespace RainCore;

             
                                                                              
                                                                             
                                                                          
                                                                            
                                                                          
   
                                                             
                                                                          
                                                                             
                                                                            
                                                                      
                                                               
                                                                    
                                                                          
                                                                         
                                                                              
                                           
                                                                              
                                                                               
                                  
                                                                              
                                                                              
                                                                            
                                                                     
                                                                
                                                                           
                                                                   
                                                                            
                                                                             
                                                                               
   
                                                                       
                                                                            
                                  
              
public class AudioManager
{
    private ALDevice _device;
    private ALContext _context;
    private bool _ready;

    private readonly Dictionary<string, int> _soundCache = new();                                                                              
    private readonly Dictionary<string, string> _soundAliases = new();                                                                                       
    private readonly List<int> _oneShotSources = new();                                                                          

                                                                                  
                                                                               
                                                   
    private static readonly string SoundsDir = Path.Combine(AppContext.BaseDirectory, "Sounds");

    private readonly Random _rng = new();

                                      
    private int _musicSourceA, _musicSourceB;
    private int _activeMusicSource;                
    private string? _currentMusicPath;                                                                   
    private readonly List<string> _musicPool = new();
    private float _musicVolume = 0.55f;
    private float _sfxVolume = 0.9f;

    private const float MusicFadeDuration = 1.5f;
    private float _musicFadeTimer;
    private bool _musicFading;

                                                                                      
                                                                                     
                                                                              
                                                                        
                                                                                    
                                                                                     
    private const float MusicIntervalActiveSeconds = 420f;                                        
    private const float MusicIntervalCalmSeconds = 900f;                                      
    private const float MusicIntervalJitterSeconds = 45f;                                                                 
    private bool _musicWaiting;
    private float _musicWaitTimer;
    private float _musicWaitTarget;

                                                                                       
                                                                                      
                                                                                   
                                                                                        
                                                            
    private MoodLevel _moodLevel = MoodLevel.Neutral;
    private float _moodValue = 55f;

                                                                                   
                                                                                         
                                                                                        
                                                                                 
    private const float ShortTrackMaxSeconds = 90f;          
    private const float LongTrackMinSeconds = 151f;          

                                                                                        
                                                                           
    private const int RecentMusicHistorySize = 2;
    private readonly List<string> _recentMusicHistory = new();

                                                                                        
                                                                                                        
    private readonly Dictionary<string, float> _trackDurationCache = new();

                                                                            
                                                                      
                                                                       
    private int _ambientSourceA, _ambientSourceB;
    private int _activeAmbientSource;
    private string? _currentAmbientPath;
    private readonly List<string> _ambientPool = new();
    private float _ambientVolume = 0.25f;

    private const float AmbientFadeDuration = 1.5f;
    private float _ambientFadeTimer;
    private bool _ambientFading;

    public void Init()
    {
        try
        {
            _device = ALC.OpenDevice(null);
            _context = ALC.CreateContext(_device, (int[]?)null);
            ALC.MakeContextCurrent(_context);
            _ready = true;

            _musicSourceA = AL.GenSource();
            _musicSourceB = AL.GenSource();
                                                                                
                                                                              
                                                                            
            AL.Source(_musicSourceA, ALSourceb.Looping, false);
            AL.Source(_musicSourceB, ALSourceb.Looping, false);

            _ambientSourceA = AL.GenSource();
            _ambientSourceB = AL.GenSource();
            AL.Source(_ambientSourceA, ALSourceb.Looping, false);
            AL.Source(_ambientSourceB, ALSourceb.Looping, false);
        }
        catch (Exception ex)
        {
                                                                               
                                               
            Console.WriteLine($"[Audio] Звук недоступен, играем без него: {ex.Message}");
            _ready = false;
        }
    }

    public void SetSfxVolume(float v)
    {
        _sfxVolume = Math.Clamp(v, 0f, 1f);
    }

    public void SetMusicVolume(float v)
    {
        _musicVolume = Math.Clamp(v, 0f, 1f);
        ApplyMusicVolumeNow();
    }

                                                                                    
                                                                                   
    public void SetAmbientVolume(float v)
    {
        _ambientVolume = Math.Clamp(v, 0f, 1f);
        ApplyAmbientVolumeNow();
    }

                                                                                           
                                                                                  
                                                                                   
                                                                                       
                                                                   
    public void SetMood(MoodLevel level, float value)
    {
        _moodLevel = level;
        _moodValue = value;
    }

                                                                                   
                                                                                 
                                                                                    
                                                                                     
                                                                                     
                                                                                       
                                                   
    private void ApplyMusicVolumeNow()
    {
        if (!_ready || _musicFading) return;
        int active = _activeMusicSource == 0 ? _musicSourceA : _musicSourceB;
        AL.Source(active, ALSourcef.Gain, _musicVolume);
    }

                                                                                         
    private void ApplyAmbientVolumeNow()
    {
        if (!_ready || _ambientFading) return;
        int active = _activeAmbientSource == 0 ? _ambientSourceA : _ambientSourceB;
        AL.Source(active, ALSourcef.Gain, _ambientVolume);
    }

                 
                                                                               
                                                                            
                                                                       
                                                                                  
                                                                             
                                                                              
                  
    public void LoadSound(string alias, string path)
    {
        if (!_ready) return;
        _soundAliases[alias] = path;
        LoadIntoCache(path);
    }

                                                                                   
                                                                              
                                                                                     
    private int LoadIntoCache(string path)
    {
        if (_soundCache.TryGetValue(path, out int cached)) return cached;

        if (!File.Exists(path))
        {
            Console.WriteLine($"[Audio] Файл звука не найден: {path}");
            _soundCache[path] = 0;
            return 0;
        }

        var (data, format, sampleRate) = LoadAudioFile(path);
        int buffer = AL.GenBuffer();
        AL.BufferData(buffer, format, data, sampleRate);
        _soundCache[path] = buffer;
        return buffer;
    }

    private static (byte[] Data, ALFormat Format, int SampleRate) LoadAudioFile(string path)
    {
        return path.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)
            ? OggLoader.Load(path)
            : WavLoader.Load(path);
    }

                 
                                                                             
                                                                              
                                                                               
                                                                             
                                                                        
                  
    public void PlaySound(string identifier, float volumeScale = 1f)
    {
        if (!_ready || string.IsNullOrEmpty(identifier)) return;

        string path = _soundAliases.TryGetValue(identifier, out var aliasPath)
            ? aliasPath
            : Path.Combine(SoundsDir, identifier);

        int buffer = LoadIntoCache(path);
        if (buffer == 0) return;

        int source = AL.GenSource();
        AL.Source(source, ALSourcei.Buffer, buffer);
        AL.Source(source, ALSourcef.Gain, _sfxVolume * volumeScale);
        AL.SourcePlay(source);
        _oneShotSources.Add(source);
    }

                                                                                    
                                                                                     
                                                                                  
                                                       
    public string? CurrentMusicPath => _currentMusicPath;

                                                                              
    public string? CurrentAmbientPath => _currentAmbientPath;

                                                                                    
                                                                                               
    public void SetMusicPool(IReadOnlyList<string> pool)
    {
        _musicPool.Clear();
        _musicPool.AddRange(pool);
    }

                                                                                           
                                                                                                    
    public void PlayFromPool(IReadOnlyList<string> pool)
    {
        SetMusicPool(pool);
        PlayMusic(PickMoodAwareTrack(_moodLevel));
    }

                                                                                      
                                                                                  
                                                                                    
                                                                                      
                                                                                    
    public void ShuffleMusic()
    {
        if (_musicPool.Count == 0) return;
        _musicWaiting = false;
        PlayMusic(PickMoodAwareTrack(_moodLevel), forceRestart: true);
    }

                                                                                             
                                                                                         
                                                                                 
                                                                                  
    public void PlayMusic(string? path, bool forceRestart = false)
    {
        if (!_ready) return;
        if (!forceRestart && path == _currentMusicPath) return;
        _currentMusicPath = path;
        _musicWaiting = false;                                                                                      

                                                                                   
                                                                             
        if (path != null)
        {
            _recentMusicHistory.Add(path);
            while (_recentMusicHistory.Count > RecentMusicHistorySize)
                _recentMusicHistory.RemoveAt(0);
        }

        int nextSource = _activeMusicSource == 0 ? _musicSourceB : _musicSourceA;

        if (path != null && File.Exists(path))
        {
            var (data, format, sampleRate) = LoadAudioFile(path);
            int buffer = AL.GenBuffer();
            AL.BufferData(buffer, format, data, sampleRate);
            AL.Source(nextSource, ALSourcei.Buffer, buffer);
            AL.Source(nextSource, ALSourcef.Gain, 0f);
            AL.SourcePlay(nextSource);
        }
        else if (path != null)
        {
            Console.WriteLine($"[Audio] Файл музыки не найден: {path}");
        }

        _musicFading = true;
        _musicFadeTimer = 0f;
    }

                                                                                                         
    public void SetAmbientPool(IReadOnlyList<string> pool)
    {
        _ambientPool.Clear();
        _ambientPool.AddRange(pool);
    }

                                                                                                          
    public void PlayAmbientFromPool(IReadOnlyList<string> pool)
    {
        SetAmbientPool(pool);
        PlayAmbient(PickRandomFrom(_ambientPool));
    }

                                                                                           
                                                                                          
                                                                          
    public void ShuffleAmbient()
    {
        if (_ambientPool.Count == 0) return;
        PlayAmbient(PickRandomFrom(_ambientPool, _currentAmbientPath), forceRestart: true);
    }

                                                                                           
                                                                                               
                                                                                              
                                                                                         
                                                                                            
                                                                                              
    public void PlayAmbient(string? path, bool forceRestart = false)
    {
        if (!_ready) return;
        if (!forceRestart && path == _currentAmbientPath) return;
        _currentAmbientPath = path;

        int nextSource = _activeAmbientSource == 0 ? _ambientSourceB : _ambientSourceA;

        if (path != null && File.Exists(path))
        {
            var (data, format, sampleRate) = LoadAudioFile(path);
            int buffer = AL.GenBuffer();
            AL.BufferData(buffer, format, data, sampleRate);
            AL.Source(nextSource, ALSourcei.Buffer, buffer);
            AL.Source(nextSource, ALSourcef.Gain, 0f);
            AL.SourcePlay(nextSource);
        }
        else if (path != null)
        {
            Console.WriteLine($"[Audio] Файл эмбиента не найден: {path}");
        }

        _ambientFading = true;
        _ambientFadeTimer = 0f;
    }

    public void StopAmbient() => PlayAmbient(null);

                                                                                           
                                                                                         
    private string? PickRandomFrom(List<string> pool, string? avoid = null)
    {
        if (pool.Count == 0) return null;
        if (pool.Count == 1) return pool[0];

        string pick;
        int guard = 0;
        do { pick = pool[_rng.Next(pool.Count)]; } while (pick == avoid && ++guard < 10);
        return pick;
    }

                                                                                              
                                                                                                
    public void Update(float dt)
    {
        if (!_ready) return;

        for (int i = _oneShotSources.Count - 1; i >= 0; i--)
        {
            int src = _oneShotSources[i];
            AL.GetSource(src, ALGetSourcei.SourceState, out int state);
            if ((ALSourceState)state == ALSourceState.Stopped)
            {
                AL.DeleteSource(src);
                _oneShotSources.RemoveAt(i);
            }
        }

        UpdateMusicFade(dt);
        UpdateAmbientFade(dt);

                                                                                      
                                                                                     
                                                                                       
                              
        if (!_musicFading && _currentMusicPath != null && _musicPool.Count > 0)
        {
            int active = _activeMusicSource == 0 ? _musicSourceA : _musicSourceB;
            AL.GetSource(active, ALGetSourcei.SourceState, out int state);
            if ((ALSourceState)state == ALSourceState.Stopped)
            {
                _currentMusicPath = null;                                                                                                        
                StartMusicWait();
            }
        }

        if (_musicWaiting)
        {
            _musicWaitTimer += dt;
            if (_musicWaitTimer >= _musicWaitTarget)
            {
                _musicWaiting = false;
                if (_musicPool.Count > 0)
                    PlayMusic(PickMoodAwareTrack(_moodLevel), forceRestart: true);
            }
        }

                                                                                          
                                                                                         
                               
        if (!_ambientFading && _currentAmbientPath != null && _ambientPool.Count > 0)
        {
            int active = _activeAmbientSource == 0 ? _ambientSourceA : _ambientSourceB;
            AL.GetSource(active, ALGetSourcei.SourceState, out int state);
            if ((ALSourceState)state == ALSourceState.Stopped)
                ShuffleAmbient();
        }
    }

                                                                                    
                                                                                      
                                                                         
                                                                                   
                                                                                  
                                        
    private void StartMusicWait()
    {
        _musicWaiting = true;
        _musicWaitTimer = 0f;

        float t = Math.Clamp(_moodValue / 100f, 0f, 1f);
        float baseSeconds = MusicIntervalCalmSeconds + (MusicIntervalActiveSeconds - MusicIntervalCalmSeconds) * t;
        float jitter = ((float)_rng.NextDouble() * 2f - 1f) * MusicIntervalJitterSeconds;

        _musicWaitTarget = MathF.Max(60f, baseSeconds + jitter);
    }

                 
                                                                                       
                                                                                     
                                                                                      
                                                                                   
                                                                                 
       
                                                    
                                                                                        
                                                                               
                                                                                  
                                                                  
                  
    private string? PickMoodAwareTrack(MoodLevel level)
    {
        if (_musicPool.Count == 0) return null;
        if (_musicPool.Count == 1) return _musicPool[0];

        List<string> shelf = level switch
        {
            MoodLevel.Active => _musicPool.Where(p => GetTrackDurationSeconds(p) <= ShortTrackMaxSeconds).ToList(),
            MoodLevel.Calm => _musicPool.Where(p => GetTrackDurationSeconds(p) >= LongTrackMinSeconds).ToList(),
            _ => _musicPool
        };

                                                                                    
                                                                                         
        if (shelf.Count == 0)
        {
            shelf = level switch
            {
                MoodLevel.Active => _musicPool.OrderBy(GetTrackDurationSeconds).Take(1).ToList(),
                MoodLevel.Calm => _musicPool.OrderByDescending(GetTrackDurationSeconds).Take(1).ToList(),
                _ => _musicPool
            };
        }

                                                                                  
                                                                 
        var withoutRecent = shelf.Where(p => !_recentMusicHistory.Contains(p)).ToList();
        var candidates = withoutRecent.Count > 0 ? withoutRecent : shelf;

        return candidates[_rng.Next(candidates.Count)];
    }

                                                                                         
                                                                                     
                                                                                    
                                                                                           
    private float GetTrackDurationSeconds(string path)
    {
        if (_trackDurationCache.TryGetValue(path, out float cached)) return cached;

        int buffer = LoadIntoCache(path);
        if (buffer == 0)
        {
            _trackDurationCache[path] = 0f;
            return 0f;
        }

        AL.GetBuffer(buffer, ALGetBufferi.Size, out int sizeBytes);
        AL.GetBuffer(buffer, ALGetBufferi.Channels, out int channels);
        AL.GetBuffer(buffer, ALGetBufferi.Bits, out int bits);
        AL.GetBuffer(buffer, ALGetBufferi.Frequency, out int frequency);

        float bytesPerSample = bits / 8f;
        float seconds = frequency > 0 && channels > 0 && bytesPerSample > 0
            ? sizeBytes / (channels * bytesPerSample * frequency)
            : 0f;

        _trackDurationCache[path] = seconds;
        return seconds;
    }

    private void UpdateMusicFade(float dt)
    {
        if (!_musicFading) return;
        _musicFadeTimer += dt;
        float t = Math.Clamp(_musicFadeTimer / MusicFadeDuration, 0f, 1f);

        int oldSource = _activeMusicSource == 0 ? _musicSourceA : _musicSourceB;
        int newSource = _activeMusicSource == 0 ? _musicSourceB : _musicSourceA;

        AL.Source(oldSource, ALSourcef.Gain, (1f - t) * _musicVolume);
        AL.Source(newSource, ALSourcef.Gain, t * _musicVolume);

        if (t >= 1f)
        {
            AL.SourceStop(oldSource);
            _activeMusicSource = _activeMusicSource == 0 ? 1 : 0;
            _musicFading = false;
        }
    }

    private void UpdateAmbientFade(float dt)
    {
        if (!_ambientFading) return;
        _ambientFadeTimer += dt;
        float t = Math.Clamp(_ambientFadeTimer / AmbientFadeDuration, 0f, 1f);

        int oldSource = _activeAmbientSource == 0 ? _ambientSourceA : _ambientSourceB;
        int newSource = _activeAmbientSource == 0 ? _ambientSourceB : _ambientSourceA;

        AL.Source(oldSource, ALSourcef.Gain, (1f - t) * _ambientVolume);
        AL.Source(newSource, ALSourcef.Gain, t * _ambientVolume);

        if (t >= 1f)
        {
            AL.SourceStop(oldSource);
            _activeAmbientSource = _activeAmbientSource == 0 ? 1 : 0;
            _ambientFading = false;
        }
    }

    public void Shutdown()
    {
        if (!_ready) return;
        foreach (var src in _oneShotSources) AL.DeleteSource(src);
        AL.DeleteSource(_musicSourceA);
        AL.DeleteSource(_musicSourceB);
        AL.DeleteSource(_ambientSourceA);
        AL.DeleteSource(_ambientSourceB);
        foreach (var buf in _soundCache.Values)
            if (buf != 0) AL.DeleteBuffer(buf);                                                              

        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(_context);
        ALC.CloseDevice(_device);
    }
}

             
                                                                        
                                                                        
                                                                            
                                                                         
              
internal static class WavLoader
{
    public static (byte[] Data, ALFormat Format, int SampleRate) Load(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        string riff = new(reader.ReadChars(4));
        if (riff != "RIFF") throw new InvalidDataException($"Не WAV-файл (нет RIFF): {path}");
        reader.ReadInt32();                           
        string wave = new(reader.ReadChars(4));
        if (wave != "WAVE") throw new InvalidDataException($"Не WAV-файл (нет WAVE): {path}");

        short channels = 1, bitsPerSample = 16;
        int sampleRate = 44100;
        byte[]? data = null;

        while (stream.Position < stream.Length)
        {
            string chunkId = new(reader.ReadChars(4));
            int chunkSize = reader.ReadInt32();

            if (chunkId == "fmt ")
            {
                reader.ReadInt16();                          
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32();             
                reader.ReadInt16();               
                bitsPerSample = reader.ReadInt16();
                int remaining = chunkSize - 16;
                if (remaining > 0) reader.ReadBytes(remaining);
            }
            else if (chunkId == "data")
            {
                data = reader.ReadBytes(chunkSize);
            }
            else
            {
                reader.ReadBytes(chunkSize);                                                  
            }

            if (chunkSize % 2 == 1 && stream.Position < stream.Length) reader.ReadByte();                
        }

        if (data == null) throw new InvalidDataException($"В WAV нет чанка data: {path}");

        var format = (channels, bitsPerSample) switch
        {
            (1, 8) => ALFormat.Mono8,
            (1, 16) => ALFormat.Mono16,
            (2, 8) => ALFormat.Stereo8,
            (2, 16) => ALFormat.Stereo16,
            _ => throw new NotSupportedException($"Неподдерживаемый формат WAV ({channels} каналов, {bitsPerSample} бит): {path}")
        };

        return (data, format, sampleRate);
    }
}
