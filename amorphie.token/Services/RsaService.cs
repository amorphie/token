using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Jose;
using Microsoft.IdentityModel.Tokens;

namespace amorphie.token.Services
{
    public class RsaService
    {

        public RSA LoadKey(string key)
        {
            var rsa = RSA.Create();
            var privateKey = DeserializeKey(key);
            rsa.ImportParameters(privateKey);
            return rsa;
        }


        private RSAParameters DeserializeKey(string keyString)
        {
            var sr = new StringReader(keyString);
            var xs = new XmlSerializer(typeof(RSAParameters));
            return (RSAParameters)xs.Deserialize(sr)!;
        }

        public JwkResponse GetJwks(string keyString)
        {

            var rsa = LoadKey(keyString);
            rsa.KeySize = 2048;


            var key = new RsaSecurityKey(rsa)

            {

                KeyId = Guid.NewGuid().ToString()

            };

            var parameters = key.Rsa.ExportParameters(false);

            var jwk = new JsonWebKeyModel

            {

                kty = "RSA",

                use = "sig",

                kid = key.KeyId,

                n = Base64UrlEncoder.Encode(parameters.Modulus),

                e = Base64UrlEncoder.Encode(parameters.Exponent)

            };

            var jwks = new JwkResponse

            {

                keys = new List<JsonWebKeyModel> { jwk }

            };

            return jwks;
        }
    }

    public class JwkResponse
    {
        public List<JsonWebKeyModel> keys { get; set; }
    }

    public class JsonWebKeyModel
    {
        public string kty { get; set; }

        public string use { get; set; }

        public string kid { get; set; }

        public string n { get; set; }

        public string e { get; set; }
    }
}
