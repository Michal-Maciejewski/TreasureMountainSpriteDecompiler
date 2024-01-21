using Microsoft.VisualBasic.FileIO;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace TreasureMountainSpriteDecompiler
{
    public partial class Form1 : Form
    {
        public List<Image> ImageDatas { get; set; } = new List<Image>();
        public readonly List<string> SpriteFiles = new();
        public readonly List<string> PANFiles = new();
        public List<DataToCreate> DataToCreate { get; set; } = new List<DataToCreate>();
        public int CurrentIndex = 0;
        public string MyBeginPan { get; set; }

        public Form1()
        {
            InitializeComponent();
            var files = FileSystem.GetFiles(Directory.GetCurrentDirectory() + "/../../../Files", Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly);
            foreach (var file in files)
            {
                SpriteFiles.Add(file);
            }
            var panFiles = FileSystem.GetFiles(Directory.GetCurrentDirectory() + "/../../../Files/PAN");
            foreach(var file in panFiles)
            {
                PANFiles.Add(file);
            }
        }

        private void GetPanImages()
        { 
            foreach (var file in PANFiles)
            {
                using var ms = new MemoryStream(File.ReadAllBytes(file));
                using var br = new BinaryReader(ms);
                var startToReadImageData = br.ReadUInt16() + 8;
                var tileWidth = br.ReadUInt16();
                var tileHeight = br.ReadUInt16();
                var byteAmount = br.ReadUInt16();
                ms.Position = startToReadImageData;
                //Images - read each pixel?
                var color1 = Colours.GetColor(0);
                var color2 = Colours.GetColor(0);
                var leftover = 0;
                // Iterate through every image
                var count = 0;
                while (br.BaseStream.Position < startToReadImageData + byteAmount)
                {
                    // New image
                    var newImage = new Image { FileName = file, StartStream = ms.Position, Index = count };
                    newImage.ImageData = new Bitmap(8, 8);
                    var width = 0;
                    var height = 0;

                    // Iterate through all pixels
                    for (var pixelCount = 0; pixelCount < 32;)
                    {
                        if (leftover > 0)
                        {
                            // Handle leftover pixels from the previous image
                            var leftOverAmount = leftover;
                            leftover = 0;

                            for (var i = 0; i < leftOverAmount; i++)
                            {
                                if (width == 8)
                                {
                                    width = 0;
                                    height++;
                                }

                                if (height == 8)
                                {
                                    leftover = leftOverAmount - i;
                                    newImage.EndStream = ms.Position;
                                    break;
                                }

                                newImage.ImageData.SetPixel(width, height, color1);
                                newImage.ImageData.SetPixel(width + 1, height, color2);
                                width += 2;
                                pixelCount++;
                            }
                        }
                        else
                        {
                            var bbpData = br.ReadByte();
                            var check = false;
                            var amount = 0;

                            if (bbpData == 0xA5)
                            {
                                // Handle repeating pixels
                                check = true;
                                amount = (int)br.ReadByte();
                                // Check if the next byte is 0xA5 or 0x5A, and if so, treat them separately
                                if (amount == 0xA5)
                                {
                                    check = false;
                                }
                                else
                                {
                                    bbpData = br.ReadByte();
                                }
                            }

                            byte pixel1 = (byte)((bbpData >> 4) & 0x0F);
                            byte pixel2 = (byte)(bbpData & 0x0F);

                            color1 = Colours.GetColor(pixel1);
                            color2 = Colours.GetColor(pixel2);

                            if (check)
                            {
                                // Handle repeated pixels
                                for (var i = 0; i < amount; i++)
                                {
                                    if (width == 8)
                                    {
                                        width = 0;
                                        height++;
                                    }

                                    if (height == 8)
                                    {
                                        leftover = amount - i;
                                        newImage.EndStream = ms.Position;
                                        break;
                                    }

                                    newImage.ImageData.SetPixel(width, height, color1);
                                    newImage.ImageData.SetPixel(width + 1, height, color2);
                                    width += 2;
                                    pixelCount++;
                                }
                            }
                            else
                            {
                                // Handle non-repeating pixels
                                if (width == 8)
                                {
                                    width = 0;
                                    height++;
                                }

                                newImage.ImageData.SetPixel(width, height, color1);
                                newImage.ImageData.SetPixel(width + 1, height, color2);
                                width += 2;
                                pixelCount++;
                                if(pixelCount == 32)
                                {
                                    newImage.EndStream = ms.Position;
                                }
                            }
                        }
                    }

                    ImageDatas.Add(newImage);
                    count++;
                }

                //ms.Position = 10;
                //var tileTotal = tileWidth * tileHeight;
                //while(br.BaseStream.Position < startToReadImageData)
                //{
                //    var imageChunk = new Image { FileName = "Test", StartStream = ms.Position  };
                //    for(var tileCount = 0; tileCount < tileTotal; tileCount++)
                //    {

                //    }
                //    ImageDatas.Add(imageChunk);
                //}
            }
        }


        //Goes through all files and then creates a list of images from each file
        private void buttonFindFiles_Click(object sender, EventArgs e)
        {
            ImageDatas.Clear();
            DataToCreate.Clear();
            CurrentIndex = 0;
            GetPakImages();
            GetPanImages();
            SetImageAndText();
        }

        private void GetPakImages()
        {
            foreach (var file in SpriteFiles)
            {
                Image? lastStream = null;
                using var ms = new MemoryStream(File.ReadAllBytes(file));
                using var br = new BinaryReader(ms);
                var imageCount = br.ReadInt32();
                var unknownOne = br.ReadInt16();
                var unknownTWo = br.ReadInt16();
                for (var i = 0; i < imageCount; i++)
                {
                    var startStream = br.ReadUInt16() * 16;
                    var image = new Image { StartStream = startStream, FileName = file };
                    if (i != 0 && i < imageCount - 1)
                    {
                        lastStream.EndStream = startStream;
                    }
                    else if (i == imageCount - 1)
                    {
                        image.EndStream = br.BaseStream.Length;
                        lastStream.EndStream = startStream;
                    }
                    lastStream = image;
                    ImageDatas.Add(image);
                }
                var endOfFile = br.ReadUInt32();
                if (endOfFile != 0xFFFFFFFF)
                {
                    throw new Exception();
                }
            }
            ReadImages();
        }

        //Sprite count per file
        //Begin 42
        //Close 133
        //Sprites 223
        //Read through each image by file grouping and turn it into an image
        private void ReadImages()
        {
            var imageCount = 0;
            var imageGroups = ImageDatas.GroupBy(a => a.FileName).ToList();
            foreach (var imageGroup in imageGroups)
            {
                var fileName = imageGroup.Key;
                using var ms = new MemoryStream(File.ReadAllBytes(fileName));
                using var br = new BinaryReader(ms);
                foreach (var image in imageGroup)
                {
                    imageCount++;
                    br.BaseStream.Position = image.StartStream;
                    var unknownStart = br.ReadUInt16();
                    if (unknownStart != 0x0000)
                    {
                        throw new Exception();
                    }
                    var imageWidth = (br.ReadUInt16() + 1) * 2;
                    var imageHeight = br.ReadUInt16();
                    var xOrigin = br.ReadUInt16();
                    var yOrigin = br.ReadUInt16();

                    //Unsure what these 3 bytes do
                    var maybeColorCombo = br.ReadUInt16();
                    var nextCombo = br.ReadUInt16();
                    var nextCombo2 = br.ReadUInt16();

                    image.ImageData = new Bitmap(imageWidth, imageHeight);
                    DataToCreate.Add(new DataToCreate { Index = imageCount, Height = imageHeight, Width = imageWidth, Start = 0 });

                    var widthCount = 0;
                    var heightCount = 0;
                    var firstBytePosition = br.BaseStream.Position + 1;
                    var checkFirstColourByte = br.ReadByte();
                    image.StartByte = "0x" + checkFirstColourByte.ToString("X2");
                    var repeaterByte = false;
                    //If the first byte is equal to or less than 0x0F it means you need to repeat values 2 byte values
                    //If first byte value is 1 to 12
                    if (checkFirstColourByte < 0x80)
                    {
                        repeaterByte = true;
                    }
                    br.BaseStream.Position = br.BaseStream.Position - 1;
                    var ZeroCodeColour = new byte();
                    while (br.BaseStream.Position < image.EndStream)
                    {
                        var bbpData = br.ReadByte();

                        byte pixel1 = (byte)((bbpData >> 4) & 0x0F); // Extract first pixel value (bits: 0011)
                        byte pixel2 = (byte)(bbpData & 0x0F); // Extract second pixel value (bits: 1010)

                        var color1 = Colours.GetColor(pixel1);
                        var color2 = Colours.GetColor(pixel2);

                        void SetPixel()
                        {
                            if (heightCount < imageHeight)
                            {
                                if (widthCount < imageWidth)
                                {
                                    image.ImageData.SetPixel(widthCount, heightCount, color1);
                                    widthCount++;
                                    if (widthCount >= imageWidth)
                                    {
                                        widthCount = 0;
                                        heightCount++;
                                        image.ImageData.SetPixel(widthCount, heightCount, color2);
                                    }
                                    else
                                    {
                                        image.ImageData.SetPixel(widthCount, heightCount, color2);
                                    }
                                    widthCount++;
                                }
                                else
                                {
                                    if (heightCount < imageHeight - 1)
                                    {
                                        widthCount = 0;
                                        heightCount++;
                                        image.ImageData.SetPixel(widthCount, heightCount, color1);
                                        widthCount++;
                                        image.ImageData.SetPixel(widthCount, heightCount, color2);
                                        widthCount++;
                                    }
                                }
                            }
                        }

                        //This was speculation but it seems it does nothing, it's based off maybe the first byte read/Width/Height
                        if (repeaterByte)
                        {
                            if (checkFirstColourByte == 0x00 && heightCount == 1)
                            {
                                if (imageCount == 194)
                                {
                                    var tesadfddf = 0;
                                }
                                pixel1 = (byte)((ZeroCodeColour >> 4) & 0x0F); // Extract first pixel value (bits: 0011)
                                pixel2 = (byte)(ZeroCodeColour & 0x0F); // Extract second pixel value (bits: 1010)

                                color1 = Colours.GetColor(pixel1);
                                color2 = Colours.GetColor(pixel2);
                                for (var i = 0; i < widthCount; i += 2)
                                {
                                    image.ImageData.SetPixel(widthCount, heightCount, color1);
                                    image.ImageData.SetPixel(widthCount + 1, heightCount, color2);
                                }
                                heightCount++;
                                widthCount = 0;

                                pixel1 = (byte)((bbpData >> 4) & 0x0F); // Extract first pixel value (bits: 0011)
                                pixel2 = (byte)(bbpData & 0x0F); // Extract second pixel value (bits: 1010)

                                color1 = Colours.GetColor(pixel1);
                                color2 = Colours.GetColor(pixel2);
                            }
                            if (checkFirstColourByte == 0xC6 && heightCount == imageHeight - 1 && widthCount == 0)
                            {
                                var getColors = br.ReadByte();
                                byte pixel11 = (byte)((getColors >> 4) & 0x0F);
                                byte pixel22 = (byte)(getColors & 0x0F);
                                color1 = Colours.GetColor(pixel11);
                                color2 = Colours.GetColor(pixel22);
                                for (var i = 0; i < widthCount - 1; i++)
                                {
                                    image.ImageData.SetPixel(widthCount, heightCount, color1);
                                    widthCount++;
                                }
                            }
                            if (bbpData <= 12 && bbpData >= 1 && heightCount < 44)
                            {
                                if ((bbpData > 0x00 && bbpData <= 0x0F) && br.BaseStream.Position == firstBytePosition)
                                {
                                    widthCount++;
                                    widthCount++;
                                }
                                else
                                {
                                    if (checkFirstColourByte == 0x00 && widthCount == 3 && heightCount == 0)
                                    {
                                        ZeroCodeColour = bbpData;
                                    }
                                    if (widthCount >= imageWidth && bbpData >= 0x0A && bbpData <= 0x0F)
                                    {
                                        heightCount++;
                                        widthCount = 2;
                                    }
                                    else
                                    {
                                        //How this works is, any value 10 or below? Might be higher? 01 to 0A
                                        //Current byte is the repeat amount
                                        //Next byte is the colour values
                                        //Repeat x amount with using the 2 colour values, so if they are different colours repeat that
                                        //colour combo x times
                                        var getColors = br.ReadByte();
                                        byte pixel11 = (byte)((getColors >> 4) & 0x0F);
                                        byte pixel22 = (byte)(getColors & 0x0F);
                                        color1 = Colours.GetColor(pixel11);
                                        color2 = Colours.GetColor(pixel22);
                                        for (var counts = 0; counts < bbpData; counts++)
                                        {
                                            if (heightCount < imageHeight)
                                            {
                                                if (widthCount >= imageWidth)
                                                {
                                                    heightCount++;
                                                    widthCount = 0;
                                                }
                                                if (heightCount < imageHeight)
                                                {
                                                    image.ImageData.SetPixel(widthCount, heightCount, color1);
                                                    widthCount++;
                                                    if (widthCount >= imageWidth)
                                                    {
                                                        heightCount++;
                                                        widthCount = 0;
                                                    }
                                                    image.ImageData.SetPixel(widthCount, heightCount, color2);
                                                    widthCount++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                SetPixel();
                            }
                        }
                        else
                        {
                            SetPixel();
                        }
                    }
                }
            }
            CurrentIndex = 0;
            SetImageAndText();
            buttonFindFiles.Visible = true;


        }

        private void buttonRight_Click(object sender, EventArgs e)
        {
            if (CurrentIndex < ImageDatas.Count - 1)
            {
                CurrentIndex++;
            }
            else
            {
                CurrentIndex = 0;
            }
            SetImageAndText();
        }

        private void buttonLeft_Click(object sender, EventArgs e)
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
            else
            {
                CurrentIndex = ImageDatas.Count - 1;
            }
            SetImageAndText();
        }

        private void buttonSaveFile_Click(object sender, EventArgs e)
        {
            var count = 0;
            var filePath = Directory.GetCurrentDirectory() + "\\Images\\";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            var imageGroups = ImageDatas.GroupBy(a => a.FileName);
            foreach(var imageGroup in imageGroups)
            {
                var start = imageGroup.Key.LastIndexOf('\\') + 1;
                var end = imageGroup.Key.LastIndexOf('.');

                // Adjusted folderName extraction
                var folderName = imageGroup.Key.Substring(start, end - start);

                // Use Path.Combine for constructing paths
                var directoryTest = Path.Combine(filePath, folderName);

                if (!Directory.Exists(directoryTest))
                {
                    Directory.CreateDirectory(directoryTest);
                }

                foreach (var image in imageGroup)
                {
                    // Use Path.Combine for constructing file paths
                    var imagePath = Path.Combine(directoryTest, $"image{image.Index:D4}.png");
                    image.ImageData.Save(imagePath, ImageFormat.Png);
                }
            }
            //foreach (var item in ImageDatas)
            //{
            //    item.ImageData.Save(filePath + "image" + count + ".bmp", ImageFormat.Bmp);
            //    count++;
            //}
        }


        private void SetImageAndText()
        {
            if(pictureBoxSprite.Image != null)
            {
                pictureBoxSprite.Image.Dispose();
            }
            pictureBoxSprite.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSprite.Image = UpscaleImage(ImageDatas[CurrentIndex].ImageData, 1000 / ImageDatas[CurrentIndex].ImageData.Width);
            labelImageList.Text = $"File: {ImageDatas[CurrentIndex].FileName[(ImageDatas[CurrentIndex].FileName.LastIndexOf('\\') + 1)..]} " +
                $"Image: {CurrentIndex + 1}/{ImageDatas.Count} - {ImageDatas[CurrentIndex].ImageData.Height}x{ImageDatas[CurrentIndex].ImageData.Width}(H x W) - " +
                $"Start: {ImageDatas[CurrentIndex].StartStream} | 0x{ImageDatas[CurrentIndex].StartStream:X2} " +
                $"End: {ImageDatas[CurrentIndex].EndStream} | 0x{ImageDatas[CurrentIndex].EndStream:X2} Code: {ImageDatas[CurrentIndex].StartByte}";
        }

        private Bitmap UpscaleImage(Bitmap original, int scaleFactor)
        {
            int newWidth = original.Width * scaleFactor;
            int newHeight = original.Height * scaleFactor;

            Bitmap scaledImage = new(newWidth, newHeight, PixelFormat.Format64bppArgb);

            if (!checkBoxImage.Checked)
            {
                // Draws the image in the specified size with quality mode set to HighQuality
                using Graphics graphics = Graphics.FromImage(scaledImage);
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(original, 0, 0, newWidth, newHeight);
            }
            else
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        int originalX = x / scaleFactor;
                        int originalY = y / scaleFactor;

                        Color pixelColor = original.GetPixel(originalX, originalY);
                        scaledImage.SetPixel(x, y, pixelColor);
                    }
                }
            }

            return scaledImage;
        }

        private void checkBoxImage_CheckStateChanged(object sender, EventArgs e)
        {
            if (ImageDatas.Count() > 0)
            {
                SetImageAndText();
            }
        }
    }

    static class Colours
    {
        public static Color Blue = ColorTranslator.FromHtml("#0000AA");
        public static Color LightBlue = ColorTranslator.FromHtml("#5555FF");
        public static Color Brown = ColorTranslator.FromHtml("#AA5500");
        public static Color LightRed = ColorTranslator.FromHtml("#FF5555");
        public static Color Red = ColorTranslator.FromHtml("#AA0000");
        public static Color Magenta = ColorTranslator.FromHtml("#AA00AA");
        public static Color LightMagenta = ColorTranslator.FromHtml("#FF55FF");
        public static Color Cyan = ColorTranslator.FromHtml("#00AAAA");
        public static Color Green = ColorTranslator.FromHtml("#00AA00");
        public static Color LightGreen = ColorTranslator.FromHtml("#55FF55");
        public static Color LightCyan = ColorTranslator.FromHtml("#55FFFF");
        public static Color Yellow = ColorTranslator.FromHtml("#FFFF55");
        public static Color LightGray = ColorTranslator.FromHtml("#AAAAAA");
        public static Color DarkGray = ColorTranslator.FromHtml("#555555");
        public static Color Black = ColorTranslator.FromHtml("#000000");
        public static Color White = ColorTranslator.FromHtml("#FFFFFF");

        public static Color GetColor(int colorValue)
        {
            return colorValue switch
            {
                0 => Blue,
                1 => LightBlue,
                2 => Brown,
                3 => LightRed,
                4 => Red,
                5 => Magenta,
                6 => LightMagenta,
                7 => Cyan,
                8 => Green,
                9 => LightGreen,
                10 => LightCyan,
                11 => Yellow,
                12 => White,
                13 => LightGray,
                14 => DarkGray,
                15 => Black,
                _ => White,
            };

            //0x1CA6
        }
    }


    public class DataToCreate
    {
        public int Index { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Start { get; set; }
    }

    public class Image
    {
        public int Index { get; set; }
        public string FileName { get; set; }
        public string StartByte { get; set; }
        public long StartStream { get; set; } = 0;
        public long EndStream { get; set; } = 0;
        public Bitmap ImageData { get; set; }
    }
}