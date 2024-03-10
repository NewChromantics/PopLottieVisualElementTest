using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

//	we need to dynamically change the structure as we parse, so the built in json parser wont cut it
//	com.unity.nuget.newtonsoft-json
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Object = UnityEngine.Object;


//  this is actually the bodymovin spec
namespace PopLottie
{
	//	spec is readable here
	//	https://lottiefiles.github.io/lottie-docs/breakdown/bouncy_ball/

	[Serializable] public struct AssetMeta
	{
	}
	
	[Serializable] public struct Frame_Floats
	{
		public float[]		k;
		public Float2		o;
		public float		t;	//	time
		public float[]		s;	//	start value
		public float[]		e;	//	end value
	
	}
	
	public struct Keyframed_Floats
	{
		public int					a;
		public int					ix;
		public List<Frame_Floats>	Frames;
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
		
		//[JsonConverter(typeof(Keyframed_FloatsConvertor))]
		//public Keyframed_Floats	k;	//	frames
		public float[]			k;
		
		public float			GetValue(float Time)
		{
			if ( k.Length == 0 )
				return 123;
			return k[0];
		}
	}

	
	[Serializable] public struct Keyframe2
	{
		public Vector2		i;
		public Vector2		o;
		public float		t;	//	time
		public float[]		s;	//	start value
		public float[]		e;	//	end value
	}
	
	[Serializable] public struct Float2
	{
		public float[]		x;
		public float[]		y;
	}

	
	
	//	make this generic too
	[Serializable] public struct Frame_Vector2
	{
		public Float2		i;
		public Float2		o;
		public float		t;	//	time
		public float[]		s;	//	start value
		public float[]		e;	//	end value
		
		public Float2		Value => i;
	}
	
	[Serializable] public struct Frame_Float
	{
		public float		i;
		public float		o;
		public float		t;	//	time
		public float[]		s;	//	start value
		public float[]		e;	//	end value
		
		public float		Value => i;
	}
	
	
	class Keyframed_Vector2Convertor : JsonConverter<Keyframed_Vector2>
	{
		public override void WriteJson(JsonWriter writer, Keyframed_Vector2 value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override Keyframed_Vector2 ReadJson(JsonReader reader, Type objectType, Keyframed_Vector2 existingValue, bool hasExistingValue,JsonSerializer serializer)
		{
			if ( reader.TokenType == JsonToken.StartObject )
			{
				var ThisObject = JObject.Load(reader);
				var SingleFrame = ThisObject.ToObject<Frame_Vector2>(serializer);
				existingValue.AddFrame(SingleFrame);
			}
			else if ( reader.TokenType == JsonToken.StartArray )
			{
				var ThisArray = JArray.Load(reader);
				foreach ( var Frame in ThisArray )
				{
					var FrameReader = new JTokenReader(Frame);
					var FrameObject = JObject.Load(FrameReader);
					var SingleFrame = FrameObject.ToObject<Frame_Vector2>(serializer);
					existingValue.AddFrame(SingleFrame);
				}
			}
			else 
			{
				//existingValue.ReadAnimatedOrNotAnimated(reader);
				Debug.LogWarning($"Decoding Keyframed_Vector2 unhandled token type {reader.TokenType}");
			}
			return existingValue;
		}


	}
	
	class Keyframed_FloatConvertor : JsonConverter<Keyframed_Float>
	{
		public override void WriteJson(JsonWriter writer, Keyframed_Float value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override Keyframed_Float ReadJson(JsonReader reader, Type objectType, Keyframed_Float existingValue, bool hasExistingValue,JsonSerializer serializer)
		{
			if ( reader.TokenType == JsonToken.StartObject )
			{
				var ThisObject = JObject.Load(reader);
				var SingleFrame = ThisObject.ToObject<Frame_Float>(serializer);
				existingValue.AddFrame(SingleFrame);
			}
			else if ( reader.TokenType == JsonToken.StartArray )
			{
				var ThisArray = JArray.Load(reader);
				foreach ( var Frame in ThisArray )
				{
					var FrameReader = new JTokenReader(Frame);
					var FrameObject = JObject.Load(FrameReader);
					var SingleFrame = FrameObject.ToObject<Frame_Float>(serializer);
					existingValue.AddFrame(SingleFrame);
				}
			}
			else if ( reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float )
			{
				var Value = reader.Value;
				var Number = (reader.TokenType == JsonToken.Integer) ? (long)Value : (float)Value;
				var Frame = new Frame_Float();
				Frame.i = Number;
				existingValue.AddFrame(Frame);
			}
			else 
			{
				//existingValue.ReadAnimatedOrNotAnimated(reader);
				Debug.LogWarning($"Decoding Frame_Float unhandled token type {reader.TokenType}");
			}
			return existingValue;
		}
	}
	
