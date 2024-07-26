using System;

namespace CSP.Util.Helper
{
    public static class IbanHelper
    {
        private static string[] DonusturmeTablosu_String = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", };
        private static string[] DonusturmeTablosu_Number = { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35" };


        //private static bool IsAlpha(string strAlpha)
        //{
        //    System.Text.RegularExpressions.Regex MyObj = new System.Text.RegularExpressions.Regex("[^A-Z]");
        //    return !MyObj.IsMatch(strAlpha);
        //}

        private static bool IsTR(string strIlkIki)
        {
            return strIlkIki == "TR";
        }

        private static bool IsNumber(string strNumber)
        {
            System.Text.RegularExpressions.Regex MyObj = new System.Text.RegularExpressions.Regex("[^0-9]");
            return !MyObj.IsMatch(strNumber);
        }

        private static bool IsAlphaNumeric(string strAlphaNumeric)
        {
            System.Text.RegularExpressions.Regex MyObj = new System.Text.RegularExpressions.Regex("[^A-Z0-9]");
            return !MyObj.IsMatch(strAlphaNumeric);
        }

        /// <summary>
        /// Check TR Iban with  http://www.tcmb.gov.tr/iban/teblig.htm
        /// </summary>
        /// <param name="iban"></param>
        /// <returns></returns>
        public static bool CheckIbanIfIsTR(string iban)
        {
            string strIBAN = iban.Trim().ToUpper();

            //Boşsa Retunrn False
            if (string.IsNullOrEmpty(strIBAN.Trim())) return false;

            // uzunluk en fazla 34 karakter olabilir 
            //if (strIBAN.Length > 34) return false;

            //TR Iban
            if (strIBAN.Length != 26) return false;

            // ilk iki karakteri alalım
            string str1_2 = strIBAN.Substring(0, 2).Trim();

            // kontrol karakterlerini alalım 
            string str3_4 = strIBAN.Substring(2, 2).Trim();

            string strRezerv = strIBAN.Substring(10, 1).Trim();

            // ilk iki karakter yalnızca harf olabilir 
            // if (!IsAlpha(str1_2)) return false;

            //TR Kontrolü İçin
            if (!IsTR(str1_2)) return false;

            // kontrol karakterleri yalnızca sayı olabilir 
            if (!IsNumber(str3_4)) return false;

            // Rezerv Alanı Tüm hesaplarda 0 olmalıdır. 
            if (strRezerv != "0") return false;

            // IBAN numarası yalnızca harf ve rakam olabilir 
            if (!IsAlphaNumeric(strIBAN)) return false;

            // özel olan ilk 4 karakteri alalım 
            string temp1 = strIBAN.Substring(0, 4).Trim();

            // geri kalan karakterleri alalım 
            string temp2 = strIBAN.Substring(4).Trim();

            // ilk 4 karakteri sona atalım 
            string temp3 = temp2 + temp1;

            // harfleri sayı karşılıklarına çevirelim
            string temp4 = temp3;
            for (int i = 0; i < DonusturmeTablosu_String.Length; i++)
                temp4 = temp4.Replace(DonusturmeTablosu_String[i], DonusturmeTablosu_Number[i]);

            // sayıya çevirelim 
            decimal num = Convert.ToDecimal(temp4);

            // 97'ye göre mod alalım 
            decimal mod = num % 97;

            // eğer mod bölümünden kalan 1 ise uygun bir IBAN       
            // değilse uygun olmayan bir IBAN numarası demektir.    
            if (mod != 1) return false;

            return true;
        }


    }

}