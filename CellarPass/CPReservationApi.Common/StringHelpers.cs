using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Web;
using System.Collections.Specialized;
using System.Reflection;
using System.ComponentModel;

namespace CPReservationApi.Common
{
    internal class cTripleDES
    {

        // define the triple des provider
        private TripleDESCryptoServiceProvider m_des = new TripleDESCryptoServiceProvider();

        // define the string handler
        private UTF8Encoding m_utf8 = new UTF8Encoding();

        // define the local property arrays
        private byte[] m_key;
        private byte[] m_iv;

        public cTripleDES(byte[] key, byte[] iv)
        {
            this.m_key = key;
            this.m_iv = iv;
            m_des.Mode = CipherMode.ECB;
        }

        public byte[] Encrypt(byte[] input)
        {
            return Transform(input, m_des.CreateEncryptor(m_key, m_iv));
        }

        public byte[] Decrypt(byte[] input)
        {
            return Transform(input, m_des.CreateDecryptor(m_key, m_iv));
        }

        public string Encrypt(string text)
        {
            byte[] input = m_utf8.GetBytes(text);
            byte[] output = Transform(input, m_des.CreateEncryptor(m_key, m_iv));
            return Convert.ToBase64String(output);
        }

        public string Decrypt(string text)
        {
            byte[] input = Convert.FromBase64String(text);
            byte[] output = Transform(input, m_des.CreateDecryptor(m_key, m_iv));
            return m_utf8.GetString(output);
        }

        private byte[] Transform(byte[] input, ICryptoTransform CryptoTransform)
        {
            // create the necessary streams
            MemoryStream memStream = new MemoryStream();
            CryptoStream cryptStream = new CryptoStream(memStream, CryptoTransform, CryptoStreamMode.Write);
            // transform the bytes as requested
            cryptStream.Write(input, 0, input.Length);
            cryptStream.FlushFinalBlock();
            // Read the memory stream and convert it back into byte array
            memStream.Position = 0;
            byte[] result = new byte[System.Convert.ToInt32(memStream.Length - 1) + 1];
            memStream.Read(result, 0, System.Convert.ToInt32(result.Length));
            // close and release the streams
            memStream.Close();
            cryptStream.Close();
            // hand back the encrypted buffer
            return result;
        }
    }

    public class StringHelpers
    {

        #region "Constants"


        private const char QUERY_STRING_DELIMITER = '&';
        #endregion

        #region "Members"

        private static RijndaelManaged _cryptoProvider;
        //128 bit encyption: DO NOT CHANGE    
        private static readonly byte[] Key = {
        12,
        14,
        6,
        22,
        32,
        28,
        8,
        24,
        13,
        3,
        11,
        9,
        17,
        15,
        4,
        29
    };
        private static readonly byte[] IV = {
        12,
        2,
        16,
        7,
        5,
        9,
        17,
        8,
        4,
        47,
        16,
        18,
        1,
        32,
        25,
        10

    };
        //DO NOT CHANGE
        private static readonly string _Pwd = "3BF3EE08-B757-41ba-87F5-59CC6B9A47B4";
        private static readonly byte[] _Salt = new byte[] {
        0x45,
        0xf1,
        0x61,
        0x6e,
        0x20,
        0x0,
        0x65,
        0x64,
        0x76,
        0x65,
        0x64,
        0x3,
        0x76

    };
        #endregion

        #region "Constructor"

        static StringHelpers()
        {
            _cryptoProvider = new RijndaelManaged();
            _cryptoProvider.Mode = CipherMode.CBC;
            _cryptoProvider.Padding = PaddingMode.PKCS7;
        }

        #endregion

        #region "Methods"

