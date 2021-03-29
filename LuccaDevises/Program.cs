using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LuccaDevises
{
    class Program
    {
        private static List<Currency> _currencies;
        private static List<ConversionRate> _conversionPath;

        static void Main(string[] args)
        {
            string filePath;
            string startCurrency;
            int startCurrencyAmount;
            string targetCurrency;
            _currencies = new List<Currency>();
            #region Prerequisites
            // Check validity of file path
            if (args.Length < 1 || String.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("Un chemin vers un fichier est necessaire");
                Console.ReadLine();
                return;
            }
            filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Aucun fichier n'a été trouvé au chemin indiqué");
                Console.ReadLine();
                return;
            }
            #endregion

            GetAndParseConversionRatesFile(filePath, out startCurrency, out startCurrencyAmount, out targetCurrency);

            _conversionPath = new List<ConversionRate>();
            if (_currencies.FirstOrDefault(c => c.Name == startCurrency) != null)
            {
                if (FindNextStepInPath(_currencies.First(c => c.Name == startCurrency), targetCurrency))
                {
                    decimal result = 1;
                    foreach (ConversionRate conversionRate in _conversionPath)
                    {
                        result = decimal.Round(conversionRate.Rate * result, 4);
                    }
                    Console.WriteLine(decimal.Round(result * startCurrencyAmount, 0));
                }
                else
                {
                    Console.WriteLine("Pas de conversion possible trouvée.");
                }
            }
            else
            {
                Console.WriteLine("Le fichier contient le problême suivant : la monnaie demandée n'a pas été trouvé dans la liste de conversion.");
            }
        }

        /// <summary>
        /// Recursive function to find next step for currencies conversion
        /// </summary>
        /// <param name="startCurrency">Currency at the start of the step</param>
        /// <param name="targetCurrencyName">Final target currency</param>
        /// <returns>True if a path has been found, false otherwise</returns>
        private static bool FindNextStepInPath(Currency startCurrency, string targetCurrencyName)
        {
            // Check if final step is reachable
            if (startCurrency.ConversionRates.FirstOrDefault(c => c.TargetCurrencyName == targetCurrencyName) != null)
            {
                _conversionPath.Add(startCurrency.ConversionRates.FirstOrDefault(c => c.TargetCurrencyName == targetCurrencyName));
                return true;
            }

            // Find other possibilities
            foreach (ConversionRate rate in startCurrency.ConversionRates)
            {
                // avoid infinite loop by checking if currency is already used in the path
                if(_conversionPath.Where(cp => cp.StartCurrencyName == rate.TargetCurrencyName).Count() == 0)
                {
                    _conversionPath.Add(rate);
                    if (FindNextStepInPath(_currencies.First(c => c.Name == rate.TargetCurrencyName), targetCurrencyName))
                    {
                        return true;
                    }
                }
            }

            // this path can't reach final target, remove it
            _conversionPath.RemoveAll(cr => cr.StartCurrencyName == startCurrency.Name);
            return false;
        }

        /// <summary>
        /// Get file from URL and parse it to return all informations
        /// </summary>
        /// <param name="filePath">File URL</param>
        /// <param name="startCurrency">Starting currency name</param>
        /// <param name="startCurrencyAmount">Starting currency quantity to translate</param>
        /// <param name="targetCurrency">Target currency name</param>
        private static void GetAndParseConversionRatesFile(string filePath, out string startCurrency, out int startCurrencyAmount, 
            out string targetCurrency)
        {
            string[] lines = File.ReadAllLines(filePath);
            string[] firstLine = lines[0].Split(';');
            startCurrency = firstLine[0];
            startCurrencyAmount = int.Parse(firstLine[1]);
            targetCurrency = firstLine[2];
            int conversionRateNumber = int.Parse(lines[1]);
            // limit to N lines if there is more junk lines in file
            for (int i = 2; i < conversionRateNumber + 2; i++)
            {
                string[] line = lines[i].Split(';');
                string currency1 = line[0];
                string currency2 = line[1];
                decimal rate = decimal.Parse(line[2], CultureInfo.InvariantCulture);
                if (_currencies.FirstOrDefault(c => c.Name == currency1) == null)
                {
                    Currency newCurrency = new Currency();
                    newCurrency.Name = currency1;
                    newCurrency.ConversionRates.Add(new ConversionRate(currency1, currency2, rate));
                    _currencies.Add(newCurrency);
                }
                else
                {
                    _currencies.FirstOrDefault(c => c.Name == currency1).ConversionRates.Add(new ConversionRate(currency1, currency2, rate));
                }

                if (_currencies.FirstOrDefault(c => c.Name == currency2) == null)
                {
                    Currency newCurrency = new Currency();
                    newCurrency.Name = currency2;
                    newCurrency.ConversionRates.Add(new ConversionRate(currency2, currency1, decimal.Round(1 /rate,4)));
                    _currencies.Add(newCurrency);
                }
                else
                {
                    _currencies.FirstOrDefault(c => c.Name == currency2).ConversionRates.Add(new ConversionRate(currency2, currency1, decimal.Round(1 /rate,4)));
                }
            }
        }
    }
}
