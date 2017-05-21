﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ColorPickerWPF.Code;
using ColorPickerWPF.Properties;
using Color = System.Windows.Media.Color;
using ColorPalette = ColorPickerWPF.Code.ColorPalette;
using PixelFormat = System.Windows.Media.PixelFormat;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace ColorPickerWPF
{
    /// <summary>
    /// Interaction logic for ColorPickerControl.xaml
    /// </summary>
    public partial class ColorPickerControl : UserControl
    {
        public Color Color = Colors.White;

        public delegate void ColorPickerChangeHandler(Color color);

        public event ColorPickerChangeHandler OnPickColor;

        public List<ColorSwatchItem> ColorSwatch1 = new List<ColorSwatchItem>();
        public List<ColorSwatchItem> ColorSwatch2 = new List<ColorSwatchItem>();

        public bool IsSettingValues = false;

        protected const int NumColorsFirstSwatch = 39;
        protected const int NumColorsSecondSwatch = 112;

        public static ColorPalette ColorPalette;
        
        private Bitmap _colorPicker2;
        private byte[] _colorPicker2_Data;

        public ColorPickerControl()
        {
            InitializeComponent();

            // Load from file if possible
            /*
            if (File.Exists(Settings.Default.DefaultColorPaletteFilename))
            {
                try
                {
                    ColorPalette = ColorPalette.LoadFromXml(Settings.Default.DefaultColorPaletteFilename);
                }
                catch (Exception ex)
                {
                    ex = ex;
                }
            }*/

            if (ColorPalette == null)
            {
                ColorPalette = new ColorPalette();
                ColorPalette.InitializeDefaults();
            }


            ColorSwatch1.AddRange(ColorPalette.BuiltInColors.Take(NumColorsFirstSwatch).ToArray());

            ColorSwatch2.AddRange(ColorPalette.BuiltInColors.Skip(NumColorsFirstSwatch).Take(NumColorsSecondSwatch).ToArray());
            
            Swatch1.SwatchListBox.ItemsSource = ColorSwatch1;
            Swatch2.SwatchListBox.ItemsSource = ColorSwatch2;

            CustomColorSwatch.SwatchListBox.ItemsSource = ColorPalette.CustomColors;


            RSlider.Slider.Maximum = 255;
            GSlider.Slider.Maximum = 255;
            BSlider.Slider.Maximum = 255;
            ASlider.Slider.Maximum = 255;
            HSlider.Slider.Maximum = 360;
            SSlider.Slider.Maximum = 1;
            LSlider.Slider.Maximum = 1;


            RSlider.Label.Content = "R";
            RSlider.Slider.TickFrequency = 1;
            RSlider.Slider.IsSnapToTickEnabled = true;
            GSlider.Label.Content = "G";
            GSlider.Slider.TickFrequency = 1;
            GSlider.Slider.IsSnapToTickEnabled = true;
            BSlider.Label.Content = "B";
            BSlider.Slider.TickFrequency = 1;
            BSlider.Slider.IsSnapToTickEnabled = true;

            ASlider.Label.Content = "A";
            ASlider.Slider.TickFrequency = 1;
            ASlider.Slider.IsSnapToTickEnabled = true;

            HSlider.Label.Content = "H";
            HSlider.Slider.TickFrequency = 1;
            HSlider.Slider.IsSnapToTickEnabled = true;
            SSlider.Label.Content = "S";
            //SSlider.Slider.TickFrequency = 1;
            //SSlider.Slider.IsSnapToTickEnabled = true;
            LSlider.Label.Content = "V";
            //LSlider.Slider.TickFrequency = 1;
            //LSlider.Slider.IsSnapToTickEnabled = true;


            SetColor(Color);

        }


        public void SetColor(Color color)
        {
            Color = color;

            CustomColorSwatch.CurrentColor = color;

            IsSettingValues = true;

            RSlider.Slider.Value = Color.R;
            GSlider.Slider.Value = Color.G;
            BSlider.Slider.Value = Color.B;
            ASlider.Slider.Value = Color.A;

            SSlider.Slider.Value = Color.GetSaturation();
            LSlider.Slider.Value = Color.GetBrightness();
            HSlider.Slider.Value = Color.GetHue();

            ColorDisplayBorder.Background = new SolidColorBrush(Color);

            IsSettingValues = false;
            OnPickColor?.Invoke(color);

        }


        protected void SampleImageClick(BitmapSource img, Point pos)
        {
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/82a5731e-e201-4aaf-8d4b-062b138338fe/getting-pixel-information-from-a-bitmapimage?forum=wpf

            int stride = (int) img.Width*4;
            int size = (int) img.Height*stride;
            byte[] pixels = new byte[(int) size];

            img.CopyPixels(pixels, stride, 0);


            // Get pixel
            var x = (int) pos.X;
            var y = (int) pos.Y;

            int index = y*stride + 4*x;

            byte red = pixels[index];
            byte green = pixels[index + 1];
            byte blue = pixels[index + 2];
            byte alpha = pixels[index + 3];

            var color = Color.FromArgb(alpha, blue, green, red);
            SetColor(color);
        }


        private void SampleImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(SampleImage);
            var img = SampleImage.Source as BitmapSource;
            SampleImageClick(img, pos);
        }

        private void SampleImage2_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(SampleImage2);
            var img = SampleImage2.Source as BitmapSource;
            SampleImageClick(img, pos);
        }

        private void Swatch_OnOnPickColor(Color color)
        {
            SetColor(color);
        }

        private void HSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var s = Color.GetSaturation();
                var l = Color.GetBrightness();
                var h = (float) value;
                var a = (int) ASlider.Slider.Value;
                Color = Util.FromAhsb(a, h, s, l);

                SetColor(Color);
            }
        }




        private void RSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var val = (byte) value;
                Color.R = val;
                SetColor(Color);
            }
        }

        private void GSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var val = (byte) value;
                Color.G = val;
                SetColor(Color);
            }
        }

        private void BSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var val = (byte) value;
                Color.B = val;
                SetColor(Color);
            }
        }

        private void ASlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var val = (byte)value;
                Color.A = val;
                SetColor(Color);
            }
        }

        private void SSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var s = (float) value;
                var l = Color.GetBrightness();
                var h = Color.GetHue();
                var a = (int) ASlider.Slider.Value;
                Color = Util.FromAhsb(a, h, s, l);

                SetColor(Color);
            }

        }

        private void PickerHueSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateImageForHSV();
        }


        private void UpdateImageForHSV()
        {
            //var hueChange = (int)((PickerHueSlider.Value / 360.0) * 240);
            var sliderHue = (float)PickerHueSlider.Value;

            if (_colorPicker2 == null)
            {
                LoadColorPicker2();
            }

            if (sliderHue <= 0f || sliderHue >= 360f)
            {
                // No hue change just return
                SampleImage2.Source = Util.GetBitmapImage(_colorPicker2);
                return;
            }

            var outputImage = new Bitmap(_colorPicker2.Width, _colorPicker2.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            var rect = new Rectangle(0, 0, _colorPicker2.Width, _colorPicker2.Height);
            var imagedata = outputImage.LockBits(rect, ImageLockMode.ReadOnly, outputImage.PixelFormat);
            var imagedepth = Image.GetPixelFormatSize(imagedata.PixelFormat) / 8;
            var imagebuffer = new byte[imagedata.Width * imagedata.Height * imagedepth];

            // Copy the image data from our color picker into the array for our output image, so we dont change the values of the original image.
            Array.Copy(_colorPicker2_Data, imagebuffer, imagebuffer.Length);

            for (int x = 0; x < imagedata.Width; x++)
            {
                for (int y = 0; y < imagedata.Height; y++)
                {
                    var offset = (y * imagedata.Width + x) * imagedepth;

                    System.Drawing.Color pixel;
                    // The byte-order of the pixel-format is endian-dependent
                    if (BitConverter.IsLittleEndian)
                    {
                        // On little-endian architectures the byte-order is BGRA
                        pixel = System.Drawing.Color.FromArgb(imagebuffer[offset + 3], imagebuffer[offset + 2],
                            imagebuffer[offset + 1], imagebuffer[offset]);
                    }
                    else
                    {
                        // On big-endian architectures the byte-order is ARGB
                        pixel = System.Drawing.Color.FromArgb(imagebuffer[offset], imagebuffer[offset + 1],
                            imagebuffer[offset + 2], imagebuffer[offset + 3]);
                    }

                    var newHue = (float)(sliderHue + pixel.GetHue());
                    if (newHue >= 360) newHue -= 360;

                    var color = Util.FromAhsb((int)255, newHue, pixel.GetSaturation(), pixel.GetBrightness());

                    if (BitConverter.IsLittleEndian)
                    {
                        imagebuffer[offset + 0] = color.B;
                        imagebuffer[offset + 1] = color.G;
                        imagebuffer[offset + 2] = color.R;
                        imagebuffer[offset + 3] = color.A;
                    }
                    else
                    {
                        imagebuffer[offset + 0] = color.A;
                        imagebuffer[offset + 1] = color.R;
                        imagebuffer[offset + 2] = color.G;
                        imagebuffer[offset + 3] = color.B;
                    }
                }
            }

            // Copy back the changed pixels to the image
            Marshal.Copy(imagebuffer, 0, imagedata.Scan0, imagebuffer.Length);
            outputImage.UnlockBits(imagedata);
            SampleImage2.Source = Util.GetBitmapImage(outputImage);
        }

        private void LoadColorPicker2()
        {
            //Load the embedded resource
            _colorPicker2 = new Bitmap(Application.GetResourceStream(
                               new Uri("pack://application:,,,/ColorPickerWPF;component/Resources/colorpicker2.png",
                                   UriKind.Absolute)).Stream);

            //Ensure that the resource is in the expected format
            var img = new Bitmap(_colorPicker2.Width, _colorPicker2.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(img))
            {
                g.DrawImage(_colorPicker2, new Rectangle(0, 0, _colorPicker2.Width, _colorPicker2.Height));
            }
            _colorPicker2 = img;

            //Load the pixel-data from the bitmap into a byte-array that we can access directly later.
            var rect = new Rectangle(0, 0, _colorPicker2.Width, _colorPicker2.Height);
            var imagedata = _colorPicker2.LockBits(rect, ImageLockMode.ReadOnly, _colorPicker2.PixelFormat);
            var imagedepth = Image.GetPixelFormatSize(imagedata.PixelFormat) / 8;
            var imagebuffer = new byte[imagedata.Width * imagedata.Height * imagedepth];
            Marshal.Copy(imagedata.Scan0, imagebuffer, 0, imagebuffer.Length);
            _colorPicker2_Data = imagebuffer;
            _colorPicker2.UnlockBits(imagedata);
        }

        private void LSlider_OnOnValueChanged(double value)
        {
            if (!IsSettingValues)
            {
                var s = Color.GetSaturation();
                var l = (float) value;
                var h = Color.GetHue();
                var a = (int) ASlider.Slider.Value;
                Color = Util.FromAhsb(a, h, s, l);

                SetColor(Color);
            }
        }


        public void SaveCustomPalette(string filename)
        {
            var colors = CustomColorSwatch.GetColors();
            ColorPalette.CustomColors = colors;

            try
            {
                ColorPalette.SaveToXml(filename);
            }
            catch (Exception ex)
            {
                ex = ex;
            }
        }

        public void LoadCustomPalette(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    ColorPalette = ColorPalette.LoadFromXml(filename);

                    CustomColorSwatch.SwatchListBox.ItemsSource = ColorPalette.CustomColors.ToList();

                    // Do regular one too

                    ColorSwatch1.Clear();
                    ColorSwatch2.Clear();
                    ColorSwatch1.AddRange(ColorPalette.BuiltInColors.Take(NumColorsFirstSwatch).ToArray());
                    ColorSwatch2.AddRange(ColorPalette.BuiltInColors.Skip(NumColorsFirstSwatch).Take(NumColorsSecondSwatch).ToArray());
                    Swatch1.SwatchListBox.ItemsSource = ColorSwatch1;
                    Swatch2.SwatchListBox.ItemsSource = ColorSwatch2;

                }
                catch (Exception ex)
                {
                    ex = ex;
                }

            }
        }


        public void LoadDefaultCustomPalette()
        {
            LoadCustomPalette(Settings.Default.DefaultColorPaletteFilename);
        }

    }
}
