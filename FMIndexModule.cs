using System;
using System.Threading;
using Parcs;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace FirstModule
{
    public class FMIndexModule : IModule
    {

        private static bool CompareSuffixAt(String text, int a, int b, int letter)
        {
            if (a + letter < text.Length && b + letter < text.Length)
            {
                return text[a + letter] < text[b + letter];
            }
            if (a + letter >= text.Length && b + letter < text.Length)
            {
                return true;
            }
            return false;
        }

        private static bool CompareSuffix(String text, int a, int b, int minLetter)
        {
            while (!CompareSuffixAt(text, a, b, minLetter) && !CompareSuffixAt(text, b, a, minLetter))
            {
                minLetter++;
            }
            return CompareSuffixAt(text, a, b, minLetter);
        }

        private static void Swap(List<Int32> order, int a, int b)
        {
            int tmp = order[a];
            order[a] = order[b];
            order[b] = tmp;
        }

        private static int SplitBy(String text, List<Int32> order, int left, int right, int by, int minLetter)
        {
            int i = left;
            int index = -1;
            for (int j = left; j < right; j++)
            {
                if (order[j] == by || CompareSuffix(text, order[j], by, minLetter))
                {
                    if (order[j] == by) index = i;
                    Swap(order, i, j);
                    i++;
                }
            }
            Swap(order, i - 1, index);
            return i - 1;
        }

        private static void BuildSuffixArray(
            String text, List<Int32> order,
            int left, int right,
            char leftLetter = '$', char rightLetter = 'z', int minLetter = 0
            )
        {
            if (left >= right - 1) return;
            if (leftLetter == rightLetter)
            {
                //minLetter++;
                leftLetter = '$';
                rightLetter = 'z';
            }

            int by = order[(left + right) / 2];
            int center = SplitBy(text, order, left, right, by, minLetter);
            BuildSuffixArray(text, order, left, center, leftLetter, text[by + minLetter], minLetter);
            BuildSuffixArray(text, order, center + 1, right, text[by + minLetter], rightLetter, minLetter);
        }

        public static String BuildFMIndex(String text, List<Int32> order)
        {
            BuildSuffixArray(text, order, 0, order.Count);
            StringBuilder builder = new StringBuilder();
            foreach (int s in order)
            {
                int pos = (s + text.Length - 1) % text.Length;
                builder = builder.Append(text[pos]);
            }
            return builder.ToString();
        }

        public void Run(ModuleInfo info, CancellationToken token = default(CancellationToken))
        {
            String text = info.Parent.ReadString();
            List<Int32> order = info.Parent.ReadObject<List<Int32>>();
            info.Parent.WriteData(BuildFMIndex(text, order));
        }
    }
}
