using System;
using System.Collections;
using System.Collections.Generic;
using LottiePlugin;
using UnityEngine;
using UnityEngine.UIElements;

namespace PopLottie
{
    public class LottieVisualElement : VisualElement
    {
        // Image animation variables
        string _loadingIconResourceUrl;

        PopLottie.Animation _lottieAnimation;
        IVisualElementScheduledItem lastSchedule;
        bool _animEnabled;

        public string animatedImageResourceUrl
        {
            get => _loadingIconResourceUrl;
            set
            {
                _loadingIconResourceUrl = value;

                try
                {
                    var _animationJson = Resources.Load<TextAsset>(_loadingIconResourceUrl.ToString());
                    if ( _animationJson == null )
                        throw new Exception($"Text-Asset Resource not found at {_loadingIconResourceUrl}");
                    _lottieAnimation = new Animation(_animationJson.text);
                }
                catch ( Exception e)
                {
                    Debug.LogError($"Failed to load icon resource as text asset (json); {e.Message}");
                    _lottieAnimation = null;
                }

                // Lottie setup
                if (_lottieAnimation != null)
                {
                    RebuildAnimation();
                }

                MarkDirtyRepaint();
            }
        }

        public bool animEnabled
        {
            get => _animEnabled;
            set
            {
                _animEnabled = value;

                if (_animEnabled)
                {
                    this.style.display = DisplayStyle.Flex;
                    SetStyleSize();
                }
                else
                {
                    this.style.display = DisplayStyle.None;
                }

                MarkDirtyRepaint();
            }
        }


        private void UpdateLoadingAnim()
        {
            if (_lottieAnimation != null && animEnabled == true)
            {
                // Manually update the animation, since execute does not delta time in the lottie anim class
                // We can use +1 here because the anim is called every frame delta anyway
                int CurrentFrame = (_lottieAnimation.CurrentFrame + 1 >= _lottieAnimation.TotalFramesCount) ? 0 : _lottieAnimation.CurrentFrame + 1;

                _lottieAnimation.DrawOneFrame(CurrentFrame);
                //this.style.backgroundImage = _lottieAnimation.Texture;

                MarkDirtyRepaint();
            }
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlBoolAttributeDescription enabledAttribute = new UxmlBoolAttributeDescription()
            {
                name = "animEnabled",
                defaultValue = false
            };
            UxmlStringAttributeDescription animatedImageResourceUrlAttribute = new UxmlStringAttributeDescription()
            {
                name = "animatedImageResourceUrl",
                defaultValue = "IconsAnim/loadingV4"
            };

            public UxmlTraits() { }

            // Use the Init method to assign the value of the progress UXML attribute to the C# progress property.
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                Debug.Log("Init AnimatedImage UXML");

                (ve as LottieVisualElement).animatedImageResourceUrl = animatedImageResourceUrlAttribute.GetValueFromBag(bag, cc);
                (ve as LottieVisualElement).animEnabled = enabledAttribute.GetValueFromBag(bag, cc);
            }
        }

        public LottieVisualElement()
        {
            RegisterCallback<GeometryChangedEvent>(OnVisualElementDirty);
            this.RegisterCallback<DetachFromPanelEvent>(c => { this.Dispose(); });
            generateVisualContent += GenerateVisualContent;
        }
        
        void GenerateVisualContent(MeshGenerationContext context)
        {
            if ( _lottieAnimation == null )
                return;
            
            _lottieAnimation.Render(context.painter2D,contentRect);

        }

        void OnVisualElementDirty(GeometryChangedEvent ev)
        {
            RebuildAnimation();
        }

        public void Dispose()
        {
            if (lastSchedule != null)
            {
                lastSchedule.Pause();
                lastSchedule = null;

                _lottieAnimation.Stop();
                _lottieAnimation.Dispose();
            }
        }

        void SetStyleSize()
        {
            //this.style.scale = new Vector2(1, -1);
        }

        void RebuildAnimation()
        {
            // Small note, there is a bug in rlottie where anims are upside down, in USS specify 'scale: 1 -1;' which fixes it
            // Which is automatically done in SetStyleSize();
            SetStyleSize();

            Debug.Log($"Rebuilding animation of {this.name}({this.animatedImageResourceUrl})...");

            try
            {
                //_lottieAnimation = new PopLottie.Animation();

                _lottieAnimation.Play();
                // We pause the previous ones to avoid multiple schedules
                if (lastSchedule != null)
                {
                    lastSchedule.Pause();
                    lastSchedule = null;
                }

                int _frameDelta = Mathf.RoundToInt(((float)_lottieAnimation.DurationSeconds / _lottieAnimation.TotalFramesCount) * 1000.0f);
                lastSchedule = this.schedule.Execute(UpdateLoadingAnim).Every(_frameDelta);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load icon resource as text asset (json); {e.Message}");
                _lottieAnimation = null;
            }
        }

        public new class UxmlFactory : UxmlFactory<LottieVisualElement, UxmlTraits> { }
    }
}
