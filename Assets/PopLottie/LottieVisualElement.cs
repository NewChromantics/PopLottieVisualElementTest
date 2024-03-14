using System;
using System.Collections;
using System.Collections.Generic;
using LottiePlugin;
using UnityEngine;
using UnityEngine.UIElements;

namespace PopLottie
{
	public class LottieVisualElement : VisualElement, IDisposable
	{
		// Image animation variables
		string _loadingIconResourceUrl;

		PopLottie.Animation _lottieAnimation;
		IVisualElementScheduledItem lastSchedule;
		bool _animEnabled;
		bool _enableDebug;

		public bool enableDebug
		{
			get => _enableDebug;
			set
			{
				_enableDebug = value;
				MarkDirtyRepaint();
			}
		}

		public string animatedImageResourceUrl
		{
			get => _loadingIconResourceUrl;
			set
			{
				_loadingIconResourceUrl = value;
				LoadAnimation();
				MarkDirtyRepaint();
			}
		}


		void LoadAnimation()
		{
			try
			{
				var _animationJson = Resources.Load<TextAsset>(_loadingIconResourceUrl.ToString());
				if ( _animationJson == null )
					throw new Exception($"Text-Asset Resource not found at {_loadingIconResourceUrl}");
				
				//	parse file
				_lottieAnimation = new Animation(_animationJson.text);
			}
			catch ( Exception e)
			{
				Debug.LogException(e);
				Debug.LogError($"Failed to load animation {_loadingIconResourceUrl}; {e.Message}");
				Dispose();
			}
		}
		
		public new class UxmlFactory : UxmlFactory<LottieVisualElement, UxmlTraits> { }
		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlBoolAttributeDescription enableDebugAttribute = new UxmlBoolAttributeDescription()
			{
				name = "enableDebug",
				defaultValue = false
			};
			UxmlStringAttributeDescription animatedImageResourceUrlAttribute = new UxmlStringAttributeDescription()
			{
				name = "animatedImageResourceUrl",
				defaultValue = "ExampleNoExtension"
			};

			//public UxmlTraits() { }

			// Use the Init method to assign the value of the progress UXML attribute to the C# progress property.
			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				
				(ve as LottieVisualElement).animatedImageResourceUrl = animatedImageResourceUrlAttribute.GetValueFromBag(bag, cc);
				(ve as LottieVisualElement).enableDebug = enableDebugAttribute.GetValueFromBag(bag, cc);
			}
		}

		public LottieVisualElement()
		{
			RegisterCallback<GeometryChangedEvent>(OnVisualElementDirty);
			RegisterCallback<DetachFromPanelEvent>(c => { this.OnDetached(); });
			RegisterCallback<AttachToPanelEvent>(c => { this.OnAttached(); });

			generateVisualContent += GenerateVisualContent;
			
			//	auto play by repainting this element (RIP child elements)
			var FrameDeltaMs = 30;
			this.schedule.Execute( MarkDirtyRepaint ).Every(FrameDeltaMs);
		}
		
		void OnAttached()
		{
			LoadAnimation();
		}
		void OnDetached()
		{
			Dispose();
		}
		
		public void Dispose()
		{
			_lottieAnimation?.Dispose();
			_lottieAnimation = null;
		}
		
		public TimeSpan GetTime()
		{
			return TimeSpan.FromSeconds( Time.time );
		}
		
	
		void GenerateVisualContent(MeshGenerationContext context)
		{
			//  draw an error box if we're missing the animation
			//  gr: can we render text easily here?
			if ( _lottieAnimation == null )
			{
				if ( enableDebug )
				{
					var TL = new Vector2( contentRect.xMin, contentRect.yMin );
					var TR = new Vector2( contentRect.xMax, contentRect.yMin );
					var BL = new Vector2( contentRect.xMin, contentRect.yMax );
					var BR = new Vector2( contentRect.xMax, contentRect.yMax );
					context.painter2D.BeginPath();
					context.painter2D.MoveTo( TL );
					context.painter2D.LineTo( TR );
					context.painter2D.LineTo( BR );
					context.painter2D.LineTo( BL );
					context.painter2D.LineTo( TL );
					context.painter2D.LineTo( BR );
					context.painter2D.MoveTo( BL );
					context.painter2D.LineTo( TR );
					context.painter2D.ClosePath();
					context.painter2D.lineWidth = 1;
					context.painter2D.strokeColor = Color.magenta;
					context.painter2D.Stroke();
				}
				return;
			}
			
			//var Time = GetTime();
			//_lottieAnimation.Render( Time, context.painter2D, contentRect, enableDebug );
			FrameNumber = (FrameNumber+1.001f) % (float)_lottieAnimation.FrameCount;
			_lottieAnimation.Render( FrameNumber, context.painter2D, contentRect, enableDebug );
		}
		float FrameNumber = 0;

		void OnVisualElementDirty(GeometryChangedEvent ev)
		{
			//	content rect changed
			Debug.Log($"OnVisualElementDirty anim={this._lottieAnimation} resource={this.animatedImageResourceUrl}");
		}


	}
}