	//	make this generic
	[JsonConverter(typeof(Keyframed_Vector2Convertor))]
	public struct Keyframed_Vector2
	{
		//public int					a;
		//public int					ix;
		
		List<Frame_Vector2>		Frames;
		
		public void	AddFrame(Frame_Vector2 Frame)
		{
			Frames = Frames ?? new();
			Frames.Add(Frame);
		}
		
		public Vector2 GetValue(float Time)
		{
			if ( Frames == null || Frames.Count == 0 )
				return new Vector2(1,1);
			var xy = Frames[0].i;
			return new Vector2(xy.x[0],xy.y[0]);
		}
	}
	
		//	make this generic
	[JsonConverter(typeof(Keyframed_FloatConvertor))]
	public struct Keyframed_Float
	{
		List<Frame_Float>		Frames;
		
		public void	AddFrame(Frame_Float Frame)
		{
			Frames = Frames ?? new();
			Frames.Add(Frame);
		}
		
		public float GetValue(float Time)
		{
			if ( Frames == null || Frames.Count == 0 )
				return 1;
			return Frames[0].Value;
		}
	}
	
	//	https://lottiefiles.github.io/lottie-docs/playground/json_editor/
	[Serializable] public struct AnimatedNumber
	{
		public int			a;
		public bool			Animated => a!=0;
		
		//	something somewhere has xy, which might be time=x value=y
		//public Keyframed_Vector2	k;	//	frames
		public Keyframed_Float	k;	//	frames
		
		public float		GetValue(float Time)
		{
			return k.GetValue(Time);
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
			if ( k == null )
				return Vector2.zero;
				
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

		public ControlPoint[]	GetControlPoints()
		{
			var Points = new ControlPoint[v.Count];
			for ( var Index=0;	Index<v.Count;	Index++ )
			{
				Points[Index].Position.x = v[Index][0];
				Points[Index].Position.y = v[Index][1];
				Points[Index].InTangent.x = i[Index][0];
				Points[Index].InTangent.y = i[Index][1];
				Points[Index].OutTangent.x = o[Index][0];
				Points[Index].OutTangent.y = o[Index][1];
			}
			return Points;
		}
		
		public struct ControlPoint
		{
			public Vector2	InTangent;
			public Vector2	OutTangent;
			public Vector2	Position;
		}
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
		
		public Color		GetColour(float Time)
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
		//public AnimatedVector	s;	//	scale factor, 100=no scaling
		public AnimatedPosition	a;	//	anchor point
		public AnimatedPosition	p;	//	position/translation
		//public AnimatedNumber	r;	//	rotation in degrees clockwise
		public AnimatedNumber	o;	//	opacity 0...100
		
		public Transformer		GetTransformer(float Time)
		{
			var Anchor = a.GetPosition(Time);
			var Position = p.GetPosition(Time);
			float Scale = 1;
			return new Transformer(Position,Anchor,Scale);
		}
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
	
	public class ShapeSpecificMeta
	{
	}

	public class ShapeConvertor : JsonConverter<ShapeWrapper>
	{
		public override void WriteJson(JsonWriter writer, ShapeWrapper value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
		
		public override ShapeWrapper ReadJson(JsonReader reader, Type objectType, ShapeWrapper existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var ShapeObject = JObject.Load(reader);
			var ShapeBase = new Shape();
			ShapeBase.ty = ShapeObject["ty"].Value<String>();
			
			//	now based on type, serialise
			if ( ShapeBase.Type == ShapeType.Ellipse )
			{
				ShapeBase = ShapeObject.ToObject<ShapeEllipse>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.Fill )
			{
				ShapeBase = ShapeObject.ToObject<ShapeFillAndStroke>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.Stroke )
			{
				ShapeBase = ShapeObject.ToObject<ShapeFillAndStroke>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.Transform )
			{
				ShapeBase = ShapeObject.ToObject<ShapeTransform>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.Group )
			{
				ShapeBase = ShapeObject.ToObject<ShapeGroup>(serializer);
			}
			else if ( ShapeBase.Type == ShapeType.Path )
			{
				ShapeBase = ShapeObject.ToObject<ShapePath>(serializer);
			}

			existingValue.TheShape = ShapeBase;
			return existingValue;
		}
	}
	
	[JsonConverter(typeof(ShapeConvertor))]
	[Serializable] public struct ShapeWrapper 
	{
		public Shape		TheShape;
		public ShapeType	Type => TheShape.Type; 
	}

	[Serializable] public class Shape 
	{
		public int			ind;//	?
		public int			np;		//	number of properties
		public int			cix;	//	property index
		public int			ix;		//	property index
		public int			bm;		//	blend mode
		public String		nm;		// = "Lottie File"
		public String		Name => nm ?? "Unnamed";
		public String		mn;
		public String		MatchName => mn;
		public bool			hd;	//	i think sometimes this might an int. Newtonsoft is very strict with types
		public bool			Hidden => hd;
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
	}
	
	[Serializable] public class ShapePath : Shape
	{
		public AnimatedBezier	ks;	//	bezier for path
		public AnimatedBezier	Path_Bezier => ks;
	}
		
				
	[Serializable] public class ShapeFillAndStroke : Shape 
	{
		public AnimatedColour	c;	//	colour
		public AnimatedColour	Fill_Colour => c;
		public AnimatedColour	Stroke_Colour => c;
		//public int				r;	//	fill rule
		public AnimatedNumber	o;	//	opacity? 
		public AnimatedNumber	w;	//	width
		public AnimatedNumber	Stroke_Width => w;
		
		public float			GetWidth(float Time)
		{
			var Value = w.GetValue(Time);
			//	gr: it kinda looks like unity's width is radius, and lotties is diameter, as it's consistently a bit thick
			Value *= 0.8f;
			return Value;
		}
		public Color			GetColour(float Time)
		{
			return c.GetColour(Time);
		}
	}
		
		
	[Serializable] public class ShapeTransform : Shape 
	{
		//	transform
		public AnimatedPosition	p;	//	translation
		public AnimatedPosition	a;	//	anchor
		
		//	gr: not parsing as mix of animated & not
		//public AnimatedVector	s;	//	scale
		//public AnimatedVector	r;	//	rotation
		
		public Transformer	GetTransformer(float Time)
		{
			var Anchor = a.GetPosition(Time);
			var Position = p.GetPosition(Time);
			var Scale = 1;
			return new Transformer( Position, Anchor, Scale);
		}
	}
	
	
	[Serializable] public class ShapeEllipse : Shape 
	{
		public AnimatedVector	s;
		public AnimatedPosition	p;
		public AnimatedVector	Size => s;	
		public AnimatedPosition	Center => p;	
		
	}
	
	public struct ShapeStyle
	{
		public Color?	FillColour;
		public Color?	StrokeColour;
		public float?	StrokeWidth;
		public bool		IsStroked => StrokeColour.HasValue;
		public bool		IsFilled => FillColour.HasValue;
	}

	//	also for layers, but can't call this Transform
	public struct Transformer
	{
		Vector2?	Scale2;
		float?		Scale1;
		
		Vector2		Translation;
		Vector2		Anchor;
		
		public Transformer(Vector2 Translation,Vector2 Anchor,float Scale=1)
		{
			this.Translation = Translation;
			this.Anchor = Anchor;
			Scale1 = Scale;
			Scale2 = null;
		}
		public Transformer(Vector2 Translation,Vector2 Anchor,Vector2 Scale)
		{
			this.Translation = Translation;
			this.Anchor = Anchor;
			Scale1 = null;
			Scale2 = Scale;
		}
		
		Vector2		GetScale()
		{
			if ( Scale2 is Vector2 s2 )
				return s2;
			if ( Scale1 is float s1 )
				return new Vector2(s1,s1);
			return Vector2.one;
		} 
		
		public Vector2	LocalToWorld(Vector2 LocalPosition)
		{
			//	gr: this needs to use anchor
			LocalPosition -= Anchor;
			LocalPosition *= GetScale();
			//	rotate here
			LocalPosition += Anchor;
			LocalPosition += Translation;
			
			return LocalPosition;
		}
		
		public float	LocalToWorld(float LocalSize)
		{
			LocalSize *= GetScale().x;
			return LocalSize;
		}
		
		//	gr: might all be simpler if we generate a 2D matrix...
		public Transformer	Multiply(Transformer ChildTransform)
		{
			var NewTransform = ChildTransform;
			NewTransform.Translation += this.Translation;
			NewTransform.Scale2 = ChildTransform.GetScale() * GetScale();
			return NewTransform;
		}
	}

	[Serializable] public class ShapeGroup: Shape 
	{
		public List<ShapeWrapper>		it;	//	children
		public IEnumerable<Shape>		Children => it.Select( sw => sw.TheShape );
		
		Shape				GetChild(ShapeType MatchType)
		{
			//	handle multiple instances
			foreach (var s in it)//Children)
			{
				if ( s.Type == MatchType )
					return s.TheShape;
			}
			return null;
		}
		public Transformer		GetTransformer(float Time)
		{
			var Transform = GetChild(ShapeType.Transform) as ShapeTransform;
			if ( Transform == null )
				return new Transformer();
			return Transform.GetTransformer(Time);
		}
		
		public ShapeStyle		GetShapeStyle(float Time)
		{
			var Fill = GetChild(ShapeType.Fill) as ShapeFillAndStroke;
			var Stroke = GetChild(ShapeType.Stroke) as ShapeFillAndStroke;
			var Style = new ShapeStyle();
			if ( Fill != null )
			{
				Style.FillColour = Fill.GetColour(Time);
			}
			if ( Stroke != null )
			{
				Style.StrokeColour = Stroke.GetColour(Time);
				Style.StrokeWidth = Stroke.GetWidth(Time);
			}
			return Style;
		}
	}
	

	
	[Serializable]
	public struct LayerMeta	//	shape layer
	{
		public bool		IsVisible(float Time)
		{
			if ( Time < FirstKeyframe )
				return false;
			if ( Time > LastKeyframe )
				return false;
			/*
			if ( Time < StartTime )
				return false;
				*/
			return true;
		}
	
		public float				ip;
		public float				FirstKeyframe => ip;	//	visible after this
		public float				op;	//	= 10
		public float				LastKeyframe => op;		//	invisible after this (time?)
		
		public String				nm;// = "Lottie File"
		public String				Name => nm ?? "Unnamed";

		public String				refId;
		public String				ResourceId => refId ?? "";
		public int					ind;
		public int					LayerId => ind;
		public float				st;
		public double				StartTime => st;

		public int					ddd;	//	something to do with winding
		public int					parent;
		public int					ty;
		public int					sr;
		public TransformMeta		ks;
		public TransformMeta		Transform=>ks;
		public int					ao;
		public bool					AutoOrient => ao != 0;
		public ShapeWrapper[]		shapes;
		public IEnumerable<Shape>	Children => shapes.Select( sw => sw.TheShape );
		public int					bm;
		public int					BlendMode => bm;
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
		public float	fr;
		public float	FrameRate => fr;
		public float	ip;
		public float	FirstKeyframe => ip;
		public float	op;	//	= 10
		public float	LastKeyframe => op;
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
			//	gr: can't use built in, as the structure changes depending on contents, and end up with clashing types
			//lottie = JsonUtility.FromJson<Root>(FileContents);
			//	can't use the default deserialiser, because for some reason, the parser misses out parsing
			//	[ {}, {} ] 
			//lottie = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(FileContents);
			
			//	we CAN parse with generic parser!
			var Parsed = JObject.Parse(FileContents);
			
			JsonSerializer serializer = new JsonSerializer();
			
			
			lottie = (Root)serializer.Deserialize(new JTokenReader(Parsed), typeof(Root));
			Debug.Log($"Decoded lottie ok x{lottie.layers.Length} layers");
		}
		
		public int CurrentFrame = 0;
		public int TotalFramesCount = 1000;
		public float DurationSeconds => GetDurationSeconds();
		public float CurrentTime => GetCurrentTime();

		public float GetDurationSeconds()
		{
			return (float)(lottie.LastKeyframe - lottie.FirstKeyframe);
		}
		public void DrawOneFrame(int Frame)
		{
			CurrentFrame = Frame;
		}
		
		public float GetCurrentTime()
		{
			float TimeNormal = CurrentFrame / (float)TotalFramesCount;
			var Time = Mathf.Lerp( lottie.FirstKeyframe, lottie.LastKeyframe, TimeNormal );
			return Time;
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
		
		struct DebugPoint
		{
			public Vector2	Start;
			public Vector2?	End;			//	if true, draw handle here
			public int		Uid;			//	see if we can automatically do this, but different sizes so we see overlaps
			public float	HandleSize => 1.0f + ((float)Uid*0.3f);
			public Color	Colour;
		}
		
		public void Render(Painter2D Painter,Rect ContentRect,bool EnableDebug)
		{
			var Time = CurrentTime;
			var width = ContentRect.width;
			var height = ContentRect.height;
			
			int PathsDrawn = 0;
			int EllipsesDrawn = 0;
			
			Painter.fillColor = Color.blue;

			//	scale-to-canvas transformer
			float ExtraScale = 1;	//	for debug zooming
			var ScaleToCanvasWidth = (ContentRect.width / lottie.w)*ExtraScale;
			var ScaleToCanvasHeight = (ContentRect.height / lottie.h)*ExtraScale;
			bool Stretch = false;
			bool FitHeight = false;
			var ScaleToCanvasUniform = FitHeight ? ScaleToCanvasHeight : ScaleToCanvasWidth;
			var ScaleToCanvas = Stretch ? new Vector2( ScaleToCanvasWidth, ScaleToCanvasHeight ) : new Vector2( ScaleToCanvasUniform, ScaleToCanvasUniform );
			Transformer RootTransformer = new Transformer( Vector2.zero, Vector2.zero, ScaleToCanvas );

			void RenderGroup(ShapeGroup Group,Transformer LayerTransform)
			{
				//	run through sub shapes
				var Children = Group.Children;

				//	elements (shapes) in the layer may be in the wrong order, so need to pre-extract style & transform
				var GroupTransform = Group.GetTransformer(Time);
				GroupTransform = LayerTransform.Multiply(GroupTransform);
				var GroupStyle = Group.GetShapeStyle(Time);
	
				
	
				//	to do holes in shapes, we need to do them all in one path
				//	so do all the debug stuff on the side
				List<DebugPoint> DebugPoints = new();
				void AddDebugPoint(Vector2 Position,int Uid,Color Colour,Vector2? End=null)
				{
					var Point = new DebugPoint();
					Point.Colour = Colour;
					Point.Uid = Uid;
					Point.Start = Position;
					Point.End = End;
					DebugPoints.Add(Point);
				}
	
				
				void ApplyStyle()
				{
					Painter.fillColor = GroupStyle.FillColour ?? Color.green;
					Painter.lineWidth = GroupTransform.LocalToWorld( GroupStyle.StrokeWidth ?? 1 );
					Painter.strokeColor = GroupStyle.StrokeColour ?? Color.yellow;
					if ( GroupStyle.IsStroked )
						Painter.Stroke();
					if ( GroupStyle.IsFilled )
						Painter.Fill(FillRule.OddEven);
				}
				
				Painter.BeginPath();
				
				foreach ( var Child in Children )
				{
					//	force visible with debug
					if ( !Child.Visible && !EnableDebug )
						continue; 
				
					if ( Child is ShapePath path )
					{
						var Bezier = path.Path_Bezier.GetBezier(Time);
						var Points = Bezier.GetControlPoints();//.Reverse().ToArray();
						void CurveToPoint(Bezier.ControlPoint Point,Bezier.ControlPoint PrevPoint,Bezier.ControlPoint NextPoint)
						{
							//	gr: working out this took quite a bit of time.
							//		the cubic bezier needs 4 points; Prev(start), tangent for first half of line(start+out), tangent for 2nd half(end+in), and the end
							var cp0 = PrevPoint.Position + PrevPoint.OutTangent;
							var cp1 = Point.Position + Point.InTangent;
							
							var VertexPosition = GroupTransform.LocalToWorld(Point.Position);
							var ControlPoint0 = GroupTransform.LocalToWorld(cp0);
							var ControlPoint1 = GroupTransform.LocalToWorld(cp1);
							
							AddDebugPoint( Point.Position, 0, Color.red );
							AddDebugPoint( Point.Position, 1, Color.green, cp0 );
							AddDebugPoint( Point.Position, 2, Color.cyan, cp1 );

							//if ( EnableDebug )
							if ( true )
							{
								Painter.BezierCurveTo( ControlPoint0, ControlPoint1, VertexPosition  );
								//Painter.BezierCurveTo( ControlPoint0, VertexPosition, ControlPoint1 );
								//Painter.BezierCurveTo( ControlPoint1, ControlPoint0, VertexPosition );
								//Painter.BezierCurveTo( ControlPoint1, VertexPosition, ControlPoint0 );
								//Painter.BezierCurveTo( VertexPosition, ControlPoint0, ControlPoint1  );
								//Painter.BezierCurveTo( VertexPosition, ControlPoint1, ControlPoint0  );
							}
							else
							{
								Painter.LineTo( VertexPosition );
							}
						}
						
						for ( var p=0;	p<Points.Length;	p++ )
						{
							var PrevIndex = (p==0 ? Points.Length-1 : p-1);
							var NextIndex = (p+1) % Points.Length;
							var Point = Points[p];
							var PrevPoint = Points[PrevIndex];
							var NextPoint = Points[NextIndex];
							var VertexPosition = GroupTransform.LocalToWorld(Point.Position);
							//	skipping first one gives a more solid result, so wondering if
							//	we need to be doing a mix of p and p+1...
							if ( p==0 )
								Painter.MoveTo(VertexPosition);
							else
								CurveToPoint(Point,PrevPoint,NextPoint);
						}
						
						if ( Bezier.Closed )
						{
							int DebugIndex = (CurrentFrame/2) % Points.Length;
							//CurveToPoint( Points[DebugIndex], Points[DebugIndex], Points[DebugIndex] );
							CurveToPoint( Points[0], Points[Points.Length-1], Points[1] );
						}
					}
					if ( Child is ShapeEllipse ellipse )
					{
						var EllipseSize = GroupTransform.LocalToWorld( ellipse.Size.GetValue(Time) );
						var LocalCenter = ellipse.Center.GetPosition(Time);
						var EllipseCenter = GroupTransform.LocalToWorld(LocalCenter);
		
						var Radius = EllipseSize;
						Painter.Arc( EllipseCenter, Radius, 0, 360 );
						AddDebugPoint( LocalCenter, 0, Color.magenta );
						EllipsesDrawn++;
					}
			
					if ( Child is ShapeGroup subgroup )
					{
						RenderGroup(subgroup,GroupTransform);
					}
				}
				ApplyStyle();
				Painter.ClosePath();
			
				
				if ( EnableDebug )
				{
					foreach ( var Point in DebugPoints )
					{
						var WorldStart = GroupTransform.LocalToWorld(Point.Start);
						Vector2? WorldEnd = Point.End.HasValue ? GroupTransform.LocalToWorld(Point.End.Value) : null;
						
						Painter.lineWidth = 0.2f;
						Painter.strokeColor = Point.Colour;
						Painter.BeginPath();
						Painter.MoveTo( WorldStart );
						if ( WorldEnd is Vector2 end )
						{
							Painter.LineTo( end );
							Painter.Arc( end, Point.HandleSize, 0.0f, 360.0f);
						}
						else
						{
							Painter.Arc( WorldStart, Point.HandleSize, 0.0f, 360.0f);
						}
						Painter.Stroke();
						Painter.ClosePath();
					}
				}
			}
		
			foreach ( var Layer in lottie.layers.Reverse() )
			{
				if ( !Layer.IsVisible(Time) )
					continue;
				
				var LayerTransform = Layer.Transform.GetTransformer(Time);
				LayerTransform = RootTransformer.Multiply(LayerTransform);
				
				
				//	render the shape
				foreach ( var Shape in Layer.Children )
				{
					if ( Shape is ShapeGroup group )
					{
						RenderGroup(group,LayerTransform);
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
			
			//Debug.Log($"Paths {PathsDrawn} Ellipses {EllipsesDrawn}");
		}
		
	}
	
}

