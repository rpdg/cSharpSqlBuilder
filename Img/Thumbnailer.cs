﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Lyu.Img
{
	/// <summary>
	/// Description of Thumbnailer.
	/// </summary>
	public class Thumbnailer
	{
		
		private static Size ShrinkSize (Size srcSize, int maxWidth, int maxHeight)
		{
			int width = srcSize.Width;
			int height = srcSize.Height;

			int w, h;

			if (width <= maxWidth && height <= maxHeight) {
				w = width;
				h = height;
			} else if (width > height) {
				w = maxWidth;
				h = w * height / width;
			} else {
				h = maxHeight;
				w = h * width / height;
			}

			return new Size (w, h);
		}


		/// <summary> 
		/// 生成缩略图 
		/// </summary> 
		/// <param name="imgSource">原图片</param> 
		/// <param name="newWidth">缩略图宽度</param> 
		/// <param name="newHeight">缩略图高度</param> 
		/// <param name="isCut">是否裁剪（以中心点）</param> 
		/// <returns></returns> 
		public static Image GetThumbnail (Image imgSource, int newWidth, int newHeight, bool isCut = false)
		{ 			
			int sWidth = imgSource.Width; // 原图片宽度 
			int sHeight = imgSource.Height; // 原图片高度 

			double wScale = (double)sWidth / newWidth; // 宽比例 
			double hScale = (double)sHeight / newHeight; // 高比例 

			double scale = wScale < hScale ? wScale : hScale; 

			// 如果是原图缩略
			if (!isCut) { 
				//原图比例小于所要截取的矩形框，那么保留原图 
				if (scale <= 1)
					return imgSource; 

				return imgSource.GetThumbnailImage (newWidth, newHeight, null, IntPtr.Zero);
			}

			//裁切方式				
			int rWidth = (int)Math.Floor (sWidth / scale); // 等比例缩放后的宽度 
			int rHeight = (int)Math.Floor (sHeight / scale); // 等比例缩放后的高度 

			Bitmap tmp_img = new Bitmap (rWidth, rHeight); 

			using (Graphics tGraphic = Graphics.FromImage (tmp_img)) { 
				tGraphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; /* new way */ 
				Rectangle rect = new Rectangle (0, 0, rWidth, rHeight); 
				Rectangle rectSrc = new Rectangle (0, 0, sWidth, sHeight); 
				tGraphic.DrawImage (imgSource, rect, rectSrc, GraphicsUnit.Pixel); 
			} 

			int xMove = (rWidth - newWidth) / 2; // 向右偏移（裁剪） 
			int yMove = (rHeight - newHeight) / 2; // 向下偏移（裁剪） 

			Bitmap final_img = new Bitmap (newWidth, newHeight); 

			using (Graphics fGraphic = Graphics.FromImage (final_img)) {

				fGraphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; /* new way */ 
				Rectangle rect1 = new Rectangle (0, 0, newWidth, newHeight); 
				Rectangle rectSrc1 = new Rectangle (xMove, yMove, newWidth, newHeight); 
				fGraphic.DrawImage (tmp_img, rect1, rectSrc1, GraphicsUnit.Pixel); 
			} 

			tmp_img.Dispose (); 

			return final_img; 
		}

		public static void GenThumbnail (string pathFrom, string pathTo, int maxWH)
		{
			using (Image src = Image.FromFile (pathFrom), 
			       img = FitSize (src, maxWH, maxWH)) {
				img.Save (pathTo, src.RawFormat);
			}
		}

		/// <summary>
		/// 将图形等比例适应到指定大小内, 
		/// 适用于生成128像素以下的小图形
		/// </summary>
		/// <param name="srcImg"></param>
		/// <param name="maxWidth"></param>
		/// <param name="maxHeight"></param>
		/// <returns></returns>
		public static Image FitSize (Image srcImg, int maxWidth, int maxHeight)
		{
			Size n = ShrinkSize (srcImg.Size, maxWidth, maxHeight);
			return srcImg.GetThumbnailImage (n.Width, n.Height, null, IntPtr.Zero);
		}


		/// <summary>
		/// 将图形等比例适应到指定大小内
		/// 适用于生成任意大小的清晰图形
		/// </summary>
		/// <param name="srcImg"></param>
		/// <param name="maxWidth"></param>
		/// <param name="maxHeight"></param>
		/// <returns></returns>
		public static Image FitSizeHigh (Image srcImg, int maxWidth, int maxHeight)
		{
			Size n = ShrinkSize (srcImg.Size, maxWidth, maxHeight);
			return new Bitmap (srcImg, n);


			/*int w = n.Width, h = n.Height ;

			Bitmap bitmap = new Bitmap(w ,h);
			using (Graphics g = Graphics.FromImage(bitmap)) {
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode  =  PixelOffsetMode.HighQuality;
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.Clear(Color.Transparent);
				g.DrawImage(srcImg, new Rectangle(0, 0, w, h) , new Rectangle(0, 0, srcImg.Width, srcImg.Height), GraphicsUnit.Pixel);
			}
            
			return bitmap as Image;*/
		}

		/// <summary>
		/// Uploads the image then shrink and save it.
		/// </summary>
		/// <param name="postedFile">Posted file.</param>
		/// <param name="directory">Directory.</param>
		/// <param name="fileName">File name.</param>
		/// <param name="maxWidth">Max width.</param>
		/// <param name="maxHeight">Max height.</param>
		public static void UploadShrinkAndSave (HttpPostedFile postedFile, string directory, string fileName, int maxWidth, int maxHeight)
		{
			if (postedFile != null && postedFile.ContentLength > 0) {
				if (!postedFile.ContentType.StartsWith ("image", StringComparison.OrdinalIgnoreCase))
					return;

				var uploadDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, directory);
				if (!Directory.Exists (uploadDirectory)) {
					Directory.CreateDirectory (uploadDirectory);
				}

				string saveName = string.IsNullOrEmpty (fileName) ? Path.GetFileName (postedFile.FileName) : fileName;
				string savePath = Path.Combine (uploadDirectory, saveName);

				using (
					Image originalImage = Image.FromStream (postedFile.InputStream), 
					thumb = new Bitmap (originalImage, new Size(maxWidth, maxHeight))
					//thumb = FitSizeHigh (originalImage, maxWidth, maxHeight)
				) {
					thumb.Save (savePath); 
				}
			}
		}

		public static void UploadFitAndSave (HttpPostedFile postedFile, string directory, string fileName, int maxWidth, int maxHeight)
		{
			if (postedFile != null && postedFile.ContentLength > 0) {
				if (!postedFile.ContentType.StartsWith ("image", StringComparison.OrdinalIgnoreCase))
					return;

				var uploadDirectory = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, directory);
				if (!Directory.Exists (uploadDirectory)) {
					Directory.CreateDirectory (uploadDirectory);
				}

				string saveName = string.IsNullOrEmpty (fileName) ? Path.GetFileName (postedFile.FileName) : fileName;
				string savePath = Path.Combine (uploadDirectory, saveName);

				using (
					Image originalImage = Image.FromStream (postedFile.InputStream), 
					thumb = FitSizeHigh (originalImage, maxWidth, maxHeight)
				) {
					thumb.Save (savePath); 
				}
			}
		}

		public void ImageChange (string sourcePath, string savePath, int maxWidth, int maxHeight)
		{
			using (Image srcImg = Image.FromFile (sourcePath)) {

				Size n = ShrinkSize (srcImg.Size, maxWidth, maxHeight);
				int w = n.Width, h = n.Height;

				using (Bitmap bitmap = new Bitmap (w, h)) {

					using (Graphics g = Graphics.FromImage (bitmap)) {

						g.InterpolationMode = InterpolationMode.HighQualityBicubic; 
						g.PixelOffsetMode = PixelOffsetMode.HighQuality;
						g.SmoothingMode = SmoothingMode.HighQuality;
						g.Clear (Color.Transparent);
						g.DrawImage (srcImg, new Rectangle (0, 0, w, h), new Rectangle (0, 0, srcImg.Width, srcImg.Height), GraphicsUnit.Pixel);
						bitmap.Save (savePath, ImageFormat.Jpeg);
					}
				}

			}
		}


		public static void GenThumbnailHigh (string pathFrom, string svPath, int maxWH)
		{
			Image img, bitmap;
			img = Image.FromFile (pathFrom);
			bitmap = FitSizeHigh (img, maxWH, maxWH);

			//关键质量控制              
			ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders ();

			//获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff              
			ImageCodecInfo ici = null;              
			foreach (ImageCodecInfo i in icis) { 
				if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif") {                      
					ici = i;
				}
			}

			EncoderParameters ep = new EncoderParameters (1);              
			ep.Param [0] = new EncoderParameter (System.Drawing.Imaging.Encoder.Quality, 100);//最高质量~100 

			try {
				bitmap.Save (svPath, ici, ep);
			} catch (Exception e) {
				throw new Exception (e.Message);
			} finally {
				img.Dispose ();
				bitmap.Dispose ();
			}
		}
		
	}
}
