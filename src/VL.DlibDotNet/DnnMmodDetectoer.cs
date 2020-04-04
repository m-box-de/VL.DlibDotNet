using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using OpenCvSharp;
using DlibDotNet;
using DlibDotNet.Dnn;

namespace VL.DlibDotNet
{
    public class DnnMmodDetectoer : IDisposable
    {
        LossMmod Detector;

        public DnnMmodDetectoer(string Model)
        {
            try
            {
                Detector = LossMmod.Deserialize(Model);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException.Message);
            }
        }

        public void Update(Mat Image, out IList<Rectangle> Rectangles)
        {
            Rectangles = new List<Rectangle>();

            try
            {
                if (Image.Empty())
                {
                    return;
                }

                var array = new byte[Image.Width * Image.Height * Image.ElemSize()];
                Marshal.Copy(Image.CvtColor(ColorConversionCodes.BGR2RGB).Data, array, 0, array.Length);

                using (var img = new Matrix<RgbPixel>(array, Image.Height, Image.Width, Image.ElemSize()))
                {
                    // Upsampling the image will allow us to detect smaller faces but will cause the
                    // program to use more RAM and run longer.
                    //while (img.Size < 1800 * 1800)
                    //if (PiramidUp)
                    //      Dlib.PyramidUp(img);

                    // Note that you can process a bunch of images in a std::vector at once and it runs
                    // much faster, since this will form mini-batches of images and therefore get
                    // better parallelism out of your GPU hardware.  However, all the images must be
                    // the same size.  To avoid this requirement on images being the same size we
                    // process them individually in this example.
                    using (var dets = Detector.Operator(img))
                        foreach (var det in dets)
                        {
                            foreach (var d in det)
                                Rectangles.Add(d.Rect);
                        }

                    //Console.WriteLine("Hit enter to process the next image.");
                    //Console.ReadKey();
                }
            }
            catch (CudaException e)
            {
               // Console.WriteLine(e.Message);
                Console.WriteLine(e.DllName);
                Console.WriteLine(e.DriverVersion);
                Console.WriteLine(e.ErrorCode);
                Console.WriteLine(e.ErrorName);
                Console.WriteLine(e.ErrorMessage);
                Console.WriteLine(e.RuntimeVersion);
            }
        }

        public void Dispose()
        {
            Detector.Dispose();
        }
    }
}
