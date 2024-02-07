using Microsoft.VisualBasic.FileIO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


namespace TreasureMountainSpriteDecompiler
{
    public partial class Form1 : Form
    {
        public List<Image> ImageDatas { get; set; } = new List<Image>();
        public readonly List<string> SpriteFiles = new();
        public readonly List<string> PANFiles = new();
        public readonly string EXEFile;
        public List<DataToCreate> DataToCreate { get; set; } = new List<DataToCreate>();
        public int CurrentIndex = 0;
        public string MyBeginPan { get; set; }

        public Form1()
        {
            InitializeComponent();
            var defaultPath = Directory.GetCurrentDirectory() + "/../../../Files/";
            var files = FileSystem.GetFiles(defaultPath + "PAK");
            foreach (var file in files)
            {
                SpriteFiles.Add(file);
            }
            var panFiles = FileSystem.GetFiles(defaultPath + "PAN");
            foreach (var file in panFiles)
            {
                PANFiles.Add(file);
            }
            EXEFile = defaultPath + "EXE/SST.EXE";
        }

        private void GetPanImages()
        {
            foreach (var file in PANFiles)
            {
                var imageCountStart = ImageDatas.Count;
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
                                // Check if the next byte is 0xA5, and if so, treat them separately
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
                                if (pixelCount == 32)
                                {
                                    newImage.EndStream = ms.Position;
                                }
                            }
                        }
                    }

