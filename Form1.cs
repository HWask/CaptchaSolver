using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptchaSolver
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.pictureBox15.Image = null;
            this.pictureBox13.Image = null;
            this.pictureBox14.Image = null;
            this.pictureBox12.Image = null;

            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.PicturePath = this.openFileDialog1.FileName;
                this.LoadedPicture = new Bitmap(this.openFileDialog1.FileName);
                this.pictureBox.Image = this.LoadedPicture;
                this.button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;
            this.button5.Enabled = false;

            this.pictureBox1.Image = this.RemoveMesh(this.LoadedPicture);
            this.pictureBox3.Image = this.RemoveBackground((Bitmap)this.pictureBox1.Image);
            this.pictureBox5.Image = this.RemoveArcsSmart((Bitmap)this.pictureBox3.Image, 5, 0.9f);
            this.pictureBox2.Image = this.RemoveArtifacts((Bitmap)this.pictureBox5.Image);
            this.pictureBox4.Image = this.Binarise((Bitmap)this.pictureBox2.Image);
            this.pictureBox6.Image = this.VerticalHistogram((Bitmap)this.pictureBox4.Image);
            this.pictureBox7.Image = this.SegmentLetters((Bitmap)this.pictureBox4.Image);
            this.pictureBox8.Image = this.LettersNotNormalized[0];
            this.pictureBox9.Image = this.LettersNotNormalized[1];
            this.pictureBox10.Image = this.LettersNotNormalized[2];
            this.pictureBox11.Image = this.LettersNotNormalized[3];


            if (checkBox1.Checked) // Derotate
            {
                this.pictureBox15.Image = this.NormalizeRotationCharacter(this.LettersNotNormalized[0]);
                this.pictureBox13.Image = this.NormalizeRotationCharacter(this.LettersNotNormalized[1]);
                this.pictureBox14.Image = this.NormalizeRotationCharacter(this.LettersNotNormalized[2]);
                this.pictureBox12.Image = this.NormalizeRotationCharacter(this.LettersNotNormalized[3]);
                this.pictureBox17.Image = this.NormalizeRectSize((Bitmap)this.pictureBox13.Image);
                this.pictureBox19.Image = this.NormalizeRectSize((Bitmap)this.pictureBox15.Image);
                this.pictureBox18.Image = this.NormalizeRectSize((Bitmap)this.pictureBox14.Image);
                this.pictureBox16.Image = this.NormalizeRectSize((Bitmap)this.pictureBox12.Image);
            }
            else
            {
                this.pictureBox19.Image = this.NormalizeRectSize(this.LettersNotNormalized[0]);
                this.pictureBox17.Image = this.NormalizeRectSize(this.LettersNotNormalized[1]);
                this.pictureBox18.Image = this.NormalizeRectSize(this.LettersNotNormalized[2]);
                this.pictureBox16.Image = this.NormalizeRectSize(this.LettersNotNormalized[3]);

            }

            this.button3.Enabled = true;
            this.button4.Enabled = true;
            this.button1.Enabled = true;
            this.button5.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string SavePath;

            if (this.PicturePath != null)
            {
                SavePath = System.IO.Path.GetDirectoryName(this.PicturePath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(this.PicturePath);
            }
            else
            {
                SavePath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                SavePath = System.IO.Path.GetDirectoryName(SavePath);
                SavePath = SavePath.Replace("file:\\", "");
            }

            this.pictureBox3.Image.Save(SavePath + "_nobg.png", System.Drawing.Imaging.ImageFormat.Png);
            this.pictureBox5.Image.Save(SavePath + "_arcinletter.png", System.Drawing.Imaging.ImageFormat.Png);
            this.pictureBox2.Image.Save(SavePath + "_noartifacts.png", System.Drawing.Imaging.ImageFormat.Png);
            this.pictureBox4.Image.Save(SavePath + "_binarised.png", System.Drawing.Imaging.ImageFormat.Png);
            this.pictureBox7.Image.Save(SavePath + "_segmented.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            this.label16.Text = trackBar1.Value.ToString();
        }

        private void OnValueChanged2(object sender, EventArgs e)
        {
            this.label17.Text = trackBar2.Value.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string HTMLRiot = this.DoGetRequest("https://signup.leagueoflegends.com/de/signup");
            if (HTMLRiot != null)
            {
                string CaptchaURL = "https://signup.leagueoflegends.com" + this.ParseCaptchaURL(HTMLRiot);
                Bitmap captcha = this.DoCaptchaRequest(CaptchaURL);

                if (captcha != null)
                {
                    this.LoadedPicture = new Bitmap(captcha);
                    this.pictureBox.Image = this.LoadedPicture;
                    this.button2.Enabled = true;
                    this.PicturePath = null;
                }
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.trackBar2.Enabled = false;
            this.CaptchasCount = this.trackBar2.Value;
            this.RiotCaptchas = new Bitmap[this.CaptchasCount];
            int index = 0;
            this.CaptchaCounter = 0;

            this.progressBar1.Maximum = this.trackBar2.Value;
            this.progressBar1.Value = 0;

            while (index < this.trackBar2.Value)
            {
                string HTMLRiot = this.DoGetRequest("https://signup.leagueoflegends.com/de/signup");
                if (HTMLRiot != null)
                {
                    string CaptchaURL = "https://signup.leagueoflegends.com" + this.ParseCaptchaURL(HTMLRiot);
                    Bitmap captcha = this.DoCaptchaRequest(CaptchaURL);

                    if (captcha != null)
                    {
                        this.RiotCaptchas[index] = new Bitmap(captcha);
                        index++;
                        this.progressBar1.Value += 1;
                    }
                }
            }

            this.trackBar2.Enabled = true;
            this.button8.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string SavePath;

            if (this.PicturePath != null)
            {
                SavePath = System.IO.Path.GetDirectoryName(this.PicturePath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(this.PicturePath);
            }
            else
            {
                SavePath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                SavePath = System.IO.Path.GetDirectoryName(SavePath);
                SavePath = SavePath.Replace("file:\\", "");
            }

            this.pictureBox19.Image.Save(SavePath + "_letter1.png", System.Drawing.Imaging.ImageFormat.Png);
            this.pictureBox17.Image.Save(SavePath + "_letter2.png", System.Drawing.Imaging.ImageFormat.Png);
            this.pictureBox18.Image.Save(SavePath + "_letter3.png", System.Drawing.Imaging.ImageFormat.Png);
            this.pictureBox16.Image.Save(SavePath + "_letter4.png", System.Drawing.Imaging.ImageFormat.Png);
        }


        private void SolveCaptcha()
        {
            if (this.CaptchasCount == 0)
                return;

            this.button9.Enabled = false;
            this.button11.Enabled = false;
            Bitmap captcha = this.RiotCaptchas[CaptchaCounter];

            this.LoadedPicture = captcha;
            this.pictureBox.Image = this.LoadedPicture;
            this.pictureBox1.Image = this.RemoveMesh(this.LoadedPicture);
            this.pictureBox3.Image = this.RemoveBackground((Bitmap)this.pictureBox1.Image);
            this.pictureBox5.Image = this.RemoveArcsSmart((Bitmap)this.pictureBox3.Image, 5, 0.9f);
            this.pictureBox2.Image = this.RemoveArtifacts((Bitmap)this.pictureBox5.Image);
            this.pictureBox4.Image = this.Binarise((Bitmap)this.pictureBox2.Image);
            this.pictureBox6.Image = this.VerticalHistogram((Bitmap)this.pictureBox4.Image);
            this.pictureBox7.Image = this.SegmentLetters((Bitmap)this.pictureBox4.Image);
            this.pictureBox8.Image = this.LettersNotNormalized[0];
            this.pictureBox9.Image = this.LettersNotNormalized[1];
            this.pictureBox10.Image = this.LettersNotNormalized[2];
            this.pictureBox11.Image = this.LettersNotNormalized[3];


            if (checkBox1.Checked) // Derotate
            {
                this.pictureBox15.Image = this.NormalizeRotationCharacter(this.LettersNotNormalized[0]);
                this.pictureBox13.Image = this.NormalizeRotationCharacter(this.LettersNotNormalized[1]);
                this.pictureBox14.Image = this.NormalizeRotationCharacter(this.LettersNotNormalized[2]);
                this.pictureBox12.Image = this.NormalizeRotationCharacter(this.LettersNotNormalized[3]);
                this.pictureBox17.Image = this.NormalizeRectSize((Bitmap)this.pictureBox13.Image);
                this.pictureBox19.Image = this.NormalizeRectSize((Bitmap)this.pictureBox15.Image);
                this.pictureBox18.Image = this.NormalizeRectSize((Bitmap)this.pictureBox14.Image);
                this.pictureBox16.Image = this.NormalizeRectSize((Bitmap)this.pictureBox12.Image);
            }
            else
            {
                this.pictureBox19.Image = this.NormalizeRectSize(this.LettersNotNormalized[0]);
                this.pictureBox17.Image = this.NormalizeRectSize(this.LettersNotNormalized[1]);
                this.pictureBox18.Image = this.NormalizeRectSize(this.LettersNotNormalized[2]);
                this.pictureBox16.Image = this.NormalizeRectSize(this.LettersNotNormalized[3]);

            }

            this.button9.Enabled = true;
            this.button11.Enabled = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SolveCaptcha();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //save letters



            this.CaptchaCounter++;

            if (this.CaptchaCounter >= this.CaptchasCount)
                MessageBox.Show("All captchas trained!");

            if (this.CaptchaCounter < this.CaptchasCount)
                SolveCaptcha();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.CaptchaCounter++;

            if (this.CaptchaCounter >= this.CaptchasCount)
                MessageBox.Show("All captchas trained!");

            if(this.CaptchaCounter < this.CaptchasCount)
                SolveCaptcha();
        }
    }
}