        public static string GetUSStateNameByStatecode(string stateCode)
        {
            string stateName = "California";
            switch (stateCode.ToUpper())
            {
                case "AL":
                    stateName = "Alabama";
                    break;
                case "AK":
                    stateName = "Alaska";
                    break;
                case "AS":
                    stateName = "American Samoa";
                    break;
                case "AZ":
                    stateName = "Arizona";
                    break;
                case "AR":
                    stateName = "Arkansas";
                    break;
                case "CA":
                    stateName = "California";
                    break;
                case "CO":
                    stateName = "Colorado";
                    break;
                case "CT":
                    stateName = "Connecticut";
                    break;
                case "DE":
                    stateName = "Delaware";
                    break;
                case "DC":
                    stateName = "District Of Columbia";
                    break;
                case "FM":
                    stateName = "Federated States Of Micronesia";
                    break;
                case "FL":
                    stateName = "Florida";
                    break;
                case "GA":
                    stateName = "Georgia";
                    break;
                case "GU":
                    stateName = "Guam";
                    break;
                case "HI":
                    stateName = "Hawaii";
                    break;
                case "ID":
                    stateName = "Idaho";
                    break;
                case "IL":
                    stateName = "Illinois";
                    break;
                case "IN":
                    stateName = "Indiana";
                    break;
                case "IA":
                    stateName = "Iowa";
                    break;
                case "KS":
                    stateName = "Kansas";
                    break;
                case "KY":
                    stateName = "Kentucky";
                    break;
                case "LA":
                    stateName = "Louisiana";
                    break;
                case "ME":
                    stateName = "Maine";
                    break;
                case "MH":
                    stateName = "Marshall Islands";
                    break;
                case "MD":
                    stateName = "Maryland";
                    break;
                case "MA":
                    stateName = "Massachusetts";
                    break;
                case "MI":
                    stateName = "Michigan";
                    break;
                case "MN":
                    stateName = "Minnesota";
                    break;
                case "MS":
                    stateName = "Mississippi";
                    break;
                case "MO":
                    stateName = "Missouri";
                    break;
                case "MT":
                    stateName = "Montana";
                    break;
                case "NE":
                    stateName = "Nebraska";
                    break;
                case "NV":
                    stateName = "Nevada";
                    break;
                case "NH":
                    stateName = "New Hampshire";
                    break;
                case "NJ":
                    stateName = "New Jersey";
                    break;
                case "NM":
                    stateName = "New Mexico";
                    break;
                case "NY":
                    stateName = "New York";
                    break;
                case "NC":
                    stateName = "North Carolina";
                    break;
                case "ND":
                    stateName = "North Dakota";
                    break;
                case "MP":
                    stateName = "Northern Mariana Islands";
                    break;
                case "OH":
                    stateName = "Ohio";
                    break;
                case "OK":
                    stateName = "Oklahoma";
                    break;
                case "OR":
                    stateName = "Oregon";
                    break;
                case "PW":
                    stateName = "Palau";
                    break;
                case "PA":
                    stateName = "Pennsylvania";
                    break;
                case "PR":
                    stateName = "Puerto Rico";
                    break;
                case "RI":
                    stateName = "Rhode Island";
                    break;
                case "SC":
                    stateName = "South Carolina";
                    break;
                case "SD":
                    stateName = "South Dakota";
                    break;
                case "TN":
                    stateName = "Tennessee";
                    break;
                case "TX":
                    stateName = "Texas";
                    break;
                case "UT":
                    stateName = "Utah";
                    break;
                case "VT":
                    stateName = "Vermont";
                    break;
                case "VI":
                    stateName = "Virgin Islands";
                    break;
                case "VA":
                    stateName = "Virginia";
                    break;
                case "WA":
                    stateName = "Washington";
                    break;
                case "WV":
                    stateName = "West Virginia";
                    break;
                case "WI":
                    stateName = "Wisconsin";
                    break;
                case "WY":
                    stateName = "Wyoming";
                    break;
            }
            return stateName;
        }

        public static void ParseCustomerName(string custName, ref string firstName, ref string lastName)
        {
            //char splitChar = ' ';
            if (custName.Contains(","))
            {
                string[] arr = custName.Split(',');
                lastName = arr[0];
                if (arr.Length > 1)
                    firstName = arr[1];
            }
            else
            {
                string[] arr = custName.Split(' ');
                firstName = arr[0];
                if (arr.Length > 1)
                    lastName = arr[1];

            }
        }
        /// <summary>
        /// Encrypts a given string.
        /// </summary>
        /// <param name="unencryptedString">Unencrypted string</param>
        /// <returns>Returns an encrypted string</returns>
        public static string EncryptQuery(string unencryptedString)
        {
            byte[] bytIn = ASCIIEncoding.ASCII.GetBytes(unencryptedString);

            // Create a MemoryStream
            MemoryStream ms = new MemoryStream();

            // Create Crypto Stream that encrypts a stream
            CryptoStream cs = new CryptoStream(ms, _cryptoProvider.CreateDecryptor(Key, IV), CryptoStreamMode.Write);

            // Write content into MemoryStream
            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();

            byte[] bytOut = ms.ToArray();
            return Convert.ToBase64String(bytOut);
        }

