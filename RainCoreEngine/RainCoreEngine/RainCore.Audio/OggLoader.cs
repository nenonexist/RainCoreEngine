using NVorbis;
using OpenTK.Audio.OpenAL;

namespace RainCore;

             
                                                                      
                                                                         
                                                                        
                               
              
internal static class OggLoader
{
    public static (byte[] Data, ALFormat Format, int SampleRate) Load(string path)
    {
        using var vorbis = new VorbisReader(path);

        int channels = vorbis.Channels;
        int sampleRate = vorbis.SampleRate;

        var floatBuffer = new float[vorbis.TotalSamples * channels];
        int read = vorbis.ReadSamples(floatBuffer, 0, floatBuffer.Length);

                                                                      
                                                      
        var pcm = new byte[read * 2];
        for (int i = 0; i < read; i++)
        {
            short sample = (short)Math.Clamp(floatBuffer[i] * short.MaxValue, short.MinValue, short.MaxValue);
            pcm[i * 2] = (byte)(sample & 0xFF);
            pcm[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        var format = channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
        return (pcm, format, sampleRate);
    }
}