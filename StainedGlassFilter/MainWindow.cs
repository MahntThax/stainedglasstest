using ImageProcessor;
using ImageProcessor.Imaging.Filters.EdgeDetection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StainedGlassFilter.Classes;

namespace StainedGlassFilter
{
    public partial class MainWindow : Form
    {
        Bitmap originalImg;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(imgOpenDiag.ShowDialog() == DialogResult.OK)
            {
                originalImg = LoadImage(imgOpenDiag.OpenFile());
                Bitmap resultImg = new Bitmap(originalImg);
                resultImg = StainedGlassColor(originalImg);
                pictureBox1.Image = resultImg;
            }
        }

        private Bitmap LoadImage(Stream stream)
        {
            return new Bitmap(stream);
        }

        private Bitmap StainedGlassColor(Bitmap original)
        {
            BitmapData imgData = original.LockBits(new Rectangle(0, 0, original.Width, original.Height), ImageLockMode.ReadWrite, original.PixelFormat);
            IntPtr imagePtr = imgData.Scan0;

            //Value of the bytes of the image
            int bytes = Math.Abs(imgData.Stride) * original.Height;
            byte[] imgValues = new byte[bytes];
            byte[] resultValues = new byte[bytes];

            Marshal.Copy(imagePtr, imgValues, 0, bytes);
            original.UnlockBits(imgData);

            int blockSize = 20;                                                         //Size of the blocks that simulate the pieces of the glass
            int neighbourHoodTotal = 0;
            int originalOffset = 0;
            int resultOffset = 0;
            int currentPixelDistance = 0;
            int nearestPixelDistance = 0;
            int nearesttPointIndex = 0;

            Random randomizer = new Random();

            List<VoronoiPoint> randomPointsList = new List<VoronoiPoint>();


            //create the voronoi points
            for (int row = 0; row < original.Height - blockSize; row += blockSize)
            {
                for (int col = 0; col < original.Width - blockSize; col += blockSize)
                {
                    originalOffset = row * imgData.Stride + col * 3;

                    neighbourHoodTotal = 0;

                    for (int y = 0; y < blockSize; y++)
                    {
                        for (int x = 0; x < blockSize; x++)
                        {
                            resultOffset = originalOffset + y * imgData.Stride + x * 4;
                            neighbourHoodTotal += imgValues[resultOffset];
                            neighbourHoodTotal += imgValues[resultOffset + 1];
                            neighbourHoodTotal += imgValues[resultOffset + 2];
                        }
                    }

                    randomizer = new Random(neighbourHoodTotal);

                    VoronoiPoint randomPoint = new VoronoiPoint(randomizer.Next(0, blockSize) + col,
                                                                    randomizer.Next(0, blockSize) + row);
                    randomPointsList.Add(randomPoint);
                }
            }

            int rowOffset = 0;
            int colOffset = 0;

            for (int pixelOffset = 0; pixelOffset < imgValues.Length - 3; pixelOffset += 3)
            {
                rowOffset = pixelOffset / imgData.Stride;
                colOffset = (pixelOffset % imgData.Stride) / 3;

                currentPixelDistance = 0;
                nearestPixelDistance = blockSize * 4;
                nearesttPointIndex = 0;

                List<VoronoiPoint> pointSubset = new List<VoronoiPoint>();

                pointSubset.AddRange(from p in randomPointsList
                                     where
                                        rowOffset >= p.YOffset - blockSize * 2 &&
                                        rowOffset <= p.YOffset + blockSize * 2
                                     select p);

                for (int index = 0; index < pointSubset.Count; index++)
                {
                    currentPixelDistance = CalculateDistanceChebyshev(pointSubset[index].XOffset, colOffset, pointSubset[index].YOffset, rowOffset);

                    if (currentPixelDistance <= nearestPixelDistance)
                    {
                        nearestPixelDistance = currentPixelDistance;
                        nearesttPointIndex = index;

                        if (nearestPixelDistance <= blockSize / 2)
                        {
                            break;
                        }
                    }
                }

                PixelValues tmpPixel = new PixelValues(
                                                colOffset,
                                                rowOffset,
                                                imgValues[pixelOffset],
                                                imgValues[pixelOffset + 1],
                                                imgValues[pixelOffset + 2]);

                pointSubset[nearesttPointIndex].AddPixel(tmpPixel);
            }

            for (int index = 0; index < randomPointsList.Count; index++)
            {
                randomPointsList[index].CalculateAvarages();

                for (int jndex = 0; jndex < randomPointsList[index].PixelCollection.Count; jndex++)
                {
                    resultOffset = randomPointsList[index].PixelCollection[jndex].YPos *
                           imgData.Stride +
                           randomPointsList[index].PixelCollection[jndex].XPos * 3;


                    resultValues[resultOffset] = randomPointsList[index].RedAvarage;
                    resultValues[resultOffset + 1] = randomPointsList[index].GreenAvarage;
                    resultValues[resultOffset + 2] = randomPointsList[index].BlueAvarage;
                }
            }

            Bitmap resultImg = new Bitmap(original.Width, original.Height);

            BitmapData resultData = resultImg.LockBits(new Rectangle(0, 0,
               resultImg.Width, resultImg.Height),
               ImageLockMode.WriteOnly,
               resultImg.PixelFormat);

            Marshal.Copy(resultValues, 0, resultData.Scan0, resultValues.Length);

            resultImg.UnlockBits(resultData);

            return resultImg;
        }

        private int CalculateDistanceChebyshev(int x1, int x2, int y1, int y2)
        {
            return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        }
    }
}
