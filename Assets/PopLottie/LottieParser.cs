using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


//  this is actually the bodymovin spec
namespace PopLottie
{
	[Serializable]
	public struct AssetMeta
	{
	}
	
	[Serializable]
	public struct LayerMeta
	{
	}
	
	[Serializable]
	public struct MarkerMeta
	{
	}
	
		
	[Serializable]
	public struct Root
	{
		public string	v;	//"5.9.2"
		public double	fr;
		public double	ip;
		public double	FirstKeyframe => ip;
		public double	op;	//	= 10
		public double	LastKeyframe => op;
		public int		w;//: = 100
		public int		h;//: = 100
		public String	nm;// = "Lottie File"
		public String	Name => nm ?? "Unnamed";
		public int		ddd;	// = 0	//	not sure what this is, but when it's 3 "things are reversed"
			
		public AssetMeta[]	assets;
		public LayerMeta[]	layers;
		public MarkerMeta[]	markers;

		public AssetMeta[]	Assets => assets ?? Array.Empty<AssetMeta>();
		public LayerMeta[]	Layers => layers ?? Array.Empty<LayerMeta>();
		public MarkerMeta[]	Markers => markers ?? Array.Empty<MarkerMeta>();
	}
	
	public class Animation : IDisposable
	{
		Root	lottie;
		
		public Animation(string FileContents)
		{
			lottie = JsonUtility.FromJson<Root>(FileContents);
		}
		
		public int CurrentFrame = 0;
		public int TotalFramesCount = 100;
		public float DurationSeconds => GetDurationSeconds();

		public float GetDurationSeconds()
		{
			return (float)(lottie.LastKeyframe - lottie.FirstKeyframe);
		}
		public void DrawOneFrame(int Frame)
		{
			
		}
		
		public void Play()
		{
			
		}
		
		public void Stop()
		{
		}
		
		public void Dispose()
		{
			
		}
		
		public void Render(Painter2D Painter,Rect ContentRect)
		{
			float width = ContentRect.width;
			float height = ContentRect.height;

			Painter.lineWidth = 10.0f;
			Painter.lineCap = LineCap.Butt;

			// Draw the track
			//Painter.strokeColor = m_TrackColor;
			Painter.BeginPath();
			Painter.Arc(new Vector2(width * 0.5f, height * 0.5f), width * 0.5f, 0.0f, 360.0f);
			Painter.Stroke();

			// Draw the progress
			//Painter.strokeColor = m_ProgressColor;
			Painter.BeginPath();
			float progress = 20; 
			Painter.Arc(new Vector2(width * 0.5f, height * 0.5f), width * 0.5f, -90.0f, 360.0f * (progress / 100.0f) - 90.0f);
			Painter.Stroke();
		}
		
	}
	
}

