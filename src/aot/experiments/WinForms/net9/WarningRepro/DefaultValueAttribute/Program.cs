using System.ComponentModel;

namespace WinFormsWrnRepro
{
    internal class Program
    {
        public class FlatButtonAppearance
        {
            //[DefaultValue(typeof(Color), "")]
            private Color _borderColor = Color.Empty;
            [DefaultValue(typeof(Color), "")]
            public Color BorderColor
            {
                get
                {
                    return _borderColor;
                }
                set
                {
                    //if (_borderColor != value)
                    //{
                        _borderColor = value;
                    //}
                }
            }
        }

        public readonly struct Color
        {
            //
            // Summary:
            //     Represents a color that is null.
            public static readonly Color Empty;

            //
            // Summary:
            //     Gets a system-defined color that has an ARGB value of #FF66CDAA.
            //
            // Returns:
            //     A System.Drawing.Color representing a system-defined color.
            public static Color MediumAquamarine { get; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
