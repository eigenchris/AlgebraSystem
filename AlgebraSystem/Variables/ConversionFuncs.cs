using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    public static class ConversionFuncs {

        public delegate string ConvertMethod(List<string> args);

        public static ConvertMethod _IntAdd = IntAdd;
        public static ConvertMethod _IntSub = IntSub;
        public static ConvertMethod _IntMult = IntMult;
        public static ConvertMethod _IntDiv = IntDiv;
        public static ConvertMethod _IntMod = IntMod;
        public static ConvertMethod _IntPow = IntPow;
        public static ConvertMethod _IntNeg = IntNeg;

        public static ConvertMethod _EQ = EQ;
        public static ConvertMethod _LT = LT;
        public static ConvertMethod _GT = GT;

        public static Tuple<int,int> ParseIntPair(List<string> args) {
            int int1, int2;
            bool success1 = Int32.TryParse(args[0], out int1);
            bool success2 = Int32.TryParse(args[1], out int2);
            if (!(success1 && success2)) return null;
            return new Tuple<int,int>(int1,int2);
        }

        public static string ParseBool(bool b) {
            if (b) return "true";
            return "false";
        }

        public static string IntAdd(List<string> args) {
            if (args.Count != 2) return "";
            var pair = ParseIntPair(args);
            if (pair == null) return "";
            return (pair.Item1 + pair.Item2).ToString();
        }

        public static string IntSub(List<string> args) {
            if (args.Count != 2) return "";
            var pair = ParseIntPair(args);
            if (pair == null) return "";
            return (pair.Item1 - pair.Item2).ToString();
        }

        public static string IntMult(List<string> args) {
            if (args.Count != 2) return "";
            var pair = ParseIntPair(args);
            if (pair == null) return "";
            return (pair.Item1 * pair.Item2).ToString();
        }

        public static string IntDiv(List<string> args) {
            if (args.Count != 2) return "";
            var pair = ParseIntPair(args);
            if (pair == null) return "";
            if (pair.Item2 == 0) return "";
            return (pair.Item1 / pair.Item2).ToString();
        }

        public static string IntMod(List<string> args) {
            if (args.Count != 2) return "";
            var pair = ParseIntPair(args);
            if (pair == null) return "";
            if (pair.Item2 == 0) return "";
            return (pair.Item1 % pair.Item2).ToString();
        }

        public static string IntPow(List<string> args) {
            if (args.Count != 2) return "";
            var pair = ParseIntPair(args);
            if (pair == null) return "";
            if (pair.Item2 < 0) return "";
            return IntPow(pair.Item1,(uint)pair.Item2).ToString();
        }

        public static string IntNeg(List<string> args) {
            if (args.Count != 1) return "";
            int int1 = 0;
            bool success1 = Int32.TryParse(args[0], out int1);
            if (!success1) return "";
            return (-int1).ToString();
        }

        public static string EQ(List<string> args) {
            if (args.Count != 2) return "";
            var pair = ParseIntPair(args);
            if (pair == null) return "";
            return ParseBool(pair.Item1==pair.Item2);
        }

        public static string LT(List<string> args) {
            if (args.Count != 2) return "";
            var pair = ParseIntPair(args);
            if (pair == null) return "";
            return ParseBool(pair.Item1 < pair.Item2);
        }

        public static string GT(List<string> args) {
            if (args.Count != 2) return "";
            var pair = ParseIntPair(args);
            if (pair == null) return "";
            return ParseBool(pair.Item1 > pair.Item2);
        }



        // for integer powers... stolen from StackOverflow...
        public static int IntPow(int x, uint pow) {
            int ret = 1;
            while (pow != 0) {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }
    }
}
