using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lesson23
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void calculate_Click(object sender, RoutedEventArgs e)
        {
            errorsOut.Text = "";
            concOut.Text = "";
            pvalOut.Text = "";

            String bitString = scrllBinaryIn.Text;
            int stringLength = bitString.Length; // "n" value
            int blockLength;                     // "M" value
            int blockCount;                      // "N" value

            bool checkM = int.TryParse(mValIn.Text, out blockLength);

            // Error checking inputs
            if (checkM == false || blockLength == 0)
            {
                errorsOut.Text = "Please enter a valid integer greater than zero!";
                return;
            }
            else if ((stringLength / blockLength) == 0)
            {
                errorsOut.Text = "Your M value is too big! Please enter a smaller one!";
                return;
            }
            else
            {
                errorsOut.Text = "";
            }
            
            blockCount = stringLength / blockLength;

            // Calculating chi-squared test statistic using methods
            double testStat = chiSquared(blockCount, blockLength, blockArray(blockCount, blockLength, bitString));

            // Calculating p-value with the gamma functions
            double pValue = GammaFunctions.GammaUpper(((double)blockCount / 2), (testStat / 2));
            pvalOut.Text = pValue.ToString();

            // Conditions determining random or non-randomness
            if (pValue < 0.01)
            {
                concOut.Text = "Since the computed P-value of " + pValue.ToString() + " is < 0.01, the alpha level, it is concluded that the sequence is non-random.";
            }
            else
            {
                concOut.Text = "Since the computed P-value of " + pValue.ToString() + " is > 0.01, the alpha level, it is concluded that the sequence is random!";
            }
        }

        // Creates the array of blocks of bits according to the number of blocks and their size
        String[] blockArray(int N, int M, String bits)
        {
            String[] blocks = new String[N]; // Array of blocks
            int newStrLen = N * M;                        // Discarding
            String newStr = bits.Substring(0, newStrLen); // unused bits
            int counter = 0;
            String tempString;

            // Creating N blocks of length M
            for (int i = 0; i < newStrLen; i += M)
            {
                tempString = newStr.Substring(i, M);
                blocks[counter] = tempString;
                counter++;
            }

            return blocks;
        }

        // Calculates pi sub i, or the proportion of ones found in each block
        double piSubI (int M, int i, String[] blocks)
        {
            String block = blocks[i];
            int oneCounter = 0;
            
            foreach (char c in block)
            {
                if(c.Equals('1'))
                {
                    oneCounter++;
                }
            }

            double proportion = (double)oneCounter / (double)M;
            return proportion;
        }

        // Calculates the chi-squared test statistic, using pi sub i and the array of blocks
        double chiSquared(int N, int M, String[] blocks)
        {
            double propSum = 0.0;

            for (int i = 0; i < N; i++)
            {
                double piDiff = piSubI(M, i, blocks) - 0.5;
                propSum += Math.Pow(piDiff, 2);
            }

            double testStat = 4 * M * propSum;
            return testStat;
        }

        // Restricting character inputs
        private void scrllBinaryIn_TextChanged(object sender, TextChangedEventArgs e)
        {
            errorsOut.Text = "";
            concOut.Text = "";
            pvalOut.Text = "";

            String binTxt = scrllBinaryIn.Text;

            // Restricts character input to binary characters 0 to 1
            foreach (char c in binTxt)
            {
                if (!((c >= '0' && c <= '1')) && binTxt != String.Empty)
                {
                    scrllBinaryIn.Text = binTxt.Remove(binTxt.Length - 1, 1);
                    scrllBinaryIn.SelectionStart = scrllBinaryIn.Text.Length;
                }
            }
        }

        // Restricting character inputs
        private void mValIn_TextChanged(object sender, TextChangedEventArgs e)
        {
            errorsOut.Text = "";
            concOut.Text = "";
            pvalOut.Text = "";

            String mVal = mValIn.Text;

            // Restricts character input to numbers 0 to 9
            foreach (char c in mVal)
            {
                if (!((c >= '0' && c <= '9')) && mVal != String.Empty)
                {
                    mValIn.Text = mVal.Remove(mVal.Length - 1, 1);
                    mValIn.SelectionStart = mValIn.Text.Length;
                }
            }
        }
        
        //Code provided in cooperation between Microsoft and NIST (Government Special Issue 2013 Code Downloads)
        public class GammaFunctions
        {
            public static double GammaLower(double a, double x)
            {
                // incomplete Gamma 'P' (lower) aka 'igam'
                if (x < 0.0 || a <= 0.0)
                    throw new Exception("Bad args in GammaLower");
                if (x < a + 1)
                    return GammaLowerSer(a, x); // no surprise
                else
                    return 1.0 - GammaUpperCont(a, x); // indirectly is faster
            }

            public static double GammaUpper(double a, double x)
            {
                // incomplete Gamma 'Q' (upper) == (1 - GammaP) but computed for efficiency
                // aka 'igamc' (incomplete gamma complement)
                if (x < 0.0 || a <= 0.0)
                    throw new Exception("Bad args in GammaUpper");
                if (x < a + 1)
                    return 1.0 - GammaLowerSer(a, x); // indirect is faster
                else
                    return GammaUpperCont(a, x);
            }

            // -------------------------------------------------------------------------------

            private static double LogGamma(double x)
            {
                // Log of Gamma from Lanczos with g=5, n=6/7
                // not in A & S 
                double[] coef = new double[6] { 76.18009172947146, -86.50532032941677,
        24.01409824083091, -1.231739572450155,
        0.1208650973866179E-2, -0.5395239384953E-5 };
                double LogSqrtTwoPi = 0.91893853320467274178;
                double denom = x + 1;
                double y = x + 5.5;
                double series = 1.000000000190015;
                for (int i = 0; i < 6; ++i)
                {
                    series += coef[i] / denom;
                    denom += 1.0;
                }
                return (LogSqrtTwoPi + (x + 0.5) * Math.Log(y) - y + Math.Log(series / x));
            }

            private static double GammaLowerSer(double a, double x)
            {
                // incomplete gamma lower (computed by series expansion)
                if (x < 0.0)
                    throw new Exception("x param less than 0.0 in GammaLowerSer");

                double gln = LogGamma(a);
                double ap = a;
                double del = 1.0 / a;
                double sum = del;
                for (int n = 1; n <= 100; ++n)
                {
                    ++ap;
                    del *= x / ap;
                    sum += del;
                    if (Math.Abs(del) < Math.Abs(sum) * 3.0E-7) // close enough?
                        return sum * Math.Exp(-x + a * Math.Log(x) - gln);
                }
                throw new Exception("Unable to compute GammaLowerSer to desired accuracy");
            }

            private static double GammaUpperCont(double a, double x)
            {
                // incomplete gamma upper computed by continuing fraction
                if (x < 0.0)
                    throw new Exception("x param less than 0.0 in GammaUpperCont");
                double gln = LogGamma(a);
                double b = x + 1.0 - a;
                double c = 1.0 / 1.0E-30; // div by close to double.MinValue
                double d = 1.0 / b;
                double h = d;
                for (int i = 1; i <= 100; ++i)
                {
                    double an = -i * (i - a);
                    b += 2.0;
                    d = an * d + b;
                    if (Math.Abs(d) < 1.0E-30) d = 1.0E-30; // got too small?
                    c = b + an / c;
                    if (Math.Abs(c) < 1.0E-30) c = 1.0E-30;
                    d = 1.0 / d;
                    double del = d * c;
                    h *= del;
                    if (Math.Abs(del - 1.0) < 3.0E-7)
                        return Math.Exp(-x + a * Math.Log(x) - gln) * h;  // close enough?
                }
                throw new Exception("Unable to compute GammaUpperCont to desired accuracy");
            }

        }
    }
}
