namespace Muonroi.BuildingBlock.External.Extensions
{
    public static class MRSAUtilsExtention
    {
        public static string ExportPublicKeyToPEM(RSAParameters rsaParameters)
        {
            using StringWriter stringWriter = new();
            PemWriter pemWriter = new(stringWriter);
            RsaKeyParameters rsaKeyParameters = new(false,
                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Modulus),
                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Exponent));
            pemWriter.WriteObject(rsaKeyParameters);
            pemWriter.Writer.Flush();
            return stringWriter.ToString();
        }

        public static string ExportPrivateKeyToPEM(RSAParameters rsaParameters)
        {
            using StringWriter stringWriter = new();
            PemWriter pemWriter = new(stringWriter);
            RsaPrivateCrtKeyParameters rsaPrivateKeyParameters = new(
                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Modulus),
                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Exponent),
                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.D),
                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.P),
                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.Q),
                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.DP),
                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.DQ),
                new Org.BouncyCastle.Math.BigInteger(1, rsaParameters.InverseQ));
            pemWriter.WriteObject(rsaPrivateKeyParameters);
            pemWriter.Writer.Flush();
            return stringWriter.ToString();
        }
    }
}