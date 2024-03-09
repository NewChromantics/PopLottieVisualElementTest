using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


//  this is actually the bodymovin spec
namespace PopLottie
{
	//	spec is readable here
	//	https://lottiefiles.github.io/lottie-docs/breakdown/bouncy_ball/

	[Serializable] public struct AssetMeta
	{
	}
	
	
	[Serializable] public struct KeyframeFloats
	{
		public float[]		i;
		public float		t;	//	time
		public float[]		s;	//	start value
		public float[]		e;	//	end value
	}
	
	//	https://lottiefiles.github.io/lottie-docs/playground/json_editor/
	[Serializable] public struct AnimatedVector
	{
		public int				a;
		public bool				Animated => a!=0;
		
		//	animated
		//public KeyframeFloats[]	k;	//	frames
		//	non animated
		public float[]		k;	//	frames
		
		public float			GetValue(float Time)
		{
			return k[0];
		}
	}

	
	[Serializable] public struct Keyframe2
	{
		public Vector2		i;
		public float		t;	//	time
		public float[]		s;	//	start value
		public float[]		e;	//	end value
	}
	
	//	https://lottiefiles.github.io/lottie-docs/playground/json_editor/
	[Serializable] public struct AnimatedNumber
	{
		public int			a;
		public bool			Animated => a!=0;
		public float[]		k;	//	frames
		
		public float		GetValue(double Time)
		{
			if ( k.Length < 1 )
				return 0;
			return k[0];
		}
	}


	//	https://lottiefiles.github.io/lottie-docs/playground/json_editor/
	[Serializable] public struct AnimatedPosition
	{
		public int			a;
		public bool			Animated => a!=0;
		public int			ix;	//	property index

		//	animated
		//public Keyframe2[]	k;	//	frames
		//	non animated
		public float[]		k;	//	frames
		
		public Vector2		GetPosition(float Time)
		{
			return new Vector2( k[0], k[1] );
		}
	}
	
	
	
	[Serializable] public struct Bezier
	{
		public List<float[]>	i;	//	in-tangents
		public List<float[]>	o;	//	out-tangents
		public List<float[]>	v;	//	vertexes
		public bool		c;
		public bool		Closed => c;
	}
	
	[Serializable] public struct AnimatedBezier
	{
		public int			a;
		public bool			Animated => a!=0;
		//	if not animated, k==Vector3
		public Bezier		k;	//	frames
		public int			ix;	//	property index
		
		public Bezier		GetBezier(float Time)
		{
			return k;
		}
	}
	
	[Serializable] public struct AnimatedColour
	{
		public int			a;
		public bool			Animated => a!=0;
		//	if not animated, k==Vector3
		public float[]		k;	//	4 elements 0..1
		public int			ix;	//	property index
		
		public Color		GetColour(double Time)
		{
			if ( k.Length < 4 )
				return Color.magenta;
			return new Color(k[0],k[1],k[2],k[3]);
		}
	}
	
	
	[Serializable] public struct TransformMeta
	{
	/*
		public float[]	a;	//	anchor point
		public float[]	s;	//	scale factor, 100=no scaling
		public float	r;	//	rotation in degrees clockwise
		public float	sk;	//	skew angle degrees
		public float	sa;	//	Direction at which skew is applied, in degrees (0 skews along the X axis, 90 along the Y axis)
		*/
		public AnimatedVector	s;	//	scale factor, 100=no scaling
		public AnimatedPosition	a;	//	anchor point
		public AnimatedPosition	p;	//	position/translation
		public AnimatedNumber	r;	//	rotation in degrees clockwise
		public AnimatedNumber	o;	//	opacity 0...100
	}
	
	public enum ShapeType
	{
		Fill,
		Stroke,
		Transform,
		Group,
		Path,
		Ellipse,
	}
	
	
	[Serializable] public struct Shape
	{
		//	path
		public AnimatedBezier	ks;	//	bezier for path
		public AnimatedBezier	Path_Bezier => ks;
		
		//	fill & stroke
		public AnimatedColour	c;	//	colour
		public AnimatedColour	Fill_Colour => c;
		public AnimatedColour	Stroke_Colour => c;
		public int				r;	//	fill rule
		public AnimatedNumber	o;	//	opacity? 
		public AnimatedNumber	w;	//	width
		public AnimatedNumber	Stroke_Width => w;
	
		//	ellipse
		public AnimatedVector	s;	//	
		public AnimatedVector	Ellipse_Size => s;	
		public AnimatedPosition	Ellipse_Center => p;	
	
		//	transform
		public AnimatedPosition	p;	//	translation
		public AnimatedPosition	a;	//	anchor
		//public AnimatedVector	s;	//	scale
		//public AnimatedVector	r;	//	rotation
	
		public int			ind;//	?
		public int			np;		//	number of properties
		public int			cix;	//	property index
		public int			ix;		//	property index
		public int			bm;		//	blend mode
		public String		nm;		// = "Lottie File"
		public String		Name => nm ?? "Unnamed";
		public String		mn;
		public String		MatchName => mn;
		public int			hd;
		public bool			Hidden => hd == 1;
		public bool			Visible => !Hidden;
		public String		ty;	
		public ShapeType	Type => ty switch
		{
			"gr" => ShapeType.Group,
			"sh" => ShapeType.Path,
			"fl" => ShapeType.Fill,
			"tr" => ShapeType.Transform,
			"st" => ShapeType.Stroke,
			"el" => ShapeType.Ellipse,
			_ => throw new Exception($"Unknown type {ty}")
		};
		public Shape[]		it;	//	children
		public Shape[]		Children => it;
	}
	
