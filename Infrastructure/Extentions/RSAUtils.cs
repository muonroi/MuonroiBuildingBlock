namespace MuonroiBuildingBlock.Infrastructure.Extentions
{
    public static class RSAUtils
    {
        public static string ExportPublicKeyToPEM(RSACryptoServiceProvider rsa)
        {
            RSAParameters parameters = rsa.ExportParameters(false);
            using StringWriter stringWriter = new();
            PemWriter pemWriter = new(stringWriter);
            pemWriter.WriteObject(new RsaKeyParameters(false, new Org.BouncyCastle.Math.BigInteger(1, parameters.Modulus), new Org.BouncyCastle.Math.BigInteger(1, parameters.Exponent)));
            pemWriter.Writer.Flush();
            return stringWriter.ToString();
        }

        public static RSAParameters ConvertPemToRSAParameters(string pem)
        {
            using StringReader stringReader = new(pem);
            PemReader pemReader = new(stringReader);
            return pemReader.ReadObject() is RsaKeyParameters rsaKeyParameters
                ? new RSAParameters
                {
                    Modulus = rsaKeyParameters.Modulus.ToByteArrayUnsigned(),
                    Exponent = rsaKeyParameters.Exponent.ToByteArrayUnsigned()
                }
                : throw new Exception("Invalid PEM format.");
        }
    }
}