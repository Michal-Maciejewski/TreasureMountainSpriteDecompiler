using Microsoft.VisualBasic.FileIO;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace TreasureMountainSpriteDecompiler
{
    public partial class Form1 : Form
    {
        public List<Image> ImageDatas { get; set; } = new List<Image>();
        public readonly List<string> SpriteFiles = new List<string>();
        public List<DataToCreate> DataToCreate { get; set; } = new List<DataToCreate>();
        public int CurrentIndex = 0;

        public Form1()
        {
            InitializeComponent();
            var files = FileSystem.GetFiles(Directory.GetCurrentDirectory() + "/../../../Files");
            foreach (var file in files)
            {
                SpriteFiles.Add(file);
            }
        }

        //Goes through all files and then creates a list of images from each file
        private void buttonFindFiles_Click(object sender, EventArgs e)
        {
            ImageDatas.Clear();
            DataToCreate.Clear();
            CurrentIndex = 0;
            foreach (var file in SpriteFiles)
            {
                Image lastStream = null;
                using (var ms = new MemoryStream(File.ReadAllBytes(file)))
                {
                    using (var br = new BinaryReader(ms))
                    {
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
                using (var ms = new MemoryStream(File.ReadAllBytes(fileName)))
                {
                    using (var br = new BinaryReader(ms))
                    {
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
            foreach (var item in ImageDatas)
            {
                item.ImageData.Save(filePath + "image" + count + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                count++;
            }
        }


        private void SetImageAndText()
        {
            pictureBoxSprite.Image = ImageDatas[CurrentIndex].ImageData;
            pictureBoxSprite.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSprite.Width = 1000;
            pictureBoxSprite.Height = 1000;
            pictureBoxSprite.ClientSize = new Size(1000, 1000);
            pictureBoxSprite.ClientSize = new Size(1000, 1000);
            labelImageList.Text = $"File: {ImageDatas[CurrentIndex].FileName.Substring(ImageDatas[CurrentIndex].FileName.LastIndexOf('\\') + 1)} Image: {CurrentIndex + 1}/{ImageDatas.Count} - {ImageDatas[CurrentIndex].ImageData.Height}x{ImageDatas[CurrentIndex].ImageData.Width}(H x W) - Start: {ImageDatas[CurrentIndex].StartStream} | 0x{ImageDatas[CurrentIndex].StartStream.ToString("X2")} End: {ImageDatas[CurrentIndex].EndStream} | 0x{ImageDatas[CurrentIndex].EndStream.ToString("X2")} Code: {ImageDatas[CurrentIndex].StartByte}";
        }

    }

    static class Colours
    {
        public static Color Green = ColorTranslator.FromHtml("#00AA00");
        public static Color LightBlue = ColorTranslator.FromHtml("#5555FF");
        public static Color Brown = ColorTranslator.FromHtml("#AA5500");
        public static Color LightRed = ColorTranslator.FromHtml("#FF5555");
        public static Color Red = ColorTranslator.FromHtml("#AA0000");
        public static Color Magenta = ColorTranslator.FromHtml("#AA00AA");
        public static Color LightMagenta = ColorTranslator.FromHtml("#FF55FF");
        public static Color Cyan = ColorTranslator.FromHtml("#00AAAA");
        public static Color Blue = ColorTranslator.FromHtml("#0000AA");
        public static Color LightGreen = ColorTranslator.FromHtml("#55FF55");
        public static Color LightCyan = ColorTranslator.FromHtml("#55FFFF");
        public static Color Yellow = ColorTranslator.FromHtml("#FFFF55");
        public static Color White = ColorTranslator.FromHtml("#FFFFFF");
        public static Color LightGray = ColorTranslator.FromHtml("#AAAAAA");
        public static Color DarkGray = ColorTranslator.FromHtml("#555555");
        public static Color Black = ColorTranslator.FromHtml("#000000");

        public static Color GetColor(int colorValue)
        {
            switch (colorValue)
            {
                case 0:
                    return Blue;
                case 1:
                    return LightBlue;
                case 2:
                    return Brown;
                case 3:
                    return LightRed;
                case 4:
                    return Red;
                case 5:
                    return Magenta;
                case 6:
                    return LightMagenta;
                case 7:
                    return Cyan;
                case 8:
                    return Green;
                case 9:
                    return LightGreen;
                case 10:
                    return LightCyan;
                case 11:
                    return Yellow;
                case 12:
                    return White;
                case 13:
                    return LightGray;
                case 14:
                    return DarkGray;
                case 15:
                    return Black;
                default:
                    return White;
            }
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
        public string FileName { get; set; }
        public string StartByte { get; set; }
        public long StartStream { get; set; } = 0;
        public long EndStream { get; set; } = 0;
        public Bitmap ImageData { get; set; }
    }
}