                    ImageDatas.Add(newImage);
                    count++;
                }

                br.BaseStream.Position = 8;
                var tileTotal = tileWidth * tileHeight;
                var imageCountTest = 0;
                var startTileImageCount = ImageDatas.Count;
                while (br.BaseStream.Position < startToReadImageData)
                {
                    var currentX = 0;
                    var currentY = 0;
                    var imageChunk = new Image { FileName = file, StartStream = ms.Position, Index = count };
                    imageChunk.ImageData = new Bitmap(tileWidth * 8, tileHeight * 8);
                    ImageDatas.Add(imageChunk);

                    for (var tileCount = 0; tileCount < tileTotal;)
                    {
                        var data = br.ReadUInt16();

                        if (data == 0x0FFF)
                        {
                            var imageExcess = br.ReadByte();
                            byte nibble1 = (byte)((imageExcess >> 4) & 0x0F);
                            byte nibble2 = (byte)(imageExcess & 0x0F);
                            var xPos = (int)(nibble1 * 0.5 * 8);
                            var yPos = (int)(nibble2 * 8);
                            var imagePage = br.ReadByte();
                            using (Graphics g = Graphics.FromImage(imageChunk.ImageData))
                            {
                                // Draw the current image onto the modified image
                                g.DrawImage(imageChunk.ImageData, new Point(0, 0));

                                // Copy a row of 8 pixels from the specified image in ImageDatas

                                Bitmap rowToCopy = new Bitmap(tileWidth * 8, 8);

                                using (Graphics rowGraphics = Graphics.FromImage(rowToCopy))
                                {
                                    rowGraphics.DrawImage(ImageDatas[startTileImageCount + imagePage].ImageData,
                                        new Rectangle(0, 0, tileWidth * 8, 8),
                                        new Rectangle(0, yPos, tileWidth * 8, 8),
                                        GraphicsUnit.Pixel);
                                }

                                Rectangle destinationRect = new Rectangle(0, currentY * 8, tileWidth * 8, 8);
                                g.DrawImage(rowToCopy, destinationRect, new Rectangle(0, 0, tileWidth * 8, 8), GraphicsUnit.Pixel);
                            }
                            currentY++;
                            tileCount += tileWidth;
                        }
                        else
                        {
                            // Get the 1st nibble (most significant) in big-endian
                            int firstNibble = (data >> 4) & 0xF;
                            // Get the 2nd nibble in big-endian
                            int secondNibble = data & 0xF;
                            // Get the 3rd nibble in big-endian
                            int thirdNibble = ((data >> 12) & 0xF) + 1;
                            // Get the 4th nibble (least significant) in big-endian
                            int fourthNibble = (data >> 8) & 0xF;


                            // Check if the value of the second nibble is odd
                            bool isSecondNibbleOdd = (secondNibble % 2 == 1);
                            var tileToPrint = new Bitmap(8, 8);
                            var imageCount = firstNibble * 8 + (int)Math.Ceiling(secondNibble * 0.5) + fourthNibble * 128;
                            if (isSecondNibbleOdd)
                            {

                                var img1 = ImageDatas[imageCountStart + imageCount - 1].ImageData;
                                var img2 = ImageDatas[imageCountStart + imageCount].ImageData;
                                Bitmap bottomHalf = new Bitmap(img1.Width, img1.Height / 2);
                                Bitmap topHalf = new Bitmap(img2.Width, img2.Height / 2);

                                // Draw the bottom half of img1
                                using (Graphics g = Graphics.FromImage(bottomHalf))
                                {
                                    g.DrawImage(img1, new Rectangle(0, 0, img1.Width, img1.Height / 2), new Rectangle(0, img1.Height / 2, img1.Width, img1.Height / 2), GraphicsUnit.Pixel);
                                }

                                // Draw the top half of img2
                                using (Graphics g = Graphics.FromImage(topHalf))
                                {
                                    g.DrawImage(img2, new Rectangle(0, 0, img2.Width, img2.Height / 2), new Rectangle(0, 0, img2.Width, img2.Height / 2), GraphicsUnit.Pixel);
                                }

                                using (Graphics g = Graphics.FromImage(tileToPrint))
                                {
                                    g.DrawImage(bottomHalf, new Point(0, 0));
                                    g.DrawImage(topHalf, new Point(0, 4));
                                }

                            }
                            else
                            {

                                tileToPrint = ImageDatas[imageCountStart + imageCount].ImageData;

                            }

                            for (int i = 0; i < thirdNibble; i++)
                            {
                                using (Graphics g = Graphics.FromImage(imageChunk.ImageData))
                                {
                                    g.DrawImage(tileToPrint, new Rectangle(currentX * 8, currentY * 8, 8, 8));
                                }
                                currentX++;

                                // Check if it's time to move to the next row
                                if (currentX == tileWidth)
                                {
                                    currentX = 0;
                                    currentY++;
                                }
                                tileCount++;
                            }
                        }
                    }
                    imageChunk.EndStream = br.BaseStream.Position;
                    imageCountTest++;
                    count++;
                }
            }
        }

        private void GetLevelData()
        {
            //15F65 - Level data -  starting tile 89957
            //15F85 First row ended maybe?

            //11 chunks before a 3 byte with one byte which is the tile which is repeated x amount it looks like then another 3 bytes
            //157 some 2 byte thing - Unsure what it is?
            var imageToCreate = new Image { FileName = "SST.EXE", Index = ImageDatas.Count };
            imageToCreate.ImageData = new Bitmap(128 * 16, 128 * 18);
            using var ms = new MemoryStream(File.ReadAllBytes(EXEFile));
            using var br = new BinaryReader(ms);
            br.BaseStream.Position = 0x15F65;
            imageToCreate.StartStream = br.BaseStream.Position;
            for (var y = 0; y < 18; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    var tile = br.ReadByte();
                    var test = br.ReadByte();
                    using (Graphics g = Graphics.FromImage(imageToCreate.ImageData))
                    {

                        // Draw the current image onto the modified image
                        g.DrawImage(imageToCreate.ImageData, new Point(0, 0));

                        // Copy a row of 8 pixels from the specified image in ImageDatas

                        Bitmap rowToCopy = new Bitmap(128, 128);

                        using (Graphics rowGraphics = Graphics.FromImage(rowToCopy))
                        {
                            rowGraphics.DrawImage(ImageDatas[tile].ImageData,
                                new Rectangle(0, 0, 128, 128),
                                new Rectangle(0, 0, 128, 8),
                                GraphicsUnit.Pixel);
                        }

                        Rectangle destinationRect = new Rectangle(128 * 8, 128 * y, 128, 128);
                        g.DrawImage(rowToCopy, destinationRect, new Rectangle(0, 0, 128, 128), GraphicsUnit.Pixel);
                    }
                }
            }
            imageToCreate.EndStream = br.BaseStream.Position;
            ImageDatas.Add(imageToCreate);
        }

        //Goes through all files and then creates a list of images from each file
        private void buttonFindFiles_Click(object sender, EventArgs e)
        {
            //15F65 - Level data -  starting tile 89957
            //15F85 First row ended maybe?
            //1927 - 1850 = 77
            //77 or 78
            ImageDatas.Clear();
            DataToCreate.Clear();
            CurrentIndex = 0;
            GetPakImages();
            GetPanImages();
            buttonFindFiles.Visible = true;
            textBoxIndex.Visible = true;
            labelIndex.Visible = true;
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
                byte[] imageData = File.ReadAllBytes(fileName);
                foreach (var image in imageGroup)
                {
                    imageCount++;

                    using (MemoryStream ms = new MemoryStream(imageData))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        br.BaseStream.Position = image.StartStream;

                        var unknownStart = br.ReadUInt16();
                        if (unknownStart != 0x0000)
                        {
                            throw new Exception();
                        }

                        var imageWidth = br.ReadUInt16() * 2;
                        var imageHeight = br.ReadUInt16();
                        var xOrigin = br.ReadUInt16();
                        var yOrigin = br.ReadUInt16();

                        // Ignore the three unknown bytes
                        var unknown1 = br.ReadUInt16();
                        var unknown2 = br.ReadUInt16();
                        var unknown3 = br.ReadUInt16();

                        image.ImageData = new Bitmap(imageWidth, imageHeight);
                        image.Index = imageCount;
                        //DataToCreate.Add(new DataToCreate { Index = imageCount, Height = imageHeight, Width = imageWidth, Start = 0 });

                        var notEndOfFile = false;
                        var heightCount = 0;
                        while (!notEndOfFile)
                        {
                            var widthCount = 0;

                            var lineStart = br.ReadByte();
                            if (lineStart > 0x80)
                            {
                                while (widthCount < imageWidth)
                                {
                                    var pixelData = br.ReadByte();
                                    byte pixel1 = (byte)((pixelData >> 4) & 0x0F);
                                    byte pixel2 = (byte)(pixelData & 0x0F);

                                    var color1 = Colours.GetColorPAK(pixel1);
                                    var color2 = Colours.GetColorPAK(pixel2);
                                    image.ImageData.SetPixel(widthCount, heightCount, color1);
                                    image.ImageData.SetPixel(widthCount + 1, heightCount, color2);
                                    widthCount += 2;
                                }
                            }
                            else
                            {
                                var repeatAmountTest = lineStart;
                                for (var count = 0; count < repeatAmountTest; count += 2)
                                {
                                    var repeatAmount = br.ReadByte();
                                    var pixelData = br.ReadByte();
                                    byte pixel1 = (byte)((pixelData >> 4) & 0x0F);
                                    byte pixel2 = (byte)(pixelData & 0x0F);

                                    var color1 = Colours.GetColorPAK(pixel1);
                                    var color2 = Colours.GetColorPAK(pixel2);
                                    for (var i = 0; i < repeatAmount; i++)
                                    {
                                        if (widthCount < imageWidth)
                                        {
                                            image.ImageData.SetPixel(widthCount, heightCount, color1);
                                            image.ImageData.SetPixel(widthCount + 1, heightCount, color2);
                                            widthCount += 2;
                                        }
                                    }
                                }
                            }
                            heightCount++;
                            if (heightCount >= imageHeight)
                            {
                                notEndOfFile = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        int GetLineLength(byte b)
        {
            return b & 0x7F; // Removing the upper bit
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
            foreach (var imageGroup in imageGroups)
            {
                var start = imageGroup.Key.LastIndexOf('\\') + 1;

                // Adjusted folderName extraction
                var folderName = imageGroup.Key.Substring(start).Replace(".", "");

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
        }


        private void SetImageAndText()
        {
            if (pictureBoxSprite.Image != null)
            {
                pictureBoxSprite.Image.Dispose();
            }
            pictureBoxSprite.SizeMode = PictureBoxSizeMode.Zoom;
            textBoxIndex.Text = (CurrentIndex + 1).ToString();
            pictureBoxSprite.Image = UpscaleImage(ImageDatas[CurrentIndex].ImageData, 1000 / ImageDatas[CurrentIndex].ImageData.Width);
            labelImageList.Text = $"File: {ImageDatas[CurrentIndex].FileName[(ImageDatas[CurrentIndex].FileName.LastIndexOf('\\') + 1)..]} " +
                $"Image: {CurrentIndex + 1}/{ImageDatas.Count} - {ImageDatas[CurrentIndex].ImageData.Height}x{ImageDatas[CurrentIndex].ImageData.Width}(H x W) - " +
                $"Start: {ImageDatas[CurrentIndex].StartStream} | 0x{ImageDatas[CurrentIndex].StartStream:X2} " +
                $"End: {ImageDatas[CurrentIndex].EndStream} | 0x{ImageDatas[CurrentIndex].EndStream:X2} Code: {ImageDatas[CurrentIndex].StartByte}";
            labelIndex.Text = $"/{ImageDatas.Count}";
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

        private void textBoxIndex_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {

                e.Handled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBoxIndex_TextChanged(object sender, EventArgs e)
        {
            var value = int.Parse(textBoxIndex.Text);
            if (value <= 0)
            {
                value = 1;
            }
            if (value != CurrentIndex + 1)
            {
                CurrentIndex = value;
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
        public static Color Transparent = ColorTranslator.FromHtml("#00000000");

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

        public static Color GetColorPAK(int colorValue)
        {
            return colorValue switch
            {
                0 => Transparent,
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