/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2014/12/26
 * Time: 11:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Lyu.Img
{
	/// <summary>
	/// Description of Thumbnailer.
	/// </summary>
	public class Thumbnailer
	{
		/// <summary> 
		/// 生成缩略图 
		/// </summary> 
		/// <param name="imgSource">原图片</param> 
		/// <param name="newWidth">缩略图宽度</param> 
		/// <param name="newHeight">缩略图高度</param> 
		/// <param name="isCut">是否裁剪（以中心点）</param> 
		/// <returns></returns> 
		public static Image GetThumbnail(Image imgSource, int newWidth, int newHeight, bool isCut=false) 
		{ 			
			int sWidth = imgSource.Width; // 原图片宽度 
			int sHeight = imgSource.Height; // 原图片高度 
			
			double wScale = (double)sWidth / newWidth; // 宽比例 
			double hScale = (double)sHeight / newHeight; // 高比例 
			
			double scale = wScale < hScale ? wScale : hScale; 
			
			try { 
				// 如果是原图缩略
				if (!isCut) { 
					//原图比例小于所要截取的矩形框，那么保留原图 
					if(scale <= 1)
						return imgSource; 
					
					return imgSource.GetThumbnailImage(newWidth , newHeight , null, IntPtr.Zero );
				}
			
				//裁切方式				
				int rWidth = (int)Math.Floor(sWidth / scale); // 等比例缩放后的宽度 
				int rHeight = (int)Math.Floor(sHeight / scale); // 等比例缩放后的高度 
				
				Bitmap thumbnail = new Bitmap(rWidth, rHeight); 
				
				using (Graphics tGraphic = Graphics.FromImage(thumbnail)) { 
					tGraphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; /* new way */ 
					Rectangle rect = new Rectangle(0, 0, rWidth, rHeight); 
					Rectangle rectSrc = new Rectangle(0, 0, sWidth, sHeight); 
					tGraphic.DrawImage(imgSource, rect, rectSrc, GraphicsUnit.Pixel); 
				} 
				
				int xMove = (rWidth - newWidth) / 2; // 向右偏移（裁剪） 
				int yMove = (rHeight - newHeight) / 2; // 向下偏移（裁剪） 
					
				Bitmap final_image = new Bitmap(newWidth, newHeight); 
					
				using (Graphics fGraphic = Graphics.FromImage(final_image)) {
					
					fGraphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; /* new way */ 
					Rectangle rect1 = new Rectangle(0, 0, newWidth, newHeight); 
					Rectangle rectSrc1 = new Rectangle(xMove, yMove, newWidth, newHeight); 
					fGraphic.DrawImage(thumbnail, rect1, rectSrc1, GraphicsUnit.Pixel); 
				} 
					
				thumbnail.Dispose(); 
					
				return final_image; 
			} 
			catch (Exception e) { 
				//随便回一个空白图算了
				return new Bitmap(newWidth, newHeight); 
			} 
		}
		
		/// <summary>
		/// 将图形等比例适应到指定大小内
		/// </summary>
		/// <param name="srcImg"></param>
		/// <param name="maxWidth"></param>
		/// <param name="maxHeight"></param>
		/// <returns></returns>
		public static Image FitSize(Image srcImg , int maxWidth , int maxHeight)
		{
			int sw = srcImg.Width; // 原图片宽度 
			int sh = srcImg.Height; // 原图片高度 
			int w , h ;//
			if (sw <= maxWidth  && sh <= maxHeight) {
				w = sw ;
				h = sh;
			}
			else if (sw > sh) {
				w = maxWidth ;
				h = (int) w * sh/sw ;
			}
			else {
				h = maxHeight ;
				w = (int) h * sw/sh;
			}
			return srcImg.GetThumbnailImage( w , h , null, IntPtr.Zero );
		}
		
	}
}
