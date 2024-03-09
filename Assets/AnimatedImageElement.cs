using System;
using System.Collections;
using System.Collections.Generic;
using LottiePlugin;
using UnityEngine;
using UnityEngine.UIElements;

namespace Studio5UI
{
    public class AnimatedImageElement : VisualElement
    {
        // Image animation variables
        string _loadingIconResourceUrl;

        LottieAnimation _lottieAnimation;
        uint _textureWidth = 24;
        uint _textureHeight = 24;
        uint _superSampling = 2;
        TextAsset _animationJson;
        IVisualElementScheduledItem lastSchedule;
        bool _animEnabled;

        public uint superSampling
        {
            get => _superSampling;
            set
            {
                _superSampling = value;
                MarkDirtyRepaint();
            }
        }

        public string animatedImageResourceUrl
        {
            get => _loadingIconResourceUrl;
            set
            {
                _loadingIconResourceUrl = value;

                //check if icon is null or empty and remove it if it is
                if (string.IsNullOrEmpty(_loadingIconResourceUrl))
                {
                    this.style.backgroundImage = null;
                    MarkDirtyRepaint();
                    return;
                }

                //_loadingIcon.style.backgroundImage = new StyleBackground(Resources.Load<VectorImage>(_loadingIconResourceUrl.ToString()));
                try
                {
                    _animationJson = Resources.Load<TextAsset>(_loadingIconResourceUrl.ToString());
                    if ( _animationJson == null )
                        throw new Exception($"Text-Asset Resource not found at {_loadingIconResourceUrl}");
                }
                catch ( Exception e)
                {
                    Debug.LogError($"Failed to load icon resource as text asset (json); {e.Message}");
                    _animationJson = null;
                }

                // Lottie setup
                if (_animationJson != null)
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
                this.style.backgroundImage = _lottieAnimation.Texture;

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

                (ve as AnimatedImageElement).animatedImageResourceUrl = animatedImageResourceUrlAttribute.GetValueFromBag(bag, cc);
                (ve as AnimatedImageElement).animEnabled = enabledAttribute.GetValueFromBag(bag, cc);
            }
        }

        public AnimatedImageElement()
        {
            RegisterCallback<GeometryChangedEvent>(OnVisualElementDirty);
            this.RegisterCallback<DetachFromPanelEvent>(c => { this.Dispose(); });
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
            if (this.resolvedStyle.width > 0)
            {
                _textureWidth = (uint)Mathf.RoundToInt(this.resolvedStyle.width * superSampling);
            }

            if (this.resolvedStyle.height > 0)
            {
                _textureHeight = (uint)Mathf.RoundToInt(this.resolvedStyle.height * superSampling);
            }

            this.style.scale = new Vector2(1, -1);
        }

        void RebuildAnimation()
        {
            // Small note, there is a bug in rlottie where anims are upside down, in USS specify 'scale: 1 -1;' which fixes it
            // Which is automatically done in SetStyleSize();
            SetStyleSize();

            Debug.Log($"Rebuilding animation of {this.name}({this.animatedImageResourceUrl}) at {_textureWidth}x{_textureHeight}...");

            try
            {
                _lottieAnimation = LottieAnimation.LoadFromJsonData(
                                    _animationJson.text,
                                    string.Empty,
                                    _textureWidth,
                                    _textureHeight);

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
                _animationJson = null;
                _lottieAnimation = null;
            }
        }

        public new class UxmlFactory : UxmlFactory<AnimatedImageElement, UxmlTraits> { }
    }
}