	[Serializable]
	public struct LayerMeta	//	shape layer
	{
		public bool		IsVisible(double Time)
		{
			if ( Time < FirstKeyframe )
				return false;
			if ( Time > LastKeyframe )
				return false;
			return true;
		}
	
		public double	ip;
		public double	FirstKeyframe => ip;	//	visible after this
		public double	op;	//	= 10
		public double	LastKeyframe => op;		//	invisible after this (time?)
		
		public String	nm;// = "Lottie File"
		public String	Name => nm ?? "Unnamed";

		public String	refId;
		public String	ResourceId => refId ?? "";
		public int		ind;
		public int		LayerId => ind;
		public double	st;
		public double	StartTime => st;

		public int		ddd;	//	something to do with winding
		public int		parent;
		public int		ty;
		public int		sr;
		public TransformMeta	ks;
		public TransformMeta	Transform=>ks;
		public int		ao;
		public bool		AutoOrient => ao != 0;
		public Shape[]	shapes;
		public int		bm;
		public int		BlendMode => bm;
	}
	
	[Serializable]
	public struct MarkerMeta
	{/*
		public var cm : String
		public var id : String		{ return Name }
		public var Name : String	{	return cm	}
		public var tm : Int
		public var Frame : Int	{	return tm	}
		public var dr : Int
		*/
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
			CurrentFrame = Frame;
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
			var Time = CurrentFrame;
			var width = ContentRect.width;
			var height = ContentRect.height;
			
			int PathsDrawn = 0;
			int EllipsesDrawn = 0;
			
			Painter.fillColor = Color.blue;

			void RenderGroup(Shape Group)
			{
				//	run through sub shapes
				var Children = Group.Children;
			
				Painter.strokeColor = Color.green;
				Vector3 Transform;
				bool Filled = false;
				bool Stroked = false;
				
				foreach ( var Child in Children )
				{
					if ( Child.Type == ShapeType.Fill )
					{
						Painter.fillColor = Child.Fill_Colour.GetColour(Time);
						Filled = true;
					}
					if ( Child.Type == ShapeType.Stroke )
					{
						Painter.strokeColor = Child.Stroke_Colour.GetColour(Time);
						Painter.lineWidth = Child.Stroke_Width.GetValue(Time);
						Stroked = true;
					}
					if ( Child.Type == ShapeType.Path )
					{
						var Bezier = Child.Path_Bezier.GetBezier(Time);
		
						Painter.BeginPath();
						//Painter.Arc(new Vector2(width * 0.3f, height * 0.3f), width * 0.5f, 0.0f, 360.0f);
						//Painter.BezierCurveTo();
						if ( Bezier.Closed )
							Painter.ClosePath();
						PathsDrawn++;
					}
					if ( Child.Type == ShapeType.Ellipse )
					{
						var EllipseSize = Child.Ellipse_Size.GetValue(Time);
						var EllipseCenter = Child.Ellipse_Center.GetPosition(Time);
		
						var Radius = EllipseSize;
		
						Painter.BeginPath();
						Painter.Arc( EllipseCenter, Radius, 0, 360 );
						Painter.ClosePath();
						EllipsesDrawn++;
					}
					if ( Child.Type == ShapeType.Transform )
					{
						//	do transform stuff
						Transform = Vector3.one;
					}
				}
				
				Painter.lineWidth = 10.0f;
				Painter.lineCap = LineCap.Butt;
				if ( Stroked )
					Painter.Stroke();
				if ( Filled )
					Painter.Fill();
				if ( !Filled && !Stroked )
					Debug.Log($"Layer not filled or stroked");
			}
		
			foreach ( var Layer in lottie.layers )
			{
				if ( !Layer.IsVisible(Time) )
					continue;
				
				//	render the shape
				foreach ( var Shape in Layer.shapes )
				{
					if ( Shape.Type == ShapeType.Group )
					{
						RenderGroup(Shape);
					}
					else
					{
						Debug.Log($"Not a group...");
					}
				}
			}
		/*
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
			*/
			
			Debug.Log($"Paths {PathsDrawn} Ellipses {EllipsesDrawn}");
		}
		
	}
	
}

