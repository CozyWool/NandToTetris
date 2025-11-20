using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPicture
{

    public static class ShowPictureTask
    {
        // pixels[y, x] — note the order of coordinates!
        public static string[] GenerateShowPictureCode(bool[,] pixels)
        {
            // Note! the least significant bit in screen memory word is leftmost.
            var program = new List<string>();
            var row = pixels.GetLength(0);
            var col = pixels.GetLength(1);
            for (var i = 0; i < row; i++)
            {
                var counter = 0;
                for (var j = 0; j < col; j++)
                {
                    var sb = new StringBuilder();
                }
                Console.WriteLine();
            }

            return null;
        }
    }
}
