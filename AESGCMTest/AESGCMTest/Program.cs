using System;

namespace AESGCMTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

            string hexKey = AesGcm256.toHex("d4b71024131dba4bf4834247c7953fac"); //클라에서 사용하고 있는 암호 (uuid + accesstoken 이기 때문에 클라마다 달라짐)
            string hexIV = AesGcm256.toHex("IV_VALUE_16_BYTE"); // 이값을 이용할것인지 말것인지는 알아서 결정 해주세요.. (클라랑 같이 수정해야함)

            string plainText = "암호화";
            Console.WriteLine("Plain Text: " + plainText);

            string encryptedText = AesGcm256.encrypt
                  (plainText, AesGcm256.HexToByte(hexKey), AesGcm256.HexToByte(hexIV));
            Console.WriteLine("Encrypted base64 encoded: " + encryptedText);
            
            string decryptedText = AesGcm256.decrypt
                  (encryptedText, AesGcm256.HexToByte(hexKey), AesGcm256.HexToByte(hexIV));
            Console.WriteLine("Decrypted Text: " + decryptedText);
            
            Console.Read();
        }
	}
}
