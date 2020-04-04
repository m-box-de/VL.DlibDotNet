using System;
using System.Runtime.InteropServices;
using OpenCvSharp;
using DlibDotNet;

namespace VL.DlibDotNet
{
    public class VideoTracker : IDisposable
    {
        CorrelationTracker tracker;

        public VideoTracker()
        {
            tracker = new CorrelationTracker();
        }

        public void Update(Mat Image, DRectangle DRectangle, out DRectangle dRectangle, bool SetTracker = false)
        {
            dRectangle = new DRectangle();
            try
            {
                if (Image.Empty())
                {
                    return;
                }

                var array = new byte[Image.Width * Image.Height * Image.ElemSize()];
                Marshal.Copy(Image.Data, array, 0, array.Length);

                if (SetTracker && !DRectangle.IsEmpty)
                {
                    using (var img = Dlib.LoadImageData<BgrPixel>(array, (uint)Image.Height, (uint)Image.Width, (uint)(Image.Width * Image.ElemSize())))
                    {
                        tracker.StartTrack(img, DRectangle);
                    }
                }
                else
                {
                    if (tracker.GetPosition().IsEmpty)
                        return;

                    using (var img = Dlib.LoadImageData<BgrPixel>(array, (uint)Image.Height, (uint)Image.Width, (uint)(Image.Width * Image.ElemSize())))
                    {
                        tracker.Update(img);
                        dRectangle = tracker.GetPosition();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Dispose()
        {
            tracker.Dispose();
        }
    }
}
