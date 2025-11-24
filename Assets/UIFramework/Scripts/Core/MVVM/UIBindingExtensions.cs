using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIFramework.MVVM
{
    /// <summary>
    /// Extension methods for binding reactive properties to common Unity UI elements.
    /// Provides one-way and two-way binding support.
    /// </summary>
    public static class UIBindingExtensions
    {
        #region Text Bindings (One-Way: ViewModel -> View)

        /// <summary>
        /// Binds a reactive property to a Unity UI Text component.
        /// </summary>
        public static IDisposable BindToText(this PropertyBinder binder, IReadOnlyReactiveProperty<string> property, Text text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return binder.Bind(property, value => text.text = value ?? string.Empty);
        }

        /// <summary>
        /// Binds a reactive property to a TextMeshProUGUI component.
        /// </summary>
        public static IDisposable BindToText(this PropertyBinder binder, IReadOnlyReactiveProperty<string> property, TextMeshProUGUI text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return binder.Bind(property, value => text.text = value ?? string.Empty);
        }

        /// <summary>
        /// Binds a numeric reactive property to text with formatting.
        /// </summary>
        public static IDisposable BindToText<T>(this PropertyBinder binder, IReadOnlyReactiveProperty<T> property, TextMeshProUGUI text, Func<T, string> formatter)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            return binder.Bind(property, value => text.text = formatter(value));
        }

        /// <summary>
        /// Binds a numeric reactive property to Unity UI Text with formatting.
        /// </summary>
        public static IDisposable BindToText<T>(this PropertyBinder binder, IReadOnlyReactiveProperty<T> property, Text text, Func<T, string> formatter)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            return binder.Bind(property, value => text.text = formatter(value));
        }

        #endregion

        #region InputField Bindings (Two-Way: ViewModel <-> View)

        /// <summary>
        /// Binds a reactive property to a Unity UI InputField (two-way binding).
        /// </summary>
        public static IDisposable BindToInputField(this PropertyBinder binder, ReactiveProperty<string> property, InputField inputField)
        {
            if (inputField == null)
                throw new ArgumentNullException(nameof(inputField));

            // ViewModel -> View
            var subscription = binder.Bind(property, value =>
            {
                if (inputField.text != value)
                    inputField.text = value ?? string.Empty;
            });

            // View -> ViewModel
            inputField.onEndEdit.AddListener(value => property.Value = value);

            return subscription;
        }

        /// <summary>
        /// Binds a reactive property to a TMP_InputField (two-way binding).
        /// </summary>
        public static IDisposable BindToInputField(this PropertyBinder binder, ReactiveProperty<string> property, TMP_InputField inputField)
        {
            if (inputField == null)
                throw new ArgumentNullException(nameof(inputField));

            // ViewModel -> View
            var subscription = binder.Bind(property, value =>
            {
                if (inputField.text != value)
                    inputField.text = value ?? string.Empty;
            });

            // View -> ViewModel
            inputField.onEndEdit.AddListener(value => property.Value = value);

            return subscription;
        }

        #endregion

        #region Toggle Bindings (Two-Way: ViewModel <-> View)

        /// <summary>
        /// Binds a reactive boolean property to a Toggle (two-way binding).
        /// </summary>
        public static IDisposable BindToToggle(this PropertyBinder binder, ReactiveProperty<bool> property, Toggle toggle)
        {
            if (toggle == null)
                throw new ArgumentNullException(nameof(toggle));

            // ViewModel -> View
            var subscription = binder.Bind(property, value =>
            {
                if (toggle.isOn != value)
                    toggle.isOn = value;
            });

            // View -> ViewModel
            toggle.onValueChanged.AddListener(value => property.Value = value);

            return subscription;
        }

        #endregion

        #region Slider Bindings (Two-Way: ViewModel <-> View)

        /// <summary>
        /// Binds a reactive float property to a Slider (two-way binding).
        /// </summary>
        public static IDisposable BindToSlider(this PropertyBinder binder, ReactiveProperty<float> property, Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            // ViewModel -> View
            var subscription = binder.Bind(property, value =>
            {
                if (!Mathf.Approximately(slider.value, value))
                    slider.value = value;
            });

            // View -> ViewModel
            slider.onValueChanged.AddListener(value => property.Value = value);

            return subscription;
        }

        /// <summary>
        /// Binds a reactive int property to a Slider (two-way binding).
        /// </summary>
        public static IDisposable BindToSlider(this PropertyBinder binder, ReactiveProperty<int> property, Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            // ViewModel -> View
            var subscription = binder.Bind(property, value =>
            {
                if (slider.value != value)
                    slider.value = value;
            });

            // View -> ViewModel
            slider.onValueChanged.AddListener(value => property.Value = Mathf.RoundToInt(value));

            return subscription;
        }

        #endregion

        #region Dropdown Bindings (Two-Way: ViewModel <-> View)

        /// <summary>
        /// Binds a reactive int property to a Dropdown (two-way binding).
        /// </summary>
        public static IDisposable BindToDropdown(this PropertyBinder binder, ReactiveProperty<int> property, Dropdown dropdown)
        {
            if (dropdown == null)
                throw new ArgumentNullException(nameof(dropdown));

            // ViewModel -> View
            var subscription = binder.Bind(property, value =>
            {
                if (dropdown.value != value)
                    dropdown.value = value;
            });

            // View -> ViewModel
            dropdown.onValueChanged.AddListener(value => property.Value = value);

            return subscription;
        }

        /// <summary>
        /// Binds a reactive int property to a TMP_Dropdown (two-way binding).
        /// </summary>
        public static IDisposable BindToDropdown(this PropertyBinder binder, ReactiveProperty<int> property, TMP_Dropdown dropdown)
        {
            if (dropdown == null)
                throw new ArgumentNullException(nameof(dropdown));

            // ViewModel -> View
            var subscription = binder.Bind(property, value =>
            {
                if (dropdown.value != value)
                    dropdown.value = value;
            });

            // View -> ViewModel
            dropdown.onValueChanged.AddListener(value => property.Value = value);

            return subscription;
        }

        #endregion

        #region Image Bindings (One-Way: ViewModel -> View)

        /// <summary>
        /// Binds a reactive sprite property to an Image.
        /// </summary>
        public static IDisposable BindToImage(this PropertyBinder binder, IReadOnlyReactiveProperty<Sprite> property, Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return binder.Bind(property, value => image.sprite = value);
        }

        /// <summary>
        /// Binds a reactive color property to an Image.
        /// </summary>
        public static IDisposable BindToImageColor(this PropertyBinder binder, IReadOnlyReactiveProperty<Color> property, Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return binder.Bind(property, value => image.color = value);
        }

        /// <summary>
        /// Binds a reactive float property to an Image's fill amount.
        /// </summary>
        public static IDisposable BindToImageFillAmount(this PropertyBinder binder, IReadOnlyReactiveProperty<float> property, Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return binder.Bind(property, value => image.fillAmount = Mathf.Clamp01(value));
        }

        #endregion

        #region GameObject/CanvasGroup Bindings (One-Way: ViewModel -> View)

        /// <summary>
        /// Binds a reactive boolean property to GameObject active state.
        /// </summary>
        public static IDisposable BindToActive(this PropertyBinder binder, IReadOnlyReactiveProperty<bool> property, GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            return binder.Bind(property, value => gameObject.SetActive(value));
        }

        /// <summary>
        /// Binds a reactive boolean property to GameObject active state (inverted).
        /// </summary>
        public static IDisposable BindToInactive(this PropertyBinder binder, IReadOnlyReactiveProperty<bool> property, GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            return binder.Bind(property, value => gameObject.SetActive(!value));
        }

        /// <summary>
        /// Binds a reactive boolean property to a CanvasGroup's interactable state.
        /// </summary>
        public static IDisposable BindToInteractable(this PropertyBinder binder, IReadOnlyReactiveProperty<bool> property, CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
                throw new ArgumentNullException(nameof(canvasGroup));

            return binder.Bind(property, value => canvasGroup.interactable = value);
        }

        /// <summary>
        /// Binds a reactive float property to a CanvasGroup's alpha.
        /// </summary>
        public static IDisposable BindToAlpha(this PropertyBinder binder, IReadOnlyReactiveProperty<float> property, CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
                throw new ArgumentNullException(nameof(canvasGroup));

            return binder.Bind(property, value => canvasGroup.alpha = Mathf.Clamp01(value));
        }

        #endregion

        #region Button Bindings (Command to Interactable)

        /// <summary>
        /// Binds a command's CanExecute to a button's interactable state.
        /// </summary>
        public static IDisposable BindToButton(this PropertyBinder binder, ReactiveCommand command, Button button)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (button == null)
                throw new ArgumentNullException(nameof(button));

            // Bind command execution
            button.onClick.AddListener(() => command.Execute());

            // Bind CanExecute to interactable
            return binder.Bind(command.CanExecute, canExecute => button.interactable = canExecute);
        }

        /// <summary>
        /// Binds a command with parameter to a button.
        /// </summary>
        public static IDisposable BindToButton<T>(this PropertyBinder binder, ReactiveCommand<T> command, Button button, T parameter)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (button == null)
                throw new ArgumentNullException(nameof(button));

            // Bind command execution with parameter
            button.onClick.AddListener(() => command.Execute(parameter));

            // Bind CanExecute to interactable
            return binder.Bind(command.CanExecute, canExecute => button.interactable = canExecute);
        }

        #endregion

        #region ScrollRect Bindings (Two-Way: ViewModel <-> View)

        /// <summary>
        /// Binds a reactive Vector2 property to a ScrollRect's normalized position (two-way binding).
        /// </summary>
        public static IDisposable BindToScrollPosition(this PropertyBinder binder, ReactiveProperty<Vector2> property, ScrollRect scrollRect)
        {
            if (scrollRect == null)
                throw new ArgumentNullException(nameof(scrollRect));

            // ViewModel -> View
            var subscription = binder.Bind(property, value => scrollRect.normalizedPosition = value);

            // View -> ViewModel
            scrollRect.onValueChanged.AddListener(value => property.Value = value);

            return subscription;
        }

        #endregion
    }
}
