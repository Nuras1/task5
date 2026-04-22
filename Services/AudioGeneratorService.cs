using MeltySynth;
using NAudio.Wave;

namespace task5.Services
{
    public class AudioGeneratorService
    {
        private readonly string _sf2 = "wwwroot/soundfonts/general.sf2";

        public byte[] Generate(int seed, int page)
        {
            var finalSeed = seed + page * 1000;
            var rng = new Random(finalSeed);
            int sampleRate = 44100;

            int genre = finalSeed % 4;

            var sf = new SoundFont(_sf2);
            var synth = new Synthesizer(sf, sampleRate);

            using var ms = new MemoryStream();
            using var writer = new WaveFileWriter(ms, new WaveFormat(sampleRate, 2));

            switch (genre)
            {
                case 0: GenerateEDM(synth, writer, rng); break;
                case 1: GenerateLoFi(synth, writer, rng); break;
                case 2: GeneratePop(synth, writer, rng); break;
                default: GenerateAmbient(synth, writer, rng); break;
            }

            return ms.ToArray();
        }

        private void GenerateEDM(Synthesizer synth, WaveFileWriter writer, Random rng)
        {
            int bpm = rng.Next(120, 140);
            int sr = 44100;
            int beat = sr * 60 / bpm;
            int total = sr * 60;

            int leadCh = 0;
            int padCh = 1;
            int bassCh = 2;
            int drumCh = 9;

            synth.ProcessMidiMessage(0xC0 | leadCh, 81, 0, 0);
            synth.ProcessMidiMessage(0xC0 | padCh, 89, 0, 0);
            synth.ProcessMidiMessage(0xC0 | bassCh, 33, 0, 0);

            int root = 60 + rng.Next(12);

            for (int i = 0; i < total; i++)
            {
                if (i % (beat / 4) == 0)
                {
                    int note = root + new[] { 0, 4, 7, 12 }[rng.Next(4)];
                    synth.NoteOn(leadCh, note, 120);
                    synth.NoteOff(leadCh, note);
                }

                if (i % (beat * 4) == 0)
                {
                    PlayChord(synth, padCh, root, rng);
                }

                if (i % beat == 0)
                {
                    int bass = root - 12;
                    synth.NoteOn(bassCh, bass, 100);
                    synth.NoteOff(bassCh, bass);
                }

                if (i % beat == 0)
                {
                    synth.NoteOn(drumCh, 36, 120);
                    synth.NoteOff(drumCh, 36);
                }

                if (i % (beat / 2) == 0)
                {
                    synth.NoteOn(drumCh, 42, 60);
                    synth.NoteOff(drumCh, 42);
                }

                if (i % (beat * 2) == beat)
                {
                    synth.NoteOn(drumCh, 38, 100);
                    synth.NoteOff(drumCh, 38);
                }

                Render(synth, writer);
            }
        }
        private void GenerateLoFi(Synthesizer synth, WaveFileWriter writer, Random rng)
        {
            int bpm = rng.Next(60, 80);
            int sr = 44100;
            int beat = sr * 60 / bpm;
            int total = sr * 80;

            int chordCh = 0;
            int bassCh = 1;
            int drumCh = 9;

            synth.ProcessMidiMessage(0xC0 | chordCh, 4, 0, 0);
            synth.ProcessMidiMessage(0xC0 | bassCh, 33, 0, 0);

            int root = 60;

            for (int i = 0; i < total; i++)
            {
                if (i % (beat * 2) == 0)
                {
                    PlayChord(synth, chordCh, root, rng);
                }

                if (i % beat == 0)
                {
                    synth.NoteOn(bassCh, root - 12, 70);
                    synth.NoteOff(bassCh, root - 12);
                }

                if (i % (beat * 2) == beat)
                {
                    synth.NoteOn(drumCh, 38, 60);
                    synth.NoteOff(drumCh, 38);
                }

                if (i % (beat / 2) == 0 && rng.NextDouble() > 0.5)
                {
                    synth.NoteOn(drumCh, 42, 40);
                    synth.NoteOff(drumCh, 42);
                }

                Render(synth, writer);
            }
        }

        private void GeneratePop(Synthesizer synth, WaveFileWriter writer, Random rng)
        {
            int bpm = rng.Next(90, 110);
            int sr = 44100;
            int beat = sr * 60 / bpm;
            int total = sr * 70;

            int leadCh = 0;
            int chordCh = 1;
            int bassCh = 2;

            synth.ProcessMidiMessage(0xC0 | leadCh, 40, 0, 0);
            synth.ProcessMidiMessage(0xC0 | chordCh, 0, 0, 0);
            synth.ProcessMidiMessage(0xC0 | bassCh, 33, 0, 0);

            int[] scale = { 60, 62, 64, 65, 67, 69, 71 };

            int root = scale[0];

            for (int i = 0; i < total; i++)
            {
                if (i % (beat / 2) == 0)
                {
                    int note = scale[rng.Next(scale.Length)];
                    synth.NoteOn(leadCh, note, 100);
                    synth.NoteOff(leadCh, note);
                }

                if (i % (beat * 4) == 0)
                {
                    PlayChord(synth, chordCh, root, rng);
                }

                if (i % beat == 0)
                {
                    synth.NoteOn(bassCh, root - 12, 90);
                    synth.NoteOff(bassCh, root - 12);
                }

                Render(synth, writer);
            }
        }

        private void GenerateAmbient(Synthesizer synth, WaveFileWriter writer, Random rng)
        {
            int sr = 44100;
            int total = sr * 90;

            int padCh = 0;

            synth.ProcessMidiMessage(0xC0 | padCh, 89, 0, 0);

            int root = 50 + rng.Next(20);

            for (int i = 0; i < total; i++)
            {
                if (i % (sr * 5) == 0)
                {
                    PlayChord(synth, padCh, root, rng);
                }

                if (i % (sr * 10) == 0)
                {
                    root += rng.Next(-3, 3);
                }

                Render(synth, writer);
            }
        }
        private void PlayChord(Synthesizer synth, int ch, int root, Random rng)
        {
            int[][] chords =
            {
        new[]{0,4,7},
        new[]{0,3,7},
        new[]{0,4,11}
    };

            var chord = chords[rng.Next(chords.Length)];

            foreach (var n in chord)
                synth.NoteOn(ch, root + n, 70);
        }

        private void Render(Synthesizer synth, WaveFileWriter writer)
        {
            float[] l = new float[1];
            float[] r = new float[1];

            synth.Render(l, r);

            writer.WriteSample(l[0]);
            writer.WriteSample(r[0]);
        }
    }
}