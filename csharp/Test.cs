using System;
using System.Text;
using System.Collections.Generic;
using Lisk32;

public class TestLisk32
{
    private static Dictionary<string, string> VALID_ADDRESS = new Dictionary<string, string>  {
        {"lsk24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu", "c247a42e09e6aafd818821f75b2f5b0de47c8235"},
        {"lskxwnb4ubt93gz49w3of855yy9uzntddyndahm6s", "0dce64c0d36a3e04b6e8679eb5c62d800f3d6a27"},
        {"lskzkfw7ofgp3uusknbetemrey4aeatgf2ntbhcds", "053d7733df22210dd0e6b4ec595a29cdb33ffb07"}
    };
    private static string[] INVALID_ADDRESS = new string[] {
        "24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu"    ,  // missing prefix
        "lsk24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5e"  ,  // incorrect length (length 40 instead of 41)
        "lsk24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5euu",  // incorrect length (length 42 instead of 41)
        "LSK24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu" ,  // incorrect prefix
        "tsk24cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu" ,  // incorrect prefix
        "lsk24cd35u4jdq8sz03pnsqe5dsxwrnazyqqqg5eu" ,  // invalid character (contains a zero)
        "lsk24Cd35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu" ,  // invalid character (contains an upper case 'C' instead of lower case 'c')
        "lsk24dc35u4jdq8szo3pnsqe5dsxwrnazyqqqg5eu"    // invalid checksum due to character swap
    };

    public static string BytesToHexString(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
    }

    private static void testValidAddresses() {
        foreach (KeyValuePair<string, string> entry in VALID_ADDRESS) {
            Console.Write(" * " + entry.Key + " -> " + entry.Value + " ... ");
            // Test Decode
            byte[] decodeBytes = Converter.DecodeLisk32(entry.Key);
            string decode = BytesToHexString(decodeBytes);
            if(!decode.Equals(entry.Value)) {
                Console.WriteLine("Failed!");
                Console.WriteLine("Invalid Decode test for valid address " + entry.Key + " -> " + entry.Value);
                Console.WriteLine("Decode returned wrongly: " + decode);
                throw new Exception("Invalid test");
            }
            // Test Encode
            string encode = Converter.EncodeLisk32(decodeBytes);
            if(!encode.Equals(entry.Key)) {
                Console.WriteLine("Failed!");
                Console.WriteLine("Invalid Encode test for valid address " + entry.Key + " -> " + entry.Value);
                Console.WriteLine("Encode returned wrongly: " + encode);
                throw new Exception("Invalid test");
            }
            Console.WriteLine("OK.");
        }
    }
    private static void testInvalidAddress() {
        foreach (string entry in INVALID_ADDRESS) {
            Console.Write(" * " + entry + " ... ");
            try {
                Converter.DecodeLisk32(entry);
                Console.WriteLine("Failed!");
                Console.WriteLine("Decode didn't throw for: " + entry);
                throw new Exception("Invalid test");
            }
            catch(Lisk32ConversionException) {
                Console.WriteLine("OK.");
                continue;
            }
        }
    }
    public static void Main(string[] args)
    {
        Console.WriteLine ("Lisk32 Test suite running...");
        Console.WriteLine ("Testing Valid Addresses...");
        try {
            testValidAddresses();
        }
        catch(Lisk32ConversionException e) {
            Console.WriteLine("Error: "+ e.Message);
        }
        Console.WriteLine ("Testing Invalid Addresses...");
        testInvalidAddress();
    }
}