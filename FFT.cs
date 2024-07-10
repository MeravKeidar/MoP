using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NAudio.Wave;
using MathNet.Numerics.IntegralTransforms;

namespace MoP
{
    public class FFT : GH_Component
    {
        public FFT()
          : base("FFT", "FFT",
              "Finds the n most dominant frequencies in an audio file",
              "Category", "Subcategory")
        {
        }

        public override Guid ComponentGuid => new Guid("31b7c6b6-76f6-417c-83bf-499174d4aa2c");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Grasshopper.Kernel.Parameters.Param_FilePath(), "AudioFilePath", "A", "Path to the audio file", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumberOfFrequencies", "N", "Number of dominant frequencies to output", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("DominantFrequencies", "F", "The n most dominant frequencies", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_String ghFilePath = null;
            int numberOfFrequencies = 0;

            if (!DA.GetData(0, ref ghFilePath)) return;
            if (!DA.GetData(1, ref numberOfFrequencies)) return;

            string filePath = ghFilePath?.Value;
            if (string.IsNullOrEmpty(filePath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid file path.");
                return;
            }

            // Ensure the file path is properly formatted
            filePath = filePath.Replace(@"\", "/");

            if (!File.Exists(filePath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The specified file does not exist.");
                return;
            }

            List<double> dominantFrequencies;
            try
            {
                dominantFrequencies = GetDominantFrequencies(filePath, numberOfFrequencies);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error processing file: {ex.Message}");
                return;
            }

            DA.SetDataList(0, dominantFrequencies);
        }

        private List<double> GetDominantFrequencies(string filePath, int n)
        {
            List<double> frequencies = new List<double>();

            using (var reader = new AudioFileReader(filePath))
            {
                int sampleRate = reader.WaveFormat.SampleRate;
                int samples = (int)reader.Length / reader.WaveFormat.BlockAlign;
                float[] buffer = new float[samples];
                reader.Read(buffer, 0, samples);

                // Convert buffer to complex format
                System.Numerics.Complex[] fftBuffer = buffer.Select(x => new System.Numerics.Complex(x, 0)).ToArray();

                // Perform FFT using MathNet.Numerics
                Fourier.Forward(fftBuffer, FourierOptions.Matlab);

                // Get magnitudes
                var magnitudes = fftBuffer.Select(c => c.Magnitude).ToArray();

                // Get the most dominant frequencies
                var topFrequencies = magnitudes
                    .Select((value, index) => new { Value = value, Frequency = index * sampleRate / (double)samples })
                    .OrderByDescending(x => x.Value)
                    .Take(n)
                    .Select(x => x.Frequency)
                    .ToList();

                frequencies.AddRange(topFrequencies);
            }

            return frequencies;
        }
    }
}