        public static string DecryptQuery(string encryptedString)
        {
            if (encryptedString is object)
            {
                if (encryptedString.Trim().Length != 0)
                {
                    // Convert from Base64 to binary
                    var bytIn = Convert.FromBase64String(encryptedString);

                    // Create a MemoryStream
                    var ms = new MemoryStream(bytIn, 0, bytIn.Length);

                    // Create a CryptoStream that decrypts the data
                    var cs = new CryptoStream(ms, _cryptoProvider.CreateDecryptor(Key, IV), CryptoStreamMode.Read);

                    // Read the Crypto Stream
                    var sr = new StreamReader(cs);
                    return sr.ReadToEnd();
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        public static NameValueCollection DecryptQueryString(string queryString)
        {

            if (queryString != null)
            {

                if (queryString.Length != 0)
                {
                    //NameValue
                    NameValueCollection queryItems = new NameValueCollection();

                    //Decode the string
                    //string decodedQueryString = HttpUtility.UrlDecode(queryString);

                    //Decrypt the string
                    string decryptedQueryString = Decrypt(queryString);

                    //decryptedQueryString = HttpUtility.UrlDecode(decryptedQueryString);

                    //Now split the string based on each parameter
                    string[] actionQueryString = decryptedQueryString.Split(new char[] { QUERY_STRING_DELIMITER });

                    NameValueCollection newQueryString = new NameValueCollection();

                    //loop around for each name value pair.
                    int index = 0;
                    while (index < actionQueryString.Length)
                    {
                        string[] queryStringItem = actionQueryString[index].Split(new char[] { '=' });
                        queryItems.Add(queryStringItem[0], queryStringItem[1]);
                        System.Math.Max(System.Threading.Interlocked.Increment(ref index), index - 1);
                    }

                    return queryItems;
                }
                else
                {
                    //No query string was passed in.
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static string EncryptQueryString(NameValueCollection queryString)
        {
            //create a string for each value in the query string passed in.
            string tempQueryString = "";

            int index = 0;
            while (index < queryString.Count)
            {
                tempQueryString += queryString.GetKey(index) + "=" + queryString[index];
                if (index != queryString.Count - 1)
                {
                    tempQueryString += QUERY_STRING_DELIMITER;
                }

                System.Math.Max(System.Threading.Interlocked.Increment(ref index), index - 1);
            }

            return Encrypt(tempQueryString);
        }

        public static string EncryptedCardNumber(string clearText, string saltValue, string DecryptKey)
        {
            if (clearText.Trim() == string.Empty)
                return "";
            else
            {
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
                byte[] pwd = Convert.FromBase64String(DecryptKey);
                // Dim pwd As Byte() = Encoding.ASCII.GetBytes(DecryptKey)
                cTripleDES des = new cTripleDES(pwd, saltValueBytes);

                return des.Encrypt(clearText);
            }
        }

        public static string GenerateRandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch = '\0';
            for (int i = 0; i <= size - 1; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
            {
                return builder.ToString().ToLower();
            }
            return builder.ToString();
        }

        public static string EncryptOneWay(string ToEncrypt)
        {

            try
            {
                byte[] arrbyte = new byte[ToEncrypt.Length + 1];
                MD5 hash = new MD5CryptoServiceProvider();
                arrbyte = hash.ComputeHash(Encoding.UTF8.GetBytes(ToEncrypt));
                return Convert.ToBase64String(arrbyte);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static string Decrypt(string cipherText)
        {
            if ((cipherText != null))
            {
                if (!string.IsNullOrEmpty(cipherText.Trim()))
                {
                    try
                    {
                        byte[] cipherBytes = Convert.FromBase64String(cipherText);
                        Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(_Pwd, _Salt);
                        byte[] decryptedData = DoDecrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
                        return System.Text.Encoding.Unicode.GetString(decryptedData);
                    }
                    catch (Exception ex)
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        private static byte[] DoDecrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = null;
            try
            {
                Rijndael alg = Rijndael.Create();
                alg.Key = Key;
                alg.IV = IV;
                cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(cipherData, 0, cipherData.Length);
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
            catch
            {
                return null;
            }
            finally
            {
                cs.Close();
            }
        }

        public static string Encrypt(string clearText)
        {
            if (clearText == null)
            {
                return "";
            }
            if (object.ReferenceEquals(clearText.Trim(), string.Empty))
            {
                return "";
            }
            else
            {
                byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(_Pwd, _Salt);
                byte[] encryptedData = DoEncrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
                return Convert.ToBase64String(encryptedData);
            }
        }

        private static byte[] DoEncrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = null;
            try
            {
                Rijndael alg = Rijndael.Create();
                alg.Key = Key;
                alg.IV = IV;
                cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(clearData, 0, clearData.Length);
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
            catch
            {
                return null;
            }
            finally
            {
                cs.Close();
            }
        }

        public static string ExtractNumber(string inputString)
        {
            string retvalue = "";
            try
            {
                if (inputString.Length > 0)
                {
                    System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(inputString, "[0-9]");
                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        retvalue += match.Value;
                    }
                }
            }
            catch
            {
            }
            return retvalue;
        }

        public static string ParseBetweenTags(string startToken, string endToken, string parseThis)
        {
            // The pattern to match is: (?<={span}).*(?={/span}) 

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("(?<=" + startToken + ").*(?=" + endToken + ")");
            System.Text.RegularExpressions.Match match = regex.Match(parseThis.Replace(Constants.vbCrLf, ""));

            if (match.Success == true)
            {
                return match.Value;
            }
            else
            {
                return "";
            }

        }

        public static string Sanitize(string inputString)
        {

            string newStr = "";

            newStr = Regex.Replace(inputString, "[^\\w\\. -']", "");

            return newStr.Trim();

        }

        public static string FormatTelephoneNumber(string phoneNumber, string country)
        {

            string FormattedPhone = "";

            try
            {
                switch (country)
                {
                    case "CN":
                        if (phoneNumber.Length == 10)
                        {
                            FormattedPhone = string.Format("({0})-{1}", phoneNumber.Substring(0, 2), phoneNumber.Substring(2, 8));
                        }
                        else
                        {
                            FormattedPhone = phoneNumber;
                        }
                        break; // TODO: might not be correct. Was : Exit Select

                        break;
                    case "GB":
                        if (phoneNumber.Length > 10)
                        {
                            FormattedPhone = string.Format("{0} {1} {2} {3}", phoneNumber.Substring(0, 2), phoneNumber.Substring(2, 3), phoneNumber.Substring(5, 3), phoneNumber.Substring(8, phoneNumber.Length - 8));
                        }
                        else
                        {
                            FormattedPhone = phoneNumber;
                        }
                        break; // TODO: might not be correct. Was : Exit Select

                        break;
                    case "US":
                        {
                            if (phoneNumber.Length == 10)
                                FormattedPhone = string.Format("+1 ({0}) {1}-{2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6));
                            else if (phoneNumber.Length == 11)
                            {
                                phoneNumber = phoneNumber.TrimStart('1');
                                FormattedPhone = string.Format("+1 ({0}) {1}-{2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6, 4));
                                //if (Common.Common.Left(phoneNumber.Trim(), 1) == "1")
                                //{
                                //    phoneNumber = phoneNumber.TrimStart('1');
                                //    FormattedPhone = string.Format("+1 ({0}) {1}-{2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6,4));
                                //}
                                //else
                                //    FormattedPhone = phoneNumber;
                            }
                            else if (phoneNumber.Length == 12 || phoneNumber.Length == 13)
                            {
                                phoneNumber = phoneNumber.Replace("+1 ", "").Replace("+1", "");
                                FormattedPhone = string.Format("+1 ({0}) {1}-{2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6, 4));
                                //if (phoneNumber.Trim().IndexOf("+1") > -1)
                                //{
                                //    phoneNumber = phoneNumber.Replace("+1 ", "").Replace("+1", "");
                                //    FormattedPhone = string.Format("+1 ({0}) {1}-{2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6));
                                //}
                                //else
                                //    FormattedPhone = phoneNumber;
                            }
                            else
                                FormattedPhone = "+1" + phoneNumber;
                            break;
                        }

                    default:
                        {
                            FormattedPhone = "+1" + phoneNumber;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                FormattedPhone = "+1" + phoneNumber;
            }

            return FormattedPhone;

        }

        public static bool IsPhoneNumberValid(string phoneNumber, string country)
        {
            bool isValidPhoneNum = false;
            if (string.IsNullOrWhiteSpace(country))
                country = "US";
            else
                country = country.Trim().ToUpper();
            try
            {
                var phoneNumUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                var phoneNumberObj = phoneNumUtil.Parse(phoneNumber, country);
                isValidPhoneNum = phoneNumUtil.IsValidNumber(phoneNumberObj);
            }
            catch {
                isValidPhoneNum = true;
            }
            return isValidPhoneNum;

        }

        public static string FormatTelephoneCommerce7(string phoneNumber, string country, bool doValidation = true, PhoneNumbers.PhoneNumberFormat PhoneNumberFormat = PhoneNumbers.PhoneNumberFormat.E164, string Member_phoneNumber="", string Member_country = "")
        {
            string FormattedPhone = "";
            if (string.IsNullOrWhiteSpace(country))
                country = "US";
            else
                country = country.Trim().ToUpper();

            try
            {
                var phoneNumUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                var phoneNumberObj = phoneNumUtil.Parse(phoneNumber, country);
                if (doValidation)
                {
                    if (phoneNumUtil.IsValidNumber(phoneNumberObj))
                        FormattedPhone = phoneNumUtil.Format(phoneNumberObj, PhoneNumberFormat);
                    else
                    {
                        phoneNumberObj = phoneNumUtil.Parse(Member_phoneNumber, Member_country);

                        if (phoneNumUtil.IsValidNumber(phoneNumberObj))
                            FormattedPhone = phoneNumUtil.Format(phoneNumberObj, PhoneNumberFormat);
                    }
                }
                else
                    FormattedPhone = phoneNumUtil.Format(phoneNumberObj, PhoneNumberFormat);
            }
            catch { }

            return FormattedPhone;

        }

        public static string AlphaNumbericOnly(string source)
        {
            return Regex.Replace(source, "[^A-Za-z0-9]+", "");
        }

        //To check decimal is a integer or not
        public static bool IsInteger(double number)
        {
            return (number % 1 == 0);
        }
        #endregion

        #region Encryption & Decryption
        public static string Encryption(string simpleText)
        {
            string cipherText = string.Empty;
            if (string.IsNullOrEmpty(simpleText))
                return cipherText;
            cipherText = Encrypt(simpleText);
            return cipherText;
        }

        public static string Decryption(string cipherText)
        {
            string simpleText = string.Empty;
            if (string.IsNullOrEmpty(cipherText))
                return simpleText;
            simpleText = Decrypt(cipherText);
            return simpleText;
        }
        #endregion

        public static string ToTitleCase(string inputStr)
        {
            string newValue = "";

            if (!string.IsNullOrEmpty(inputStr))
            {
                var textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;

                newValue = textInfo.ToTitleCase(inputStr.ToLower());
            }

            return newValue;
        }

        public static string GetMd5Hash(string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i <= data.Length - 1; i++)
                sBuilder.Append(data[i].ToString("x2"));

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static string GetImagePath(ImageType imageType, ImagePathType pathType)
        {
            string path = "";
            string basePath = "";
            string folderPath = "";
            switch (pathType)
            {
                case ImagePathType.cdn:
                    {
                        basePath = "https://cdn.cellarpass.com";
                        break;
                    }

                case ImagePathType.azure:
                    {
                        basePath = "https://cdncellarpass.blob.core.windows.net";
                        break;
                    }
            }

            switch (imageType)
            {
                case ImageType.cpImage:
                    {
                        folderPath = "";
                        break;
                    }

                case ImageType.memberImage:
                    {
                        folderPath = "/photos/profiles";
                        break;
                    }

                case ImageType.rsvpEventImage:
                    {
                        folderPath = "/photos/events";
                        break;
                    }

                case ImageType.ticketEventImage:
                    {
                        folderPath = "/photos/tickets";
                        break;
                    }

                case ImageType.adImage:
                    {
                        folderPath = "/photos/ads";
                        break;
                    }

                case ImageType.user:
                    {
                        folderPath = "/photos/users";
                        break;
                    }

                case ImageType.ProductImage:
                    {
                        folderPath = "/photos/product_images";
                        break;
                    }
                case ImageType.ReceiptLogo:
                    {
                        folderPath = "/photos/receipt_logo";
                        break;
                    }
                case ImageType.OrderSignature:
                    {
                        folderPath = "/photos/signatures";
                        break;
                    }
                case ImageType.LocationMaps:
                    {
                        folderPath = "/photos/location_maps";
                        break;
                    }
                case ImageType.RsvpExport:
                    {
                        folderPath = "/admin/rsvp_exports";
                        break;
                    }
            }

            path = string.Format("{0}{1}", basePath, folderPath);
            return path;
        }
    }
}
