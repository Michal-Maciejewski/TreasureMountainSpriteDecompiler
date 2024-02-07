namespace TreasureMountainSpriteDecompiler
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonFindFiles = new Button();
            pictureBoxSprite = new PictureBox();
            buttonLeft = new Button();
            buttonRight = new Button();
            labelImageList = new Label();
            buttonSaveFile = new Button();
            checkBoxImage = new CheckBox();
            textBoxIndex = new TextBox();
            labelIndex = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSprite).BeginInit();
            SuspendLayout();
            // 
            // buttonFindFiles
            // 
            buttonFindFiles.Location = new Point(1909, 991);
            buttonFindFiles.Name = "buttonFindFiles";
            buttonFindFiles.Size = new Size(112, 34);
            buttonFindFiles.TabIndex = 0;
            buttonFindFiles.Text = "Find Files";
            buttonFindFiles.UseVisualStyleBackColor = true;
            buttonFindFiles.Click += buttonFindFiles_Click;
            // 
            // pictureBoxSprite
            // 
            pictureBoxSprite.BackgroundImageLayout = ImageLayout.None;
            pictureBoxSprite.Location = new Point(133, 106);
            pictureBoxSprite.Name = "pictureBoxSprite";
            pictureBoxSprite.Size = new Size(698, 523);
            pictureBoxSprite.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxSprite.TabIndex = 1;
            pictureBoxSprite.TabStop = false;
            // 
            // buttonLeft
            // 
            buttonLeft.Location = new Point(1864, 1041);
            buttonLeft.Name = "buttonLeft";
            buttonLeft.Size = new Size(43, 34);
            buttonLeft.TabIndex = 2;
            buttonLeft.Text = "<";
            buttonLeft.UseVisualStyleBackColor = true;
            buttonLeft.Click += buttonLeft_Click;
            // 
            // buttonRight
            // 
            buttonRight.Location = new Point(2013, 1041);
            buttonRight.Name = "buttonRight";
            buttonRight.Size = new Size(43, 34);
            buttonRight.TabIndex = 3;
            buttonRight.Text = ">";
            buttonRight.UseVisualStyleBackColor = true;
            buttonRight.Click += buttonRight_Click;
            // 
            // labelImageList
            // 
            labelImageList.AutoSize = true;
            labelImageList.Location = new Point(166, 1050);
            labelImageList.Name = "labelImageList";
            labelImageList.Size = new Size(0, 25);
            labelImageList.TabIndex = 4;
            // 
            // buttonSaveFile
            // 
            buttonSaveFile.Location = new Point(1909, 1126);
            buttonSaveFile.Name = "buttonSaveFile";
            buttonSaveFile.Size = new Size(112, 34);
            buttonSaveFile.TabIndex = 5;
            buttonSaveFile.Text = "Save Files";
            buttonSaveFile.UseVisualStyleBackColor = true;
            buttonSaveFile.Click += buttonSaveFile_Click;
            // 
            // checkBoxImage
            // 
            checkBoxImage.AutoSize = true;
            checkBoxImage.Location = new Point(1854, 1193);
            checkBoxImage.Name = "checkBoxImage";
            checkBoxImage.Size = new Size(192, 29);
            checkBoxImage.TabIndex = 6;
            checkBoxImage.Text = "High Quality Image";
            checkBoxImage.UseVisualStyleBackColor = false;
            checkBoxImage.CheckStateChanged += checkBoxImage_CheckStateChanged;
            // 
            // textBoxIndex
            // 
            textBoxIndex.Location = new Point(166, 994);
            textBoxIndex.Name = "textBoxIndex";
            textBoxIndex.Size = new Size(150, 31);
            textBoxIndex.TabIndex = 7;
            textBoxIndex.Visible = false;
            textBoxIndex.TextChanged += textBoxIndex_TextChanged;
            textBoxIndex.KeyPress += textBoxIndex_KeyPress;
            // 
            // labelIndex
            // 
            labelIndex.AutoSize = true;
            labelIndex.Location = new Point(322, 994);
            labelIndex.Name = "labelIndex";
            labelIndex.Size = new Size(0, 25);
            labelIndex.TabIndex = 8;
            labelIndex.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2229, 1307);
            Controls.Add(labelIndex);
            Controls.Add(textBoxIndex);
            Controls.Add(checkBoxImage);
            Controls.Add(buttonSaveFile);
            Controls.Add(labelImageList);
            Controls.Add(buttonRight);
            Controls.Add(buttonLeft);
            Controls.Add(pictureBoxSprite);
            Controls.Add(buttonFindFiles);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxSprite).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonFindFiles;
        private PictureBox pictureBoxSprite;
        private Button buttonLeft;
        private Button buttonRight;
        private Label labelImageList;
        private Button buttonSaveFile;
        private CheckBox checkBoxImage;
        private TextBox textBoxIndex;
        private Label labelIndex;
    }
}